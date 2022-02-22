using System.Collections.Generic;

namespace Unity.Gamepad.Mappings
{
    public class PS4Mapping : InputMapping
    {
        public static readonly List<string> GetControllerAliases = new List<string>() { "Wireless Controller" };
        
        public override void MapBindings(int deviceNumber)
        {
            ButtonBindingLookupTable[GamepadButton.RightBumper] = $"joystick {deviceNumber} button 5";
            ButtonBindingLookupTable[GamepadButton.LeftBumper] = $"joystick {deviceNumber} button 4";
            ButtonBindingLookupTable[GamepadButton.RightStickButton] = $"joystick {deviceNumber} button 11";

            ButtonBindingLookupTable[GamepadButton.ActionSouth] = $"joystick {deviceNumber} button 1";
            ButtonBindingLookupTable[GamepadButton.ActionWest] = $"joystick {deviceNumber} button 0";
            ButtonBindingLookupTable[GamepadButton.ActionEast] = $"joystick {deviceNumber} button 2";
            ButtonBindingLookupTable[GamepadButton.ActionNorth] = $"joystick {deviceNumber} button 3";
            ButtonBindingLookupTable[GamepadButton.Start] = $"joystick {deviceNumber} button 9";
            ButtonBindingLookupTable[GamepadButton.BackSelect] = $"joystick {deviceNumber} button 8";

            AxisBindingLookupTable[GamepadAxis.LeftHorizontal] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 0", Minimum = -1.0f, Maximum = 1.0f, DeadZoneOffset = 0.1f };
            AxisBindingLookupTable[GamepadAxis.LeftVertical] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 1", Minimum = -1.0f, Maximum = 1.0f, Inverted = true, DeadZoneOffset = 0.1f };
            AxisBindingLookupTable[GamepadAxis.RightHorizontal] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 2", Minimum = -1.0f, Maximum = 1.0f, DeadZoneOffset = 0.1f };
            AxisBindingLookupTable[GamepadAxis.RightVertical] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 5", Minimum = -1.0f, Maximum = 1.0f, Inverted = true, DeadZoneOffset = 0.1f };

            AxisBindingLookupTable[GamepadAxis.LeftTrigger] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 3", Minimum = -1.0f, Maximum = 1.0f, DeadZoneOffset = 0.3f, UnpressedValue = -1 };
            AxisBindingLookupTable[GamepadAxis.RightTrigger] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 4", Minimum = -1.0f, Maximum = 1.0f, DeadZoneOffset = 0.3f, UnpressedValue = -1 };

            AxisBindingLookupTable[GamepadAxis.DpadHorizontal] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 6", Minimum = -1.0f, Maximum = 1.0f, DeadZoneOffset = 0.3f, UnpressedValue = 0 };
            AxisBindingLookupTable[GamepadAxis.DpadVertical] = new GamepadAxisInfo() { AxisName = $"joystick {deviceNumber} analog 7", Minimum = -1.0f, Maximum = 1.0f, DeadZoneOffset = 0.3f, UnpressedValue = 0 };
        }
    }
}