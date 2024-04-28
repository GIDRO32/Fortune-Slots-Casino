using UnityEngine;

public class OrbitalGame : MonoBehaviour
{
    public Transform centerObject; // The object around which the arrow will orbit
    public Transform arrowObject; // The arrow object
    public float orbitSpeed = 1f; // Speed of the orbit

    private bool isMousePressed = false;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isMousePressed = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isMousePressed = false;
        }

        if (isMousePressed)
        {
            // Get the mouse position in world space
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Ensure the z-coordinate is 0 to keep it on the same plane as the objects

            // Calculate the direction from the centerObject to the mouse position
            Vector3 directionToMouse = mousePosition - centerObject.position;

            // Calculate the desired position for the arrow to point towards the mouse position
            float angleToMouse = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
            arrowObject.rotation = Quaternion.Euler(0, 0, angleToMouse);

            // Calculate the desired position for the arrow to maintain its distance from the center while following the mouse
            float distanceToMouse = directionToMouse.magnitude;
            Vector3 orbitPosition = centerObject.position + directionToMouse.normalized * (distanceToMouse + arrowObject.localScale.x / 2);

            // Set the position of the arrow to the calculated orbit position
            arrowObject.position = orbitPosition;
        }
        else
        {
            // Calculate the desired position for the arrow to orbit around the centerObject
            Vector3 orbitPosition = centerObject.position + Quaternion.Euler(0, 0, Time.time * orbitSpeed) * Vector3.right;

            // Set the position of the arrow to the calculated orbit position
            arrowObject.position = orbitPosition;
        }
    }
}
