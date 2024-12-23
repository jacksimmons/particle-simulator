using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace ParticleGame;

public class ParticleGame : Game
{
    private GraphicsDeviceManager m_graphics;
    private SpriteBatch m_spriteBatch;
    private SpriteFont m_debugFont;

    private Vector2 m_screenParticles;

    private Particle[,] m_particles;

    private double m_secsSinceLastIteration;
    private double m_secsBetweenIterations = 0;


    public ParticleGame()
    {
        m_graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }


    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        m_screenParticles = new(GraphicsDevice.Viewport.Width / Particle.SIZE, GraphicsDevice.Viewport.Height / Particle.SIZE);
        m_particles = new Particle[(int)m_screenParticles.X, (int)m_screenParticles.Y];

        for (int i = 0; i < m_screenParticles.X; i++)
        {
            for (int j = 0; j < m_screenParticles.Y; j++)
            {
                Particle particle;

                // Add ground
                if (j == m_screenParticles.Y - 1)
                    particle = new(ParticleType.Ground, GraphicsDevice);
                // Fill rest with void
                else
                    particle = new(ParticleType.Void, GraphicsDevice);

                m_particles[i, j] = particle;
            }
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        m_spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        m_debugFont = Content.Load<SpriteFont>("DebugFont");
    }

    protected override void Update(GameTime gameTime)
    {
        InputHandler.Update();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        m_secsSinceLastIteration += gameTime.ElapsedGameTime.TotalSeconds;
        if (m_secsSinceLastIteration > m_secsBetweenIterations)
        {
            m_secsSinceLastIteration = 0;
            List<Particle> iteratedParticles = new();

            // Iterate from bottom right to top left
            for (int i = (int)m_screenParticles.X - 1; i >= 0; i--)
            {
                for (int j = (int)m_screenParticles.Y - 1; j >= 0; j--)
                {
                    Particle particle = m_particles[i, j];

                    // Do nothing for empty pixels, and pixels which have already been simulated
                    if (particle == null || iteratedParticles.Contains(particle)) continue;

                    bool hasParticleBelow = j + 1 < m_screenParticles.Y;

                    if (hasParticleBelow)
                    {
                        // Handle gravity
                        float densityBelow = m_particles[i, j + 1].Properties.Density;
                        if (particle.Properties.Density > densityBelow && particle.Properties.Gravity)
                        {
                            m_particles[i, j] = m_particles[i, j + 1];
                            m_particles[i, j + 1] = particle;
                        }

                        // Handle liquids
                        if (hasParticleBelow && particle.Properties.State == ParticleState.Liquid)
                        {
                            Particle below = m_particles[i, j + 1];

                            // A liquid particle above another liquid particle moves horizontally
                            if (below.Properties.State == ParticleState.Liquid)
                            {
                                DisplaceLiquid(i, j);
                            }
                        }

                        // Handle gases
                        if (particle.Properties.State == ParticleState.Gas)
                        {
                            DisplaceGas(i, j);
                        }

                        // Handle lava
                        if (particle.Type == ParticleType.Lava)
                        {
                            var neighbourhood = GetNeumannNeighbourhood(i, j);
                            bool lavaFrozen = false;

                            var makeSteam = () =>
                            {
                                lavaFrozen = true;
                                return new Particle(ParticleType.Steam, GraphicsDevice);
                            };

                            if (neighbourhood.Item1?.Type == ParticleType.Water)
                            {
                                m_particles[i, j - 1] = makeSteam();
                            }
                            if (neighbourhood.Item2?.Type == ParticleType.Water)
                            {
                                m_particles[i + 1, j] = makeSteam();
                            }
                            if (neighbourhood.Item3?.Type == ParticleType.Water)
                            {
                                m_particles[i, j + 1] = makeSteam();
                            }
                            if (neighbourhood.Item4?.Type == ParticleType.Water)
                            {
                                m_particles[i - 1, j] = makeSteam();
                            }

                            if (lavaFrozen) m_particles[i, j] = new(ParticleType.Ground, GraphicsDevice);
                        }
                    }
                }
            }
        }

        if (InputHandler.MouseLeftDown)
        {
            Vector2 pos = InputHandler.MousePos;

            // Ignore mouse input outside the viewport
            if (GraphicsDevice.Viewport.Bounds.Contains(pos))
            {
                m_particles[(int)pos.X / Particle.SIZE, (int)pos.Y / Particle.SIZE] = new(InputHandler.SelectedType, GraphicsDevice);
            }
        }

        base.Update(gameTime);
    }


