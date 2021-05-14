using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    #region SINGLETON
    private static CameraManager instance;
    private CameraManager() { }
    public static CameraManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CameraManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<CameraManager>();
                }
            }
            return instance;
        }
    }
    #endregion

    [Header("Camera Controls")]
    public CinemachineVirtualCameraBase virtualCamera;
    CinemachineRecomposer vCamRecomposer;

    [Header("Camera Move Settings")]
    public float cameraMoveRange = 5f;
    public float cameraMoveSensitivity = 10f;
    [Header("Camera Zoom Settings")]
    public float cameraZoomSensitivity = 0.1f;
    public float cameraMinZoom = 1f;
    public float cameraMaxZoom = 0.5f;

    public void PreInitialize()
    {
        vCamRecomposer = virtualCamera.GetComponent<CinemachineRecomposer>();
    }

    public void Refresh()
    {
        if (InputManager.Instance.canUseInputs)
            CameraControls();
    }

    /// <summary>
    /// Allow Use to move the Camera around the world
    /// </summary>
    public void CameraControls()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            if (vCamRecomposer.m_ZoomScale > cameraMaxZoom)
                vCamRecomposer.m_ZoomScale -= cameraZoomSensitivity;
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            if (vCamRecomposer.m_ZoomScale < cameraMinZoom)
                vCamRecomposer.m_ZoomScale += cameraZoomSensitivity;
        }

        if (InputManager.Instance.keyboard.wKey.isPressed && virtualCamera.transform.position.y < cameraMoveRange)
            virtualCamera.transform.position += new Vector3(0f, cameraMoveSensitivity, 0f) * Time.deltaTime;
        else if (InputManager.Instance.keyboard.sKey.isPressed && virtualCamera.transform.position.y > -cameraMoveRange)
            virtualCamera.transform.position -= new Vector3(0f, cameraMoveSensitivity, 0f) * Time.deltaTime;
        else if (InputManager.Instance.keyboard.aKey.isPressed && virtualCamera.transform.position.x > -cameraMoveRange)
            virtualCamera.transform.position -= new Vector3(cameraMoveSensitivity, 0f, 0f) * Time.deltaTime;
        else if (InputManager.Instance.keyboard.dKey.isPressed && virtualCamera.transform.position.x < cameraMoveRange)
            virtualCamera.transform.position += new Vector3(cameraMoveSensitivity, 0f, 0f) * Time.deltaTime;
    }
}
