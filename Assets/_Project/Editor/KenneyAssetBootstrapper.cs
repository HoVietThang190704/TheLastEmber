using System;
using System.IO;
using TheLastEmber.Features.BuildingSystem;
using TheLastEmber.Features.Economy;
using UnityEditor;
using UnityEngine;

namespace TheLastEmber.EditorTools
{
    [InitializeOnLoad]
    public static class KenneyAssetBootstrapper
    {
        private const string MarkerPath = "Assets/_Project/Settings/KenneyAssetBootstrapComplete.txt";

        private const string SurvivalFbxRoot = "Assets/ThirdParty/Kenney/SurvivalKit/Models/FBX/";
        private const string NatureFbxRoot = "Assets/ThirdParty/Kenney/NatureKit/Models/FBX/";

        private const string EconomyPrefabRoot = "Assets/_Project/Features/Economy/Prefabs/";
        private const string BuildingPrefabRoot = "Assets/_Project/Features/BuildingSystem/Prefabs/";
        private const string KenneyBuildingPrefabRoot = "Assets/_Project/Features/BuildingSystem/Prefabs/Kenney/";
        private const string KenneyBuildingDataRoot = "Assets/_Project/Features/BuildingSystem/Data/Kenney/";
        private const string EnvironmentPrefabRoot = "Assets/_Project/Features/Environment/Prefabs/Kenney/";

        static KenneyAssetBootstrapper()
        {
            QueueBuildOnceAfterImport();
        }

        [MenuItem("The Last Ember/Kenney/Rebuild Applied Asset Prefabs")]
        public static void RebuildAppliedAssetPrefabs()
        {
            BuildAll();
            WriteMarker();
            Debug.Log("[KenneyAssetBootstrapper] Kenney prefabs and buildable definitions rebuilt.");
        }

        public static void QueueBuildOnceAfterImport()
        {
            EditorApplication.delayCall -= BuildOnceAfterImport;
            EditorApplication.delayCall += BuildOnceAfterImport;
        }

        private static void BuildOnceAfterImport()
        {
            if (File.Exists(MarkerPath)) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                QueueBuildOnceAfterImport();
                return;
            }

            if (!ModelExists(SurvivalFbxRoot + "resource-wood.fbx"))
            {
                QueueBuildOnceAfterImport();
                return;
            }

            BuildAll();
            WriteMarker();
            Debug.Log("[KenneyAssetBootstrapper] Kenney assets applied. Use The Last Ember/Kenney/Rebuild Applied Asset Prefabs to regenerate them.");
        }

