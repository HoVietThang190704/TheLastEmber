using System;
using System.Collections.Generic;
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
                new GroundMaterialSpec("Damp Soil", new Color(0.39f, 0.285f, 0.19f), 0.82f, 0f),
                new GroundMaterialSpec("Dry Soil", new Color(0.55f, 0.42f, 0.27f), 0.9f, 0f),
                new GroundMaterialSpec("Patchy Grass", new Color(0.29f, 0.43f, 0.22f), 0.78f, 0f),
                new GroundMaterialSpec("Dark Pebbles", new Color(0.17f, 0.17f, 0.16f), 0.68f, 0f),
                new GroundMaterialSpec("Pale Gravel", new Color(0.55f, 0.52f, 0.45f), 0.74f, 0f)
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
