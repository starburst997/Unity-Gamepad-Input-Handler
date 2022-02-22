using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Gamepad.Mappings;
using UnityEngine;

namespace Unity.Gamepad
{
    public enum GamepadButton
    {
        ActionSouth = 0,
        ActionEast = 1,
        ActionWest = 2,
        ActionNorth = 3,
        LeftBumper = 4,
        RightBumper = 5,
        BackSelect = 6,
        Start = 7,
        LeftStickButton = 8,
        RightStickButton = 9
    }

    public enum GamepadAxis
    {
        LeftHorizontal = -2,
        LeftVertical = -1,
        RightHorizontal = 4,
        RightVertical = 5,
        DpadHorizontal = 6,
        DpadVertical = 7,
        LeftTrigger = 9,
        RightTrigger = 10
    }

    public enum PositiveNegativeAxis
    {
        Indifferent = 0,
        Negative = -1,
        Positive = 1
    }

    public struct GamepadAxisInfo
    {
        public string AxisName;
        public bool Inverted;
        public float Minimum;
        public float Maximum;
        public float DeadZoneOffset;
        public float UnpressedValue;
    }

    public class AxisState
    {
        public InputMapping BelongingMapping;
        public GamepadAxis Axis;
        public bool PressedLastFrame;
        public bool PressedInCurrentFrame;
        public PositiveNegativeAxis DoesAxisMatter;
    }

    public class InputHandler : MonoBehaviour
    {
        public static InputHandler Instance;
        
        public readonly List<InputMapping> PlayerMappings = new List<InputMapping>();
        public readonly Dictionary<string, int> NameToInputMappingLookupTable = new Dictionary<string, int>();
        private readonly List<AxisState> _axisToButtonStates = new List<AxisState>();

        public int MaxController = 4;
        
        public bool SkipUnknown = true;
        public bool DetectController = false;
        
        public bool HasUpdatedFrame = false;
        public float CheckForNewControllerTimer = 5.0f;
        
        public event Action<int> OnNewControllerConnected;
        public event Action<int> OnControllerDisconnected;
        
        private WaitForSeconds _wait;
        private WaitForEndOfFrame _waitFrame;

