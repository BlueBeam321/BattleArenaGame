﻿
using UnityEngine;
using System.Collections;
using System;

public class Ai_Controller : Controller
{
    public GameObject bombPrefab;
    public GameObject projectilePrefab;
    public float launchVelocity = 0.0007f;

    public bool drop_bomb_on_player;
    public AI_MOVE_MODE move_mode;

    private AI_STATES state = AI_STATES.IDLE;
    private bool dodge; // avoid bombs

    private bool on_bomb = false;
    private bool power_up; // Pick up "power up"
    private bool follow; // Follow other players to trap them

    private ArrayList wait_bombs = new ArrayList();
    private Vector3 next_pos;
    private Vector3 next_dir;
    private ArrayList path = new ArrayList();
    private struct dir_dist {
        public Vector3 dir;
        public int dist;
        public string tag;
        public int path_value;
    }

    private ArrayList detections;
    private Player player;
    private Rigidbody rigidBody;
    private Animator animator;

    // Use this for initialization
	void Start () {
        player = GetComponent<Player>();
        rigidBody = GetComponent<Rigidbody>();
        animator = transform.Find ("PlayerModel").GetComponent<Animator> ();

        // some interesting variables to shift
        follow = true;
        power_up = true;
        dodge = true;
	}
	
    private bool bombs_exist() {
        foreach (GameObject go in wait_bombs)
        {
            if (go != null)
                return true;
        }
        return false;
    }

    private bool old_bomb_collision(Vector3 pos_test) {
        // return true if colliding with an old Bomb!
        if (wait_bombs.Count > 0) {
            foreach (GameObject go in wait_bombs) {
                if (go != null) {
                    if (go.transform.position.x == pos_test.x || go.transform.position.z == pos_test.z)
                    return true;
                }
            }
        }
        return false;
    }

	// Update is called once per frame
	void Update ()
    {
        animator.SetBool ("Walking", false);

        switch (state)
        {
            case AI_STATES.DODGE: // Move away from Bomb
                move(next_dir, next_pos);
                break;
            case AI_STATES.CENTER: // center to tile
                move(next_dir, Round(transform.position));
                break;
            case AI_STATES.FOLLOW:
                move_path();
                break;
            case AI_STATES.BOMB:
                //dropBomb();
                launchBullet();
                on_bomb = true;
                player.bombs--;
                state = AI_STATES.IDLE;
                break;

            case AI_STATES.IDLE:
                /* When idle decide next Move, see interrupt levels below */
                //Do raycast in all 4 directions to see what we can do...
                detections = get_closest_collisions(transform.position);
                //1 avoid Bomb
                if (dodge)
                {
                    if (contains_tag(detections, "Bomb") || on_bomb) // Detect if standing on a Bomb instead
                    {
                        on_bomb = false;
                        calculate_next_dodge_pos(detections);
                        state = AI_STATES.DODGE;
                        break;
                    }
                    else
                    {
                        if (wait_bombs.Count > 0)
                        {
                            if (!bombs_exist())
                                wait_bombs.Clear();
                        }
                    }
                }
                
                //2 pick up "power up", if accessible
                if (power_up)
                {
                    if (contains_tag(detections, "powerup"))
                    {
                        foreach(dir_dist d in detections)
                        {
                            if (d.tag == "powerup")
                            {
                                path.Clear(); // currently removes the goal, maybe not the smartest idea
                                path.Add(transform.position + d.dir);
                                state = AI_STATES.FOLLOW;
                                break;
                            }
                        }
                    }
                }

                //3 place Bomb, if player or breakable, optimize
                if (player.bombs != 0)
                {
                    if (time_to_place_bomb())
                    {
                        state = AI_STATES.BOMB;    
                        break;
                    }
                }
                
                //4 Set a ai goal, a position the bot will try to reach
                if (follow)
                {
                    if (path.Count == 0)
                    {
                        // ready for next goal
                        switch (move_mode)
                        {
                            case AI_MOVE_MODE.AGGRESSIVE:
                                // find player
                                foreach (Player_Controller p in FindObjectsOfType<Player_Controller>())
                                {
                                    if (p.isActiveAndEnabled)
                                    {
                                        path = calculate_path_to(transform.position, Round(p.transform.position));
                                        break;
                                    }
                                }
                                break;

                            case AI_MOVE_MODE.RANDOM:               
                                path = calculate_path_to(transform.position, get_random_walkable_node_position());
                                break;

                            case AI_MOVE_MODE.FARM:
                                path  = calculate_next_closest_breakable(Round(transform.position), detections);
                                break;

                            case AI_MOVE_MODE.DEFENSIVE:
                                path = calculate_safe_position_path(transform.position);
                                break;
                        }
                    }
                    else
                    {
                        if (Vector3.Distance(transform.position, (Vector3)path[0]) > 1)
                        {
                            // this means we have dodged a Bomb and don't have a continuous path to goal
                            // we should update path, but i will clear for now
                            path.Clear();
                        }
                        else
                        {
                            state = AI_STATES.FOLLOW;
                            break;
                        }
                    }             
                }
                
                //5 Center - center player to box to detect correctly!
                if (transform.position != Round(transform.position))
                    state = AI_STATES.CENTER;
                break;
        }
    }

