using System.Collections.Generic;
using TheLastEmber.Features.Combat;
using TheLastEmber.Features.DayNightCycle;
using TheLastEmber.Features.Economy;
using UnityEngine;

namespace TheLastEmber.Features.NightMarket
{
    public class NightWaveSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DayNightManager dayNightManager;
        [SerializeField] private GreatFurnace furnace;
        [SerializeField] private Transform target;
        [SerializeField] private ResourceWallet playerWallet;

        [Header("Spawn")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private float spawnInterval = 4f;
        [SerializeField] private int maxAliveEnemies = 4;
        [SerializeField] private float minimumSpawnRadius = 12f;
        [SerializeField] private float spawnRadiusOffset = 4f;
        [SerializeField] private bool despawnEnemiesAtDawn = true;

        [Header("Wave Scaling (per Night)")]
        [Tooltip("Mỗi đêm, maxAliveEnemies tăng thêm bao nhiêu")]
        [SerializeField] private int enemiesPerNight = 1;
        [Tooltip("Mỗi đêm, spawnInterval giảm bao nhiêu giây (min 1s)")]
        [SerializeField] private float intervalReductionPerNight = 0.3f;
        [Tooltip("Mỗi đêm, chỉ số stats (HP/damage/speed) tăng thêm bao nhiêu %")]
        [SerializeField] private float statScalePerNight = 0.15f;

        [Header("Boss / Elite Monster")]
        [SerializeField] private string golemResourcePath = "Monsters/mist_golem";
        [SerializeField] private Vector3 golemScale = new Vector3(1.4f, 1.4f, 1.4f);
        [SerializeField] private Color golemColor = new Color(0.28f, 0.16f, 0.22f, 1f);
        private bool hasSpawnedBossThisNight = false;

        [Header("Placeholder Enemy")]
        [SerializeField] private string monsterResourcePath = "Monsters/character-oozi";
        [SerializeField] private Vector3 enemyScale = new Vector3(1.1f, 1.1f, 1.1f);
        [SerializeField] private Color enemyColor = new Color(0.12f, 0.18f, 0.28f, 1f);

        private readonly List<MistEnemy> aliveEnemies = new List<MistEnemy>();
        private float spawnTimer;
        private bool waveActive;
        private float currentStatMultiplier = 1f;
        private int currentMaxEnemies;
        private float currentSpawnInterval;

        private void Start()
        {
            ResolveReferences();
            currentMaxEnemies = maxAliveEnemies;
            currentSpawnInterval = spawnInterval;
            spawnTimer = 1f;
        }

        private void Update()
        {
            RemoveDestroyedEnemies();

            if (dayNightManager == null) return;

            bool shouldRunWave = dayNightManager.CurrentPhase == DayPhase.Night;
            if (shouldRunWave)
            {
                TickNightWave();
                return;
            }

            StopNightWaveIfNeeded();
        }

        private void ResolveReferences()
        {
            if (dayNightManager == null)
            {
                dayNightManager = FindAnyObjectByType<DayNightManager>();
            }

            if (furnace == null)
            {
                furnace = FindAnyObjectByType<GreatFurnace>();
            }

            if (target == null)
            {
                PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
                if (player != null)
                {
                    target = player.transform;
                }
            }

            if (playerWallet == null)
            {
                playerWallet = FindAnyObjectByType<ResourceWallet>();
            }
        }

        private void TickNightWave()
        {
            if (!waveActive)
            {
                waveActive = true;
                spawnTimer = 0f;
                hasSpawnedBossThisNight = false;
                ApplyWaveScaling();
                Debug.Log($"<color=#9bd0ff>[Night Wave]</color> Đêm thứ {(dayNightManager != null ? dayNightManager.NightCount : 1)} bắt đầu. Quái: {currentMaxEnemies}, Interval: {currentSpawnInterval:F1}s, Stat x{currentStatMultiplier:F2}");
            }

            if (target == null) return;
            if (aliveEnemies.Count >= currentMaxEnemies) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0f) return;

            SpawnEnemy();
            spawnTimer = currentSpawnInterval;
        }

        private void StopNightWaveIfNeeded()
        {
            if (!waveActive) return;

            waveActive = false;
            spawnTimer = 1f;
            Debug.Log("<color=#9bd0ff>[Night Wave]</color> Night wave ended.");

            if (despawnEnemiesAtDawn)
            {
                DespawnAliveEnemies();
            }
        }

        private void SpawnEnemy()
        {
            Vector3 spawnPosition = GetSpawnPosition();
            bool shouldSpawnBoss = !hasSpawnedBossThisNight && dayNightManager != null && dayNightManager.NightCount >= 2;

            GameObject enemyObject;
            if (shouldSpawnBoss)
            {
                enemyObject = CreateGolemBoss(spawnPosition);
                hasSpawnedBossThisNight = true;
            }
            else
            {
                enemyObject = enemyPrefab != null
                    ? Instantiate(enemyPrefab, spawnPosition, Quaternion.identity)
                    : CreatePlaceholderEnemy(spawnPosition);
            }

            MistEnemy enemy = enemyObject.GetComponent<MistEnemy>();
            if (enemy == null)
            {
                enemy = enemyObject.AddComponent<MistEnemy>();
            }

            if (shouldSpawnBoss)
            {
                enemy.ConfigureAsElite("Mist Golem");
            }

            enemy.Initialize(target, HandleEnemyDied, playerWallet, currentStatMultiplier);
            aliveEnemies.Add(enemy);

            Debug.Log($"<color=#9bd0ff>[Night Wave]</color> Spawned {enemy.name}. Alive: {aliveEnemies.Count}/{maxAliveEnemies}");
        }

