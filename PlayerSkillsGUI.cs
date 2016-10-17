using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Die Klasse repraesentiert die GUI der aufladbaren Faehigkeiten. Zusaetzlich ent- oder sperrt 
/// die Komponente die Ausloesung der Fahigkeiten.
/// </summary>
public class PlayerSkillsGUI : MonoBehaviour {

	/// <summary>Aufladbare Faehigkeiten.</summary>
	private Skill[] skills;

	/// <summary>Einzelne Elemente einer aufladbaren Faehigkeit.</summary>
	struct Skill {
		/// <summary>Bild.</summary>
		public Image skillImg;
		/// <summary>Abklingzeit.</summary>
		public float cooldown;
		/// <summary>Zeitpunkt der letzten Ausloesung.</summary>
		public float triggeredAt;
	}

	/// <summary>Abklingzeit als Wert. (1 = eine Faehigkeit ist aufgeladen.)</summary>
	float allAmount = 0;

	/// <summary>
	/// Initialisiert die Komponente.
	/// </summary>
	void Start() {
		
		skills = new Skill[1];

		for (int i = 0; i < skills.Length; ++i) {
			Skill skill = new Skill ();
			skill.skillImg = GameObject.Find ("img_skill_" + i).GetComponent<Image> ();
			skill.cooldown = i + 20; // Abklingzeit
			skills [i] = skill;

			// ............
			// TODO -> gleich nach Start aufgeladen
			// ......
	
		}
	}

	/// <summary>
	/// Startet die Abklingzeit.
	/// </summary>
	/// <returns>Wahrheitswert, ob gestartet werden konnte.</returns>
	/// <param name="i">Index der Faehigkeit.</param>
	public bool Trigger(int i) {
		
		if (skills [i].skillImg.fillAmount >= 1) {
			skills [i].triggeredAt = Time.fixedTime;
			allAmount = 0;
			return true;
		} else
			return false;
	}

	/// <summary>
	/// Verarbeitet visuelle Darstellung von Abklingzeit. Die Faehigkeit wird im 
	/// Uhrzeigersinn aufgeladen.
	/// </summary>
	void Update () {
		
		if ( allAmount >= 3)
			return;
		
		allAmount = 0;

		for (int i = 0; i < skills.Length; ++i) {
			skills [i].skillImg.fillAmount = (Time.fixedTime - skills [i].triggeredAt) / skills [i].cooldown;
			allAmount += skills [i].skillImg.fillAmount;
		}
	}
}
