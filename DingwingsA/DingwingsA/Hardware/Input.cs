using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Diagnostics;

namespace Hardware
{
    class Input
    {
        public static KeyboardState keyboardState;
        public static MouseState mouseState;
        public static GamePadState gamepadState;
        public static TouchCollection touchState;
        public static float deadzone = .3F;
        public static void update()
        {
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            touchState = TouchPanel.GetState();
            gamepadState = GamePad.GetState(0);
        }

        public static bool getA()
        {
            return keyboardState.IsKeyDown(Keys.J) ||
                keyboardState.IsKeyDown(Keys.Z) ||
                gamepadState.IsButtonDown(Buttons.A);
        }

        public static bool getB()
        {
            return keyboardState.IsKeyDown(Keys.K) ||
                keyboardState.IsKeyDown(Keys.X) ||
                gamepadState.IsButtonDown(Buttons.B);
        }

        public static bool getUp()
        {
            return keyboardState.IsKeyDown(Keys.W) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                gamepadState.DPad.Up == ButtonState.Pressed ||
                gamepadState.ThumbSticks.Left.Y > deadzone;
        }

        public static bool getDown()
        {
            return keyboardState.IsKeyDown(Keys.S) ||
                keyboardState.IsKeyDown(Keys.Down) ||
                gamepadState.DPad.Down == ButtonState.Pressed ||
                gamepadState.ThumbSticks.Left.Y < -deadzone; ;
        }

        public static bool getLeft()
        {
            return keyboardState.IsKeyDown(Keys.A) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                gamepadState.DPad.Left == ButtonState.Pressed ||
                gamepadState.ThumbSticks.Left.X < -deadzone; ;
        }

        public static bool getRight()
        {
            return keyboardState.IsKeyDown(Keys.D) ||
                keyboardState.IsKeyDown(Keys.Right) ||
                gamepadState.DPad.Right == ButtonState.Pressed ||
                gamepadState.ThumbSticks.Left.X > deadzone; ;
        }

        public static bool getStart()
        {
            return keyboardState.IsKeyDown(Keys.Enter) ||
                gamepadState.IsButtonDown(Buttons.Start);
        }
        
        public static bool getLeftMouse()
        {
            return mouseState.LeftButton == ButtonState.Pressed;
        }

        public static bool getRightMouse()
        {
            return mouseState.RightButton == ButtonState.Pressed;
        }

        public static int touchCount()
        {
            return touchState.Count;
        }

        public static TouchLocation getTouch(int num)
        {
            return touchState[0];
        }
    }
}