        private GameObject CreateGolemBoss(Vector3 spawnPosition)
        {
            GameObject modelPrefab = Resources.Load<GameObject>(golemResourcePath);
            GameObject golemObject;

            if (modelPrefab != null)
            {
                golemObject = Instantiate(modelPrefab);
                golemObject.name = "MistGolem_Boss";
                golemObject.transform.position = spawnPosition;
                golemObject.transform.localScale = golemScale;
            }
            else
            {
                golemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                golemObject.name = "MistGolem_Boss";
                golemObject.transform.position = spawnPosition;
                golemObject.transform.localScale = golemScale * 1.5f;
            }

            CapsuleCollider col = golemObject.GetComponent<CapsuleCollider>();
            if (col == null)
            {
                col = golemObject.AddComponent<CapsuleCollider>();
                col.height = 2.8f;
                col.radius = 1.1f;
                col.center = new Vector3(0f, 1.4f, 0f);
            }

            Renderer[] renderers = golemObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                foreach (Material material in r.materials)
                {
                    if (material.HasProperty("_BaseColor"))
                    {
                        material.SetColor("_BaseColor", golemColor);
                    }
                    else if (material.HasProperty("_Color"))
                    {
                        material.SetColor("_Color", golemColor);
                    }
                }
            }

            return golemObject;
        }

        private GameObject CreatePlaceholderEnemy(Vector3 spawnPosition)
        {
            GameObject modelPrefab = Resources.Load<GameObject>(monsterResourcePath);
            GameObject enemyObject;

            if (modelPrefab != null)
            {
                enemyObject = Instantiate(modelPrefab);
                enemyObject.name = "MistEnemy";
                enemyObject.transform.position = spawnPosition;
                enemyObject.transform.localScale = enemyScale;

                // Thêm CapsuleCollider cho model quái vật để có thể nhận đòn đánh
                CapsuleCollider col = enemyObject.GetComponent<CapsuleCollider>();
                if (col == null)
                {
                    col = enemyObject.AddComponent<CapsuleCollider>();
                    col.height = 1.4f;
                    col.radius = 0.5f;
                    col.center = new Vector3(0f, 0.7f, 0f);
                }
            }
            else
            {
                enemyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemyObject.name = "MistEnemy";
                enemyObject.transform.position = spawnPosition;
                enemyObject.transform.localScale = enemyScale;
            }

            Renderer[] renderers = enemyObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                foreach (Material material in r.materials)
                {
                    if (material.HasProperty("_BaseColor"))
                    {
                        material.SetColor("_BaseColor", enemyColor);
                    }
                    else if (material.HasProperty("_Color"))
                    {
                        material.SetColor("_Color", enemyColor);
                    }
                }
            }

            return enemyObject;
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 center = furnace != null ? furnace.Position : transform.position;
            float safeRadius = furnace != null ? furnace.CurrentRadius : minimumSpawnRadius;
            float spawnRadius = Mathf.Max(minimumSpawnRadius, safeRadius + spawnRadiusOffset);
            float angle = Random.Range(0f, Mathf.PI * 2f);

            return new Vector3(
                center.x + Mathf.Cos(angle) * spawnRadius,
                1f,
                center.z + Mathf.Sin(angle) * spawnRadius);
        }

        private void HandleEnemyDied(MistEnemy enemy)
        {
            aliveEnemies.Remove(enemy);
        }

        private void ApplyWaveScaling()
        {
            int night = dayNightManager != null ? dayNightManager.NightCount : 1;
            int extraNights = Mathf.Max(0, night - 1); // Đêm 1 không scale

            currentMaxEnemies = maxAliveEnemies + extraNights * enemiesPerNight;
            currentSpawnInterval = Mathf.Max(1f, spawnInterval - extraNights * intervalReductionPerNight);
            currentStatMultiplier = 1f + extraNights * statScalePerNight;
        }

        private void RemoveDestroyedEnemies()
        {
            for (int i = aliveEnemies.Count - 1; i >= 0; i--)
            {
                if (aliveEnemies[i] == null)
                {
                    aliveEnemies.RemoveAt(i);
                }
            }
        }

        private void DespawnAliveEnemies()
        {
            for (int i = aliveEnemies.Count - 1; i >= 0; i--)
            {
                if (aliveEnemies[i] != null)
                {
                    Destroy(aliveEnemies[i].gameObject);
                }
            }

            aliveEnemies.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = furnace != null ? furnace.Position : transform.position;
            float radius = furnace != null ? Mathf.Max(minimumSpawnRadius, furnace.CurrentRadius + spawnRadiusOffset) : minimumSpawnRadius;

            Gizmos.color = new Color(0.25f, 0.7f, 1f, 0.35f);
            Gizmos.DrawWireSphere(center, radius);
        }
    }
}
