using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    Camera cam;
    private float moveSpeed = 15.0f;
    private float zoomSpeed = 10.0f;
    public bool CameraMovementActivate;

    public bool isFocus;
    public GameObject unitToFocus;
    private float smooth = 0.2f;
    private float padding = 1.3f;
    private float focusZoom = 5.0f;

    void Awake()
    {
        cam = gameObject.GetComponent<Camera>();
        CameraMovementActivate = false;
        isFocus = false;
    }

    void Update()
    {
        if(CameraMovementActivate)
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += transform.up * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position -= transform.up * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position -= transform.right * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += transform.right * moveSpeed * Time.deltaTime;
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if(cam.orthographicSize - scroll * zoomSpeed < 1)
            {
                cam.orthographicSize = 1;
            }

            else
            {
                cam.orthographicSize -= scroll * zoomSpeed;
                moveSpeed -= scroll * moveSpeed / 2;
            }
        }
    }

    void FixedUpdate()
    {
        if(isFocus && unitToFocus != null)
        {
            Vector3 targetPos = new Vector3(unitToFocus.transform.position.x, unitToFocus.transform.position.y, unitToFocus.transform.position.z - padding);
            
            if(Vector3.Distance(transform.position, targetPos) >= 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, smooth);
            }

            else
            {
                isFocus = false;
            }
        }
    }

    public void InitFocusUnit()
    {
        isFocus = false;
        unitToFocus = null;
    }

    public void FocusUnit(GameObject unitObject)
    {
        isFocus = true;
        unitToFocus = unitObject;
        cam.orthographicSize = focusZoom;
    }

    public void MoveCameraToCenter()
    {
        GridManager Grid = GameObject.FindWithTag("GridManager").GetComponent<GridManager>();
        gameObject.transform.position = new Vector3((float)Grid.getWidth()/2, (float)Grid.getHeight()/2, -10);
    }
}
