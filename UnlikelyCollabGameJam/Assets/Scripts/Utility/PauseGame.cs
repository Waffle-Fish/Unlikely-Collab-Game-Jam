using UnityEngine;

public class PauseGame : MonoBehaviour
{
    public PauseGame Instance {get; private set;}
    private void Awake() {
          if (Instance != null && Instance != this) Destroy(this);
          else Instance = this;
    }

    public void Pause() {
        Time.timeScale = 0;
    }

    public void Resume() {
        Time.timeScale = 1;
    }
}
