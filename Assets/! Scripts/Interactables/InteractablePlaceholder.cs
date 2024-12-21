using UnityEngine;

public class InteractablePlaceholder : Interactable
{
    public override void DoHoverOverActions()
    {
        Debug.Log("Hovering over interactable");
    }

    public override void DoInteraction()
    {
        Debug.Log("Interacted");
    }
}
