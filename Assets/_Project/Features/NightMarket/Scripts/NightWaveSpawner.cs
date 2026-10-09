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

        [Header("Placeholder Enemy")]
        [SerializeField] private Vector3 enemyScale = new Vector3(0.8f, 1.4f, 0.8f);
        [SerializeField] private Color enemyColor = new Color(0.24f, 0.55f, 0.62f, 1f);

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
            GameObject enemyObject = enemyPrefab != null
                ? Instantiate(enemyPrefab, spawnPosition, Quaternion.identity)
                : CreatePlaceholderEnemy(spawnPosition);

            MistEnemy enemy = enemyObject.GetComponent<MistEnemy>();
            if (enemy == null)
            {
                enemy = enemyObject.AddComponent<MistEnemy>();
            }

            enemy.Initialize(target, HandleEnemyDied, playerWallet, currentStatMultiplier);
            aliveEnemies.Add(enemy);

            Debug.Log($"<color=#9bd0ff>[Night Wave]</color> Spawned {enemy.name}. Alive: {aliveEnemies.Count}/{maxAliveEnemies}");
        }

        private GameObject CreatePlaceholderEnemy(Vector3 spawnPosition)
        {
            GameObject enemyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemyObject.name = "MistEnemy";
            enemyObject.transform.position = spawnPosition;
            enemyObject.transform.localScale = enemyScale;

            Renderer enemyRenderer = enemyObject.GetComponent<Renderer>();
            if (enemyRenderer != null)
            {
                foreach (Material material in enemyRenderer.materials)
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
