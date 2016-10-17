using UnityEngine;
using System.Collections;

/// <summary>
/// Die Klasse repraesentiert die Konfiguration der statischen NPCs. 
/// </summary>
public class NPCStaticConfig : MonoBehaviour {

	/// <summary>Name der Einheit. </summary>
	[SerializeField] private string name;

	/// <summary>Lebenspunkte. </summary>
	[SerializeField] private int life;

	/// <summary>Teamzugehoerigkeit. </summary>
	[SerializeField] private int team;

	/// <summary>Position der ergaenzenden Informationen. </summary>
	[SerializeField] private float offsetLifePosition;

	/// <summary>Lokale Netzwerkmanager, dem die lokale Teamzugehoerigkeit entnommen wird. </summary>
	private Manager manager;

	/// <summary>Wahheitswert, ob die Einheit lokal ein Gegner ist. </summary>
	private bool whoIam;

	/// <summary>
	/// Initialisiert die Komponente.
	/// </summary>
	void Start () {

		// Hole den lokalen Netzwerkmanager
		if (!(manager = GameObject.Find ("NetworkManager").GetComponent<Manager> ())) {
			Debug.Log ("#ERR: NPCStaticConfig->Manager nicht gefunden.");

			return;
		}

		// true = teammate | false enemy
		whoIam = (manager.myTeam == team);

		// Setze Symbol in die Miniaturkarte
		GetComponentInParent<StaticMiniMapSymbol> ().Init(whoIam);

		// Erstelle ergaenzende Informationen
		GetComponentInParent<AdditionalGUIInfo> ().Init(name, whoIam, offsetLifePosition, life);
	}

}
