using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(Collider2D))]
public class Goal : MonoBehaviour
{
    public event Action OnPlayerReachedGoal;
    [SerializeField] PlayableDirector playableDirector;
    private void Start() {
        if (!playableDirector) {
            Debug.LogWarning("No Playable Director Found");
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playableDirector.Play();
        // OnPlayerReachedGoal?.Invoke();
    }
}
