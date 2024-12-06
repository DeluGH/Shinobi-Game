using UnityEngine;

[CreateAssetMenu(fileName = "NewSmokeBomb", menuName = "Items/SmokeBomb")]
public class ItemSmokebomb : Item
{
    public GameObject smokeEffectPrefab;
    public float minCharge = 1f;
    public float maxCharge = 20f;
    public float chargePerSecond = 20f;
    public float chargeForce = 0f;

    public override void Use(GameObject player)
    {
        Debug.Log($"{itemName} used!");

        if (smokeEffectPrefab != null && player != null)
        {
            GameObject smokeBomb = Instantiate(smokeEffectPrefab, player.transform.position, Quaternion.identity);
            SmokeBomb smokeBombScript = smokeBomb.GetComponent<SmokeBomb>();
            if (smokeBombScript != null)
            {
                smokeBombScript.ActivateSmokeBomb(chargeForce, player.transform.forward);
            }

            // Reset chargeForce after use
            chargeForce = minCharge;
        }
    }

    public override void Charge(GameObject player, float chargeTime)
    {
        if (!canHoldCharge) return;

        // Calculate and clamp chargeForce
        chargeForce = Mathf.Clamp(chargeTime * chargePerSecond, minCharge, maxCharge);
        Debug.Log($"Charging {itemName}. Current force: {chargeForce}");
    }
}
