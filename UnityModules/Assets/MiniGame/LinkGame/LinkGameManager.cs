using UnityEngine;

namespace Nakul.LinkGame
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

