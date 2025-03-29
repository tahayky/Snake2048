using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Snake2048
{
    public class EnemyAIController : CharacterBase
    {
        [Header("AI Ayarları")]
        public float detectionRange = 15f;        // Algılama mesafesi
        public float navMeshCheckDistance = 5f;   // İleri yönde NavMesh kontrolü mesafesi
        public float directionChangeInterval = 3f; // Rastgele yön değiştirme aralığı

        // Hedefler ve yönler
        private Transform targetTransform;        // Hedef yılan
        private bool isChasing;                   // Kovalama mı kaçma mı?
        private Vector3 currentDirection;         // Şu anki hareket yönü
        private float directionTimer;             // Yön değiştirme zamanlayıcısı

        // NavMesh referansı ve kontrolü
        private NavMeshHit navHit;                // NavMesh kontrolü için
        protected override void Start()
        {
            base.Start();
            gameObject.name = GenerateRandomNickname();
            // Başlangıçta rastgele bir yön seç
            GetRandomDirection();
            
            // Periyodik olarak çevredeki yılanları kontrol et
            InvokeRepeating(nameof(ScanForSnakes), 0.5f, 0.5f);
            
            CanvasNicknameManager.Instance.AddNickname(
                transform,      // Target (karakter transformu)
                gameObject.name,      // Nickname metni
                Color.white     // Renk
            );
        }

        protected override void FixedUpdate()
        {
            // Hedefe göre yönü güncelle
            UpdateMoveDirection();
            // CharacterBase'in Move metodunu çağır - bu bizim için hareket vector3'ünü ayarlar
            Move(currentDirection);
            
            // Base sınıfın FixedUpdate'ini çağır - bu hareket mekaniğini yürütür
            base.FixedUpdate();
        }

        // Etraftaki yılanları tara
        private void ScanForSnakes()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange);
            Transform bestTarget = null;
            bool shouldChase = false;
            float closestDistance = float.MaxValue;
            
            foreach (Collider col in colliders)
            {
                CharacterBase snake = col.GetComponent<CharacterBase>();
                if (snake != null && snake != this && snake.gameObject.activeInHierarchy)
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    
                    // Eğer tehdit varsa (bizden büyük)
                    if (snake.Size > Size)
                    {
                        // Tehdit avdan öncelikli - hemen tehdide odaklan
                        if (distance < closestDistance)
                        {
                            bestTarget = col.transform;
                            shouldChase = false; // Kaçma modunda
                            closestDistance = distance;
                        }
                    }
                    // Eğer av varsa (bizden küçük) ve daha yakın bir tehdit yoksa
                    else if (snake.Size < Size && (bestTarget == null || shouldChase))
                    {
                        if (distance < closestDistance)
                        {
                            bestTarget = col.transform;
                            shouldChase = true; // Kovalama modunda
                            closestDistance = distance;
                        }
                    }
                }
            }
            
            // Hedefi ve durumu ayarla
            targetTransform = bestTarget;
            isChasing = shouldChase;
            
            // Turbo modunu ayarla - tehdit veya av varsa hızlan
            Turbo(targetTransform != null);
        }

        // Hedef durumuna göre hareket yönünü güncelle
        private void UpdateMoveDirection()
        {
            // Eğer bir hedef varsa (tehdit veya av)
            if (targetTransform != null && targetTransform.gameObject.activeInHierarchy)
            {
                Vector3 direction;
                
                if (isChasing)
                {
                    // Hedefe doğru git
                    direction = (targetTransform.position - transform.position).normalized;
                }
                else
                {
                    // Hedeften uzaklaş
                    direction = (transform.position - targetTransform.position).normalized;
                }
                
                // Yönü yatay düzlemde tut
                direction.y = 0;
                
                // Gelecekteki pozisyon NavMesh üzerinde mi kontrol et
                Vector3 futurePosition = transform.position + direction * navMeshCheckDistance;
                
                if (IsPositionOnNavMesh(futurePosition))
                {
                    // Eğer geçerli bir NavMesh pozisyonu ise, yönü ayarla
                    currentDirection = direction.normalized;
                }
                else
                {
                    // NavMesh dışındaysa, NavMesh üzerinde kalacak bir yön bul
                    FindSafeDirection();
                }
            }
            // Hedef yoksa, belli aralıklarla rastgele yön değiştir
            else
            {
                directionTimer -= Time.fixedDeltaTime;
                if (directionTimer <= 0)
                {
                    GetRandomDirection();
                    directionTimer = directionChangeInterval;
                }
                
                // NavMesh kontrolü yap
                Vector3 futurePosition = transform.position + currentDirection * navMeshCheckDistance;
                if (!IsPositionOnNavMesh(futurePosition))
                {
                    FindSafeDirection();
                }
            }
        }

        // Pozisyonun NavMesh üzerinde olup olmadığını kontrol et
        private bool IsPositionOnNavMesh(Vector3 position)
        {
            // NavMesh.SamplePosition en yakın NavMesh noktasını bulur
            if (NavMesh.SamplePosition(position, out navHit, 1.0f, NavMesh.AllAreas))
            {
                // Eğer pozisyonumuz NavMesh'e yeterince yakınsa (1 birim içinde)
                return true;
            }
            return false;
        }

        // NavMesh üzerinde kalacak güvenli bir yön bul
        private void FindSafeDirection()
        {
            // 12 farklı yönü kontrol et (30 derece aralıklarla)
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                Vector3 futurePosition = transform.position + direction * navMeshCheckDistance;
                
                if (IsPositionOnNavMesh(futurePosition))
                {
                    currentDirection = direction;
                    return;
                }
            }
            
            // Eğer hiçbir yön güvenli değilse, şu anki konuma en yakın NavMesh noktasına doğru git
            if (NavMesh.SamplePosition(transform.position, out navHit, 10f, NavMesh.AllAreas))
            {
                currentDirection = (navHit.position - transform.position).normalized;
            }
            else
            {
                // Son çare - rastgele bir yön seç
                GetRandomDirection();
            }
        }

        // Rastgele bir yön seç ve NavMesh kontrolü yap
        private void GetRandomDirection()
        {
            // İlk olarak rastgele bir yön
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;
            
            // Bu yönde NavMesh kontrolü yap
            Vector3 futurePosition = transform.position + randomDirection * navMeshCheckDistance;
            
            if (IsPositionOnNavMesh(futurePosition))
            {
                currentDirection = randomDirection;
            }
            else
            {
                // Güvenli bir yön bul
                FindSafeDirection();
            }
            
            directionTimer = directionChangeInterval;
        }

        // Debug görselleştirme
        private void OnDrawGizmosSelected()
        {
            // Algılama mesafesi
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Geçerli yön
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, currentDirection * 5f);
            
            // NavMesh kontrol mesafesi
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + currentDirection * navMeshCheckDistance, 1f);
            
            // Hedef bağlantısı
            if (targetTransform != null)
            {
                Gizmos.color = isChasing ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position, targetTransform.position);
            }
        }
        private static List<string> adjectives = new List<string>
        {
            "Swift", "Mystic", "Dark", "Silent", "Iron",
            "Crimson", "Golden", "Shadow", "Frozen", "Noble",
            "Ancient", "Red", "Screaming", "Laughing", "Mad"
        };

        private static List<string> nouns = new List<string>
        {
            "Wolf", "Phoenix", "Dragon", "Giant", "Warrior",
            "Knight", "Hunter", "Ghost", "Storm", "Raven",
            "Samurai", "Mage", "Bandit", "Tiger", "Emperor"
        };
        public static string GenerateRandomNickname()
        {
            // Rastgele sıfat ve isim seç
            string randomAdjective = adjectives[Random.Range(0, adjectives.Count)];
            string randomNoun = nouns[Random.Range(0, nouns.Count)];
        
            // 100-999 arası rastgele numara ekle
            int randomNumber = Random.Range(100, 1000);
        
            return $"{randomAdjective}{randomNoun}{randomNumber}";
        }
    }
}