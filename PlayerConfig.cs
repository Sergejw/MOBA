using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Repraesentiert eine Komponente der Spielcharaktere, welche die Grundkonfigurationen / Voreinstellungen
/// behandelt. Behandlung ist in dem Zusammenhang die Verteilung der Voreinstellung (Spielername und 
/// Teamzugehoerigkeit) auf alle Duplikate eigener Instanz. Die lokale Instanz der Clients besitzt die 
/// Daten vor dem Start und hat die Aufgabe diese mittels Server weiterzugeben. Nich lokale Instanz holt 
/// sich die Daten waehrend des Starts und aktualisiert mit diesen den nicht lokalen Spielcharakter.
/// </summary>
public class PlayerConfig : NetworkBehaviour {

    /// <summary>Grundwerte der spielbaren Charaktere. </summary>
	[SerializeField] private int life, damage, level;

    /// <summary>Hoehe, in der sich die erngaenzenden Informationen befinden.</summary>
	[SerializeField] private float offsetLifePosition;

    /// <summary>Lokale Netzwerkmanager.</summary>
	private Manager manager;

    /// <summary>Wahrheitswert, ob Gegner oder nicht.</summary>
	private bool whoIam;

    /// <summary>Ergaenzende Informationen.</summary>
	private AdditionalGUIInfo info;

    /// <summary>Erfahrungsbalken (GUI).</summary>
	private Slider sliderExp;

    /// <summary>Lebensbalken (GUI).</summary>
	private Slider sliderLife;

    /// <summary>Erfahrungswert zu Erfahrungsbalken (GUI).</summary>
	private Text txtExp;

    /// <summary>Lebenswert zu Lebensbalken (GUI).</summary>
	private Text txtLife;

    /// <summary>Schadens- und Rüstungswert (GUI).</summary>
	private Text txtArmor, txtDamage;

	/// <summary> Charaktersteuerung. </summary>
	private PlayerController pc;

	/// <summary> Komponente die die Schadenspunkte verarbeitet. </summary>
	private PlayerDamageReciever pdr;

	/// <summary> Gegnerkennung.</summary>
	private string enemy;

	/// <summary>
	/// Spielerkonfiguration, die den Spielernamen und 
	/// Teamzugehoerigkeit enthaelt. 
	/// </summary>
	[SyncVar] 
	private PlayerData data;

	/// <summary>
	/// Leitet die Spielerkonfiguration an alle Duplikaten (Instanzen) des 
	/// dieses Clients weiter.
	/// </summary>
	/// <param name="localData">Spielerkonfiguration.</param>
	[Command]
	void CmdSyncPlayerData(PlayerData localData) {
		data = localData;
	}
		
	/// <summary>
	/// Initialisiert die lokale Instanz.
	/// </summary>
	public override void OnStartLocalPlayer() {

		// Gebe Daten an Duplikate weiter
		CmdSyncPlayerData (data);

		NetworkManager.singleton.client.connection.RegisterHandler (9999, OnGlobalMatchEvent);

        // Setze Teamzugehoerigkeit lokaler Instanz
		GameObject.Find("NetworkManager").GetComponent<Manager>().myTeam = data.team;
		transform.tag = ""+data.team;

		// Erstelle Symbol in der Miniaturkarte
		GetComponentInParent<DynamicMiniMapSymbol> ().CreateLokalPlayerSymbol ();

		// Aktiviere Charaktersteuerung
		SetUpPlayerController ();

		// Aktiviere aufladbare Faehigkeit (GUI)
		GetComponentInParent<PlayerSkillsGUI> ().enabled = true;

        // GUI Elemente linke obere Ecke
		sliderExp = GameObject.Find ("slider_exp").GetComponent<Slider> ();
		sliderExp.maxValue = 10;
		sliderExp.value = sliderExp.minValue;
		sliderLife = GameObject.Find ("slider_life").GetComponent<Slider> ();
		sliderLife.maxValue = 300;
		sliderLife.value = sliderLife.maxValue;

		txtExp = GameObject.Find ("txt_exp").GetComponent<Text> ();
		txtLife = GameObject.Find ("txt_life").GetComponent<Text> ();
		txtExp.text = "Erfahrung: " + sliderExp.value + "/" + sliderExp.maxValue + " | Level: ";
		txtLife.text = "Leben: " + sliderLife.value + "/" + sliderLife.maxValue;

		txtArmor = GameObject.Find ("txt_armor").GetComponent<Text> ();
		txtDamage = GameObject.Find ("txt_damage").GetComponent<Text> ();

		// Initialisiere Komponente die die Schadenspunkte verarbeitet
		pdr =  GetComponentInParent<PlayerDamageReciever> ();

		// Aktiviere aufladbare Faehigkeit
		enemy = (transform.tag.Equals("0")) ? "1" : "0";
		GetComponentInParent<PlayerSkills> ().Init (damage, enemy);
	}

