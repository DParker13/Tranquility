using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour {

    public Item[] inventoryItems;
    public List<GameObject> inventoryGO;
    public GameObject player;
    public MenuController MC;
	
	// Update is called once per frame
	void Update ()
    {
        //updates the inventory with the correct sprites when it is open
        if(MC.inInventoryMenu)
        {
            inventoryItems = player.GetComponent<PlayerController>().playerInventory.ToArray();

            for (int i = 0; i < inventoryItems.Length; i++)
            {
                Image inventorySR = inventoryGO[i].GetComponent<Image>();
                if (inventoryGO[i] != null)
                {
                    inventorySR.sprite = inventoryItems[i].itemSprite;
                    inventorySR.color = new Color(255, 255, 255, 1);
                }
                else
                {
                    inventorySR.sprite = null;
                    inventorySR.color = new Color(0, 0, 0, 0);
                }
            }
        }
    }
}
