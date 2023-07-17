using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Global_Game_Controller : MonoBehaviour {
	private Text level_label;
	private Text enemy_label;
	private Text time_label;
	private float remainingTime;
	private bool gameRunning;

	public GameObject map_parent;
	public Map map;

	int myScore;
	List<int> enemyScore = new List<int>();

	// Use this for initialization
	void Start () {
		Application.targetFrameRate = 30;

		// init labels
		foreach (Text t in FindObjectsOfType<Text>())
		{
			switch (t.tag)
			{
			case "enemies":
				enemy_label = t;
				break;
			case "level":
				level_label = t;
				break;
			case "time":
				time_label = t;
				break;
			}
		}

		if (PlayerPrefs.GetInt("current_level").ToString().Length == 0)
			PlayerPrefs.SetInt("current_level", 1);
		
		int level = 1;//PlayerPrefs.GetInt("current_level");
		map =  gameObject.AddComponent<Map>();
		map.construct(1 + 3, 20, 20, map_parent);

		remainingTime = 5 * 60;
		gameRunning = true;
	}

	IEnumerator gameover_wait()
    {
        yield return new WaitForSeconds(1f);
        show_gameover_panel();
    }

	private void show_gameover_panel()
    {
        foreach (hide_on_start h in Resources.FindObjectsOfTypeAll<hide_on_start>())
        {
            if (h.tag == "gameover")
            {
                h.toggle_gameover(myScore, enemyScore);
                break;
            }
        }
    }

	void Update()
	{
		if (remainingTime < 0 && gameRunning)
		{
			foreach (Player a in FindObjectsOfType<Player>())
			{
				if (a.GetComponent<Player_Controller>().isActiveAndEnabled)
					myScore = a.died;
				else
					enemyScore.Add(a.died);
				Destroy(a.gameObject);
			}
			StartCoroutine(gameover_wait());
			gameRunning = false;
		}

		if (remainingTime >= 0 && gameRunning)
		{
			remainingTime -= Time.deltaTime;
			time_label.text = Mathf.RoundToInt(remainingTime).ToString() + " sec";
		}
	}

	public void update_labels() {
		int i = 0;
		foreach (Player a in FindObjectsOfType<Player>()) {
			if (a.isActiveAndEnabled)
				i++;
		}

		if (i <= 1) {
			if (FindObjectOfType<door_script>())
				Destroy(FindObjectOfType<door_script>().gameObject);
		}

		if (i > 0)
			i-= 1;

		enemy_label.text = (i).ToString();
		if (level_label)
			level_label.text = PlayerPrefs.GetInt("current_level").ToString();
	}

	public void Restart() {
		// get animation
		fade_script fade = new fade_script();
		// init fader
        foreach (fade_script f in FindObjectsOfType<fade_script>()) {
            if (f.tag == "fader")
               fade = f;
			else
               continue;
        }
		
		// reset values

		// load map
		if (Application.CanStreamedLevelBeLoaded("Game"))
			StartCoroutine(GameObject.FindObjectOfType<fade_script>().FadeAndLoadScene(fade_script.FadeDirection.In, "Game"));
		else
		 	StartCoroutine(GameObject.FindObjectOfType<fade_script>().FadeAndLoadScene(fade_script.FadeDirection.In, "Game_mobile"));
	}
}
