using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadUSData : MonoBehaviour
{
    List<DataRow> rows = new List<DataRow>();

    // The prefab for the data points to be instantiated
    public GameObject PointPrefab;
    public float scaleX = 3f;
    public float scaleY = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        // TextAssets are READ-ONLY - like a prefab.
        TextAsset unitedStatesAmerica = Resources.Load<TextAsset>("unitedStatesAmerica");

        string[] data = unitedStatesAmerica.text.Split('\n');
        Debug.Log(data.Length);
        
        for (int i = 1; i < data.Length -1; i++)
        {
            string[] csvRow = data[i].Split(',');

            if (csvRow[0] != "")
            {
                DataRow r = new DataRow();
                int.TryParse(csvRow[0], out r.year);
                int.TryParse(csvRow[2], out r.rate);

                rows.Add(r);
            }
        }

        foreach (DataRow r in rows)
        {
            Debug.Log(r.year + " : " + r.rate);

            float xCoord = (r.year - 1980);
            float yCoord = (r.rate);
            float zCoord = 0f;

            // Instantiate prefab
            Instantiate(PointPrefab, new Vector3(xCoord * scaleX, yCoord * scaleY, zCoord), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
