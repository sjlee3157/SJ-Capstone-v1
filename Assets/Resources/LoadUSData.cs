using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadUSData : MonoBehaviour
{
    // Loading CSV data
    List<DataRow> dataRows = new List<DataRow>();
    List<DataRow>[] walls; // Array of DataRow lists grouped by wall

    // Global scale
    static int xPartition = 10; // Split graph into 10 years per wall. Must use int (relies on integer division).
    static float rampSize = 5f; // Z-axis
    float xShift = rampSize; // For square corner platforms
    static float graphWidth = 100f - (rampSize * 2);
    float xOffset;
    float xMax;
    float xRange;
    static float graphHeight = 100f;
    float yOffset;
    float yMax;
    float yRange;

    // Local Scale
    float xLocalOffset;
    float xLocalMax;
    float xLocalRange;

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
    Vector3 vertex0;
    Vector3 vertex1;

    // Combining meshes
    List<CombineInstance> combines;

    // Start is called before the first frame update
    void Start()
    {
        LoadData();
        SetGlobalScale();
        WallifyData();
        PlotDataBalls();
        GenerateRamp();
        CombineRamp();
    }

    void LoadData() {
        // TextAssets are READ-ONLY - like a prefab.
        TextAsset unitedStatesAmerica = Resources.Load<TextAsset>("unitedStatesAmerica");
        string[] rawCSVData = unitedStatesAmerica.text.Split('\n');
        for (int i = 1; i < rawCSVData.Length; i++) { // Start at index 1 to skip the header row
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

     void SetGlobalScale() {
        xOffset = 0 - dataRows[0].year;
        xRange = MaxYear(dataRows) - dataRows[0].year;

        yOffset = 0 - dataRows[0].rate;
        yRange = MaxRate(dataRows) - dataRows[0].rate;
    }

    float MaxYear(List<DataRow> dataRows) {
        float max = 0;
        foreach (DataRow r in dataRows) {
            if (r.year > max) {
                max = r.year;
            }
        }
        return max;
    }

    float MaxRate(List<DataRow> dataRows) {
        float max = 0;
        foreach (DataRow r in dataRows) {
            if (r.rate > max) {
                max = r.rate;
            }
        }
        return max;
    }

    void WallifyData()
    {
        // First we have to initialize types and sizes
        int n = ((int)xRange / xPartition) + 2; // You have to round up!
        walls = new List<DataRow>[n];
        for (int i = 0; i < n; i++)
        {
            walls[i] = new List<DataRow>();
        }
        // Partition the graph by years in a new nested structure called xPartitionedDataRows
        for (int i = 0; i < dataRows.Count; i++)
        {
            if (i == 0) {                       // First item is just the first data point
                walls[0].Add(dataRows[i]);
            }
            else {
                int wall = ((dataRows[i].year - dataRows[0].year) / xPartition) + 1;
                walls[wall].Add(dataRows[i]);
            }
        }
    }

    void PlotDataBalls() {
        // Plot all data points on north wall
        for (int i = 0; i < dataRows.Count; i++)
        {
            DataRow r = dataRows[i];
            xCoord = xShift + (((r.year + xOffset) / xRange) * graphWidth);
            yCoord = ((r.rate + yOffset) / yRange) * graphHeight;

            // Plot the data points
            GameObject dataBall = Instantiate(DataBallPrefab, new Vector3(xCoord, yCoord, 0), Quaternion.identity);
            dotContainer = transform.Find("dotContainer").GetComponent<Transform>();
            dataBall.transform.SetParent(dotContainer);
            dataBall.name = $"DataBall {r.year}, {r.rate}";
        }
    }

    void GenerateRamp() {
        // 1. Loop through walls from i = 1 to i = length - 1
        // 2. Set local scale
        // 3. Loop through wall from j = 0 to j = count - 1
                // Generate mesh with coordinates, add to combines list
                // Store vertex 2 + 3 outside of loop
        // 4. Generate join platform from prev, add to combines list
        // 5. If i == length - 1, generate last join platform, add to combines list
        // 6. Create a GameObject from the combine instances (list to array)
        // 7. Transform GameObject to the correct wall i
    }

    void CombineRamp() {
        // Combine all meshes as combine instances into one ramp GameObject
    }
    
    // void GenerateRamp() {
    
    //     // First, iterate by wall
    //     for (int wallCombineListsIndex = 0; wallCombineListsIndex < xPartitionedDataRows.Length; wallCombineListsIndex++) {
            
    //         // Initialize the null array that will hold lists of combine instances
    //         wallCombineLists[wallCombineListsIndex] = new List<CombineInstance>();
            
    //         List<DataRow> localDataRows = xPartitionedDataRows[wallCombineListsIndex];
            
    //         // Calculate scale for this wall
    //         float xLocalOffset = 0 - localDataRows[0].year;
    //         float xLocalRange = MaxYear(localDataRows) - localDataRows[0].year;

    //         // float xLocalOffset = xOffset;
    //         // float xLocalRange = xRange;

    //          // Next, iterate by row
    //         for (int localIndex = 0; localIndex < localDataRows.Count; localIndex++) {
    //             if (wallCombineListsIndex > 0 || localIndex > 0) { // Skip the first dot since there's no connection
    //                 DataRow r = localDataRows[localIndex];
    //                 xCoord = xShift + (((r.year + xLocalOffset) / xLocalRange ) * graphWidth);
    //                 yCoord = ((r.rate + yOffset) / yRange ) * graphHeight;
                    
    //                 bool isLastRampOnWall = (localIndex == (localDataRows.Count - 1));
                    
    //                 CombineInstance combine = new CombineInstance();
    //                 combine.mesh = GenerateDotConnectionMesh(isLastRampOnWall); 
                    
    //                 wallCombineLists[wallCombineListsIndex].Add(combine);
    //             }
    //         }

    //         // Finally, add join platforms by wall (takes 1 argument - wall index)
    //         // Debug.Log("going into join platform function");
    //         GenerateCornerJoinPlatforms(wallCombineListsIndex);
    //     }
    //     // Reset vertices
    //     vertex0 = new Vector3(rampSize, 0, 0);
    //     vertex1 = new Vector3(rampSize, 0, 0 - rampSize);
    // }

    // Mesh GenerateDotConnectionMesh(bool isLastRampOnWall) {
    //     // Debug.Log(xCoord);
    //     // Debug.Log(yCoord);
    //     Mesh mesh = new Mesh();
    //     List<Vector3> vertexList = new List<Vector3>() {
    //         vertex0,
    //         vertex1,
    //         new Vector3(xCoord, yCoord, 0),
    //         new Vector3(xCoord, yCoord, 0 - rampSize)
    //     };
    //     mesh.SetVertices(vertexList);
    //     mesh.SetTriangles(new List<int>() {
    //         0, 2, 1,
    //         2, 3, 1
    //     }, 0);
    //     // TODO: check Normals and UVs
    //     mesh.SetNormals(new List<Vector3>() { -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward});
    //     mesh.SetUVs(0, new List<Vector2>() { // For texture mapping
    //         new Vector2(0, 0),
    //         new Vector2(1, 0),
    //         new Vector2(0, 1),
    //         new Vector2(1, 1),
    //     });
    //     // Save current vertices for generating the next 
    //     vertex0 = vertexList[2];
    //     vertex1 = vertexList[3];
    //     // Add vertex to corner join platform list if it's the last ramp on the wall
    //     if (isLastRampOnWall) {
    //         cornerPlatformVertices.Add(vertexList[2]);
    //     }
    //     return mesh;
    // }

    // void GenerateCornerJoinPlatforms(int wallCombineListsIndex) {
    //     // Debug.Log($"Corner vertices {cornerPlatformVertices.Count}");
    //     foreach (Vector3 vertex0 in cornerPlatformVertices) {
    //         CombineInstance combine = new CombineInstance();
    //         combine.mesh = GenerateCornerJoinPlatformMesh(vertex0);
    //         wallCombineLists[wallCombineListsIndex].Add(combine);
    //     }
    //     // Important! Clear vertex list before moving on to the next wall:
    //     cornerPlatformVertices.Clear();
    // }

    // Mesh GenerateCornerJoinPlatformMesh(Vector3 vertex0) {
    //     Mesh mesh = new Mesh();
    //     List<Vector3> vertexList = new List<Vector3>() {
    //         vertex0,
    //         new Vector3(vertex0.x, vertex0.y, 0 - rampSize),
    //         new Vector3(vertex0.x + rampSize, vertex0.y, 0),
    //         new Vector3(vertex0.x + rampSize, vertex0.y, 0 - rampSize)
    //     };
    //     mesh.SetVertices(vertexList);
    //     mesh.SetTriangles(new List<int>() {
    //         0, 2, 1,
    //         2, 3, 1
    //     }, 0);
    //     // TODO: check Normals and UVs
    //     mesh.SetNormals(new List<Vector3>() { -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward});
    //     mesh.SetUVs(0, new List<Vector2>() { // For texture mapping
    //         new Vector2(0, 0),
    //         new Vector2(1, 0),
    //         new Vector2(0, 1),
    //         new Vector2(1, 1),
    //     });
    //     return mesh;
    // }

    // void CreateAndTransformGameObjects() {
    //     Tower = transform.Find("Tower").GetComponent<Transform>();
    //     Tower_Wall_1_North = Tower.Find("Tower_Wall_1_North").GetComponent<Transform>();
    //     Tower_Wall_2_East = Tower.Find("Tower_Wall_2_East").GetComponent<Transform>();
    //     Tower_Wall_3_South = Tower.Find("Tower_Wall_3_South").GetComponent<Transform>();
    //     Tower_Wall_4_West = Tower.Find("Tower_Wall_4_West").GetComponent<Transform>();

    //     Wall_1_Graph_Container = Tower_Wall_1_North.Find("Wall_1_Graph_Container").GetComponent<Transform>();
    //     Wall_2_Graph_Container = Tower_Wall_2_East.Find("Wall_2_Graph_Container").GetComponent<Transform>();
    //     Wall_3_Graph_Container = Tower_Wall_3_South.Find("Wall_3_Graph_Container").GetComponent<Transform>();
    //     Wall_4_Graph_Container = Tower_Wall_4_West.Find("Wall_4_Graph_Container").GetComponent<Transform>();

    //     for (int i = 0; i < wallCombineLists.Length; i++) {
    //         int wallNumber = i + 1;
    //         GameObject combinedRamp = CombineMeshes(i);
    //         MapToWall(wallNumber, combinedRamp);
    //     }
    // }

    // GameObject CombineMeshes(int wallCombineListsIndex) {
    //     // Combine the ramp meshes into a game object called "Graphed Ramp"
    //     Mesh combinedMesh = new Mesh();
    //     combinedMesh.name = "combinedRampsMesh";
    //     combinedMesh.CombineMeshes(wallCombineLists[wallCombineListsIndex].ToArray(), true, false);
    //     combinedMesh.RecalculateNormals();

    //     // Create a new Game Object to hold the combined target object
    //     GameObject combinedRamp = new GameObject();
    //     combinedRamp.name = "GraphedRamp";
        
    //     // Add mesh filter, renderer, and collider components
    //     combinedRamp.AddComponent<MeshFilter>().sharedMesh = combinedMesh;
    //     combinedRamp.AddComponent<MeshRenderer>().sharedMaterial = new Material(DotConnectionRampMaterial);
    //     MeshCollider collider = combinedRamp.AddComponent<MeshCollider>();
    //     collider.sharedMesh = combinedMesh;
        
    //     return combinedRamp;
    // }

    // void MapToWall(int wallNumber, GameObject combinedRamp) {
    //     switch(wallNumber)
    //     {
    //         case 1:
    //             combinedRamp.transform.SetParent(Wall_1_Graph_Container);
    //             combinedRamp.transform.position = Tower_Wall_1_North.InverseTransformPoint(Tower_Wall_1_North.position);
    //             Wall_1_Graph_Container.transform.localRotation = Tower_Wall_1_North.rotation;
    //             break;
    //         case 2: 
    //             combinedRamp.transform.SetParent(Wall_2_Graph_Container);
    //             combinedRamp.transform.position = Tower_Wall_1_North.InverseTransformPoint(Tower_Wall_2_East.position);
    //             Wall_2_Graph_Container.transform.localRotation = Tower_Wall_2_East.rotation;
    //             break;
    //         case 3:
    //             combinedRamp.transform.SetParent(Wall_3_Graph_Container);
    //             combinedRamp.transform.position = Tower_Wall_1_North.InverseTransformPoint(Tower_Wall_3_South.position);
    //             Wall_3_Graph_Container.transform.localRotation = Tower_Wall_3_South.rotation;
    //             break;
    //         case 4:
    //             combinedRamp.transform.SetParent(Wall_4_Graph_Container);
    //             combinedRamp.transform.position = Tower_Wall_1_North.InverseTransformPoint(Tower_Wall_4_West.position);
    //             Wall_4_Graph_Container.transform.localRotation = Tower_Wall_4_West.rotation;
    //             break;
    //     } 
    // }
}
