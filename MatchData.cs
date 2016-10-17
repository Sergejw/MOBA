using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

/// <summary>
/// Die Klasse repraesentiert Matchdaten. Diese fuerht je Team eine Liste mit Spielerdaten.
/// Spielerdaten bestehen aus ID, Spielername und Startposition.
/// </summary>
public class MatchData {
    
    /// <summary>Anzahl der Spieler je Team.</summary>
	private int firstTeamPlayerNum, secondTeamPlayerNum;

    /// <summary> Liste der Spielerdaten (ID, Startposition und Name).</summary>
	private Dictionary<int, PlayerData> allPlayerData;

    /// <summary>
    /// Initialisiert die Klasse und somit die Spielerdaten.
    /// </summary>
	public MatchData() {
		allPlayerData = new Dictionary<int, PlayerData> ();
	}

    /// <summary>
    /// Entfernt Spieler aus der Liste.
    /// </summary>
    /// <param name="connectionId">Key, an dem der zu entfernende Spieler identifiziert wird.</param>
	public void RemovePlayer(int connectionId) {
		if (allPlayerData.ContainsKey(connectionId) && allPlayerData [connectionId].team == 0) {
			firstTeamPlayerNum--;

		} else {
			secondTeamPlayerNum--;
		}
		
		allPlayerData.Remove (connectionId);
	}

    /// <summary>
    /// Fuegt der Liste mit Spielerdaten einen neuen Spieler zu.
    /// </summary>
    /// <param name="connectionId">Key.</param>
    /// <param name="name">Spielername.</param>
    /// <returns></returns>
	public PlayerData AddPlayer(int connectionId, string name) {
		PlayerData newPlayer = new PlayerData ();
		newPlayer.name = name;
			
		if (firstTeamPlayerNum > secondTeamPlayerNum) {
			newPlayer.team = 1;
			// Startposition
			newPlayer.x = 3.75f;
			newPlayer.z = 34.37f;
			secondTeamPlayerNum++;
		
		} else {
			newPlayer.team = 0;
			newPlayer.x = 35f;
			newPlayer.z = 4f;
			firstTeamPlayerNum++;
		}
			
		allPlayerData.Add (connectionId, newPlayer);

		return newPlayer;
	}
		

    /// <summary>
    /// Liefert Spielerdaten.
    /// </summary>
    /// <param name="connectionId">Key, vom welchen Spieler die Daten ausgegeben werden sollen.</param>
    /// <returns></returns>
	public PlayerData GetMyData(int connectionId) {
		return allPlayerData[connectionId];
	}
}