using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace particle_game;


public static class InputHandler
{
    public static bool MouseLeftClicked { get; private set; } = false;
    public static bool MouseLeftDown { get; private set; } = false;

    public static Vector2 MousePos { get; private set; } = new();
    private static MouseState m_ms = new MouseState(), m_prevMs;


    public static void Update()
    {
        m_prevMs = m_ms;
        m_ms = Mouse.GetState();
        
        MouseLeftClicked =
            m_ms.LeftButton != ButtonState.Pressed &&
            m_prevMs.LeftButton == ButtonState.Pressed;
        MouseLeftDown = m_ms.LeftButton == ButtonState.Pressed;

        MousePos = new(m_ms.Position.X, m_ms.Position.Y);
    }
}