        private static void BuildAll()
        {
            EnsureFolders();

            CreateCollectiblePrefab(
                "WoodPickup",
                SurvivalFbxRoot + "resource-wood.fbx",
                EconomyPrefabRoot + "WoodPickup.prefab",
                ResourceType.Wood,
                1,
                new Vector3(0f, 0f, 0f),
                new Vector3(1.35f, 1.35f, 1.35f),
                0.7f);

            CreateCollectiblePrefab(
                "StonePickup",
                SurvivalFbxRoot + "resource-stone.fbx",
                EconomyPrefabRoot + "StonePickup.prefab",
                ResourceType.Stone,
                1,
                new Vector3(0f, 0f, 0f),
                new Vector3(1.2f, 1.2f, 1.2f),
                0.65f);

            CreateStaticPrefab(
                "TorchPost",
                SurvivalFbxRoot + "campfire-stand.fbx",
                BuildingPrefabRoot + "TorchPost.prefab",
                new Vector3(0f, 0f, 0f),
                Vector3.one,
                true,
                new Vector3(0f, 1.75f, 0f),
                7f,
                2.4f);

            GameObject campfire = CreateStaticPrefab(
                "CampfirePit",
                SurvivalFbxRoot + "campfire-pit.fbx",
                KenneyBuildingPrefabRoot + "CampfirePit.prefab",
                new Vector3(0f, 0f, 0f),
                Vector3.one,
                true,
                new Vector3(0f, 0.8f, 0f),
                5f,
                1.5f);

            GameObject fence = CreateStaticPrefab(
                "FortifiedFence",
                SurvivalFbxRoot + "fence-fortified.fbx",
                KenneyBuildingPrefabRoot + "FortifiedFence.prefab",
                new Vector3(0f, 0f, 0f),
                Vector3.one,
                false,
                Vector3.zero,
                0f,
                0f);

            GameObject workbench = CreateStaticPrefab(
                "Workbench",
                SurvivalFbxRoot + "workbench.fbx",
                KenneyBuildingPrefabRoot + "Workbench.prefab",
                new Vector3(0f, 0f, 0f),
                Vector3.one,
                false,
                Vector3.zero,
                0f,
                0f);

            CreateStaticPrefab("SupplyChest", SurvivalFbxRoot + "chest.fbx", KenneyBuildingPrefabRoot + "SupplyChest.prefab", Vector3.zero, Vector3.one, false, Vector3.zero, 0f, 0f);
            CreateStaticPrefab("Barrel", SurvivalFbxRoot + "barrel.fbx", KenneyBuildingPrefabRoot + "Barrel.prefab", Vector3.zero, Vector3.one, false, Vector3.zero, 0f, 0f);

            CreateEnvironmentPrefab("PineTall", NatureFbxRoot + "tree_pineTallA.fbx", new Vector3(0f, 0f, 0f), Vector3.one);
            CreateEnvironmentPrefab("DarkTree", NatureFbxRoot + "tree_default_dark.fbx", new Vector3(0f, 0f, 0f), Vector3.one);
            CreateEnvironmentPrefab("FatTree", NatureFbxRoot + "tree_fat.fbx", new Vector3(0f, 0f, 0f), Vector3.one);
            CreateEnvironmentPrefab("Log", NatureFbxRoot + "log.fbx", new Vector3(0f, 0f, 0f), Vector3.one);
            CreateEnvironmentPrefab("LogStack", NatureFbxRoot + "log_stack.fbx", new Vector3(0f, 0f, 0f), Vector3.one);
            CreateEnvironmentPrefab("RoundStump", NatureFbxRoot + "stump_round.fbx", new Vector3(0f, 0f, 0f), Vector3.one);
            CreateEnvironmentPrefab("LargeRock", NatureFbxRoot + "rock_largeA.fbx", new Vector3(0f, 0f, 0f), Vector3.one);
            CreateEnvironmentPrefab("LargeStone", NatureFbxRoot + "stone_largeA.fbx", new Vector3(0f, 0f, 0f), Vector3.one);
            CreateEnvironmentPrefab("MushroomPatch", NatureFbxRoot + "mushroom_redGroup.fbx", new Vector3(0f, 0f, 0f), Vector3.one);
            CreateEnvironmentPrefab("Bush", NatureFbxRoot + "plant_bush.fbx", new Vector3(0f, 0f, 0f), Vector3.one);
            CreateEnvironmentPrefab("TallGrass", NatureFbxRoot + "grass_large.fbx", new Vector3(0f, 0f, 0f), Vector3.one);
            CreateEnvironmentPrefab("StonePath", NatureFbxRoot + "path_stone.fbx", new Vector3(0f, 0f, 0f), Vector3.one);

            CreateBuildableDefinition(
                "TorchPostDefinition",
                "Torch Post",
                BuildingPrefabRoot + "TorchPost.prefab",
                "Assets/_Project/Features/BuildingSystem/Data/TorchPostDefinition.asset",
                new Vector2Int(1, 1),
                0f,
                new[] { new CostSpec(ResourceType.Wood, 2) });

            CreateBuildableDefinition(
                "CampfireDefinition",
                "Campfire",
                KenneyBuildingPrefabRoot + "CampfirePit.prefab",
                KenneyBuildingDataRoot + "CampfireDefinition.asset",
                new Vector2Int(1, 1),
                0f,
                new[] { new CostSpec(ResourceType.Wood, 4), new CostSpec(ResourceType.Stone, 2) });

            CreateBuildableDefinition(
                "FortifiedFenceDefinition",
                "Fortified Fence",
                KenneyBuildingPrefabRoot + "FortifiedFence.prefab",
                KenneyBuildingDataRoot + "FortifiedFenceDefinition.asset",
                new Vector2Int(1, 1),
                0f,
                new[] { new CostSpec(ResourceType.Wood, 3) });

            CreateBuildableDefinition(
                "WorkbenchDefinition",
                "Workbench",
                KenneyBuildingPrefabRoot + "Workbench.prefab",
                KenneyBuildingDataRoot + "WorkbenchDefinition.asset",
                new Vector2Int(2, 1),
                0f,
                new[] { new CostSpec(ResourceType.Wood, 5), new CostSpec(ResourceType.Stone, 3) });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _ = campfire;
            _ = fence;
            _ = workbench;
        }

        private static void CreateCollectiblePrefab(
            string name,
            string modelPath,
            string prefabPath,
            ResourceType resourceType,
            int amount,
            Vector3 modelLocalPosition,
            Vector3 modelLocalScale,
            float triggerRadius)
        {
            GameObject root = CreateRootWithModel(name, modelPath, modelLocalPosition, modelLocalScale);
            SphereCollider trigger = root.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = triggerRadius;
            trigger.center = new Vector3(0f, triggerRadius * 0.55f, 0f);

            CollectibleResource resource = root.AddComponent<CollectibleResource>();
            SerializedObject serialized = new SerializedObject(resource);
            serialized.FindProperty("resourceType").enumValueIndex = (int)resourceType;
            serialized.FindProperty("amount").intValue = amount;
            serialized.FindProperty("destroyOnPickup").boolValue = true;
            serialized.FindProperty("generateDefaultVisual").boolValue = false;
            serialized.FindProperty("hidePlaceholderRenderer").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SaveAndDestroy(root, prefabPath);
        }

