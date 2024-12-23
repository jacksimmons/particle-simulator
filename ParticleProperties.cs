using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleGame;


partial class Particle
{
    public static Dictionary<ParticleType, ParticleProperties> m_typeToProperties = new Dictionary<ParticleType, ParticleProperties>
    {
        //https://en.wikipedia.org/wiki/Density kg/m3
        { ParticleType.Void, new ParticleProperties { Density = 101, Gravity = false } },
        { ParticleType.Ground, new ParticleProperties { Density = float.PositiveInfinity, Gravity = false } },
        { ParticleType.Sand, new ParticleProperties { Density = 1600 } },
        { ParticleType.Water, new ParticleProperties { Density = 1000, State = ParticleState.Liquid } },
        { ParticleType.Steam, new ParticleProperties { Density = 100, State = ParticleState.Gas } },
        { ParticleType.Lava, new ParticleProperties { Density = 3000, State = ParticleState.Liquid } },
    };
    public ParticleProperties Properties => m_typeToProperties[Type];
}


public enum ParticleState
{
    Solid,
    Liquid,
    Gas
}


public struct ParticleProperties
{
    public float Density { get; set; } = 0;
    public bool Gravity { get; set; } = true;
    public ParticleState State { get; set; } = ParticleState.Solid;


    public ParticleProperties() { }
}
