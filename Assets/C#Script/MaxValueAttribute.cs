using UnityEngine;

public class MaxValueAttribute : PropertyAttribute
{
    public float Max { get; }

    public MaxValueAttribute(float max)
    {
        Max = max;
    }
}
