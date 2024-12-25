using System.Collections;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUIController : MonoBehaviour
{
    public static GameplayUIController Instance;

    [Header("Health")]
    public Slider healthSlider;
    public int currentHealth;
    public int maxHealth;

    [Header("Ghost")]
    public Slider ghostSlider;
    public int currentGhost;
    public int maxGhost;

    [Header("Util Count")]
    public TextMeshProUGUI itemText;

    [Header("Wheel")]
    public Animator anim;
    public bool isOpen = false;
    private bool weaponWheelSelected = false;
    public Image selectedItem;
    public Sprite noImage;
    public static int weaponID;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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

    public void UpdateHealthSlider(int currentHealth, int maxHealth)
    {
        healthSlider.minValue = 0;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        StartCoroutine(LerpHealth(currentHealth));
    }
    private IEnumerator LerpHealth(int targetHealth)
    {
        float elapsed = 0f;
        float duration = 0.5f; // Smooth transition duration
        float startValue = healthSlider.value;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            healthSlider.value = Mathf.Lerp(startValue, targetHealth, elapsed / duration);
            yield return null;
        }

        healthSlider.value = targetHealth;
    }

    public void UpdateGhostSlider(int currentGhost, int maxGhost)
    {
        ghostSlider.minValue = 0;
        ghostSlider.maxValue = maxGhost;
        ghostSlider.value = currentGhost;

        StartCoroutine(LerpGhost(currentGhost));
    }
    private IEnumerator LerpGhost(int targetGhost)
    {
        float elapsed = 0f;
        float duration = 0.5f; // Smooth transition duration
        float startValue = ghostSlider.value;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ghostSlider.value = Mathf.Lerp(startValue, targetGhost, elapsed / duration);
            yield return null;
        }

        ghostSlider.value = targetGhost;
    }

    public void UpdateItemText(int currentItems, int maxItems)
    {
        itemText.text = $"{currentItems}/{maxItems}";

        if (currentItems == 0 && maxItems == 0) itemText.text = $"";
    }
}
