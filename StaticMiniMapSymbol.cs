using UnityEngine;
using System.Collections;

/// <summary>
/// Die Klasse repraesentiert ein statisches Symbol der Miniatukarte.
/// </summary>
public class StaticMiniMapSymbol : MonoBehaviour  {

	/// <summary> Symbol als Objekt.</summary>
	private GameObject symbol;

	/// <summary>
	/// Initiiert das Symbol. Abhaengig vom Parameter wird entweder ein Symbol fuer Gegner oder Mitspieler 
	/// aus eigenem Team erstellt. Anschliessend wird dieses passend zur Charakterposition in die 
	/// Miniaturkarte positioniert.
	/// </summary>
	/// <param name="whoIam">Wahrheitswert, ob Gegner oder nicht (false = Gegner).</param>
	public void Init(bool whoIam) {

		string symbolName = (whoIam) ? "teammate" : "enemy";

		symbol = Instantiate (Resources.Load ("components/minimap_symbol_" + symbolName, typeof(GameObject))) as GameObject;
		symbol.transform.SetParent (GameObject.Find ("minimap").transform);
		symbol.transform.position = new Vector3(Screen.width - 200 + transform.position.x * 5, Screen.height - 200 + transform.position.z * 5, 0);
	}
		
	/// <summary>
	/// Zerstoert das Symbol.
	/// </summary>
	void OnDestroy() {
		Destroy (symbol);
	}
}
