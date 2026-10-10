using UnityEngine;

namespace TheLastEmber.Features.Economy
{
    [ExecuteAlways]
    [RequireComponent(typeof(Collider))]
    public class CollectibleResource : MonoBehaviour
    {
        [Header("Resource")]
        [SerializeField] private ResourceType resourceType = ResourceType.Wood;
        [SerializeField] private int amount = 1;

        [Header("Pickup")]
        [SerializeField] private bool destroyOnPickup = true;

        [Header("Visual")]
        [SerializeField] private bool generateDefaultVisual = true;
        [SerializeField] private bool hidePlaceholderRenderer = true;

        private const string GeneratedVisualRootName = "__GeneratedResourceVisual";

        private bool hasBeenCollected;
        private bool editorRefreshQueued;
        private bool placeholderRendererWasHidden;
        private Material generatedPrimaryMaterial;
        private Material generatedSecondaryMaterial;
        private Material generatedGlowMaterial;

        public void Configure(
            ResourceType type,
            int resourceAmount,
            bool shouldDestroyOnPickup = true,
            bool shouldGenerateDefaultVisual = true)
        {
            resourceType = type;
            amount = Mathf.Max(1, resourceAmount);
            destroyOnPickup = shouldDestroyOnPickup;
            generateDefaultVisual = shouldGenerateDefaultVisual;
            hasBeenCollected = false;

            Collider resourceCollider = GetComponent<Collider>();
            if (resourceCollider != null)
            {
                resourceCollider.isTrigger = true;
            }

            RefreshGeneratedVisual();
        }

        private void OnEnable()
        {
            RefreshGeneratedVisual();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!isActiveAndEnabled) return;

            if (editorRefreshQueued)
            {
                return;
            }

            editorRefreshQueued = true;
            UnityEditor.EditorApplication.delayCall -= RefreshGeneratedVisualInEditor;
            UnityEditor.EditorApplication.delayCall += RefreshGeneratedVisualInEditor;
        }

        private void RefreshGeneratedVisualInEditor()
        {
            editorRefreshQueued = false;
            if (this == null || !isActiveAndEnabled) return;

            RefreshGeneratedVisual();
        }
#endif

        private void OnDisable()
        {
            ClearGeneratedVisual();
        }

        private void Reset()
        {
            Collider resourceCollider = GetComponent<Collider>();
            resourceCollider.isTrigger = true;
        }

        private void RefreshGeneratedVisual()
        {
            ClearGeneratedVisual();

            if (!generateDefaultVisual || HasAuthoredChildVisual())
            {
                RestorePlaceholderRenderer();
                return;
            }

            Transform root = new GameObject(GeneratedVisualRootName).transform;
            root.gameObject.hideFlags = HideFlags.DontSave;
            root.SetParent(transform, false);
            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;
            root.localScale = Vector3.one;

            CreateMaterials();

            switch (resourceType)
            {
                case ResourceType.Wood:
                    BuildWoodVisual(root);
                    break;
                case ResourceType.Stone:
                    BuildStoneVisual(root);
                    break;
                case ResourceType.SilverEmber:
                    BuildSilverEmberVisual(root);
                    break;
            }

            if (hidePlaceholderRenderer && TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.enabled = false;
                placeholderRendererWasHidden = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasBeenCollected) return;

            ResourceWallet wallet = other.GetComponent<ResourceWallet>();
            if (wallet == null) return;

            hasBeenCollected = true;
            wallet.Add(resourceType, amount);

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private bool HasAuthoredChildVisual()
        {
            Renderer[] childRenderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer childRenderer in childRenderers)
            {
                if (childRenderer.transform == transform) continue;
                if (IsUnderGeneratedVisualRoot(childRenderer.transform)) continue;

                return true;
            }

            return false;
        }

