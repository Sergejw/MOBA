using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary>
/// Die Klasse repraesentiert einen Button bzw. den Code dahinter, der das Verlassen der 
/// Spielrunde ermoeglicht.
/// </summary>
public class BtnQuitMatch : NetworkBehaviour {

	/// <summary>
	/// Beendet das laufende Match.
	/// </summary>
	public void QuitMatch() {

		// Matchinitiator (Host)
		if (NetworkServer.active && NetworkClient.active)
			GameObject.Find ("NetworkManager").GetComponent<Manager> ().StopHost ();

		// Den Match beigetreten (Client)
		else if (!NetworkServer.active && NetworkClient.active)
			GameObject.Find ("NetworkManager").GetComponent<Manager> ().StopClient ();
	}

}
