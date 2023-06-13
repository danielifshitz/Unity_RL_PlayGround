using UnityEngine;

namespace Common.Player
{
    public class RaySensor : MonoBehaviour
    {
        private Quaternion _startingRotation;
        private Vector3 _offset;
        
        public GameObject player;

        private void Awake()
        {
            var rayTransform = transform;
            var rotation = rayTransform.rotation;
            
            _startingRotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

            _offset = rayTransform.position - player.transform.position;
        }
        
        private void LateUpdate()
        {
            transform.SetPositionAndRotation(player.transform.position + _offset, _startingRotation);
        }
    }
}

