using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int count;
}

public class Inventory : MonoBehaviour
{
    [Header("Inventory Wheel Settings")]
    public float openWheelKeyPressTime = 0.1f; // Hold inventory button for this long to open
    public float inventoryPressStartTime = 0f;
    public float inventoryPressTime = 0f;

    [Header("Item Throw Transform")]
    public Transform instantitatePosition;

    [Header("Hand Positions")]
    public Transform utilHandPos;
    public Transform mainHandPos;

    [Header("Debug (Reference)")]
    public float chargeStartTime;
    public Item utilHand; // swaps with Inventory Items
    public GameObject mainHand; // For holding sword or swapping with back items
    public GameObject onBack; // Item currently stored on the back

    [Header("References (Auto)")]
    public GameObject player; // This game object
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
   

    private void Start()
    {
        player = gameObject ?? GameObject.FindWithTag("Player");

        if (utilHandPos == null) utilHandPos = GameObject.FindWithTag("Util Hand").transform;
        if (mainHandPos == null) mainHandPos = GameObject.FindWithTag("Main Hand").transform;

        UpdateItemPositions();
    }

    private void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Check for holding right click
        //CHARGING ONLY
        if (Input.GetKeyDown(KeybindManager.Instance.keybinds["Use Utility"]) && utilHand != null && utilHand.canHoldCharge)
        {
            chargeStartTime = Time.time; // Record when charging started
        }
        if (Input.GetKey(KeybindManager.Instance.keybinds["Use Utility"]) && utilHand != null && utilHand.canHoldCharge)
        {
            float chargeTime = Time.time - chargeStartTime;
            utilHand.Charge(player, chargeTime); // charging - change values within smoke bomb
        }

        //Lift Use Util key
        if (Input.GetKeyUp(KeybindManager.Instance.keybinds["Use Utility"]))
        {
            if (utilHand != null)
            {
                UseEquippedUtilItem();

                UpdateItemPositions(); // Update Model Hand
            }
        }

        // Check if unequipUtilHand key is pressed
        if (Input.GetKeyDown(KeybindManager.Instance.keybinds["Unequip Offhand"]))
        {
            UnequipUtilHand();

            UpdateItemPositions(); // Update Model Hand
        }

        // INVENTORY HANDLING
        // Wheel or Swap??
        // Quick swap?
        if (Input.GetKeyDown(KeybindManager.Instance.keybinds["Inventory"]))
        {
            inventoryPressStartTime = Time.time;
        }
        if (Input.GetKey(KeybindManager.Instance.keybinds["Inventory"]))
        {
            inventoryPressTime = Time.time - inventoryPressStartTime;

            // Open wheel
            if (inventoryPressTime >= openWheelKeyPressTime)
            {
                WeaponWheelController.Instance.OpenInventoryWheel();
            }
        }
        //Swap Back w Hand / Hand w Back
        if (Input.GetKeyUp(KeybindManager.Instance.keybinds["Inventory"]))
        {
            if (inventoryPressTime < openWheelKeyPressTime) SwapBackAndMain();
            
            // Close Wheel
            if (WeaponWheelController.Instance.isOpen) WeaponWheelController.Instance.CloseInventoryWheel();

            UpdateItemPositions(); // Update Model Hand
        }

        // Equip Items (keys: 1-4)
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && i < inventorySlots.Count)
            {
                EquipFromInventorySlot(i);

                UpdateItemPositions(); // Update Model Hand
            }
        }
    }

    void SwapBackAndMain()
    {
        GameObject temp;
        temp = mainHand;

        mainHand = onBack;
        onBack = temp;
    }

    void UseEquippedUtilItem()
    {
        if (utilHand != null)
        {
            try
            {
                utilHand.Use(player); // Call the Use method on the equipped item
                RemoveUtilHandObject();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error while using {utilHand.itemName}: {ex.Message}\nCould be player null reference.");
            }
        }
        else
        {
            Debug.Log("No item equipped!");
        }
    }

    void RemoveUtilHandObject()
    {
        if (utilHand != null && utilHand.removeOnUse) 
        {
            utilHand = null;
        }
    }


    void EquipFromInventorySlot(int slotIndex) // Equip when using keys 1-4
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count) return;

        InventorySlot slot = inventorySlots[slotIndex];
        if (slot.item == null || slot.count <= 0) return;

        // Items restricted to inventory go directly to onHand
        AddToInventory(utilHand); // Put the current onHand item back into inventory
        utilHand = slot.item;

        RemoveFromSlot(slotIndex); // Remove item from inventory
    }

    // Putting things back into inventory when swapping items
    bool AddToInventory(Item item)
    {
        if (item == null) return false;

        // Check if item already exists in inventory
        foreach (var slot in inventorySlots)
        {
            if (slot.item == item && slot.count < item.maxStackCount)
            {
                slot.count++;
                return true;
            }
        }

        // Add to the first empty slot if the item doesn't already exist in the inventory
        foreach (var slot in inventorySlots)
        {
            if (slot.item == null)
            {
                slot.item = item;
                slot.count = 1;
                return true;
            }
        }

        Debug.Log("Inventory full!");
        return false;
    }

    // Removal from inventory when swapping items
    void RemoveFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count) return;

        if (inventorySlots[slotIndex].count > 1)
        {
            inventorySlots[slotIndex].count--;
        }
        else
        {
            inventorySlots[slotIndex].item = null;
            inventorySlots[slotIndex].count = 0;
        }
    }

    void UnequipUtilHand()
    {
        if (utilHand != null)
        {
            // Attempt to add the utilHand item back into the inventory
            bool addedToInventory = AddToInventory(utilHand);

            if (addedToInventory)
            {
                utilHand = null; // Unequip the item if successfully added to the inventory
            }
            else
            {
                Debug.Log("No space in inventory to unequip the item!");
            }
        }
        else
        {
            Debug.Log("No item equipped in the util hand!");
        }
    }

    // MODELS
    void UpdateItemPositions()
    {
        //Clean hand
        foreach (Transform child in utilHandPos)
        {
            Destroy(child.gameObject); // Destroy old item model
        }
        //UTIL HAND
        if (utilHand != null && utilHand.itemHoldModel != null)
        {
            // Spawning
            GameObject itemModel = Instantiate(utilHand.itemHoldModel, utilHandPos.position, utilHandPos.rotation);
            itemModel.transform.SetParent(utilHandPos);
        }

        //Clean hand
        foreach (Transform child in mainHandPos)
        {
            Destroy(child.gameObject);
        }
        //MAIN HAND
        if (mainHand != null && mainHand != null)
        {
            // Spawning
            GameObject itemModel = Instantiate(mainHand, mainHandPos.position, mainHandPos.rotation);
            itemModel.transform.SetParent(mainHandPos);
        }
    }

}
