using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StreamVideo : MonoBehaviour {

	public RawImage rawImage;
	public VideoPlayer videoPlayer;
	public VideoSource VideoSource;
	//public VideoClip videoToPlay;
	public AudioSource audioSource;

	public void PlayPause(){
		if (!videoPlayer.isPlaying) {
			videoPlayer.Play ();
			audioSource.Play ();
			rawImage.enabled = false;
			print ("play");
		} else {
			videoPlayer.Pause ();
			audioSource.Pause ();
			rawImage.enabled = true;
			print ("pause");
		}
	}
}
