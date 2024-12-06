using UnityEngine;

public enum StoreType
{
    Inventory,
    Back,
}

public abstract class Item : ScriptableObject
{
    public string itemName;
    public StoreType storeType = StoreType.Inventory;
    public int maxStackCount = 0;
    public bool minusOnUse = true;
    public bool canHoldCharge = false;

    public abstract void Use(GameObject player);
    public virtual void Charge(GameObject player, float chargeTime)
    {

    }
}
