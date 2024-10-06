using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace particle_game;


public enum ParticleType
{
    Ground,
    Sand
}


/// <summary>
/// Class representing a non-empty pixel.
/// </summary>
public class Particle
{
    public Color Colour { get; }
    public Texture2D Texture { get; }
    public ParticleType Type { get; }

    public bool Gravity { get; set; } = true;


    public Particle(ParticleType type, Texture2D texture)
    {
        Texture = texture;
        Type = type;

        switch (Type)
        {
            case ParticleType.Ground:
                Colour = Color.Brown;
                Gravity = false;
                break;
            case ParticleType.Sand:
                Colour = Color.Yellow;
                break;
        }

        Texture.SetData(new Color[] { Colour });
    }


    /// <summary>
    /// Performs an iteration on the particle, returning its new position.
    /// </summary>
    public Vector2 Iteration(Vector2 position, Vector2 screenSize)
    {
        if (Gravity)
        {
            float y = MathF.Min(position.Y + 1, screenSize.Y);
            position = new(position.X, y);
        }

        return position;
    }
}
