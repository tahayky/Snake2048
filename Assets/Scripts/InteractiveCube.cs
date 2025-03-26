using UnityEngine;

namespace Snake2048
{
    public class InteractiveCube : MonoBehaviour, IInteractive
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        public void Interact(ICharacter character)
        {
            character.AddCube();
            Destroy(gameObject);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
