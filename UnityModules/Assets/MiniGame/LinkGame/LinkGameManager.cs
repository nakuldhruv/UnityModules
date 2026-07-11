using UnityEngine;

namespace  UnityModules
{
    public class LinkGameManager : MonoBehaviour
    {
        [SerializeField] private LinkGameMap linkGameMap;
        
        private void Awake()
        {
            linkGameMap.CreateMap();
        }
    }   
}

