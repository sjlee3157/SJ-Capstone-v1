using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadUSData : MonoBehaviour
{
    // Loading CSV data
    List<DataRow> dataRows = new List<DataRow>();

    // Scale
    static float rampSize = 5f; // Z-axis
    static float graphWidth = 100f - (rampSize * 2);
    float xOffset;
    float xRange;
    float xShift = rampSize;
    static float graphHeight = 100f;
    float yOffset;
    float yRange;

    // Prefabs, Materials, etc.
    public GameObject DataBallPrefab;
    public Material DotConnectionRampMaterial;

    // Showing graph
    Transform Tower;
    Transform dotContainer;

    Transform Tower_Wall_1_North;
    Transform Tower_Wall_2_East;
    Transform Tower_Wall_3_South;
    Transform Tower_Wall_4_West;

    Transform Wall_1_Graph_Container;
    Transform Wall_2_Graph_Container;
    Transform Wall_3_Graph_Container;
    Transform Wall_4_Graph_Container;
   
    float xCoord;
    float yCoord;

    // Store previous Vertex 2 and 3 outside of the loop that generates meshes for dot connections
    Vector3 vertex0 = new Vector3(rampSize, 0, 0);
    Vector3 vertex1 = new Vector3(rampSize, 0, 0 - rampSize);

    // Showing Corner Join Platforms
    List<Vector3> cornerPlatformVertices = new List<Vector3>() { new Vector3(0, 0, 0) };

    // Combining meshes
    List<CombineInstance> rampCombineInstanceList = new List<CombineInstance>();

    // Start is called before the first frame update
    void Start()
    {
        LoadGraph();
        CalculateScale(); // calculate wrap, to optimize for corner join platforms
        ShowGraph();
        ShowCornerJoinPlatforms();
        CombineMeshes();
    }

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
        xOffset = - dataRows[0].year;
        xRange = MaxYear() - dataRows[0].year;

        yOffset = 0 - dataRows[0].rate;
        yRange = MaxRate() - dataRows[0].rate;

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
            foreach (DataRow r in dataRows) {
                if (r.rate > max) {
                    max = r.rate;
                }
            }
            return max;
        }
    }
    
    void ShowGraph() {
        Tower = transform.Find("Tower").GetComponent<Transform>();
        dotContainer = transform.Find("dotContainer").GetComponent<Transform>();

        Tower_Wall_1_North = Tower.Find("Tower_Wall_1_North").GetComponent<Transform>();
        Tower_Wall_2_East = Tower.Find("Tower_Wall_2_East").GetComponent<Transform>();
        Tower_Wall_3_South = Tower.Find("Tower_Wall_3_South").GetComponent<Transform>();
        Tower_Wall_4_West = Tower.Find("Tower_Wall_4_West").GetComponent<Transform>();

        Wall_1_Graph_Container = Tower_Wall_1_North.Find("Wall_1_Graph_Container").GetComponent<Transform>();
        Wall_2_Graph_Container = Tower_Wall_2_East.Find("Wall_2_Graph_Container").GetComponent<Transform>();
        Wall_3_Graph_Container = Tower_Wall_3_South.Find("Wall_3_Graph_Container").GetComponent<Transform>();
        Wall_4_Graph_Container = Tower_Wall_4_West.Find("Wall_4_Graph_Container").GetComponent<Transform>();

        for (int i = 0; i < dataRows.Count; i++) {
            DataRow r = dataRows[i];
            xCoord = xShift + (((r.year + xOffset) / xRange ) * graphWidth);
            yCoord = ((r.rate + yOffset) / yRange ) * graphHeight;

            // Plot the data points
            GameObject dataBall = Instantiate(DataBallPrefab, new Vector3(xCoord, yCoord, 0), Quaternion.identity);
            dataBall.transform.SetParent(dotContainer);
            dataBall.name = $"DataBall {r.year}, {r.rate}";

            // Generate the dot connections (ramps) between each data point (skip the first dot)
            if (i > 0) {
                CombineInstance combine = new CombineInstance();
                combine.mesh = GenerateDotConnectionMesh(i + 1);
                rampCombineInstanceList.Add(combine);
            }
        }
    }

    Mesh GenerateDotConnectionMesh(int dataPointNumber) {
        Mesh mesh = new Mesh();
        List<Vector3> vertexList = new List<Vector3>() {
            vertex0,
            vertex1,
            new Vector3(xCoord, yCoord, 0),
            new Vector3(xCoord, yCoord, 0 - rampSize)
        };
        mesh.SetVertices(vertexList);
        mesh.SetTriangles(new List<int>() {
                    0, 2, 1,
                    2, 3, 1
                }, 0);
        // TODO: check Normals and UVs
        mesh.SetNormals(new List<Vector3>() { -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward});
        mesh.SetUVs(0, new List<Vector2>() { // For texture mapping
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                });
        // Save current vertices for generating the next 
        vertex0 = vertexList[2];
        vertex1 = vertexList[3];
        // Add vertex to corner list if modulo is correct
        if (dataPointNumber % 12 == 0) {
            cornerPlatformVertices.Add(vertexList[2]);
        }
        return mesh;
    }

    void ShowCornerJoinPlatforms() {
        foreach (Vector3 vertex0 in cornerPlatformVertices) {
            CombineInstance combine = new CombineInstance();
            combine.mesh = GenerateCornerJoinPlatformMesh(vertex0);
            rampCombineInstanceList.Add(combine);
        }
    }

    Mesh GenerateCornerJoinPlatformMesh(Vector3 vertex0) {
        Mesh mesh = new Mesh();
        List<Vector3> vertexList = new List<Vector3>() {
            vertex0,
            new Vector3(vertex0.x, vertex0.y, 0 - rampSize),
            new Vector3(vertex0.x + rampSize, vertex0.y, 0),
            new Vector3(vertex0.x + rampSize, vertex0.y, 0 - rampSize)
        };
        mesh.SetVertices(vertexList);
        mesh.SetTriangles(new List<int>() {
                    0, 2, 1,
                    2, 3, 1
                }, 0);
        // TODO: check Normals and UVs
        mesh.SetNormals(new List<Vector3>() { -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward});
        mesh.SetUVs(0, new List<Vector2>() { // For texture mapping
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                });
        return mesh;
    }

    void CombineMeshes() {
        // Combine the ramp meshes into a game object called "Graphed Ramp"
        Mesh combinedMesh = new Mesh();
        combinedMesh.name = "combinedRampsMesh";
        combinedMesh.CombineMeshes(rampCombineInstanceList.ToArray(), true, false);
        combinedMesh.RecalculateNormals();

        GameObject combinedRamp = new GameObject();
        combinedRamp.name = "GraphedRamp";
        combinedRamp.transform.SetParent(Wall_3_Graph_Container);
        // combinedRamp.transform.SetParent(Wall_1_Graph_Container);

        combinedRamp.AddComponent<MeshFilter>().sharedMesh = combinedMesh;
        combinedRamp.AddComponent<MeshRenderer>().sharedMaterial = new Material(DotConnectionRampMaterial);
        MeshCollider collider = combinedRamp.AddComponent<MeshCollider>();
        collider.cookingOptions -= MeshColliderCookingOptions.EnableMeshCleaning;
        collider.sharedMesh = combinedMesh;

        combinedRamp.transform.position = Tower_Wall_1_North.InverseTransformPoint(Tower_Wall_3_South.position);
        Wall_3_Graph_Container.transform.localRotation = Tower_Wall_3_South.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
