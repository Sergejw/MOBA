using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Die Klasse repraesentiert die Komponente Hauptmenue. Das Menue gehoert zu der ersten Szene und erfasst 
/// die Voreinstellungen der Benutzer. Zu diesen gehoeren Benutzername, Spielcharakter und Verbindungsdaten.
/// </summary>
public class MainMenu : MonoBehaviour {

	/// <summary>Button fuer Erstellung einer Spielrunde.</summary>
	[SerializeField] private Button btnCreateMatch;

	/// <summary>Button fuer Beitritt einer Spielrunde.</summary>
	[SerializeField] private Button btnJoinMatch;

	/// <summary>Eingabefeld fuer die IP Adresse.</summary>
	[SerializeField] private InputField inputNetworkAdress;

	/// <summary>Eingabefeld fuer die Posrtnummer.</summary>
	[SerializeField] private InputField inputNetworkPort;

	/// <summary>Eingabefeld fuer Spielernamen.</summary>
	[SerializeField] private InputField inputPlayerName;

	/// <summary>Textausgabe bezueglich Spielername.</summary>
	[SerializeField] private Text logMsgPlayerName;

	/// <summary>Textausgabe bezueglich IP Adresse.</summary>
	[SerializeField] private Text logMsgMatchIp;

	/// <summary>Textausgabe bezueglich Portnummer.</summary>
	[SerializeField] private Text logMsgMatchPort;

	/// <summary>Spielcharaktername.</summary>
	[SerializeField] private Text charName;

	/// <summary>Textausgabe bezüglich Fehler allgemein.</summary>
	[SerializeField] private Text txtError;

	/// <summary>Dropdown-Liste fuer Anzahl der SPieler.</summary>
	[SerializeField] private Dropdown playerNum;

	/// <summary>Objekt der Vorschaucharaktere.</summary>
	private GameObject go;

	/// <summary>Netzwerkmanager (Lokal pro Client).</summary>
	private Manager manager;

	/// <summary>
	/// Initiiert das Hauptmenue.
	/// </summary>
	void Start() {
		manager = GameObject.Find ("NetworkManager").GetComponent<Manager> ();

		// Lade Vorschaucharakter
		go = Instantiate(Resources.Load("chars_preview/spider", typeof(GameObject))) as GameObject;
		charName.text = "Spielcharakter: spider";
		manager.cName = "spider";

		// Setze Eingabemuster in die Eingabefelder
		inputNetworkAdress.text = "127.0.0.1";
		inputNetworkPort.text = "7777";
	}

	/// <summary>
	/// Erstellt die Spielcharaktervorschau in der Mitte der Szene. 
	/// </summary>
	/// <param name="name">Charaktername.</param>
	public void LoadCharPreview(string name) {

		// Zerstoere letzte Vorschau
		if (go)
			Destroy (go);

		// Lade Vorschaucharakter
		go = Instantiate(Resources.Load("chars_preview/" + name, typeof(GameObject))) as GameObject;
		// Name von Vorschaucharakter
		charName.text = "Spielcharakter: " + name;
		// Teile dem Manager die Charakterauswahl mit
		manager.cName = name;
	}
		

	/// <summary>
	/// Loest den Beitritt zu einer Spielrunde aus. War der Versuch nicht erfolgreich, wird eine Fehlermeldung ausgegeben.
	/// </summary>
	public void JoinMatch() {
		if (inputPlayerName.text.Length > 3 && inputNetworkAdress.text.Length > 6 && inputNetworkPort.text.Length > 1) {
			manager.networkPort = Convert.ToInt32(inputNetworkPort.text);
			manager.networkAddress = inputNetworkAdress.text;
			manager.StartClient();
		} else {
			txtError.text = txtError.text = "#FEHLER: Angaben fehlerhaft siehe links unten!";
		}
	}
		
	/// <summary>
	/// Loest die Erstellung einer Spielrunde aus. War der Versuch nicht erfolgreich, wird eine Fehlermeldung ausgegeben.
	/// </summary>
	public void CreateMatch() {
		
		// TODO IP und Port bessere ueberprüfung
		if (inputPlayerName.text.Length > 3 && inputNetworkAdress.text.Length > 6 && inputNetworkPort.text.Length > 1) {
			manager.networkPort = Convert.ToInt32(inputNetworkPort.text);
			manager.networkAddress = inputNetworkAdress.text;
			Debug.Log (playerNum.value);
			if (playerNum.value == 0) {
				manager.maxConnections = 1;

			}
			else if (playerNum.value == 1)
				manager.maxConnections =  3;

			Debug.Log (manager.maxConnections);

			manager.StartHost ();
		} else {
			txtError.text = txtError.text = "#FEHLER: Angaben fehlerhaft siehe links unten!";
		}
	}
		
	/// <summary>
	/// Prueft, ob ein Spielername vorhanden und markiert entsprechend farblich den Informationstext, 
	/// der zu Eingabe des Spielernamen auffordert.
	/// </summary>
	/// 
	public void ValidatePlayerNameInput() {

		// Wenn Spielername vorhanden, dann markiere Informationstext gruen
		// sonst gelb. 
		if (inputPlayerName.text.Length > 3) {
			logMsgPlayerName.color = Color.green;
			manager.playerName = inputPlayerName.text;

		} else {

			if (txtError.text.Length > 0)
				txtError.text = "";
			
			logMsgPlayerName.color = Color.yellow;
			manager.playerName = null;
		}
	}


	/// <summary>
	/// Prueft, ob ein Spielername vorhanden und markiert entsprechend farblich den Informationstext, 
	/// der zu Eingabe des Spielernamen auffordert.
	/// </summary>
	/// 
	public void ValidateMatchIp() {

		// Wenn Matchname vorhanden, dann markiere Informationstext gruen
		// sonst gelb. 
		if (inputNetworkAdress.text.Length > 6) {
			logMsgMatchIp.color = Color.green;

		} else {
				logMsgMatchIp.color = Color.yellow;
		}
	}

	/// <summary>
	/// Validiert die Eingabe der Portnummer.
	/// </summary>
	public void ValidateMatchPort() {

		// Wenn Matchname vorhanden, dann markiere Informationstext gruen
		// sonst gelb. 
		if (inputNetworkPort.text.Length > 0) {
			logMsgMatchPort.color = Color.green;

		} else {
			logMsgMatchPort.color = Color.yellow;
		}
	}

	/// <summary>
	/// Schliesst / Beendet die Anwendung.
	/// </summary>
	public void Quit() {
		Application.Quit();
	}
}
