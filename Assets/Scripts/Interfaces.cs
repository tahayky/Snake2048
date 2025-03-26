using UnityEngine;
namespace Snake2048
{
    public interface IInteractive
    {
        public void Interact(ICharacter character);
    }
    public interface ICharacter
    {
        public void AddCube();
    }
}
