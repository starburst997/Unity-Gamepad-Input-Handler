using System.Collections.Generic;

namespace Unity.Gamepad
{
    public abstract class InputMapping
    {
        public readonly Dictionary<GamepadButton, string> ButtonBindingLookupTable = new Dictionary<GamepadButton, string>();
        public readonly Dictionary<GamepadAxis, GamepadAxisInfo> AxisBindingLookupTable = new Dictionary<GamepadAxis, GamepadAxisInfo>();
        
        //protected int InputNumber = 1;
        
        public int OriginalIndex;
        public bool IsDisconnected = false;
        public bool OverridesAxisReading = false;

        public abstract void MapBindings(int deviceNumber);

        public abstract List<string> GetControllerAliases();

        public virtual float OverrideAxisReading(GamepadAxis axis)
        {
            return 0.0f;
        }
    }
}