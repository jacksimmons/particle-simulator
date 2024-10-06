using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace particle_game;

public class ParticleGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Vector2 _screenSize;

    private Particle[,] _particles;

    private double _secsSinceLastIteration;
    private double _secsBetweenIterations;

    public ParticleGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _screenSize = new(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        _particles = new Particle[(int)_screenSize.X, (int)_screenSize.Y];

        // Add ground
        for (int i = 0; i < _screenSize.X; i++)
        {
            Particle groundPart = new(ParticleType.Ground, new(GraphicsDevice, 1, 1));
            _particles[i, (int)_screenSize.Y - 1] = groundPart;
        }

        for (int i = 0; i < 100; i++)
        {
            _particles[i, i] = new(ParticleType.Sand, new(GraphicsDevice, 1, 1));
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        InputHandler.Update();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        _secsSinceLastIteration += gameTime.ElapsedGameTime.TotalSeconds;
        if (_secsSinceLastIteration > _secsBetweenIterations)
        {
            _secsSinceLastIteration = 0;
            List<Particle> iteratedParticles = new();

            for (int i = 0; i < _particles.GetLength(0); i++)
            {
                for (int j = 0; j < _particles.GetLength(1); j++)
                {
                    Particle particle = _particles[i, j];

                    // Do nothing for empty pixels, and pixels which have already been simulated
                    if (particle == null || iteratedParticles.Contains(particle)) continue;

                    Vector2 newPos = _particles[i, j].Iteration(new(i, j), _screenSize);
                    iteratedParticles.Add(_particles[i, j]);

                    // Try and move the particle
                    Particle newParticle = _particles[(int)newPos.X, (int)newPos.Y];
                    if (newParticle == null)
                    {
                        _particles[(int)newPos.X, (int)newPos.Y] = _particles[i, j];
                        _particles[i, j] = null;
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
                _particles[(int)pos.X, (int)pos.Y] = new(ParticleType.Sand, new(GraphicsDevice, 1, 1));
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        _spriteBatch.Begin();
        for (int i = 0; i < _particles.GetLength(0); i++)
        {
            for (int j = 0; j < _particles.GetLength(1); j++)
            {
                // Do nothing for empty pixels
                Particle particle = _particles[i, j];
                if (particle == null) continue;

                _spriteBatch.Draw(particle.Texture, new Vector2(i,j), particle.Colour);
            }
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
