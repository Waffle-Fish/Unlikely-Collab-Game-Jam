using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSFXManager : MonoBehaviour
{
    [Header("Movement SFX")]
    [SerializeField] List<AudioClip> run;
    [SerializeField] AudioClip jump;
    [SerializeField] AudioClip fall;
    [SerializeField] AudioClip dash;
    [SerializeField] AudioClip land;
    bool isRun = false;

    [Header("Attacks SFX")]
    [SerializeField] AudioClip attack1;
    [SerializeField] AudioClip attack2;
    [SerializeField] AudioClip attack3;
    [SerializeField] AudioClip screamAttack;
    [SerializeField] AudioClip fireballCharge;
    [SerializeField] AudioClip fireballAttack;

    [Header("Hurt")]
    [SerializeField] AudioClip takeDamage;
    [SerializeField] AudioClip lowHealth;
    [SerializeField] AudioClip death;

    private AudioSource movementSource;
    private AudioSource attackSource;
    private AudioSource hurtSource;

    private void Awake() {
        List<AudioSource> audioSources = new();
        GetComponents<AudioSource>(audioSources);

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
    public void PlayFromMovement(AudioClip audioClip) {
        movementSource.Stop();
        movementSource.PlayOneShot(audioClip);
        isRun = false;
    }
    public void PlayRunSFX() { 
        isRun = true;
        StartCoroutine(PlayRunTrack());
    }

    IEnumerator PlayRunTrack() {
        int i = 0;
        while (isRun) {
            movementSource.Stop();
            movementSource.PlayOneShot(run[i]);
            yield return new WaitForSeconds(run[i].length);
            i = (i+1) % run.Count;
        }
    }

    public void PlayJumpSFX() { PlayFromMovement(jump); }

    public void PlayFallSFX() { PlayFromMovement(fall); }
    
    public void PlayDashSFX() { PlayFromMovement(dash); }
    public void PlayLandSFX() { PlayFromMovement(land); }
    #endregion

    #region Attack SFX
    public void PlayFromAttack(AudioClip audioClip) {
        attackSource.Stop();
        attackSource.PlayOneShot(audioClip);
    }
    public void PlayAttack1SFX() { PlayFromAttack(attack1); }
    public void PlayAttack2SFX() { PlayFromAttack(attack2); }
    public void PlayAttack3SFX() { PlayFromAttack(attack3); }
    public void PlayScreamAttack() { PlayFromAttack(screamAttack); }
    #endregion

    #region Hurt SFX
    public void PlayFromHurt(AudioClip audioClip) {
        hurtSource.Stop();
        hurtSource.PlayOneShot(audioClip);
    }
    public void PlayTakeDamage() { PlayFromHurt(takeDamage); }
    public void PlayLowHealth() { 
        hurtSource.Stop();
        hurtSource.clip = lowHealth;
        hurtSource.Play();
        hurtSource.loop = true;
    }
    public void PlayDeath() { PlayFromHurt(death); }

    #endregion
}
