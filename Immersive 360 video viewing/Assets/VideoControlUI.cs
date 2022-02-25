using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoControlUI : MonoBehaviour {

    public VideoPlayer videoPlayer;
    public VideoClip[] clips;

    private bool _inMenu;
    private Text[] _progressTextElems;
    private Text _playPauseText;
    private Text _volSliderText;
    private int _activeClip;

    private void Start() {
        _activeClip = 0;
        videoPlayer.clip = clips[_activeClip];
        videoPlayer.Play();
        videoPlayer.sendFrameReadyEvents = true;
        videoPlayer.frameReady += UpdateProgress;

        var progressSlider = DebugUIBuilder.instance.AddSlider("Progress", 0.0f, 1.0f, Scrub, false);
        _progressTextElems = progressSlider.GetComponentsInChildren<Text>();
        Assert.AreEqual(_progressTextElems.Length, 2, "Slider prefab format requires 2 text components (label + value)");
        UpdateProgress(videoPlayer, 0);

        var playPauseToggle = DebugUIBuilder.instance.AddButton("PlayPause", PlayPause);
        _playPauseText = playPauseToggle.GetComponentInChildren<Text>();
        _playPauseText.text = "Pause";

        DebugUIBuilder.instance.AddDivider();

        var volumeSlider = DebugUIBuilder.instance.AddSlider("Volume", 0.0f, 100.0f, VolumeChange, false);
        var volumeTextElems = volumeSlider.GetComponentsInChildren<Text>();
        Assert.AreEqual(volumeTextElems.Length, 2, "Slider prefab format requires 2 text components (label + value)");
        volumeTextElems[0].text = "Volume:";
        _volSliderText = volumeTextElems[1];
        Assert.IsNotNull(_volSliderText, "No text component on slider prefab");
        volumeSlider.GetComponentInChildren<Slider>().value = 100.0f;
        VolumeChange(volumeSlider.GetComponentInChildren<Slider>().value);


        // DebugUIBuilder.instance.AddButton("Button Pressed", LogButtonPressed);
        // DebugUIBuilder.instance.AddLabel("Label");
        // var sliderPrefab = DebugUIBuilder.instance.AddSlider("Slider", 1.0f, 10.0f, SliderPressed, true);
        // var textElementsInSlider = sliderPrefab.GetComponentsInChildren<Text>();
        // Assert.AreEqual(textElementsInSlider.Length, 2, "Slider prefab format requires 2 text components (label + value)");
        // _sliderText = textElementsInSlider[1];
        // Assert.IsNotNull(_sliderText, "No text component on slider prefab");
        // _sliderText.text = sliderPrefab.GetComponentInChildren<Slider>().value.ToString();
        // DebugUIBuilder.instance.AddDivider();
        // DebugUIBuilder.instance.AddToggle("Toggle", TogglePressed);
        // DebugUIBuilder.instance.AddRadio("Radio1", "group", delegate (Toggle t) { RadioPressed("Radio1", "group", t); });
        // DebugUIBuilder.instance.AddRadio("Radio2", "group", delegate (Toggle t) { RadioPressed("Radio2", "group", t); });
        // DebugUIBuilder.instance.AddLabel("Secondary Tab", 1);
        // DebugUIBuilder.instance.AddDivider(1);
        // DebugUIBuilder.instance.AddRadio("Side Radio 1", "group2", delegate (Toggle t) { RadioPressed("Side Radio 1", "group2", t); }, DebugUIBuilder.DEBUG_PANE_RIGHT);
        // DebugUIBuilder.instance.AddRadio("Side Radio 2", "group2", delegate (Toggle t) { RadioPressed("Side Radio 2", "group2", t); }, DebugUIBuilder.DEBUG_PANE_RIGHT);

        DebugUIBuilder.instance.Show();
        _inMenu = true;
    }

    private string SecToString(int sec) {
        int m = sec / 60;
        int s = sec % 60;
        return string.Format("{0}:{1:00}", m, s);
    }

    public void Scrub(float f) {
        long newFrame = (long)(f * videoPlayer.frameCount);
        videoPlayer.frame = newFrame;
        UpdateProgress(videoPlayer, newFrame);
    }

    public void UpdateProgress(VideoPlayer source, long frameIdx) {
        int progress = (int)(frameIdx / source.frameRate);
        int length = (int)source.length;
        _progressTextElems[0].text = SecToString(progress);
        _progressTextElems[1].text = SecToString(length);
    }

    public void PlayPause() {
        if (videoPlayer.isPaused) {
            _playPauseText.text = "Pause";
            videoPlayer.Play();
        } else {
            _playPauseText.text = "Play";
            videoPlayer.Pause();
        }
    }

    public void VolumeChange(float f) {
        int volInt = (int)f;
        _volSliderText.text = volInt.ToString() + "%";
        for (ushort i = 0; i < videoPlayer.audioTrackCount; i++)
            videoPlayer.SetDirectAudioVolume(i, f / 100.0f);
    }

    private void Update() {
        if (OVRInput.GetDown(OVRInput.Button.Two) || OVRInput.GetDown(OVRInput.Button.Start)) {
            if (_inMenu)
                DebugUIBuilder.instance.Hide();
            else DebugUIBuilder.instance.Show();
            _inMenu = !_inMenu;
        }
    }

}
