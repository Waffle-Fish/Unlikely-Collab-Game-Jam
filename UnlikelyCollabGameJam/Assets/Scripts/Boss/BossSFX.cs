using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSFX : MonoBehaviour
{
    [SerializeField] List<AudioClip> stabs;
    [SerializeField] AudioClip fireBall1;
    [SerializeField] AudioClip fireBall2;
    [SerializeField] AudioClip fireBallShoot;
    [SerializeField] AudioClip scream;
    [SerializeField] List<AudioClip> takeDamage;
    [SerializeField] AudioClip rage;


    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource damageSource;

    bool dmgClipIsPlaying = false;

    public void PlayAudioClip(AudioClip audioClip) {
        if (audioClip == null) return;
        if (!audioSource.enabled) return;
        audioSource.Stop();
        audioSource.PlayOneShot(audioClip);
    }

    public void PlayStabsSFX() { PlayAudioClip(stabs[Random.Range(0, stabs.Count)]); }
    public void PlayFireBall1SFX() { PlayAudioClip(fireBall1); }
    public void PlayFireBall2SFX() { PlayAudioClip(fireBall2); }
    public void PlayFireBallShootSFX() { PlayAudioClip(fireBallShoot); }
    public void PlayScreamSFX() { PlayAudioClip(scream); }
    public void PlayRageSFX() { 
        damageSource.Stop();
        damageSource.PlayOneShot(rage);
    }
    public void PlayTakeDamageSFX() { 
        if (dmgClipIsPlaying) return;
        StartCoroutine(PlayDmgClip());
        
        IEnumerator PlayDmgClip() {
            dmgClipIsPlaying = true;
            AudioClip dmgClip = takeDamage[Random.Range(0, takeDamage.Count)];
            damageSource.PlayOneShot(dmgClip);
            yield return new WaitForSeconds(dmgClip.length);
            dmgClipIsPlaying = false;
        }
    }
}
