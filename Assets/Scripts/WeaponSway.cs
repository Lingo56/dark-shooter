using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Want sway to do translation as well as rotation. Currently only does rotation.
// This video shows translation.
// https://youtu.be/DR4fTllQnXg?si=IzaeJ4yVMUwhAGsB

public class WeaponSway : MonoBehaviour
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
    private float _curveSin { get => Mathf.Sin(_speedCurve); }
    private float _curveCos { get => Mathf.Cos(_speedCurve); }

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
        getInput();

        sway();
        swayRotation();
        BobOffset();
        BobRotation();

        CompositePositionRotation();
    }

    void getInput() {
        // Player direction
        _walkInput.x = Input.GetAxis("Horizontal");
        _walkInput.y = Input.GetAxis("Vertical");
        _walkInput = _walkInput.normalized;

        // Mouse input
        _lookInput.x = Input.GetAxis("Mouse X");
        _lookInput.y = Input.GetAxis("Mouse Y");
    }

    void sway()
    {
        Vector3 invertLook = _lookInput * -_swayStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -_maxSwayStepDistance, _maxSwayStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -_maxSwayStepDistance, _maxSwayStepDistance);

        _swayPos = invertLook;
    }

    private void swayRotation() {
        Vector2 invertLook = _lookInput * -_rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -_maxRotationStep, _maxRotationStep);
        invertLook.y = Mathf.Clamp(invertLook.y, -_maxRotationStep, _maxRotationStep);
        _swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
    }

    void BobOffset()
    {
        _speedCurve += Time.deltaTime * ((Input.GetAxis("Horizontal") + Input.GetAxis("Vertical")) * _bobExaggeration) + 0.01f;

        _bobPosition.x = (_curveCos * _bobLimit.x) - (_walkInput.x * _travelLimit.x);
        _bobPosition.y = (_curveSin * _bobLimit.y) - (Input.GetAxis("Vertical") * _travelLimit.y);
        _bobPosition.z = -(_walkInput.y * _travelLimit.z);
    }

    void BobRotation()
    {
        _bobEulerRotation.x = (_walkInput != Vector2.zero ? _multiplier.x * (Mathf.Sin(2 * _speedCurve)) : _multiplier.x * (Mathf.Sin(2 * _speedCurve) / 2));
        _bobEulerRotation.y = (_walkInput != Vector2.zero ? _multiplier.y * _curveCos : 0);
        _bobEulerRotation.z = (_walkInput != Vector2.zero ? _multiplier.z * _curveCos * _walkInput.x : 0);
    }

    void CompositePositionRotation()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, _swayPos + _bobPosition, Time.deltaTime * _smooth);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(_swayEulerRot) * Quaternion.Euler(_bobEulerRotation), Time.deltaTime * _smoothRot);
    }
}
