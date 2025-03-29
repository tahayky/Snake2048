using UnityEngine;
using System.Collections.Generic;

namespace Snake2048
{
    public class Trail : MonoBehaviour
    {
        public float fixedDistance = 1.0f;
        public int maxPoints = 100;
        private List<Vector3> positions;
        private List<Quaternion> rotations;
        private Vector3 lastPosition;
        public bool showDebug = true;

        void Start()
        {
            positions = new List<Vector3>(maxPoints);
            rotations = new List<Quaternion>(maxPoints);
            lastPosition = transform.position;
            AddPoint(transform.position, transform.rotation);
        }

        void Update()
        {
            float distance = Vector3.Distance(transform.position, lastPosition);
            if (distance >= fixedDistance)
            {
                Vector3 direction = (transform.position - lastPosition).normalized;
                int points = Mathf.FloorToInt(distance / fixedDistance);
                for (int i = 0; i < points; i++)
                {
                    Vector3 newPos = lastPosition + direction * fixedDistance;
                    Quaternion newRot = Quaternion.LookRotation(direction);
                    AddPoint(newPos, newRot);
                    lastPosition = newPos;
                }
            }
        }

        void AddPoint(Vector3 position, Quaternion rotation)
        {
            // Listenin başına ekleme yaparak en yeni nokta hep index 0'da olur
            positions.Insert(0, position);
            rotations.Insert(0, rotation);
            
            // Maksimum nokta sayısını aşarsak listenin sonundan eleman çıkar
            while (positions.Count > maxPoints)
            {
                positions.RemoveAt(positions.Count - 1);
                rotations.RemoveAt(rotations.Count - 1);
            }
        }

        public Vector3 GetPositionAtDistance(float requestedDistance)
        {
            if (positions.Count <= 1)
                return transform.position;
                
            float firstDistance = Vector3.Distance(transform.position, lastPosition);
            float interpolation = firstDistance / fixedDistance;
            requestedDistance -= interpolation;
            
            // Mesafe sonrası indeksi hesapla (2.7 -> indeks 2)
            int baseIndex = Mathf.FloorToInt(requestedDistance);
            
            // Tam bir pozisyon ise
            if (Mathf.Approximately(requestedDistance, baseIndex))
                return GetPositionAtIndex(baseIndex);
                
            // Kesirli kısmı al (2.7 -> 0.7)
            float fraction = requestedDistance - baseIndex;
            
            // İki pozisyonu al ve direkt Lerp yap
            return Vector3.Lerp(
                GetPositionAtIndex(baseIndex),
                GetPositionAtIndex(baseIndex + 1),
                fraction
            );
        }

        public Quaternion GetRotationAtDistance(float requestedDistance)
        {
            if (positions.Count <= 1)
                return transform.rotation;
                
            float firstDistance = Vector3.Distance(transform.position, lastPosition);
            float interpolation = firstDistance / fixedDistance;
            requestedDistance -= interpolation;
            int baseIndex = Mathf.FloorToInt(requestedDistance);
            float fraction = requestedDistance - baseIndex;
            
            return Quaternion.Slerp(
                GetRotationAtIndex(baseIndex),
                GetRotationAtIndex(baseIndex + 1),
                fraction
            );
        }

        public Vector3 GetPositionAtIndex(int index)
        {
            if (index < 0)
                return transform.position;
            if (index >= positions.Count)
                return positions[positions.Count - 1];
                
            return positions[index]; // Doğrudan erişim - başa ekleme yapıldığından en yeni nokta index 0'dadır
        }

        public Quaternion GetRotationAtIndex(int index)
        {
            if (index < 0)
                return transform.rotation;
            if (index >= rotations.Count)
                return rotations[rotations.Count - 1];
                
            return rotations[index]; // Doğrudan erişim
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying || !showDebug) return;
            
            // Güvenlik kontrolü - listeler henüz oluşturulmamışsa
            if (positions == null || rotations == null) return;
            
            // 1. Geçerli noktaları güvenli bir şekilde çizme
            Gizmos.color = Color.blue;
            for (int i = 0; i < positions.Count; i++)
            {
                Gizmos.DrawSphere(positions[i], 0.1f);
                
                if (i < positions.Count - 1)
                {
                    Gizmos.DrawLine(positions[i], positions[i + 1]);
                }
            }
            
            // 2. Sadece birkaç önemli noktayı görselleştir
            // Head (kafa noktası) - daima geçerli transform pozisyonu
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
            
            // İlk kayıtlı nokta (sadece bir nokta bile olsa)
            if (positions.Count > 0)
            {
                Gizmos.color = Color.cyan;
                Vector3 firstPos = positions[0]; // En yeni nokta (başa ekleme yapıldığından)
                Gizmos.DrawWireCube(firstPos, Vector3.one * 0.15f);
            }
            
            // 3. Birkaç "mesafe noktasını" göster (baştan itibaren)
            int maxDistancePoints = 5; // Gösterilecek maksimum mesafe noktası
            for (int i = 0; i < maxDistancePoints; i++)
            {
                float distance = i * fixedDistance;
                try {
                    // Try-catch ile GetPositionAtDistance'ın olası hatalarını yakala
                    Vector3 pos = GetPositionAtDistance(distance);
                    
                    // Sadece geçerli pozisyonları göster (NaN, Infinity kontrolü)
                    if (!float.IsNaN(pos.x) && !float.IsInfinity(pos.x) &&
                        !float.IsNaN(pos.y) && !float.IsInfinity(pos.y) &&
                        !float.IsNaN(pos.z) && !float.IsInfinity(pos.z))
                    {
                        Gizmos.color = new Color(1, 0, 0, 0.7f); // Kırmızı, şeffaf
                        Gizmos.DrawSphere(pos, 0.1f);
#if UNITY_EDITOR
                        UnityEditor.Handles.Label(pos + Vector3.up * 0.2f, $"D:{distance:F1}");

#endif
                    }
                }
                catch {
                    // Hata durumunda sessizce devam et
                }
            }
        }
    }
}