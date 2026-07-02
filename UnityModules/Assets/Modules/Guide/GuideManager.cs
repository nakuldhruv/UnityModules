using System;
using UnityEngine;
using UnityModules.Event.Zxs;

namespace UnityModules.Guide
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