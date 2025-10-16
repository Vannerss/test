using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
    public class EmptyState : PlayerState
    {
        //When enters state.
        protected override void OnEnter(Player entity)
        {
            throw new System.NotImplementedException();
        }

        //When exits state.
        protected override void OnExit(Player entity)
        {
            throw new System.NotImplementedException();
        }

        //OnUpdate
        protected override void OnStep(Player entity)
        {
            throw new System.NotImplementedException();
        }

        //OnCollision
        public override void OnContact(Player entity, Collider other)
        {
            throw new System.NotImplementedException();
        }
    }
}