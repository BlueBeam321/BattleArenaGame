
using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class Player_Controller : Controller
{
    private string FireAxis = "Fire 1";

    public bool canDropBombs = true;
    //Can the player drop bombs?
    public bool canMove = true;
    //Can the player move?

    public float launchVelocity = 0.0007f;
    //Prefabs
    //public GameObject bombPrefab;
    public GameObject projectilePrefab;
    
    //Cached components
    private Rigidbody rigidBody;
    private Transform myTransform;
    private Animator animator;
    private Player player;
    private bool mobile;
    private Transform caption;

    [SerializeField]
    private Text _Caption;

    [SerializeField]
    private Canvas _CaptionCanvas;

    [SerializeField]
    private Vector3 _TestVector;

    // Use this for initialization
    void Start ()
    {
	    if (Application.CanStreamedLevelBeLoaded("Game"))
            mobile = false;
        else
            mobile = true;

        player = GetComponent<Player>();
        //Cache the attached components for better performance and less typing
        rigidBody = GetComponent<Rigidbody>();
        myTransform = transform;
        animator = myTransform.Find("PlayerModel").GetComponent<Animator>();
    }

  
    public void UpdateCaption(string caption) {
        _Caption.GetComponent<Text>().text = caption;
    }

  // Update is called once per frame
    void Update ()
    {
        UpdateMovement ();
    }
    
    private void UpdateMovement ()
    {
        animator.SetBool ("Walking", false);
        //Depending on the player number, use different input for moving
        UpdatePlayer2Movement ();
    }

    /// <summary>
    /// Updates Player 2's movement and facing rotation using the arrow keys and drops bombs using Enter or Return
    /// </summary>
    private void UpdatePlayer2Movement ()
    {
        if (Input.GetButton("Up")) //Up movement
        {
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, player.moveSpeed);
            myTransform.rotation = Quaternion.Euler (0, 0, 0);
            animator.SetBool("Walking", true);
        }

        if (Input.GetButton("Left")) //Left movement
        {
            rigidBody.velocity = new Vector3(-player.moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
            myTransform.rotation = Quaternion.Euler(0, 270, 0);
            animator.SetBool("Walking", true);
        }

        if (Input.GetButton("Down")) //Down movement
        {
            rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, -player.moveSpeed);
            myTransform.rotation = Quaternion.Euler(0, 180, 0);
            animator.SetBool("Walking", true);
        }

        if (Input.GetButton("Right")) //Right movement
        {
            rigidBody.velocity = new Vector3(player.moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
            myTransform.rotation = Quaternion.Euler(0, 90, 0);
            animator.SetBool("Walking", true);
        }
        _CaptionCanvas.transform.eulerAngles =  new Vector3(0,0,0);

        if (mobile)
        {
            Vector3 vel = new Vector3 (Input.GetAxis("Horizontal")*player.moveSpeed, rigidBody.velocity.y, Input.GetAxis("Vertical")*player.moveSpeed);
            if (vel != rigidBody.velocity)
            {
                rigidBody.velocity = vel;
                myTransform.rotation = Quaternion.Euler(0, FindDegree(Input.GetAxis("Horizontal"),  Input.GetAxis("Vertical")), 0);
                animator.SetBool("Walking", true);
            }
        }
        

        // if (canDropBombs && (Input.GetKeyDown (KeyCode.KeypadEnter) || Input.GetKeyDown (KeyCode.Return) || Input.GetButtonDown("Submit")))
        // { //Drop Bomb. For Player 2's bombs, allow both the numeric enter as the return key or players without a NUMPAD will be unable to drop bombs
        //     dropBomb ();
        // }

        if (canDropBombs && (Input.GetKeyDown (KeyCode.KeypadEnter) || Input.GetKeyDown (KeyCode.Return) || Input.GetButtonDown("Submit")))
		{
            GameObject bullet = Instantiate(projectilePrefab, transform.position + new Vector3(0, 1.8f, 0), transform.rotation);
            Vector3 direct = new Vector3(0, 0, 0);       
            switch (myTransform.rotation.eulerAngles.y)
			{
                case 0:
                    direct = new Vector3(0, 0, 1);
                    break;
                case 90:
                    direct = new Vector3(1, 0, 0);
                    break;
                case 180:
                    direct = new Vector3(0, 0, -1);
                    break;
                case 270:
                    direct = new Vector3(-1, 0, 0);
                    break;
            }
            bullet.GetComponent<Projetile>().Set(transform.position + direct / 2.0f, direct, launchVelocity);
        }
    }

    public static float FindDegree(float x, float y)
	{
        float value = (float)((Mathf.Atan2(x, y) / Math.PI) * 180f);
        if (value < 0)
			value += 360f;
 
     	return value;
    }
	
    /*private void dropBomb ()
    {
        if (player.bombs != 0)
        {
           player.bombs--;
            if (bombPrefab)
            {
                //Check if bomb prefab is assigned first
                GameObject go = Instantiate(bombPrefab, 
                    new Vector3(Mathf.RoundToInt(myTransform.position.x), bombPrefab.transform.position.y, Mathf.RoundToInt(myTransform.position.z)),
                    bombPrefab.transform.rotation);

                go.GetComponent<Bomb>().explode_size = player.explosion_power;
                go.GetComponent<Bomb>().player = player;
                if (player.canKick)
                    go.GetComponent<Rigidbody>().isKinematic = false; // make bomb kickable
            }
        }
    }*/
}
