using System.Collections;
using UnityEngine;

namespace TheLastEmber.Core.Visual
{
    /// <summary>
    /// Hiệu ứng vệt chém lưỡi liềm (Slash Arc Visual) xuất hiện phía trước Player khi vung kiếm.
    /// Tạo mesh vòng cung mỏng phát sáng và mờ dần trong tích tắc.
    /// </summary>
    public class SlashArcVisual : MonoBehaviour
    {
        private static SlashArcVisual instance;
        public static SlashArcVisual Instance => instance;

        private Mesh arcMesh;
        private Material arcMaterial;
        private GameObject arcObject;
        private MeshRenderer arcRenderer;
        private Coroutine slashRoutine;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            BuildArcMesh();
            CreateArcObject();
        }

        public static void PlaySlash(Vector3 playerPosition, Vector3 playerForward, float radius = 1.4f)
        {
            if (Instance == null) return;
            Instance.TriggerSlash(playerPosition, playerForward, radius);
        }

        private void TriggerSlash(Vector3 playerPos, Vector3 forward, float radius)
        {
            if (arcObject == null) return;

            if (slashRoutine != null)
            {
                StopCoroutine(slashRoutine);
            }

            slashRoutine = StartCoroutine(SlashRoutine(playerPos, forward, radius));
        }

        private IEnumerator SlashRoutine(Vector3 playerPos, Vector3 forward, float radius)
        {
            arcObject.SetActive(true);
            Vector3 center = playerPos + Vector3.up * 0.8f + forward * (radius * 0.5f);
            arcObject.transform.position = center;
            arcObject.transform.rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0f, 0f, Random.Range(-12f, 12f));

            float duration = 0.14f;
            float elapsed = 0f;

            Color baseColor = new Color(1f, 0.85f, 0.45f, 0.95f);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                float alpha = Mathf.Sin((1f - progress) * Mathf.PI * 0.5f);
                Color c = baseColor;
                c.a = alpha;

                if (arcMaterial != null)
                {
                    if (arcMaterial.HasProperty("_BaseColor")) arcMaterial.SetColor("_BaseColor", c);
                    else if (arcMaterial.HasProperty("_Color")) arcMaterial.SetColor("_Color", c);
                }

                // Hơi phóng to nhẹ trong khi chém
                arcObject.transform.localScale = Vector3.one * Mathf.Lerp(0.85f, 1.25f, progress);
                yield return null;
            }

            arcObject.SetActive(false);
            slashRoutine = null;
        }

        private void CreateArcObject()
        {
            arcObject = new GameObject("__SlashArcHolder");
            arcObject.transform.SetParent(transform, false);
            arcObject.hideFlags = HideFlags.DontSave;

            MeshFilter mf = arcObject.AddComponent<MeshFilter>();
            mf.sharedMesh = arcMesh;

            arcRenderer = arcObject.AddComponent<MeshRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Standard");

            arcMaterial = new Material(shader)
            {
                name = "SlashArcMaterial",
                hideFlags = HideFlags.DontSave
            };

            // Thiết lập chế độ Transparent
            arcMaterial.SetFloat("_Surface", 1f); // Transparent in URP
            arcMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            arcMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            arcMaterial.SetInt("_ZWrite", 0);
            arcMaterial.renderQueue = 3000;

            arcRenderer.sharedMaterial = arcMaterial;
            arcObject.SetActive(false);
        }

        private void BuildArcMesh()
        {
            arcMesh = new Mesh
            {
                name = "SlashArcMesh",
                hideFlags = HideFlags.DontSave
            };

            int segments = 16;
            float innerRadius = 0.8f;
            float outerRadius = 1.35f;
            float angleSpan = 110f; // Vòng cung 110 độ
            float startAngle = -angleSpan * 0.5f;

            Vector3[] vertices = new Vector3[(segments + 1) * 2];
            int[] triangles = new int[segments * 6];

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float angle = (startAngle + t * angleSpan) * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                vertices[i * 2] = new Vector3(sin * innerRadius, 0f, cos * innerRadius);
                vertices[i * 2 + 1] = new Vector3(sin * outerRadius, 0f, cos * outerRadius);

                if (i < segments)
                {
                    int triIdx = i * 6;
                    int vIdx = i * 2;

                    triangles[triIdx] = vIdx;
                    triangles[triIdx + 1] = vIdx + 1;
                    triangles[triIdx + 2] = vIdx + 2;

                    triangles[triIdx + 3] = vIdx + 1;
                    triangles[triIdx + 4] = vIdx + 3;
                    triangles[triIdx + 5] = vIdx + 2;
                }
            }

            arcMesh.vertices = vertices;
            arcMesh.triangles = triangles;
            arcMesh.RecalculateNormals();
            arcMesh.RecalculateBounds();
        }
    }
}
