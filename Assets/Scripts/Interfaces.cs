using UnityEngine;
namespace Snake2048
{
    public interface IInteractive
    {
        public void Interact(IInteractor character);
    }
    public interface IInteractor
    {
        public int Size { get; }
        public void AddCube(int pow);
    }
}
