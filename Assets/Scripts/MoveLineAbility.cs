using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveLineAbility : MonoBehaviour
{
    float speed=2f;
    public Vector3Int end;
    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, GridManager.GetTileToCellFromWorld(end))<0.15f)
        {
            Destroy(gameObject,1f);
        }
        else
        {
            Vector3 targetPos = GridManager.GetTileToCellFromWorld(end);
            transform.position += (targetPos - transform.position).normalized * speed*Time.deltaTime;
        }
       
    }
}
