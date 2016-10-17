using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Die Klasse repraesentiert eine Komponente, die von der Instanz von Spielinitiator aus die NPCs erzeugt. 
/// Die Instanz des Initiators kommuniziert dabei mit der Serverinstanz, welche die Kontrolle der NPCs
/// uebernimmt.
/// </summary>
public class NPCManager : NetworkBehaviour {

	/// <summary> Startposition der NPCs aus dem ersten Team. </summary>
	private Vector3 startpositionFirstTeam;

	/// <summary> Startposition der NPCs aus dem zweiten Team. </summary>
	private Vector3 startpositionSecondTeam;

	/// <summary>Anzahl erzeugter NPCs in einer Welle. </summary>
	int i = 0;

	/// <summary>
	/// Erzeugt statische NPCs und initialisiert die Startpositionen der NPCs.
	/// (INFO: Wird aufgerufen, sobald Serverinstanz startet.)
	/// </summary>
	public override void OnStartServer() {
		startpositionFirstTeam = GameObject.Find ("startposition_team_0").transform.position;
		startpositionSecondTeam = GameObject.Find ("startposition_team_1").transform.position;

		Spawn ("base_0");
		Spawn ("base_1");
		Spawn ("tower_0");
		Spawn ("tower_1");
		Spawn ("tower_0_1");
		Spawn ("tower_1_1");
		Spawn ("tower_0_2");
		Spawn ("tower_1_2");
	}

	/// <summary>
	/// Beendet die Intervalle, wo die NPCs erzeugt werden. Anschliessend werden noch vorhandene
	/// NPCs zerstoert.
	/// </summary>
	public void Stop() {

		// Beende Intervalle
		CancelInvoke ("Wave1");
		CancelInvoke ("Wave");

		// Suche und zerstoere NPCs des ersten Teams
		GameObject[] gos = GameObject.FindGameObjectsWithTag ("0");
		foreach (GameObject go in gos) {
			if (go.name.Contains ("vasal"))
				Destroy (go);
		}

		// Suche und zerstoere NPCs des zweiten Teams
		gos = GameObject.FindGameObjectsWithTag ("1");
		foreach (GameObject go in gos) {
			if (go.name.Contains ("vasal"))
				Destroy (go);
		}
			
	}

	/// <summary>
	/// Startet Intervallmethode zur Erzeugung der NPC-Wellen.
	/// </summary>
	public void CreateNPCs() {
		if (!IsInvoking ("Wave1"))
			InvokeRepeating ("Wave1", 0, 40f);
	}

	/// <summary>
	/// Startet Intervallmethode zur Erzeugung der NPCs. Erzeugt werden einzelne NPCs mit Zeitabstaenden in 
	/// der Intervallmethode der Wellen.
	/// </summary>
	void Wave1() {
		i = 0;
		InvokeRepeating ("Wave", 0, 0.7f);
	}
		

	/// <summary>
	/// Erzeugt NPCs.
	/// </summary>
	void Wave() {
		if (i < 7) {
			i++;
			// Erzeuge NPCs des ersten Teams
			GameObject go = Instantiate (Resources.Load ("NPCvasal_0", typeof(GameObject))) as GameObject;
			go.transform.position = startpositionFirstTeam;
			NetworkServer.Spawn (go);

			// Erzeuge NPCs des zweiten Teams
			go = Instantiate (Resources.Load ("NPCvasal_1", typeof(GameObject))) as GameObject;
			go.transform.position = startpositionSecondTeam;
			NetworkServer.Spawn (go);

		} else {
			CancelInvoke ("Wave");
		}
	}

	/// <summary>
	/// Erzeugt einen NPC.
	/// </summary>
	/// <param name="objectName">Prefabname.</param>
	private void Spawn(string objectName) {
		GameObject go = Instantiate(Resources.Load(objectName, typeof(GameObject))) as GameObject;
		NetworkServer.Spawn (go);
	}
}