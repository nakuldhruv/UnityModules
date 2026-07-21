using UnityEngine;

namespace Nakul.DesignPattern
{
    public class Player : MonoBehaviour
    {
        public StateMachine StateMachine { get; set; }
        public PlayerIdleState IdleState { get; set; }
        public PlayerWalkState WalkState { get; set; }

        private void Awake()
        {
            IdleState = new PlayerIdleState(this);
            WalkState = new PlayerWalkState(this);
            StateMachine = new StateMachine(IdleState);
        }

        private void Update()
        {
            StateMachine?.Update();
        }
    }
}