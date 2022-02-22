using System.Collections.Generic;

namespace Unity.Gamepad.Mappings
{
    public class XboxOneMapping : InputMapping
    {
        public override List<string> GetControllerAliases()
        {
            return new List<string>() { "Controller (XBOX One For Windows)" };
        }

        public override void MapBindings(int deviceNumber)
        {
            ButtonBindingLookupTable[GamepadButton.LeftBumper] = $"joystick {deviceNumber} button 4";
            ButtonBindingLookupTable[GamepadButton.RightBumper] = $"joystick {deviceNumber} button 5";
            ButtonBindingLookupTable[GamepadButton.RightStickButton] = $"joystick {deviceNumber} button 9";


            ButtonBindingLookupTable[GamepadButton.ActionSouth] = $"joystick {deviceNumber} button 0";
            ButtonBindingLookupTable[GamepadButton.ActionWest] = $"joystick {deviceNumber} button 2";
            ButtonBindingLookupTable[GamepadButton.ActionEast] = $"joystick {deviceNumber} button 1";
            ButtonBindingLookupTable[GamepadButton.ActionNorth] = $"joystick {deviceNumber} button 3";
            ButtonBindingLookupTable[GamepadButton.Start] = $"joystick {deviceNumber} button 7";
            ButtonBindingLookupTable[GamepadButton.BackSelect] = $"joystick {deviceNumber} button 6";


            AxisBindingLookupTable[GamepadAxis.LeftHorizontal] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 0", Minimum = -1.0f, Maximum = 1.0f, DeadZoneOffset = 0.1f };
            AxisBindingLookupTable[GamepadAxis.LeftVertical] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 1", Minimum = -1.0f, Maximum = 1.0f, Inverted = true, DeadZoneOffset = 0.1f };
            AxisBindingLookupTable[GamepadAxis.RightHorizontal] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 5", Minimum = -1.0f, Maximum = 1.0f, DeadZoneOffset = 0.01f };
            AxisBindingLookupTable[GamepadAxis.RightVertical] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 4", Minimum = -1.0f, Maximum = 1.0f, Inverted = true, DeadZoneOffset = 0.01f };

            AxisBindingLookupTable[GamepadAxis.LeftTrigger] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 9", Minimum = 0f, Maximum = 1.0f, DeadZoneOffset = 0.2f, UnpressedValue = 0 };
            AxisBindingLookupTable[GamepadAxis.RightTrigger] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 10", Minimum = 0f, Maximum = 1.0f, DeadZoneOffset = 0.2f, UnpressedValue = 0 };

            AxisBindingLookupTable[GamepadAxis.DpadHorizontal] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 6", Minimum = -1.0f, Maximum = 1.0f, DeadZoneOffset = 0.3f, UnpressedValue = 0 };
            AxisBindingLookupTable[GamepadAxis.DpadVertical] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 7", Minimum = -1.0f, Maximum = 1.0f, DeadZoneOffset = 0.3f, UnpressedValue = 0 };
        }
    }
}