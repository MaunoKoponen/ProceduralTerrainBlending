using UnityEngine;

public class TerrainPatch : IPatch
{
	private PatchManager.TerrainInfo m_info;
	private NoiseModule m_mountainNoise = new PerlinNoise(InfiniteLandscape.RandomSeed);
	private NoiseModule m_plainsNoise = new PerlinNoise(InfiniteLandscape.RandomSeed);
	private NoiseModule m_mountainNoiseRidged = new RidgedNoise(InfiniteLandscape.RandomSeed);

	private int landmassTypes;


	public TerrainPatch(int globTileX_i, int globTileZ_i, Terrain terrain_i, int h0_i, int h1_i, Vector3 pos_i, PatchManager.TerrainInfo info)
	{
		globalTileX = globTileX_i;
		globalTileZ = globTileZ_i;
		terrain = terrain_i;
		m_info = info;
		h0 = h0_i;
		h1 = h1_i;
		pos = pos_i;
	}

	//Todo

	// set 4 different noises (ridged mountain, round mountain, plains, small hills)

	// set up 16 landmass type  using all combinations of 4 noises on or off

	// set up TerrainPatch Landmadstype, obtained from worldLandmAsses array

	// create falloff (sine) function

	// use landmass aray [1,0,0,1] to turn on noises, but use falloff if the adjacent tiles value (0 or 1 ) is not same.


	private int globalTileX, globalTileZ, h0, h1;
	private Vector3 pos;
	private Terrain terrain;

	private int Landmasstype;

	public void ExecutePatch()
	{
		FillTerrainPatch();
		if (h1 == InfiniteTerrain.m_heightMapSize)
		{
			terrain.terrainData.SetHeights(0, 0, InfiniteTerrain.m_terrainHeights);  //SetHeights calculates terrain collider
			terrain.transform.position = pos;
		}
	}

	private void FillTerrainPatch()
	{
		int hRes = InfiniteTerrain.m_heightMapSize;
		float ratio = (float)InfiniteTerrain.m_landScapeSize / (float)hRes;
		float z0 = (InfiniteLandscape.initialGlobalIndexZ * (InfiniteTerrain.m_heightMapSize - 1)) * ratio;
		float z1 = (InfiniteLandscape.initialGlobalIndexZ * (InfiniteTerrain.m_heightMapSize - 1)) * ratio + hRes * ratio;
		float y0 = 0.0f;
		float y1 = 1.0f;

		bool plainsExist = false;
		bool hillsExist = false;
		bool mountainsExist = false;
		bool ridgedMountainsExist = false;

		string key = globalTileX.ToString() + "_" + globalTileZ.ToString();
		int massType = InfiniteTerrain.GetOrAssignLandMassTypes(key);


		if ((massType & 1) > 0)
		{
			hillsExist = true;
		}
		if ((massType & 2) > 0)
		{
			mountainsExist = true;
		}
		if ((massType & 4) > 0)
		{
			ridgedMountainsExist = true;
		}
		if ((massType & 8) > 0)
		{
			plainsExist = true;
		}

		for (int z = h0; z < h1; z++)
		{
			float worldPosZ = (z + globalTileZ * (InfiniteTerrain.m_heightMapSize - 1)) * ratio;
			float hx = Mathf.Clamp((y1 - y0) / (z1 - z0) * (worldPosZ - z0) + y1, -4, 8);

			for (int x = 0; x < hRes; x++)
			{
				float worldPosX = (x + globalTileX * (InfiniteTerrain.m_heightMapSize - 1)) * ratio;
				float sum = 0;

				if (hillsExist)
				{
					//float hills = m_mountainNoise.FractalNoise2D(worldPosX, worldPosZ, 1, 100, 0.02f) + 0.01f; // good small bumpy thing
					// makes "brain" bumps, that look odd from top   
					//float hills = -(m_mountainNoiseRidged.FractalNoise2D(worldPosX, worldPosZ, 1, 100, 0.01f)) + 0.005f; // flipped small ridge
					float hills = -(m_mountainNoiseRidged.FractalNoise2D(worldPosX, worldPosZ, 6, 250, 0.015f)); // flipped small ridge
					//hills = 0.2f; // for testing falloff map ignore noise
					hills = BlendLandmass(hills, x, z, key, 1);
					sum += hills;
				}


				if (mountainsExist)
				{
					float mountainsPerlin = m_mountainNoise.FractalNoise2D(worldPosX, worldPosZ, 4, 3000, 0.4f);
					//mountainsPerlin = 0.2f; // for testing falloff map ignore noise
					mountainsPerlin = BlendLandmass(mountainsPerlin, x, z, key, 2);
					sum += mountainsPerlin;
				}

				if (ridgedMountainsExist)
				{
					float mountainsRidged = m_mountainNoiseRidged.FractalNoise2D(worldPosX, worldPosZ, 4, 3000, 0.2f);
					//mountainsRidged = 0.2f; // for testing falloff map ignore noise
					float accentHills = m_mountainNoise.FractalNoise2D(worldPosX, worldPosZ, 1, 100, 0.005f); // good small bumpy thing
					//accentHills = 0f; // for testing falloff map ignore noise
					mountainsRidged += accentHills;
					mountainsRidged = BlendLandmass(mountainsRidged, x, z, key, 4);
					sum += mountainsRidged;

				}

				if (plainsExist)
				{
					float plains = m_mountainNoise.FractalNoise2D(worldPosX, worldPosZ, 4, 9000, 0.1f) + 0.05f; //
					//plains = 0.2f; // for testing falloff map ignore noise
					plains = BlendLandmass(plains, x, z, key, 8);
					sum += plains;
				}

				// for debugging textures, create flat surfacce
				//sum = 0.04f;

				float height = (sum) + 0.003f * hx;
				InfiniteTerrain.m_terrainHeights[z, x] = height;

				

			}
		}
	}

