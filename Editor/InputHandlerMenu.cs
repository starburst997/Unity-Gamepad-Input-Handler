using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class InputHandlerMenu : EditorWindow
{
    private static EditorWindow _window;
    private static int _numberOfGamepadsToAdd = 10;
    
    [MenuItem("Tools/Input Handler/Setup Input Manager")]
    public static void SetupInputManager()
    {
        _window = GetWindow<InputHandlerMenu>("Confirmation");
        _window.maxSize = new Vector2(600, 200);
        _window.minSize = new Vector2(600, 200);
    }

    private void OnGUI()
    {
        GUI.skin.label.wordWrap = true;
        GUILayout.Label("This will populate the Input Manager with 20 entries per gamepad supported, do you wish to continue?");
        GUILayout.Space(10);

        var recover = true;
        recover = GUILayout.Toggle(recover, "Create backup of old InputManager before making new?");
        GUILayout.Space(10);

        GUILayout.Label("How many gamepads do you want to support? Note that if you have more gamepads connected, than you support, you may run into issues, so recommended would be around 10 or so.");
        _numberOfGamepadsToAdd = EditorGUILayout.IntSlider(_numberOfGamepadsToAdd, 1, 16);
        
        if (GUILayout.Button("Yes"))
        {
            if (recover)
                SaveCopyOfInputManager();
            
            FillManagerWithJoysticks();
        }
        GUILayout.Space(10);
        if (GUILayout.Button("No"))
        {
            _window.Close();
        }
    }

    private void SaveCopyOfInputManager()
    {
        var manager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];

        string finalName = GetUniqueName("InputManagerBackup", Application.dataPath + "/", ".txt");
        File.Copy("ProjectSettings/InputManager.asset", Application.dataPath + "/" + finalName);
    }

    private string GetUniqueName(string name, string folderPath, string extension)
    {
        string validatedName = name + extension;
        int number = 1;
        while (File.Exists(folderPath + validatedName))
        {
            validatedName = string.Format("{0} [{1}]" + extension, name, number++);
        }
        return validatedName;
    }

    private void FillManagerWithJoysticks()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 1; i <= _numberOfGamepadsToAdd; i++)
        {
            for (int x = 0; x < 20; x++)
            {
                sb.Append("\n");
                sb.Append(string.Format(
                    @"  - serializedVersion: 3
    m_Name: joystick {0} analog {1}
    descriptiveName:
    descriptiveNegativeName: 
    negativeButton:
    positiveButton: 
    altNegativeButton:
    altPositiveButton: 
    gravity: 0
    dead: 0.19
    sensitivity: 1
    snap: 0
    invert: 0
    type: 2
    axis: {1}
    joyNum: {0}", i, x));
            }
        }

        File.AppendAllText("ProjectSettings/InputManager.asset", sb.ToString());

        AssetDatabase.Refresh();
    }
}
