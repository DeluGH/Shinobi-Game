using UnityEngine;

public class GhostSwordInteractable : Interactable
{
    public bool disappearOnInteract = true;
    public AudioSource audioSource;
    public AudioClip pickUpClip;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) Debug.LogWarning("No audio sauce!");
    }

    public override void DoHoverOverActions()
    {
        Debug.Log("Hovering over interactable");
    }

    public override void DoInteraction()
    {
        Debug.Log("Interacted");

        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        player.hasGhostSword = true;

        audioSource.PlayOneShot(pickUpClip);

        if (disappearOnInteract) Destroy(gameObject);
    }
}
