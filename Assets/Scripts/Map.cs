using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
    This file contains the map class
    This class creates and handles a map instance
 */
public class Map : MonoBehaviour {

    // These variables most be added in editor!

    public int width;
    public int height;

    private GameObject wall_prefab;
    private GameObject breakable_prefab;
    private GameObject startpos_prefab;
    private GameObject floor_prefab;
    private GameObject goal_prefab;
    private GameObject door_prefab;
    private GameObject powerup_prefab;
    private GameObject Map_parent;

    private int start_poses;

    private Blocks[,] array_representation;

    public void construct(int _start_poses, int x, int y, GameObject parent) {
        // init a map
        
        // load  variables
        start_poses = _start_poses;
        Map_parent = parent;
        
        startpos_prefab = (GameObject) Resources.Load("Startpos",typeof(GameObject));
        breakable_prefab = (GameObject) Resources.Load("Breakable", typeof(GameObject));
        door_prefab = (GameObject) Resources.Load("Door",typeof(GameObject));
        goal_prefab = (GameObject) Resources.Load("Goal",typeof(GameObject));
        floor_prefab = (GameObject) Resources.Load("Floor",typeof(GameObject));
        wall_prefab = (GameObject) Resources.Load("Wall",typeof(GameObject));
        
        // create map
        //minimum values are 2x2 map due to calculations below
        // a clamp if you will ;)
        if (x <= 2)
            x = 2;
        
        if (y <= 2)
            y = 2;

        if (_start_poses >= 7)
            start_poses = 7;

        width = x;
        height = y;

        array_representation = new Blocks[x, y];
        create_map(x, y);
    }

    public bool is_walkable(int x, int y) {
        return array_representation[x, y] != Blocks.Wall;
    }

    private void create_map(int x_count, int y_count) {
        /* Yet again ugly code but fast working for now */
        // player start pos
        GameObject t = new_instance(1, 0, y_count / 2 +  (y_count % 2), startpos_prefab);
        t.GetComponent<startpos_script>().player_controller = true;
        array_representation[1, y_count / 2] = Blocks.Startpos;

        if (start_poses > 1) 
        {
            new_instance(1, 0, y_count - 2 , startpos_prefab);
            array_representation[1, y_count - 2] = Blocks.Startpos;
        }

        if (start_poses > 2) {
            new_instance(1, 0, 1, startpos_prefab);
            array_representation[1, 1] = Blocks.Startpos;
        }

        if (start_poses > 3) {
            new_instance(x_count-2, 0, y_count - 2, startpos_prefab);
            array_representation[x_count - 2, y_count - 2] = Blocks.Startpos;
        }

        if (start_poses > 4) {
            new_instance(x_count - 2, 0, 1, startpos_prefab);
            array_representation[x_count - 2, 1] = Blocks.Startpos;
        }

        if (start_poses > 5) {
            new_instance(x_count / 2, 0, 1, startpos_prefab);
            array_representation[x_count / 2, 1] = Blocks.Startpos;
        }

        if (start_poses > 6) {
            new_instance(x_count / 2, 0, y_count - 2, startpos_prefab);
            array_representation[x_count / 2, y_count - 2] = Blocks.Startpos;
        }

        if (start_poses > 7) {
            new_instance(x_count - 2, 0, y_count / 2, startpos_prefab);
            array_representation[x_count - 2, y_count / 2] = Blocks.Startpos;
        }

        for (int x = 0; x < x_count; x++) {
            for (int y = 0; y < y_count; y++) {
                // Create floor
                new_instance(x, -1, y, floor_prefab);
                
                // Create all wall limits 
                if (x == 0 || x == x_count - 1 || y == 0 || y == y_count - 1) {
                    if (y == y_count / 2 && x == x_count - 1) {
                        array_representation[x, y] = Blocks.Door;
                        new_instance(x, 0, y, door_prefab);
                        new_instance(x + 1, 0, y, goal_prefab);
                    }
                    else {
                        array_representation[x, y] = Blocks.Wall;
                        new_instance(x, 0, y, wall_prefab);
                    }
                }
                /*else {
                    // add wall in center of map (random later on!)
                    if (x % 2 == 0 && y % 2 == 0 && x != x_count - 2 && y != y_count - 2) {
                        array_representation[x, y] = Blocks.Wall;
                        new_instance(x, 0, y, wall_prefab);
                    }
                    else {
                        // add breakables
                        if (!start_next_to(x, y)) {
                            array_representation[x, y] = Blocks.Breakable;
                            //new_instance(x, 0, y, breakable_prefab);
                        }
                    }
                }*/
            } 
        }

        int obstacle_count = x_count * y_count / 8;
        for (int i = 0; i < obstacle_count; i++) {
            int rx = Random.Range(0, x_count);
            int ry = Random.Range(0, y_count);
            if (array_representation[rx, ry] == Blocks.Breakable && !start_next_to(rx, ry)) {
                int h = Random.Range(1, 5);
                array_representation[rx, ry] = Blocks.Wall;
                for (int j = 0; j < h; j++)
                    new_instance(rx, j, ry, wall_prefab);
            }
        }
    }

    private bool start_next_to(int x, int y) {
        if (array_representation[x-1, y] == Blocks.Startpos) {
            return true;
        }
        if (array_representation[x+1, y] == Blocks.Startpos) {
            return true;
        }
        if (array_representation[x, y+1] == Blocks.Startpos) {
            return true;
        }
        if (array_representation[x, y-1] == Blocks.Startpos) {
            return true;
        }
        if (array_representation[x, y] == Blocks.Startpos) {
            return true;
        }
        if (array_representation[x-1, y-1] == Blocks.Startpos) {
            return true;
        }
        if (array_representation[x+1, y+1] == Blocks.Startpos) {
            return true;
        }
        return false;
    }

    private GameObject new_instance(int x, int y, int z, GameObject prefab) {
        GameObject temp_floor = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) ; // create new prefab instance
        temp_floor.transform.SetParent(Map_parent.transform); // set parent
        return temp_floor;
    }
}
