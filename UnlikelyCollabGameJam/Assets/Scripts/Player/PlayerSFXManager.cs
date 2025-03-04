using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFXManager : MonoBehaviour
{
    [Header("Movement SFX")]
    [SerializeField] List<AudioClip> run;
    [SerializeField] AudioClip jump;
    [SerializeField] AudioClip fall;
    [SerializeField] AudioClip dash;
    [SerializeField] AudioClip land;
    public enum MoveAudioClips {none,run, jump, fall, dash, land};
    private MoveAudioClips currentMoveClip = MoveAudioClips.none;
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
    private List<AudioSource> audioSources;
    private Rigidbody2D rb2d;

    private void Awake() {
        audioSources = new();
        GetComponents<AudioSource>(audioSources);
        rb2d = GetComponent<Rigidbody2D>();

        movementSource = audioSources[0];
        attackSource = audioSources[1];
        hurtSource = audioSources[2];
    }

    void Update()
    {
        if (rb2d.linearVelocity == Vector2.zero) currentMoveClip = MoveAudioClips.none;
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
        StopCoroutine(nameof(StopRunSFX));
        movementSource.PlayOneShot(audioClip);
        isRun = false;
    }
    public void PlayRunSFX() { 
        if (isRun) return;
        isRun = true;
        StartCoroutine(PlayRunTrack());
    }

    public void StopRunSFX() { 
        isRun = false;
        movementSource.Stop();
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

    public void PlayFallSFX() {
        if (currentMoveClip == MoveAudioClips.fall) return;
        currentMoveClip = MoveAudioClips.fall;
        PlayFromMovement(fall);
    }
    
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
