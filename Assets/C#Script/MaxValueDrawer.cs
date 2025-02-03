using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MaxValueAttribute))]
public class MaxValueDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MaxValueAttribute maxValueAttribute = (MaxValueAttribute)attribute;

        if (property.propertyType == SerializedPropertyType.Integer)
        {
            property.intValue = Mathf.Min(EditorGUI.IntField(position, label, property.intValue), (int)maxValueAttribute.Max);
        }
        else if (property.propertyType == SerializedPropertyType.Float)
        {
            property.floatValue = Mathf.Min(EditorGUI.FloatField(position, label, property.floatValue), maxValueAttribute.Max);
        }
    }
}
