using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSFXManager : MonoBehaviour
{
    [Header("Movement SFX")]
    [SerializeField] AudioClip run;
    [SerializeField] AudioClip jump;
    [SerializeField] AudioClip fall;
    [SerializeField] AudioClip dash;

    [Header("Attacks SFX")]
    [SerializeField] AudioClip attack1;
    [SerializeField] AudioClip attack2;
    [SerializeField] AudioClip attack3;
    [SerializeField] AudioClip screamAttack;

    [Header("Hurt")]
    [SerializeField] AudioClip takeDamage;
    [SerializeField] AudioClip lowHealth;
    [SerializeField] AudioClip death;

    private AudioSource movementSource;
    private AudioSource attackSource;
    private AudioSource hurtSource;

    private void Awake() {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        // Force have at least 3 audio source components
        if (audioSources.Length < 3) {
            Array.Resize(ref audioSources, 3);
            for (int i = audioSources.Length; i < 3; i++)
            {
                audioSources[i] = gameObject.AddComponent<AudioSource>();
            }
        }

        movementSource = audioSources[0];
        attackSource = audioSources[1];
        hurtSource = audioSources[2];
    }

    #region Utility
    public void StopAllAudio() {
        movementSource.Stop();
        attackSource.Stop();
        hurtSource.Stop();
    }
    #endregion

    #region Movement SFX
    public void PlayRunSFX() { movementSource.PlayOneShot(run); }

    public void PlayJumpSFX() { movementSource.PlayOneShot(jump); }

    public void PlayFallSFX() { movementSource.PlayOneShot(fall); }
    
    public void PlayDashSFX() { movementSource.PlayOneShot(dash); }
    #endregion

    #region Attack SFX
    public void PlayAttack1SFX() { attackSource.PlayOneShot(attack1); }
    public void PlayAttack2SFX() { attackSource.PlayOneShot(attack2); }
    public void PlayAttack3SFX() { attackSource.PlayOneShot(attack3); }
    public void PlayScreamAttack() { attackSource.PlayOneShot(screamAttack); }
    #endregion

    #region Hurt SFX
    public void PlayTakeDamage() { hurtSource.PlayOneShot(takeDamage); }
    public void PlayLowHealth() { 
        hurtSource.Stop();
        hurtSource.clip = lowHealth;
        hurtSource.Play();
        hurtSource.loop = true;
    }
    public void PlayDeath() { hurtSource.PlayOneShot(death); }

    #endregion
}
