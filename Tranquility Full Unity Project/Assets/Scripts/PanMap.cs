using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanMap : MonoBehaviour
{
    public int moveSpeed;

    // Update is called once per frame
    void Update ()
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x + moveSpeed * Time.deltaTime, 0, -10);
	}
}
