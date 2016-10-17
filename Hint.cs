using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Die Klasse repraesentiert Hinweise, die Gegenstandbeschreibungen aus dem Shop darstellen.
/// </summary>
public class Hint : MonoBehaviour {

	/// <summary>Beschreibung als Objekt.</summary>
	private Text text;

	/// <summary>
	/// Initiiert die Komponente.
	/// </summary>
	void Start() {
		text = GetComponentInParent<Text> ();
	}

	/// <summary>
	/// Zeigt die Beschreibung zu einem Gegenstand an.
	/// </summary>
	/// <param name="info">Gegenstandbeschreibung.</param>
	public void Show(string info) {
		text.text = info;
	}

	/// <summary>
	/// Entfernt bzw. leert die Beschreibung.
	/// </summary>
	public void Clear() {
		text.text = "";
	}
}