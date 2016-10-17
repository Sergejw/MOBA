using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

/// <summary>
/// Die Klasse repraesentiert die Steuerung der spielbaren Chraktere. Diese besteht aus Navigation, 
/// Kamera und Kampf. Die Komponente wird fuer die lokale Instanz aktiviert. Duplikat benoetigen 
/// diese nicht. Der Anteil der Kamera setzt sich aus Benutzereingaben und Kameraobjekt zusammen. 
/// Der Anteil der Navigation setzt sich aus Benutzereingaben und NevMeshAgent. Der Anteil des Kampfes 
/// setzt sich aus Benutzereingaben und Kommunikation mit dem Server.
/// </summary>
public class PlayerController : NetworkBehaviour {

	/// <summary> Objekt, womit der Charakter navigiert.</summary>
	NavMeshAgent agent;

	/// <summary> Objekt, womit der Charakteranimationen abgespielt werden.</summary>
	private Animator animator;

	/// <summary>Angriffsradius.</summary>
	private float range = 2;

	/// <summary> Grundschaden.</summary>
	private int dmg = 10;

	/// <summary> Angriffsgeschwindigkeit.</summary>
	private float attackSpeed = 2;

	/// <summary> Chat.</summary>
	private Chat chat;

	/// <summary> Kamera.</summary>
	Camera camera;

	/// <summary> Abstand der Kamera zum Charakter, wenn die Kamera den Charakter verfolgt.</summary>
	Vector3 plus = new Vector3(0, 10, -7);

	/// <summary> Ziel.</summary>
	private Transform target;

	/// <summary>Id des Zeils.</summary>
	private uint targetNetId;

	/// <summary> Startposition.</summary>
	private Vector3 spawn;

	/// <summary>Wahrheitswert, ob Kamera den Charakter verfolgen soll.</summary>
	bool follow = true;

	/// <summary>Abstand am linken Bildschirmrand fuer die freie Kamerabewegung. 
	/// Ist der Mauscursor in diesem Bereich, wird die Kamera nach links navigiert.</summary>
	Vector3 xMove = new Vector3(20f, 0, 0);

	/// <summary>Abstand am unteren Bildschirmrand fuer die freie Kamerabewegung. 
	/// Ist der Mauscursor in diesem Bereich, wird die Kamera nach unten navigiert.</summary>
	Vector3 yMove = new Vector3(0, 0, 20f);

	/// <summary>Abstand am rechten Bildschirmrand fuer die freie Kamerabewegung. 
	/// Ist der Mauscursor in diesem Bereich, wird die Kamera nach rechts navigiert.</summary>
	int xMargin = (Screen.width - 10);

	/// <summary>Abstand am oberen Bildschirmrand fuer die freie Kamerabewegung. 
	/// Ist der Mauscursor in diesem Bereich, wird die Kamera nach oben navigiert.</summary>
	int yMargin = (Screen.height - 10);

	/// <summary>
	/// Initialisiert die Komponente.
	/// </summary>
	public override void OnStartLocalPlayer() {
		NetworkManager.singleton.client.RegisterHandler (8989, OnKill);

		// Navigation
		agent = GetComponent<NavMeshAgent> ();

		// Kamera
		camera = GameObject.Find ("Main Camera").GetComponent<Camera> ();

		// Animation
		animator = GetComponent<Animator> ();

		// Chat
		chat = GetComponentInParent<Chat> ();
	}
		

	/// <summary>
	/// Handler fuer Netzwerkmitteilung, wenn jemand besiegt wurde.
	/// </summary>
	/// <param name="msg">Mitteilung mit Namen von Sieger und besiegten.</param>
	private void OnKill(NetworkMessage message) {

		OnKillNetworkMessage okm = message.ReadMessage<OnKillNetworkMessage> ();

		// Schreibe in den Chat als Systemmeldung
		chat.OnKill (okm.targetName.Split(')')[1], okm.killedByName);

		// Respawn, wenn selber gestorben
		if (okm.targetNetId == netId.Value)
			Respawn ();
		
		// Wurde mein Angriffsziel besiegt von mir oder jemand anderem, 
		// dann resete mein Ziel.
		else if (target && okm.targetNetId == targetNetId) 
			ResetTarget ();

		// Wurde eine Basis zerstoert?
		if (okm.targetName.Contains ("base")) {

			// Zeige Meldung, ob gewonnen oder verloren
			if (!GameObject.Find (okm.targetName).tag.Contains (transform.tag)) {
				GameObject go = Instantiate (Resources.Load ("u_win", typeof(GameObject))) as GameObject;
				go.transform.SetParent (GameObject.Find("Canvas").transform);
			} else {
				GameObject go = Instantiate (Resources.Load ("u_lose", typeof(GameObject))) as GameObject;
				go.transform.SetParent (GameObject.Find("Canvas").transform);
			}

			// Beende das Match
			if (isServer && isLocalPlayer) {
				GameObject.Find ("NetworkManager").GetComponent<Manager> ().gameOver = true;
				GameObject.Find ("NPCManager").GetComponent<NPCManager> ().Stop ();
			}
				
		}
	}


