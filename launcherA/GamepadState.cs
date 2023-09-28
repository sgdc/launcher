using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace launcherA
{
    public class GamepadState
    {
        uint lastPacket;
        
        public GamepadState(Controller_Wrapper.PlayerIndex userIndex)
        {
            UserIndex = userIndex;
            Controller = new Controller_Wrapper.Controller(userIndex);
            HoldDirectionThreshold = 0.25f; // time holding direction before rapidly pressing that direction
            HoldDirectionInterval = 0.09f; // how quickly to move in the held direction (0.25f = 4 times per second)

            fakeUp = false;
            fakeDown = false;
        }

        public readonly Controller_Wrapper.PlayerIndex UserIndex;
        public readonly Controller_Wrapper.Controller Controller;

        public DPadState DPad { get; set; }
        public DPadState DPadPrev { get; set; }
        public Controller_Wrapper.Thumbstick LeftStick { get; private set; }
        public Controller_Wrapper.Thumbstick RightStick { get; private set; }
        public Controller_Wrapper.Thumbstick LeftStickPrev { get; private set; }
        public Controller_Wrapper.Thumbstick RightStickPrev { get; private set; }
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
            get { return Controller.Connected; }
        }

        public void Vibrate(float leftMotor, float rightMotor)
        {
            /*Controller.SetVibration(new Vibration
            {
                LeftMotorSpeed = (ushort)(MathHelper.Saturate(leftMotor) * ushort.MaxValue),
                RightMotorSpeed = (ushort)(MathHelper.Saturate(rightMotor) * ushort.MaxValue)
            });*/

            // Porting to Controller_Wrapper, idgaf about vibrations
        }

        public void Update()
        {
            // If not connected, nothing to update
            if (!Connected) return;

            

            // If same packet, nothing to update
            Controller.Update();
            CheckHolding(); // assign fakeUp and fakeDown if holding up/down for long enough


            // Shoulders
            LeftShoulder = Controller.LeftShoulder;
            RightShoulder = Controller.RightShoulder;

            // Triggers
            LeftTrigger = LeftTrigger;
            RightTrigger = RightTrigger;

            // Buttons
            Start = Controller.Start;
            Back = Controller.Back;

            A = Controller.A;
            B = Controller.B;
            X = Controller.X;
            Y = Controller.Y;

            // D-Pad
            DPad = new DPadState(Controller.DPadUp,
                                 Controller.DPadDown,
                                 Controller.DPadLeft,
                                 Controller.DPadRight);

            LeftStick = Controller.LeftThumbstick;
            RightStick = Controller.RightThumbstick;
        }

        public void SetUDLR()
        {
            Up = (LeftStickYPrev < 0.75 && LeftStick.Y >= 0.75) || (DPad.Up && !DPadPrev.Up) || fakeUp;
            Down = (LeftStickYPrev > -0.75 && LeftStick.Y <= -0.75) || (DPad.Down && !DPadPrev.Down) || fakeDown;

            Right = (LeftStickXPrev < 0.75 && LeftStick.X >= 0.75) || (DPad.Right && !DPadPrev.Right);
            Left = (LeftStickXPrev > -0.75 && LeftStick.X <= -0.75) || (DPad.Left && !DPadPrev.Left);

            DPadPrev = DPad;
            fakeUp = false;
            fakeDown = false;
        }

        public struct DPadState
        {
            public readonly bool Up, Down, Left, Right;

            public DPadState(bool up, bool down, bool left, bool right)
            {
                Up = up; Down = down; Left = left; Right = right;
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
