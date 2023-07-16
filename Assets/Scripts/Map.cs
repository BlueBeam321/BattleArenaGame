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

    private GameObject wallPrefab;
    private GameObject startPosPrefab;
    private GameObject floorPrefab;
    private GameObject MapParent;

    private Blocks[,] blockMaps;

    public void construct(int start_poses, int x, int y, GameObject parent)
    {       
        MapParent = parent;
        
        startPosPrefab = (GameObject) Resources.Load("Startpos",typeof(GameObject));
        floorPrefab = (GameObject) Resources.Load("Floor",typeof(GameObject));
        wallPrefab = (GameObject) Resources.Load("Wall",typeof(GameObject));
        
        if (x <= 2)
            x = 2;
        
        if (y <= 2)
            y = 2;

        if (start_poses > 4)
            start_poses = 4;

        width = x;
        height = y;

        blockMaps = new Blocks[width, height];

        blockMaps[1, height - 2] = Blocks.StartPos;
        blockMaps[1, 1] = Blocks.StartPos;
        blockMaps[1, height / 2 + (height % 2)] = Blocks.StartPos;
        blockMaps[width - 2, height - 2] = Blocks.StartPos;
        blockMaps[width - 2, height - 2] = Blocks.StartPos;
        blockMaps[width - 2, 1] = Blocks.StartPos;

        create_players(start_poses, width, height);
        create_map(width, height);
    }

    public void generate_player(bool enemy)
    {
        int[] jj = { height - 2, height / 2 + (height % 2), 1 };
        if (enemy)
        {
            int i = width - 2;
            int j = jj[Random.Range(0, 3)];
            new_instance(i, 0, j, startPosPrefab);
            blockMaps[i, j] = Blocks.StartPos;
        }
        else
        {
            int i = 1;
            int j = jj[Random.Range(0, 3)];
            GameObject t = new_instance(i, 0, j, startPosPrefab);
            t.GetComponent<startpos_script>().player_controller = true;
            blockMaps[i, j] = Blocks.StartPos;
        }
    }

    public bool is_walkable(int x, int y)
    {
        return blockMaps[x, y] != Blocks.Wall;
    }

    private void create_players(int num_players, int x_count, int y_count)
    {
        int i = 1;
        int j = y_count / 2 +  (y_count % 2);
        GameObject t = new_instance(i, 0, j, startPosPrefab);
        t.GetComponent<startpos_script>().player_controller = true;
        blockMaps[i, j] = Blocks.StartPos;

        if (num_players > 1) 
        {
            i = x_count - 2;
            j = y_count - 2;
            new_instance(i, 0, j , startPosPrefab);
            blockMaps[i, j] = Blocks.StartPos;
        }

        if (num_players > 2)
        {
            i = x_count - 2;
            j = 1;
            new_instance(i, 0, j, startPosPrefab);
            blockMaps[i, j] = Blocks.StartPos;
        }

        if (num_players > 3)
        {
            i = width - 2;
            j = height / 2 +  (height % 2);
            new_instance(i, 0, j, startPosPrefab);
            blockMaps[i, j] = Blocks.StartPos;
        }
    }

    private void create_map(int x_count, int y_count)
    {
        for (int x = 0; x < x_count; x++)
        {
            for (int y = 0; y < y_count; y++)
            {
                // Create floor
                new_instance(x, -1, y, floorPrefab);
                
                // Create all wall limits 
                if (x == 0 || x == x_count - 1 || y == 0 || y == y_count - 1)
                {
                    blockMaps[x, y] = Blocks.Wall;
                    new_instance(x, 0, y, wallPrefab);
                }
            } 
        }

        int obstacle_count = x_count * y_count / 8;
        for (int i = 0; i < obstacle_count; i++)
        {
            int rx = Random.Range(0, x_count);
            int ry = Random.Range(0, y_count);
            if (blockMaps[rx, ry] == Blocks.Breakable && !start_next_to(rx, ry)) {
                int h = Random.Range(1, 4);
                blockMaps[rx, ry] = Blocks.Wall;
                for (int j = 0; j < h; j++)
                    new_instance(rx, j, ry, wallPrefab);
            }
        }
    }

    private bool start_next_to(int x, int y)
    {
        if (blockMaps[x-1, y] == Blocks.StartPos)
            return true;
        if (blockMaps[x+1, y] == Blocks.StartPos)
            return true;
        if (blockMaps[x, y+1] == Blocks.StartPos)
            return true;
        if (blockMaps[x, y-1] == Blocks.StartPos)
            return true;
        if (blockMaps[x, y] == Blocks.StartPos)
            return true;
        if (blockMaps[x-1, y-1] == Blocks.StartPos)
            return true;
        if (blockMaps[x+1, y+1] == Blocks.StartPos)
            return true;
        return false;
    }

    private GameObject new_instance(int x, int y, int z, GameObject prefab)
    {
        GameObject temp_floor = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) ; // create new prefab instance
        temp_floor.transform.SetParent(MapParent.transform); // set parent
        return temp_floor;
    }
}
