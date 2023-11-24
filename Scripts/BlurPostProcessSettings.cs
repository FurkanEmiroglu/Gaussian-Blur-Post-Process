using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Gaussian Blur")]
public class BlurPostProcessSettings : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Standard Deviation (spread) of the blur. Grid size is approx. 3x larger.")]
    [SerializeField]
    private ClampedFloatParameter strength = new(0.0f, MinStrength, MaxStrength);

    private const float MinStrength = 0.001f;
    private const float MaxStrength = 15.0f;
    
    public bool IsActive()
    {
        return strength.value > MinStrength && active;
    }

    public bool IsTileCompatible()
    {
        return false;
    }

    public void SetBlurStrength(float str)
    {
        strength.SetValue(new FloatParameter(Mathf.Clamp(str, MinStrength, MaxStrength)));
        strength.SetValue(new FloatParameter(str));
    }
    
    public float GetBlurValue()
    {
        return strength.value * 6.0f;
    }
}