        private void Awake()
        {
            // If the singleton hasn't been initialized yet
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return; //Avoid doing anything else
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // TODO: Switch to enum or something instead of index (but bewareof enum in dictionary, they create alloc if I remember correctly...)
        private readonly List<List<string>> _supportedInputMappings = new List<List<string>>()
        {
            PS4Mapping.GetControllerAliases, Xbox360Mapping.GetControllerAliases, XboxOneMapping.GetControllerAliases, SwitchProControllerMapping.GetControllerAliases
        };

        private void Start()
        {
            _wait = new WaitForSeconds(CheckForNewControllerTimer);
            _waitFrame = new WaitForEndOfFrame();
            
            var standardKeyboardMapping = new KeyboardMapping();
            
            standardKeyboardMapping.MapBindings(-1);
            standardKeyboardMapping.OriginalIndex = -1;
            PlayerMappings.Add(standardKeyboardMapping);
            FillNameToInputMappingLookupTable();

            var devices = Input.GetJoystickNames();

            for (var i = 0; i < devices.Length; i++)
            {
                if (!string.IsNullOrEmpty(devices[i]))
                {
                    if (NameToInputMappingLookupTable.TryGetValue(devices[i], out var typeofInput))
                    {
                        InputMapping instance = null;
                        switch (typeofInput)
                        {
                            case 0:
                                instance = new PS4Mapping();
                                break;
                            case 1:
                                instance = new Xbox360Mapping();
                                break;
                            case 2:
                                instance = new XboxOneMapping();
                                break;
                            case 3:
                                instance = new SwitchProControllerMapping();
                                break;
                        }
                        
                        instance.OriginalIndex = i;
                        instance.MapBindings(i + 1);
                        
                        PlayerMappings.Add(instance);
                    }
#if UNITY_WEBGL
                    else // WebGL have standard gamepad (I thinK?) so we can assume X360 mapping
#else
                    else if (!SkipUnknown)
#endif
                    {
                        var mapping = new Xbox360Mapping();
                        mapping.OriginalIndex = i;
                        mapping.MapBindings(i + 1);
                        PlayerMappings.Add(mapping);
                    }

                    if (PlayerMappings.Count >= MaxController + 1)
                        break;
                }
            }

            // WebGL needs to check periodically
#if !UNITY_WEBGL
            if (DetectController)
#endif
                StartCoroutine(CheckForNewControllersCoroutine());
        }

        public void RefreshControllers()
        {
            // This creates alloc (in editor at least, need to verify when build)
            // Sucks there isn't a non-alloc version
            var devices = Input.GetJoystickNames();

            // This assumes that everything that is detected by Unity, stays seen by GetJoystickNames, either by name or as empty. (Also skip 1 because of keyboard)
            for (var i = 1; i < PlayerMappings.Count; i++)
            {
                if (devices.Length >= PlayerMappings[i].OriginalIndex)
                {
                    if (string.IsNullOrEmpty(devices[PlayerMappings[i].OriginalIndex]))
                    {
                        if (!PlayerMappings[i].IsDisconnected)
                        {
                            PlayerMappings[i].IsDisconnected = true;
                            OnControllerDisconnected?.Invoke(i);
                        }
                    }
                    else if (PlayerMappings[i].IsDisconnected)
                    {
                        PlayerMappings[i].IsDisconnected = false;
                        OnNewControllerConnected?.Invoke(i);
                    }
                }
            }

            if (PlayerMappings.Count >= MaxController + 1) return;
            
            for (var i = 0; i < devices.Length; i++)
            {
                if (string.IsNullOrEmpty(devices[i])) continue;
                
                var alreadyExists = false;
                
                // Check that the old mappings aren't made again due to a shift in the devices.
                for (var x = 1; x < PlayerMappings.Count; x++)
                {
                    if (PlayerMappings[x].OriginalIndex == i)
                    {
                        alreadyExists = true;
                    }
                }
                
                if (alreadyExists) continue;

                if (NameToInputMappingLookupTable.TryGetValue(devices[i], out var typeofInput))
                {
                    InputMapping instance = null;
                    switch (typeofInput)
                    {
                        case 0:
                            instance = new PS4Mapping();
                            break;
                        case 1:
                            instance = new Xbox360Mapping();
                            break;
                        case 2:
                            instance = new XboxOneMapping();
                            break;
                        case 3:
                            instance = new SwitchProControllerMapping();
                            break;
                    }
                    
                    instance.OriginalIndex = i;
                    instance.MapBindings(i + 1);

                    PlayerMappings.Add(instance);
                }
#if UNITY_WEBGL
                else
#else
                else if (!SkipUnknown)
#endif
                {
                    var mapping = new Xbox360Mapping();
                    mapping.OriginalIndex = i;
                    mapping.MapBindings(i + 1);
                        
                    PlayerMappings.Add(mapping);
                }

                OnNewControllerConnected?.Invoke(PlayerMappings.Count - 1);
                
                if (PlayerMappings.Count >= MaxController + 1)
                    break;
            }
        }
        
        private IEnumerator CheckForNewControllersCoroutine()
        {
            while (true)
            {
                yield return _wait;
                
                RefreshControllers();
            }
        }

        private void Update()
        {
            if (!HasUpdatedFrame)
            {
                UpdateStates();
                HasUpdatedFrame = true;
            }
        }

        public void UpdateStates()
        {
            foreach (var state in _axisToButtonStates)
            {
                var value = GetAxisValue(state.Axis, state.BelongingMapping);

                if (state.DoesAxisMatter == PositiveNegativeAxis.Indifferent)
                    state.PressedInCurrentFrame = value > 0.01f;
                else
                    state.PressedInCurrentFrame = state.DoesAxisMatter == PositiveNegativeAxis.Negative ? value < -0.1f : value > 0.1f;
            }
            
            HasUpdatedFrame = true;
        }

        private void LateUpdate()
        {
            foreach (var state in _axisToButtonStates)
                state.PressedLastFrame = state.PressedInCurrentFrame;
            
            HasUpdatedFrame = false;
        }

        #region Regular Buttons

        public bool GetButtonDown(GamepadButton button, int playerNumber)
        {
            if (playerNumber >= PlayerMappings.Count) return false;
            return Input.GetKeyDown(PlayerMappings[playerNumber].ButtonBindingLookupTable[button]);
        }

        public bool GetButton(GamepadButton button, int playerNumber)
        {
            if (playerNumber >= PlayerMappings.Count) return false;
            return Input.GetKey(PlayerMappings[playerNumber].ButtonBindingLookupTable[button]);
        }

        public bool GetButtonUp(GamepadButton button, int playerNumber)
        {
            if (playerNumber >= PlayerMappings.Count) return false;
            return Input.GetKeyUp(PlayerMappings[playerNumber].ButtonBindingLookupTable[button]);
        }

        #endregion Regular Buttons

        /// <summary>
        /// Mimics an axis to act like a button. This is useful for triggers on controllers like PS4 or Xbox, where you want the triggers to be a button, and not an analog value
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="playerNumber"></param>
        /// <returns></returns>
        public bool GetAxisAsButtonDown(GamepadAxis axis, int playerNumber)
        {
            if (playerNumber >= PlayerMappings.Count) return false;
            
            var mapping = _axisToButtonStates.Where(state => state.BelongingMapping == PlayerMappings[playerNumber] && state.Axis == axis && state.DoesAxisMatter == PositiveNegativeAxis.Indifferent).ToList();
            if (mapping.Count != 0)
            {
                return mapping[0].PressedInCurrentFrame && !mapping[0].PressedLastFrame;
            }
            else
            {
                //Add to list (Basically "subscribe" to it)
                var newAxisState = new AxisState() { BelongingMapping = PlayerMappings[playerNumber], Axis = axis };
                if (PlayerMappings[playerNumber].OverridesAxisReading)
                {
                    var value = PlayerMappings[playerNumber].OverrideAxisReading(axis);
                    newAxisState.PressedInCurrentFrame = value != 0;
                }
                else
                {
                    var info = PlayerMappings[playerNumber].AxisBindingLookupTable[axis];
                    var value = GetAxisValue(axis, playerNumber);

                    var multiplier = value >= info.UnpressedValue ? 1 : -1;
                    if (multiplier < 0)
                    {
                        newAxisState.PressedInCurrentFrame = value < info.UnpressedValue + (multiplier * info.DeadZoneOffset);
                    }
                    else
                    {
                        newAxisState.PressedInCurrentFrame = value > info.UnpressedValue + (multiplier * info.DeadZoneOffset);
                    }
                }
                _axisToButtonStates.Add(newAxisState);
                return newAxisState.PressedInCurrentFrame;
            }
        }

        /// <summary>
        /// Mostly used for DPAD on controllers, since you want to access each axis, as a button, but internally, it is seen as one axis.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="playerNumber"></param>
        /// <param name="whichDirection"></param>
        /// <returns></returns>
        public bool GetAxisAsButtonDown(GamepadAxis axis, int playerNumber, PositiveNegativeAxis whichDirection)
        {
            if (playerNumber >= PlayerMappings.Count) return false;
            
            var mapping = _axisToButtonStates.Where(state => state.BelongingMapping == PlayerMappings[playerNumber] && state.Axis == axis && state.DoesAxisMatter == whichDirection).ToList();
            if (mapping.Count != 0)
            {
                return mapping[0].PressedInCurrentFrame && !mapping[0].PressedLastFrame;
            }
            else
            {
                //Add to list (Basically "subscribe" to it)
                var newAxisState = new AxisState() { BelongingMapping = PlayerMappings[playerNumber], Axis = axis, DoesAxisMatter = whichDirection };
                //newAxisState.PressedInCurrentFrame = this.PlayerMappings[playerNumber].OverridesAxisReading ? this.PlayerMappings[playerNumber].OverrideAxisReading(axis) > 0.01f : Input.GetAxisRaw(this.PlayerMappings[playerNumber].AxisBindingLookupTable[axis]) > 0.01f;
                if (PlayerMappings[playerNumber].OverridesAxisReading)
                {
                    var value = PlayerMappings[playerNumber].OverrideAxisReading(axis);
                    newAxisState.PressedInCurrentFrame = value != 0;
                }
                else
                {
                    var info = PlayerMappings[playerNumber].AxisBindingLookupTable[axis];
                    var value = GetAxisValue(axis, playerNumber);

                    var multiplier = whichDirection == PositiveNegativeAxis.Negative ? -1 : 1;
                    if (multiplier < 0)
                    {
                        newAxisState.PressedInCurrentFrame = value < info.UnpressedValue + (multiplier * info.DeadZoneOffset);
                    }
                    else
                    {
                        newAxisState.PressedInCurrentFrame = value > info.UnpressedValue + (multiplier * info.DeadZoneOffset);
                    }
                }
                _axisToButtonStates.Add(newAxisState);
                return newAxisState.PressedInCurrentFrame;
            }
        }

        public bool GetAxisAsButton(GamepadAxis axis, int playerNumber)
        {
            var axisVal = GetAxisValue(axis, playerNumber);
            return Mathf.Abs(axisVal) > 0.01f;
        }

        public bool GetAxisAsButtonUp(GamepadAxis axis, int playerNumber)
        {
            if (playerNumber >= PlayerMappings.Count) return false;
            
            var mapping = _axisToButtonStates.Where(state => state.BelongingMapping == PlayerMappings[playerNumber] && state.Axis == axis && state.DoesAxisMatter == PositiveNegativeAxis.Indifferent).ToList();
            if (mapping.Count != 0)
            {
                return !mapping[0].PressedInCurrentFrame && mapping[0].PressedLastFrame;
            }
            else
            {
                //Add to list (Basically "subscribe" to it
                var newAxisState = new AxisState() { BelongingMapping = PlayerMappings[playerNumber], Axis = axis };

                if (PlayerMappings[playerNumber].OverridesAxisReading)
                {
                    var value = PlayerMappings[playerNumber].OverrideAxisReading(axis);
                    newAxisState.PressedInCurrentFrame = value != 0;
                }
                else
                {
                    var info = PlayerMappings[playerNumber].AxisBindingLookupTable[axis];
                    var value = Input.GetAxisRaw(info.AxisName);

                    var multiplier = value >= info.UnpressedValue ? 1 : -1;
                    if (multiplier < 0)
                    {
                        newAxisState.PressedInCurrentFrame = value < info.UnpressedValue + (multiplier * info.DeadZoneOffset);
                    }
                    else
                    {
                        newAxisState.PressedInCurrentFrame = value > info.UnpressedValue + (multiplier * info.DeadZoneOffset);
                    }
                }
                //newAxisState.PressedInCurrentFrame = value > info.UnpressedValue + (multiplier * info.DeadZoneOffset);
                _axisToButtonStates.Add(newAxisState);
                return !newAxisState.PressedInCurrentFrame && newAxisState.PressedLastFrame;
            }
        }

        public bool GetAxisAsButtonUp(GamepadAxis axis, int playerNumber, PositiveNegativeAxis whichDirection)
        {
            if (playerNumber >= PlayerMappings.Count) return false;
            
            var mapping = _axisToButtonStates.Where(state => state.BelongingMapping == PlayerMappings[playerNumber] && state.Axis == axis && state.DoesAxisMatter == whichDirection).ToList();
            if (mapping.Count != 0)
            {
                return !mapping[0].PressedInCurrentFrame && mapping[0].PressedLastFrame;
            }
            else
            {
                //Add to list (Basically "subscribe" to it
                var newAxisState = new AxisState() { BelongingMapping = PlayerMappings[playerNumber], Axis = axis, DoesAxisMatter = whichDirection };

                if (PlayerMappings[playerNumber].OverridesAxisReading)
                {
                    var value = PlayerMappings[playerNumber].OverrideAxisReading(axis);
                    newAxisState.PressedInCurrentFrame = value != 0;
                }
                else
                {
                    var info = PlayerMappings[playerNumber].AxisBindingLookupTable[axis];
                    var value = Input.GetAxisRaw(info.AxisName);

                    var multiplier = whichDirection == PositiveNegativeAxis.Negative ? -1 : 1;
                    if (multiplier < 0)
                    {
                        newAxisState.PressedInCurrentFrame = value < info.UnpressedValue + (multiplier * info.DeadZoneOffset);
                    }
                    else
                    {
                        newAxisState.PressedInCurrentFrame = value > info.UnpressedValue + (multiplier * info.DeadZoneOffset);
                    }
                }
                //newAxisState.PressedInCurrentFrame = value > info.UnpressedValue + (multiplier * info.DeadZoneOffset);
                _axisToButtonStates.Add(newAxisState);
                return !newAxisState.PressedInCurrentFrame && newAxisState.PressedLastFrame;
            }
        }

        public float GetAxisValue(GamepadAxis axis, int playerNumber)
        {
            if (playerNumber >= PlayerMappings.Count) return 0f;
            
            if (PlayerMappings[playerNumber].OverridesAxisReading)
            {
                return PlayerMappings[playerNumber].OverrideAxisReading(axis);
            }

            var input = Input.GetAxisRaw(PlayerMappings[playerNumber].AxisBindingLookupTable[axis].AxisName);
            //int multiplier = input > this.PlayerMappings[playerNumber].AxisBindingLookupTable[axis].UnpressedValue ? 1 : -1;
            //Make sure that we don't get false positives, due to the deadzone there is on buttons
            //bool valueBiggerThanDeadzone = input > this.PlayerMappings[playerNumber].AxisBindingLookupTable[axis].UnpressedValue + (multiplier * this.PlayerMappings[playerNumber].AxisBindingLookupTable[axis].DeadZoneOffset);

            if (IsValueInDeadzone(input, PlayerMappings[playerNumber].AxisBindingLookupTable[axis]))
            {
                return 0.0f;
            }
            return input * (PlayerMappings[playerNumber].AxisBindingLookupTable[axis].Inverted ? -1 : 1);
        }

        public float GetAxisValue(GamepadAxis axis, InputMapping mapping)
        {
            if (mapping.OverridesAxisReading)
            {
                return mapping.OverrideAxisReading(axis);
            }
            var input = Input.GetAxisRaw(mapping.AxisBindingLookupTable[axis].AxisName);
            //bool valueBiggerThanDeadzone = Math.Abs(input) > mapping.AxisBindingLookupTable[axis].UnpressedValue + mapping.AxisBindingLookupTable[axis].DeadZoneOffset;
            if (IsValueInDeadzone(input, mapping.AxisBindingLookupTable[axis]))
            {
                return 0.0f;
            }
            return input * (mapping.AxisBindingLookupTable[axis].Inverted ? -1 : 1);
        }

        public Vector2 GetCombinedAxis(GamepadAxis axisX, GamepadAxis axisY, int playerNumber, float deadZone = 0.0f)
        {
            if (playerNumber >= PlayerMappings.Count) return Vector2.zero;
            
            var VectorToReturn = new Vector2();
            if (PlayerMappings[playerNumber].OverridesAxisReading)
            {
                VectorToReturn.x = PlayerMappings[playerNumber].OverrideAxisReading(axisX);
                VectorToReturn.y = PlayerMappings[playerNumber].OverrideAxisReading(axisY);
                return VectorToReturn;
            }

            VectorToReturn.x = Input.GetAxisRaw(PlayerMappings[playerNumber].AxisBindingLookupTable[axisX].AxisName) * (PlayerMappings[playerNumber].AxisBindingLookupTable[axisX].Inverted ? -1 : 1);
            VectorToReturn.y = Input.GetAxisRaw(PlayerMappings[playerNumber].AxisBindingLookupTable[axisY].AxisName) * (PlayerMappings[playerNumber].AxisBindingLookupTable[axisY].Inverted ? -1 : 1);
       
            if (VectorToReturn.magnitude <= deadZone)
            {
                return Vector2.zero;
            }

            //if (this.IsValueInDeadzone(VectorToReturn.x, this.PlayerMappings[playerNumber].AxisBindingLookupTable[AxisX]))
            //{
            //    return 0.0f;
            //}
            return VectorToReturn;
        }

        private bool IsValueInDeadzone(float value, GamepadAxisInfo info)
        {
            var multiplier = value >= info.UnpressedValue ? 1 : -1;
            //Make sure that we don't get false positives, due to the deadzone there is on buttons
            if (multiplier < 0)
            {
                return value > info.UnpressedValue + (multiplier * info.DeadZoneOffset);
            }
            return value < info.UnpressedValue + (multiplier * info.DeadZoneOffset);
        }

        /// <summary>
        /// For each supported input mapping, get their aliasses and add to the dictionary for later use.
        /// </summary>
        private void FillNameToInputMappingLookupTable()
        {
            for (var i = 0; i < _supportedInputMappings.Count; i++)
            {
                var aliases = _supportedInputMappings[i];
                foreach (var alias in aliases)
                    NameToInputMappingLookupTable[alias] = i;
            }
        }
    }
}