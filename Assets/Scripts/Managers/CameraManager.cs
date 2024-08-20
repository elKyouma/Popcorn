using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    //public enum CameraModes
    //{
    //    Follow,
    //    Fixed
    //}
    //private CameraModes currentCameraMode = CameraModes.Fixed;
    enum ZoomLevels
    {
        SmallShip,
        NormalShip,
        BigShip,
        HugeShip
    }
    [SerializeField] private ZoomLevels currentZoomLevel = ZoomLevels.NormalShip;
    Dictionary<ZoomLevels, float> zoomLevels = new Dictionary<ZoomLevels, float> {
        { ZoomLevels.SmallShip, 0.5f },
        { ZoomLevels.NormalShip, 1.0f },
        { ZoomLevels.BigShip, 1.5f },
        { ZoomLevels.HugeShip, 2.0f }
    };

    private Transform target;
    [SerializeField]
    private float defaultZoom = 5.0f;
    private CinemachineVirtualCamera virtualCamera;

    [Header("Zoom on Velocity")]
    [Tooltip("Maximum FOV when ship is at speed threshold")]
    [SerializeField] private float maxFOV = 100;
    [Tooltip("At what speed should the camera be at max FOV")]
    [SerializeField] private float speedThreshold = 10;
    private float extraZoom = 0;

    private void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        target = virtualCamera.Follow;
    }

    private void Update()
    {
        AdjustExtraZoom();
        SetCamZoomMultiplier(zoomLevels[currentZoomLevel]);
    }

    public void AdjustExtraZoom()
    {
        if(target.GetComponent<Rigidbody2D>())
            extraZoom = target.GetComponent<Rigidbody2D>().velocity.magnitude > speedThreshold ? maxFOV : 0;
    }

    public void SetCamZoomMultiplier(float zoom)
    {
        float current = virtualCamera.m_Lens.OrthographicSize;
        float target = defaultZoom * zoom + extraZoom;

        if (current == target) return;

        PrimeTween.Tween.Custom(current, target, 0.5f, (float x) => { virtualCamera.m_Lens.OrthographicSize = x;  });
    }
}
