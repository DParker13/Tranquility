using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float playerSpeed = 10f;
    public int playerHealth = 100;
    public PlayerInfo playerSaveInfo;
    public List<Item> playerInventory;
    public Collider2D[] nearbyItems; //Array of all nearby colliders
    public float playerPickupRadius = 1f; //Radius that items will float to the character
    public float itemPickupSpeed = 0.5f; //Speed the item floats to the character
    public float truePickupDistance = 0.1f; //Distance it takes for the item to be added to inventory

    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInventory = new List<Item>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        //Makes a circle around the player that gets any object with a collider
        nearbyItems = Physics2D.OverlapCircleAll(gameObject.transform.position, playerPickupRadius);

        //MOVING THE PLAYER
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * playerSpeed;
        var y = Input.GetAxis("Vertical") * Time.deltaTime * playerSpeed;

        transform.Translate(x, y, 0);

        //Allows for the player to pick up certain objects
        PickUpItems();

        //Allows the player to be able to interact with certain objects in the world
        InteractWithWorld();

        //UPDATING SAVE DATA
        playerSaveInfo.playerHealth = playerHealth;
        playerSaveInfo.playerX = transform.position.x;
        playerSaveInfo.playerY = transform.position.y;
        //UPDATING SAVE DATA

        //------------ANIMATION---------------

        animator.SetFloat("XSpeed", x);
        animator.SetFloat("YSpeed", y);

        //------------END ANIMATION---------------
    }

    private void PickUpItems()
    {
        if(nearbyItems.Length > 0)
        {
            //Searches for all nearby items around the player
            for (int i = 0; i < nearbyItems.Length; i++)
            {
                //If the nearby object is a PickupObject check the distance
                if (nearbyItems[i].tag == "PickupObject")
                {
                    float distance = Vector2.Distance(gameObject.transform.position, nearbyItems[i].transform.position);

                    //If the inventory is NOT full, try to pick up the object
                    if (playerInventory.Capacity < 32)
                    {
                        //Moves the item towards the player if the object ISN'T over the player
                        if (distance > truePickupDistance)
                        {
                            nearbyItems[i].gameObject.transform.position = Vector2.MoveTowards(nearbyItems[i].gameObject.transform.position, gameObject.transform.position, itemPickupSpeed);
                        }

                        //Adds the item to the player's inventory if the object IS over the player
                        if (distance <= truePickupDistance)
                        {
                            bool foundItem = false;

                            //Loops through the inventory to see if there are more than one of that object
                            for (int j = 0; j < playerInventory.Capacity; j++)
                            {
                                if (nearbyItems[i].name == playerInventory[j].itemName && playerInventory[j].itemAmount < 1000)
                                {
                                    playerInventory[j].itemAmount += 1;

                                    //Hides the item
                                    nearbyItems[i].gameObject.SetActive(false);

                                    foundItem = true;

                                    //Breaks out of the loop when an object is found
                                    break;
                                }
                            }

                            //If there this item isn't in the inventory, add it to the inventory
                            if (foundItem == false)
                            {
                                //Creates a new item for pickup in the inventory
                                Item pickedUpItem = new Item();
                                pickedUpItem.itemSprite = nearbyItems[i].GetComponent<SpriteRenderer>().sprite;
                                pickedUpItem.itemName = nearbyItems[i].name;
                                pickedUpItem.itemID = i;
                                pickedUpItem.itemAmount += 1;

                                //Adds the item to the player inventory
                                playerInventory.Add(pickedUpItem);

                                //Hides the item
                                nearbyItems[i].gameObject.SetActive(false);
                            }

                        }
                    }
                }
            }
        }
    }

    private void InteractWithWorld()
    {
        //INTERACT WITH WORLD

        for (int i = 0; i < nearbyItems.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

                Collider2D currentCollider = nearbyItems[i].GetComponent<Collider2D>();

                if (hit.collider == currentCollider && nearbyItems[i].tag == "Trunk")
                {
                    Transform treeParent = currentCollider.gameObject.transform.parent;
                    ObjectInfo currentObjectInfo = treeParent.GetComponent<ObjectInfo>();

                    if (currentObjectInfo.numHits < currentObjectInfo.numHitsTillBreak && !currentObjectInfo.broken)
                    {
                        currentObjectInfo.numHits += 1;
                        nearbyItems[i].GetComponentInChildren<Animation>().Play("TreeHit");
                        nearbyItems[i].GetComponentInChildren<ParticleSystem>().Emit(5);
                    }

                    if(currentObjectInfo.numHits == currentObjectInfo.numHitsTillBreak && !currentObjectInfo.broken)
                    {
                        //Sets the object to broken
                        currentObjectInfo.broken = true;

                        //Chooses a random direction for the tree to fall
                        int randDirection = Random.Range(0, 2);
                        if(randDirection == 0)
                        {
                            nearbyItems[i].GetComponentInChildren<Animation>().Play("FallingRight");
                        }
                        else
                        {
                            nearbyItems[i].GetComponentInChildren<Animation>().Play("FallingLeft");
                        }
                    }
                }
            }
        }

        //INTERACT WITH WORLD
    }
}
