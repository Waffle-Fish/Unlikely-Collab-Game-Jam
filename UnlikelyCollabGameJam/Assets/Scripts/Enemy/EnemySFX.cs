using System.Collections.Generic;
using UnityEngine;

public class EnemySFX : MonoBehaviour
{
    [Header("Movement SFX")]
    [SerializeField] AudioClip move;
    [SerializeField] AudioClip move2;
    [SerializeField] AudioClip move3;
    [SerializeField] AudioClip move4;
    [SerializeField] AudioClip fall;
    [SerializeField] AudioClip jump;
    [SerializeField] AudioClip land;

    [Header("Attack SFX")]
    [SerializeField] AudioClip attack;

    [Header("Hurt SFX")]
    [SerializeField] List<AudioClip> takeDamage;
    [SerializeField] List<AudioClip> grunts;
    [SerializeField] AudioClip death;

    [Header("ETC")]
    [SerializeField] float audioRange = 0f;
    float gruntTimer = 0f;

    private AudioSource audioSource;
    private Rigidbody2D rb2d;
    private Transform playerTransform;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
        rb2d = GetComponent<Rigidbody2D>();
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    public void PlayAudioClip(AudioClip audioClip) {
        if (audioClip == null) return;
        if (!audioSource.enabled) return;
        audioSource.Stop();
        audioSource.PlayOneShot(audioClip);
    }

    private void Update() {
        audioSource.enabled = Vector2.Distance(transform.position, playerTransform.position) < audioRange;

        // play grunt at interval
        if (grunts.Count > 0 && Time.time > gruntTimer) {
            gruntTimer = Time.time + Random.Range(10f,30f);
            PlayGruntSFX();
        }
    }

    public void PlayMoveSFX() { PlayAudioClip(move); }
    public void PlayMove2SFX() { PlayAudioClip(move2); }
    public void PlayMove3SFX() { PlayAudioClip(move3); }
    public void PlayMove4SFX() { PlayAudioClip(move4); }
    public void PlayFallSFX() { PlayAudioClip(fall); }
    public void PlayLandSFX() { PlayAudioClip(land); }
    public void PlayJumpSFX() { PlayAudioClip(jump); }
    public void PlayTakeDmgSFX() { PlayAudioClip(takeDamage[Random.Range(0,takeDamage.Count)]); }
    public void PlayGruntSFX() { PlayAudioClip(grunts[Random.Range(0,grunts.Count)]); }
    public void PlayDeathSFX() { PlayAudioClip(death); }
    public void PlayAttackSFX() { PlayAudioClip(attack); }
}
