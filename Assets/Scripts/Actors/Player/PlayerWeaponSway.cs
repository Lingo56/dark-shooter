using UnityEngine;
using UnityEngine.Serialization;

namespace Actors.Player
{
    public class PlayerWeaponSway : MonoBehaviour
    {
        [SerializeField] private float smooth = 10f;
        
        [Header("Mouse Movement Sway Settings")]
        [SerializeField] private float positionStep = 0.01f;
        [SerializeField] private float maxPositionStepDistance = 0.06f;
        private Vector3 _swayPos;
        
        [SerializeField] private float rotationStep = 4f;
        [SerializeField] private float maxRotationStep = 5f;
        private Vector3 _swayEulerRot;
        private float _smoothRot = 12f;
        
        [Header("Character Movement Travel Settings")]
        [SerializeField] private Vector3 travelLimit = Vector3.one * 0.025f;
        
        [Header("Idle Bob Settings")]
        [SerializeField] private Vector3 bobLimit = Vector3.one * 0.01f;
        [SerializeField] private Vector3 bobRotationMultiplier;
        [SerializeField] private float bobExaggeration;

        private float _speedCurve;
        private float CurveSin { get => Mathf.Sin(_speedCurve); }
        private float CurveCos { get => Mathf.Cos(_speedCurve); }

        private Vector3 _bobPosition;
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
            Vector3 invertLook = _lookInput * -positionStep;
            invertLook.x = Mathf.Clamp(invertLook.x, -maxPositionStepDistance, maxPositionStepDistance);
            invertLook.y = Mathf.Clamp(invertLook.y, -maxPositionStepDistance, maxPositionStepDistance);

            _swayPos = invertLook;
        }

        private void SwayRotation()
        {
            Vector2 invertLook = _lookInput * -rotationStep;
            invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep, maxRotationStep);
            invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);
            _swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
        }

        private void BobOffset()
        {
            _speedCurve += Time.deltaTime * ((Input.GetAxis("Horizontal") + Input.GetAxis("Vertical")) * bobExaggeration) + 0.01f;

            _bobPosition.x = (CurveCos * bobLimit.x) - (_walkInput.x * travelLimit.x);
            _bobPosition.y = (CurveSin * bobLimit.y) - (Input.GetAxis("Vertical") * travelLimit.y);
            _bobPosition.z = -(_walkInput.y * travelLimit.z);
        }

        private void BobRotation()
        {
            _bobEulerRotation.x = (_walkInput != Vector2.zero ? bobRotationMultiplier.x * (Mathf.Sin(2 * _speedCurve)) : bobRotationMultiplier.x * (Mathf.Sin(2 * _speedCurve) / 2));
            _bobEulerRotation.y = (_walkInput != Vector2.zero ? bobRotationMultiplier.y * CurveCos : 0);
            _bobEulerRotation.z = (_walkInput != Vector2.zero ? bobRotationMultiplier.z * CurveCos * _walkInput.x : 0);
        }

        private void CompositePositionRotation()
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _swayPos + _bobPosition, Time.deltaTime * smooth);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(_swayEulerRot) * Quaternion.Euler(_bobEulerRotation), Time.deltaTime * _smoothRot);
        }
    }
}