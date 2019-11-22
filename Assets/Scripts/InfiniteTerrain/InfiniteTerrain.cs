using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InfiniteTerrain : InfiniteLandscape
{
    // Holds the name of the terrain tile and the randomly generated number, that determines the combination of land mass types 
	// used to calculate the height if the terrain at each coordinate 
	public static Dictionary<string, int> LandmassDict = new Dictionary<string, int>();

    // 2-dimensional table for holding values that form a round sine bell shape 
	public static float[,] fallOffTable;

	static int[] m_storedRandoms = new int[100];
	static int storedRandomsCounter = 0;
	private IPatch patchToBeFilled = null;
    bool terrainIsFlushed = true;

    public const int m_heightMapSize = 513;
    public const float m_terrainHeight = 1500;
    public static float[,] m_terrainHeights = new float[m_heightMapSize, m_heightMapSize];

    public static Terrain[,] m_terrainGrid = new Terrain[dim, dim];

    //Trees
    public static int numOfTreePrototypes = 6;

    public GameObject m_tree0, m_tree1, m_tree2, m_tree3, m_tree4,m_tree5;


    public const int numOfDetailPrototypes = 4;
    private DetailPrototype[] m_detailProtoTypes = new DetailPrototype[numOfDetailPrototypes];
    public Texture2D[] detailTexture = new Texture2D[numOfDetailPrototypes];
    
    public GameObject[] detailMesh = new GameObject[2];

    private DetailRenderMode detailMode;
    

    public static int numOfTreesPerTerrain = 9000;
    private GameObject[] trees = new GameObject[numOfTreePrototypes];
    TreePrototype[] m_treeProtoTypes = new TreePrototype[numOfTreePrototypes];
    public float m_treeDistance = 2000.0f;          //The distance at which trees will no longer be drawn
    public float m_treeBillboardDistance = 400.0f;  //The distance at which trees meshes will turn into tree billboards
    public float m_treeCrossFadeLength = 50.0f;     //As trees turn to billboards there transform is rotated to match the meshes, a higher number will make this transition smoother
    public int m_treeMaximumFullLODCount = 400;     //The maximum number of trees that will be drawn in a certain area. 

    //Splat
    public const int numOfSplatPrototypes = 4;
    //Original
    public const int m_alphaMapSize = (InfiniteTerrain.m_heightMapSize - 1) / 2;  
    //public const int m_alphaMapSize = (InfiniteTerrain.m_heightMapSize - 1) *2; // gives more details but is slower
    public static float[, ,] m_alphaMap = new float[m_alphaMapSize, m_alphaMapSize, numOfSplatPrototypes];
    public Texture2D[] splat = new Texture2D[numOfSplatPrototypes];

    private SplatPrototype[] m_splatPrototypes = new SplatPrototype[numOfSplatPrototypes];
    //Details

    public const int m_detailMapSize = m_alphaMapSize;                 //Resolutions of detail (Grass) layers SHOULD BE EQUAL TO SPLAT RES
    public static int[,] detailMap0 = new int[m_detailMapSize, m_detailMapSize];
    public static int[,] detailMap1 = new int[m_detailMapSize, m_detailMapSize];
    public static int[,] detailMap2 = new int[m_detailMapSize, m_detailMapSize];
    public static int[,] detailMap3 = new int[m_detailMapSize, m_detailMapSize];
    
    public int m_detailObjectDistance = 500;                                //The distance at which details will no longer be drawn
    public float m_detailObjectDensity = 40.0f;                             //Creates more dense details within patch
    public int m_detailResolutionPerPatch = 32;                             //The size of detail patch. A higher number may reduce draw calls as details will be batch in larger patches
    public float m_wavingGrassStrength = 0.4f;
    public float m_wavingGrassAmount =  0.2f;
    public float m_wavingGrassSpeed = 0.4f;
    public Color m_wavingGrassTint = Color.white;
    public Color m_grassHealthyColor = Color.white;
    public Color m_grassDryColor = Color.white;


    public float Tile0 = 300;
    public float Tile1 = 5;
    public float Tile2 = 300;
    public float Tile3 = 5;

    void Awake()
    {
		Texture2D falloff = new Texture2D(513,513);
       	fallOffTable = GenerateFalloffTable();

		Random.InitState(InfiniteLandscape.RandomSeed);
		for (int i = 0; i < m_storedRandoms.Length; i++)
		{
			//m_storedRandoms[i] = Random.Range(0, 7); // lots of plains

			m_storedRandoms[i] = Random.Range(1, 9); // less plains, more water

		}

		// splat[0] = Resources.Load("Textures/" + "rock_2048") as Texture2D;
		// splat[1] = Resources.Load("Textures/" + "forst_1024") as Texture2D;
		// splat[2] = Resources.Load("Textures/" + "snow_512") as Texture2D;
		// splat[3] = Resources.Load("Textures/" + "GoodDirt") as Texture2D;

		//Debug.Log("m_alphaMapSize " + m_alphaMapSize);

// to set certain areas non-random, this can be done:
        // use combination of 1,2,4,8
        // there is no limit of different landmass types, other than performance, 4 is just easy to test amount that gives good variety already
        
        /*
        LandmassDict.Add("9002_9002", 1);
        LandmassDict.Add("9003_9002", 1);
        LandmassDict.Add("9004_9002", 3);
        LandmassDict.Add("9005_9002", 3);
        LandmassDict.Add("9006_9002", 3);

        LandmassDict.Add("9002_9003", 1);
        LandmassDict.Add("9003_9003", 1);
        LandmassDict.Add("9004_9003", 2);
        LandmassDict.Add("9005_9003", 3);
        LandmassDict.Add("9006_9003", 3);

        LandmassDict.Add("9002_9004", 4);
        LandmassDict.Add("9003_9004", 4);
        LandmassDict.Add("9004_9004", 5); // center
        LandmassDict.Add("9005_9004", 6);
        LandmassDict.Add("9006_9004", 6);

        LandmassDict.Add("9002_9005", 7);
        LandmassDict.Add("9003_9005", 7);
        LandmassDict.Add("9004_9005", 8);
        LandmassDict.Add("9005_9005", 9);
        LandmassDict.Add("9006_9005", 9);

        LandmassDict.Add("9002_9006", 7);
        LandmassDict.Add("9003_9006", 7);
        LandmassDict.Add("9004_9006", 8);
        LandmassDict.Add("9005_9006", 9);
        LandmassDict.Add("9006_9006", 9);
        */

        /*
        LandmassDict.Add("9001_9001", 1);
        LandmassDict.Add("9002_9001", 1);
        LandmassDict.Add("9003_9001", 1);
        LandmassDict.Add("9004_9001", 1);
        LandmassDict.Add("9005_9001", 1);

        LandmassDict.Add("9001_9002", 1);
        LandmassDict.Add("9002_9002", 1);
        LandmassDict.Add("9003_9002", 1);
        LandmassDict.Add("9004_9002", 1);
        LandmassDict.Add("9005_9002", 1);

        LandmassDict.Add("9001_9003", 1);
        LandmassDict.Add("9002_9003", 1);
        LandmassDict.Add("9003_9003", 1);
        LandmassDict.Add("9004_9003", 1);
        LandmassDict.Add("9005_9003", 1);

        LandmassDict.Add("9001_9004", 1);
        LandmassDict.Add("9002_9004", 1);
        LandmassDict.Add("9003_9004", 1);
        LandmassDict.Add("9004_9004", 3);
        LandmassDict.Add("9005_9004", 1);

        LandmassDict.Add("9001_9005", 1);
        LandmassDict.Add("9002_9005", 1);
        LandmassDict.Add("9003_9005", 1);
        LandmassDict.Add("9004_9005", 1);
        LandmassDict.Add("9005_9005", 1);
        */

        // changed tree types:
        m_treeProtoTypes = new TreePrototype[numOfTreePrototypes];

        m_treeProtoTypes[0] = new TreePrototype();
        m_treeProtoTypes[0].prefab = m_tree0;

        m_treeProtoTypes[1] = new TreePrototype();
        m_treeProtoTypes[1].prefab = m_tree1;

        m_treeProtoTypes[2] = new TreePrototype();
        m_treeProtoTypes[2].prefab = m_tree2;

        m_treeProtoTypes[3] = new TreePrototype();
        m_treeProtoTypes[3].prefab = m_tree3;

        m_treeProtoTypes[4] = new TreePrototype();
        m_treeProtoTypes[4].prefab = m_tree4;

		m_treeProtoTypes[5] = new TreePrototype();
		m_treeProtoTypes[5].prefab = m_tree5;


		trees[0] = m_treeProtoTypes[0].prefab as GameObject;
        trees[1] = m_treeProtoTypes[1].prefab as GameObject;
        trees[2] = m_treeProtoTypes[2].prefab as GameObject;
        trees[3] = m_treeProtoTypes[3].prefab as GameObject;
		trees[4] = m_treeProtoTypes[4].prefab as GameObject;
		trees[5] = m_treeProtoTypes[5].prefab as GameObject;


		Vector2[] splatTileSize = new Vector2[4] { new Vector2(Tile0, Tile0), new Vector2(Tile1, Tile1), new Vector2(Tile2, Tile2), new Vector2(Tile3, Tile3) };
        for (int i = 0; i < numOfSplatPrototypes; i++)
            m_splatPrototypes[i] = new SplatPrototype();

        for (int i = 0; i < numOfSplatPrototypes; i++)
        {
            m_splatPrototypes[i].texture = splat[i];
            m_splatPrototypes[i].tileOffset = Vector2.zero;
            m_splatPrototypes[i].tileSize = splatTileSize[i];
            m_splatPrototypes[i].texture.Apply(true);
        }

        for (int i = 0; i < numOfDetailPrototypes; i++)
        {

            /*
            if(i==0 || i== 1) // overriding first grass with detail mesh
            {
                m_detailProtoTypes[i] = new DetailPrototype();
                m_detailProtoTypes[i].usePrototypeMesh = true;

                m_detailProtoTypes[i].prototype = detailMesh[i];
                m_detailProtoTypes[i].renderMode = DetailRenderMode.VertexLit;

                m_detailProtoTypes[i].minHeight = 1;
                m_detailProtoTypes[i].minWidth = 1;


                m_detailProtoTypes[i].maxHeight = 2;
                m_detailProtoTypes[i].maxWidth = 2;

                //m_detailProtoTypes[i].noiseSpread = ???

                m_detailProtoTypes[i].healthyColor = m_grassHealthyColor;
                m_detailProtoTypes[i].dryColor = m_grassDryColor;
                
            }
            else
            */
            {
                
                m_detailProtoTypes[i] = new DetailPrototype();
                m_detailProtoTypes[i].prototypeTexture = detailTexture[i];
                m_detailProtoTypes[i].renderMode = detailMode;
                m_detailProtoTypes[i].healthyColor = m_grassHealthyColor;
                m_detailProtoTypes[i].dryColor = m_grassDryColor;
                m_detailProtoTypes[i].maxHeight = 1;//0.5f;
                m_detailProtoTypes[i].maxWidth = 1;//0.2f;
                m_detailProtoTypes[i].noiseSpread = 0.5f;
            }
        }
        for (int i = 0; i < numOfTreePrototypes; i++)
        {
            m_treeProtoTypes[i] = new TreePrototype();
            m_treeProtoTypes[i].prefab = trees[i];
        }
        for (int i = 0; i < dim; i++)
        {
            for (int j = 0; j < dim; j++)
            {
                TerrainData terrainData = new TerrainData();

                terrainData.wavingGrassStrength = m_wavingGrassStrength;
                terrainData.wavingGrassAmount = m_wavingGrassAmount;
                terrainData.wavingGrassSpeed = m_wavingGrassSpeed;
                terrainData.wavingGrassTint = m_wavingGrassTint;
                terrainData.heightmapResolution = m_heightMapSize;
                terrainData.size = new Vector3(m_landScapeSize, m_terrainHeight, m_landScapeSize);
                terrainData.alphamapResolution = m_alphaMapSize;
                
                // TODO update to new api:
                terrainData.splatPrototypes = m_splatPrototypes;

                terrainData.treePrototypes = m_treeProtoTypes;
                terrainData.SetDetailResolution(m_detailMapSize, m_detailResolutionPerPatch);
                terrainData.detailPrototypes = m_detailProtoTypes;

                m_terrainGrid[i, j] = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
            }
        }

        for (int i = 0; i < dim; i++)
        {
            for (int j = 0; j < dim; j++)
            {
                m_terrainGrid[i, j].gameObject.AddComponent<TerrainScript>();
                m_terrainGrid[i, j].transform.parent = gameObject.transform;

                m_terrainGrid[i, j].transform.position = new Vector3(
                m_terrainGrid[1, 1].transform.position.x + (i - 1) * m_landScapeSize, m_terrainGrid[1, 1].transform.position.y,
                m_terrainGrid[1, 1].transform.position.z + (j - 1) * m_landScapeSize);

                m_terrainGrid[i, j].treeDistance = m_treeDistance;
                m_terrainGrid[i, j].treeBillboardDistance = m_treeBillboardDistance;
                m_terrainGrid[i, j].treeCrossFadeLength = m_treeCrossFadeLength;
                m_terrainGrid[i, j].treeMaximumFullLODCount = m_treeMaximumFullLODCount;

                m_terrainGrid[i, j].detailObjectDensity = m_detailObjectDensity;
                m_terrainGrid[i, j].detailObjectDistance = m_detailObjectDistance;

                m_terrainGrid[i, j].GetComponent<Collider>().enabled = false;
                m_terrainGrid[i, j].basemapDistance = 4000;
                m_terrainGrid[i, j].castShadows = false;

                // m_terrainGrid[i, j].terrainData.wavingGrassAmount = 1000;

                string xName = (curGlobalIndexX + i - 1).ToString(); // name will be used for identifying the correct entry in landmamassDictionary
                string zName = (curGlobalIndexZ + j - 1).ToString();
                m_terrainGrid[i, j].name = xName + "_new_" + zName;

                PatchManager.AddTerrainInfo(curGlobalIndexX + i - 1, curGlobalIndexZ + j - 1, m_terrainGrid[i, j], m_terrainGrid[i, j].transform.position);
            }
        }
        PatchManager.MakePatches();

        int patchCount = PatchManager.patchQueue.Count;
        for(int i = 0; i < patchCount; i++)
            PatchManager.patchQueue.Dequeue().ExecutePatch();

        UpdateIndexes();
        UpdateTerrainNeighbors();

        StartCoroutine(FlushTerrain());
        terrainIsFlushed = true;

        m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].GetComponent<Collider>().enabled = false;
        m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].GetComponent<Collider>().enabled = true;
    }

    void UpdateTerrainNeighbors()
    {
        int iC = curCyclicIndexX;           int jC = curCyclicIndexZ;
        int iP = PreviousCyclicIndex(iC);   int jP = PreviousCyclicIndex(jC);
        int iN = NextCyclicIndex(iC);       int jN = NextCyclicIndex(jC);

        m_terrainGrid[iP, jP].SetNeighbors(null, m_terrainGrid[iP, jC], m_terrainGrid[iC, jP], null);
        m_terrainGrid[iC, jP].SetNeighbors(m_terrainGrid[iP, jP], m_terrainGrid[iC, jC], m_terrainGrid[iN, jP], null);
        m_terrainGrid[iN, jP].SetNeighbors(m_terrainGrid[iC, jP], m_terrainGrid[iN, jC], null, null);
        m_terrainGrid[iP, jC].SetNeighbors(null, m_terrainGrid[iP, jN], m_terrainGrid[iC, jC], m_terrainGrid[iP, jP]);
        m_terrainGrid[iC, jC].SetNeighbors(m_terrainGrid[iP, jC], m_terrainGrid[iC, jN], m_terrainGrid[iN, jC], m_terrainGrid[iC, jP]);
        m_terrainGrid[iN, jC].SetNeighbors(m_terrainGrid[iC, jC], m_terrainGrid[iN, jN], null, m_terrainGrid[iN, jP]);
        m_terrainGrid[iP, jN].SetNeighbors(null, null, m_terrainGrid[iC, jN], m_terrainGrid[iP, jC]);
        m_terrainGrid[iC, jN].SetNeighbors(m_terrainGrid[iP, jN], null, m_terrainGrid[iN, jN], m_terrainGrid[iC, jC]);
        m_terrainGrid[iN, jN].SetNeighbors(m_terrainGrid[iC, jN], null, null, m_terrainGrid[iN, jC]);
    }

    private int NextCyclicIndex(int i)
    {
        if (i < 0 || i > dim - 1)
            Debug.LogError("index outside dim");
        return (i + 1) % dim;
    }

    private int PreviousCyclicIndex(int i)
    {
        if (i < 0 || i > dim - 1)
            Debug.LogError("index outside dim");
        return i == 0 ? dim - 1 : (i-1) % dim;
    }

    private void UpdateTerrainPositions()
	{
		Debug.Log("UpdateTerrainPositions, prevX /curX " + prevGlobalIndexX + " / " + curGlobalIndexX + "prevZ / curZ " + prevGlobalIndexZ + " / " + curGlobalIndexZ);
		if (curGlobalIndexZ != prevGlobalIndexZ && curGlobalIndexX != prevGlobalIndexX)
        {
            int z; int z0;
            if (curGlobalIndexZ > prevGlobalIndexZ)
            {
                z0 = curGlobalIndexZ + 1;
                z = PreviousCyclicIndex(prevCyclicIndexZ);
            }
            else
            {
                z0 = curGlobalIndexZ - 1;
                z = NextCyclicIndex(prevCyclicIndexZ);
            }

            int[] listX = { PreviousCyclicIndex(prevCyclicIndexX), prevCyclicIndexX, NextCyclicIndex(prevCyclicIndexX) };
            for (int i = 1; i < dim; i++)
            {
                Vector3 newPos = new Vector3(
                m_terrainGrid[prevCyclicIndexX, curCyclicIndexZ].transform.position.x + (i - 1) * m_landScapeSize,
                m_terrainGrid[prevCyclicIndexX, curCyclicIndexZ].transform.position.y,
                m_terrainGrid[prevCyclicIndexX, curCyclicIndexZ].transform.position.z + (curGlobalIndexZ - prevGlobalIndexZ) * m_landScapeSize);

                PatchManager.AddTerrainInfo(prevGlobalIndexX + i - 1, z0, m_terrainGrid[listX[i], z], newPos);
            }
            int x; int x0;
            if (curGlobalIndexX > prevGlobalIndexX)
            {
                x0 = curGlobalIndexX + 1;
                x = PreviousCyclicIndex(prevCyclicIndexX);
            }
            else
            {
                x0 = curGlobalIndexX - 1;
                x = NextCyclicIndex(prevCyclicIndexX);
            }

            int[] listZ = { PreviousCyclicIndex(curCyclicIndexZ), curCyclicIndexZ, NextCyclicIndex(curCyclicIndexZ) };
            for (int i = 0; i < dim; i++)
            {
                Vector3 newPos = new Vector3(
                m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.x + (curGlobalIndexX - prevGlobalIndexX) * m_landScapeSize,
                m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.y,
                m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.z + (i - 1) * m_landScapeSize);

                PatchManager.AddTerrainInfo(x0, curGlobalIndexZ + i - 1, m_terrainGrid[x, listZ[i]], newPos);
            }
        }
        else if (curGlobalIndexZ != prevGlobalIndexZ)
        {
            int z; int z0;
            if (curGlobalIndexZ > prevGlobalIndexZ)
            {
                z0 = curGlobalIndexZ + 1;
                z = PreviousCyclicIndex(prevCyclicIndexZ);
            }
            else
            {
                z0 = curGlobalIndexZ - 1;
                z = NextCyclicIndex(prevCyclicIndexZ);
            }
                int[] listX = { PreviousCyclicIndex(prevCyclicIndexX), prevCyclicIndexX, NextCyclicIndex(prevCyclicIndexX) };
                for (int i = 0; i < dim; i++)
                {
                    Vector3 newPos = new Vector3(
                    m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.x + (i - 1) * m_landScapeSize,
                    m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.y,
                    m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.z + (curGlobalIndexZ - prevGlobalIndexZ) * m_landScapeSize);

                    PatchManager.AddTerrainInfo(curGlobalIndexX + i - 1, z0, m_terrainGrid[listX[i], z], newPos);
                }
        }
        else if (curGlobalIndexX != prevGlobalIndexX)
        {
            int x; int x0;
            if (curGlobalIndexX > prevGlobalIndexX)
            {
                x0 = curGlobalIndexX + 1;
                x = PreviousCyclicIndex(prevCyclicIndexX);
            }
            else
            {
                x0 = curGlobalIndexX - 1;
                x = NextCyclicIndex(prevCyclicIndexX);
            }

            int[] listZ = { PreviousCyclicIndex(prevCyclicIndexZ), prevCyclicIndexZ, NextCyclicIndex(prevCyclicIndexZ) };
            for (int i = 0; i < dim; i++)
            {
                Vector3 newPos = new Vector3(
                m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.x + (curGlobalIndexX - prevGlobalIndexX) * m_landScapeSize,
                m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.y,
                m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.z + (i - 1) * m_landScapeSize);

				PatchManager.AddTerrainInfo(x0, curGlobalIndexZ + i - 1, m_terrainGrid[x, listZ[i]], newPos);
            }
        }
        PatchManager.MakePatches();
	}

    IEnumerator CountdownForPatch()
    {
        patchIsFilling = true;
        yield return new WaitForEndOfFrame();
        patchIsFilling = false;
    }

    IEnumerator FlushTerrain()
    {
        for (int i = 0; i < dim; i++)
            for (int j = 0; j < dim; j++)
            {
               //Debug.Log("Enabling terrain collider");
               m_terrainGrid[i, j].transform.GetComponent<TerrainCollider>().enabled = true;
                m_terrainGrid[i, j].Flush();
                yield return new WaitForEndOfFrame();
            }
    }

    float StartTime;
    float oneUpdateTime;
    bool updateRound;
    int updatecounter = 0;
    float biggestUpdateTime = 0f;

    protected override void Update()
    {
        base.Update();

        //-------Just debugging--------
        if(updateRound)
        {
            // previous was updateRound
            float executionTime = Time.time - StartTime;
            //Debug.Log(updatecounter + ": ----------------------------------------> Execution time " + executionTime);
            updateRound = false;
        }
        updateRound = updateLandscape;
        StartTime = Time.deltaTime;
        //---------------


        if (updateLandscape == true)
        {
            
            m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].GetComponent<Collider>().enabled = true;        //Slow operation
            m_terrainGrid[prevCyclicIndexX, prevCyclicIndexZ].GetComponent<Collider>().enabled = false;

            UpdateTerrainNeighbors();
            UpdateTerrainPositions();
        }

        if (PatchManager.patchQueue.Count != 0)
        {
            terrainIsFlushed = false;
            if (patchIsFilling == false)
            {
                patchToBeFilled = PatchManager.patchQueue.Dequeue();
                StartCoroutine(CountdownForPatch());
            }
            if (patchToBeFilled != null)
            {

                float execTime = Time.time - oneUpdateTime;

/* 
                if(patchToBeFilled is TerrainPatch)
                    Debug.Log( execTime + "     Executed TerrainPatch...");
                if (patchToBeFilled is SplatDetailPatch)
                    Debug.Log(execTime + "     Executed SplatDetailPatch...");
                if (patchToBeFilled is TreePatch)
                    Debug.Log(execTime + "     Execute TreePatch...");
*/
                if(execTime > 0.1f)
                {
                    //Debug.LogError(Time.time +  "------------------> exec time " + execTime); // error to get red color in console, not actual error
                    biggestUpdateTime = execTime;
                }

				//for debugging
                oneUpdateTime = Time.time;
                
				patchToBeFilled.ExecutePatch();
                patchToBeFilled = null;
            }
        }
        else if (PatchManager.patchQueue.Count == 0 && terrainIsFlushed == false)
        {
            StartCoroutine(FlushTerrain());
            terrainIsFlushed = true;
        }
    }

