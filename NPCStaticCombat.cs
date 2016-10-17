using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Die Klasse repraesentiert die Kampfkomponente der Verteidigungstuerme. Die Tuerme holen sich die moeglichen Ziele 
/// in die getrennten Ziellisten. Es gibt je eine Zielliste für NPCs und 
/// Spieler. Die Trennung ist noetig, weil die NPCs immer als erstes angegriffen werden sollen. Falls keine 
/// NPCs vorhanden, dann werden die Spieler angegriffen.
/// </summary>
public class NPCStaticCombat : MonoBehaviour {

	/// <summary> Zielliste fuer NPCs. </summary>
	private List<NPCDamageReciever> npcTargets;

	/// <summary> Zielliste fuer Spieler. </summary>
	private List<PlayerDamageReciever> pdrTargets;

	/// <summary>Temporaerer Empfaenger der Schadenspunkte der NPCs. </summary>
	private NPCDamageReciever npcStats;

	/// <summary>Temporaerer Empfaenger der Schadenspunkte der Spieler. </summary>
	private PlayerDamageReciever pdr;

	/// <summary>Gegnerische Teamkennung. </summary>
	[SerializeField] private int team;

	/// <summary>Schadenspunkte der Komponente. </summary>
	[SerializeField] private int  damage;

	/// <summary>Angriffsintervall der Komponente. </summary>
	[SerializeField] private int  interval;

	/// <summary>Name der Komponente. </summary>
	[SerializeField] private string name;

	/// <summary>Geschoss der Tuerme. </summary>
	private Transform bullet;

	/// <summary>Ausgangsposition der Geschosse. </summary>
	private Vector3 bulletStartPos;

	/// <summary>Aktuelles Ziel. </summary>
	private Transform target;

	/// <summary>
	/// Initialisiert die Komponente.
	/// </summary>
	public void Start() {	
			
		bullet = transform.GetChild (0).transform;
		bulletStartPos = bullet.transform.position;

		npcTargets = new List<NPCDamageReciever> ();
		pdrTargets = new List<PlayerDamageReciever> ();
	}
		
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
		
		else if (pdr = target.GetComponent<PlayerDamageReciever> ())
			pdrTargets.Add (pdr);

		if (!IsInvoking ("Attack"))
			InvokeRepeating ("Attack", 0, interval);
	}


	/// <summary>
	/// Bewegt das Geschoss zum Ziel.
	/// </summary>
	void Update() {
		if (target) {
			bullet.transform.position = Vector3.Lerp (bullet.transform.position, target.position, 7f * Time.deltaTime);

			if (Vector3.Distance (bullet.transform.position, target.position) < 1.5f || !IsInvoking ("Attack")) {
				bullet.transform.position = bulletStartPos;
				target = null;
			}
		}
	}


	/// <summary>
	/// Geht in Intervallen die Ziellisten durch und attackiert pro Durchlauf ein
	/// Ziel. Ausserdem wird darauf geachtet, dass zuerst die NPCs angegriffen werden.
	/// </summary>
	/// 
	void Attack() {
		
		// Greife NPC an 
		// temp -> wegen doppelter Zugriff auf List mit Remove bei null
		// während eines Angriffs kann das Ziel jemand anderes toeten
		NPCDamageReciever[] tempNpcTargets = new NPCDamageReciever[npcTargets.Count];
		npcTargets.CopyTo (tempNpcTargets);

		// Gehe alle Ziele durch bis ein Treffer
		foreach (NPCDamageReciever t in tempNpcTargets)
			if (t) {
				target = t.transform;
				t.TakeDamage (damage, name, t.netId.Value);

				return;

			} else 
				npcTargets.Remove (t);

		// kein NPC vorhanden bzw. getroffen weil null wegen gestorben
		// Greife Spieler an
		PlayerDamageReciever[] tempPlayerTargets = new PlayerDamageReciever[pdrTargets.Count];
		pdrTargets.CopyTo (tempPlayerTargets);

		foreach (PlayerDamageReciever t in tempPlayerTargets)
			if (t && Vector3.Distance(transform.position, t.transform.position) < 5) {
				target = t.transform;
				t.TakeDamage (damage, name, t.netId.Value);

				return;

			} else 
				pdrTargets.Remove (t);	

		// Ersatz für OnTriggerExit, wenn innerhalb des Radius alle
		// Listen leer werden.
		Reset ();
	}

	/// <summary>
	/// Passt die Ziellisten an und kontrolliert, ob noch Ziele vorhanden sind. 
	/// Wenn keine Ziele mehr vorhanden, dann wird der laufende Agriff beendet.
	/// </summary>
	/// <param name="target">Collider des Ziels, das den Angriffsradius 
	/// verlassen hat.</param>
	/// 
	void OnTriggerExit(Collider target) {
		if (!npcTargets.Remove(target.GetComponent<NPCDamageReciever>()))
			pdrTargets.Remove(target.GetComponent<PlayerDamageReciever>());

		Reset ();
	}
		
	/// <summary>
	/// Reset this instance.
	/// </summary>
	private void Reset() {
		if (IsInvoking ("Attack") && npcTargets.Count < 1 && pdrTargets.Count < 1)
			CancelInvoke ("Attack");
	}

	/// <summary>
	/// Beendet die Intervallmethode, falls Objekt zerstoert wird.
	/// </summary>
	void OnDestroy() {
		CancelInvoke ("Attack");
	}

}
