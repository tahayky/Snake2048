using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
namespace Snake2048
{
    public class PlayerController : MonoBehaviour, ICharacter
    {
        Rigidbody rb;
        public float speed;
        public float turboFactor;
        private float currentSpeed;
        private Vector3 direction3D;
        public List<Cube> cubes;
        private Trail _trail;
        public Cube cubePrefab;
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            _trail = GetComponent<Trail>();
        }

        private void Start()
        {
            currentSpeed = speed;
        }

        public void Move(InputAction.CallbackContext  context)
        {
            Vector2 mousePosition = context.ReadValue<Vector2>();
            Vector2 centeredMousePosition = new Vector2(
                mousePosition.x - Screen.width / 2f,
                mousePosition.y - Screen.height / 2f
            );
            direction3D = new Vector3(centeredMousePosition.x, 0, centeredMousePosition.y);
        }

        public void Turbo(InputAction.CallbackContext  context)
        {
            if (context.started)
            {
                currentSpeed = speed*turboFactor;
            }else if(context.canceled)
            {
                currentSpeed = speed;
            }
        }
        private void FixedUpdate()
        {
            rb.rotation = Quaternion.LookRotation(direction3D.normalized,Vector3.up);

            rb.linearVelocity = transform.forward*currentSpeed;
            

        }

        private void LateUpdate()
        {
            MoveTail();
        }
        
        private void MoveTail()
        {
            for (int i=0; i<cubes.Count; i++)
            {
                float index = cubes[i].transform.localScale.x/_trail.fixedDistance;
               cubes[i].transform.SetPositionAndRotation(_trail.GetPositionAtDistance(index*(i+1)),_trail.GetRotationAtDistance(index*(i+1)));
            }
        }

        private void SetTrailProperties()
        {
            float step = 0.5f;

            _trail.fixedDistance = step;
            _trail.maxPoints = Mathf.FloorToInt(GetTrailSize()/step)+1;
        }
        private float GetTrailSize()
        {
            float sum = 0;
            foreach (var cube in cubes)
            {
                sum += cube.transform.localScale.x;
            }

            return sum;
        }
        private void OnCollisionEnter(Collision other)
        {
            if (other.collider.CompareTag("Interactable"))
            {
                other.gameObject.GetComponent<IInteractive>().Interact(this);
            }
        }
        
        public void AddCube()
        {
            var cube = Instantiate(cubePrefab,transform.position,Quaternion.identity);
            cubes.Insert(0,cube);
            SetTrailProperties();
        }
    }
}
