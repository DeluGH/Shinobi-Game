using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class WeaponWheelController : MonoBehaviour
{
    public Animator anim;
    public bool isOpen = false;
    private bool weaponWheelSelected = false;
    public Image selectedItem;
    public Sprite noImage;
    public static int weaponID;
    public static WeaponWheelController Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
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

    public void OpenInventoryWheel()
    {
        isOpen = true;
        weaponWheelSelected = true;
        anim.SetBool("OpenWeaponWheel", true);

        // Unlock and show the cursor while the weapon wheel is open
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseInventoryWheel()
    {
        isOpen = false;
        weaponWheelSelected = false;
        anim.SetBool("OpenWeaponWheel", false);

        // Lock and hide the cursor after the weapon wheel is closed
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
