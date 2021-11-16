using UnityEngine;
using System.Collections;

public class triggerSound : MonoBehaviour {
	public AudioSource source;
	public bool playing;
	public AudioClip[] clip;
	// Update is called once per frame
	void OnTriggerStay(Collider col){
		if(!source.isPlaying&&playing==false){
			source.clip = clip[Random.Range(0,clip.Length)];
			source.pitch = Random.Range(0.9f,1.25f);
			source.Play();
			playing = true;
		}
	}
	void OnTriggerExit(Collider col){
		playing = false;
	}
}
