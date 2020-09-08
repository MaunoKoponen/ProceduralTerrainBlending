using UnityEngine;
using System.Collections;

public class SplatDetailPatch : IPatch  //To save some calls I have merged the splat & details patches
{
    private float biggestStoneNoise = 0f;
	
	private Terrain terrain;
	private PatchManager.TerrainInfo m_info;
	
	private int globalTileX, globalTileZ, h0, h1;

    private NoiseModule m_detailNoise = new PerlinNoise(InfiniteLandscape.RandomSeed);
    //private NoiseModule m_SplatNoise = new PerlinNoise(InfiniteLandscape.RandomSeed);

    public SplatDetailPatch(int globTileX_i, int globTileZ_i, Terrain terrain_i, int h0_i, int h1_i, PatchManager.TerrainInfo info )
    {
        terrain = terrain_i;
		m_info = info;
		h0 = h0_i;
        h1 = h1_i;
        globalTileX = globTileX_i;
        globalTileZ = globTileZ_i;
    }

    public void ExecutePatch()
    {
        FillSplatDetailPatch();
        if (h1 == InfiniteTerrain.m_alphaMapSize)
        {
            terrain.terrainData.SetAlphamaps(0, 0, InfiniteTerrain.m_alphaMap);

            terrain.terrainData.SetDetailLayer(0, 0, 0, InfiniteTerrain.detailMap0);
            terrain.terrainData.SetDetailLayer(0, 0, 1, InfiniteTerrain.detailMap1);
            terrain.terrainData.SetDetailLayer(0, 0, 2, InfiniteTerrain.detailMap2);
            terrain.terrainData.SetDetailLayer(0, 0, 3, InfiniteTerrain.detailMap3);
            
        }
    }


