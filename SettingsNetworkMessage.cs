using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Die Klasse repraesentiert die Teilkomponente der Voreinstellungen. Voreinstellungen enthalten den eingegebenen
/// Spielernamen und ausgewaehlten Spielcharakter. Diese werden als eine Netzwerknachricht aus der ersten 
/// Szene in die zweite geschickt. Diese Netzwerknachricht ist diese Klasse.
/// </summary>
public class SettingsNetworkMessage : MessageBase {

	/// <summary>Spielcharakter.</summary>
	public string character;

	/// <summary>Spielername.</summary>
	public string playerName;
}
