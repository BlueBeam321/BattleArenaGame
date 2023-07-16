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

    public int lifes = 100;
    public bool dead = false;

    public int killed = 0;
    public bool respawning = false;

    private fade_script fade;

    public void update_label(POWERUPS powerup)
    {
        switch (powerup)
        {
            case POWERUPS.LIFE:
                life_label.text = lifes.ToString();
                break;

            case POWERUPS.DIED:
                died_label.text = killed.ToString();
                break;
        }
    }

    IEnumerator respawn_wait()
    {
        yield return new WaitForSeconds(5);
        respawning = false;
    }

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
            GetComponent<Player_Controller>().UpdateCaption("HP : " + lifes.ToString());
            lifes = lifes - 20;
            if (lifes < 0)
                lifes = 0;
            
            //Debug.Log(tag + " collision : " +  collision.collider.tag + lifes);
            if (GetComponent<Player_Controller>().isActiveAndEnabled)
                update_label(POWERUPS.LIFE);

            if (lifes <= 0)
			{
                dead = true; // 1
                if (GetComponent<Player_Controller>().isActiveAndEnabled)
				{     
                    killed ++;
                    update_label(POWERUPS.DIED);
                    // StartCoroutine(gameover_wait());
                    StartCoroutine(dmg_animation());
                }
				else
				{
                    // Destroy(gameObject);
                    FindObjectOfType<Global_Game_Controller>().update_labels();
                }
                
                respawning = true;
                lifes = 100;
                StartCoroutine(respawn_wait());
                // Instantiate(Explosion, transform.position, Quaternion.identity);
            }
        }
    }
}
