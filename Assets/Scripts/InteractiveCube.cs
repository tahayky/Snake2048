using UnityEngine;

namespace Snake2048
{
    public class InteractiveCube : Cube, IInteractive
    {

        public void Interact(IInteractor character)
        {
            if(character.Size<power) return;
            character.AddCube(power);
            Destroy(gameObject);
        }
        
    }
}
