using UnityEngine;

public class GlobalGradientFlowController : MonoBehaviour
{
    public GameObject startObject;  // The first object in the gradient flow
    public GameObject endObject;    // The last object in the gradient flow
    public Material flowMaterial;
    public float flowSpeed = 1.0f;
    public Color colorA = new Color(0.149f, 0.141f, 0.912f, 1.0f);
    public Color colorB = new Color(1.0f, 0.833f, 0.224f, 1.0f);

    private void Update()
    {
        if (startObject == null || endObject == null)
        {
            Debug.LogError("You need to assign start and end objects!");
            return;
        }

        flowMaterial.SetVector("_StartPosition", startObject.transform.position);
        flowMaterial.SetVector("_EndPosition", endObject.transform.position);

        flowMaterial.SetFloat("_FlowSpeed", flowSpeed);
        flowMaterial.SetColor("_ColorA", colorA);
        flowMaterial.SetColor("_ColorB", colorB);
    }
}