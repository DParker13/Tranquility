using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeAnimationController : MonoBehaviour
{
    public void DestroyTree()
    {
        Destroy(gameObject);
    }

    public void SpawnSticks(GameObject dropableObject)
    {
        for(int i = 0; i < 5; i++)
        {
            float randX = Random.Range(-1f, 1f);
            float randY = Random.Range(-1f, 1f);

            var currentObject = Instantiate(dropableObject, new Vector3(transform.position.x + randX, transform.position.y + randY, 0), Quaternion.identity, transform);
            currentObject.transform.parent = null;
        }
    }
}
