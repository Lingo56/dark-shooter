using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    public GameObject[] strings; // Array to hold string GameObjects
    public GameObject mainObject; // Reference to the main object
    public float dragCoefficient = 0.5f; // Drag coefficient, adjust as needed
    public float stringLength = 1f; // Length of the strings
    public float stringTension = 1f; // Tension of the strings

    void FixedUpdate()
    {
        // Calculate velocity of the main object
        Vector3 velocity = (mainObject.transform.position - transform.position) / Time.fixedDeltaTime;

        // Calculate drag force
        Vector3 dragForce = -dragCoefficient * velocity;

        // Update string positions
        foreach (GameObject str in strings)
        {
            // Calculate desired position based on tension and object's movement
            Vector3 desiredPos = transform.position + (str.transform.position - transform.position).normalized * stringLength;

            // Apply drag force to the string
            Vector3 stringForce = dragForce + stringTension * (desiredPos - str.transform.position);
            str.transform.position += stringForce * Time.fixedDeltaTime;
        }
    }
}