    /// <summary>
    /// Returns the neighbourhood of the particle at the provided position.
    /// Item1 = North, Item2 = East, Item3 = South, Item4 = West.
    /// </summary>
    private Tuple<Particle, Particle, Particle, Particle> GetNeumannNeighbourhood(int i, int j)
    {
        Particle north = null, east = null, south = null, west = null;

        if (i > 0) west = m_particles[i - 1, j];
        if (i < m_screenParticles.X - 1) east = m_particles[i + 1, j];
        if (j > 0) north = m_particles[i, j - 1];
        if (j < m_screenParticles.Y - 1) south = m_particles[i, j + 1];

        return new(north, east, south, west);
    }


    /// <summary>
    /// Randomly displaces any available void particle in the neighbourhood.
    /// If a non-void/gas exists on one side, will pick one of the others.
    /// If a non-void/gas exists on all sides, will do nothing.
    /// </summary>
    private void DisplaceGas(int i, int j)
    {
        var canBeDisplaced = (Particle p) => p != null && (p.Type == ParticleType.Void || p.Properties.State == ParticleState.Gas);
        var neighbourhood = GetNeumannNeighbourhood(i, j);
        var displacable = new List<Particle> { neighbourhood.Item1, neighbourhood.Item2, neighbourhood.Item3, neighbourhood.Item4 };

        Random rng = new();
        while (displacable.Count > 0)
        {
            int randIndex = rng.Next(0, displacable.Count);
            if (canBeDisplaced(displacable[randIndex]))
            {
                if (displacable[randIndex] == neighbourhood.Item1)
                {
                    m_particles[i, j - 1] = m_particles[i, j];
                    m_particles[i, j] = displacable[randIndex];
                }

                if (displacable[randIndex] == neighbourhood.Item2)
                {
                    m_particles[i + 1, j] = m_particles[i, j];
                    m_particles[i, j] = displacable[randIndex];
                }

                if (displacable[randIndex] == neighbourhood.Item3)
                {
                    m_particles[i, j + 1] = m_particles[i, j];
                    m_particles[i, j] = displacable[randIndex];
                }

                if (displacable[randIndex] == neighbourhood.Item4)
                {
                    m_particles[i - 1, j] = m_particles[i, j];
                    m_particles[i, j] = displacable[randIndex];
                }

                break;
            }
            else
            {
                displacable.RemoveAt(randIndex);
            }
        }
    }


    /// <summary>
    /// Randomly displaces any available void particle on the left or right.
    /// If a non-void or solid exists on one side, will pick the other.
    /// If a non-void or solid exists on both sides, will do nothing.
    /// </summary>
    private void DisplaceLiquid(int i, int j)
    {
        bool canDisplaceLeft = false;
        var canBeDisplaced = (Particle p) => p.Type == ParticleType.Void || p.Properties.State != ParticleState.Solid;

        if (i > 0)
        {
            if (m_particles[i - 1, j].Type == ParticleType.Void || m_particles[i - 1, j].Properties.State == ParticleState.Liquid)
            {
                canDisplaceLeft = true;
            }
        }

        bool canDisplaceRight = false;

        if (i < m_screenParticles.X - 1)
        {
            if (m_particles[i + 1, j].Type == ParticleType.Void || m_particles[i + 1, j].Properties.State == ParticleState.Liquid)
            {
                canDisplaceRight = true;
            }
        }

        // Disable one of the two flags to make the displacement random
        if (canDisplaceLeft && canDisplaceRight)
        {
            Random rng = new();
            if (rng.Next(0, 2) == 1)
                canDisplaceLeft = false;
            else
                canDisplaceRight = false;
        }

        // Displace to the left
        if (canDisplaceLeft)
        {
            Particle left = m_particles[i - 1, j];
            m_particles[i - 1, j] = m_particles[i, j];
            m_particles[i, j] = left;
        }

        // Displace to the right
        else if (canDisplaceRight)
        {
            Particle right = m_particles[i + 1, j];
            m_particles[i + 1, j] = m_particles[i, j];
            m_particles[i, j] = right;
        }
    }


    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code here
        m_spriteBatch.Begin();
        for (int i = 0; i < m_particles.GetLength(0); i++)
        {
            for (int j = 0; j < m_particles.GetLength(1); j++)
            {
                // Do nothing for empty pixels
                Particle particle = m_particles[i, j];
                if (particle == null) continue;

                m_spriteBatch.Draw(particle.Texture, new Vector2(i,j) * Particle.SIZE, particle.Colour);
            }
        }
        m_spriteBatch.DrawString(m_debugFont, $"Drawing: {InputHandler.SelectedType}", new Vector2(0, 0), Color.White);

        string allParticles = "";
        foreach (ParticleType type in Enum.GetValues(typeof(ParticleType)))
        {
            allParticles += $"{type}: ";
            int typeCount = 0;
            foreach (Particle particle in m_particles)
            {
                if (particle.Type == type) typeCount++;
            }
            allParticles += $"{typeCount}\n";
        }

        m_spriteBatch.DrawString(m_debugFont, allParticles, new Vector2(0, 20), Color.White);
        m_spriteBatch.End();

        base.Draw(gameTime);
    }
}
