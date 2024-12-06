using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public float chargeStartTime;

    [System.Serializable]
    public class InventorySlot
    {
        public Item item;
        public int count;
    }

    public GameObject player; // This game object
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public Item onHand; // Item currently held in hand
    public Item onBack; // Item currently stored on the back

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
        if (Input.GetKeyDown(KeyCode.Mouse0) && onHand != null && onHand.canHoldCharge)
        {
            chargeStartTime = Time.time; // Record when charging started
        }
        if (Input.GetKey(KeyCode.Mouse0) && onHand != null && onHand.canHoldCharge)
        {
            float chargeTime = Time.time - chargeStartTime;
            onHand.Charge(player, chargeTime); // charging - change values within smoke bomb
        }

        // Equip items based on number keys 1-4
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && i < inventorySlots.Count)
            {
                EquipFromSlot(i);
            }
        }

        // Lift left-click, handle Use or Charge
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (onHand != null)
            {
                UseEquippedItem();
            }
        }
    }

    void UseEquippedItem() // Left clicking or Fire1
    {
        if (onHand != null)
        {
            try
            {
                onHand.Use(player); // Call the Use method on the equipped item
                RemoveHandObject();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error while using {onHand.itemName}: {ex.Message}\nCould be player null reference.");
            }
        }
        else
        {
            Debug.Log("No item equipped!");
        }
    }

    void RemoveHandObject()
    {
        if (onHand != null && onHand.minusOnUse)
        {
            onHand = null; // Remove from hand
        }
    }


    void EquipFromSlot(int slotIndex) // Equip when using keys 1-4
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count) return;

        var slot = inventorySlots[slotIndex];

        if (slot.item == null || slot.count <= 0) return;


        if (slot.item.storeType == StoreType.Back)
        {
            // If item can go on the back, switch the onHand item to onBack
            if (onHand != null)
            {
                if (onHand.storeType == StoreType.Inventory)
                {
                    AddToInventory(onHand);
                }
                else if (onHand.storeType == StoreType.Back)
                {
                    onBack = onHand; // Place the current onHand item on the back
                }
            }

            onHand = slot.item; // Equip the new item to onHand
        }
        else if (slot.item.storeType == StoreType.Inventory)
        {
            // Items restricted to inventory go directly to onHand
            AddToInventory(onHand); // Put the current onHand item back into inventory
            onHand = slot.item;
        }

        RemoveFromSlot(slotIndex); // Remove item from inventory
    }

    // Putting things back into inventory when swapping items
    void AddToInventory(Item item)
    {
        if (item == null) return;

        // Check if item already exists in inventory
        foreach (var slot in inventorySlots)
        {
            if (slot.item == item && slot.count < item.maxStackCount)
            {
                slot.count++;
                return;
            }
        }

        // Add to first empty slot
        foreach (var slot in inventorySlots)
        {
            if (slot.item == null)
            {
                slot.item = item;
                slot.count = 1;
                return;
            }
        }

        Debug.Log("Inventory full!");
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
}
