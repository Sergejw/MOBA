using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Die Klasse repraesentiert den Empfaenger fuer Schaden der Spieler. Der Empfaenger verarbeitet die 
/// Schadenspunkte. Der aktuelle Stand der Lebenspunkte wird synchronisiert und wenn keine Lebenspunkte 
/// mehr vorhanden sind, dann wird diesbezueglich eine globale Mitteilung gesendet und Objekt zerstoert.
/// </summary>
public class PlayerDamageReciever : NetworkBehaviour {

	/// <summary>Zustand der Lebenspunkte. Synchronisierte Variable, die bei 
	/// Zustandsaenderung die UpdateLifeState-Methode ausloest.</summary>
	[SyncVar (hook="UpdateLifeState")]
	private float life;

	/// <summary>Ergaenzende Informationen, die in Berechnungen der 
	/// Lebenspunkte einbezogen werden.</summary>
	private AdditionalGUIInfo info;

	/// <summary>Netzwerkmitteilung als Objekt, wenn jemand besiegt wird.</summary>
	private OnKillNetworkMessage okm;

	/// <summary>Lebensbalken GUI.</summary>
	private Slider guiHealthBar;

	/// <summary>Text zum Lebensbalken GUI.</summary>
	private Text guiHealthTxt;

	/// <summary>Ruestung des Spielcharakters.</summary>
	[SyncVar]
	private int armor = 0;

	/// <summary>
	/// Initialisiert die Komponente.
	/// </summary>
	void Start() {
		info = GetComponentInParent<AdditionalGUIInfo>();
		okm = new OnKillNetworkMessage ();

		// GUI
		if (isLocalPlayer) {
			// Lebensbalken
			guiHealthBar = GameObject.Find ("slider_life").GetComponent<Slider> ();
			guiHealthBar.maxValue = info.GetHealthBarValueOnDamage (0);
			guiHealthBar.value = guiHealthBar.maxValue;

			// Text zum Lebensbalken
			guiHealthTxt = GameObject.Find ("txt_life").GetComponent<Text> ();
			guiHealthTxt.text = "Leben: " + guiHealthBar.value + "/" + guiHealthBar.maxValue;
		}
	}
		
	/// <summary>
	/// Erhoeht die Ruestung und liefert anschliessend diese.
	/// </summary>
	/// <returns>Wert der Ruestung.</returns>
	/// <param name="value">Wert, um den die Ruestung erhoeht wird.</param>
	public int IncreaseArmor(int value) {
		armor += value;
		CmdSyncArmor(armor);
		return armor;
	}

	/// <summary>
	/// Teilt den Clients mit, dass die Ruestug um einen Wert erhoeht werden soll.
	/// </summary>
	/// <param name="value">Wert, um den die Ruestung erhoeht wird.</param>
	[Command]
	void CmdSyncArmor(int value) {
		armor = value;
	}

	/// <summary>
	/// Verarbeitet den ankommenden Schaden, den die Serverinstanz uebergibt.
	/// </summary>
	/// <param name="damage">Schadenspunkte.</param>
	/// <param name="attackerName">Angreifername.</param>
	/// <param name="id">ID, wer angegriffen wird.</param>
	public void TakeDamage(int damage, string attackerName, uint id) {
		if (isServer) {
			damage -= armor;

			if (damage > 0) {
				life = info.GetHealthBarValueOnDamage (damage);

				if (life < 1)
					SendOnKillMessage (name, attackerName, id);
			}
		}
	}
		
	/// <summary>
	/// Verarbeitet den ankommenden Schaden, den die Serverinstanz uebergibt und 
	/// der von einem der Spieler kommt.
	/// </summary>
	/// <param name="damage">Schadenspunkte.</param>
	/// <param name="attackerName">Angreifername.</param>
	/// <param name="id">ID, wer angegriffen wird.</param>
	[ClientRpc]
	public void RpcTakeDamage(int damage, string attackerName, uint id) {
		damage -= armor;

		// Erfrage Lebenspunkte durch den Schaden
		life = info.GetHealthBarValueOnDamage(damage);

		if (life < 1) {
			SendOnKillMessage (name, attackerName, id);
			info.ResetHealthBarValue ();
		}
	}

	/// <summary>
	/// Aktuallisiert die Lebenswerte bzw. Anzeigen der lokalen Spieler.
	/// </summary>
	/// <param name="life">Aktueller Lebensstand.</param>
	private void UpdateLokalPlayerLife(float life) {
		guiHealthBar.value = (life < 1) ? guiHealthBar.maxValue : life;
		guiHealthTxt.text = "Leben: " + guiHealthBar.value + "/" + guiHealthBar.maxValue;

	}

	/// <summary>
	/// Aktuallisiert die Lebenswerte bzw. Anzeigen der nicht lokalen Spieler.
	/// </summary>
	/// <param name="life">Aktueller Lebensstand.</param>
	private void UpdateLifeState(float life) {
		info.SetHealthBarValue (life);

		if (life < 1)
			info.ResetHealthBarValue ();

		if (isLocalPlayer)
			UpdateLokalPlayerLife (life);
	}


	/// <summary>
	/// Senden an alle eine Mitteilung, dass jemand besiegt wurde.
	/// </summary>
	/// <param name="targetName">Spieler- bzw. NPCName, des besiegten.</param>
	/// <param name="killedByName">Spielername des Siegers.</param>
	/// <param name="targetNetId">Id des Siegers.</param>
	private void SendOnKillMessage(string targetName, string killedByName, uint targetNetId) {
		okm.killedByName = killedByName;
		okm.targetName = targetName;
		okm.targetNetId = targetNetId;
		NetworkServer.SendToAll (8989, okm);
	}

}
