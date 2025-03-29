using UnityEngine;
namespace Snake2048
{
    public interface IInteractive
    {
        public bool MarkedForDestroy { get; set; }
        public void Interact(CharacterBase character);
    }
    public interface IScorable
    {
        public string Name { get; }
        public float Score { get; }
    }
}
