using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(Collider2D))]
public class AreaTriggerTimeline : MonoBehaviour
{
    [SerializeField] PlayableDirector playableDirector;
    [SerializeField] bool onlyOnce = true;
    bool activated = false;
    private void Start() {
        if (!playableDirector) {
            Debug.LogWarning("No Playable Director Found");
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (onlyOnce && activated) return;
        if (!collision.CompareTag("Player")) return;
        playableDirector.gameObject.SetActive(true);
        playableDirector.Play();
        activated = true;
    }
}
