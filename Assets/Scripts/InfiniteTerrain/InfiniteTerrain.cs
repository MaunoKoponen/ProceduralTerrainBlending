using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;


public class AreaData
{
	public string areaName;
	public int landMassValue;
	public int mapX;
	public int mapZ;
	public float xCoord;
	public float yCoord;
	public CastleData castleData;
}

public class CastleData
{
	public int size;
	public GameObject rootGameObject;

	public int mapX;
	public int mapZ;

	public float coordX;

	public float coordZ;

}

public class InfiniteTerrain : InfiniteLandscape
{
	
	// Holds the name of the terrain tile and the randomly generated number, that determines the combination of land mass types 
	// used to calculate the height of the terrain at each coordinate 

	// for using something else than Unity built-in materia for terrain
	public bool useTestMaterial;
	public static bool RenderTreesStatic;
	public bool RenderTrees;
	public static bool RenderDetailsStatic;
	public bool RenderDetails;
	public Material testMaterial;
	public bool useDrawInstancing;

	//public static Dictionary<string, int> LandmassDict = new Dictionary<string, int>();


	public static Dictionary<string, AreaData> AreaDict = new Dictionary<string, AreaData>();


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
	public const int numOfSplatPrototypes = 5;
	//Original
	public const int m_alphaMapSize = (InfiniteTerrain.m_heightMapSize - 1) / 2;  
    //public const int m_alphaMapSize = (InfiniteTerrain.m_heightMapSize - 1) *2; // gives more details but is slower
    public static float[, ,] m_alphaMap = new float[m_alphaMapSize, m_alphaMapSize, numOfSplatPrototypes];
    public Texture2D[] splat = new Texture2D[numOfSplatPrototypes];

	public Texture2D[] splatNormals = new Texture2D[numOfSplatPrototypes];

	public Color[] splatSpecular = new Color[numOfSplatPrototypes];

	private SplatPrototype[] m_splatPrototypes = new SplatPrototype[numOfSplatPrototypes];
    
	//Details
    public const int m_detailMapSize = m_alphaMapSize;                 //Resolutions of detail (Grass) layers SHOULD BE EQUAL TO SPLAT RES
    public static int[,] detailMap0 = new int[m_detailMapSize, m_detailMapSize];
    public static int[,] detailMap1 = new int[m_detailMapSize, m_detailMapSize];
    public static int[,] detailMap2 = new int[m_detailMapSize, m_detailMapSize];
    public static int[,] detailMap3 = new int[m_detailMapSize, m_detailMapSize];

	//These are private, because changing these without too much thought will introduce artifacts in density/placement
	private int m_detailObjectDistance = 1000;//500;                                //The distance at which details will no longer be drawn
	private float m_detailObjectDensity = 0.5f;                             //Creates more dense details within patch
    private int m_detailResolutionPerPatch = 16;                             //The size of detail patch. A higher number may reduce draw calls as details will be batch in larger patches
    
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

	public float Tile4 = 20;



	//UI
	public Text terrainName;
	public Text hillsValue;
	public Text mountainsValue;
	public Text ridgedMountainsValue;
	public Text plainsValue;

	// Test: a castle/town that will obey terrain height
	public CastleCreator castleCreator;
	public bool createCastle;
	private List<CastleData> CastleBuildList = new List<CastleData>();


	// this is used to create "fjord"- landmass type. Its like small walleys and flat topped and hills 
	public AnimationCurve TestCurve;
	public static AnimationCurve StaticTestCurve;

	// not in use
	public AnimationCurve TestCurve2;
	public static AnimationCurve StaticTestCurve2;

