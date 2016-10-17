using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Die Klasse repraesentiert einen Flaechenangriff der Spielcharaktere. Alle Gegner, die 
/// sich im Angriffsradius befinden, werden in eine Liste erfasst und in Intervallen angegriffen. 
/// </summary>
public class PlayerSkillAoE : MonoBehaviour {

	/// <summary>Kennung der Gegner.</summary>
	public string enemy;

	/// <summary>Liste mit erfassten Gegnern.</summary>
	private List<GameObject> targets = new List<GameObject>();

	/// <summary>Callback zum Spielcharakter, weil zum Weitergeben der Schadenspunkte 
	/// Netzwerkfunktionen benoetigt werden.</summary>
	public Action<GameObject> act;

	/// <summary>
	/// Erfasst alle Gegner in eine Liste und startet den Angriff, falls noch 
	/// nicht aktiv.
	/// </summary>
	/// <param name="other">Gegner.</param>
	void OnTriggerEnter(Collider other) {
		if (act != null)
		if (other.tag.Contains (enemy) && !targets.Contains(other.gameObject)) {
			targets.Add (other.gameObject);

			if (!IsInvoking ("Attack"))
				InvokeRepeating ("Attack", 0, 1);
		}
	}

	/// <summary>
	/// Greift alle Gegner aus der Liste in Intervallen an.
	/// </summary>
	void Attack() {
		GameObject[] tempTargets = new GameObject[targets.Count];
		targets.CopyTo (tempTargets);

		foreach (GameObject go in tempTargets)
			if (go == null) {
				targets.Remove (go);
			} else {
				act (go);
			}			
	}
		
	/// <summary>
	/// Entfernt Gegner, die den Angriffsradius verlassen, aus der Liste.
	/// Beendet Angriff, wenn Liste leer. 
	/// </summary>
	/// <param name="other">Gegner.</param>
	void OnTriggerExit(Collider other) {
		targets.Remove (other.gameObject);

		if (targets.Count < 1)
			CancelInvoke ("Attack");
	}
}
  