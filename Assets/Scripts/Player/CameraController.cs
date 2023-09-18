using UnityEngine;

public class CameraController : MonoBehaviour, IInitialize
{
    /////////////////////////////////////////////////////////////////////////////////////

    public float mouseSensitivity = 0.5f;
    public float verticalLimit = 60f;

    /////////////////////////////////////////////////////////////////////////////////////

    private Transform playerTransform;
    private Vector3 posOffset;
    private Quaternion originalRotation;

    public bool isActive { get; set; } = false;

    /////////////////////////////////////////////////////////////////////////////////////

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Store the player's transform
        playerTransform = GameObject.Find("Player Body").transform;

        // Store the original rotation of the camera
        originalRotation = transform.localRotation;
        
        // Store offset from player
        posOffset = transform.position - playerTransform.position;

        isActive = true;
    }

    public void Deinitialize()
    {
        isActive = false;
    }

    /////////////////////////////////////////////////////////////////////////////////////

    void LateUpdate()
    {
        if (isActive == false) return;

        if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Home))
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }

        MouseLook();
        SetPosition();
    }

    /////////////////////////////////////////////////////////////////////////////////////

    private float xRotation = 0f;

    void MouseLook()
    {
        // Get the mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Adjust the vertical rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalLimit, verticalLimit);

        // Combine the vertical rotation with the horizontal rotation and apply it to the camera
        transform.localRotation = Quaternion.Euler(xRotation, transform.localEulerAngles.y + mouseX, 0f);
        playerTransform.localRotation = Quaternion.Euler(0f, transform.localEulerAngles.y, 0f);
    }

    /////////////////////////////////////////////////////////////////////////////////////
    
    void SetPosition()
    {
        // Set the camera's position to the player's forward
        transform.position = playerTransform.position + posOffset;
    }
}
