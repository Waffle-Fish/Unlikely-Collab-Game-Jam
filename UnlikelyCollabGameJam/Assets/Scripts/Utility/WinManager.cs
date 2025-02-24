using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinManager : MonoBehaviour
{
    [Tooltip("Goal: Make it to the end of the stage\nTime: Survive until the timer runs out\nElimination: Defeat all mobs")]
    public enum ClearCondition {Goal, Time, Elimination}
    [SerializeField] private ClearCondition levelClearCondition = ClearCondition.Goal;
    [SerializeField] Goal goalObj;
    [Tooltip("How long the player must survive for")]
    [SerializeField] float clearDuration;
    SceneController sceneControllerInstance;

    void Start()
    {
        sceneControllerInstance = SceneController.Instance;
        goalObj.OnPlayerReachedGoal += ProcessWin;

        goalObj.gameObject.SetActive(false);
        switch (levelClearCondition) {
            case ClearCondition.Goal:
                ActivateGoal();
            break;
            case ClearCondition.Time:
                StartCoroutine(ProcessTimeCondition());
            break;
            case ClearCondition.Elimination:
                // ProcessElimination();
            break;
            default:
                ActivateGoal();
            break;
        }
    }

    private void ActivateGoal() {
        goalObj.gameObject.SetActive(true);
    }

    IEnumerator ProcessTimeCondition() {
        float goalTime = Time.time + clearDuration;
        while (Time.time < goalTime) {
            yield return null;
        }
        ActivateGoal();
    }

    // private void ProcessElimination()
    // {
    //      Get Count of all Enemies
    //      Activate Goal when there are no more enemies (count == 0)
    //
    // }

    private void ProcessWin()
    {
        // Play Animation
        int curScene = SceneManager.GetActiveScene().buildIndex;
        if (SceneManager.sceneCountInBuildSettings >= curScene+1) curScene -= 1;
        sceneControllerInstance.LoadScene(curScene + 1);
    }
}
