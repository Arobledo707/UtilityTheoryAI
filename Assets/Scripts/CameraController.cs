using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float m_movementSpeed = 10.0f;

    [SerializeField]
    private float m_zoomSpeed = 100.0f;

	void Update()
    {
        UpdateMovement();
	}

    public void UpdateMovement()
    {
        float xMovPos;
        float yMovPos;
        float zoomAmount;

        xMovPos = Input.GetAxis("Horizontal") * m_movementSpeed * Time.deltaTime;
        yMovPos = Input.GetAxis("Vertical") * m_movementSpeed * Time.deltaTime;
        zoomAmount = Input.GetAxis("Mouse ScrollWheel") * m_zoomSpeed * Time.deltaTime;

        gameObject.GetComponent<Camera>().orthographicSize += zoomAmount;

        transform.Translate(xMovPos, yMovPos, 0);
    }
}