//Generates sine "bell shape" falloff table, used for blending  terrain height values for each landmass type
public static float[,] GenerateFalloffTable()
     {
        int size = 513; // assuming height is the same
        int halfSize = 257; // Actually half the size + "middle pixel" also
        float n256 = size *0.5f;
        float[,]  table = new float[513,513]; 
        Vector2 center = new Vector2(size / 2f, size / 2f);
        List<int> errorValues = new List<int>();

		// we only iterate one quarter of the area, and "mirror" the results to rest of the area:
		// its faster, and we can be absolutely sure that when fallofftable is used, left and right (and top and bottom) 
		// values are exactly same and there will be no visible seams  
        for (int y = 0; y < halfSize; y++)
        {
            for (int x = 0; x < halfSize; x++)
            {
                float DistanceFromCenter = Vector2.Distance(center, new Vector2(x, y));
                float currentAlpha = 0; // value will act as "alpha mask" when blending terrain heights

                if (DistanceFromCenter > 513 / 2f)
                {
                    currentAlpha = 0f;
                }
                else
                {
                    float normalized = 2 * ((DistanceFromCenter / n256) * (3.1415926f * 0.500f));
                    currentAlpha = (Mathf.Sin(1.5f - normalized) / 2f) + 0.5f;
                }

                if (x <= 257 && y <= 257  && x >=10 && y >= 10)
                {

                    table[x, y] = currentAlpha;
                    table[size - x, y] = currentAlpha;
                    table[x, size - y] = currentAlpha;
                    table[size - x, size - y] = currentAlpha;
                }
                else
                {
                   //debugging:
                   //errorValues.Add(x);
                }
            }
        }

        foreach(int error in errorValues)
            Debug.Log("x error: " + error);
        return table;    
    }


    // If value for landmasses has already been decided, it is returned. If not, random value is assigned to the new dictionary entry. 
	public static int GetOrAssignLandMassTypes(string key)
    {
        if (LandmassDict.ContainsKey(key))
        {
            return LandmassDict[key];
        }
        else
        {
			//int value = Random.Range(1, 17); // randomly combine all types with even weights - gives more square and blocky coastline
			//int value =  Random.Range(1, 9) + 8; // always have plains - gives more natural coastline

			// Using stored random values to assure no unplanned changes
			
			int value = m_storedRandoms[storedRandomsCounter] + 8; 
			
			storedRandomsCounter++;
			if(storedRandomsCounter >= m_storedRandoms.Length)
				storedRandomsCounter = 0;

			Debug.Log("adding key >>> " + key + " value: " + value);
            LandmassDict.Add(key, value);
            return value;
        }
    }
}