    private void FillSplatDetailPatch()
    {
		float snowHeight = 500;
		float tundraHeight = 300;
		float highlandsHeight = 100;
		float sandHeight = 60;

		float ratio = (float)InfiniteLandscape.m_landScapeSize / (float)InfiniteTerrain.m_heightMapSize;

        for (int x = h0; x < h1; x++)
        {
			float worldPosX = (x + globalTileX * (InfiniteTerrain.m_alphaMapSize - 1)) * ratio;
			for (int z = 0; z < InfiniteTerrain.m_alphaMapSize; z++)
			{	
				float worldPosZ = (z + globalTileZ * (InfiniteTerrain.m_alphaMapSize - 1)) * ratio;

				float normX = x * 1.0f / (InfiniteTerrain.m_alphaMapSize - 1);
				float normZ = z * 1.0f / (InfiniteTerrain.m_alphaMapSize - 1);

				float angle = terrain.terrainData.GetSteepness(normX, normZ);
				float height = terrain.terrainData.GetInterpolatedHeight(normX, normZ);
				float slopeValue = angle / 90.0f;

				InfiniteTerrain.detailMap0[z, x] = 0;
				InfiniteTerrain.detailMap1[z, x] = 0;
				InfiniteTerrain.detailMap2[z, x] = 0;
				InfiniteTerrain.detailMap3[z, x] = 0;

				InfiniteTerrain.m_alphaMap[z, x, 0] = 0; //  remove reseting once logic is ready
				InfiniteTerrain.m_alphaMap[z, x, 1] = 0;
				InfiniteTerrain.m_alphaMap[z, x, 2] = 0;
				InfiniteTerrain.m_alphaMap[z, x, 3] = 0;
				InfiniteTerrain.m_alphaMap[z, x, 4] = 0;

				// The height manipulation leads to green grass to be prominent in sealeve, yellow grass in highlands
				float detailNoise = m_detailNoise.FractalNoise2D(worldPosX, worldPosZ, 5, 100, 3.0f) + 2.2f - (height/100.0f);

				float amountLeft = 1.0f;

				if (height > snowHeight)
				{
					InfiniteTerrain.m_alphaMap[z, x, 0] = 1; //all snow;
					InfiniteTerrain.m_alphaMap[z, x, 1] = 0;
					InfiniteTerrain.m_alphaMap[z, x, 2] = 0;
					InfiniteTerrain.m_alphaMap[z, x, 3] = 0;
					InfiniteTerrain.m_alphaMap[z, x, 4] = 0;
				}
				/*
				else if ( height < 51) // test for beach sand
				{
					InfiniteTerrain.m_alphaMap[z, x, 0] = 0;
					InfiniteTerrain.m_alphaMap[z, x, 1] = 0;
					InfiniteTerrain.m_alphaMap[z, x, 2] = 0;
					InfiniteTerrain.m_alphaMap[z, x, 3] = 0;
					InfiniteTerrain.m_alphaMap[z, x, 4] = 1;
				}
				*/
				else
				{
					if (height > tundraHeight)
					{
						//slide the snow value from 1 snow to 0
						float slideValue = map(height, tundraHeight, snowHeight,  0, 1);

						// based on slope reserve portion to snow, pass rest to following
						slideValue =  Mathf.Max(slideValue, (slideValue * 3 * ( 1 - slopeValue)));

						InfiniteTerrain.m_alphaMap[z, x, 0] = slideValue;
						amountLeft = 1 - slideValue;
					}
					else
					{
						// no snow below tundra
						InfiniteTerrain.m_alphaMap[z, x, 0] = 0;
					}

					float textureNoise = detailNoise;

					// big pattern
					//float textureNoise = m_detailNoise.FractalNoise2D(worldPosX, worldPosZ, 5, 3000, 3.0f);
					float clumpNoise = m_detailNoise.FractalNoise2D(worldPosX, worldPosZ, 5, 30, 3.0f);

					if (slopeValue > 0.55f)
					{						
						// All left from snow is rock
						InfiniteTerrain.m_alphaMap[z, x, 1] = amountLeft;
					}
					else
					{
						var percent = map(slopeValue, 0.3f, 0.4f, 0, 1);
						
						//Blend rock
						InfiniteTerrain.m_alphaMap[z, x, 1] = percent;

						amountLeft  -= percent;

						// for rest, alternate moss and grass with noise pattern
						//InfiniteTerrain.m_alphaMap[z, x, 2] = amountLeft * (1- textureNoise);
						var grassValue = amountLeft * textureNoise;

						if(textureNoise > 0.5f)
							InfiniteTerrain.m_alphaMap[z, x, 3] = amountLeft;
						else
							InfiniteTerrain.m_alphaMap[z, x, 2] = amountLeft;

						if (InfiniteTerrain.RenderDetailsStatic)
						{
							if (grassValue > 0.7f && amountLeft > 0 && height < tundraHeight && detailNoise > 0.7f && height > InfiniteLandscape.waterHeight + 3f)
							{
								if(clumpNoise>0.5f)
									InfiniteTerrain.detailMap0[z, x] = 100; // OK density
																		// Dist 300, density 0.5, res/patch 16
							}
							else if( amountLeft > 0 && height < tundraHeight && height > InfiniteLandscape.waterHeight +3f)
							{

								if (clumpNoise < 0.5f)
								InfiniteTerrain.detailMap1[z, x] = 40; // value 20 clumps clearly separated
							}

							//float stoneNoise = m_detailNoise.FractalNoise2D(worldPosX, worldPosZ, 5, 100, 3.0f) + 2.2f - (height / 100.0f); uniformly dropped
							float stoneNoise = m_detailNoise.FractalNoise2D(worldPosX, worldPosZ, 2, 100, 3.0f) + 2.2f - (height / 100.0f);
							float beachStoneNoise = m_detailNoise.FractalNoise2D(worldPosX, worldPosZ, 2, 100, 3.0f);


						
							// stones  here and there
							if (stoneNoise > 0.7f && stoneNoise < 0.72f && height < tundraHeight && slopeValue < 0.3f)
								InfiniteTerrain.detailMap2[z, x] = 5;

							// near waterline stones
							if (beachStoneNoise > 0.7f && height > 50 && height < 60 && slopeValue < 0.1f)
								InfiniteTerrain.detailMap2[z, x] = 5;
						}
					}
				}
			}
		}
	}

	float map(float s, float a1, float a2, float b1, float b2)
	{
		return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
	}
}