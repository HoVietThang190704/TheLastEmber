using System;
using System.IO;
using TheLastEmber.Features.Environment;
using TheLastEmber.Features.Economy;
using UnityEditor;
using UnityEngine;

namespace TheLastEmber.EditorTools
{
    [InitializeOnLoad]
    public static class RealisticTreeBootstrapper
    {
        private const string ArtDir = "Assets/_Project/Art/Environment/";
        private const string BarkTexPath = ArtDir + "tree_bark_diffuse.png";
        private const string LeafTexPath = ArtDir + "tree_leaf_diffuse.png";
        private const string BarkMatPath = ArtDir + "M_Tree_Bark.mat";
        private const string LeafMatPath = ArtDir + "M_Tree_Leaves.mat";
        private const string GrassTexPath = ArtDir + "grass_carpet_diffuse.png";
        private const string GrassMatPath = ArtDir + "M_Grass_Carpet.mat";
        private const string GrassObjPath = ArtDir + "grass_carpet.obj";
        private const string GrassPrefabPath = ArtDir + "Grass_Carpet.prefab";
        private const string GrassResourcesDir = "Assets/_Project/Resources/Environment/";

        private static readonly (string objFile, string prefabName, float colRadius, float colHeight)[] TreeDefinitions = new[]
        {
            ("tree_small.obj", "Tree_Small.prefab", 0.55f, 5.0f),
            ("tree_medium.obj", "Tree_Medium.prefab", 0.85f, 8.0f),
            ("tree_large.obj", "Tree_Large.prefab", 1.40f, 12.0f)
        };

        static RealisticTreeBootstrapper()
        {
            EditorApplication.delayCall += InitializeAssets;
        }

        [MenuItem("The Last Ember/Environment/Setup All Environment Assets (Trees + Grass)")]
        public static void SetupTreeAssetsMenu()
        {
            SetupMaterialsAndPrefabs(true);
            SetupGrassCarpet(true);
        }

        [MenuItem("The Last Ember/Environment/Spawn Preview Trees In Scene")]
        public static void SpawnPreviewTrees()
        {
            SetupMaterialsAndPrefabs(false);

            for (int i = 0; i < TreeDefinitions.Length; i++)
            {
                string prefabPath = ArtDir + TreeDefinitions[i].prefabName;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null) continue;

                GameObject treeInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (treeInstance != null)
                {
                    treeInstance.transform.position = new Vector3(-15f + i * 15f, 0f, 15f);
                    Undo.RegisterCreatedObjectUndo(treeInstance, "Spawn Tree Preview");
                }
            }
            Debug.Log("[RealisticTreeBootstrapper] Spawned 3 preview trees in scene.");
        }