	void Awake()
    {
		RenderDetailsStatic = RenderDetails;
		RenderTreesStatic = RenderTrees;
		StaticTestCurve = TestCurve;
		StaticTestCurve2 = TestCurve2;

		Texture2D falloff = new Texture2D(513,513);
       	fallOffTable = GenerateFalloffTable();

		UnityEngine.Random.InitState(InfiniteLandscape.RandomSeed);
		for (int i = 0; i < m_storedRandoms.Length; i++)
		{
			//m_storedRandoms[i] = Random.Range(0, 7); // lots of plains

			m_storedRandoms[i] = UnityEngine.Random.Range(1, 9); // less plains, more water

		}

		// to set certain areas non-random, this can be done:
        // use combination of 1,2,4,8
        // there is no limit of different landmass types, other than performance, 4 is just easy to test amount that gives good variety already
        // here starting coordinate is 9003/9003

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

		Vector2[] splatTileSize = new Vector2[5] { new Vector2(Tile0, Tile0), new Vector2(Tile1, Tile1), new Vector2(Tile2, Tile2), new Vector2(Tile3, Tile3), new Vector2(Tile4, Tile4) };
        for (int i = 0; i < numOfSplatPrototypes; i++)
            m_splatPrototypes[i] = new SplatPrototype();


        for (int i = 0; i < numOfSplatPrototypes; i++)
        {
            m_splatPrototypes[i].texture = splat[i];
			m_splatPrototypes[i].normalMap = splatNormals[i];
			m_splatPrototypes[i].specular = splatSpecular[i];
			m_splatPrototypes[i].tileOffset = Vector2.zero;
            m_splatPrototypes[i].tileSize = splatTileSize[i];
            m_splatPrototypes[i].texture.Apply(true);
        }

        for (int i = 0; i < numOfDetailPrototypes; i++)
        {
            if(i == 2) // overriding third detail with mesh
            {
                m_detailProtoTypes[i] = new DetailPrototype();
                m_detailProtoTypes[i].usePrototypeMesh = true;

                m_detailProtoTypes[i].prototype = detailMesh[0];
                m_detailProtoTypes[i].renderMode = DetailRenderMode.VertexLit;

                m_detailProtoTypes[i].minHeight = 0.5f;
                m_detailProtoTypes[i].minWidth = 0.5f;

                m_detailProtoTypes[i].maxHeight = 1;
                m_detailProtoTypes[i].maxWidth = 1;

                //m_detailProtoTypes[i].noiseSpread = ???

                m_detailProtoTypes[i].healthyColor = Color.white;
                m_detailProtoTypes[i].dryColor = Color.white;
                
            }
            else
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

				if (useTestMaterial)
				{
					m_terrainGrid[i, j].materialType = Terrain.MaterialType.Custom;
					m_terrainGrid[i, j].materialTemplate = testMaterial;
				}

				if(useDrawInstancing)
					m_terrainGrid[i, j].drawInstanced = false;

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


		
		//if(createCastle)
		//	castleCreator.CreateCastle(1500, 1500, );
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
			int z; int z0; int deletionZ;
			if (curGlobalIndexZ > prevGlobalIndexZ)
			{
				z0 = curGlobalIndexZ + 1;
				z = PreviousCyclicIndex(prevCyclicIndexZ);
				deletionZ = prevGlobalIndexZ - 1;
			}
			else
			{
				z0 = curGlobalIndexZ - 1;
				z = NextCyclicIndex(prevCyclicIndexZ);
				deletionZ = prevGlobalIndexZ + 1;
			}

			int[] listX = { PreviousCyclicIndex(prevCyclicIndexX), prevCyclicIndexX, NextCyclicIndex(prevCyclicIndexX) };
			for (int i = 1; i < dim; i++)
			{
				Vector3 newPos = new Vector3(
				m_terrainGrid[prevCyclicIndexX, curCyclicIndexZ].transform.position.x + (i - 1) * m_landScapeSize,
				m_terrainGrid[prevCyclicIndexX, curCyclicIndexZ].transform.position.y,
				m_terrainGrid[prevCyclicIndexX, curCyclicIndexZ].transform.position.z + (curGlobalIndexZ - prevGlobalIndexZ) * m_landScapeSize);

				PatchManager.AddTerrainInfo(prevGlobalIndexX + i - 1, z0, m_terrainGrid[listX[i], z], newPos);
				int mapX = prevGlobalIndexX + i - 1;
				int mapZ = z0;
				StoreCordinatesForCastle(mapX, mapZ, newPos);
				DeleteCastle( curGlobalIndexX + i - 1,deletionZ);
			}
			int x; int x0; int deletionX;
			if (curGlobalIndexX > prevGlobalIndexX)
			{
				x0 = curGlobalIndexX + 1;
				x = PreviousCyclicIndex(prevCyclicIndexX);
				deletionX = prevGlobalIndexX - 1;
			}
			else
			{
				x0 = curGlobalIndexX - 1;
				x = NextCyclicIndex(prevCyclicIndexX);
				deletionX = prevGlobalIndexX + 1;
			}

			int[] listZ = { PreviousCyclicIndex(curCyclicIndexZ), curCyclicIndexZ, NextCyclicIndex(curCyclicIndexZ) };
			for (int i = 0; i < dim; i++)
			{
				Vector3 newPos = new Vector3(
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.x + (curGlobalIndexX - prevGlobalIndexX) * m_landScapeSize,
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.y,
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.z + (i - 1) * m_landScapeSize);

				PatchManager.AddTerrainInfo(x0, curGlobalIndexZ + i - 1, m_terrainGrid[x, listZ[i]], newPos);
				int mapX = x0;
				int mapZ = curGlobalIndexZ + i - 1;
				StoreCordinatesForCastle(mapX, mapZ, newPos);
				DeleteCastle(deletionX, curGlobalIndexZ + i - 1);
			}
		}
		else if (curGlobalIndexZ != prevGlobalIndexZ)
		{
			Debug.Log("BBBBB");
			
			int z; int z0; int deletionZ;
			if (curGlobalIndexZ > prevGlobalIndexZ)
			{
				z0 = curGlobalIndexZ + 1;
				z = PreviousCyclicIndex(prevCyclicIndexZ);
				deletionZ = prevGlobalIndexZ - 1;
			}
			else
			{
				z0 = curGlobalIndexZ - 1;
				z = NextCyclicIndex(prevCyclicIndexZ);
				deletionZ = prevGlobalIndexZ + 1;
			}
			int[] listX = { PreviousCyclicIndex(prevCyclicIndexX), prevCyclicIndexX, NextCyclicIndex(prevCyclicIndexX) };
			for (int i = 0; i < dim; i++)
			{
				Vector3 newPos = new Vector3(
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.x + (i - 1) * m_landScapeSize,
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.y,
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.z + (curGlobalIndexZ - prevGlobalIndexZ) * m_landScapeSize);

				PatchManager.AddTerrainInfo(curGlobalIndexX + i - 1, z0, m_terrainGrid[listX[i], z], newPos);

				int mapX = curGlobalIndexX + i - 1;
				int mapZ = z0;
				StoreCordinatesForCastle(mapX, mapZ, newPos);
				DeleteCastle( curGlobalIndexX + i - 1, deletionZ);
			}
		}
		else if (curGlobalIndexX != prevGlobalIndexX)
		{
			Debug.Log("DDDD -> prevGlobalIndexX  " + prevGlobalIndexX  + ", curGlobalIndexX: " + curGlobalIndexX );

			int x; int x0; int deletionX;
			if (curGlobalIndexX > prevGlobalIndexX)
			{
				x0 = curGlobalIndexX + 1;
				x = PreviousCyclicIndex(prevCyclicIndexX);
				deletionX = prevGlobalIndexX - 1;
			}
			else
			{
				x0 = curGlobalIndexX - 1;
				x = NextCyclicIndex(prevCyclicIndexX);
				deletionX = prevGlobalIndexX + 1;
			}

			int[] listZ = { PreviousCyclicIndex(curCyclicIndexZ), curCyclicIndexZ, NextCyclicIndex(curCyclicIndexZ) };
			for (int i = 0; i < dim; i++)
			{
				Vector3 newPos = new Vector3(
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.x + (curGlobalIndexX - prevGlobalIndexX) * m_landScapeSize,
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.y,
				m_terrainGrid[curCyclicIndexX, curCyclicIndexZ].transform.position.z + (i - 1) * m_landScapeSize);

				PatchManager.AddTerrainInfo(x0, curGlobalIndexZ + i - 1, m_terrainGrid[x, listZ[i]], newPos);
				int mapX = x0;
				int mapZ = curGlobalIndexZ + i - 1;
				StoreCordinatesForCastle(mapX, mapZ, newPos);

				DeleteCastle(deletionX, curGlobalIndexZ + i - 1);
			}
		}
		PatchManager.MakePatches();
	}


