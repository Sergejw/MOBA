using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Die Klasse repraesentiert einen Chat, der einfachen Austausch von Nachrichten ermoeglicht. 
/// Die Nachrichten werden dem Eingabefeld entnommen und mittels Server snychronisiert. Alle 
/// Clientinstanzen schreiben somit ihre eigenen Nachrichten in das Chatfenster, welches sich 
/// in der linken unteren Ecke befindet.
/// </summary>
public class Chat : NetworkBehaviour {

	/// <summary>Zuletzt lokal abgeschickte Nachricht, die die Duplikate holen.</summary>
	[SyncVar (hook="UpdateChatContent")]
	private string lastChatMessage;

	/// <summary>Bereich, der die Nachrichten enthaelt.</summary>
	private GameObject content;

	/// <summary>Scrollbalken, womit der Bereich mit Nachrichten nach unten gescrollt wird.</summary>
	private Scrollbar scrollbar;

	/// <summary>Eingabefeld.</summary>
	private InputField chatInputField;

	/// <summary>Verbindung zum Inventar bzw. Shop, welche Eingabe von Cheat waehrend der 
	/// Tests ermoeglicht.</summary>
	private PlayerInventar pi;

	/// <summary>Wahrheitswert, ob eine neue Nachricht vorhanden ist.</summary>
	bool newMsg = false;

	/// <summary>
	/// Initialisiert den Chat.
	/// </summary>
	void Start() {
		if (isLocalPlayer)
			pi = GetComponentInParent<PlayerInventar> ();
	
			
		scrollbar = GameObject.Find ("Scrollbar Vertical").GetComponent<Scrollbar> ();
		scrollbar.onValueChanged.AddListener(delegate{ScrollContentToBottom();});

		chatInputField = GameObject.Find ("InputField").GetComponent<InputField>();
		chatInputField.onEndEdit.AddListener(delegate{SyncChatMessage(chatInputField);});

		content =  GameObject.Find ("Content");
	}


	/// <summary>
	/// Scrollt den Bereich mit Nachrichten runten.
	/// </summary>
	void ScrollContentToBottom() {
		if (newMsg) {
			scrollbar.value = 0;
			newMsg = false;
		}
	}


	/// <summary>
	/// Aktualisiert den Chatinhalt.
	/// </summary>
	/// <param name="message">Nachricht, die dem Chat zugefuegt wird.</param>
	private void UpdateChatContent(string message) {

        if (message == null)
            return;

		// Workaround: Listener für Scrollbar. Scrollbar ist sonst mit 
		// Cursor nicht bewegbar, da die Position immer auf 0 gesetzt wird.
		newMsg = true;

		// .......................................
		// TODO
		// Linebreak passend zur Groesse
		// .........................................
			
		// Erstelle Objekt der Nachricht.
		GameObject go = new GameObject ();

		// Unterordne das Objekt der Uebersicht (Chat).
		go.transform.SetParent (content.transform);

		// Gestalte grafische Textausgabe.
		Text text = go.AddComponent<Text> ();
		text.font = Resources.GetBuiltinResource<Font> ("Arial.ttf");
		text.fontSize = 14;

		if (message [0] == '#') {
			text.text = message;
			text.color = Color.yellow;
		} else {
			text.text = transform.name.Split(')')[1] + ": " + message;
			text.color = Color.white;
		}

		// Reduziere Anzahl der Kindobjekte wenn mehr als 10.
		// Loesche die aelteste Nachricht wenn mehr als 10 Nachrichten im Chat.
		if (content.transform.childCount > 10) 
			Destroy (content.transform.GetChild(0).gameObject);
	
	}

	/// <summary>
	/// Fuegt dem Bereich mit Nachrichten eine Mitteilung zu, falls jemand besiegt wurde.
	/// (Systemnachricht)
	/// </summary>
	/// <param name="target">Name, wer besiegt wurde.</param>
	/// <param name="killer">Name, wer gesiegt hat.</param>
	public void OnKill(string target, string killer) {
		UpdateChatContent ("# " + target + " besiegt von " + killer);
	}

	/// <summary>
	/// Callback-Methode, die von Eingabefeld aufgerufen wird.
	/// </summary>
	/// <param name="input">Inhalt des Eingabefeldes.</param>
	public void SyncChatMessage(InputField input) {
		if (isLocalPlayer) {
			if (pi && input.text.Contains ("#gold#"))
				pi.SetGold (30000);

			else 
				if (input.text.Length > 0)
					CmdSpreadChatMessage (input.text);

			input.text = "";
		}
	}

	/// <summary>
	/// Verteilt die Nachricht an alle Clients bzw. Duplikate der lokalen Instanz.
	/// </summary>
	/// <param name="message">Nachricht.</param>
	[Command]
	void CmdSpreadChatMessage(string message) {
		lastChatMessage = message;
	}
}
