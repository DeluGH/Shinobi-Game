using UnityEngine;
using UnityEngine.UI;

public class WeaponWheelController : MonoBehaviour
{
    public Animator anim;
    private bool weaponWheelSelected = false;
    public Image selectedItem;
    public Sprite noImage;
    public static int weaponID;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            weaponWheelSelected = true;
            anim.SetBool("OpenWeaponWheel", true);

            // Unlock and show the cursor while the weapon wheel is open
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Close weapon wheel when Tab is released
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            weaponWheelSelected = false;
            anim.SetBool("OpenWeaponWheel", false);

            // Lock and hide the cursor after the weapon wheel is closed
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


        switch (weaponID)
        {
            case 0: // nothing is selected
                selectedItem.sprite = noImage;
                break;
            case 1: // Item 1
                Debug.Log("Kunai");
                break;
            case 2: // Item 2
                Debug.Log("Shuriken");
                break;
            case 3: // Item 3
                Debug.Log("Smokebomb");
                break;
            case 4: // Item 4
                Debug.Log("Windchime");
                break;
        }
    }
}
