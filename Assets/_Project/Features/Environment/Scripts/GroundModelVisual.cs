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

        [Header("Decorations")]
        [SerializeField] private bool generateDecorations = true;
        [SerializeField] private int treeCount = 46;
        [SerializeField] private int rockCount = 28;
        [SerializeField] private int logCount = 12;
        [SerializeField] private float innerClearRadius = 8f;
        [SerializeField] private float outerRadius = 22f;
        [SerializeField] private int decorationSeed = 190704;
        [SerializeField] private bool logsAreCollectible = true;
        [SerializeField] private int woodPerLog = 1;

        private Mesh generatedMesh;
        private Material[] generatedMaterials;
        private Material treeLeafMaterial;
        private Material treeDarkLeafMaterial;
        private Material trunkMaterial;
        private Material rockMaterial;
        private Material logMaterial;

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
            mesh.SetNormals(normals);
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

            mesh.RecalculateBounds();
            return true;
        }

        private static Material[] CreateMaterials(int count)
        {
            GroundMaterialSpec[] specs =
            {
                new GroundMaterialSpec("Damp Loam", new Color(0.31f, 0.245f, 0.19f), 0.7f, 0f),
                new GroundMaterialSpec("Ash Soil", new Color(0.38f, 0.31f, 0.24f), 0.74f, 0f),
                new GroundMaterialSpec("Muted Moss", new Color(0.28f, 0.38f, 0.235f), 0.66f, 0f),
                new GroundMaterialSpec("Charcoal Stone", new Color(0.19f, 0.185f, 0.175f), 0.58f, 0f),
                new GroundMaterialSpec("Cold Gravel", new Color(0.42f, 0.39f, 0.335f), 0.62f, 0f)
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
            ClearDecorationRoot();
        }

        private void RebuildDecorations()
        {
            ClearDecorationRoot();
            CreateDecorationMaterials();

            Transform root = new GameObject(DecorationRootName).transform;
            root.gameObject.hideFlags = HideFlags.DontSave;
            root.SetParent(transform, false);
            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;
            root.localScale = GetInverseLocalScale(transform.localScale);

            System.Random random = new System.Random(decorationSeed);

            for (int i = 0; i < treeCount; i++)
            {
                Vector3 position = GetDecorPosition(random);
                float scale = RandomRange(random, 0.85f, 1.45f);
                CreateTree(root, position, scale, i);
            }

            for (int i = 0; i < rockCount; i++)
            {
                Vector3 position = GetDecorPosition(random);
                float scale = RandomRange(random, 0.55f, 1.35f);
                CreateRock(root, position, scale, i);
            }

            for (int i = 0; i < logCount; i++)
            {
                Vector3 position = GetDecorPosition(random);
                float scale = RandomRange(random, 0.65f, 1.15f);
                CreateLog(root, position, scale, RandomRange(random, 0f, 360f), i);
            }
        }

        private Vector3 GetDecorPosition(System.Random random)
        {
            float angle = RandomRange(random, 0f, Mathf.PI * 2f);
            float distance = Mathf.Sqrt(RandomRange(random, innerClearRadius * innerClearRadius, outerRadius * outerRadius));
            return new Vector3(Mathf.Cos(angle) * distance, 0.08f, Mathf.Sin(angle) * distance);
        }

        private void CreateTree(Transform parent, Vector3 localPosition, float scale, int index)
        {
            GameObject tree = new GameObject($"Tree_{index:00}");
            tree.hideFlags = HideFlags.DontSave;
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = localPosition;
            tree.transform.localRotation = Quaternion.Euler(0f, (index * 47f) % 360f, 0f);
            tree.transform.localScale = Vector3.one * scale;

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.hideFlags = HideFlags.DontSave;
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            trunk.transform.localScale = new Vector3(0.22f, 0.75f, 0.22f);
            SetRendererMaterial(trunk, trunkMaterial);
            DestroyGeneratedObject(trunk.GetComponent<Collider>());

            Color leafTint = index % 3 == 0 ? treeDarkLeafMaterial.color : treeLeafMaterial.color;
            Material leafMaterial = index % 3 == 0 ? treeDarkLeafMaterial : treeLeafMaterial;
            _ = leafTint;

            CreateCone(tree.transform, "CrownBottom", new Vector3(0f, 1.25f, 0f), 0.92f, 1.05f, leafMaterial);
            CreateCone(tree.transform, "CrownTop", new Vector3(0f, 1.95f, 0f), 0.68f, 0.95f, leafMaterial);
        }

        private void CreateRock(Transform parent, Vector3 localPosition, float scale, int index)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = $"Rock_{index:00}";
            rock.hideFlags = HideFlags.DontSave;
            rock.transform.SetParent(parent, false);
            rock.transform.localPosition = localPosition + new Vector3(0f, 0.18f * scale, 0f);
            rock.transform.localRotation = Quaternion.Euler(index * 23f, index * 61f, 0f);
            rock.transform.localScale = new Vector3(0.75f, 0.38f, 0.58f) * scale;
            SetRendererMaterial(rock, rockMaterial);
            DestroyGeneratedObject(rock.GetComponent<Collider>());
        }

        private void CreateLog(Transform parent, Vector3 localPosition, float scale, float yRotation, int index)
        {
            GameObject log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.name = $"Log_{index:00}";
            log.hideFlags = HideFlags.DontSave;
            log.transform.SetParent(parent, false);
            log.transform.localPosition = localPosition + new Vector3(0f, 0.22f * scale, 0f);
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
