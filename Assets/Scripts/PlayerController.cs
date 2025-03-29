using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
namespace Snake2048
{
    public class PlayerController : CharacterBase
    {

  
        public AudioSource eatingSource;
        public AudioSource deathSource;
        public AudioSource mergeSource;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            CanvasNicknameManager.Instance.AddNickname(
                transform,      // Target (karakter transformu)
                gameObject.name,      // Nickname metni
                Color.white     // Renk
            );
        }

        public void Move(InputAction.CallbackContext  context)
        {
            Vector2 mousePosition = context.ReadValue<Vector2>();
            Vector2 centeredMousePosition = new Vector2(
                mousePosition.x - Screen.width / 2f,
                mousePosition.y - Screen.height / 2f
            );
            Move(new Vector3(centeredMousePosition.x, 0, centeredMousePosition.y));
        }
        
        public void Turbo(InputAction.CallbackContext  context)
        {
            if (context.started) 
                Turbo(true);
            else if(context.canceled)
                Turbo(false);
            
        }
        
        protected override void FixedUpdate()
        {
            base.FixedUpdate();

        }

        protected override void LateUpdate()
        {
           base.LateUpdate();
        }
        
        protected override void OnCollisionEnter(Collision other)
        {
            base.OnCollisionEnter(other);
        }
        public override void AddCube(int pow)
        {
            base.AddCube(pow);
            eatingSource.Play();
        }

        public override bool MergeCubes()
        {
            var merged = base.MergeCubes();
            if (merged)
            {
                mergeSource.Play();
                return true;
            }

            return false;
        }
    }
}
