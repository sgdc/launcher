using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.XInput;

namespace launcherA
{
    public class GamepadState
    {
        uint lastPacket;
        
        public GamepadState(UserIndex userIndex)
        {
            UserIndex = userIndex;
            Controller = new Controller(userIndex);

            HoldDirectionThreshold = 0.25f; // time holding direction before rapidly pressing that direction
            HoldDirectionInterval = 0.09f; // how quickly to move in the held direction (0.25f = 4 times per second)

            fakeUp = false;
            fakeDown = false;
        }

        public readonly UserIndex UserIndex;
        public readonly Controller Controller;

        public DPadState DPad { get; set; }
        public DPadState DPadPrev { get; set; }
        public ThumbstickState LeftStick { get; private set; }
        public ThumbstickState RightStick { get; private set; }
        public ThumbstickState LeftStickPrev { get; private set; }
        public ThumbstickState RightStickPrev { get; private set; }
        public float LeftStickXPrev { get; set; }
        public float LeftStickYPrev { get; set; }

        public bool A { get; private set; }
        public bool B { get; private set; }
        public bool X { get; private set; }
        public bool Y { get; private set; }

        public bool Up { get; private set; }
        public bool Down { get; private set; }
        public bool Left { get; private set; }
        public bool Right { get; private set; }

        public bool RightShoulder { get; private set; }
        public bool LeftShoulder { get; private set; }

        public bool Start { get; private set; }
        public bool Back { get; private set; }

        public float RightTrigger { get; private set; }
        public float LeftTrigger { get; private set; }

        public float HoldDirectionThreshold { get; private set; }
        public float HoldDirectionInterval { get; private set; }

        private float holdUpTime { get; set; }
        private float holdDownTime { get; set; }

        private bool fakeUp { get; set; }
        private bool fakeDown { get; set; }

        public bool Connected
        {
            get { return Controller.IsConnected; }
        }

        public void Vibrate(float leftMotor, float rightMotor)
        {
            Controller.SetVibration(new Vibration
            {
                LeftMotorSpeed = (ushort)(MathHelper.Saturate(leftMotor) * ushort.MaxValue),
                RightMotorSpeed = (ushort)(MathHelper.Saturate(rightMotor) * ushort.MaxValue)
            });
        }

        public void Update()
        {
            // If not connected, nothing to update
            if (!Connected) return;

            CheckHolding(); // assign fakeUp and fakeDown if holding up/down for long enough

            // If same packet, nothing to update
            State state = Controller.GetState();
            if (lastPacket == state.PacketNumber) return;
            lastPacket = state.PacketNumber;

            var gamepadState = state.Gamepad;

            // Shoulders
            LeftShoulder = (gamepadState.Buttons & GamepadButtonFlags.LeftShoulder) != 0;
            RightShoulder = (gamepadState.Buttons & GamepadButtonFlags.RightShoulder) != 0;

            // Triggers
            LeftTrigger = gamepadState.LeftTrigger / (float)byte.MaxValue;
            RightTrigger = gamepadState.RightTrigger / (float)byte.MaxValue;

            // Buttons
            Start = (gamepadState.Buttons & GamepadButtonFlags.Start) != 0;
            Back = (gamepadState.Buttons & GamepadButtonFlags.Back) != 0;

            A = (gamepadState.Buttons & GamepadButtonFlags.A) != 0;
            B = (gamepadState.Buttons & GamepadButtonFlags.B) != 0;
            X = (gamepadState.Buttons & GamepadButtonFlags.X) != 0;
            Y = (gamepadState.Buttons & GamepadButtonFlags.Y) != 0;

            // D-Pad
            DPad = new DPadState((gamepadState.Buttons & GamepadButtonFlags.DPadUp) != 0,
                                 (gamepadState.Buttons & GamepadButtonFlags.DPadDown) != 0,
                                 (gamepadState.Buttons & GamepadButtonFlags.DPadLeft) != 0,
                                 (gamepadState.Buttons & GamepadButtonFlags.DPadRight) != 0);

            LeftStick = new ThumbstickState(
                Normalize(gamepadState.LeftThumbX, gamepadState.LeftThumbY, Gamepad.GamepadLeftThumbDeadZone),
                (gamepadState.Buttons & GamepadButtonFlags.LeftThumb) != 0);
            RightStick = new ThumbstickState(
                Normalize(gamepadState.RightThumbX, gamepadState.RightThumbY, Gamepad.GamepadRightThumbDeadZone),
                (gamepadState.Buttons & GamepadButtonFlags.RightThumb) != 0);
        }

        public void SetUDLR()
        {
            Up = (LeftStickYPrev < 0.75 && LeftStick.Position.Y >= 0.75) || (DPad.Up && !DPadPrev.Up) || fakeUp;
            Down = (LeftStickYPrev > -0.75 && LeftStick.Position.Y <= -0.75) || (DPad.Down && !DPadPrev.Down) || fakeDown;

            Right = (LeftStickXPrev < 0.75 && LeftStick.Position.X >= 0.75) || (DPad.Right && !DPadPrev.Right);
            Left = (LeftStickXPrev > -0.75 && LeftStick.Position.X <= -0.75) || (DPad.Left && !DPadPrev.Left);

            DPadPrev = DPad;
            fakeUp = false;
            fakeDown = false;
        }

        static Vector2 Normalize(short rawX, short rawY, short threshold)
        {
            var value = new Vector2(rawX, rawY);
            var magnitude = value.Length();
            var direction = value / (magnitude == 0 ? 1 : magnitude);

            var normalizedMagnitude = 0.0f;
            if (magnitude - threshold > 0)
                normalizedMagnitude = Math.Min((magnitude - threshold) / (short.MaxValue - threshold), 1);

            return direction * normalizedMagnitude;
        }

        public struct DPadState
        {
            public readonly bool Up, Down, Left, Right;

            public DPadState(bool up, bool down, bool left, bool right)
            {
                Up = up; Down = down; Left = left; Right = right;
            }
        }

        public struct ThumbstickState
        {
            public readonly Vector2 Position;
            public readonly bool Clicked;

            public ThumbstickState(Vector2 position, bool clicked)
            {
                Clicked = clicked;
                Position = position;
            }
        }

        private void CheckHolding()
        {
            if ((LeftStickYPrev >= 0.75) || (DPadPrev.Up)) // holding up
            {
                holdUpTime += 0.016f; // timer is set to 16ms
                if (holdUpTime > HoldDirectionInterval + HoldDirectionThreshold)
                {
                    holdUpTime = HoldDirectionThreshold;
                    fakeUp = true;
                }
            }
            else
            {
                // not holding up
                holdUpTime = 0f;
            }

            if ((LeftStickYPrev <= -0.75) || DPadPrev.Down) // holding down
            {
                holdDownTime += 0.016f; // timer is set to 16ms
                if (holdDownTime > HoldDirectionInterval + HoldDirectionThreshold)
                {
                    holdDownTime = HoldDirectionThreshold;
                    fakeDown = true;
                }
            }
            else
            {
                holdDownTime = 0f;
            }
        }
    }

    public static class MathHelper
    {
        public static float Saturate(float value)
        {
            return value < 0 ? 0 : value > 1 ? 1 : value;
        }
    }
}