	/// <summary>
	/// Entfernt das erfasste Angriffsziel.
	/// </summary>
	private void ResetTarget() {
		target = null;

		// Setze Animation auf Idle.
		animator.SetBool ("hasPath", false);
		animator.SetBool ("attack", false);

		// Unterbreche Angriff, falls gerade aktiv
		if (IsInvoking("AttackTarget"))
			CancelInvoke ("AttackTarget");
	}

	/// <summary>
	/// Setzt die Startposition.
	/// </summary>
	/// <param name="value">Startposition.</param>
	public void SetSpawnPosition(Vector3 value) {
		spawn = value;
		transform.position = value;
		GetComponentInParent<NavMeshAgent> ().enabled = true;
	}

	/// <summary>
	/// Vesetzt den Charakter nach dem Sterben in die Basis und setzt die Werte neu.
	/// </summary>
	private void Respawn() {
		
		if (isLocalPlayer) {

			// Deaktiviere Navigation
			GetComponentInParent<NavMeshAgent> ().enabled = false;
			transform.position = spawn;

			// Entferne Ziel
			target = null;

			if (agent.isActiveAndEnabled) {
				agent.ResetPath ();

				// Beende animation
				GetComponent<Animator> ().SetBool ("hasPath", false);
			}

			// Aktiviere Navigation
			GetComponentInParent<NavMeshAgent> ().enabled = true;
			ResetTarget ();
		}

	}

	/// <summary>
	/// Setzt den Schaden, den der Charakter austeilt.
	/// </summary>
	/// <param name="damage">Schadenspunkte.</param>
	public void SetDamage(int damage) {
		dmg = damage;
	}

	/// <summary>
	/// Liefert erhoehte Schadenspunkte, die der Charakter austeilt.
	/// </summary>
	/// <returns>Neue Schadenspunkte.</returns>
	/// <param name="value">Wert, um den die Schadenspunkte erhoeht werden.</param>
	public int IncreaseDamage(int value) {
		return dmg += value;
	}

	/// <summary>
	/// Liefert die Schadenspunkte.
	/// </summary>
	/// <returns>Schadenspunkte.</returns>
	public int GetDamage() {
		return dmg;
	}

	/// <summary>
	/// Steuert den Charakter.
	/// </summary>
	void Update() {
		if (isLocalPlayer) {

			// Gibt es ein Ziel und dieses ist in Reichweite
			if (target != null && 
				Vector3.Distance (transform.position, target.position) <= range &&
				!IsInvoking("AttackTarget")) {

				// Stoppe Nvigation
				agent.destination = transform.position;

				// Greife an
				animator.SetBool ("hasPath", false);
				animator.SetBool ("attack", true);
				InvokeRepeating("AttackTarget", 0, attackSpeed);

			// Ziel nicht in Reichweite
			} else if (target != null && 
				Vector3.Distance (transform.position, target.position) > range && 
				IsInvoking("AttackTarget")) {

				// Navigiere zum Ziel
				agent.destination = target.position;
				animator.SetBool ("attack", false);
			}
			
			animator.SetBool ("hasPath", agent.hasPath);

			// Mausklick fuer Navigation oder Angriff
			if (Input.GetMouseButtonDown (0) && (!EventSystem.current.IsPointerOverGameObject() || EventSystem.current.transform.name.Contains("NPC"))) {
				RaycastHit hit;
				GameObject spieler = null;

				if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, 100)) {
					// Navigiere zur Position
					agent.destination = hit.point;
					animator.SetBool ("attack", false);

					// Einen Gegner angeklickt
					if (!hit.collider.tag.Contains (transform.tag) && (hit.collider.tag.Equals("0") || hit.collider.tag.Equals("1"))) {
						// Setze Angriffsziel
						target = hit.transform;
						targetNetId = target.GetComponent<NetworkIdentity>().netId.Value;

					// Breche Angriff auf Grund von Navigation ab
					} else if (target != null) {
						target = null;
						CancelInvoke ("AttackTarget");
					}

				}
			}