        private static GameObject CreateStaticPrefab(
            string name,
            string modelPath,
            string prefabPath,
            Vector3 modelLocalPosition,
            Vector3 modelLocalScale,
            bool addLight,
            Vector3 lightLocalPosition,
            float lightRange,
            float lightIntensity)
        {
            GameObject root = CreateRootWithModel(name, modelPath, modelLocalPosition, modelLocalScale);
            AddFittedBoxCollider(root);

            if (addLight)
            {
                GameObject lightObject = new GameObject("Warm Light");
                lightObject.transform.SetParent(root.transform, false);
                lightObject.transform.localPosition = lightLocalPosition;

                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.53f, 0.22f);
                light.range = lightRange;
                light.intensity = lightIntensity;
                light.shadows = LightShadows.Soft;
            }

            return SaveAndDestroy(root, prefabPath);
        }

        private static void CreateEnvironmentPrefab(string name, string modelPath, Vector3 modelLocalPosition, Vector3 modelLocalScale)
        {
            GameObject root = CreateRootWithModel(name, modelPath, modelLocalPosition, modelLocalScale);
            AddFittedBoxCollider(root);
            SaveAndDestroy(root, EnvironmentPrefabRoot + name + ".prefab");
        }

        private static GameObject CreateRootWithModel(string name, string modelPath, Vector3 modelLocalPosition, Vector3 modelLocalScale)
        {
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelAsset == null)
            {
                throw new InvalidOperationException($"Kenney model not imported: {modelPath}");
            }

            GameObject root = new GameObject(name);
            GameObject modelInstance = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (modelInstance == null)
            {
                modelInstance = UnityEngine.Object.Instantiate(modelAsset);
            }

            modelInstance.name = "Visual";
            modelInstance.transform.SetParent(root.transform, false);
            modelInstance.transform.localPosition = modelLocalPosition;
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = modelLocalScale;

            return root;
        }

        private static GameObject SaveAndDestroy(GameObject root, string prefabPath)
        {
            EnsureParentFolder(prefabPath);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void AddFittedBoxCollider(GameObject root)
        {
            Bounds bounds = CalculateRendererBounds(root);
            BoxCollider collider = root.AddComponent<BoxCollider>();

            if (bounds.size == Vector3.zero)
            {
                collider.center = new Vector3(0f, 0.5f, 0f);
                collider.size = Vector3.one;
                return;
            }

            collider.center = root.transform.InverseTransformPoint(bounds.center);
            collider.size = bounds.size;
        }

        private static Bounds CalculateRendererBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static void CreateBuildableDefinition(
            string assetName,
            string displayName,
            string prefabPath,
            string assetPath,
            Vector2Int footprint,
            float yOffset,
            CostSpec[] costs)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Buildable prefab not found: {prefabPath}");
            }

            BuildableDefinition definition = AssetDatabase.LoadAssetAtPath<BuildableDefinition>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<BuildableDefinition>();
                definition.name = assetName;
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("prefab").objectReferenceValue = prefab;
            serialized.FindProperty("footprint").vector2IntValue = footprint;
            serialized.FindProperty("yOffset").floatValue = yOffset;

            SerializedProperty costArray = serialized.FindProperty("costs");
            costArray.arraySize = costs.Length;

            for (int i = 0; i < costs.Length; i++)
            {
                SerializedProperty item = costArray.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("resourceType").enumValueIndex = (int)costs[i].ResourceType;
                item.FindPropertyRelative("amount").intValue = costs[i].Amount;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/ThirdParty");
            EnsureFolder("Assets/ThirdParty/Kenney");
            EnsureFolder("Assets/_Project/Editor");
            EnsureFolder("Assets/_Project/Features/Economy/Prefabs");
            EnsureFolder("Assets/_Project/Features/Environment/Prefabs");
            EnsureFolder(EnvironmentPrefabRoot.TrimEnd('/'));
            EnsureFolder(KenneyBuildingPrefabRoot.TrimEnd('/'));
            EnsureFolder(KenneyBuildingDataRoot.TrimEnd('/'));
            EnsureFolder("Assets/_Project/Settings");
        }

        private static void EnsureParentFolder(string assetPath)
        {
            string folder = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(folder))
            {
                EnsureFolder(folder);
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(folderPath);

            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static bool ModelExists(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) != null;
        }

        private static void WriteMarker()
        {
            EnsureParentFolder(MarkerPath);
            File.WriteAllText(MarkerPath, "Kenney survival and nature assets applied.\n");
            AssetDatabase.ImportAsset(MarkerPath);
        }

        private readonly struct CostSpec
        {
            public CostSpec(ResourceType resourceType, int amount)
            {
                ResourceType = resourceType;
                Amount = amount;
            }

            public ResourceType ResourceType { get; }
            public int Amount { get; }
        }
    }

    public sealed class KenneyAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.StartsWith("Assets/ThirdParty/Kenney/", StringComparison.Ordinal))
                {
                    KenneyAssetBootstrapper.QueueBuildOnceAfterImport();
                    return;
                }
            }
        }
    }
}
