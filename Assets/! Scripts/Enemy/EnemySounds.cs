using UnityEngine;
using UnityEngine.Audio;

public class EnemySounds : MonoBehaviour
    // THIS NEEDS TO BE A CHILD OBJECT
{
    [Header("Combat")]
    public AudioClip[] dieSounds;
    public AudioClip[] hitSounds;
    public AudioClip[] finalHitSounds;

    [Header("Detection")]
    public AudioClip[] aware;
    public AudioClip[] investigate;
    public AudioClip[] alerted;
    public AudioClip[] backToNormal;

    [Header("Activity")]
    public AudioClip[] ambience;

    [Header("References (Auto)")]
    public Enemy enemyScript;

    private void Start()
    {
        if (enemyScript == null) enemyScript = GetComponentInParent<Enemy>();
        if (enemyScript == null) Debug.LogWarning("No enemyScript!!");
    }

    //Combat
    public void PlayDyingSound()
    {
        int random = Random.Range(0, dieSounds.Length);
        enemyScript.audioSource.PlayOneShot(dieSounds[random]);
    }
    public void PlayHitSound()
    {
        if (enemyScript != null && !enemyScript.isDead)
        {
            int random = Random.Range(0, hitSounds.Length);
            enemyScript.audioSource.PlayOneShot(hitSounds[random]);
        }
            
    }
    public void PlayFinalHitSound()
    {
        if (enemyScript != null && !enemyScript.isDead)
        {
            int random = Random.Range(0, finalHitSounds.Length);
            enemyScript.audioSource.PlayOneShot(finalHitSounds[random]);
        }
    }

    //Detection
    public void PlayBackToNormal()
    {
        if (enemyScript != null && !enemyScript.isDead)
        {
            int random = Random.Range(0, backToNormal.Length);
            enemyScript.audioSource.PlayOneShot(backToNormal[random]);
        }
    }
    public void PlayAware()
    {
        if (enemyScript != null && !enemyScript.isDead)
        {
            int random = Random.Range(0, aware.Length);
            enemyScript.audioSource.PlayOneShot(aware[random]);
        }
    }
    public void PlayInvestigate()
    {
        if (enemyScript != null && !enemyScript.isDead)
        {
            int random = Random.Range(0, investigate.Length);
            enemyScript.audioSource.PlayOneShot(investigate[random]);
        }
    }
    public void PlayAlerted()
    {
        if (enemyScript != null && !enemyScript.isDead)
        {
            int random = Random.Range(0, alerted.Length);
            enemyScript.audioSource.PlayOneShot(alerted[random]);
        }
            
    }

    //Ambience
    public void PlayAmbience()
    {
        if (enemyScript != null && !enemyScript.isDead)
        {
            int random = Random.Range(0, ambience.Length);
            enemyScript.audioSource.PlayOneShot(ambience[random]);
        }
            
    }
}
