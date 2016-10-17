using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Die Klasse repraesentiert die Spielzeit, die in der zweiten Szene als gespielte Zeit 
/// angezeit wird.
/// </summary>
public class Matchtime : MonoBehaviour {

    /// <summary> Aktuelle Zeitausgabe. </summary>
	private Text txtTime;

	/// <summary>
    /// Initialisiert die Komponente und startet die Intervalle, die die Zeitausgabe 
    /// aktuellisieren.
    /// </summary>
	void Start () {
		txtTime = GameObject.Find ("txt_time").GetComponent<Text> ();
		InvokeRepeating ("RunTime", 0, 1f);
	}
	
    /// <summary>
    /// Schreibt in Intervallen die aktuell gespielte Zeit in die Zeitausgabe.
    /// </summary>
	void RunTime() {
		string minutes = Mathf.Floor(Time.timeSinceLevelLoad / 60).ToString("00");
		string seconds = Mathf.Floor(Time.timeSinceLevelLoad % 60).ToString("00");

		txtTime.text = "Gespielte Zeit: " + minutes + ":" + seconds;
	}
}
