using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ParticleGame;


public static class InputHandler
{
    public static bool MouseLeftClicked { get; private set; } = false;
    public static bool MouseLeftDown { get; private set; } = false;

    public static Vector2 MousePos { get; private set; } = new();
    private static MouseState m_ms = new MouseState(), m_prevMs;

    private static KeyboardState m_prevKeyboardState;
    private static KeyboardState m_currentKeyboardState = Keyboard.GetState();

    public static ParticleType SelectedType { get; private set; } = ParticleType.Ground;


    public static void Update()
    {
        m_prevMs = m_ms;
        m_ms = Mouse.GetState();
        
        MouseLeftClicked =
            m_ms.LeftButton != ButtonState.Pressed &&
            m_prevMs.LeftButton == ButtonState.Pressed;
        MouseLeftDown = m_ms.LeftButton == ButtonState.Pressed;

        MousePos = new(m_ms.Position.X, m_ms.Position.Y);

        m_prevKeyboardState = m_currentKeyboardState;
        m_currentKeyboardState = Keyboard.GetState();

        if (WasKeyJustPressed(Keys.Right))
        {
            SelectedType = Extensions.Next(SelectedType);
        }
        else if (WasKeyJustPressed(Keys.Left))
        {
            SelectedType = Extensions.Prev(SelectedType);
        }
    }


    private static bool WasKeyJustPressed(Keys key)
    {
        return (!m_prevKeyboardState.IsKeyDown(key) && m_currentKeyboardState.IsKeyDown(key));
    }
}
