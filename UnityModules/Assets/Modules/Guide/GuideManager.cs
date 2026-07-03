using UnityEngine;
using UnityModules.Zxs.Event;

namespace UnityModules.Zxs.Guide
{
    public class GuideManager : MonoBehaviour
    {
        private void Awake()
        {
            GameSignalBus.Instance.Subscribe<StartGuide>(StartGuide);
        }

        private void StartGuide(StartGuide args)
        {
        }
    }
}