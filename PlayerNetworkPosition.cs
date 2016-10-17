using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

/// <summary>
/// Die Klasse repraesentiert die Komponente, die die Position und Rotation der Spieler innerhalb des Netzwerks 
/// synchronisiert.
/// </summary>
[NetworkSettings (channel=1, sendInterval=.01f)]
public class PlayerNetworkPosition : NetworkBehaviour {

	/// <summary> Synchronisierte Variable der Position. Sobald sich der Positionswert aendert, 
	/// wird die Methode AddPosition ausgeloest, die die Werte in die Liste eintraegt.</summary>
	[SyncVar (hook="AddPosition")]
	Vector3 position;

	/// <summary> Synchronisierte Variable der Rotation.</summary>
	[SyncVar]
	Quaternion rotation;

	/// <summary> Letzte Drehung die als Pruefwert fuer Sensibilitaet genutzt wird. </summary>
	Quaternion lastRotation;

	/// <summary> Letzte Position die als Pruefwert fuer Sensibilitaet genutzt wird. </summary>
	Vector3 lastPos;

	/// <summary> Sensibilitaet. Veraenderte Positions- und Rotationswerte die kleiner sind, werden
	/// nicht uebertragen.</summary>
	float gap = 0.04f;

	/// <summary> Maximale Verarbeitungsgeschwindigkeit der Positionswerte.</summary>
	float maxSpeed = 80f;

	/// <summary> Liste der Positionswerte. </summary>
	private List<Vector3> positions = new List<Vector3> ();

	/// <summary>
	/// Initialisiert die erste Position und Rotation.
	/// </summary>
	[Client]
	void Start () {
		lastPos = transform.position;
		lastRotation = transform.rotation;
	}

	/// <summary>
	/// Verarbeitet die Werte der Position und Rotation pro Bild.
	/// </summary>
	[Client]
	void Update () {

		// Wenn nicht lokaler Spieler, uebernehme synchronisierte Rotation
		if (!isLocalPlayer) {
			transform.rotation = rotation;
			lastRotation = transform.rotation;
		}

		// Wenn nicht lokaler Spieler und es sind neue Positionswerte vorhanden
		if (!isLocalPlayer && positions.Count > 0) {
			// Verarbeite Positionswerte mit Interpolation
			if (positions.Count > 10)
				transform.position = Vector3.Lerp (transform.position, positions[0], Time.deltaTime * maxSpeed);

			// Verarbeite Positionswerte, wenn die Liste zu lang wird
			else
				transform.position = Vector3.Lerp (transform.position, positions[0], Time.deltaTime * 30f);

			// Entferne verarbeitete Werte aus der Liste
			if (Vector3.Distance (transform.position, positions[0]) <= gap)
				positions.RemoveAt (0);

			lastPos = transform.position;
		}

		// Synchronisiere Position, wenn Spieler sich ausreichend bewegt hat
		if (isLocalPlayer && Vector3.Distance (transform.position, lastPos) > gap) {
			CmdSyncPosition (transform.position);
			lastPos = transform.position;
		}

		// Synchronisiere Position, wenn Spieler sich ausreichend gedreht hat
		if (isLocalPlayer && Quaternion.Angle (transform.rotation, lastRotation) > gap) {
			CmdSyncRotation (transform.rotation);
			lastRotation = transform.rotation;
		}
	}

	/// <summary>
	/// Teilt den Clients mit, dass diese die Position synchronisieren sollen.
	/// </summary>
	/// <param name="pos">Position.</param>
	[Command]
	void CmdSyncPosition(Vector3 pos) {
		position = pos;
	}

	/// <summary>
	/// Teilt den Clients mit, dass diese die Rotation synchronisieren sollen.
	/// </summary>
	/// <param name="rot">Rotation.</param>
	[Command]
	void CmdSyncRotation(Quaternion rot) {
		rotation = rot;
	}

	/// <summary>
	/// Fuegt neue Position einer Liste zu.
	/// </summary>
	/// <param name="pos">Position.</param>
	[Client]
	void AddPosition(Vector3 pos) {
		if (!isLocalPlayer)		
			positions.Add(pos);
	}
}
