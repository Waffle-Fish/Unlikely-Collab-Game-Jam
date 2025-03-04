using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button),typeof(AudioSource))]
public class ButtonSFX : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] AudioClip selectNoise;
    [SerializeField] AudioClip hightlightNoise;
    Button button;
    AudioSource audioSource;
    private void Awake() {
        button = GetComponent<Button>();
        audioSource = GetComponent<AudioSource>();
        button.onClick.AddListener(SelectNoise);
    }

    private void SelectNoise() {
        // audioSource.Stop();
        audioSource.PlayOneShot(selectNoise);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {   
        // audioSource.Stop();
        audioSource.PlayOneShot(hightlightNoise);
    }
}
