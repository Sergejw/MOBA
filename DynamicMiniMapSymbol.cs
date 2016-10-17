using UnityEngine;
using System.Collections;

/// <summary>
/// Die Klasse repraesentiert ein bewegliches Symbol innerhalb der Miniaturkarte.
/// </summary>
public class DynamicMiniMapSymbol : MonoBehaviour {

	/// <summary>Das Symbol.</summary>
	private GameObject symbol;

	/// <summary>
	/// Initialisiert das Symbol.
	/// </summary>
	/// <param name="whoIam">Wahrheitswert, ob Gegner oder nicht. (false = Gegner)</param>
	public void Init(bool whoIam) {

		// Setze passende Farbe nach Wahrheitswert
		string symbolName = (whoIam) ? "teammate" : "enemy";

		symbol = Instantiate (Resources.Load ("components/minimap_symbol_" + symbolName, typeof(GameObject))) as GameObject;
		symbol.transform.SetParent (GameObject.Find ("minimap").transform);
	}

	/// <summary>
	/// Erstellt ein Symbol für lokalen Spieler.
	/// </summary>
	public void CreateLokalPlayerSymbol() {
		symbol = Instantiate (Resources.Load ("components/minimap_symbol_me", typeof(GameObject))) as GameObject;
		symbol.transform.SetParent (GameObject.Find ("minimap").transform);
	}

	/// <summary>
	/// Bewegt das Symbol innerhalb der Miniaturkarte. 
	/// </summary>
	void Update() {
		if (symbol != null)
			symbol.transform.position = new Vector3(Screen.width - 200 + transform.position.x * 5, Screen.height - 200 + transform.position.z * 5, 0);

		// ````````````````````
		// TODO
		// OPTIMIERUNG
		// ....................
	}

	/// <summary>
	/// Zerstoert das Symbol.
	/// </summary>
	void OnDestroy() {
		if (symbol)
			Destroy (symbol.gameObject); 
	}
}