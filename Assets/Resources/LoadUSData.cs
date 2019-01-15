using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadUSData : MonoBehaviour
{
    List<DataRow> dataRows = new List<DataRow>();

    // The prefab for the data points to be instantiated
    public GameObject DataBallPrefab;
    public Material DotConnectionRampMaterial;
    public Material CornerJoinPlatformMaterial;

    private Transform graphContainer;

    float stepDepth = -10f;

    float graphWidth = 100f;
    float xOffset = 0f;
    float xMaximum = 0f;

    float graphHeight = 100f;
    float yOffset = 0f;
    float yMaximum = 0f;

    // Start is called before the first frame update
    void Start()
    {
        LoadGraph();
        CalculateScale(); // calculate wrap, to optimize for corner join platforms
        // Add Wall Corner Join Platform to dataRows
        ShowGraph();

        void LoadGraph() {
            // TextAssets are READ-ONLY - like a prefab.
            TextAsset unitedStatesAmerica = Resources.Load<TextAsset>("unitedStatesAmerica");
            string[] rawCSVData = unitedStatesAmerica.text.Split('\n');
            for (int i = 1; i < rawCSVData.Length - 1; i++) {
                string[] csvRow = rawCSVData[i].Split(',');
                if (csvRow[0] != "") { // If the first column of the row isn't an empty string
                    // Grab Column 0 (Year) and Column 2 (Prison population rate) from CSV
                    DataRow r = new DataRow();
                    int.TryParse(csvRow[0], out r.year);
                    int.TryParse(csvRow[2], out r.rate);
                    dataRows.Add(r);
                }
            }
        }

        void CalculateScale() {
            xOffset = dataRows[0].year;
            xMaximum = MaxYear();

            yOffset = dataRows[0].rate;
            yMaximum = MaxRate();

            float MaxYear() {
                float max = 0;
                foreach (DataRow r in dataRows) {
                    if (r.year > max) {
                        max = r.year;
                    }
                }
                return max;
            }

            float MaxRate() {
                float max = 0;
                foreach (DataRow r in dataRows)
                {
                    if (r.rate > max) {
                        max = r.rate;
                    }
                }
                return max;
            }
        }
       
        void ShowGraph() {
            // Store previous Vertex 2 and 3 outside of step generating loop
            Vector3 vertex0 = new Vector3(0, 0, 0);
            Vector3 vertex1 = new Vector3(0, 0, stepDepth);

            graphContainer = transform.Find("graphContainer").GetComponent<Transform>();

            // Loop through data to instantiate meshes
            for (int i = 0; i < dataRows.Count; i++) {
                DataRow r = dataRows[i];
                float xCoord = ((r.year - xOffset) / (xMaximum - xOffset) ) * graphWidth;
                float yCoord = ((r.rate - yOffset) / (yMaximum - yOffset) ) * graphHeight;

                Debug.Log("PLOTTING " + r.year + " : " + r.rate);
                Debug.Log("X COORD: " + xCoord + "; Y COORD: " + yCoord);

                // Instantiate prefab DataBall point
                GameObject dataBall = Instantiate(DataBallPrefab, new Vector3(xCoord, yCoord, 0), Quaternion.identity);
                dataBall.transform.SetParent(graphContainer);
                dataBall.name = $"DataBall {r.year}, {r.rate}";

                // Generate dot connector between each point (skip first)
                if (i > 0) {
                    Mesh dotConnectionMesh = GenerateRampMesh();

                    // Create a new dot connection ramp at each data point
                    GameObject newDotConnection = new GameObject();
                    if (i == 1) {
                    newDotConnection.name = "Wall Corner Join Platform";
                    } else {
                        newDotConnection.name = $"Dot Connection Ramp {i - 1}: {r.year}, {r.rate}";
                    }
                    newDotConnection.transform.SetParent(graphContainer);
                    // newDotConnection.transform.localPosition = new Vector3(10, 0, 0);

                    MeshRenderer renderer = newDotConnection.AddComponent<MeshRenderer>();
                    // TODO: ternary
                    if (i == 1) {
                        renderer.sharedMaterial = new Material(CornerJoinPlatformMaterial);
                    } else {
                        renderer.sharedMaterial = new Material(DotConnectionRampMaterial);
                    }

                    MeshFilter filter = newDotConnection.AddComponent<MeshFilter>();
                    filter.sharedMesh = dotConnectionMesh;

                    // Give the dot connection ramp a collider and mesh so we can walk on it
                    MeshCollider collider = newDotConnection.AddComponent<MeshCollider>();
                    collider.cookingOptions -= MeshColliderCookingOptions.EnableMeshCleaning;
                    collider.sharedMesh = dotConnectionMesh;
                }

                Mesh GenerateRampMesh()
                {
                    Mesh mesh = new Mesh();
                    mesh.name = "dotConnectionMesh";
                    mesh.SetVertices(new List<Vector3>() {
                                vertex0,
                                vertex1,
                                new Vector3(xCoord, yCoord, 0),
                                new Vector3(xCoord, yCoord, stepDepth)
                            });
                    mesh.SetTriangles(new List<int>() {
                                0, 2, 1,
                                2, 3, 1
                            }, 0);
                    mesh.SetNormals(new List<Vector3>() { Vector3.up, Vector3.up, Vector3.up, Vector3.up });
                    mesh.SetUVs(0, new List<Vector2>() {
                                new Vector2(0, 0),
                                new Vector2(1, 0),
                                new Vector2(0, 1),
                                new Vector2(1, 1),
                            });
                    // Save current vertices for generating the next new Ramp
                    Vector3[] vertices = mesh.vertices;
                    vertex0 = vertices[2];
                    vertex1 = vertices[3];

                    return mesh;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
