using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

/// <summary>
/// Die Klasse repraesentiert die aufladbaren Faehigkeiten der Spielcharaktere. Aus zeitlichen Gruenden wurde als Beispiel zunaechst 
/// eine Faehigkeit implementiert.
/// </summary>
public class PlayerSkills : NetworkBehaviour {

	/// <summary>Aufladbare Faehigkeit als Objekt.</summary>
	private GameObject skill;

	/// <summary>Spielerkonfiguration.</summary>
	private PlayerConfig config;

	/// <summary>Schaden.</summary>
	private int damage;

	/// <summary>Gegnerkennung.</summary>
	private string enemy;

	/// <summary>
	/// Initialisiert aufladbare Faehigkeiten fuer lokale Instanz.
	/// </summary>
	/// <param name="dmg">Grundschaden.</param>
	/// <param name="ene">Kennung der Gegner. (team = 0 oder team = 1)</param>
	public void Init(int dmg, string ene) {
		damage = dmg;
		enemy = ene;
		config = GetComponentInParent<PlayerConfig> ();
	}

	/// <summary>
	/// Setzt den Schaden.
	/// </summary>
	/// <param name="dmg">Schaden.</param>
	public void SetDamage(int dmg) {
		damage = dmg;
	}

	/// <summary>
	/// Loest eine aufladbare Faehigkeit aus.
	/// </summary>
	/// <param name="name">Prefabname der Faehigkeit.</param>
	/// <param name="i">Index, wenn mehrere.</param>
	[ClientRpc]
	public void RpcTrigger(string name, int i) {
		skill = Instantiate (Resources.Load ("skills/SCSpell_BladeWhirlwind", typeof(GameObject))) as GameObject;
		skill.transform.position = transform.position;

		// Zerstoere die Faehigkeit nach 3 Sekunden
		Destroy (skill, 3);

		// Folge dem Spielcharakter
		InvokeRepeating ("Follow", 0, Time.deltaTime);

		// Aktiviere die Faehigkeit
		if (isLocalPlayer) {
			skill.GetComponent<PlayerSkillAoE> ().enabled = true;
			skill.GetComponent<PlayerSkillAoE> ().enemy = enemy;
			skill.GetComponent<PlayerSkillAoE> ().act = SendDamage; //Callback
		}
	}

	/// <summary>
	/// Sendet den Schaden an Ziele.
	/// </summary>
	/// <param name="go">Ziel als Objekt.</param>
	void SendDamage(GameObject go) {

		// NPC
		if (go.GetComponent<NPCDamageReciever> ()) 
			CmdSendFindDamage (go.name, transform.name);

		// Spieler
		else
			CmdSendDamage (go, transform.name);

		// Erhoehe Erfahrung
		config.IncreaseExperience (1);
	}

	/// <summary>
	/// Teilt den Clients mit, dass der Charakter Schaden empfangen soll.
	/// </summary>
	/// <param name="go">Charakter als Objekt.</param>
	/// <param name="name">Name des Angreifers.</param>
	[Command]
	void CmdSendDamage(GameObject go, string name) {
		uint id = go.GetComponentInParent<NetworkIdentity> ().netId.Value;
		go.GetComponent<PlayerDamageReciever> ().TakeDamage (damage + 10, name, id);
	}

	/// <summary>
	/// Teilt den Clients mit, dass das NPC-Objekt Schaden empfangen soll.
	/// </summary>
	/// <param name="name">Name von NPC-Objekt.</param>
	/// <param name="attacker">Angreifername.</param>
	[Command]
	void CmdSendFindDamage(string name, string attacker) {
		GameObject go = GameObject.Find (name);

		if (go) {
			uint id = go.GetComponentInParent<NetworkIdentity> ().netId.Value;
			go.GetComponent<NPCDamageReciever> ().TakeDamage (damage + 10, attacker, id);
		}
	}

	/// <summary>
	/// Folgt dem Spielcharakter.
	/// </summary>
	void Follow() {
		if (!skill) {
			CancelInvoke ("Follow");
		}
		 else 
			skill.transform.position = transform.position;
	}


}
