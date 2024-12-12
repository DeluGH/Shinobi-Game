using UnityEngine;

public abstract class Item : ScriptableObject
{
    public string itemName;
    public int maxStackCount = 0;
    public bool minusOnUse = true;
    public bool canHoldCharge = false;

    public GameObject itemHoldModel;

    public abstract void Use(GameObject player);
    public virtual void Charge(GameObject player, float chargeTime)
    {

    }
}
