using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Die Klasse repraesentiert die Konfiguration der dynamischen NPCs. 
/// </summary>
public class NPCDynamicConfig : NetworkBehaviour {

	/// <summary>Name der Einheit. </summary>
	[SerializeField] private string name;

	/// <summary>Lebenspunkte. </summary>
	[SerializeField] private int life;

	/// <summary>Teamzugehoerigkeit. </summary>
	[SerializeField] private int team;

	/// <summary>Schadenspunkte. </summary>
	[SerializeField] private int damage;

	/// <summary>Angriffsintervall. </summary>
	[SerializeField] private int interval;

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
		whoIam = ("" + manager.myTeam).Equals (transform.tag);

		// Setze Symbol in die Miniaturkarte
		GetComponentInParent<DynamicMiniMapSymbol> ().Init(whoIam);

		// Erstelle ergaenzende Informationen
		GetComponentInParent<AdditionalGUIInfo> ().Init(name, whoIam, offsetLifePosition, life);

		// Setzte Objektname fuer interne Funktionen bezueglich GameObject
		transform.name += transform.GetComponent<NetworkIdentity> ().netId.Value;

		// ............................. Serverfunktion

		if (!isServer)
			return;

		// Aktiviere Navigation
		NavMeshAgent agent = GetComponent<NavMeshAgent> ();
		agent.enabled = true;

		// Hole und setze Hauptziele (Basis)
		if (transform.tag.Equals("0"))
			agent.destination = GameObject.Find ("base_1(Clone)").transform.position;

		else if (transform.tag.Equals("1"))
			agent.destination = GameObject.Find ("base_0(Clone)").transform.position;

		// Aktiviere Collider fuer den Kampf
		transform.GetChild (0).GetComponent<SphereCollider>().enabled = true;

		// Aktiviere Kampfkomponente
		NPCDynamicCombat combat = transform.GetChild (0).GetComponent<NPCDynamicCombat>();
		combat.enabled = true;
		combat.Init (team, damage, interval, name, "" + netId.Value);
	}
}
