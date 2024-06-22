using UnityEngine;

public class PlayerWeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float _swayStep = 0.01f;

    [SerializeField] private float _maxSwayStepDistance = 0.06f;
    private Vector3 _swayPos;

    [Header("Sway Rotation Settings")]
    [SerializeField] private float _rotationStep = 4f;

    [SerializeField] private float _maxRotationStep = 5f;
    private Vector3 _swayEulerRot;
    [SerializeField] private float _smooth = 10f;
    private float _smoothRot = 12f;

    [Header("Bob Settings")]
    [SerializeField] private float _speedCurve;

    private float CurveSin { get => Mathf.Sin(_speedCurve); }
    private float CurveCos { get => Mathf.Cos(_speedCurve); }

    [SerializeField] private Vector3 _travelLimit = Vector3.one * 0.025f;
    [SerializeField] private Vector3 _bobLimit = Vector3.one * 0.01f;
    private Vector3 _bobPosition;

    [SerializeField] private float _bobExaggeration;

    [Header("Bob Rotation Settings")]
    [SerializeField] private Vector3 _multiplier;

    private Vector3 _bobEulerRotation;

    private Vector2 _walkInput;
    private Vector2 _lookInput;

    private void Update()
    {
        GetInput();

        Sway();
        SwayRotation();
        BobOffset();
        BobRotation();

        CompositePositionRotation();
    }

    private void GetInput()
    {
        // Player direction
        _walkInput.x = Input.GetAxis("Horizontal");
        _walkInput.y = Input.GetAxis("Vertical");
        _walkInput = _walkInput.normalized;

        // Mouse input
        _lookInput.x = Input.GetAxis("Mouse X");
        _lookInput.y = Input.GetAxis("Mouse Y");
    }

    private void Sway()
    {
        Vector3 invertLook = _lookInput * -_swayStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -_maxSwayStepDistance, _maxSwayStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -_maxSwayStepDistance, _maxSwayStepDistance);

        _swayPos = invertLook;
    }

    private void SwayRotation()
    {
        Vector2 invertLook = _lookInput * -_rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -_maxRotationStep, _maxRotationStep);
        invertLook.y = Mathf.Clamp(invertLook.y, -_maxRotationStep, _maxRotationStep);
        _swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
    }

    private void BobOffset()
    {
        _speedCurve += Time.deltaTime * ((Input.GetAxis("Horizontal") + Input.GetAxis("Vertical")) * _bobExaggeration) + 0.01f;

        _bobPosition.x = (CurveCos * _bobLimit.x) - (_walkInput.x * _travelLimit.x);
        _bobPosition.y = (CurveSin * _bobLimit.y) - (Input.GetAxis("Vertical") * _travelLimit.y);
        _bobPosition.z = -(_walkInput.y * _travelLimit.z);
    }

    private void BobRotation()
    {
        _bobEulerRotation.x = (_walkInput != Vector2.zero ? _multiplier.x * (Mathf.Sin(2 * _speedCurve)) : _multiplier.x * (Mathf.Sin(2 * _speedCurve) / 2));
        _bobEulerRotation.y = (_walkInput != Vector2.zero ? _multiplier.y * CurveCos : 0);
        _bobEulerRotation.z = (_walkInput != Vector2.zero ? _multiplier.z * CurveCos * _walkInput.x : 0);
    }

    private void CompositePositionRotation()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, _swayPos + _bobPosition, Time.deltaTime * _smooth);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(_swayEulerRot) * Quaternion.Euler(_bobEulerRotation), Time.deltaTime * _smoothRot);
    }
}