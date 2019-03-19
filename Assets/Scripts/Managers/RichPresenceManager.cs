using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RichPresenceManager : MonoBehaviour {
	
	public DiscordController dcController;
	public long startTimeStamp;

	void Start () {
		dcController = GetComponent<DiscordController>();

		startTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		dcController.presence.startTimestamp = startTimeStamp;
		dcController.presence.largeImageKey = "logo";
	}
	
	void Update () {
		if( SceneManager.GetActiveScene().buildIndex == 0 && dcController.presence.details != "In Main Menu" ) {
			dcController.presence.details = "In Main Menu";
		} else if( SceneManager.GetActiveScene().buildIndex != 0 && dcController.presence.details != "Commanding Facility" ) {
			dcController.presence.details = "Commanding Facility";
		}
	}
}
