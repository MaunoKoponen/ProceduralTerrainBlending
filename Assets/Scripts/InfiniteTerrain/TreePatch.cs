using UnityEngine;
using System.Collections;

public class TreePatch : IPatch
{
    private Terrain terrain;

	private PatchManager.TerrainInfo m_info;
	private int h1;
    private TreeInstance[] treeInstances;

     NoiseModule m_treeNoise = new RidgedNoise(InfiniteLandscape.RandomSeed);

    public TreePatch(int globTileX_i, int globTileZ_i, Terrain terrain_i, int h0_i, int h1_i, PatchManager.TerrainInfo info)
    {
        terrain = terrain_i;
		m_info = info;
		h1 = h1_i;
    }
    
    public void ExecutePatch()
    {
        FillTreePatch();
        if (h1 == InfiniteTerrain.numOfTreesPerTerrain)
        {
            terrain.terrainData.treeInstances = treeInstances;
        }
    }

    private void FillTreePatch()
    {
		treeInstances = new TreeInstance[InfiniteTerrain.numOfTreesPerTerrain];
		float bushHeight = 55;
		float testHeight = 100;
		float pineHeight = 200;
		float noTreeHeight = 350;

		// TODO: use terrain type information (via m_info) to decide what kingd of forestation is needed:

		// "savannah " trees in random position, spread evenly
		// temperate medow/forest - trees form forests, meadows between them
		// " bumpy " areas - rather small bushes than trees
		// low land, small bushes, weed
		// mangrove - big trees near and in water, even if steep angle near water 
		// boreal - tundra Pine forests and open lands, trees get smaller ad rare when going up the hill, bare coasts 


		//for (int k = 0; k < InfiniteTerrain.numOfTreesPerTerrain; k++)

		//int testAmount = 1000;

		// test accessing landmassype in patch

		float ForestCenterX = Random.Range(0.5f,0.6f);
		float ForestCenterY = Random.Range(0.5f, 0.6f);

		for (int k = 0; k < InfiniteTerrain.numOfTreesPerTerrain; k++)
		{
			float x = Random.value;// ForestCenterX + Random.Range(0.0f, 0.4f);
			float y = Random.value;//ForestCenterY + Random.Range(0.0f, 0.4f);

			float forestX = ForestCenterX + Random.Range(0.0f, 0.4f);
			float forestY = ForestCenterY + Random.Range(0.0f, 0.4f);


			float angle = terrain.terrainData.GetSteepness(x, y);
			float forestAngle = terrain.terrainData.GetSteepness(forestX, forestY);


			float ht = terrain.terrainData.GetInterpolatedHeight(x, y);
			float forestHt = terrain.terrainData.GetInterpolatedHeight(forestX, forestY);


			if (ht > testHeight * 1.1f && ht < pineHeight && angle < 20  && ! m_info.HasHills)
			{
				treeInstances[k].position = new Vector3(x, ht / InfiniteTerrain.m_terrainHeight, y);
				treeInstances[k].prototypeIndex = 3;//Random.Range(1, 2);
				treeInstances[k].widthScale = Random.Range(4f, 4.5f);
				treeInstances[k].heightScale = Random.Range(4f, 4.5f);
				treeInstances[k].color = Color.white;
				treeInstances[k].lightmapColor = Color.white;
			}
			else
			{
				if (ht > InfiniteTerrain.waterHeight * 1.1f)
				{
					float noise = 1;  //m_treeNoise.FractalNoise2D(x, y, 2, 100, 0.4f); //= 1; 
					if (ht < bushHeight)
					{
						if (noise > 0)
						{
							treeInstances[k].position = new Vector3(x, ht / InfiniteTerrain.m_terrainHeight, y);
							treeInstances[k].prototypeIndex = 0;
							treeInstances[k].widthScale = Random.Range(8f, 9f);
							treeInstances[k].heightScale = Random.Range(8f, 9f);
							treeInstances[k].color = Color.white;
							treeInstances[k].lightmapColor = Color.white;
						}
					}
					else if (forestHt > bushHeight && forestHt < pineHeight && forestAngle < 20)
					{
						noise = m_treeNoise.FractalNoise2D(forestX, forestY, 2, 100, 0.4f); 
						if (noise > 0)
						{
							treeInstances[k].position = new Vector3(forestX, forestHt / InfiniteTerrain.m_terrainHeight, forestY);
							treeInstances[k].prototypeIndex = Random.Range(1, 4);
							treeInstances[k].widthScale = Random.Range(2f, 2.5f);
							treeInstances[k].heightScale = Random.Range(2f, 2.5f);
							treeInstances[k].color = Color.white;
							treeInstances[k].lightmapColor = Color.white;
						}
					}
					else if (ht > pineHeight && ht < noTreeHeight && angle < 20)
					{
						if (noise > 0)
						{
							treeInstances[k].position = new Vector3(x, ht / InfiniteTerrain.m_terrainHeight, y);
							treeInstances[k].prototypeIndex = Random.Range(4, 6) ;
							treeInstances[k].widthScale = Random.Range(2f, 2.5f);
							treeInstances[k].heightScale = Random.Range(2f, 2.5f);
							treeInstances[k].color = Color.white;
							treeInstances[k].lightmapColor = Color.white;
						}
					}
				}
				else
				{
					treeInstances[k].widthScale = 0;
					treeInstances[k].heightScale = 0;
				}
			}
		}
	}
}
