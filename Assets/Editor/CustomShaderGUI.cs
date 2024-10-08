using UnityEditor;
using UnityEngine;

public class CustomShaderGUI : ShaderGUI
{
    private MaterialProperty _mode;
    private MaterialProperty _baseMap;
    private MaterialProperty _color;
    private MaterialProperty _fade;
    private MaterialProperty _geoRes;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // Find properties
        _mode = FindProperty("_Mode", properties);
        _baseMap = FindProperty("_BaseMap", properties);
        _color = FindProperty("_Color", properties);
        _fade = FindProperty("_Fade", properties);
        _geoRes = FindProperty("_GeoRes", properties);

        EditorGUI.BeginChangeCheck();

        // Dropdown for selecting mode
        string[] options = { "Texture", "Color" };
        _mode.floatValue = EditorGUILayout.Popup("Mode", (int)_mode.floatValue, options);

        // Conditionally show the color or texture field
        if (_mode.floatValue == 1)
        {
            materialEditor.ShaderProperty(_color, "Color");
        }
        else
        {
            materialEditor.TexturePropertySingleLine(new GUIContent("Texture"), _baseMap);
        }

        materialEditor.ShaderProperty(_fade, "Fade Amount");
        materialEditor.ShaderProperty(_geoRes, "Geometry Resolution");

        if (EditorGUI.EndChangeCheck())
        {
            // Apply changes to the material
            foreach (var obj in _baseMap.targets)
            {
                MaterialChanged((Material)obj);
            }
        }
    }

    private static void MaterialChanged(Material material)
    {
        // Optionally add logic to handle material changes
    }
}