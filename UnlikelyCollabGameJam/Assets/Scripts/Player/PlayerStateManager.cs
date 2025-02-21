using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public enum State { Jumping, Falling, Grounded, Dashing }
    public State CurrentState = State.Grounded;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
