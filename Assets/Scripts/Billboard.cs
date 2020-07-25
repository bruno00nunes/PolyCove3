using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _cam;

    public Transform Cam
    {
        get => _cam;
        set => _cam = value;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + _cam.forward);
    }
}
