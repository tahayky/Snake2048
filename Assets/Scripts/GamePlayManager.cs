using System;
using System.Collections;
using Snake2048;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager instance;
    public NavMeshSurface  navMeshSurface;
    public InteractiveCube  interactiveCube;
    private int totalFoodCount = 0;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(FoodSpawnerLoop());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator FoodSpawnerLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            var cube = Instantiate(interactiveCube,GetRandomPositionInsideNavMesh(),GetRandomYRotation()).GetComponent<InteractiveCube>();
            cube.SetNumber(UnityEngine.Random.Range(1,3));
            cube.InflateAnimation(cube.power,cube.enlargeFactor);
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
