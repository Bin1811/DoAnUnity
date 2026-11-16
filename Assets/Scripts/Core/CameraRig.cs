using UnityEngine;

// Giu cho ban do luon nam gon trong khung hinh voi moi ty le man hinh.
[DefaultExecutionOrder(50)]
public class CameraFit : MonoBehaviour
{
    public const float DesignWidth = 22.8f;
    public const float DesignHeight = 13.8f;
    public const float CenterY = -0.5f;

    public static Vector3 BasePosition => new Vector3(0f, CenterY, -10f);

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (cam == null) cam = GetComponent<Camera>();
        float aspect = Mathf.Max(1.2f, (float)Screen.width / Mathf.Max(1, Screen.height));
        float halfH = Mathf.Max(DesignHeight * 0.5f, DesignWidth / (2f * aspect));
        cam.orthographic = true;
        cam.orthographicSize = halfH;
        transform.position = BasePosition;
        transform.rotation = Quaternion.identity;
    }
}

[DefaultExecutionOrder(150)]
public class CameraShake : MonoBehaviour
{
    public static CameraShake I;

    float timer;
    float strength;

    void Awake()
    {
        I = this;
    }

    public void Shake(float amount)
    {
        strength = Mathf.Max(strength, amount);
        timer = Mathf.Max(timer, 0.32f);
    }

    void LateUpdate()
    {
        if (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(timer / 0.32f);
            float amp = strength * k;
            float t = Time.unscaledTime * 60f;
            Vector3 offset = new Vector3(Mathf.Sin(t * 1.7f) * amp, Mathf.Cos(t * 2.3f) * amp, 0f);
            transform.position = CameraFit.BasePosition + offset;
            if (timer <= 0f) strength = 0f;
        }
        else
        {
            transform.position = CameraFit.BasePosition;
        }
    }
}