        private bool IsUnderGeneratedVisualRoot(Transform candidate)
        {
            Transform current = candidate;
            while (current != null && current != transform)
            {
                if (current.name == GeneratedVisualRootName)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private void BuildWoodVisual(Transform root)
        {
            CreatePrimitive(
                root,
                PrimitiveType.Cylinder,
                "WoodLog_A",
                new Vector3(-0.08f, 0.24f, -0.08f),
                Quaternion.Euler(0f, 18f, 90f),
                new Vector3(0.26f, 0.62f, 0.26f),
                generatedPrimaryMaterial);

            CreatePrimitive(
                root,
                PrimitiveType.Cylinder,
                "WoodLog_B",
                new Vector3(0.12f, 0.28f, 0.12f),
                Quaternion.Euler(0f, -16f, 90f),
                new Vector3(0.22f, 0.56f, 0.22f),
                generatedPrimaryMaterial);

            CreatePrimitive(
                root,
                PrimitiveType.Cylinder,
                "WoodLog_C",
                new Vector3(0.03f, 0.45f, 0f),
                Quaternion.Euler(0f, 8f, 90f),
                new Vector3(0.18f, 0.48f, 0.18f),
                generatedSecondaryMaterial);
        }

        private void BuildStoneVisual(Transform root)
        {
            CreatePrimitive(
                root,
                PrimitiveType.Sphere,
                "Stone_A",
                new Vector3(-0.12f, 0.22f, 0.02f),
                Quaternion.Euler(8f, 28f, 0f),
                new Vector3(0.48f, 0.3f, 0.38f),
                generatedPrimaryMaterial);

            CreatePrimitive(
                root,
                PrimitiveType.Sphere,
                "Stone_B",
                new Vector3(0.17f, 0.18f, -0.09f),
                Quaternion.Euler(-5f, 62f, 0f),
                new Vector3(0.34f, 0.24f, 0.3f),
                generatedSecondaryMaterial);
        }

        private void BuildSilverEmberVisual(Transform root)
        {
            CreatePrimitive(
                root,
                PrimitiveType.Sphere,
                "SilverEmberGlow",
                new Vector3(0f, 0.28f, 0f),
                Quaternion.identity,
                new Vector3(0.32f, 0.32f, 0.32f),
                generatedGlowMaterial);

            CreatePrimitive(
                root,
                PrimitiveType.Cube,
                "SilverEmberCore",
                new Vector3(0f, 0.31f, 0f),
                Quaternion.Euler(18f, 28f, 13f),
                new Vector3(0.28f, 0.28f, 0.28f),
                generatedPrimaryMaterial);
        }

        private void CreatePrimitive(
            Transform parent,
            PrimitiveType primitiveType,
            string objectName,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Material material)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = objectName;
            primitive.hideFlags = HideFlags.DontSave;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = localRotation;
            primitive.transform.localScale = localScale;

            if (primitive.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.sharedMaterial = material;
            }

            Collider primitiveCollider = primitive.GetComponent<Collider>();
            if (primitiveCollider != null)
            {
                DestroyGeneratedObject(primitiveCollider);
            }
        }

        private void CreateMaterials()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            generatedPrimaryMaterial = CreateMaterial(shader, "Generated Resource Primary", GetPrimaryColor());
            generatedSecondaryMaterial = CreateMaterial(shader, "Generated Resource Accent", GetSecondaryColor());

            if (resourceType == ResourceType.SilverEmber)
            {
                generatedGlowMaterial = CreateMaterial(shader, "Generated Resource Glow", new Color(1f, 0.72f, 0.22f));
                SetColor(generatedGlowMaterial, "_EmissionColor", new Color(1f, 0.55f, 0.16f) * 1.5f);
                generatedGlowMaterial.EnableKeyword("_EMISSION");
            }
        }

        private Color GetPrimaryColor()
        {
            switch (resourceType)
            {
                case ResourceType.Stone:
                    return new Color(0.42f, 0.42f, 0.39f);
                case ResourceType.SilverEmber:
                    return new Color(1f, 0.61f, 0.2f);
                default:
                    return new Color(0.42f, 0.25f, 0.13f);
            }
        }

        private Color GetSecondaryColor()
        {
            switch (resourceType)
            {
                case ResourceType.Stone:
                    return new Color(0.55f, 0.54f, 0.49f);
                case ResourceType.SilverEmber:
                    return new Color(1f, 0.82f, 0.34f);
                default:
                    return new Color(0.58f, 0.34f, 0.16f);
            }
        }

        private static Material CreateMaterial(Shader shader, string materialName, Color color)
        {
            Material material = new Material(shader)
            {
                name = materialName,
                hideFlags = HideFlags.DontSave
            };

            SetColor(material, "_BaseColor", color);
            SetColor(material, "_Color", color);
            SetFloat(material, "_Smoothness", 0.38f);
            return material;
        }

        private void RestorePlaceholderRenderer()
        {
            if (!placeholderRendererWasHidden) return;

            if (TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.enabled = true;
            }

            placeholderRendererWasHidden = false;
        }

        private void ClearGeneratedVisual()
        {
            Transform existing = transform.Find(GeneratedVisualRootName);
            if (existing != null)
            {
                DestroyGeneratedObject(existing.gameObject);
            }

            DestroyGeneratedMaterial(ref generatedPrimaryMaterial);
            DestroyGeneratedMaterial(ref generatedSecondaryMaterial);
            DestroyGeneratedMaterial(ref generatedGlowMaterial);
        }

        private void DestroyGeneratedMaterial(ref Material material)
        {
            if (material == null) return;
            DestroyGeneratedObject(material);
            material = null;
        }

        private static void DestroyGeneratedObject(Object target)
        {
            if (target == null) return;

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private static void SetColor(Material material, string propertyName, Color color)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, color);
            }
        }

        private static void SetFloat(Material material, string propertyName, float value)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }
    }
}
