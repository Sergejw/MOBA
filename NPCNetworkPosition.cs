using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

/// <summary>
/// Die Klasse repraesentiert die Komponente, die die Position und Rotation der NPCs innerhalb des Netzwerks 
/// synchronisiert.
/// </summary>
[NetworkSettings (channel=1, sendInterval=.01f)]
public class NPCNetworkPosition : NetworkBehaviour {

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
	/// Initiiert die Komponente.
	/// </summary>
	void Start () {
		lastPos = transform.position;
		lastRotation = transform.rotation;
	}

	/// <summary>
	/// Verarbeitet die Werte der Position und Rotation pro Bild.
	/// </summary>
	void Update () {

		// Wenn nicht Server, uebernehme synchronisierte Rotation
		if (!isServer) {
			transform.rotation = rotation;
			lastRotation = transform.rotation;
		}

		// Wenn nicht Server und es sind neue Positionswerte vorhanden
		if (!isServer && positions.Count > 0) {
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

		// Synchronisiere Position, wenn NPC sich ausreichend bewegt hat
		if (isServer && Vector3.Distance (transform.position, lastPos) > gap) {
			position = transform.position;
			lastPos = transform.position;
		}

		// Synchronisiere Position, wenn NPC sich ausreichend gedreht hat
		if (isServer && Quaternion.Angle (transform.rotation, lastRotation) > gap) {
			rotation = transform.rotation;
			lastRotation = transform.rotation;
		}
	}
		
	/// <summary>
	/// Fuegt neue Position einer Liste zu.
	/// </summary>
	/// <param name="pos">Position.</param>
	void AddPosition(Vector3 pos) {
		if (!isLocalPlayer)		
			positions.Add(pos);
	}
}
