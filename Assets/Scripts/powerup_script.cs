using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerup_script : MonoBehaviour
{
	public GameObject life;

	private GameObject text;
	private GameObject curr;

	public POWERUPS powerup;

	// Use this for initialization
	void Start ()
	{
		powerup = (POWERUPS)Random.Range(0, 6);

		// load prefab look
		switch (powerup)
		{
			case POWERUPS.LIFE:
				curr = life;
				break;
		}
			// curr position
		GameObject go =	Instantiate(curr, transform.position, Quaternion.identity) as GameObject;
		go.GetComponent<Transform>().SetParent(this.transform);

		text = Resources.Load("PopupTextParent") as GameObject;
	}
	
	void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            Destroy(gameObject); // 3  
			Player player = collider.GetComponent<Player>();

			string s = "";
			switch (powerup)
			{
				case POWERUPS.LIFE:
					player.health++;
					s = "+1 Life";
					break;
			}
		
			if (player.GetComponent<Player_Controller>().isActiveAndEnabled) // if human controlled
				player.update_label(powerup);

			GameObject go = Instantiate(text, collider.transform.position, Quaternion.identity) as GameObject;
			go.GetComponent<FloatingText>().setText(s, new Color());

			foreach(Canvas c in  FindObjectsOfType<Canvas>())
			{
				if (c.tag == "world_canvas")
					go.transform.SetParent(c.transform);
			}
        }
    }
}
