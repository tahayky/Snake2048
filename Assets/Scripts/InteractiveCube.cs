using System;
using UnityEngine;
using UnityEngine.Events;

namespace Snake2048
{
    public class InteractiveCube : Cube
    {
        public UnityAction OnDestroyed;
        public override void Interact(CharacterBase character)
        {
            if(character.Size<power) return;
            character.AddCube(power);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            OnDestroyed?.Invoke();  
        }
    }
}
