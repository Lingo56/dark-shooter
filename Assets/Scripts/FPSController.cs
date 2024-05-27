using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private float _walkSpeed = 6f;
    [SerializeField] private float _runSpeed = 6f;
    [SerializeField] private float _jumpForce = 6f;
    [SerializeField] private float _gravity = 6f;
    [SerializeField] private float _pushPower = 2.0f;

    [SerializeField] private float _lookSpeed = 2f;
    [SerializeField] private float _lookXLimit = 45f;

    private Vector3 _moveDirection = Vector3.zero;
    private float _rotationX = 0;

    [SerializeField] private bool _canMove = true;

    private CharacterController _characterController;

    private Quaternion shakeOffset = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        #region Move
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = _canMove ? (isRunning ? _runSpeed : _walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = _canMove ? (isRunning ? _runSpeed : _walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = _moveDirection.y;
        _moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        #endregion

        #region Jump
        if (Input.GetButton("Jump") && _canMove && _characterController.isGrounded)
        {
            _moveDirection.y = _jumpForce;
        }
        else 
        {
            _moveDirection.y = movementDirectionY;
        }

        if (!_characterController.isGrounded) {
            _moveDirection.y -= _gravity * Time.deltaTime;
        }

        #endregion

        #region Camera
        _characterController.Move(_moveDirection * Time.deltaTime);

        if (_canMove) {
            _rotationX += -Input.GetAxis("Mouse Y") * _lookSpeed;
            _rotationX = Mathf.Clamp(_rotationX, -_lookXLimit, _lookXLimit);
            _playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0) * shakeOffset;
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * _lookSpeed, 0);
        }

        #endregion

    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic)
        {
            return;
        }

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3)
        {
            return;
        }

        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // If you know how fast your character is trying to move,
        // then you can also multiply the push velocity by that.

        // Apply the push
        body.velocity = pushDir * _pushPower;
    }

    public IEnumerator ShakeCamera(float shakeAmount, float shakeRiseDuration, float shakeFallDuration)
    {
        float elapsed = 0.0f;
        float currentShakeAmount = 0.0f;

        // Shake the camera
        while (elapsed < shakeRiseDuration)
        {
            float t = elapsed / shakeRiseDuration; // Calculate interpolation parameter
            currentShakeAmount = Mathf.Lerp(0, shakeAmount, t);
            shakeOffset = Quaternion.Euler(-currentShakeAmount, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final rotation is the target rotation
        shakeOffset = Quaternion.Euler(-shakeAmount, 0, 0);

        // Reset the elapsed time
        elapsed = 0.0f;

        // Smoothly interpolate back to original rotation
        while (elapsed < shakeFallDuration)
        {
            float t = elapsed / shakeFallDuration; // Calculate interpolation parameter
            currentShakeAmount = Mathf.Lerp(shakeAmount, 0, t);
            shakeOffset = Quaternion.Euler(-currentShakeAmount, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final rotation is exactly the original one
        shakeOffset = Quaternion.identity;
    }
}
