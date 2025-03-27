using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
namespace Snake2048
{
    public class PlayerController : MonoBehaviour, IInteractor
    {
        Rigidbody rb;
        public Cube mainCube;
        public float speed;
        public float turboFactor;
        private float currentSpeed;
        private Vector3 direction3D;
        public List<Cube> cubes;
        private Trail _trail;
        public Cube cubePrefab;
        public AudioSource eatingSource;
        public AudioSource deathSource;
        public AudioSource mergeSource;
        BoxCollider boxCollider;
        public int Size
        {
            get => mainCube.power;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            _trail = GetComponent<Trail>();
            boxCollider=GetComponent<BoxCollider>();
        }

        private void Start()
        {
            mainCube.SetNumber(1);
            mainCube.InflateAnimation(1,mainCube.enlargeFactor);
            currentSpeed = speed;
            StartCoroutine(MergeCoroutine());
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
            float sum = 0;
            for (int i=0; i<cubes.Count; i++)
            {

                float dist = cubes[i].transform.localScale.x/(_trail.fixedDistance);
                if (i == 0) sum += mainCube.transform.localScale.x / (_trail.fixedDistance * 2f);
                else sum += cubes[i-1].transform.localScale.x / (_trail.fixedDistance * 2f);
                sum += dist/2;
                cubes[i].transform.SetPositionAndRotation(_trail.GetPositionAtDistance(sum),_trail.GetRotationAtDistance(sum));
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
            sum+=mainCube.transform.localScale.x;
            return sum;
        }
        private void OnCollisionEnter(Collision other)
        {
            if (other.collider.CompareTag("Interactable"))
            {
                other.gameObject.GetComponent<IInteractive>().Interact(this);
            }
        }

        public void MergeCubes()
        {
            List<Cube> cubesForDestroy = new List<Cube>();
            for (var index = 0; index < cubes.Count; index++)
            {
                Cube previousCube =null;
                Cube nextCube=null;
                if(index!=0) previousCube = cubes[index-1];
                else previousCube = mainCube;
                if(index!=cubes.Count-1) nextCube = cubes[index+1];
                var cube = cubes[index];
                if (previousCube!=null && cube.GetNumber() == previousCube.GetNumber())
                {
                    Debug.Log("BOING THE PREVIOUS CUBE");
                    RemoveCube(cube);
                    previousCube.Power();
                    mergeSource.Play();
                    cubesForDestroy.Add(cube);
                }
                else if (nextCube != null && cube.GetNumber() == nextCube.GetNumber())
                {
                    Debug.Log("BOING THE NEXT CUBE");
                    RemoveCube(nextCube);
                    cube.Power();
                    mergeSource.Play();
                    cubesForDestroy.Add(nextCube);

                }
                
            }

            for (int i = 0; i < cubesForDestroy.Count; i++)
            {
                Destroy(cubesForDestroy[i].gameObject);

            }
        }

        IEnumerator MergeCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                MergeCubes();
            }
        }
        public void AddCube(int pow)
        {
            var cube = Instantiate(cubePrefab,transform.position,Quaternion.identity);
            cube.SetNumber(pow);
            cube.InflateAnimation(cube.power,cube.enlargeFactor);
            // Binary Search ile doğru pozisyonu bul
            int index = cubes.BinarySearch(cube, Comparer<Cube>.Create((a, b) =>b.power.CompareTo(a.power)));
            
            if (index < 0)
            {
                index = ~index; // Ekleme pozisyonunu hesapla
            }

            // Listeye ekle
            cubes.Insert(index, cube);

            SetTrailProperties();
            mainCube.InflateAnimation(mainCube.power,mainCube.enlargeFactor);
            InflateBoxCollider();
            eatingSource.Play();
        }

        void InflateBoxCollider()
        {
            boxCollider.size = Vector3.one + Vector3.one*mainCube.enlargeFactor*mainCube.power;
            boxCollider.center = Vector3.up/2 + Vector3.up*mainCube.enlargeFactor*mainCube.power/2;
        }
        public void RemoveCube(Cube cube)
        {
            cubes.Remove(cube);
        }
    }
}