	// Adjust the given coordinate height value by fading the value towards  neighbouring terrain tiles without same landmasstype
	// this leads to smooth transition between different landmass types
	private float BlendLandmass(float noiseValueToAdjust, int x, int z, string myKey, int typeInt)
	{
		int xValueToUse = 0;
		int zValueToUse = 0;

		int verticalNeighbourHeight = 0;
		int horizontalNeighbourHeight = 0;
		int diagonallyOppositeHeight = 0;

		string horizontalNeighborKey = "";
		string verticalNeighborKey = "";
		string diagonalNeighborKey = "";

		int myHeight = HasLandmassType(myKey, typeInt);  // Can be 0 or 1

		// sSplit the terrain to 4 corners, get neighbors for each corners, 

		if (x < 256 && z < 256)
		{
			// top left
			horizontalNeighborKey = (globalTileX - 1).ToString() + "_" + (globalTileZ).ToString();
			verticalNeighborKey = (globalTileX).ToString() + "_" + (globalTileZ - 1).ToString();
			diagonalNeighborKey = (globalTileX - 1).ToString() + "_" + (globalTileZ - 1).ToString();
		}

		if (x >= 256 && z < 256)
		{
			// top right
			horizontalNeighborKey = (globalTileX + 1).ToString() + "_" + (globalTileZ).ToString();
			verticalNeighborKey = (globalTileX).ToString() + "_" + (globalTileZ - 1).ToString();
			diagonalNeighborKey = (globalTileX + 1).ToString() + "_" + (globalTileZ - 1).ToString();
		}

		if (x < 256 && z >= 256)
		{
			// bottom left
			horizontalNeighborKey = (globalTileX - 1).ToString() + "_" + (globalTileZ).ToString();
			verticalNeighborKey = (globalTileX).ToString() + "_" + (globalTileZ + 1).ToString();
			diagonalNeighborKey = (globalTileX - 1).ToString() + "_" + (globalTileZ + 1).ToString();

		}

		if (x >= 256 && z >= 256)
		{
			// bottom right
			horizontalNeighborKey = (globalTileX + 1).ToString() + "_" + (globalTileZ).ToString();
			verticalNeighborKey = (globalTileX).ToString() + "_" + (globalTileZ + 1).ToString();
			diagonalNeighborKey = (globalTileX + 1).ToString() + "_" + (globalTileZ + 1).ToString();
		}

		verticalNeighbourHeight = HasLandmassType(verticalNeighborKey, typeInt);
		horizontalNeighbourHeight = HasLandmassType(horizontalNeighborKey, typeInt);
		diagonallyOppositeHeight = HasLandmassType(diagonalNeighborKey, typeInt);


		if (myHeight == 0)
			return noiseValueToAdjust; // no mask

		if (myHeight == 1 && verticalNeighbourHeight == 1 && horizontalNeighbourHeight == 1 && diagonallyOppositeHeight == 1)
			return noiseValueToAdjust; // no mask

		if (myHeight == 1 && verticalNeighbourHeight == 1 && horizontalNeighbourHeight == 1 && diagonallyOppositeHeight == 0)
		{
			xValueToUse = x;
			zValueToUse = 256;

			float falloff1 = InfiniteTerrain.fallOffTable[xValueToUse, zValueToUse];

			xValueToUse = 256;
			zValueToUse = z;

			float falloff2 = InfiniteTerrain.fallOffTable[xValueToUse, zValueToUse];

			return noiseValueToAdjust * Mathf.Max(falloff1, falloff2);
		}

		if (myHeight == 1 && verticalNeighbourHeight == 0 && horizontalNeighbourHeight == 0)
		{
			xValueToUse = x;
			zValueToUse = z;
		}

		if (myHeight == 1 && verticalNeighbourHeight == 1 && horizontalNeighbourHeight == 0)
		{
			xValueToUse = x;
			zValueToUse = 256;

		}
		if (myHeight == 1 && verticalNeighbourHeight == 0 && horizontalNeighbourHeight == 1)
		{
			xValueToUse = 256;
			zValueToUse = z;
		}

		float falloff = InfiniteTerrain.fallOffTable[xValueToUse, zValueToUse];

		return noiseValueToAdjust * falloff;
	}


// 
	private int HasLandmassType(string key, int typeInt)
	{
		int massType = InfiniteTerrain.GetOrAssignLandMassTypes(key);
		bool is1 = false;

		switch (typeInt)
		{

			case 1:
				if (((massType & 1) > 0)) // hills
					is1 = true;
				break;
			case 2:
				if (((massType & 2) > 0)) // mountains
					is1 = true;
				break;
			case 4:
				if (((massType & 4) > 0)) // ridged mountains
					is1 = true;
				break;
			case 8:
				if (((massType & 8) > 0)) /// plains
					is1 = true;
				break;
		}

		if (is1 == true)
			return 1;
		else
			return 0;
	}
}