	/// <summary>
	/// Verarbeitet globale Netzwerknachrichten vor dem Start einer Spielrunde.
	/// </summary>
	/// <param name="message">Globale Netzwerknachricht.</param>
	private void OnGlobalMatchEvent(NetworkMessage message) {
		GlobalMatchEventMessage gmem = message.ReadMessage<GlobalMatchEventMessage> ();
        
		// Spieler vollzaehlig
        if (isLocalPlayer && gmem.gameState) {

			// Erzeuge dynamische NPCs
            if (isServer)
				GameObject.Find("NPCManager").GetComponent<NPCManager>().CreateNPCs();

			// Entferne Begrenzung, damit Einheiten durchlaufen und sich gegenseitig angreifen koennen
            Destroy(GameObject.Find("block"));

			// Aktiviere Inventra bzw. Shop
            SetUpPlayerInventar();
        }

		// Schreibe aktuelle Anzahl der Spieler, die dem Match beigetreten sind
		GameObject.Find ("txt_player_num").GetComponent<Text> ().text = gmem.message;
	}

	/// <summary>
	/// Initialisiert Instanzuebergreifende Charakterelemente.
	/// </summary>
	void Start() {

		// Interne Teamkennung und Objektname
		transform.tag = ""+data.team;
		transform.name += data.name;

		// Hole ergaenzende Informationen
		info = transform.GetComponentInParent<AdditionalGUIInfo> ();

		// Initialisiere ergaenzende Informationen fuer lokalen Spieler
		if (isLocalPlayer) {
			info.Init (level + "/" + data.name, true, offsetLifePosition, life);
			return;
		}
			
		// Hole lokalen Netzwerkmanager
		if (!(manager = GameObject.Find ("NetworkManager").GetComponent<Manager> ())) {
			Debug.Log ("#ERR: NPCStaticConfig->Manager nicht gefunden.");

			return;
		}

		// true = teammate | false enemy
		whoIam = ("" + manager.myTeam).Equals (transform.tag);

		// Initialisiere ergaenzende Informationen fuer Duplikate anderer Spieler
		info.Init (level + "/" + data.name, whoIam, offsetLifePosition, life);

		// Erstelle Symbol in der Miniaturkarte
		GetComponentInParent<DynamicMiniMapSymbol> ().Init(whoIam);
	}
		
	/// <summary>
	///	Aktiviert die Charaktersteuerung.
	/// </summary>
	private void SetUpPlayerController() {
		pc = GetComponentInParent<PlayerController> ();
        pc.enabled = true;
        pc.SetSpawnPosition(new Vector3(data.x, 0, data.z));
	}

	// Liefert die Startposition des Spielcharakters.
	public Vector3 GetSpawnPosition() {
		return new Vector3 (data.x, 0, data.z);
	}
		
	/// <summary>
	/// Aktiviert das Intentar bzw. den Shop.
	/// </summary>
	public void SetUpPlayerInventar() {
		PlayerInventar inv = GetComponentInParent<PlayerInventar> ();
		inv.enabled = true;
	}

	/// <summary>
	/// Initialisiert die Spielerkonfiguration.
	/// </summary>
	/// <param name="playerData">Spielerkonfiguration.</param>
	[Client]
	public void SetPlayerData(PlayerData playerData) {
		data = playerData;
	}

	/// <summary>
	/// Liefert den Spielernamen.
	/// </summary>
	/// <returns>Spielername.</returns>
	[Client]
	public string GetName() {
		return data.name;
	}

	/// <summary>
	/// Liefert die Teamzugehoerigkeit.
	/// </summary>
	/// <returns>Teamzugehoerigkeit.</returns>
	[Client]
	public int GetTeam() {
		return data.team;
	}


	/// <summary>
	/// Verteilt einen Maximalwert der Lebenspunkte an die Clients, welche dann um diesen Wert
	/// die Lebenspunkte erhoehen. Der Maximalwert erhoeht die Lebenspunkte allgemein, wodurch 
	/// Spieler insgesamt mehr Lebenspunkte haben.
	/// </summary>
	/// <param name="value">Lebenspunkte.</param>
	[Command]
	public void CmdIncreaseMaxValueHealthBar(int value) {
		RpcUpdateMaxValueHealthBar (value);
	}

