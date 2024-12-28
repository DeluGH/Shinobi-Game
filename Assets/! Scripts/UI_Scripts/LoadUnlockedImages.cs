using UnityEngine;
using UnityEngine.UI;

public class LoadUnlockedImages : MonoBehaviour
{
    public GameObject ContentHolder;
    public GameObject imagePrefab;

    [Header("GhostMode")]
    public Sprite ghostImage;
    public string OnHoverMessage;

    private void Start()
    {
        LoadUnlocked();
    }

    public void LoadUnlocked()
    {
        if (Unlocked.Instance.IsGhostModeUnlocked())
        {
            // Instantiate the image prefab as a child of the ContentHolder
            GameObject newImage = Instantiate(imagePrefab, ContentHolder.transform);

            // Set the sprite of the Image component to ghostImage
            Image imageComponent = newImage.GetComponent<Image>();
            if (imageComponent != null)
            {
                imageComponent.sprite = ghostImage;
            }
            else
            {
                Debug.LogError("The prefab does not have an Image component!");
            }
        }
    }
}
