using System;
using System.Collections.Generic;
using TheLastEmber.Features.Economy;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLastEmber.Features.Environment
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GroundModelVisual : MonoBehaviour
    {
        [SerializeField] private string resourcePath = "Ground/dat_colored_mesh";
        [SerializeField] private bool rebuildOnEnable = true;
        [SerializeField] private bool updateCollider = true;

        [Header("3D Environment Models")]
        [Tooltip("Các mẫu cây 3D. Tự nạp Tree_Small, Tree_Medium từ Resources/Environment nếu để trống.")]
        [SerializeField] private GameObject[] treePrefabs;
        [Tooltip("Các mẫu bụi cỏ 3D. Tự nạp TallGrass, Bush từ Resources/Environment nếu để trống.")]
        [SerializeField] private GameObject[] grassPrefabs;

        [Header("Decorations (Bản đồ mở rộng x4)")]
        [SerializeField] private bool generateDecorations = true;
        [SerializeField] private int treeCount = 160;
        [Tooltip("Độ lún của gốc cây vào lòng đất để thân cây chạm đất tự nhiên, kín khít 360 độ")]
        [SerializeField] private float treeRootSink = 0.20f;
        [SerializeField] private int grassCount = 1600;
        [Tooltip("Đảo ngược chiều cỏ (mặc định false: cỏ mọc thẳng đứng từ mặt đất lên trời)")]
        [SerializeField] private bool invertGrass = false;
        [SerializeField] private int rockCount = 38;
        [SerializeField] private int logCount = 20;
        [SerializeField] private float innerClearRadius = 18f;
        [SerializeField] private float outerRadius = 92f;
        [SerializeField] private int decorationSeed = 190704;
        [SerializeField] private bool logsAreCollectible = true;
        [SerializeField] private int woodPerLog = 1;
        [SerializeField] private bool rocksAreMineable = true;
        [SerializeField] private bool treesAreChoppable = true;

        private Mesh generatedMesh;
        private Material[] generatedMaterials;
        private Material treeLeafMaterial;
        private Material treeDarkLeafMaterial;
        private Material trunkMaterial;
        private Material rockMaterial;
        private Material logMaterial;
        private Mesh grassCarpetMesh;
        private Material grassCarpetMaterial;

        private const string DecorationRootName = "__GeneratedTerrainDecor";

        private void OnEnable()
        {
            if (rebuildOnEnable)
            {
                Rebuild();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!isActiveAndEnabled) return;
            Rebuild();
        }
#endif

        private void OnDisable()
        {
            ReleaseGeneratedAssets();
        }

        [ContextMenu("Rebuild Ground Visual")]
        public void Rebuild()
        {
            TextAsset meshData = Resources.Load<TextAsset>(resourcePath);
            if (meshData == null)
            {
                Debug.LogWarning($"[GroundModelVisual] Mesh resource not found: Resources/{resourcePath}", this);
                return;
            }

            if (!TryBuildMesh(meshData.bytes, out Mesh mesh, out int subMeshCount))
            {
                Debug.LogWarning("[GroundModelVisual] Could not read ground mesh data.", this);
                return;
            }

            ReleaseGeneratedAssets();

            generatedMesh = mesh;
            generatedMaterials = CreateMaterials(subMeshCount);

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshFilter.sharedMesh = generatedMesh;
            meshRenderer.sharedMaterials = generatedMaterials;

            if (updateCollider && TryGetComponent(out MeshCollider meshCollider))
            {
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = generatedMesh;
            }

            if (generateDecorations)
            {
                RebuildDecorations();
            }
        }

        private static bool TryBuildMesh(byte[] bytes, out Mesh mesh, out int subMeshCount)
        {
            mesh = null;
            subMeshCount = 0;

            if (bytes == null || bytes.Length < 16) return false;
            if (bytes[0] != 'T' || bytes[1] != 'L' || bytes[2] != 'E' || bytes[3] != 'F') return false;

            int offset = 8;
            int version = ReadInt(bytes, ref offset);
            if (version != 1) return false;

            int vertexCount = ReadInt(bytes, ref offset);
            if (vertexCount <= 0) return false;

            List<Vector3> vertices = new List<Vector3>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
            {
                vertices.Add(ReadVector3(bytes, ref offset));
            }

            List<Vector3> normals = new List<Vector3>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
            {
                normals.Add(ReadVector3(bytes, ref offset));
            }

            List<Vector2> uvs = new List<Vector2>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
            {
                uvs.Add(ReadVector2(bytes, ref offset));
            }

            subMeshCount = ReadInt(bytes, ref offset);
            if (subMeshCount <= 0) return false;

            mesh = new Mesh
            {
                name = "Ground_Dat_ColoredModel",
                indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16,
                hideFlags = HideFlags.DontSave
            };

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = subMeshCount;

            for (int subMesh = 0; subMesh < subMeshCount; subMesh++)
            {
                ReadInt(bytes, ref offset);
                int indexCount = ReadInt(bytes, ref offset);
                int[] indices = new int[indexCount];

                for (int i = 0; i < indexCount; i++)
                {
                    indices[i] = ReadInt(bytes, ref offset);
                }

                mesh.SetTriangles(indices, subMesh, true);
            }

            // Tự tính lại Normals chuẩn từ hình học thay vì dùng normal ngược từ file model
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return true;
        }

        private static Material[] CreateMaterials(int count)
        {
            // Bảng màu Dark Fantasy hài hòa: cỏ rừng rêu phong tối, đất than củi, sỏi xám tự nhiên
            // Giảm độ bóng (smoothness thấp ~0.15) để mặt đất không bị bóng nhờn nhựa
            GroundMaterialSpec[] specs =
            {
                new GroundMaterialSpec("Dark Forest Grass", new Color(0.18f, 0.28f, 0.16f), 0.15f, 0f),
                new GroundMaterialSpec("Deep Moss", new Color(0.14f, 0.23f, 0.13f), 0.12f, 0f),
                new GroundMaterialSpec("Forest Loam", new Color(0.22f, 0.18f, 0.14f), 0.15f, 0f),
                new GroundMaterialSpec("Charcoal Soil", new Color(0.15f, 0.13f, 0.12f), 0.10f, 0f),
                new GroundMaterialSpec("Ancient Slate", new Color(0.22f, 0.23f, 0.25f), 0.18f, 0f)
            };

            Material[] materials = new Material[count];
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            for (int i = 0; i < count; i++)
            {
                GroundMaterialSpec spec = specs[Mathf.Min(i, specs.Length - 1)];
                Material material = new Material(shader)
                {
                    name = $"Ground {spec.Name}",
                    hideFlags = HideFlags.DontSave
                };

                SetColor(material, "_BaseColor", spec.BaseColor);
                SetColor(material, "_Color", spec.BaseColor);
                SetFloat(material, "_Smoothness", spec.Smoothness);
                SetFloat(material, "_Metallic", spec.Metallic);

                materials[i] = material;
            }

            return materials;
        }

        private void ReleaseGeneratedAssets()
        {
            if (generatedMesh != null)
            {
                DestroyGeneratedObject(generatedMesh);
                generatedMesh = null;
            }

            if (generatedMaterials != null)
            {
                foreach (Material material in generatedMaterials)
                {
                    if (material != null)
                    {
                        DestroyGeneratedObject(material);
                    }
                }

                generatedMaterials = null;
            }

            DestroyGeneratedMaterial(ref treeLeafMaterial);
            DestroyGeneratedMaterial(ref treeDarkLeafMaterial);
            DestroyGeneratedMaterial(ref trunkMaterial);
            DestroyGeneratedMaterial(ref rockMaterial);
            DestroyGeneratedMaterial(ref logMaterial);
            DestroyGeneratedMaterial(ref grassCarpetMaterial);
            if (grassCarpetMesh != null)
            {
                DestroyGeneratedObject(grassCarpetMesh);
                grassCarpetMesh = null;
            }
            ClearDecorationRoot();
        }

        private void RebuildDecorations()
        {
            ClearDecorationRoot();
            CreateDecorationMaterials();
            Resolve3DModelAssets();

            Transform root = new GameObject(DecorationRootName).transform;
            root.gameObject.hideFlags = HideFlags.DontSave;
            root.SetParent(transform, false);
            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;
            root.localScale = GetInverseLocalScale(transform.localScale);

            System.Random random = new System.Random(decorationSeed);

            // 1. Phân bổ Cây 3D (kích thước chuẩn 5.5m - 8.5m, rễ cắm sâu vào lòng đất)
            for (int i = 0; i < treeCount; i++)
            {
                Vector3 position = GetDecorPosition(random, innerClearRadius, outerRadius);
                float variation = RandomRange(random, 0.88f, 1.15f);
                CreateTree(root, position, variation, i);
            }

            // 2. Phân bổ Bụi cỏ 3D từ grass_carpet.glb (thảm cỏ rộng 2.5m - 3.5m, cao 0.9m - 1.3m)
            for (int i = 0; i < grassCount; i++)
            {
                Vector3 position = GetDecorPosition(random, innerClearRadius * 0.65f, outerRadius * 0.98f);
                float scale = RandomRange(random, 0.92f, 1.35f);
                CreateGrass(root, position, scale, i, random);
            }

            // 3. Phân bổ Mỏ đá (StoneNode)
            for (int i = 0; i < rockCount; i++)
            {
                Vector3 position = GetDecorPosition(random, innerClearRadius, outerRadius);
                float scale = RandomRange(random, 0.65f, 1.45f);
                CreateRock(root, position, scale, i);
            }

            // 4. Phân bổ Khúc gỗ nhặt củi (CollectibleResource)
            for (int i = 0; i < logCount; i++)
            {
                Vector3 position = GetDecorPosition(random, innerClearRadius, outerRadius);
                float scale = RandomRange(random, 0.75f, 1.25f);
                CreateLog(root, position, scale, RandomRange(random, 0f, 360f), i);
            }
        }

        private float GetGroundSurfaceHeight(float worldX, float worldZ)
        {
            Ray ray = new Ray(new Vector3(worldX, 30f, worldZ), Vector3.down);
            if (TryGetComponent(out MeshCollider mc) && mc.sharedMesh != null && mc.Raycast(ray, out RaycastHit hit, 60f))
            {
                return hit.point.y;
            }
            return 0.12f;
        }

        private void Resolve3DModelAssets()
        {
            bool needReloadTrees = treePrefabs == null || treePrefabs.Length == 0;
            if (!needReloadTrees)
            {
                for (int i = 0; i < treePrefabs.Length; i++)
                {
                    if (treePrefabs[i] == null) { needReloadTrees = true; break; }
                    MeshFilter mf = treePrefabs[i].GetComponentInChildren<MeshFilter>();
                    if (mf == null || mf.sharedMesh == null) { needReloadTrees = true; break; }
                }
            }

            if (needReloadTrees)
            {
                List<GameObject> trees = new List<GameObject>();
                GameObject tree1 = Resources.Load<GameObject>("Environment/Tree_Small");
                GameObject tree2 = Resources.Load<GameObject>("Environment/Tree_Medium");
                GameObject tree3 = Resources.Load<GameObject>("Environment/Tree_Large");
                if (IsValidTreePrefab(tree1)) trees.Add(tree1);
                if (IsValidTreePrefab(tree2)) trees.Add(tree2);
                if (IsValidTreePrefab(tree3)) trees.Add(tree3);

#if UNITY_EDITOR
                if (trees.Count == 0)
                {
                    GameObject t1 = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Environment/Tree_Small.prefab");
                    GameObject t2 = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Environment/Tree_Medium.prefab");
                    GameObject t3 = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Environment/Tree_Large.prefab");
                    if (IsValidTreePrefab(t1)) trees.Add(t1);
                    if (IsValidTreePrefab(t2)) trees.Add(t2);
                    if (IsValidTreePrefab(t3)) trees.Add(t3);
                }
#endif
                if (trees.Count > 0) treePrefabs = trees.ToArray();
            }

            if (grassPrefabs == null || grassPrefabs.Length == 0)
            {
                List<GameObject> grasses = new List<GameObject>();
                GameObject g0 = Resources.Load<GameObject>("Environment/Grass_Carpet");
                if (g0 != null) grasses.Add(g0);

#if UNITY_EDITOR
                if (grasses.Count == 0)
                {
                    GameObject gArt = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Environment/Grass_Carpet.prefab");
                    if (gArt != null) grasses.Add(gArt);
                }
#endif
                if (grasses.Count > 0) grassPrefabs = grasses.ToArray();
            }
        }

        private static bool IsValidTreePrefab(GameObject prefab)
        {
            if (prefab == null) return false;
            MeshFilter mf = prefab.GetComponentInChildren<MeshFilter>();
            return mf != null && mf.sharedMesh != null;
        }

        private static Vector3 GetDecorPosition(System.Random random, float minRadius, float maxRadius)
        {
            float angle = RandomRange(random, 0f, Mathf.PI * 2f);
            float distance = Mathf.Sqrt(RandomRange(random, minRadius * minRadius, maxRadius * maxRadius));
            return new Vector3(Mathf.Cos(angle) * distance, 0.08f, Mathf.Sin(angle) * distance);
        }

        private void CreateTree(Transform parent, Vector3 localPosition, float randomVariation, int index)
        {
            float groundY = GetGroundSurfaceHeight(localPosition.x, localPosition.z);

            // Sử dụng trực tiếp file 3D Model / Prefab của các mẫu cây
            if (treePrefabs != null && treePrefabs.Length > 0)
            {
                GameObject prefab = treePrefabs[index % treePrefabs.Length];
                if (prefab != null)
                {
                    // Thu nhỏ cây về kích thước cân đối chuẩn game 3D (cao ~5.5m - 8.5m)
                    float baseScale = 0.28f;
                    string pName = prefab.name.ToLower();
                    if (pName.Contains("small"))
                    {
                        baseScale = 0.35f; // tree_small gốc 16.5m * 0.35 = ~5.8m tall, tán 6.5m
                    }
                    else if (pName.Contains("medium"))
                    {
                        baseScale = 0.25f; // tree_medium gốc 30.5m * 0.25 = ~7.6m tall, tán 6.5m
                    }
                    else if (pName.Contains("large"))
                    {
                        baseScale = 0.20f; // tree_large gốc 38.5m * 0.20 = ~7.7m tall, tán 6.4m
                    }

                    float finalScale = baseScale * randomVariation;

                    GameObject treeInstance = Instantiate(prefab, parent);
                    treeInstance.name = $"Tree_{index:00}_{prefab.name}";
                    treeInstance.hideFlags = HideFlags.DontSave;

                    MeshFilter mf = treeInstance.GetComponentInChildren<MeshFilter>();
                    MeshRenderer mr = treeInstance.GetComponentInChildren<MeshRenderer>();

#if UNITY_EDITOR
                    // Tự phục hồi Mesh trực tiếp từ OBJ nếu prefab bị mất mesh trong Editor
                    if (mf != null && mf.sharedMesh == null)
                    {
                        string objFileName = pName.Contains("large") ? "tree_large.obj" : (pName.Contains("medium") ? "tree_medium.obj" : "tree_small.obj");
                        GameObject rawModel = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Art/Environment/{objFileName}");
                        if (rawModel != null)
                        {
                            MeshFilter rawMf = rawModel.GetComponentInChildren<MeshFilter>();
                            if (rawMf != null) mf.sharedMesh = rawMf.sharedMesh;
                        }
                    }
#endif

                    // Nếu có Mesh hợp lệ thì đặt cây vào đúng mặt đất
                    if (mf != null && mf.sharedMesh != null)
                    {
                        if (mr != null && (mr.sharedMaterials == null || mr.sharedMaterials.Length == 0 || mr.sharedMaterials[0] == null))
                        {
                            mr.sharedMaterials = new Material[] { trunkMaterial, treeLeafMaterial };
                        }

                        // Cắm gốc cây nhẹ 0.12m vào mặt đất để thân cây chạm đất 360 độ tự nhiên
                        float sink = treeRootSink * (pName.Contains("large") ? 1.3f : 1.0f);
                        treeInstance.transform.position = new Vector3(localPosition.x, groundY - sink, localPosition.z);
                        treeInstance.transform.localRotation = Quaternion.Euler(0f, (index * 47f) % 360f, 0f);
                        treeInstance.transform.localScale = Vector3.one * finalScale;

                        if (treesAreChoppable && Application.isPlaying && treeInstance.GetComponent<TreeResourceNode>() == null)
                        {
                            treeInstance.AddComponent<TreeResourceNode>();
                        }
                        return;
                    }
                    else
                    {
                        // Nếu instance không có Mesh, huỷ để rơi xuống fallback đảm bảo luôn có cây hiển thị!
                        DestroyGeneratedObject(treeInstance);
                    }
                }
            }

            // Fallback nếu chưa tải được asset 3D: Dựng cây thông 3D vững chãi, tán xanh 2 tầng
            float fallbackScale = 0.35f * randomVariation;
            GameObject tree = new GameObject($"Tree_{index:00}");
            tree.hideFlags = HideFlags.DontSave;
            tree.transform.SetParent(parent, false);
            tree.transform.position = new Vector3(localPosition.x, groundY - 0.10f, localPosition.z);
            tree.transform.localRotation = Quaternion.Euler(0f, (index * 47f) % 360f, 0f);
            tree.transform.localScale = Vector3.one * fallbackScale;

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.hideFlags = HideFlags.DontSave;
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            trunk.transform.localScale = new Vector3(0.22f, 0.75f, 0.22f);
            SetRendererMaterial(trunk, trunkMaterial);
            DestroyGeneratedObject(trunk.GetComponent<Collider>());

            Material leafMaterial = index % 2 == 0 ? treeDarkLeafMaterial : treeLeafMaterial;
            CreateCone(tree.transform, "CrownBottom", new Vector3(0f, 1.25f, 0f), 0.92f, 1.05f, leafMaterial);
            CreateCone(tree.transform, "CrownTop", new Vector3(0f, 1.95f, 0f), 0.68f, 0.95f, leafMaterial);

            if (treesAreChoppable && Application.isPlaying)
            {
                tree.AddComponent<TreeResourceNode>();
            }
        }

        private void CreateGrass(Transform parent, Vector3 localPosition, float scale, int index, System.Random random)
        {
            float groundY = GetGroundSurfaceHeight(localPosition.x, localPosition.z);
            float randomYaw = RandomRange(random, 0f, 360f);
            Vector3 grassPos;
            Quaternion grassRot;

            if (invertGrass)
            {
                // Chiều ngược (nếu muốn lộn 180 độ)
                float grassHeight = 0.95f * scale;
                grassPos = new Vector3(localPosition.x, groundY + grassHeight - 0.02f, localPosition.z);
                grassRot = Quaternion.Euler(180f, randomYaw, 0f);
            }
            else
            {
                // Mọc chuẩn từ mặt đất vươn thẳng lên trời
                grassPos = new Vector3(localPosition.x, groundY - 0.02f, localPosition.z);
                grassRot = Quaternion.Euler(0f, randomYaw, 0f);
            }

            // 1. Sử dụng trực tiếp Prefab Grass_Carpet nếu có
            if (grassPrefabs != null && grassPrefabs.Length > 0)
            {
                GameObject prefab = grassPrefabs[index % grassPrefabs.Length];
                if (prefab != null)
                {
                    GameObject grassInstance = Instantiate(prefab, parent);
                    grassInstance.name = $"Grass_Carpet_{index:00}_{prefab.name}";
                    grassInstance.hideFlags = HideFlags.DontSave;

                    grassInstance.transform.position = grassPos;
                    grassInstance.transform.localRotation = grassRot;
                    grassInstance.transform.localScale = Vector3.one * scale;

                    // Cho phép chạy qua cỏ mượt mà (Trigger Collider, không chặn di chuyển)
                    Collider[] colliders = grassInstance.GetComponentsInChildren<Collider>();
                    foreach (Collider c in colliders)
                    {
                        c.isTrigger = true;
                    }
                    return;
                }
            }

            // 2. Sử dụng trực tiếp dữ liệu Mesh 3D và Texture từ grass_carpet.glb
            if (grassCarpetMesh != null && grassCarpetMaterial != null)
            {
                GameObject grassInstance = new GameObject($"Grass_Carpet_{index:00}");
                grassInstance.hideFlags = HideFlags.DontSave;
                grassInstance.transform.SetParent(parent, false);
                grassInstance.transform.position = grassPos;
                grassInstance.transform.localRotation = grassRot;
                grassInstance.transform.localScale = Vector3.one * scale;

                MeshFilter mf = grassInstance.AddComponent<MeshFilter>();
                MeshRenderer mr = grassInstance.AddComponent<MeshRenderer>();
                mf.sharedMesh = grassCarpetMesh;
                mr.sharedMaterial = grassCarpetMaterial;

                BoxCollider col = grassInstance.AddComponent<BoxCollider>();
                col.isTrigger = true;
                col.center = new Vector3(0f, 0.48f, 0f);
                col.size = new Vector3(2.75f, 0.95f, 2.45f);
                return;
            }

            // Fallback tạo bụi cỏ 3D nếu chưa nạp được asset
            CreateProceduralGrassTuft(parent, localPosition, scale, index, random);
        }

        private void CreateProceduralGrassTuft(Transform parent, Vector3 localPosition, float scale, int index, System.Random random)
        {
            float groundY = GetGroundSurfaceHeight(localPosition.x, localPosition.z);
            GameObject grass = new GameObject($"GrassTuft_{index:00}");
            grass.hideFlags = HideFlags.DontSave;
            grass.transform.SetParent(parent, false);
            grass.transform.position = new Vector3(localPosition.x, groundY - 0.02f, localPosition.z);
            grass.transform.localRotation = Quaternion.Euler(0f, RandomRange(random, 0f, 360f), 0f);
            grass.transform.localScale = Vector3.one * (scale * 0.35f);

            MeshFilter mf = grass.AddComponent<MeshFilter>();
            MeshRenderer mr = grass.AddComponent<MeshRenderer>();
            mf.sharedMesh = GetOrCreateGrassMesh();
            mr.sharedMaterial = index % 2 == 0 ? treeLeafMaterial : treeDarkLeafMaterial;
        }

        private static Mesh cachedGrassMesh;
        private static Mesh GetOrCreateGrassMesh()
        {
            if (cachedGrassMesh != null) return cachedGrassMesh;

            int bladeCount = 3;
            Vector3[] vertices = new Vector3[bladeCount * 4];
            int[] triangles = new int[bladeCount * 12];
            Vector2[] uvs = new Vector2[bladeCount * 4];

            int vertIdx = 0;
            int triIdx = 0;

            for (int b = 0; b < bladeCount; b++)
            {
                float angle = b * (Mathf.PI / bladeCount);
                Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.6f;

                int baseV = vertIdx;
                vertices[vertIdx++] = -dir;
                vertices[vertIdx++] = dir;
                vertices[vertIdx++] = -dir * 0.7f + Vector3.up * 0.85f;
                vertices[vertIdx++] = dir * 0.7f + Vector3.up * 0.85f;

                uvs[baseV + 0] = new Vector2(0f, 0f);
                uvs[baseV + 1] = new Vector2(1f, 0f);
                uvs[baseV + 2] = new Vector2(0f, 1f);
                uvs[baseV + 3] = new Vector2(1f, 1f);

                triangles[triIdx++] = baseV + 0;
                triangles[triIdx++] = baseV + 2;
                triangles[triIdx++] = baseV + 1;
                triangles[triIdx++] = baseV + 1;
                triangles[triIdx++] = baseV + 2;
                triangles[triIdx++] = baseV + 3;

                triangles[triIdx++] = baseV + 0;
                triangles[triIdx++] = baseV + 1;
                triangles[triIdx++] = baseV + 2;
                triangles[triIdx++] = baseV + 1;
                triangles[triIdx++] = baseV + 3;
                triangles[triIdx++] = baseV + 2;
            }

            cachedGrassMesh = new Mesh
            {
                name = "Procedural_3D_GrassCluster",
                hideFlags = HideFlags.DontSave
            };
            cachedGrassMesh.SetVertices(vertices);
            cachedGrassMesh.SetUVs(0, uvs);
            cachedGrassMesh.SetTriangles(triangles, 0);
            cachedGrassMesh.RecalculateNormals();
            cachedGrassMesh.RecalculateBounds();
            return cachedGrassMesh;
        }

        private void CreateRock(Transform parent, Vector3 localPosition, float scale, int index)
        {
            float groundY = GetGroundSurfaceHeight(localPosition.x, localPosition.z);
            GameObject modelPrefab = Resources.Load<GameObject>("ThirdParty/Kenney/rock_largeA");
            GameObject rock;

            if (modelPrefab != null)
            {
                rock = Instantiate(modelPrefab, parent);
                rock.name = $"Rock_{index:00}";
                rock.hideFlags = HideFlags.DontSave;
                rock.transform.position = new Vector3(localPosition.x, groundY, localPosition.z);
                rock.transform.localRotation = Quaternion.Euler(0f, index * 67f, 0f);
                rock.transform.localScale = Vector3.one * (scale * 0.85f);
            }
            else
            {
                rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = $"Rock_{index:00}";
                rock.hideFlags = HideFlags.DontSave;
                rock.transform.SetParent(parent, false);
                rock.transform.position = new Vector3(localPosition.x, groundY + 0.12f * scale, localPosition.z);
                rock.transform.localRotation = Quaternion.Euler(index * 23f, index * 61f, 0f);
                rock.transform.localScale = new Vector3(0.75f, 0.38f, 0.58f) * scale;
                SetRendererMaterial(rock, rockMaterial);
            }

            if (rocksAreMineable && Application.isPlaying)
            {
                // Cho phép khai thác đá trong Play Mode
                if (rock.GetComponent<Collider>() == null)
                {
                    rock.AddComponent<BoxCollider>();
                }
                rock.AddComponent<StoneNode>();
            }
            else
            {
                DestroyGeneratedObject(rock.GetComponent<Collider>());
            }
        }

        private void CreateLog(Transform parent, Vector3 localPosition, float scale, float yRotation, int index)
        {
            float groundY = GetGroundSurfaceHeight(localPosition.x, localPosition.z);
            GameObject log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.name = $"Log_{index:00}";
            log.hideFlags = HideFlags.DontSave;
            log.transform.SetParent(parent, false);
            log.transform.position = new Vector3(localPosition.x, groundY + 0.15f * scale, localPosition.z);
            log.transform.localRotation = Quaternion.Euler(0f, yRotation, 90f);
            log.transform.localScale = new Vector3(0.28f, 0.85f, 0.28f) * scale;
            SetRendererMaterial(log, logMaterial);

            Collider logCollider = log.GetComponent<Collider>();
            if (logsAreCollectible)
            {
                if (logCollider != null)
                {
                    logCollider.isTrigger = true;
                }

                CollectibleResource collectible = log.AddComponent<CollectibleResource>();
                collectible.Configure(ResourceType.Wood, woodPerLog, true, false);
            }
            else if (logCollider != null)
            {
                DestroyGeneratedObject(logCollider);
            }
        }

        private void CreateCone(Transform parent, string name, Vector3 localPosition, float radius, float height, Material material)
        {
            GameObject cone = new GameObject(name);
            cone.hideFlags = HideFlags.DontSave;
            cone.transform.SetParent(parent, false);
            cone.transform.localPosition = localPosition;

            MeshFilter meshFilter = cone.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = cone.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = CreateConeMesh(radius, height, 8);
            meshFilter.sharedMesh.hideFlags = HideFlags.DontSave;
            meshRenderer.sharedMaterial = material;
        }

        private static Mesh CreateConeMesh(float radius, float height, int sides)
        {
            Vector3[] vertices = new Vector3[sides + 2];
            int[] triangles = new int[sides * 6];

            vertices[0] = Vector3.up * height;
            vertices[1] = Vector3.zero;

            for (int i = 0; i < sides; i++)
            {
                float angle = (Mathf.PI * 2f * i) / sides;
                vertices[i + 2] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            }

            int triangleIndex = 0;
            for (int i = 0; i < sides; i++)
            {
                int current = i + 2;
                int next = i == sides - 1 ? 2 : i + 3;

                triangles[triangleIndex++] = 0;
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = current;

                triangles[triangleIndex++] = 1;
                triangles[triangleIndex++] = current;
                triangles[triangleIndex++] = next;
            }

            Mesh mesh = new Mesh { name = "Generated Low Poly Cone" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void CreateDecorationMaterials()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            treeLeafMaterial = CreateMaterial(shader, "Decor Pine Needles", new Color(0.17f, 0.31f, 0.18f));
            treeDarkLeafMaterial = CreateMaterial(shader, "Decor Dark Needles", new Color(0.11f, 0.22f, 0.16f));
            trunkMaterial = CreateMaterial(shader, "Decor Trunk", new Color(0.31f, 0.19f, 0.12f));
            rockMaterial = CreateMaterial(shader, "Decor Rock", new Color(0.32f, 0.32f, 0.3f));
            logMaterial = CreateMaterial(shader, "Decor Fallen Log", new Color(0.36f, 0.21f, 0.13f));

            // Load dữ liệu Mesh gốc từ grass_carpet.glb (dưới dạng binary resource)
            TextAsset grassCarpetData = Resources.Load<TextAsset>("Environment/grass_carpet_mesh");
            if (grassCarpetData != null && TryBuildMesh(grassCarpetData.bytes, out Mesh gMesh, out _))
            {
                gMesh.name = "Grass_Carpet_GLTF_Mesh";
                grassCarpetMesh = gMesh;
            }

            // Load Texture và tạo Material URP Lit Alpha Clipping hai mặt cho Grass Carpet
            Texture2D grassTex = Resources.Load<Texture2D>("Environment/grass_carpet_diffuse");
#if UNITY_EDITOR
            if (grassTex == null)
            {
                grassTex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Environment/grass_carpet_diffuse.png");
            }
#endif
            grassCarpetMaterial = new Material(shader)
            {
                name = "M_Grass_Carpet_Runtime",
                hideFlags = HideFlags.DontSave
            };
            if (grassTex != null)
            {
                SetTexture(grassCarpetMaterial, "_BaseMap", grassTex);
                SetTexture(grassCarpetMaterial, "_MainTex", grassTex);
            }
            else
            {
                SetColor(grassCarpetMaterial, "_BaseColor", new Color(0.24f, 0.38f, 0.19f));
                SetColor(grassCarpetMaterial, "_Color", new Color(0.24f, 0.38f, 0.19f));
            }

            SetFloat(grassCarpetMaterial, "_AlphaClip", 1f);
            SetFloat(grassCarpetMaterial, "_AlphaToMask", 1f); // Alpha-To-Coverage via hardware MSAA
            SetFloat(grassCarpetMaterial, "_Cutoff", 0.35f);
            SetFloat(grassCarpetMaterial, "_Cull", 0f); // Two-sided render
            SetFloat(grassCarpetMaterial, "_Smoothness", 0.15f);
            SetFloat(grassCarpetMaterial, "_Metallic", 0f);
            grassCarpetMaterial.EnableKeyword("_ALPHATEST_ON");
            grassCarpetMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
        }

        private static Material CreateMaterial(Shader shader, string name, Color color)
        {
            Material material = new Material(shader)
            {
                name = name,
                hideFlags = HideFlags.DontSave
            };

            SetColor(material, "_BaseColor", color);
            SetColor(material, "_Color", color);
            SetFloat(material, "_Smoothness", 0.35f);
            return material;
        }

        private void ClearDecorationRoot()
        {
            Transform existing = transform.Find(DecorationRootName);
            if (existing != null)
            {
                MeshFilter[] meshFilters = existing.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter meshFilter in meshFilters)
                {
                    if (meshFilter.sharedMesh != null && meshFilter.sharedMesh.name == "Generated Low Poly Cone")
                    {
                        DestroyGeneratedObject(meshFilter.sharedMesh);
                    }
                }

                DestroyGeneratedObject(existing.gameObject);
            }
        }

        private void DestroyGeneratedMaterial(ref Material material)
        {
            if (material == null) return;
            DestroyGeneratedObject(material);
            material = null;
        }

        private static void SetRendererMaterial(GameObject target, Material material)
        {
            if (target.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.sharedMaterial = material;
            }
        }

        private static float RandomRange(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        private static Vector3 GetInverseLocalScale(Vector3 scale)
        {
            return new Vector3(
                Mathf.Approximately(scale.x, 0f) ? 1f : 1f / scale.x,
                Mathf.Approximately(scale.y, 0f) ? 1f : 1f / scale.y,
                Mathf.Approximately(scale.z, 0f) ? 1f : 1f / scale.z);
        }

        private static void DestroyGeneratedObject(UnityEngine.Object target)
        {
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
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, color);
            }
        }

        private static void SetFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void SetTexture(Material material, string propertyName, Texture texture)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static int ReadInt(byte[] bytes, ref int offset)
        {
            int value = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            return value;
        }

        private static Vector3 ReadVector3(byte[] bytes, ref int offset)
        {
            float x = BitConverter.ToSingle(bytes, offset);
            float y = BitConverter.ToSingle(bytes, offset + sizeof(float));
            float z = BitConverter.ToSingle(bytes, offset + sizeof(float) * 2);
            offset += sizeof(float) * 3;
            return new Vector3(x, y, z);
        }

        private static Vector2 ReadVector2(byte[] bytes, ref int offset)
        {
            float x = BitConverter.ToSingle(bytes, offset);
            float y = BitConverter.ToSingle(bytes, offset + sizeof(float));
            offset += sizeof(float) * 2;
            return new Vector2(x, y);
        }

        private readonly struct GroundMaterialSpec
        {
            public GroundMaterialSpec(string name, Color baseColor, float smoothness, float metallic)
            {
                Name = name;
                BaseColor = baseColor;
                Smoothness = smoothness;
                Metallic = metallic;
            }

            public string Name { get; }
            public Color BaseColor { get; }
            public float Smoothness { get; }
            public float Metallic { get; }
        }
    }
}
