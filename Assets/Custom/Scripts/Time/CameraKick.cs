// CameraKick.cs
using UnityEngine;

public class CameraKick : MonoBehaviour
{
    [SerializeField] private LocalTimeManager timeManager;
    [SerializeField] private Camera cam;
    [SerializeField] private float addFov = 8f;
    [SerializeField] private float upTime = 0.08f;
    [SerializeField] private float holdTime = 0.12f;
    [SerializeField] private float downTime = 0.2f;

    float baseFov;

    void Awake(){ if (!cam) cam = Camera.main; baseFov = cam.fieldOfView; }

    void OnEnable(){ timeManager.SlowStarted += Kick; timeManager.SlowEnded += ResetKick; }
    void OnDisable(){ timeManager.SlowStarted -= Kick; timeManager.SlowEnded -= ResetKick; }

    void Kick(){ StopAllCoroutines(); StartCoroutine(KickCo()); }
    void ResetKick(){ StopAllCoroutines(); StartCoroutine(BackCo()); }

    System.Collections.IEnumerator KickCo()
    {
        float t=0f; float start=cam.fieldOfView; float target=baseFov+addFov;
        while(t<upTime){ t+=Time.unscaledDeltaTime; cam.fieldOfView=Mathf.Lerp(start,target,t/upTime); yield return null; }
        t=0f; while(t<holdTime){ t+=Time.unscaledDeltaTime; yield return null; }
    }
    System.Collections.IEnumerator BackCo()
    {
        float t=0f; float start=cam.fieldOfView; float target=baseFov;
        while(t<downTime){ t+=Time.unscaledDeltaTime; cam.fieldOfView=Mathf.Lerp(start,target,t/downTime); yield return null; }
        cam.fieldOfView=baseFov;
    }
}
