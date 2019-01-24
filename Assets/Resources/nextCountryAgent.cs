using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nextCountryAgent : MonoBehaviour
{
    // Loading next country
    // List<AgentDataRow> agentDataRows = new List<AgentDataRow>();
    
    Dictionary<int,float> rateDictionary = new Dictionary<int,float>();
    Dictionary<int,string> countryDictionary = new Dictionary<int,string>();

    private string rampName;
    public DirectedAgent directedAgent;
    GameObject fpc;

    Rigidbody rb;
    GameObject RBCast;

    Vector3 rbPreviousPos;

    public GameObject DataBallPrefab;

    // Start is called before the first frame update
    void Start()
    {
        fpc = GameObject.Find("FPSController");
        RBCast = GameObject.Find("RBCast");
        rb = RBCast.GetComponent<Rigidbody>();

        LoadNextCountry();
    }

    void LoadNextCountry()
    {
        TextAsset countryRanks = Resources.Load<TextAsset>("countryRanks");
        string[] rawCountryRanksData = countryRanks.text.Split('\n');

        for (int i = 1; i < rawCountryRanksData.Length; i++)
        { // Start at index 1 to skip the header row
            string[] csvRow = rawCountryRanksData[i].Split(',');

            // If the first column of the row isn't an empty string
            if (csvRow[0] != "")
            {
                // Grab Column 0 (Year), Column 1 (Country), and Column 2 (Rate) from CSV
                // AgentDataRow r = new AgentDataRow();
                int year;
                float rate;
                string country;

                int.TryParse(csvRow[0], out year);
                country = csvRow[1];
                float.TryParse(csvRow[2], out rate);

                rateDictionary.Add(year,rate);
                countryDictionary.Add(year,country);
            }

        }
    }

    void Update()
    {
        float xFPC = fpc.transform.position.x;
        float zFPC = fpc.transform.position.z;

        int year = 0;
        float agentRate;

        float xShift = 15f;
        float graphWidth = 70f;

        // If it's on a corner

        if (xFPC <= 15f && zFPC >= -15f) // Corner 1
        {
            year = 1980;
        } else if (xFPC >= 85f && zFPC >= -15f) // Corner 2
        {
            year = 1990;
        } else if (xFPC >= 85f && zFPC <= -85f) // Corner 3
        {
            year = 2000;
        } else if (xFPC <= 15f && zFPC <= -85f) // Corner 4
        {
            year = 2010;
        } else if (zFPC >= -15f) // Wall 1
        {
            float xCoord = xFPC;
            float xLocalRange = 10f;
            float xLocalOffset = -1980f;
            float floatYear = ((xCoord - xShift) * xLocalRange) / graphWidth - xLocalOffset;
            year = (int)floatYear;
        } else if (xFPC >= 85f)
        {
            float xCoord = -zFPC;
            float xLocalRange = 10f;
            float xLocalOffset = -1990f;
            float floatYear = ((xCoord - xShift) * xLocalRange) / graphWidth - xLocalOffset;
            year = (int)floatYear;
        } else if (zFPC <= -85f) // Wall 3
        {
            float xCoord = 100 - xFPC;
            float xLocalRange = 10f;
            float xLocalOffset = -2000f;
            float floatYear = ((xCoord - xShift) * xLocalRange) / graphWidth - xLocalOffset;
            year = (int)floatYear;
        } else if (xFPC <= 15f) // Wall 4
        {
            float xCoord = 100 + zFPC;
            float xLocalRange = 10f;
            float xLocalOffset = -2010f;
            float floatYear = ((xCoord - xShift) * xLocalRange) / graphWidth - xLocalOffset;
            year = (int)floatYear;
        }

        // Debug.Log(year);
        if (year != 0)
        {
            if (year > 2018)
            { 
                year = 2018;
            }

            agentRate = rateDictionary[year];

            // TODO: grab this directly from class instance
            float graphHeight = 90f;
            float yOffset = -220f;
            float MaxRate = 755f;
            float yRange = 535f;

            if (agentRate > MaxRate)
            {
                agentRate = MaxRate;
            }

            // Debug.Log(agentRate);

            float yCoord = ((agentRate + yOffset) / yRange) * graphHeight;
            Vector3 rbCastPos = new Vector3(50, yCoord, -50);
            // set rb.transform.y position
            RBCast.transform.SetPositionAndRotation(rbCastPos, Quaternion.Euler(0,0,180));
            rbPreviousPos = rbCastPos;

            // Declare a raycast hit to store information about what our raycast has hit
            RaycastHit[] hits = rb.SweepTestAll(-RBCast.transform.up, 100f);
            if (hits.Length != 0)
            {
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.tag != "box")
                    {
                        Debug.Log("a hit!");
                        // directedAgent.MoveToLocation(hit.point);
                        GameObject hitPoint = Instantiate(DataBallPrefab, hit.point + Vector3.up * 3, Quaternion.identity);
                        hitPoint.tag = "box";

                        break;
                    }
                }
                
            }
        } else {
            // Make sure the rigidbody doesn't fall!
            RBCast.transform.SetPositionAndRotation(rbPreviousPos, Quaternion.Euler(0,0,180));
        }
    }

    int LookUpWall(int year)
    {
        int wall = ((year-1980)-1)/10;
        return wall;
    }
}



    