	/// <summary>
	/// Erhoeht die Lebenspunkte eines Charakters zu dem uebergebenen Wert.
	/// Der Maximalwert erhoeht die Lebenspunkte allgemein, wodurch Spieler insgesamt mehr Lebenspunkte haben.
	/// </summary>
	/// <param name="value">Lebenspunkte.</param>
	[ClientRpc]
	void RpcUpdateMaxValueHealthBar(int value) {

		// Clientuebergreifend
		info.IncreaseMaxValueHealthBar (value);

		// Aktuallisiere etnsprechend die GUI des lokalen Spielers
		if (isLocalPlayer) {
			sliderLife.maxValue += value;
			txtLife.text = "Leben: " + sliderLife.value + "/" + sliderLife.maxValue;
		}
	}

	/// <summary>
	/// Verteilt einen aktuellen Wert der Lebenspunkte an die Clients, welche dann um diesen Wert
	/// die Lebenspunkte erhoehen. Der aktuelle Wert repraesentiert den aktuellen Zustand der Lebenspunkte.
	/// </summary>
	/// <param name="value">Lebenspunkte.</param>
	[Command]
	public void CmdIncreaseValueHealthBar(int value) {
		RpcUpdateHealthBar (value);
	}

	/// <summary>
	/// Erhoeht den aktuellen Stand der Lebenspunkte.
	/// </summary>
	/// <param name="value">Lebenspunkte.</param>
	[ClientRpc]
	void RpcUpdateHealthBar(int value) {

		// Clientuebergreifend
		info.SetHealthBarValue (info.GetHealthBarValueOnDamage(0) + value);

		// Aktuallisiere etnsprechend die GUI des lokalen Spielers
		if (isLocalPlayer) {
			sliderLife.value += value;
			txtLife.text = "Leben: " + sliderLife.value + "/" + sliderLife.maxValue;
		}
	}

	/// <summary>
	/// Teilt den Clients mit, dass diese die Lebenspunkte des Charakter auf maximalen 
	/// Wert erhoehen sollen.
	/// </summary>
	[Command]
	void CmdResetHealthBar() {
		RpcResetHealthBar ();
	}

	/// <summary>
	/// Erhoeht die Lebenspunkte des Charakters auf einen maximalen Wert.
	/// </summary>
	[ClientRpc]
	void RpcResetHealthBar() {

		// Clientuebergreifend
		info.ResetHealthBarValue ();

		// Aktuallisiere etnsprechend die GUI des lokalen Spielers
		if (isLocalPlayer) {
			sliderLife.value += sliderLife.maxValue;
			txtLife.text = "Leben: " + sliderLife.value + "/" + sliderLife.maxValue;
		}
	}

	/// <summary>
	/// Erhoeht die Erfahrung der spielbaren Charaktere. 
	/// </summary>
	/// <param name="value">Value.</param>
	public void IncreaseExperience(int value) {

		sliderExp.value += value;

		// Neues Level erreicht.
		if (sliderExp.value >= (10 * level)) {

			// Erhoehe Level und Schadenspunkte
			level++;
			damage++;
			pc.IncreaseDamage (1); // Charakterstuerung
			txtDamage.text = "Schaden: " + pc.GetDamage(); // GUI
			CmdIncreaseMaxValueHealthBar (10); // Maximale Lebenspunkte
			CmdResetHealthBar (); // Setze Lebenspunkte auf maximalen Wert

			// Synchronisiere Level (Anzeige in dem Namen)
            CmdSyncLevel(level);

			// Erhoehe benoetigte Erfahrung fuer naechstes Level
			sliderExp.maxValue = 10 * level;
			sliderExp.value = 0;
		}

		// Aktuallisiere Ausgabe der GUI
		txtExp.text = "Erfahrung: " + sliderExp.value + "/" + sliderExp.maxValue + " | Level: " + level;
	}

	/// <summary>
	/// Teilt den Clients mit, dass diesen den Level setzen sollen.
	/// </summary>
	/// <param name="value">Level.</param>
    [Command]
    void CmdSyncLevel(int value) {
        RpcSetNewLevel(value);
    }

	/// <summary>
	/// Veraendert die ergaenzenden Informationen um den neuen Level.
	/// </summary>
	/// <param name="value">Level.</param>
    [ClientRpc]
    void RpcSetNewLevel(int value) {
        info.SetLevel(value + "/" + data.name);
    }


	/// <summary>
	/// Erhoeht die Schadenspunkte.
	/// </summary>
	/// <param name="value">Wert, um den die Schadenspunkte erhoeht werden.</param>
	public void IncreaseDamage(int value) {

		// Charaktersteuerung
		pc.IncreaseDamage (value);

		// GUI
		txtDamage.text = "Schaden: " + pc.GetDamage();
	}

	/// <summary>
	/// Erhoeht die Ruestungspunkte.
	/// </summary>
	/// <param name="value">Wert, um den die Ruestungspunkte erhoeht werden.</param>
	public void IncreaseArmor(int value) {
		txtArmor.text = "Rüstung: " + pdr.IncreaseArmor(value);
	}

}
