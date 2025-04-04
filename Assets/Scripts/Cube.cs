using System;
using System.Numerics;
using UnityEngine;
using DG.Tweening;
namespace Snake2048
{
    public class Cube : MonoBehaviour,IInteractive
    {
        public MaterialList materialList;
        public NumberAbbreviation numberAbbreviation;
        // Mesh Renderer referansı
        public MeshRenderer meshRenderer;

        public CharacterBase Owner { get; set; }
        public BoxCollider boxCollider;
        public bool MarkedForDestroy { get; set; }
        public virtual void Initialize()
        {
            boxCollider = GetComponent<BoxCollider>();
        }

        // Bu fonksiyon ile verilen index'e göre material atanır
        public void SetMaterialByIndex(int index)
        {
            if (materialList == null || materialList.materials.Length == 0 || meshRenderer == null)
                return;
        
            // Döngüsel index hesaplama (array sınırlarını aşsa bile başa döner)
            int actualIndex = index % materialList.materials.Length;
        
            // Negatif index'ler için düzeltme
            if (actualIndex < 0)
                actualIndex += materialList.materials.Length;
        
            // Material'i ata
            meshRenderer.material = materialList.materials[actualIndex];
            
        }
        void Start()
        {

        }
        protected BigInteger _number;
        public int power = 1;
        public float enlargeFactor=0.1f;
        public void SetNumber(int pow)
        {
            _number=BigInteger.Pow(2,pow);
            power = pow;
            numberAbbreviation.SetNumber(_number.ToString());
            SetMaterialByIndex(pow-1);
        }
        public void InflateAnimation(int newPower,float factor)
        {
            float _newPower = 1 + newPower * factor;
            transform.DOScale((_newPower)+0.2f, 0.1f).Play().OnComplete(() =>
                transform.DOScale(_newPower, 0.1f).Play()
            );
        }
        public BigInteger GetNumber()
        {
            return _number;
        }

        public void Power()
        {
            SetNumber(power+1);
            InflateAnimation(power,enlargeFactor);
        }

        private void Update()
        {
            if(MarkedForDestroy)Destroy(this.gameObject);
        }

        public virtual void Interact(CharacterBase character)
        {
            if(MarkedForDestroy) return;
            if(character.Size<power) return;
            MarkedForDestroy = true;
            character.AddCube(power);
            Owner.WaitForBreakTail(this);
        }
        
    }
}