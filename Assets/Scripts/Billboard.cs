using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam;

    private void Update()
    {
        if (!cam)
        {
            cam = FindObjectOfType<Camera>();
            return;
        }

        transform.LookAt(cam.transform);
        transform.Rotate(Vector3.up * 180);
    }
}