	private List<GameObject> CastleRootPrefabs = new List<GameObject>();

	private void ClearItemsRelatedToTerrain(string terrainName)
	{
		// test: delete same named castle items:


	}

	private void StartCastleCreation()
	{
		foreach(var item in CastleBuildList)
		{
			
			castleCreator.CreateCastle(item);
		}
		CastleBuildList.Clear();
	}


	public static AreaData GetAreaData(int mapX, int mapZ)
	{
		string key = mapX + "_" + mapZ;
		if (AreaDict.ContainsKey(key))
		{
			return AreaDict[key];
		}
		return null;
	}

	void StoreCordinatesForCastle(int mapX, int mapZ, Vector3 pos)
	{
		string key = mapX + "_" + mapZ;

		// TODO if castle data is already created, dont recreate it, just pass the old data to build list

		var castleData = new CastleData();
		castleData.mapX = mapX;
		castleData.mapZ = mapZ;
		castleData.coordX = pos.x +1500; // position in center of terrain tile
		castleData.coordZ = pos.z +1500;

			if (AreaDict.ContainsKey(key))
			{
				AreaDict[key].castleData = castleData;
			}
		CastleBuildList.Add(castleData);
	}

	void DeleteCastle(int mapX, int mapZ)
	{
		Debug.Log("DeleteCastle from " + mapX + "_" + mapZ);

		string key = mapX + "_" + mapZ;
		if (AreaDict.ContainsKey(key))
		{	
			if(AreaDict[key].castleData != null)
			{
					GameObject root = AreaDict[key].castleData.rootGameObject;
					if (root != null)
					{
					Destroy(root);//.transform.position = new Vector3(0, 0, 0);
				}
					else
					{
					Debug.Log(" - root was null");
				}
						
			}
			else
			{
				Debug.Log(" - CastleData  not found");
			}	
		}
		else
		{
			Debug.Log(" - Did not find key " + key);
		}
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
        		Debug.Log("FlushTerrain, " + i + " " + j);
            	m_terrainGrid[i, j].transform.GetComponent<TerrainCollider>().enabled = true;
            	m_terrainGrid[i, j].Flush();
            	yield return new WaitForEndOfFrame();
            }

