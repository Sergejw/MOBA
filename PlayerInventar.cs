using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Die Klasse repraesentiert das Inventar bzw. den Shop, wo die Spieler die Gegenstaende
/// erwerben koennen.
/// </summary>
public class PlayerInventar : MonoBehaviour {

	/// <summary>Waehrung, mit der eingekauft wird.</summary>
	private int gold = 0;

	/// <summary>Textausgabe der Waehrung.</summary>
	private Text goldText;

	/// <summary>Gegenstaende.</summary>
	private Button item0, item1, item2, item4, item5;

	/// <summary>Spielerkonfiguration.</summary>
	private PlayerConfig pcc;

	/// <summary>
	/// Initialisiert die Komponente.
	/// </summary>
	void Start () {
		pcc = GetComponentInParent<PlayerConfig> ();

		goldText = GameObject.Find ("txt_gold").GetComponent<Text> ();

		// Erhoehe Stand der Waehrung jede halbe Sekunde
		InvokeRepeating("IncreaseGold", 0, 5.0f);


		// Initialisiere Gegenstaende
		item0 = GameObject.Find ("btn_item_0").GetComponent<Button> ();
		item0.onClick.AddListener(delegate { UseItem(0); });

		item1 = GameObject.Find ("btn_item_1").GetComponent<Button> ();
		item1.onClick.AddListener(delegate { UseItem(1); });

		item2 = GameObject.Find ("btn_item_2").GetComponent<Button> ();
		item2.onClick.AddListener(delegate { UseItem(2); });

		item4 = GameObject.Find ("btn_item_4").GetComponent<Button> ();
		item4.onClick.AddListener(delegate { UseItem(4); });

		item5 = GameObject.Find ("btn_item_5").GetComponent<Button> ();
		item5.onClick.AddListener(delegate { UseItem(5); });

	}

	/// <summary>
	/// Setzt Stand der Waehrung. 
	/// </summary>
	/// <param name="value">Werte, der dem Stand entspricht.</param>
	public void SetGold(int value) {
		gold = value;
		goldText.text = "Gold: " + gold;
	}

	/// <summary>
	/// Erhoeht Stand der Waehrung.
	/// </summary>
	private void IncreaseGold() {
		if (gold < 5000) {
			gold++;
			goldText.text = "Gold: " + gold;
		}
	}

	/// <summary>
	/// Setzt Waehrung auf Null.
	/// </summary>
	public void ResetInventar() {
		gold = 0;
	}

	/// <summary>
	/// Durchlaeuft den Erwerb der Gegenstaende.
	/// </summary>
	/// <param name="id">Id des Gegenstands, welches erworben werden soll.</param>
	public void UseItem(int id) {
		switch (id) {
		case 0:
			// "Kaufe" Ruestung
			if (gold >= 43) {
				pcc.IncreaseArmor (1); // Erhoehe Ruestung mittels Konfiguration
				gold -= 43; // Berechne den Preis
			}
			break;
		case 1:
			if (gold >= 30) {
				pcc.IncreaseExperience(2);
				gold -= 30;
			}
			break;
		case 2:
			if (gold >= 66) {
				pcc.IncreaseDamage(1);
				gold -= 66;
			}
			break;
		case 3:
			break;
		case 4:
			if (gold >= 25) {
				pcc.CmdIncreaseValueHealthBar(40);
				gold -= 25;
			}
			break;
		case 5:
			if (gold >= 58) {
				pcc.CmdIncreaseMaxValueHealthBar (10);
				gold -= 58;
			}
			break;
		}

		goldText.text = "Gold: " + gold;
	}
}
