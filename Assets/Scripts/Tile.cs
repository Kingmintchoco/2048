using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public TileState state { get; private set; }
    public TileCell cell { get; private set; }

    private Image background;
    private TextMeshProUGUI text;

    // only be able to merge it once per piece. so, we need to prevent double merge
    public bool locked { get; set; }

    private void Awake(){
        background = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetState(TileState state){
        this.state = state;

        background.color = state.backgroundColor;
        text.color = state.textColor;
        text.text = state.number.ToString();
    }

    // tile spawn function
    public void Spawn(TileCell cell){
        // already occupied, assign null
        if(this.cell != null) this.cell.tile = null;

        this.cell =  cell;
        this.cell.tile = this;

        // tile position = cell position
        transform.position = cell.transform.position;
    }

    public void MoveTo(TileCell cell){
        if(this.cell != null) this.cell.tile = null;

        this.cell = cell;
        this.cell.tile = this;

        StartCoroutine(Animate(cell.transform.position, false));
    }

    public void Merge(TileCell cell){
        // already occupied, assign null -> clear
        if(this.cell != null) this.cell.tile = null; 

        this.cell = null;
        cell.tile.locked = true;

        StartCoroutine(Animate(cell.transform.position, true));
    }

    // tile animation
    private IEnumerator Animate(Vector3 to, bool merging){
        float elaspsed = 0f;
        float duration = 0.1f;

        Vector3 from = transform.position;

        while(elaspsed < duration){
            transform.position = Vector3.Lerp(from, to, elaspsed / duration);
            elaspsed += Time.deltaTime;
            yield return null;
        }

        transform.position = to;

        if(merging) Destroy(gameObject);
    }
}
