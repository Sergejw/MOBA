using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Die Klasse repraesentiert die Kampfkomponente der beweglichen NPCs. Die NPCs holen sich die 
/// moeglichen Ziele in die getrennten Ziellisten. Es gibt je eine Zielliste für NPCs und 
/// Spieler. Die Trennung ist noetig, weil die NPCs immer als erstes angegriffen werden sollen. Falls keine 
/// NPCs vorhanden, dann werden die Spieler angegriffen.
/// </summary>
public class NPCDynamicCombat : MonoBehaviour {

	/// <summary> Zielliste fuer NPCs. </summary>
	private List<NPCDamageReciever> npcTargets = new List<NPCDamageReciever> ();

	/// <summary> Zielliste fuer Spieler. </summary>
	private List<PlayerDamageReciever> playerTargets = new List<PlayerDamageReciever> ();

	/// <summary>Temporaerer Empfaenger der Schadenspunkte der NPCs. </summary>
	private NPCDamageReciever npcStats;

	/// <summary>Temporaerer Empfaenger der Schadenspunkte der Spieler. </summary>
	private PlayerDamageReciever playerStats;

	/// <summary>Gegnerische Teamkennung. </summary>
	private int team;

	/// <summary>Schadenspunkte der Komponente. </summary>
	private int damage;

	/// <summary>Angriffsintervall der Komponente. </summary>
	private int interval;

	/// <summary>Name der Komponente. </summary>
	private string name;

	/// <summary>NavMeshAgent, der die NPCs navigiert. </summary>
	private NavMeshAgent agent;

	/// <summary>Hauptziel (Basis). </summary>
	private Vector3 mainTarget;

	/// <summary>Temporaeres Zeil (NPC oder Spieler). </summary>
	private Transform secondTarget;

	/// <summary>Angriffsradius. </summary>
	private float range = 2f;

	/// <summary>Id Komponente. </summary>
	private string attacker;

	/// <summary>Abklingzeit je Angriff bzw. Schlag </summary>
	private float ttt;

	/// <summary>
	/// Initialisiert lokale Elemente der Komponente.
	/// </summary>
	void Start() {		
		agent = GetComponentInParent<NavMeshAgent> ();
		agent.stoppingDistance = range;
		mainTarget = agent.destination;

		ttt = Time.fixedTime;
	}

	/// <summary>
	/// Initialisiert alle Instanzen der Komponente.
	/// </summary>
	/// <param name="team">Teamzugehoerigkeit der Gegner.</param>
	/// <param name="damage">Schaden, den die Komponente als NPC austeilt.</param>
	/// <param name="interval">Angriffsintervall.</param>
	/// <param name="name">Name.</param>
	/// <param name="attacker">Id.</param>
	public void Init(int team, int damage, int interval, string name, string attacker) {
		this.attacker = attacker;
		this.team = team;
		this.damage = damage;
		this.interval = interval;
		this.name = name;
	}

	// ......................................................................... Ein Ziel in Reichweite?

	/// <summary>
	/// Prueft, ob das neue Objekt im Angriffsradius angreifbar ist. Falls ja,
	/// dann wird es in die Zielliste eingetragen und ein Angriff wird gestartet.
	/// </summary>
	/// <param name="target">Collider eines moeglichen Ziels.</param>
	/// 
	void OnTriggerEnter(Collider target) {
		if (target.tag.Equals ("" + team) || (!target.tag.Equals ("0") && !target.tag.Equals ("1")))
			return;

		if (npcStats = target.GetComponent<NPCDamageReciever> ()) 
			npcTargets.Add (npcStats);

		else if (playerStats = target.GetComponent<PlayerDamageReciever> ()) 
			playerTargets.Add (playerStats);
		
		if (!IsInvoking ("Attack") && !IsInvoking ("Follow")) {
			float tempTime = Time.fixedTime - ttt;
			InvokeRepeating ("Attack", (tempTime > 2) ? 0 : 2 - tempTime, interval);
		}
	}
		
