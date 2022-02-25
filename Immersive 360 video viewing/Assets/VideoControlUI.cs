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
    private Text _sliderText;
    private int _activeClip;

    private void Start() {
        _activeClip = 0;
        videoPlayer.clip = clips[_activeClip];
        videoPlayer.Play();

        var volumeSlider = DebugUIBuilder.instance.AddSlider("Volume", 0.0f, 100.0f, VolumeChange, false);
        var textElementsInSlider = volumeSlider.GetComponentsInChildren<Text>();
        Assert.AreEqual(textElementsInSlider.Length, 2, "Slider prefab format requires 2 text components (label + value)");
        textElementsInSlider[0].text = "Volume:";
        _sliderText = textElementsInSlider[1];
        Assert.IsNotNull(_sliderText, "No text component on slider prefab");
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

    public void TogglePressed(Toggle t) {
        Debug.Log("Toggle pressed. Is on? " + t.isOn);
    }
    public void RadioPressed(string radioLabel, string group, Toggle t) {
        Debug.Log("Radio value changed: " + radioLabel + ", from group " + group + ". New value: " + t.isOn);
    }

    public void VolumeChange(float f) {
        int volInt = (int) f;
        _sliderText.text = volInt.ToString() + "%";
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

    private void LogButtonPressed() {
        Debug.Log("Button pressed");
    }
}
