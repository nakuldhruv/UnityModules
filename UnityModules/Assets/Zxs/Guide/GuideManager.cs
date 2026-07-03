using UnityEngine;
using Zxs.Event;

namespace Zxs.Guide
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