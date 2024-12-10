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
    [Header("Debug (Reference)")]
    public float chargeStartTime;
    public Item utilHand; // swaps with Inventory Items
    public Item mainHand; // For holding sword or swapping with back items
    public Item onBack; // Item currently stored on the back

    [Header("References (Auto)")]
    public GameObject player; // This game object
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
   

    private void Awake()
    {
        player = gameObject ?? GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Check for holding left click
        if (Input.GetKeyDown(KeybindManager.Instance.keybinds["Use Utility"]) && utilHand != null && utilHand.canHoldCharge)
        {
            chargeStartTime = Time.time; // Record when charging started
        }
        if (Input.GetKey(KeybindManager.Instance.keybinds["Use Utility"]) && utilHand != null && utilHand.canHoldCharge)
        {
            float chargeTime = Time.time - chargeStartTime;
            utilHand.Charge(player, chargeTime); // charging - change values within smoke bomb
        }


        // Equip items based on number keys 1-4
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && i < inventorySlots.Count)
            {
                EquipFromInventorySlot(i);
            }
        }

        // Lift left-click, handle Use or Charge
        if (Input.GetKeyUp(KeybindManager.Instance.keybinds["Use Utility"]))
        {
            if (utilHand != null)
            {
                UseEquippedItem();
            }
        }

        // Check if unequipUtilHand key is pressed
        if (Input.GetKeyDown(KeybindManager.Instance.keybinds["Unequip Offhand"]))
        {
            UnequipUtilHand();
        }
    }

    void UseEquippedItem() // Left clicking or Fire1
    {
        if (utilHand != null)
        {
            try
            {
                utilHand.Use(player); // Call the Use method on the equipped item
                RemoveHandObject();
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

    void RemoveHandObject()
    {
        if (utilHand != null && utilHand.minusOnUse)
        {
            utilHand = null; // Remove from hand
        }
    }


    void EquipFromInventorySlot(int slotIndex) // Equip when using keys 1-4
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count) return;

        var slot = inventorySlots[slotIndex];

        if (slot.item == null || slot.count <= 0) return;


        if (slot.item.storeType == StoreType.Back)
        {
            // If item can go on the back, switch the onHand item to onBack
            if (utilHand != null)
            {
                if (utilHand.storeType == StoreType.Inventory)
                {
                    AddToInventory(utilHand);
                }
                else if (utilHand.storeType == StoreType.Back)
                {
                    onBack = utilHand; // Place the current onHand item on the back
                }
            }

            utilHand = slot.item; // Equip the new item to onHand
        }
        else if (slot.item.storeType == StoreType.Inventory)
        {
            // Items restricted to inventory go directly to onHand
            AddToInventory(utilHand); // Put the current onHand item back into inventory
            utilHand = slot.item;
        }

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
}
