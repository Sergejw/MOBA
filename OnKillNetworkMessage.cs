using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Die Klasse repraesentiert die Netwerknachricht, die einen Zwischensieg
/// mitteilt.
/// </summary>
public class OnKillNetworkMessage : MessageBase {

	/// <summary> Name des Siegers. </summary>
	public string killedByName;

	/// <summary> Name des Verlierers. </summary>
	public string  targetName;

	/// <summary> Id des Verlierers. </summary>
	public uint targetNetId;

	/// <summary> Id des Siegers. </summary>
	public uint  killedByNetId;
}
