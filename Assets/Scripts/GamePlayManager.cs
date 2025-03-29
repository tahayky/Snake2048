using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Snake2048;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Snake2048
{
    public class GamePlayManager : MonoBehaviour
    {
        public static GamePlayManager instance;
        public NavMeshSurface navMeshSurface;
        public InteractiveCube interactiveCube;
        public EnemyAIController enemyAIPrefab;
        public PlayerController playerPrefab;
        private int totalFoodCount = 0;
        public float cubeSpawnInterval = 0.5f;
        public int cubesSpawnCount = 10;
        public int maxCubeCount = 100;
        public int maxEnemyCount = 40;
        public List<CharacterBase> enemies;
        private PlayerController player;
        public UIManager uIManager;
        public CinemachineCamera cinemachineCamera;
        private void Awake()
        {
            instance = this;
        }

        public void OnScoreUpdated()
        {
            IScorable[] scorables = new IScorable[enemies.Count+1];
            scorables = enemies.ToArray() as  IScorable[];
            scorables[^1] = player;
            uIManager.UpdateScoreBoard(ref scorables,player);
        }
        void Start()
        {
            SpawnPlayer();
            StartCoroutine(FoodSpawnerLoop());
            for (int i = 0; i < maxEnemyCount; i++)
            {
                CreateEnemyAI();
            }
            SpawnInteractiveCube(maxCubeCount);
        }

        public void CreateEnemyAI()
        {
            var enemy = Instantiate(enemyAIPrefab, GetRandomPositionInsideNavMesh(),Quaternion.identity).GetComponent<EnemyAIController>();
            enemy.OnScoreChanged += OnScoreUpdated;
            enemies.Add(enemy);
            enemy.OnDestroyed += (CharacterBase x) =>
            {
                enemies.Remove(x);
                WaitForSpawnEnemy();
                x.OnScoreChanged-= OnScoreUpdated;
            };
        }

        public void SpawnPlayer()
        {
            var p = Instantiate(playerPrefab, GetRandomPositionInsideNavMesh(),Quaternion.identity).GetComponent<PlayerController>();
            p.OnScoreChanged += OnScoreUpdated;
            player = p;
            
            p.OnDestroyed += (CharacterBase x) =>
            {
                
                x.OnScoreChanged-= OnScoreUpdated;

            };
            cinemachineCamera.Target.TrackingTarget= player.transform;
            cinemachineCamera.Target.LookAtTarget= player.transform;
        }
        private int spawnEnemyQueue = 0;
        public void WaitForSpawnEnemy()
        {
            spawnEnemyQueue++;
        }
        // Update is called once per frame
        void Update()
        {
            if (spawnEnemyQueue > 0)
            {
                for (int i = 0; i < spawnEnemyQueue; i++)
                {
                    CreateEnemyAI();
                }

                spawnEnemyQueue = 0;
            }
        }
        IEnumerator FoodSpawnerLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(cubeSpawnInterval);
                if(totalFoodCount > maxCubeCount)continue;
                SpawnInteractiveCube(cubesSpawnCount);

            }
        }

        public void SpawnInteractiveCube(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var cube = Instantiate(interactiveCube, GetRandomPositionInsideNavMesh(), GetRandomYRotation())
                    .GetComponent<InteractiveCube>();
                cube.SetNumber(UnityEngine.Random.Range(1, 3));
                cube.InflateAnimation(cube.power, cube.enlargeFactor);
                cube.OnDestroyed += () => { totalFoodCount--; };
                totalFoodCount++;
            }
        }
        public static Vector3 GetRandomPositionInsideNavMesh()
        {
            // NavMesh'in tüm üçgen verilerini al
            NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

            // NavMesh'in sınırlarını hesapla (min/max X, Y, Z)
            Vector3 minBounds = navMeshData.vertices[0];
            Vector3 maxBounds = navMeshData.vertices[0];
            foreach (Vector3 vertex in navMeshData.vertices)
            {
                minBounds = Vector3.Min(minBounds, vertex);
                maxBounds = Vector3.Max(maxBounds, vertex);
            }

            // NavMesh sınırları içinde rastgele bir nokta üret
            Vector3 randomPoint = new Vector3(
                Random.Range(minBounds.x, maxBounds.x),
                Random.Range(minBounds.y, maxBounds.y),
                Random.Range(minBounds.z, maxBounds.z)
            );

            // Bu noktanın NavMesh üzerinde olup olmadığını kontrol et
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1f, NavMesh.AllAreas))
            {
                return hit.position;
            }
            else
            {
                // Fallback: Eski yöntemle tekrar dene
                return GetRandomPositionFallback();
            }
        }
        
        private static Vector3 GetRandomPositionFallback()
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(
                    Random.insideUnitSphere * 100f,
                    out hit,
                    100f,
                    NavMesh.AllAreas))
            {
                return hit.position;
            }

            return Vector3.zero;
        }

        // Rastgele Y rotasyonlu Quaternion üretir (X ve Z sabit)
        public static Quaternion GetRandomYRotation()
        {
            float randomYAngle = Random.Range(0f, 360f); // Y ekseni için 0-360 derece
            return Quaternion.Euler(0f, randomYAngle, 0f); // X=0, Y=random, Z=0
        }
    }
}
