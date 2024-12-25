using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Voiceline
{
    public AudioClip audioClip; // The audio file for the voiceline
    [TextArea(2, 5)]           // Allows multiline editing in the Inspector
    public string subtitles;    // The subtitles for the voiceline
}

public class EnemySounds : MonoBehaviour
{
    [Header("DO NOT RESET SCRIPT")]
    public float audibleDistance = 24;

    [Header("Combat")]
    public Voiceline[] dieSounds;
    public Voiceline[] hitSounds;
    public Voiceline[] finalHitSounds;

    [Header("Detection")]
    public Voiceline[] aware;
    public Voiceline[] investigate;
    public Voiceline[] alerted;
    public Voiceline[] backToNormal;

    [Header("Activity")]
    public Voiceline[] ambience;

    [Header("References (Auto)")]
    public Enemy enemyScript;
    public Voiceline currentVoiceline;

    private void Start()
    {
        if (enemyScript == null) enemyScript = GetComponentInParent<Enemy>();
        if (enemyScript == null) Debug.LogWarning("No enemyScript!!");
    }

    public void StopSpeaking()
    {
        enemyScript.audioSource.Stop();
        SubtitleManager.Instance.RemoveSubtitle(currentVoiceline);
    }

    private void PlayVoiceline(Voiceline[] voicelines)
    {
        StopSpeaking();

        if (enemyScript != null && !enemyScript.isDead && voicelines.Length > 0
            && Vector3.Distance(enemyScript.transform.position, enemyScript.player.transform.position) <= audibleDistance)
        {
            int random = Random.Range(0, voicelines.Length);
            currentVoiceline = voicelines[random]; // Set the current voiceline

            // Play the audio clip
            enemyScript.audioSource.PlayOneShot(currentVoiceline.audioClip);

            // Show the subtitle
            float clipDuration = currentVoiceline.audioClip != null ? currentVoiceline.audioClip.length : 3f; // Default duration if no audio
            SubtitleManager.Instance.ShowSubtitle(currentVoiceline.subtitles, currentVoiceline, clipDuration + 2f); //add 2 seconds extra
        }
    }

    // Combat
    public void PlayDyingSound() => PlayVoiceline(dieSounds);
    public void PlayHitSound() => PlayVoiceline(hitSounds);
    public void PlayFinalHitSound() => PlayVoiceline(finalHitSounds);

    // Detection
    public void PlayBackToNormal() => PlayVoiceline(backToNormal);
    public void PlayAware() => PlayVoiceline(aware);
    public void PlayInvestigate() => PlayVoiceline(investigate);
    public void PlayAlerted() => PlayVoiceline(alerted);

    // Ambience
    public void PlayAmbience() => PlayVoiceline(ambience);
}

