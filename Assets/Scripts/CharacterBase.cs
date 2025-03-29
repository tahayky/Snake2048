using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Snake2048
{
    public class CharacterBase : MonoBehaviour, IInteractive, IScorable
    {
        protected Rigidbody rb;
        public ParticleSystem particle;
        public float turboDuration = 10;
        public float Y;
        
        //Olmas gereken hız
        public float speed;
        
        //Turbo hız faktörü
        public float turboFactor;
        
        //Baş bölgesi küpü
        public Cube mainCube;
        
        //Anlık hız
        protected float currentSpeed;
        
        //Yılanın gittiği yön
        protected Vector3 direction3D;
        
        //Küp listesi
        public List<Cube> cubes;
        
        //Küplerin dizileceği noktaları işaretleyen bir iz/trail scripti.
        protected Trail _trail;
        
        //Eklenecek küp prefabı 
        public Cube cubePrefab;
        
        //Ana küpün collideri
        BoxCollider boxCollider;

        public Cube replacementPrefab;
        public float rotationSpeed = 10f;
        public UnityAction<CharacterBase> OnDestroyed;
        public bool MarkedForDestroy { get; set; }
        protected float turboTimer=0;
        protected bool onTurbo = false;
        public UnityAction OnScoreChanged;
        //Ana kübün üzerindeki sayı
        public int Size
        {
            get => mainCube.power;
        }
        public float Score
        {
            get => score;
        }
        public string Name
        {
            get => gameObject.name;
        }
        protected float score;
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            _trail = GetComponent<Trail>();
            boxCollider=GetComponent<BoxCollider>();
        }
        protected virtual void Start()
        {
            //Ana kübün sayısını 2 üzeri 1 olarak işaretliyoruz
            mainCube.SetNumber(1);
            
            //Küpü olması gereken boyuta getiriyoruz
            mainCube.InflateAnimation(1,mainCube.enlargeFactor);
            UpdateScore();
            //anlık hızı olması gereken hıza eşitliyoruz
            currentSpeed = speed;
            
            //Kuyruğumuzdaki küpleri merge etmek için bir loop başlatıyoruz
            StartCoroutine(MergeCoroutine());
            
            
        }

        public virtual void UpdateScore()
        {
            InflateBoxCollider();
            score = Mathf.Pow(2, Size);
            OnScoreChanged?.Invoke();
        }
        protected virtual void FixedUpdate()
        {
            
            rb.rotation = Quaternion.Lerp(rb.rotation,Quaternion.LookRotation(direction3D.normalized,Vector3.up),Time.fixedDeltaTime*rotationSpeed);

            
            //Durmaksızın gitmesi için
            Vector3 velocity = transform.forward*currentSpeed;
            velocity.y = 0;
            rb.linearVelocity = velocity;
            
            transform.position =  new Vector3(rb.position.x,Y,rb.position.z);

        }
        protected virtual void LateUpdate()
        {
            if(MarkedForDestroy) Destroy(gameObject);
            if (brokenTail)
            {
                CleanupMissingCubes();
                brokenTail = false;
            }

            //Listedeki küpleri sıraya dizip kuyruk yaratıyoruz
            MoveTail();
        }
        protected void CleanupMissingCubes()
        {
            // RemoveAll metodu tek bir geçişte tüm null/missing küpleri kaldırır
            int removedCount = cubes.RemoveAll(c => c == null);
    
            // Sadece küp silindiyse trail properties'i güncelle
            if (removedCount > 0)
                SetTrailProperties();
        }
        protected virtual void Update()
        {
            if (onTurbo)
            {
                if (turboTimer > turboDuration)
                {
                    Turbo(false);
                }
                else
                {
                    turboTimer += Time.deltaTime;
                }
            }
            else if (turboTimer!=0)
            {
                turboTimer-= Time.deltaTime;
                turboTimer = Mathf.Clamp(turboTimer, 0, turboDuration);
            }
            
        }

        //direction3D değişkenini set eden method
        protected virtual void Move(Vector3 direction)
        {
            direction3D = direction;
        }
        
        public virtual void Turbo(bool state)
        {
            CustomTurbo(state, turboFactor);
            if(state) particle.Play();
            else  particle.Stop();
        }
        public virtual void CustomTurbo(bool state,float factor)
        {
            if(state) currentSpeed = speed*factor;
            else currentSpeed = speed;
            onTurbo=state;
        }
        
        //Kuyruk için küplerin büyüklüklerine göre bir dizilim yapıyoruz
        protected virtual void MoveTail()
        { 
            if(MarkedForDestroy) return;
            float sum = 0;
            for (int i=0; i<cubes.Count; i++)
            {
                if (i == 0) sum += mainCube.transform.localScale.x / (_trail.fixedDistance * 2f);
                else
                {
                    if (cubes[i-1] != null && !cubes[i-1].MarkedForDestroy)
                    {
                        sum += cubes[i - 1].transform.localScale.x / (_trail.fixedDistance * 2f);
                    }

                }
                if(cubes[i]==null||cubes[i].MarkedForDestroy)continue;
                float dist = cubes[i].transform.localScale.x/(_trail.fixedDistance);
                sum += dist/2;
                cubes[i].transform.SetPositionAndRotation(_trail.GetPositionAtDistance(sum),_trail.GetRotationAtDistance(sum));
            }
        }
        
        
        //İzi/Traili küp sayımıza göre konfigüre ediyoruz
        protected virtual void SetTrailProperties()
        {
            float step = 0.5f;

            _trail.fixedDistance = step;
            _trail.maxPoints = Mathf.FloorToInt(GetTrailSize()/step)+1;
        }
        
        //Küp sayısına göre kuruğun olması gereken uzunluğu buluyoruz
        protected virtual float GetTrailSize()
        {
            if(MarkedForDestroy) return 0;
            float sum = 0;
            foreach (var cube in cubes)
            {
                if(cube==null||cube.MarkedForDestroy)continue;
                sum += cube.transform.localScale.x;
            }
            sum+=mainCube.transform.localScale.x;
            return sum;
        }
        
        //Yeni küp almak için çarpışma algılaması
        protected virtual void OnCollisionEnter(Collision other)
        {
            if (other.collider.CompareTag("Interactable"))
            {
                var interact = other.gameObject.GetComponent<IInteractive>();
                if(interact.MarkedForDestroy) return;
                interact.Interact(this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Interactable"))
            {
                var interact = other.gameObject.GetComponent<IInteractive>();
                if(interact.MarkedForDestroy) return;
                interact.Interact(this);
            }
        }

        //Küpleri merge eden kod
        public virtual bool MergeCubes()
        {
            bool merged = false;
            List<Cube> cubesForDestroy = new List<Cube>();
            for (var index = 0; index < cubes.Count; index++)
            {
                if(cubes[index]==null||cubes[index].MarkedForDestroy)continue;
                Cube previousCube =null;
                Cube nextCube=null;
                if(index!=0) previousCube = cubes[index-1];
                else previousCube = mainCube;
                if(index!=cubes.Count-1) nextCube = cubes[index+1];
                var cube = cubes[index];
                if (previousCube!=null && cube.GetNumber() == previousCube.GetNumber())
                {
                    RemoveCube(cube);
                    previousCube.Power();
                    UpdateScore();
                    merged = true;
                    cubesForDestroy.Add(cube);
                }
                else if (nextCube != null && cube.GetNumber() == nextCube.GetNumber())
                {
                    RemoveCube(nextCube);
                    cube.Power();
                    merged = true;
                    cubesForDestroy.Add(nextCube);

                }
                
            }

            for (int i = 0; i < cubesForDestroy.Count; i++)
            {
                Destroy(cubesForDestroy[i].gameObject);

            }

            return merged;
        }
        
        protected virtual IEnumerator MergeCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                MergeCubes();
            }
        }
        
        //Dışarıdan alınan her kübü listeye ve kuruğa eklemek için bir method
        public virtual void AddCube(int pow)
        {
            var cube = Instantiate(cubePrefab,transform.position,Quaternion.identity);
            cube.Initialize();
            Physics.IgnoreCollision(boxCollider, cube.boxCollider);
            cube.SetNumber(pow);
            cube.Owner = this;
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
        }
        
        //Box collider'ı ana küp meshine uygun şekilde resize eden kod
        protected virtual void InflateBoxCollider()
        {
            boxCollider.size = Vector3.one + Vector3.one*mainCube.enlargeFactor*mainCube.power;
            boxCollider.center = Vector3.up/2 + Vector3.up*mainCube.enlargeFactor*mainCube.power/2;
            particle.transform.localScale = Vector3.one + Vector3.one * mainCube.enlargeFactor * mainCube.power;
        }
        public virtual void RemoveCube(Cube cube)
        {
            cubes.Remove(cube);
            SetTrailProperties();
        }
        public virtual void WaitForBreakTail(Cube startCube)
        {
            BreakTail(startCube);
        }

        private bool brokenTail = false;
        /// <summary>
        /// Verilen küp ve sonrasını kuyruğu koparır. Verilen küp null ise tüm kuyruk kopar.
        /// Kopan küpler yerine yeni prefablar yerleştirilir, ancak bu prefablar listeye eklenmez.
        /// </summary>
        /// <param name="startCube">Bu küpten (dahil) sonra kuyruk koparılır, null ise tüm kuyruk koparılır</param>
        /// <param name="replacementPrefab">Kopan küpler yerine konulacak prefab</param>
        public virtual void BreakTail(Cube startCube)
        {
            int startIndex = 0;
            
            if (startCube != null)
            {
                // Eğer verilen başlangıç küpü listede yoksa işlem yapma
                if (!cubes.Contains(startCube))
                    return;
                
                // Verilen küpün indeksini bul
                startIndex = cubes.IndexOf(startCube);
            }
            brokenTail=true;
            // Eğer startCube null ise, startIndex 0 olarak kalır ve tüm küpler işlenir
            
            // Silmeden önce seçilen indeksten sonraki küplerin bilgilerini kaydet
            List<CubeInfo> cubeInfos = new List<CubeInfo>();
            for (int i = startIndex; i < cubes.Count; i++)
            {
                if(cubes[i] == null || cubes[i].MarkedForDestroy) continue;
                cubeInfos.Add(new CubeInfo {    
                    position = cubes[i].transform.position,
                    rotation = cubes[i].transform.rotation,
                    power = cubes[i].power
                });
            }
            
            // Seçilen indeksten sonraki tüm küpleri listeden çıkar ve yok et
            List<Cube> cubesToRemove = cubes.GetRange(startIndex, cubes.Count - startIndex);
            foreach (Cube cube in cubesToRemove)
            {
                if(cube == null || cube.MarkedForDestroy) continue;
                cube.MarkedForDestroy = true;
            }
            
            // Kaydedilen bilgilere göre kopan küplerin yerine yeni küpler oluştur
            foreach (var info in cubeInfos)
            {
                var newCube = Instantiate(replacementPrefab, info.position, info.rotation);
                newCube.SetNumber(info.power);
                newCube.InflateAnimation(newCube.power, newCube.enlargeFactor);
                // Not: Bu küpler listeye eklenmediği için artık yılanın parçası olmayacak
            }
            
            // Trail ve collider'ı güncelle
            SetTrailProperties();
        }

        // Küp bilgilerini saklamak için yardımcı sınıf
        private class CubeInfo
        {
            public Vector3 position;
            public Quaternion rotation;
            public int power;
        }

        public void Interact(CharacterBase character)
        {
            if(MarkedForDestroy)return;
            if(character.Size<=Size) return;
            MarkedForDestroy = true;
            character.AddCube(mainCube.power);
            BreakTail(null);
        }

        protected virtual void OnDestroy()
        {
            OnDestroyed?.Invoke(this);
            if (CanvasNicknameManager.Instance != null)
            {
                CanvasNicknameManager.Instance.RemoveNickname(transform);
            }
        }
    }
}