	/// <summary>
	/// Verfolgt ein Ziel, wenn dieses in Sichtweite und ausserhalb des Angriffsradius.
	/// </summary>
	void Follow() {
		if (secondTarget && Vector3.Distance (secondTarget.transform.position, transform.position) > 3) {

			if (agent.enabled) {
				agent.SetDestination (secondTarget.position);

			} else {
				secondTarget = null;
			}

		} else {
			if (npcTargets.Count < 1 && playerTargets.Count < 1) {
				agent.SetDestination (mainTarget);

			} else {
				InvokeRepeating ("Attack", 0, interval);
			}
				
			CancelInvoke ("Follow");
		}

	}

	// ......................................................................................... Angriff


	/// <summary>
	/// Geht in Intervallen die Ziellisten durch und attackiert pro Durchlauf ein
	/// Ziel. Ausserdem wird darauf geachtet, dass zuerst die NPCs angegriffen werden.
	/// </summary>
	/// 
	void Attack() {
		ttt = Time.fixedTime;
		// Greife NPC an 
		// temp -> wegen doppelter Zugriff auf List mit Remove bei null
		// während eines Angriffs kann das Ziel jemand anderes toeten
		NPCDamageReciever[] tempNpcTargets = new NPCDamageReciever[npcTargets.Count];
		npcTargets.CopyTo (tempNpcTargets);

		// Gehe alle Ziele durch bis ein Treffer
		foreach (NPCDamageReciever t in tempNpcTargets)
			if (t) {
				if (Vector3.Distance (t.transform.position, transform.position) <= range * 2) {
					t.TakeDamage (damage, attacker, t.netId.Value);
				
				} else {
					secondTarget = t.transform;
					InvokeRepeating ("Follow", 0, Time.deltaTime);
					CancelInvoke ("Attack");
					Reset ();
				}

				return;

			} else 
				npcTargets.Remove (t);
		

		// kein NPC vorhanden bzw. getroffen weil null wegen gestorben
		// Greife Spieler an
		PlayerDamageReciever[] tempPlayerTargets = new PlayerDamageReciever[playerTargets.Count];
		playerTargets.CopyTo (tempPlayerTargets);

		foreach (PlayerDamageReciever t in tempPlayerTargets)
			if (t) {
				if (Vector3.Distance (t.transform.position, transform.position) <= range * 2) {
					t.TakeDamage (damage, attacker, t.netId.Value);

				} else {
					secondTarget = t.transform;
					InvokeRepeating ("Follow", 0, Time.deltaTime);
					CancelInvoke ("Attack");
					Reset ();
				}

				return;

			} else 
				playerTargets.Remove (t);

		// Ersatz für OnTriggerExit, wenn innerhalb des Radius alle
		// Listen leer werden.
		Reset ();

	}


	// ................................................................................. Angriff abbrechen?


	/// <summary>
	/// Passt die Ziellisten an und kontrolliert, ob noch Ziele vorhanden sind. 
	/// Wenn keine Ziele mehr vorhanden, dann wird der laufende Agriff beendet.
	/// </summary>
	/// <param name="target">Collider des Ziels, das den Angriffsradius 
	/// verlassen hat.</param>
	/// 
	void OnTriggerExit(Collider target) {
		if (!npcTargets.Remove(target.GetComponent<NPCDamageReciever>()))
			playerTargets.Remove(target.GetComponent<PlayerDamageReciever>());

		Reset ();
	}

	/// <summary>
	/// Aktiviert das Hauptziel.
	/// </summary>
	private void Reset() {
		
		if (IsInvoking("Attack") && npcTargets.Count < 1 && playerTargets.Count < 1) {
			CancelInvoke ("Attack");
			agent.SetDestination (mainTarget);
		}
	}

	/// <summary>
	/// Zerstoert das Objekt.
	/// </summary>
	void OnDestroy() {
		CancelInvoke ("Attack");
		CancelInvoke ("Follow");
	}

}
