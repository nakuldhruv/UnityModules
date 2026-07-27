using UnityEngine;

namespace Nakul.DesignPattern
{
    public class Player : MonoBehaviour
    {
        public void Move(Vector3 offset)
        {
            transform.position += offset;
        }
    }
}