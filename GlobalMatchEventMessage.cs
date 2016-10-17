using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Die Klasse repraesentiert eine Netzwerknachricht, die als Mitteilung ein globales Event ausloest.
/// Mitteilungen: Aktuelle Anzahl der Spieler, Match beginnt und Match beendet.
/// </summary>
public class GlobalMatchEventMessage : MessageBase {

	/// <summary>Nummer, was ausgeloest werden soll.</summary>
	public int eventInCase;

	/// <summary>Nachricht.</summary>
	public string message;

	/// <summary>Spielzustand. Spiel zu ende oder nicht.</summary>
	public bool gameState;
}