		StartCastleCreation();

		if (!savetestDone)
		{
			// TEST save terrain
			savetestDone = true;
			Debug.Log("Saving test");
			TerrainSaver.Save("testSaving", m_terrainGrid[0, 0].terrainData);
			Debug.Log("Saving test done");

		}
	}

    float StartTime;
    float oneUpdateTime;
    bool updateRound;
    int updatecounter = 0;
    float biggestUpdateTime = 0f;


	bool savetestDone = false;

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
																											
			// for displaying data in UI:
			Terrain current = m_terrainGrid[curCyclicIndexX, curCyclicIndexZ];

			Debug.Log("entered " + current.name);

			terrainName.text = current.name;
			int massType = InfiniteTerrain.GetOrAssignLandMassTypes(current.name);

			hillsValue.text = (massType & 1) > 0 ? "yes": "no";
			mountainsValue.text = (massType & 2) > 0 ? "yes": "no";
			ridgedMountainsValue.text = (massType & 4) > 0 ? "yes" : "no";
			plainsValue.text = (massType & 8) > 0 ? "yes" : "no";

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

	// Landmass types are assigned randomly, but it would make no sense if landmass types once set would change then the area is left and re-entered, so
	// values are stored. To make world persistent, these could be saved to a file.
	// 
	// If value for landmasses has already been decided, it is returned. If not, random value is assigned to the new dictionary entry. 
	public static int GetOrAssignLandMassTypes(string key)
	{

		// wip switch to AreaData

		if (AreaDict.ContainsKey(key))
		{
			return AreaDict[key].landMassValue;
		}
		else
		{

			int value = m_storedRandoms[storedRandomsCounter] + 8;

			storedRandomsCounter++;
			if (storedRandomsCounter >= m_storedRandoms.Length)
				storedRandomsCounter = 0;

			//Debug.Log("adding key >>> " + key + " value: " + value);

			var area = new AreaData();
			area.landMassValue = value;

			AreaDict.Add(key, area);
			return value;

		}

		/*
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

					//Debug.Log("adding key >>> " + key + " value: " + value);
					LandmassDict.Add(key, value);
					return value;
					*/

	}
}

