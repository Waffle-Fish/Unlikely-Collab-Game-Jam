using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set;}
    private void Awake() {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void LoadScene(int buildIndex) {
        Time.timeScale = 1;
        SceneManager.LoadScene(buildIndex);
    }

    public void LoadScene(int buildIndex, float delay) {
        Time.timeScale = 1;
        StartCoroutine(DelayLoadScene(buildIndex, delay));
    }

    IEnumerator DelayLoadScene(int buildIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene((buildIndex));
    }

    public void Quit() {
        Application.Quit();
    }
}
