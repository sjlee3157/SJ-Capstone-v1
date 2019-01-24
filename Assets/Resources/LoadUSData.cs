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
    static float rampSize = 15f; // Z-axis
    float xShift = rampSize;
    static float graphWidth = 100f - (rampSize * 2);
    float xOffset;
    float xMax;
    float xRange;
    static float graphHeight = 90f;
    float yOffset;
    float yMax;
    float yRange;

    // Local Scale
    float xLocalOffset;
    float xLocalMax;
    float xLocalRange;

    // Prefabs, Materials, etc.
    [SerializeField] private Material DotConnectionRampMaterial;

    // Showing graph
    float xCoord;
    float yCoord;

    // Store previous Vertex 2 outside of the loop that generates meshes for dot connections (ramps)
    Vector3 vertex0 = Vector3.zero;

    // Combining meshes
    List<CombineInstance> combines;

    // References
    Transform Tower_Wall_1_North;
    Transform Tower_Wall_2_East;
    Transform Tower_Wall_3_South;
    Transform Tower_Wall_4_West;

    Transform Wall_1_Graph_Container;
    Transform Wall_2_Graph_Container;
    Transform Wall_3_Graph_Container;
    Transform Wall_4_Graph_Container;

    // Start is called before the first frame update
    void Start()
    {
        Tower_Wall_1_North = GameObject.Find("Tower_Wall_1_North").GetComponent<Transform>();
        Tower_Wall_2_East = GameObject.Find("Tower_Wall_2_East").GetComponent<Transform>();
        Tower_Wall_3_South = GameObject.Find("Tower_Wall_3_South").GetComponent<Transform>();
        Tower_Wall_4_West = GameObject.Find("Tower_Wall_4_West").GetComponent<Transform>();

        Wall_1_Graph_Container = GameObject.Find("Wall_1_Graph_Container").GetComponent<Transform>();
        Wall_2_Graph_Container = GameObject.Find("Wall_2_Graph_Container").GetComponent<Transform>();
        Wall_3_Graph_Container = GameObject.Find("Wall_3_Graph_Container").GetComponent<Transform>();
        Wall_4_Graph_Container = GameObject.Find("Wall_4_Graph_Container").GetComponent<Transform>();

        LoadData();
        SetGlobalScale();
        WallifyData();
        GenerateRamp();
    }

    void LoadData() 
    {
        // TextAssets are READ-ONLY - like a prefab.
        TextAsset unitedStatesAmerica = Resources.Load<TextAsset>("unitedStatesAmerica");
        string[] rawCSVData = unitedStatesAmerica.text.Split('\n');

        for (int i = 1; i < rawCSVData.Length; i++) 
        { // Start at index 1 to skip the header row
            string[] csvRow = rawCSVData[i].Split(',');

            // If the first column of the row isn't an empty string
            if (csvRow[0] != "") 
            { 
                // Grab Column 0 (Year) and Column 2 (Prison population rate) from CSV
                DataRow r = new DataRow();
                int.TryParse(csvRow[0], out r.year);
                int.TryParse(csvRow[2], out r.rate);

                dataRows.Add(r);
            }
        }
    }

     void SetGlobalScale() 
     {
        xOffset = 0 - dataRows[0].year;
        xRange = dataRows[dataRows.Count - 1].year - dataRows[0].year;

        yOffset = 0 - dataRows[0].rate;

        yRange = MaxRate(dataRows) - MinRate(dataRows);
        // Debug.Log(MinRate(dataRows));
    }

    float MaxRate(List<DataRow> dataRows) 
    {
        float max = 0;
        foreach (DataRow r in dataRows) 
        {
            if (r.rate > max) 
            {
                max = r.rate;
            }
        }
        return max;
    }

    float MinRate(List<DataRow> dataRows) 
    {
        float min = dataRows[0].rate;
        foreach (DataRow r in dataRows) 
        {
            if (r.rate < min) 
            {
                min = r.rate;
            }
        }
        return min;
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

        // Partition the graph by years in a new nested structure called "walls"
        for (int i = 0; i < dataRows.Count; i++) 
        {
            if (i == 0) {                       // First item is just the first data point
                walls[0].Add(dataRows[i]);
            }
            else {
                int wallNumber = (((dataRows[i].year - dataRows[0].year) -1) / xPartition) + 1;
                List<DataRow> wall = walls[wallNumber];
                wall.Add(dataRows[i]);
            }
        }
    }

    // void PlotDataBalls() {
    //     // Plot all data points on north wall (for reference)
    //     for (int i = 0; i < dataRows.Count; i++) 
    //     {
    //         DataRow r = dataRows[i];
    //         xCoord = xShift + (((r.year + xOffset) / xRange) * graphWidth);
    //         yCoord = ((r.rate + yOffset) / yRange) * graphHeight;

    //         // Plot the data points
    //         GameObject dataBall = Instantiate(DataBallPrefab1, new Vector3(xCoord, yCoord, 0), Quaternion.identity);
    //         dataBall.transform.SetParent(dotContainer);
    //         dataBall.name = $"DataBall {r.year}, {r.rate}";
    //     }
    // }

    void GenerateRamp() 
    {
        for (int i = 1; i < walls.Length; i++) 
        {
            List<DataRow> wall = walls[i]; 
            
            DataRow carryover = walls[i-1][walls[i-1].Count - 1];
            xLocalOffset = 0 - carryover.year;
            xLocalMax = wall[wall.Count - 1].year;
            xLocalRange = xLocalMax - carryover.year;

            // Build the Start join platform mesh:
            xCoord = vertex0.x + rampSize;
            yCoord = vertex0.y;
            List<CombineInstance> combines = new List<CombineInstance>();
            CombineInstance combine = new CombineInstance();
            combine.mesh = BuildRampMesh();
            combines.Add(combine);

            for (int j = 0; j < wall.Count; j++)
            {
                DataRow r = wall[j];

                xCoord = xShift + ((r.year + xLocalOffset) / xLocalRange) * graphWidth;
                yCoord = ((r.rate + yOffset) / yRange) * graphHeight;

                combine = new CombineInstance(); 
                combine.mesh = BuildRampMesh();
                combines.Add(combine);

                // If this is the last wall, build the End join platform mesh:
                if (i == walls.Length - 1 && j == wall.Count - 1)
                {
                    xCoord = vertex0.x + rampSize;
                    yCoord = vertex0.y;
                    combine = new CombineInstance();
                    combine.mesh = BuildRampMesh();
                    combines.Add(combine);
                }    
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.name = $"RampMesh_Wall{i}";
            combinedMesh.CombineMeshes(combines.ToArray(), true, false);
            combinedMesh.RecalculateNormals();

            // Create a new Game Object to hold the combined target object
            GameObject combinedRamp = new GameObject();
            combinedRamp.name = $"Ramp_Wall{i}";
            combinedRamp.tag = "ramp"; // TODO: Remove tag (not being used)

            // Add mesh filter, renderer, and collider components
            combinedRamp.AddComponent<MeshFilter>().sharedMesh = combinedMesh;
            combinedRamp.AddComponent<MeshRenderer>().sharedMaterial = new Material(DotConnectionRampMaterial);
            MeshCollider collider = combinedRamp.AddComponent<MeshCollider>();
            collider.sharedMesh = combinedMesh;
            collider.convex = true;
            collider.tag = "ramp";

            // Transform GameObject to the correct wall i
            MapToWall(i, combinedRamp);

            // Important! Reset vertex0 only in the x direction.
            vertex0.x = 0;
        }
    }

    Mesh BuildRampMesh()
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertexList = new List<Vector3>() 
        {
            vertex0,
            new Vector3(vertex0.x, vertex0.y, 0 - rampSize),
            new Vector3(xCoord, yCoord, 0),
            new Vector3(xCoord, yCoord, 0 - rampSize)
        };
        mesh.SetVertices(vertexList);
        mesh.SetTriangles(new List<int>() 
        {
            0, 2, 1,
            2, 3, 1
        }, 0);
        mesh.SetUVs(0, new List<Vector2>() { // For texture mapping
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
        });
        vertex0 = vertexList[2];
        return mesh;
    }

    void MapToWall(int wallNumber, GameObject combinedRamp)
    {
        switch(wallNumber)
        {
            case 1:
                combinedRamp.transform.SetParent(Wall_1_Graph_Container);
                combinedRamp.transform.position = Tower_Wall_1_North.InverseTransformPoint(Tower_Wall_1_North.position);
                Wall_1_Graph_Container.transform.localRotation = Tower_Wall_1_North.rotation;
                break;
            case 2: 
                combinedRamp.transform.SetParent(Wall_2_Graph_Container);
                combinedRamp.transform.position = Tower_Wall_1_North.InverseTransformPoint(Tower_Wall_2_East.position);
                Wall_2_Graph_Container.transform.localRotation = Tower_Wall_2_East.rotation;
                break;
            case 3:
                combinedRamp.transform.SetParent(Wall_3_Graph_Container);
                combinedRamp.transform.position = Tower_Wall_1_North.InverseTransformPoint(Tower_Wall_3_South.position);
                Wall_3_Graph_Container.transform.localRotation = Tower_Wall_3_South.rotation;
                break;
            case 4:
                combinedRamp.transform.SetParent(Wall_4_Graph_Container);
                combinedRamp.transform.position = Tower_Wall_1_North.InverseTransformPoint(Tower_Wall_4_West.position);
                Wall_4_Graph_Container.transform.localRotation = Tower_Wall_4_West.rotation;
                break;
        } 
    }

    void Update()
    {

    }
}