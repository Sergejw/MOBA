using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;

/// <summary>
/// Die Klasse repraesentiet den Manager, der die Netzwerk- und Szenenkommunikation ermoeglicht. Die 
/// Komponente ist konstant in der ersten und zweiten Szene vorhanden und hat folgende Aufgaben: 
/// Mitteilung der Teamkennung der lokalen Spieler, Mitteilung der globalen Nachrichten und Verarbeitung 
/// der Voreinstellungen.
/// </summary>
public class Manager : NetworkManager {

	/// <summary>Matchdaten (Wer in welchem Team spielt).</summary>
	public MatchData matchData;

	/// <summary>Manager als Objekt.</summary>
	public NetworkManager manager;

	/// <summary>Charaktername aus den Voreinstellungen (erste Szene).</summary>
	public string cName;

	/// <summary>Spielername aus den Voreinstellungen (erste Szene).</summary>
	public string playerName;

	/// <summary>Teamkennung des lokalen Spielers.</summary>
	public int myTeam;

	/// <summary>wahrheitswert, ob das Spiel lauft oder bereits beendet ist.</summary>
	public bool gameOver = false;

	/// <summary>
	/// Initiiert den Manager als Objekt.
	/// </summary>
	void Start() {
		manager = GetComponent<NetworkManager>();
	}
		
	/// <summary>
	/// Erstellt ein Objekt, der die Spieldaten enthaelt und registriert einen 
	/// Handler, der bei Anfrage die Spieldaten an die Clients sendet.
	/// </summary>
	public override void OnStartServer() {
		matchData = new MatchData();
		NetworkServer.RegisterHandler(4545, SendMeMatchData);
	}

	/// <summary>
	/// Sendet die Matchdaten an den Client, der die Anfrage gesendet hat.
	/// </summary>
	/// <param name="netMsg">Netzwerknachricht als Anfrage.</param>
	public void SendMeMatchData(UnityEngine.Networking.NetworkMessage netMsg) {
		netMsg.conn.Send (4545, matchData.GetMyData(netMsg.conn.connectionId));
	}
		

	/// <summary>
	/// Ein Workaround, weil die Verbindung sonst doppelt gesetzt wird.
	/// </summary>
	/// <param name="conn">Verbindung.</param>
	public override void OnClientSceneChanged(NetworkConnection conn) {}


	/// <summary>
	/// Teilt dem Server mit, dass ein Spieler mit Voreinstellungen erstellt werden soll.
	/// (INFO: Die Methode wird in der Clientinstanz aufgerufen, wenn Verbindung zum Server steht.)
	/// </summary>
	/// <param name="conn">Verbindung zum Client.</param>
	public override void OnClientConnect(NetworkConnection conn) {
		SettingsNetworkMessage snm = new SettingsNetworkMessage ();
		snm.character = cName;
		snm.playerName = playerName;

		ClientScene.AddPlayer(conn, 0, snm);
	}
		
	/// <summary>
	/// Entfernt einen Client aus Matchdaten, sobald dieser die Verbindung abbricht. Nach der 
	/// Entfernung wird weiteren Spielern die neue Spielerzahl mitgeteilt.
	/// (INFO: Die Methode wird in der Serverinstanz aufgerufen, wenn Verbindung zum Client abbricht.)
	/// </summary>
	/// <param name="conn">Verbindung zum Client.</param>
	public override void OnServerDisconnect(NetworkConnection conn) {
		matchData.RemovePlayer (conn.connectionId);
		base.OnServerDisconnect (conn);

		// Sende diesbezueglich globale Nachricht
		GlobalMatchEventMessage gmem = new GlobalMatchEventMessage ();
		gmem.message = (NetworkServer.connections.Count - 1) + " / " + (maxConnections + 1) + " Spieler on";
		NetworkServer.SendToAll (9999, gmem);
	}

	/// <summary>
	/// Loescht die Matchdaten.
	/// </summary>
	public override void OnStopServer() {
		matchData = null;
	}
		
	/// <summary>
	/// Erstellt einen Spielcharakter, den ein Client mit Voreinstellungen angefragt hat. Nach der Erstellung wird alle Clients 
	/// der neue Spieler mitgeteilt.
	/// (INFO: Die Methode wird in der Serverinstanz aufgerufen, wenn Client einen Spielcharakter erstellen bzw. haben moechte.)
	/// </summary>
	/// <param name="conn">Verbindung zum Client, der die Anfrage gesendet hat.</param>
	/// <param name="playerControllerId">ID des Spielers, der in Vebindung zum Objekt steht.</param>
	/// <param name="extraMessageReader">Voreinstellungen.</param>
	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader) {

		// Lasse keinen Client mitspielen, wenn Match zu ende
		if (gameOver) {
			conn.Disconnect ();
			return;
		}

		// Lese Voreinstellungen aus der ersten Szene und erstelle daraus den angefragten Charakter
		SettingsNetworkMessage message = extraMessageReader.ReadMessage< SettingsNetworkMessage>();
		GameObject player =  Instantiate(Resources.Load("playable_chars/" + message.character, typeof(GameObject))) as GameObject;
		// Schreibe Matchdaten in die lokale Konfiguration, die der Client an seine Duplikate verteilt
		player.transform.GetComponent<PlayerConfig> ().SetPlayerData(matchData.AddPlayer (conn.connectionId, message.playerName));
		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

		// Sende diesbezueglich globale Nachricht (aktuelle Anzahl der Spieler)
		GlobalMatchEventMessage gmem = new GlobalMatchEventMessage ();

		if (NetworkServer.connections.Count == (maxConnections + 1)) {
			gmem.message = NetworkServer.connections.Count + " / " + (maxConnections + 1) + " Spieler on";
			gmem.gameState = true;
		
		} else {
			gmem.message = NetworkServer.connections.Count + " / " + (maxConnections + 1) + " Spieler on";
		}

		NetworkServer.SendToAll (9999, gmem);
	}
}
