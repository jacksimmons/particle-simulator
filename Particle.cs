using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleGame;


public enum ParticleType
{
    Void,
    Ground,
    Sand,
    Water,
    Steam,
    Lava
}


/// <summary>
/// Class representing a non-empty pixel.
/// </summary>
public partial class Particle
{
    public Color Colour { get; }
    public Texture2D Texture { get; }
    public ParticleType Type { get; }
    public const int SIZE = 10;


    public Particle(ParticleType type, GraphicsDevice device)
    {
        Texture = new(device, SIZE, SIZE);
        Type = type;

        switch (Type)
        {
            case ParticleType.Ground:
                Colour = Color.Gray;
                break;
            case ParticleType.Sand:
                Colour = Color.Yellow;
                break;
            case ParticleType.Water:
                Colour = Color.MediumAquamarine;
                break;
            case ParticleType.Steam:
                Colour = Color.Aquamarine;
                break;
            case ParticleType.Lava:
                Colour = Color.OrangeRed;
                break;
        }

        Color[] data = new Color[SIZE * SIZE];
        for (int i = 0; i < SIZE * SIZE; i++)
        {
            data[i] = Colour;
        }

        Texture.SetData(data);
    }
}
