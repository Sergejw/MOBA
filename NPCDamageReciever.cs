using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Die Klasse repraesentiert den Empfaenger fuer Schaden der NPCs. Der Empfaenger verarbeitet die 
/// Schadenspunkte. Der aktuelle Stand der Lebenspunkte wird synchronisiert und wenn keine Lebenspunkte 
/// mehr vorhanden sind, dann wird diesbezueglich eine globale Mitteilung gesendet und Objekt zerstoert.
/// </summary>
public class NPCDamageReciever : NetworkBehaviour {

	/// <summary>Zustand der Lebenspunkte. Synchronisierte Variable, die bei 
	/// Zustandsaenderung die UpdateLifeState-Methode ausloest.</summary>
	[SyncVar (hook="UpdateLifeState")]
	private float life;

	/// <summary>Ergaenzende Informationen, die in Berechnungen der 
	/// Lebenspunkte einbezogen werden.</summary>
	private AdditionalGUIInfo info;

	/// <summary>Netzwerkmitteilung als Objekt, wenn jemand besiegt wird.</summary>
	private OnKillNetworkMessage okm;

	/// <summary>
	/// Initiert die Komponente.
	/// </summary>
	void Start() {
		info = GetComponentInParent<AdditionalGUIInfo>();
		okm = new OnKillNetworkMessage ();
	}


	/// <summary>
	/// Verarbeitet den ankommenden Schaden, den die Serverinstanz uebergibt.
	/// </summary>
	/// <param name="damage">Schadenspunkte.</param>
	/// <param name="attackerName">Angreifername.</param>
	/// <param name="id">ID, wer angegriffen wird.</param>
	public void TakeDamage(int damage, string attackerName, uint id) {
		if (isServer) {

			// Erfrage Lebenspunkte durch den Schaden
			life = info.GetHealthBarValueOnDamage (damage);

			// Wenn Ziel eine Basis, dann Sende diesbezueglich globale Nachricht
			// (Systemmeldung)
			if (life < 1 && transform.name.Contains ("base")) 
				SendOnKillMessage (name, attackerName, id);
		}
	}
		

	/// <summary>
	/// Aktuallisiert die Lebenspunkte, nachdem die Variable ([SyncVar] life) den 
	/// Zustand aendert.
	/// </summary>
	/// <param name="life">Aktuelle lebenspunkte.</param>
	private void UpdateLifeState(float life) {
		info.SetHealthBarValue (life);

		if (life < 1) {
			// Wenn Ziel eine Basis, dann Sende diesbezueglich globale Nachricht
			// (Event: Matchende)
			if (transform.name.Contains ("base")) {
				GlobalMatchEventMessage gmem = new GlobalMatchEventMessage ();
				gmem.gameState = false;
				NetworkServer.SendToAll (9999, gmem);
			}

			Destroy (gameObject);
		}
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
