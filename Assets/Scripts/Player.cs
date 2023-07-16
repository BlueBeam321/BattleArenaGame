using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Text life_label;
    private Text died_label;

    public GlobalStateManager globalManager;

    public float moveSpeed = 5f;
    
    public ParticleSystem Explosion;

    public int health = 100;

    public int died = 0;
    //public bool respawning = false;

    private fade_script fade;

    public void update_label(POWERUPS powerup)
    {
        switch (powerup)
        {
            case POWERUPS.LIFE:
                life_label.text = health.ToString();
                break;

            case POWERUPS.DIED:
                died_label.text = died.ToString();
                break;
        }
    }

    /*IEnumerator respawn_wait()
    {
        yield return new WaitForSeconds(5);
        respawning = false;
    }*/

    IEnumerator gameover_wait()
    {
        yield return new WaitForSeconds(1f);
        show_gameover_panel();
    }

    // Use this for initialization
    void Start ()
    {
        // init fader
        foreach(fade_script f in FindObjectsOfType<fade_script>())
        {
            if (f.tag == "fader")
                continue;
            else
                fade = f;
        }

        // init labels
        if (GetComponent<Player_Controller>().isActiveAndEnabled)
        {
            foreach (Text t in FindObjectsOfType<Text>())
            {
                switch (t.tag)
                {
                    case "life":
                        life_label = t;
                        break;
                    case "died":
                        died_label = t;
                        break;
                }
            }
        }
        //Cache the attached components for better performance and less typing
    }

    IEnumerator dmg_animation(){
        StartCoroutine(fade.FadeOnly(fade_script.FadeDirection.In));
        yield return new WaitForSeconds(1);
        StartCoroutine(fade.FadeOnly(fade_script.FadeDirection.Out));
    
        yield return new WaitForSeconds(1);
        StartCoroutine(fade.FadeOnly(fade_script.FadeDirection.In));
    
        yield return new WaitForSeconds(1);
        StartCoroutine(fade.FadeOnly(fade_script.FadeDirection.Out));
    }

    private void show_gameover_panel()
    {
        // foreach (hide_on_start h in Resources.FindObjectsOfTypeAll<hide_on_start>())
        // {
        //     if (h.tag == "gameover")
        //     {
        //         h.toggle();
        //         break;
        //     }
        // }
        // Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag ("bullet"))
        {
            GetComponent<Player_Controller>().UpdateCaption("HP : " + health.ToString());
            health = health - 20;
            if (health < 0)
                health = 0;
            
            //Debug.Log(tag + " collision : " +  collision.collider.tag + health);
            if (GetComponent<Player_Controller>().isActiveAndEnabled)
                update_label(POWERUPS.LIFE);

            if (health <= 0)
			{
                bool enemy = true;
                if (GetComponent<Player_Controller>().isActiveAndEnabled)
				{
                    enemy = false;
                    died++;
                    update_label(POWERUPS.DIED);
                    // StartCoroutine(gameover_wait());
                    StartCoroutine(dmg_animation());
                }
				/*else
				{
                    Destroy(gameObject);
                    FindObjectOfType<Global_Game_Controller>().update_labels();
                }*/

                health = 100;
                Destroy(gameObject);
                FindObjectOfType<Global_Game_Controller>().GetComponent<Map>().generate_player(enemy);
                
                //respawning = true;
                //StartCoroutine(respawn_wait());
                Instantiate(Explosion, transform.position, Quaternion.identity);
            }
        }
    }
}
