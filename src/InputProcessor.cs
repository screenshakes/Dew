using SK.Libretro;

namespace Dew
{
    sealed class InputProcessor : IInputProcessor
    {
        public bool[] Buttons = new bool[16];
        public bool AnalogDirectionsToDigital { get; set; }

        public bool JoypadButton(int port, int button)
        {
            return Buttons[button];
        }

        public float MouseDeltaX(int port)
        {
            return 0;
        }

        public float MouseDeltaY(int port)
        {
            return 0;
        }

        public float MouseWheelDeltaX(int port)
        {
            return 0;
        }

        public float MouseWheelDeltaY(int port)
        {
            return 0;
        }

        public bool MouseButton(int port, int button)
        {
            return false;
        }


        public bool KeyboardKey(int port, int key)
        {
            return false;
        }

        public float AnalogLeftValueX(int port)
        {
            return 0;
        }

        public float AnalogLeftValueY(int port)
        {
            return 0;
        }

        public float AnalogRightValueX(int port)
        {
            return 0;
        }

        public float AnalogRightValueY(int port)
        {
            return 0;
        }

        public void Clear()
        {
            for(int i = 0; i < Buttons.Length; ++i)
                Buttons[i] = false;
        }
    }
}