    private ArrayList calculate_next_closest_breakable(Vector3 position, ArrayList det)
    {
        /* Create an ArrayList of size =1 that shows way to a breakable */
        ArrayList p = new ArrayList();
        dir_dist next = new dir_dist();
        bool next_set = false;
        ArrayList t = det;
        // next = (dir_dist) t[UnityEngine.Random.Range(0, 3)];
        foreach (dir_dist d in t)
        {
            if (d.tag == "Breakable")
            {
                if (d.dist < next.dist)
                {
                    next = d;
                    next_set = true;
                }
            } 
        }

        if (next_set)
            p.Add(position + Round(next.dir));
        else
        {
            foreach (dir_dist d in t)
            {
                if (d.dist >= 1)
                {
                    next = d;
                    next_set = true;
                }
            }
            p.Add(position + Round(next.dir));
        }
        return p;
    }

    private ArrayList calculate_path_to(Vector3 startPos, Vector3 goal)
    {
        // calculate a path between current pos and goal,
        // save as ArrayList
        bool done = false;
        ArrayList result = new ArrayList();
        Vector3 curPos = startPos;
        Map map = FindObjectOfType<Global_Game_Controller>().map;
        Vector3 oldPos = startPos;

        while (!done)
        {
            if (curPos == goal)
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
                    if (Math.Abs(goal.x - temp_pos.x) < Math.Abs(goal.x - curPos.x) || 
                        Math.Abs(goal.z - temp_pos.z) < Math.Abs(goal.z - curPos.z))
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

    private Vector3 get_random_walkable_node_position()
    {
        Vector3 res = new Vector3();
        bool found = false;
        Map m = FindObjectOfType<Global_Game_Controller>().map;

        while (!found)
        {
            Vector3 test = new Vector3(UnityEngine.Random.Range(1, m.width-1), 0,UnityEngine.Random.Range(1,m.height-1));
            if (m.is_walkable((int)test.x, (int)test.z)) {
                found = true;
                res = test;
            }
        }
       
        return res;
    }

    private ArrayList calculate_safe_position_path(Vector3 start_pos) {
        // search all nodes for a 4:way crossing
        // move towards it or the next best crossing
        /* Calculate which one step to take. */
    
        ArrayList result = new ArrayList();
        Vector3 current_pos = start_pos;
        ArrayList all_nodes = new ArrayList();

        bool found_safe_pos = false;
        ArrayList curr_det = new ArrayList();

        while(!found_safe_pos) {
            curr_det = get_closest_collisions(current_pos);
            // if done
            if (amount_usable_paths(curr_det) == 4) {
                found_safe_pos = true; 
                break;
            }

            bool got_dir = false;

            // get next direction
            foreach(dir_dist d in curr_det) {
                if (d.dist >= 1 ) {
                    if (!all_nodes.Contains(current_pos + d.dir)) {
                        current_pos += d.dir;
                        all_nodes.Add(current_pos);   
                        result.Add(current_pos);
                        
                        got_dir = true;
                        break;
                    }
                }
            }

            // back in list
            if (!got_dir) {
                if (result.Count > 1) {
                    result.RemoveAt(result.Count-1);
                    current_pos = (Vector3) result[result.Count-1];
                }
                
                if (result.Count <= 1) {
                    break; 
                }
            }
        }
        return result;
    }

    private void move_path()
    {
        // move along a path of vector3
        // always do checks for bombs on the way and avoid
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

    private void bomb_check() {
        if (dodge)
        {
            detections = get_closest_collisions(transform.position);
            if (contains_tag(detections, "Bomb") || on_bomb) // Detect if standing on a Bomb instead
            {
                on_bomb = false;
                calculate_next_dodge_pos(detections);
                state = AI_STATES.DODGE;
            }
        }
    }

    private ArrayList calculate_escape_path(Vector3 bomb_pos, Vector3 start_pos) {
        /* Calculate which one step to take. */
        ArrayList result = new ArrayList();
        Vector3 current_pos = start_pos;
        ArrayList all_nodes = new ArrayList();

        bool found_safe_pos = false;
        ArrayList curr_det = new ArrayList();

        while (!found_safe_pos) {
            // if done
            if (bomb_pos.x != current_pos.x && bomb_pos.z != current_pos.z) {
                found_safe_pos = true; 
                break;
            }

            curr_det = get_closest_collisions(current_pos);
            bool got_dir = false;
            // get next direction
            foreach(dir_dist d in curr_det) {
                if (d.dist >= 1  && !old_bomb_collision(current_pos+d.dir)) {
                    if (!all_nodes.Contains(current_pos + d.dir)) {
                        current_pos += d.dir;
                        all_nodes.Add(current_pos);   
                        result.Add(current_pos);
                        
                        got_dir = true;
                        break;
                    }
                }
            }

            // back in list
            if (!got_dir) {
                if (result.Count > 1) {
                    result.RemoveAt(result.Count-1);
                    current_pos = (Vector3) result[result.Count-1];
                }
                if (result.Count <= 1) {
                    break; 
                }
            }
        }

        return result;
    }
 
    private bool time_to_place_bomb()
    {
        detections = get_closest_collisions(transform.position);
        foreach(dir_dist d in detections)
        {
           if (d.tag == "Breakable" || (d.tag == "Player" && drop_bomb_on_player))
           {
                if (d.dist < player.explosion_power)
                {
                    ArrayList temp = calculate_escape_path(transform.position, transform.position);
                    if (temp.Count >= 2)
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
        Debug.Log("Bot: Launching bullet");
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
        bullet.GetComponent<Projetile>().Set(transform.position, direct, launchVelocity);
    }

    private void dropBomb() {
        if (bombPrefab) { //Check if Bomb prefab is assigned first
            GameObject go = Instantiate(bombPrefab, 
                new Vector3(Mathf.RoundToInt(transform.position.x), bombPrefab.transform.position.y, Mathf.RoundToInt(transform.position.z)),
                bombPrefab.transform.rotation);

            wait_bombs.Add(go);

            go.GetComponent<Bomb>().explode_size = player.explosion_power;
            go.GetComponent<Bomb>().player = player;
        }
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
        return new Vector3(Mathf.RoundToInt(v.x),Mathf.RoundToInt(v.y),Mathf.RoundToInt(v.z));
    }

    private void calculate_next_dodge_pos(ArrayList curr_det) {
        /* Calculate which one step to take */
        dir_dist temp = new dir_dist();
        ArrayList det_array = new ArrayList();

        bool got_pos = false;
        int counter = 0;
   
        while (!got_pos) {
            if (counter > 4) {
                got_pos = true;
                continue;
            }
            counter++;    

            // get position with highest potential    
            foreach (dir_dist d in curr_det) {
                if (d.path_value > temp.path_value) {
                    temp = d;
                }
            }
            next_pos = transform.position + temp.dir;
            next_dir = temp.dir;
            // check if Bomb
            det_array = get_closest_collisions(next_pos);
            if (contains_tag(det_array, "Bomb") ) {
                // update current list
                ArrayList new_array = new ArrayList();
                dir_dist t = new dir_dist();
                foreach (dir_dist d in curr_det) {
                    if (d.dir == next_dir) {
                        t = d;
                        t.path_value = 1/2;
                    }
                    else {
                        t = d;
                    }
                    new_array.Add(t);
                }
                curr_det = new_array;
            }
            else {
                got_pos = true;
                // next_pos & next_dir -- updated 
            }
        }
    }

    private void move(Vector3 direction, Vector3 position)
    {
        // Move to exact location over deltatime
        Vector3 movePosition = Vector3.MoveTowards(transform.position, position, (player.moveSpeed / 2) * Time.deltaTime);

        // Android don't like rigidbody.movement
        transform.position = movePosition;

        // player rotation
        if (direction == Vector3.forward) {
            transform.rotation = Quaternion.Euler (0, 0, 0);
        }
        else if (direction == Vector3.back) {
            transform.rotation = Quaternion.Euler (0, 180, 0);
        }
        else if (direction == Vector3.left) {
            transform.rotation = Quaternion.Euler (0, 270, 0);
        }
        else if (direction == Vector3.right) {
            transform.rotation = Quaternion.Euler (0, 90, 0);
        }

        // start animation
        animator.SetBool ("Walking", true);

        // done!
        if (Vector3.Distance(transform.position, position) == 0) {
            state = AI_STATES.IDLE; // ai idle 
        }
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
                result.path_value = result.dist;
                
                if (result.tag == "Bomb") {
                    if (result.dist > 0) {
                        result.path_value =  1/2;
                        if (!wait_bombs.Contains(hit.collider.gameObject))
                            wait_bombs.Add(hit.collider.gameObject);
                    }
                }
            }
             
            // add to length
            i++;
        }

        return result;
    }
}
