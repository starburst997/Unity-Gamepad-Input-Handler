using System.Collections.Generic;

namespace Unity.Gamepad.Mappings
{
    internal struct SpecificKeyValue
    {
        public string VirtualButton;
        public float NegativePositiveMultiplier;
    }

    public class KeyboardMapping : InputMapping
    {
        private Dictionary<GamepadAxis, SpecificKeyValue[]> axisOverrides = new Dictionary<GamepadAxis, SpecificKeyValue[]>();

        public KeyboardMapping()
        {
            OverridesAxisReading = true;
        }

        public override List<string> GetControllerAliases()
        {
            return new List<string>();
        }

        public override void MapBindings(int deviceNumber)
        {

            //Buttons
            ButtonBindingLookupTable[GamepadButton.RightBumper] = "mouse 0";
            ButtonBindingLookupTable[GamepadButton.LeftBumper] = "space";
            ButtonBindingLookupTable[GamepadButton.RightStickButton] = "left ctrl";
            ButtonBindingLookupTable[GamepadButton.ActionSouth] = "space";
            ButtonBindingLookupTable[GamepadButton.ActionWest] = "return";


            //Axis (On Keyboard this doesn't really exist, so I'll have to "fake" it)
            axisOverrides[GamepadAxis.LeftHorizontal] = new[] { new SpecificKeyValue() { VirtualButton = "a", NegativePositiveMultiplier = -1 }, new SpecificKeyValue() { VirtualButton = "d", NegativePositiveMultiplier = 1 } };
            axisOverrides[GamepadAxis.LeftVertical] = new[] { new SpecificKeyValue() { VirtualButton = "s", NegativePositiveMultiplier = -1 }, new SpecificKeyValue() { VirtualButton = "w", NegativePositiveMultiplier = 1 } };
            axisOverrides[GamepadAxis.LeftTrigger] = new[] { new SpecificKeyValue() { VirtualButton = "left shift", NegativePositiveMultiplier = 1 } };
            axisOverrides[GamepadAxis.RightTrigger] = new[] { new SpecificKeyValue() { VirtualButton = "mouse 1", NegativePositiveMultiplier = 1 } };

            axisOverrides[GamepadAxis.DpadHorizontal] = new[] { new SpecificKeyValue() { VirtualButton = "a", NegativePositiveMultiplier = -1 }, new SpecificKeyValue() { VirtualButton = "d", NegativePositiveMultiplier = 1 } };
            axisOverrides[GamepadAxis.DpadVertical] = new[] { new SpecificKeyValue() { VirtualButton = "s", NegativePositiveMultiplier = -1 }, new SpecificKeyValue() { VirtualButton = "w", NegativePositiveMultiplier = 1 } };
        }
    }
}