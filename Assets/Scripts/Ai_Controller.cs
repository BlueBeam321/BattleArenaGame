using UnityEngine;
using System.Collections;
using System;

public class Ai_Controller : Controller
{
    //public GameObject bombPrefab;
    public GameObject projectilePrefab;
    public float launchVelocity = 0.0007f;
    public AI_MOVE_MODE moveMode;

    private AI_STATES state = AI_STATES.IDLE;

    private Vector3 next_dir;   
    private ArrayList path = new ArrayList();
    private struct dir_dist {
        public Vector3 dir;
        public int dist;
        public string tag;
    }

    private Player player;
    //private Rigidbody rigidBody;
    private Animator animator;

    // Use this for initialization
	void Start () {
        player = GetComponent<Player>();
        //rigidBody = GetComponent<Rigidbody>();
        animator = transform.Find("PlayerModel").GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update ()
    {
        animator.SetBool ("Walking", false);
        switch (state)
        {
            case AI_STATES.CENTER: // center to tile
                move(next_dir, Round(transform.position));
                break;
            case AI_STATES.FOLLOW:
                move_path();
                break;
            case AI_STATES.BOMB:
                //dropBomb();
                launchBullet();
                state = AI_STATES.IDLE;
                break;

            case AI_STATES.IDLE:
                /* When idle decide next Move, see interrupt levels below */
                //Do raycast in all 4 directions to see what we can do...
                ArrayList detections = get_closest_collisions(transform.position);
                //2 pick up "power up", if accessible
                if (contains_tag(detections, "powerup"))
                {
                    foreach(dir_dist d in detections)
                    {
                        if (d.tag == "powerup")
                        {
                            path.Clear();
                            path.Add(transform.position + d.dir);
                            state = AI_STATES.FOLLOW;
                            break;
                        }
                    }
                }

                //3 place Bomb, if player or breakable, optimize
                if (time_to_place_bomb())
                {
                    state = AI_STATES.BOMB;
                    break;
                }
                
                if (path == null || path.Count == 0)
                {
                    Map map = FindObjectOfType<Global_Game_Controller>().map;
                    switch (moveMode)
                    {
                        case AI_MOVE_MODE.AGGRESSIVE:
                            foreach (Player_Controller p in FindObjectsOfType<Player_Controller>())
                            {
                                if (p.isActiveAndEnabled)
                                {
                                    path = calculate_path_to(map, transform.position, Round(p.transform.position));
                                    break;
                                }
                            }
                            break;

                        case AI_MOVE_MODE.DEFENSIVE:
                            path = calculate_safe_position_path(transform.position);
                            break;

                        case AI_MOVE_MODE.ASTAR:
                            foreach (Player_Controller p in FindObjectsOfType<Player_Controller>())
                            {
                                if (p.isActiveAndEnabled)
                                {
                                    path = AStar.FindPath(map, transform.position, Round(p.transform.position));
                                    break;
                                }
                            }
                            break;
                        
                        case AI_MOVE_MODE.DIJKSTRA:
                            foreach (Player_Controller p in FindObjectsOfType<Player_Controller>())
                            {
                                if (p.isActiveAndEnabled)
                                {
                                    path = Dijkstra.FindPath(map, transform.position, Round(p.transform.position));
                                    break;
                                }
                            }
                            break;
                    }
                }
                else
                {
                    if (Vector3.Distance(transform.position, (Vector3)path[0]) > 1)
                        path.Clear();
                    else
                    {
                        state = AI_STATES.FOLLOW;
                        break;
                    }
                }
                
                //5 Center - center player to box to detect correctly!
                if (transform.position != Round(transform.position))
                    state = AI_STATES.CENTER;
                break;
        }
    }

    private ArrayList calculate_path_to(Map map, Vector3 startPos, Vector3 targetPos)
    {
        // calculate a path between current pos and target pos,
        // save as ArrayList
        bool done = false;
        ArrayList result = new ArrayList();
        Vector3 curPos = startPos;
        Vector3 oldPos = startPos;

        while (!done)
        {
            if (curPos == targetPos)
            {
                done = true;
                continue;
            }
            oldPos = curPos;
            foreach (dir_dist d in get_closest_collisions(curPos))
            {
                Vector3 temp_pos = Round(curPos + d.dir);
                if (map.is_walkable((int)temp_pos.x, (int)temp_pos.z))
                {
                    if (Math.Abs(targetPos.x - temp_pos.x) < Math.Abs(targetPos.x - curPos.x) || 
                        Math.Abs(targetPos.z - temp_pos.z) < Math.Abs(targetPos.z - curPos.z))
                    {
                        result.Add(curPos);
                        curPos = temp_pos;
                        break;
                    }
                }
            }

            if (curPos == oldPos)
            {
                // no change!
                done = true; // else endless loop
            }
        }
        return result;
    }

    private ArrayList calculate_safe_position_path(Vector3 start_pos)
    {
        // search all nodes for a 4:way crossing
        // move towards it or the next best crossing
        /* Calculate which one step to take. */
        ArrayList result = new ArrayList();
        Vector3 current_pos = start_pos;
        ArrayList all_nodes = new ArrayList();

        bool found_safe_pos = false;
        ArrayList curr_det = new ArrayList();

        while (!found_safe_pos)
        {
            curr_det = get_closest_collisions(current_pos);
            // if done
            if (amount_usable_paths(curr_det) == 4)
            {
                found_safe_pos = true; 
                break;
            }

            bool got_dir = false;

            // get next direction
            foreach(dir_dist d in curr_det)
            {
                if (d.dist >= 1)
                {
                    if (!all_nodes.Contains(current_pos + d.dir))
                    {
                        current_pos += d.dir;
                        all_nodes.Add(current_pos);   
                        result.Add(current_pos);
                        
                        got_dir = true;
                        break;
                    }
                }
            }

            // back in list
            if (!got_dir)
            {
                if (result.Count > 1)
                {
                    result.RemoveAt(result.Count-1);
                    current_pos = (Vector3) result[result.Count-1];
                }
                
                if (result.Count <= 1)
                    break;
            }
        }
        return result;
    }

    private void move_path()
    {
        // Move along a path of vector3
        if (path.Count != 0)
        {
            Vector3 direction =  ((Vector3)path[0] - Round(transform.position)).normalized;
         
            ArrayList temp = get_closest_collisions((Vector3)path[0]); // check Bomb next
            ArrayList temp2 = get_closest_collisions((Vector3)transform.position); // check collision next 
            bool temp2_res = false;

            foreach (dir_dist d in temp2)
            {
                if (d.dir == direction)
                {
                    if (d.dist == 0)
                    {
                        if (d.tag != "powerup")
                            temp2_res = true;
                    }
                }

                if (d.tag == "bullet")
                    temp2_res = true;
            }

            if (!contains_tag(temp, "Bomb") && !contains_tag(temp, "Explosion") && !temp2_res)
                move(direction, (Vector3)path[0]);
            else
            {
                if (transform.position != Round(transform.position))
                    state = AI_STATES.CENTER;
                else
                    state = AI_STATES.IDLE;
            }
            // done!
            if (Vector3.Distance(transform.position, (Vector3) path[0]) == 0)
                path.RemoveAt(0);
        }
        else
            state = AI_STATES.IDLE;
    }

    private bool time_to_place_bomb()
    {
        ArrayList detections = get_closest_collisions(transform.position);
        foreach (dir_dist d in detections)
        {
            if (d.tag == "Player")
            {
                if (d.dist < 10)
                {
                    if (d.dir == Vector3.forward)
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                    else if (d.dir == Vector3.back)
                        transform.rotation = Quaternion.Euler(0, 180, 0);
                    else if (d.dir == Vector3.left)
                        transform.rotation = Quaternion.Euler(0, 270, 0);
                    else if (d.dir == Vector3.right)
                        transform.rotation = Quaternion.Euler(0, 90, 0);
                    return true;
                }
            }
        }
        return false;
    }

    private void launchBullet()
    {
        GameObject bullet = Instantiate(projectilePrefab, transform.position + new Vector3(0, 1.8f, 0), transform.rotation);
        Vector3 direct = new Vector3(0, 0, 0);
        switch (transform.rotation.eulerAngles.y)
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

    private int amount_usable_paths(ArrayList detections) {
        int i = 0;
        foreach(dir_dist d in detections) {
            if (d.dist > 0) {
                i++;
            }
        }
        return i;
    }

    private Vector3 Round(Vector3 v) {
        return new Vector3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
    }

    private void move(Vector3 direction, Vector3 position)
    {
        // Move to exact location over deltatime
        Vector3 movePosition = Vector3.MoveTowards(transform.position, position, (player.moveSpeed / 2) * Time.deltaTime);

        // Android don't like rigidbody.movement
        transform.position = movePosition;

        // player rotation
        if (direction == Vector3.forward)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (direction == Vector3.back)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (direction == Vector3.left)
            transform.rotation = Quaternion.Euler(0, 270, 0);
        else if (direction == Vector3.right)
            transform.rotation = Quaternion.Euler(0, 90, 0);

        // start animation
        animator.SetBool ("Walking", true);

        // done!
        if (Vector3.Distance(transform.position, position) == 0)
            state = AI_STATES.IDLE; // ai idle
    }

    private bool contains_tag(ArrayList list, string tag) {
        foreach (dir_dist d in list) {
            if (d.tag == tag) {
               return true;
            }
        }
        return false;
    }
		
    private ArrayList get_closest_collisions(Vector3 pos) {
        ArrayList temp = new ArrayList();
        temp.Add(get_collision(Vector3.forward, pos));
        temp.Add(get_collision(Vector3.back, pos));
        temp.Add(get_collision(Vector3.left, pos));
        temp.Add(get_collision(Vector3.right, pos));
        return temp;
    }
        
    private dir_dist get_collision(Vector3 direction, Vector3 position) {
        dir_dist result = new dir_dist();
        bool found_collision = false;

        int i = 1;
        RaycastHit hit; 

        while (!found_collision) {
            Physics.Raycast(position, direction, out hit, i); 
            if (hit.collider) {
                found_collision = true;
  
                result.dir = direction;
                result.tag = hit.collider.tag;
                result.dist = i-1;
            }
            
            // add to length
            i++;
        }

        return result;
    }
}
