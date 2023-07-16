using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class startpos_script : MonoBehaviour {
	public bool player_controller = false;
	private GameObject player_prefab;
	// Use this for initialization
	void Start ()
	{
		if (!player_controller)
		{
		    player_prefab = (GameObject) Resources.Load("Enemy", typeof(GameObject));

			GameObject temp_prefab = Instantiate(player_prefab, transform.position, Quaternion.identity) ; // create new prefab instance
			temp_prefab.GetComponent<Player_Controller>().enabled = false; // disable Player Controller
			
			FindObjectOfType<Global_Game_Controller>().update_labels();
			temp_prefab.GetComponent<Ai_Controller>().moveMode = AI_MOVE_MODE.ASTAR;//(AI_MOVE_MODE)Random.Range(0, 4);
		}
		else
		{
			player_prefab = (GameObject) Resources.Load("Player", typeof(GameObject));

		 	GameObject temp_prefab = Instantiate(player_prefab, transform.position, Quaternion.identity) ; // create new prefab instance 
			temp_prefab.GetComponent<Ai_Controller>().enabled = false; // Disable AI Controller
			camera_follow_player cam = FindObjectOfType<camera_follow_player>();
			cam.player_controller = temp_prefab.GetComponent<Player_Controller>();
			cam.offset = new Vector3(-1,0,-4);
		}
	}
	
}
