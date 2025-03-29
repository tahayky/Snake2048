using UnityEngine;

namespace Snake2048
{

    [CreateAssetMenu(fileName = "MaterialList", menuName = "Snake2048/MaterialList", order = 1)]
    public class MaterialList : ScriptableObject
    {
        public Material[] materials;
    }
}