using UnityEditor;
using UnityEngine;

// Custom Editor to order the variables in the Inspector similar to Image component
[CustomEditor(typeof(GradientGraphic)), CanEditMultipleObjects]
public class GradientGraphicEditor : Editor
{
    private SerializedProperty spriteProp;
    private SerializedProperty topLeftColorProp;
    private SerializedProperty topRightColorProp;
    private SerializedProperty bottomLeftColorProp;
    private SerializedProperty bottomRightColorProp;
    private SerializedProperty gradientSmoothnessProp;
    private SerializedProperty useSlicedSpriteProp;
    private SerializedProperty fillCenterProp;
    private SerializedProperty pixelsPerUnitMultiplierProp;

    private GUIContent spriteLabel;

    private void OnEnable()
    {
        spriteProp = serializedObject.FindProperty("sprite");
        topLeftColorProp = serializedObject.FindProperty("topLeftColor");
        topRightColorProp = serializedObject.FindProperty("topRightColor");
        bottomLeftColorProp = serializedObject.FindProperty("bottomLeftColor");
        bottomRightColorProp = serializedObject.FindProperty("bottomRightColor");
        gradientSmoothnessProp = serializedObject.FindProperty("gradientSmoothness");
        useSlicedSpriteProp = serializedObject.FindProperty("useSlicedSprite");
        fillCenterProp = serializedObject.FindProperty("fillCenter");
        pixelsPerUnitMultiplierProp = serializedObject.FindProperty("pixelsPerUnitMultiplier");

        spriteLabel = new GUIContent("Source Image");
    }

    public override void OnInspectorGUI()
    {
        bool isSlicedSpriteAssigned = ((GradientGraphic)target).hasBorder;

        serializedObject.Update();

        EditorGUILayout.PropertyField(spriteProp, spriteLabel);
        EditorGUILayout.PropertyField(topLeftColorProp);
        EditorGUILayout.PropertyField(topRightColorProp);
        EditorGUILayout.PropertyField(bottomLeftColorProp);
        EditorGUILayout.PropertyField(bottomRightColorProp);
        EditorGUILayout.PropertyField(gradientSmoothnessProp);

        DrawPropertiesExcluding(serializedObject, "m_Script", "sprite", "m_Color", "topLeftColor", "topRightColor", "bottomLeftColor", "bottomRightColor", "gradientSmoothness", "useSlicedSprite", "fillCenter", "pixelsPerUnitMultiplier", "m_OnCullStateChanged");

        if (isSlicedSpriteAssigned)
        {
            EditorGUILayout.PropertyField(useSlicedSpriteProp);

            EditorGUI.indentLevel++;
            if (useSlicedSpriteProp.boolValue || useSlicedSpriteProp.hasMultipleDifferentValues)
            {
                EditorGUILayout.PropertyField(fillCenterProp);
                EditorGUILayout.PropertyField(pixelsPerUnitMultiplierProp);
            }
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
