using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLastEmber.Features.DayNightCycle
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class FurnaceModelVisual : MonoBehaviour
    {
        [SerializeField] private string resourcePath = "Furnace/base_colored_mesh";
        [SerializeField] private bool rebuildOnEnable = true;

        private Mesh generatedMesh;
        private Material[] generatedMaterials;

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

        [ContextMenu("Rebuild Furnace Visual")]
        public void Rebuild()
        {
            TextAsset meshData = Resources.Load<TextAsset>(resourcePath);
            if (meshData == null)
            {
                Debug.LogWarning($"[FurnaceModelVisual] Mesh resource not found: Resources/{resourcePath}", this);
                return;
            }

            if (!TryBuildMesh(meshData.bytes, out Mesh mesh, out int subMeshCount))
            {
                Debug.LogWarning("[FurnaceModelVisual] Could not read furnace mesh data.", this);
                return;
            }

            ReleaseGeneratedAssets();

            generatedMesh = mesh;
            generatedMaterials = CreateMaterials(subMeshCount);

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshFilter.sharedMesh = generatedMesh;
            meshRenderer.sharedMaterials = generatedMaterials;
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
                name = "GreatFurnace_ColoredModel",
                indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16,
                hideFlags = HideFlags.DontSave
            };

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = subMeshCount;

            for (int subMesh = 0; subMesh < subMeshCount; subMesh++)
            {
                ReadInt(bytes, ref offset); // material index, kept aligned with generated material order.
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
            FurnaceMaterialSpec[] specs =
            {
                new FurnaceMaterialSpec("Basalt Stone", new Color(0.12f, 0.105f, 0.095f), 0f),
                new FurnaceMaterialSpec("Lava Core", new Color(0.95f, 0.18f, 0.02f), 0.65f),
                new FurnaceMaterialSpec("Blackened Iron", new Color(0.055f, 0.058f, 0.06f), 0f),
                new FurnaceMaterialSpec("Crimson Banners", new Color(0.34f, 0.025f, 0.025f), 0f),
                new FurnaceMaterialSpec("Ash Trim", new Color(0.22f, 0.19f, 0.17f), 0f),
                new FurnaceMaterialSpec("Ember Windows", new Color(0.95f, 0.34f, 0.06f), 0.45f)
            };

            Material[] materials = new Material[count];
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            for (int i = 0; i < count; i++)
            {
                FurnaceMaterialSpec spec = specs[Mathf.Min(i, specs.Length - 1)];
                Material material = new Material(shader)
                {
                    name = $"Furnace {spec.Name}",
                    hideFlags = HideFlags.DontSave
                };

                SetColor(material, "_BaseColor", spec.BaseColor);
                SetColor(material, "_Color", spec.BaseColor);

                if (spec.EmissionStrength > 0f)
                {
                    Color emissionColor = spec.BaseColor * spec.EmissionStrength;
                    SetColor(material, "_EmissionColor", emissionColor);
                    material.EnableKeyword("_EMISSION");
                }

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

        private readonly struct FurnaceMaterialSpec
        {
            public FurnaceMaterialSpec(string name, Color baseColor, float emissionStrength)
            {
                Name = name;
                BaseColor = baseColor;
                EmissionStrength = emissionStrength;
            }

            public string Name { get; }
            public Color BaseColor { get; }
            public float EmissionStrength { get; }
        }
    }
}