        private static void InitializeAssets()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += InitializeAssets;
                return;
            }

            if (!File.Exists(LeafTexPath)) return;

            SetupMaterialsAndPrefabs(false);
        }

        public static void SetupMaterialsAndPrefabs(bool forceRecreate)
        {
            ConfigureTextures();

            Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            // 1. Bark Material
            Material barkMat = AssetDatabase.LoadAssetAtPath<Material>(BarkMatPath);
            if (barkMat == null || forceRecreate)
            {
                barkMat = new Material(urpLitShader) { name = "M_Tree_Bark" };
                Texture2D barkTex = AssetDatabase.LoadAssetAtPath<Texture2D>(BarkTexPath);
                if (barkTex != null)
                {
                    barkMat.SetTexture("_BaseMap", barkTex);
                    barkMat.SetTexture("_MainTex", barkTex);
                }
                barkMat.SetFloat("_Smoothness", 0.12f);
                barkMat.SetFloat("_Metallic", 0f);
                AssetDatabase.CreateAsset(barkMat, BarkMatPath);
            }

            // 2. Leaf Material with Alpha Clipping & Double-Sided Rendering
            Material leafMat = AssetDatabase.LoadAssetAtPath<Material>(LeafMatPath);
            if (leafMat == null || forceRecreate)
            {
                leafMat = new Material(urpLitShader) { name = "M_Tree_Leaves" };
                Texture2D leafTex = AssetDatabase.LoadAssetAtPath<Texture2D>(LeafTexPath);
                if (leafTex != null)
                {
                    leafMat.SetTexture("_BaseMap", leafTex);
                    leafMat.SetTexture("_MainTex", leafTex);
                }

                // Setup URP Alpha Clipping & Cull Off
                leafMat.SetFloat("_AlphaClip", 1f);
                leafMat.SetFloat("_Cutoff", 0.35f);
                leafMat.SetFloat("_Cull", 0f); // Double-sided (Cull Off)
                leafMat.SetFloat("_Smoothness", 0.25f);
                leafMat.SetFloat("_Metallic", 0f);
                leafMat.EnableKeyword("_ALPHATEST_ON");
                leafMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;

                AssetDatabase.CreateAsset(leafMat, LeafMatPath);
            }

            // 3. Configure ModelImporter for OBJ files so 3D Preview displays full colors
            foreach (var def in TreeDefinitions)
            {
                string objPath = ArtDir + def.objFile;
                ModelImporter importer = AssetImporter.GetAtPath(objPath) as ModelImporter;
                if (importer != null)
                {
                    bool dirty = false;
                    if (importer.materialName != ModelImporterMaterialName.BasedOnMaterialName)
                    {
                        importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
                        dirty = true;
                    }
                    if (importer.materialSearch != ModelImporterMaterialSearch.RecursiveUp)
                    {
                        importer.materialSearch = ModelImporterMaterialSearch.RecursiveUp;
                        dirty = true;
                    }
                    if (dirty)
                    {
                        importer.SaveAndReimport();
                    }
                }
            }

            // 4. Create Prefabs for all 3 tree sizes
            foreach (var def in TreeDefinitions)
            {
                string objPath = ArtDir + def.objFile;
                string prefabPath = ArtDir + def.prefabName;

                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);
                if (model == null) continue;

                string objTitle = Path.GetFileNameWithoutExtension(def.prefabName);
                GameObject tempTree = new GameObject(objTitle);
                try
                {
                    MeshFilter mf = tempTree.AddComponent<MeshFilter>();
                    MeshRenderer mr = tempTree.AddComponent<MeshRenderer>();

                    MeshFilter modelMf = model.GetComponentInChildren<MeshFilter>();
                    if (modelMf != null)
                    {
                        mf.sharedMesh = modelMf.sharedMesh;
                    }

                    mr.sharedMaterials = new Material[] { barkMat, leafMat };

                    // Physics Capsule Collider for trunk
                    CapsuleCollider col = tempTree.AddComponent<CapsuleCollider>();
                    col.center = new Vector3(0f, def.colHeight * 0.5f, 0f);
                    col.radius = def.colRadius;
                    col.height = def.colHeight;

                    // Add gameplay TreeResourceNode so player can chop it
                    tempTree.AddComponent<TreeResourceNode>();

                    PrefabUtility.SaveAsPrefabAsset(tempTree, prefabPath);

                    // Đồng thời lưu vào Resources/Environment để runtime và editor nạp tức thì
                    if (!Directory.Exists(GrassResourcesDir))
                    {
                        Directory.CreateDirectory(GrassResourcesDir);
                    }
                    string resPrefabPath = GrassResourcesDir + def.prefabName;
                    PrefabUtility.SaveAsPrefabAsset(tempTree, resPrefabPath);

                    Debug.Log($"[RealisticTreeBootstrapper] Successfully generated {def.prefabName} in Art and Resources!");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(tempTree);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ConfigureTextures()
        {
            ConfigureTextureAlpha(LeafTexPath);
            ConfigureTextureAlpha(GrassTexPath);
        }

        private static void ConfigureTextureAlpha(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool dirty = false;
                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    dirty = true;
                }
                if (importer.alphaSource != TextureImporterAlphaSource.FromInput)
                {
                    importer.alphaSource = TextureImporterAlphaSource.FromInput;
                    dirty = true;
                }
                if (importer.wrapMode != TextureWrapMode.Repeat)
                {
                    importer.wrapMode = TextureWrapMode.Repeat;
                    dirty = true;
                }
                if (dirty)
                {
                    importer.SaveAndReimport();
                }
            }
        }

        [MenuItem("The Last Ember/Environment/Setup Grass Carpet Asset")]
        public static void SetupGrassCarpetMenu()
        {
            SetupGrassCarpet(true);
        }

        public static void SetupGrassCarpet(bool forceRecreate)
        {
            ConfigureTextureAlpha(GrassTexPath);

            Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            // 1. Grass Carpet Material
            Material grassMat = AssetDatabase.LoadAssetAtPath<Material>(GrassMatPath);
            if (grassMat == null || forceRecreate)
            {
                grassMat = new Material(urpLitShader) { name = "M_Grass_Carpet" };
                Texture2D grassTex = AssetDatabase.LoadAssetAtPath<Texture2D>(GrassTexPath);
                if (grassTex != null)
                {
                    grassMat.SetTexture("_BaseMap", grassTex);
                    grassMat.SetTexture("_MainTex", grassTex);
                }

                grassMat.SetFloat("_AlphaClip", 1f);
                grassMat.SetFloat("_Cutoff", 0.35f);
                grassMat.SetFloat("_Cull", 0f); // Double-sided (Cull Off)
                grassMat.SetFloat("_Smoothness", 0.15f);
                grassMat.SetFloat("_Metallic", 0f);
                grassMat.EnableKeyword("_ALPHATEST_ON");
                grassMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;

                AssetDatabase.CreateAsset(grassMat, GrassMatPath);
            }

            // 2. Grass Carpet Prefab
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(GrassObjPath);
            if (model != null)
            {
                GameObject tempGrass = new GameObject("Grass_Carpet");
                try
                {
                    MeshFilter mf = tempGrass.AddComponent<MeshFilter>();
                    MeshRenderer mr = tempGrass.AddComponent<MeshRenderer>();

                    MeshFilter modelMf = model.GetComponentInChildren<MeshFilter>();
                    if (modelMf != null)
                    {
                        mf.sharedMesh = modelMf.sharedMesh;
                    }

                    mr.sharedMaterial = grassMat;

                    // Trigger collider để nhân vật lướt qua mượt mà không cản đường
                    BoxCollider col = tempGrass.AddComponent<BoxCollider>();
                    col.isTrigger = true;
                    col.center = new Vector3(0f, 0.48f, 0f);
                    col.size = new Vector3(2.75f, 0.95f, 2.45f);

                    PrefabUtility.SaveAsPrefabAsset(tempGrass, GrassPrefabPath);

                    // Đồng thời lưu vào Resources/Environment để runtime nạp tức thì
                    if (!Directory.Exists(GrassResourcesDir))
                    {
                        Directory.CreateDirectory(GrassResourcesDir);
                    }
                    string resPrefabPath = GrassResourcesDir + "Grass_Carpet.prefab";
                    PrefabUtility.SaveAsPrefabAsset(tempGrass, resPrefabPath);

                    Debug.Log("[RealisticTreeBootstrapper] Successfully generated Grass_Carpet.prefab!");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(tempGrass);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

