using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class LocalizationManager : MonoBehaviour {

	public string yeet = "asdflad";
	public string[] enPhrases = {  };

	void Start() {
		GetPhrase( "yeet" );
	}

	public string GetPhrase( string phrase ) {
		this.GetType().GetField( phrase ).GetValue( this );
		Debug.Log( this.GetType().GetField( phrase ).GetValue( this ) );
		return "";
	}
}
