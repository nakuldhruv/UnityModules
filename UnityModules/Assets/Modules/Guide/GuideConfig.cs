using System;
using System.Collections.Generic;

namespace UnityModules.Zxs.Guide
{
    [Serializable]
    public class GuideConfig
    {
        public List<GuideItem> items;
    }

    [Serializable]
    public class GuideItem
    {
        public string key;
        public List<GuideStep> steps;

        private int _stepIndex;

        public void GetCurrentStep()
        {
        }
        
        private void RefreshStep()
        {
        }
    }

    [Serializable]
    public class GuideStep
    {
        public string key;
        public GuideType type;
    }

    public enum GuideType
    {
        None,
        Dialog,
        Finger
    }
}