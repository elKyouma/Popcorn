using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    Dictionary<string, float> zoomLevels = new Dictionary<string, float> {
        {"SmallShip", 0.5f},
        {"NormalShip", 1f},
        {"BigShip", 2f},
        {"HugeShip", 4f}
    };


    [SerializeField]
    private const float defaultZoom = 5.0f;
    CinemachineVirtualCamera virtualCamera;

    private void Start() {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    // temporary solution
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SetCamZoomMultiplier(zoomLevels["SmallShip"]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SetCamZoomMultiplier(zoomLevels["NormalShip"]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            SetCamZoomMultiplier(zoomLevels["BigShip"]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            SetCamZoomMultiplier(zoomLevels["HugeShip"]);
        }
    }

    public void SetCamZoomMultiplier(float zoom) {
        float current = virtualCamera.m_Lens.OrthographicSize;
        float target = defaultZoom * zoom;
        Debug.Log("Current: " + current + " Target: " + target);

        // Smoothly interpolate between the current size and the target size.
        LeanTween.value(gameObject, current, target, 0.5f).setOnUpdate((float val) => {
            virtualCamera.m_Lens.OrthographicSize = val;
        });
    }

}