			// Starte aufladbare Faehigkeit
			if (Input.GetKeyDown ("q")) {
				if (GetComponentInParent<PlayerSkillsGUI> ().Trigger(0))
					CmdShoot(gameObject, 0); 
			}
		}
	}

	/// <summary>
	/// Teile den Clients mit, dass die aufladbare Faehigkeit gestartet werden soll.
	/// </summary>
	/// <param name="go">Charakterobjekt.</param>
	/// <param name="i">Index der Faehigkeit.</param>
	[Command]
	void CmdShoot(GameObject go, int i) {
		go.GetComponent<PlayerSkills>().RpcTrigger(go.name, i);
	}
		
	/// <summary>
	///	Steuert die Kamera.
	/// </summary>
	[Client]
	void LateUpdate() {

		if (!isLocalPlayer)
			return;

		// Kamera folge dem Spielcharakter
		if (follow)
			camera.transform.position = Vector3.Lerp (camera.transform.position, transform.position + plus, Time.deltaTime * 10f);

		else {
			// Bewege Kamera nach links
			if (Input.mousePosition.x < 10 && camera.transform.position.x >= 0) 
				camera.transform.position -= xMove * Time.deltaTime;

			// Bewege Kamera nach rechts
			else if (Input.mousePosition.x > xMargin && camera.transform.position.x <= 40)
				camera.transform.position += xMove * Time.deltaTime;

			// Bewege Kamera nach oben
			if (Input.mousePosition.y < 10 && camera.transform.position.z >= 0)
				camera.transform.position -= yMove * Time.deltaTime;

			// Bewege Kamera nach unten
			else if (Input.mousePosition.y > yMargin  && camera.transform.position.z <= 36)
				camera.transform.position += yMove * Time.deltaTime;
		}

		// Fixiere Kamera auf Spielcharakter oder gebe sie frei.
		if (Input.GetKeyDown ("b")) 
			follow = (follow) ? false : true;
	}
		


	/// <summary>
	/// Attakiert das ausgewaehlte Ziel in Intervallen.
	/// </summary>
	private void AttackTarget() {
		if (target == null)
			return;
		
		// Verschiedene Funktionen, weil Objekte in verschiedenen Instanzen
		// Ein NPC?
		if (target.GetComponentInParent<NPCDamageReciever>()) 
			CmdFindDam (target.name, dmg, transform.name, target.GetComponentInParent<NetworkIdentity>().netId.Value);
		
		// Oder Player
		else 
			CmdDam (target.gameObject, dmg, transform.name, target.GetComponentInParent<NetworkIdentity>().netId.Value);

		// Erhoehe Erfahrung
		GetComponentInParent<PlayerConfig> ().IncreaseExperience (1);
	}

	/// <summary>
	/// Leite Schaden an NPC weiter.
	/// </summary>
	/// <param name="name">Name des Angegriffenen.</param>
	/// <param name="dmg">Schaden.</param>
	/// <param name="attacker">Angreifername.</param>
	/// <param name="attackerId">Id des Angreifers.</param>
	[Command]
	void CmdFindDam(string name, int dmg, string attacker, uint attackerId) {
		GameObject go = GameObject.Find (name);

		if (go)
			go.GetComponent<NPCDamageReciever> ().TakeDamage (dmg, attacker, attackerId);
	}

	/// <summary>
	///  Leite Schaden an Spieler weiter.
	/// </summary>
	/// <param name="go">Charakterobjekt.</param>
	/// <param name="dmg">Schaden.</param>
	/// <param name="attacker">Angreifername.</param>
	/// <param name="attackerId">Id des Angreifers.</param>
	[Command]
	void CmdDam(GameObject go, int dmg, string attacker, uint attackerId) {
		if (go)
			go.GetComponent<PlayerDamageReciever> ().RpcTakeDamage (dmg, attacker, attackerId);
	}
}