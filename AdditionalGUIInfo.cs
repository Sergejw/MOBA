using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Die Klasse repraesentiert die erngaenzenden Informationen, die auf der unteren 
/// grafischen Ebene zu sehen sind. Die Informationen werden ueber den Spielcharakteren angezeigt und 
/// aendern ihren Zustand. Zustand ist in dem Zusammenhang die Position und Lebenspunkte. Name aendert sich 
/// nur bei spielbaren Charakteren (Level).
/// </summary>
public class AdditionalGUIInfo : MonoBehaviour {

	/// <summary>Ergaenzenden Informationen als Objekt.</summary>
	private GameObject info;

	/// <summary>Lebensbalken.</summary>
	private Slider healthBar;

	/// <summary>Spielername.</summary>
	private Text name;

	/// <summary>Entfernung zum Spielcharakter.</summary>
	private float offsetLifePosition;

	/// <summary>
	/// Initialisiert die ergaenzenden Informationen.
	/// </summary>
	/// <param name="name">Spielername.</param>
	/// <param name="whoIam">Teamkennung. (false = Gegner)</param>
	/// <param name="offsetLifePosition">Entfernung zum Spielcharakter.</param>
	/// <param name="life">Lebenspunkte.</param>
	public void Init(string name, bool whoIam, float offsetLifePosition, float life) {

		// Setze Abstand zum Charakter
		this.offsetLifePosition = offsetLifePosition;

		// Erstelle ergaenzende Informationen als Objekt in dafuer vorgesehenen Ebene
		Transform layer = GameObject.Find ("Canvas").transform.FindChild ("info_layer");
		info = Instantiate (Resources.Load ("NPCAdditionalGUIInfo", typeof(GameObject))) as GameObject;
		info.transform.SetParent (layer.transform);

		// Initiere Spielernamen
		this.name = info.transform.Find ("NameBackground").transform.Find ("Name").GetComponent<Text> ();
		this.name.text = name;

		// Initiere Lebensbalken
		healthBar = info.transform.Find ("HealthBar").GetComponent<Slider> ();
		healthBar.maxValue = life;
		healthBar.value = healthBar.maxValue;

		// Wenn Charakter lokal ein Gegner, dann aendere Lebensbalkenfarbe
		if (!whoIam)
			SetHealthBarColor (new Color (1f, 0.3f, 0.3f, 1f)); // Rot
	}

	/// <summary>
	/// Aendert den Spielernamen, wenn dieser neuen Level erreicht.
	/// </summary>
	/// <param name="levelAndName">Level und Spielername.</param>
    public void SetLevel(string levelAndName) {
        name.text = levelAndName;
    }

	/// <summary>
	/// Veraendert die Lebensbalkenfarbe.
	/// </summary>
	/// <param name="color">Farbe, die Lebensbalken gesetzt bekommen soll.</param>
	/// 
	public void SetHealthBarColor(Color color) {
		healthBar.transform.FindChild ("Fill Area").transform.GetComponentInChildren<Image> ().color = color;
	}
		
	/// <summary>
	/// Passt die Position der Informationen an die Position des Spielcharakters an.
	/// </summary>
	void Update () {
		if (!info)
			return;

		Vector3 tempWorldToScreenPoint = Camera.main.WorldToScreenPoint (transform.position);
		tempWorldToScreenPoint.y += offsetLifePosition;

		info.transform.position = tempWorldToScreenPoint;

	}

	/// <summary>
	/// Liefert Lebenspunkte, die nach einem Angriff noch vorhanden sind.
	/// </summary>
	/// <param name="value">Schadenspunkte.</param>
	public float GetHealthBarValueOnDamage(float damageValue) {
		return healthBar.value -= damageValue;
	}

	/// <summary>
	/// Setzt lebenspunkte auf einen bestimmten Wert.
	/// </summary>
	/// <param name="value">Lebenspunkte als Wert.</param>
	public void SetHealthBarValue(float value) {
		healthBar.value = value;
	}

	/// <summary>
	/// Setzt den Lebensbalken auf maximalen Wert.
	/// </summary>
	public void ResetHealthBarValue() {
		healthBar.value = healthBar.maxValue;
	}

	/// <summary>
	/// Erhoeht die Lebenspunkte um einen Wert.
	/// </summary>
	/// <param name="value">Wert um den die Lebenspunkte erhoeht werden.</param>
	public void IncreaseMaxValueHealthBar(int value) {
		healthBar.maxValue += value;
	}

	/// <summary>
	/// Zerstoert die ergaenzenden Informationen, wenn die dazugehoerende Spielfigur nicht mehr 
	/// existiert.
	/// </summary>
	void OnDestroy() {
		if (info)
			Destroy (info);
	}

}
