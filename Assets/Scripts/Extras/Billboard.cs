using UnityEngine;

namespace Extras
{
    public class Billboard : MonoBehaviour
    {
        private Transform _cam;

        public Transform Cam
        {
            set => _cam = value;
        }

        private void LateUpdate()
        {
            transform.LookAt(transform.position + _cam.forward);
        }
    }
}
