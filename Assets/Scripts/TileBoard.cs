using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBoard : MonoBehaviour
{
    public GameManager gameManager;

    public Tile tilePrefab;
    public TileState[] tileStates;
    private TileGrid grid;
    private List <Tile> tiles;

    // prevent from pressing input while it's animating
    private bool waiting;

    private void Awake(){
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16);                 // our gird 4x4 => 16
    }

    public void ClearBoard(){
        foreach(var cell in grid.cells){
            cell.tile = null;
        }

        foreach(var tile in tiles){
            Destroy(tile.gameObject);
        }

        tiles.Clear();
    }

    public void CreateTile(){
        Tile tile = Instantiate(tilePrefab, grid.transform);
        tile.SetState(tileStates[0]);
        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);
    }

    private void Update(){
        if(!waiting){
            // get key to move tiles
            if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)){
                Move(Vector2Int.up, 0, 1, 1, 1);   
            }else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)){
                Move(Vector2Int.down, 0, 1, grid.height - 2, -1); 
            }else if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)){
                Move(Vector2Int.left, 1, 1, 0, 1); 
            }else if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)){
                Move(Vector2Int.right, grid.width - 2, -1, 0, 1); 
            }
        }

    }

    private void Move(Vector2Int direction, int startX, int incrementX, int startY, int incrementY){
        bool changed = false;

        for(int x = startX; x >= 0 && x < grid.width; x += incrementX){
            for(int y = startY; y >= 0 && y < grid.height; y += incrementY){
                TileCell cell = grid.GetCell(x, y);

                if(cell.occupied){
                    changed |= MoveTile(cell.tile, direction);
                }
            }
        }

        if(changed) StartCoroutine(WaitForChanges());
    }

    // *** game main logic
    private bool MoveTile(Tile tile, Vector2Int direction){
        TileCell newCell = null;
        TileCell adjacentCell = grid.GetAdjacentCell(tile.cell, direction);

        while(adjacentCell != null){    // (1) not null, we can move tile
            if(adjacentCell.occupied){  
                // merging
                if(CanMerge(tile, adjacentCell.tile)){
                    MergeTiles(tile, adjacentCell.tile);
                    return true;
                }
                break;
            }

            newCell = adjacentCell;
            adjacentCell = grid.GetAdjacentCell(adjacentCell, direction);
        }

        if(newCell != null){            // (2) not null, we can move tile
             tile.MoveTo(newCell);
             //StartCoroutine(WaitForChanges());  < it's not good because it's called too many times
             return true;
        }

        return false;
    }

    // each tiles has same number tile?
    private bool CanMerge(Tile a, Tile b){
        return a.state == b.state && !b.locked; 
    }

    // merge
    private void MergeTiles(Tile a, Tile b){
        // destroy tile a, and new merged tile in b's posiion 
        tiles.Remove(a);
        a.Merge(b.cell);

        // update to state of b(next tile level)
        // int index = IndexOf(b.state) + 1; < It's inappropriate becase the state level can be increased indefinitely
        int index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);
        TileState newState = tileStates[index];

        b.SetState(newState);
        gameManager.IncreaseScore(newState.number);
    }

    private int IndexOf(TileState state){
        for(int i = 0; i < tileStates.Length; ++i){
            if(state == tileStates[i]){
                return i;
            }
        }

        return -1;
    }

    private IEnumerator WaitForChanges(){
        waiting = true;

        yield return new WaitForSeconds(0.1f);  // while duration, wait

        waiting = false;

        // merge lock init
        foreach(var tile in tiles){
            tile.locked = false;
        }

        // create new tile: available space exist
        if(tiles.Count != grid.size) CreateTile();
        
        // check for gameover
        if(CheckForGameOver()){
            gameManager.GameOver();
        }

    }

    private bool CheckForGameOver(){
        // (1) the entire board is occupied
        if(tiles.Count != grid.size){
            return false;
        }

        // (2) there's no available merges
        foreach(var tile in tiles){
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);

            if(up != null && CanMerge(tile, up.tile)) return false;
            if(down != null && CanMerge(tile, down.tile)) return false;
            if(left != null && CanMerge(tile, left.tile)) return false;
            if(right != null && CanMerge(tile, right.tile)) return false;
        }

        return true;
    }
}
