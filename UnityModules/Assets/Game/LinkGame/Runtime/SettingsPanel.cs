using UnityEngine;
using UnityEngine.UI;

namespace Nakul.LinkGame
{
    /// <summary>
    /// 设置面板：控制音效音量大小，以及面板的打开/关闭。
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("面板")]
        [SerializeField] private GameObject _panel;

        [Header("音量")]
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private AudioSource _audioSource;

        [Header("按钮")]
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;

        private const string VolumePrefsKey = "LinkGame_Volume";

        private void Awake()
        {
            // 读取上次保存的音量
            float savedVolume = PlayerPrefs.GetFloat(VolumePrefsKey, 1f);
            ApplyVolume(savedVolume);

            if (_volumeSlider != null)
            {
                _volumeSlider.value = savedVolume;
                _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }

            if (_openButton != null)
            {
                _openButton.onClick.AddListener(Open);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Close);
            }

            // 默认关闭面板
            Close();
        }

        private void OnVolumeChanged(float value)
        {
            ApplyVolume(value);
            PlayerPrefs.SetFloat(VolumePrefsKey, value);
            PlayerPrefs.Save();
        }

        private void ApplyVolume(float value)
        {
            if (_audioSource != null)
            {
                _audioSource.volume = Mathf.Clamp01(value);
            }
        }

        public void Open()
        {
            if (_panel != null)
            {
                _panel.SetActive(true);
            }
        }

        public void Close()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        public void Toggle()
        {
            if (_panel != null)
            {
                _panel.SetActive(!_panel.activeSelf);
            }
        }
    }
}
