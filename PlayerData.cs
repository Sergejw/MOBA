using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Die Klasse repraesentiert die Spielerdaten, die nach der ersten Szene zusammengesetzt werden und 
/// welche anschliessend an Duplikate weiter gegeben werden.
/// </summary>
public class PlayerData : MessageBase {

	/// <summary>Spielername.</summary>
	public string name;

	/// <summary>Teamzugehörigkeit.</summary>
	public int team;

	/// <summary>X-Wert der Startposition.</summary>
	public float x;

	/// <summary>Z-Wert der Startposition.</summary>
	public float z;
}