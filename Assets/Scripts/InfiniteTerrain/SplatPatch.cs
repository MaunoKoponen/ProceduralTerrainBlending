using UnityEngine;
using System.Collections;

public class SplatDetailPatch : IPatch  //To save some calls I have merged the splat & details patches
{
    private Terrain terrain;
	private PatchManager.TerrainInfo m_info;
	
	private int globalTileX, globalTileZ, h0, h1;

    private NoiseModule m_detailNoise = new PerlinNoise(InfiniteLandscape.RandomSeed);
    private NoiseModule m_SplatNoise = new PerlinNoise(InfiniteLandscape.RandomSeed);

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

			for (int z = 0; z < InfiniteTerrain.m_alphaMapSize; z++)
			{
				float worldPosX = (x + globalTileX * (InfiniteTerrain.m_heightMapSize - 1)) * ratio;
				float worldPosZ = (z + globalTileZ * (InfiniteTerrain.m_heightMapSize - 1)) * ratio;

				
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


				//float detailNoise = m_detailNoise.FractalNoise2D(worldPosX, worldPosZ, 2, 200, 1.0f);
				float detailNoise = m_detailNoise.FractalNoise2D(worldPosX, worldPosZ, 5, 300, 3.0f) + 2.2f - (height/100.0f);


				float totalAmount = 1.0f;

				if (height > snowHeight)
				{
					InfiniteTerrain.m_alphaMap[z, x, 0] = 1; //all snow;
					InfiniteTerrain.m_alphaMap[z, x, 1] = 0;
					InfiniteTerrain.m_alphaMap[z, x, 2] = 0;
					InfiniteTerrain.m_alphaMap[z, x, 3] = 0;
					
				}
				else
				{
					if (height > tundraHeight)
					{
						//slide the snow value from 1 snow to 0
						float slideValue = map(height, tundraHeight, snowHeight,  0, 1);

						// based on slope reserve portion to snow, pass rest to following
						slideValue =  Mathf.Max(slideValue, (slideValue * 3 * ( 1 - slopeValue)));

						InfiniteTerrain.m_alphaMap[z, x, 0] = slideValue;
						totalAmount = 1 - slideValue;
					}
					else
					{
						// no snow below tundra
						InfiniteTerrain.m_alphaMap[z, x, 0] = 0;


		//----- Set the Detail Objects, grass here:------------

						if (detailNoise > 0.0f)
						{
							//float rnd = Random.value;
							//if (rnd < 0.5f)
								InfiniteTerrain.detailMap0[z, x] = 20;
							//else if (rnd < 0.66f)
							//	InfiniteTerrain.detailMap1[z, x] = 1;
							//else if (rnd < 0.86f)
							//	InfiniteTerrain.detailMap2[z, x] = 1;
							//else if (rnd < 0.96f)
							//	InfiniteTerrain.detailMap3[z, x] = 1;
						}
		//------------------------------------------------------
					}


					float textureNoise = detailNoise;//m_detailNoise.FractalNoise2D(worldPosX, worldPosZ, 2, 1000, 1.0f);


					if (slopeValue > 0.4f)
					{						
						// All Rock
						InfiniteTerrain.m_alphaMap[z, x, 1] = totalAmount;
					}
					else if (slopeValue > 0.2f || true)
					{
						// Should alternate

						//var percent = map(slopeValue, 0.2f, 0.4f, 0, 1);
						
						InfiniteTerrain.m_alphaMap[z, x, 2] = totalAmount * (1- textureNoise);
						InfiniteTerrain.m_alphaMap[z, x, 3] = totalAmount * (textureNoise);
					}
					else
					{
					
					
						if(height >highlandsHeight)
							InfiniteTerrain.m_alphaMap[z, x, 2] = totalAmount;
						else
							InfiniteTerrain.m_alphaMap[z, x, 3] = totalAmount;
						
						//var percent = map(slopeValue, 0f, 0.2f, 0, 1);
						//InfiniteTerrain.m_alphaMap[z, x, 1] = totalAmount * percent;
						//InfiniteTerrain.m_alphaMap[z, x, 3] = 1 - (totalAmount * percent);
					}
					
				}
			}
		}
	}

	float map(float s, float a1, float a2, float b1, float b2)
	{
		return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
	}

	void FillSplatDetailPatch2()
    {
        /*
        float heightThres = 700;
        float sandHeight = 80;
        float ratio = (float)InfiniteLandscape.m_landScapeSize / (float)InfiniteTerrain.m_heightMapSize;

        int debug = 0;

        for (int x = h0; x < h1; x++)
        {
            

            for (int z = 0; z < InfiniteTerrain.m_alphaMapSize; z++)
            {
                float worldPosX = (x + globalTileX * (InfiniteTerrain.m_heightMapSize - 1)) * ratio;
                float worldPosZ = (z + globalTileZ * (InfiniteTerrain.m_heightMapSize - 1)) * ratio;

                InfiniteTerrain.detailMap0[z, x] = 0;
                InfiniteTerrain.detailMap1[z, x] = 0;
                InfiniteTerrain.detailMap2[z, x] = 0;
                InfiniteTerrain.detailMap3[z, x] = 0;

                InfiniteTerrain.m_alphaMap[z, x, 0] = 0;
                InfiniteTerrain.m_alphaMap[z, x, 1] = 0;
                InfiniteTerrain.m_alphaMap[z, x, 2] = 0;
                InfiniteTerrain.m_alphaMap[z, x, 3] = 0;

                float normX = x * 1.0f / (InfiniteTerrain.m_alphaMapSize - 1);
                float normZ = z * 1.0f / (InfiniteTerrain.m_alphaMapSize - 1);

                float angle = terrain.terrainData.GetSteepness(normX, normZ);
                float height = terrain.terrainData.GetInterpolatedHeight(normX, normZ);
                float frac = angle / 90.0f;

                //debug++;
                
                if (height < heightThres)
                {
                    //details
                    if (frac < 0.6f && height > 1.1f * InfiniteLandscape.waterHeight)
                    {
                        // float noise = m_detailNoise.FractalNoise2D(worldPosX, worldPosZ, 2, 3000, 1.0f);
                        float noise = m_detailNoise.FractalNoise2D(worldPosX, worldPosZ, 2, 1000, 1.0f);


                        float c = Mathf.Clamp01(Mathf.Pow(height / sandHeight, 3));

                        InfiniteTerrain.m_alphaMap[z, x, 0] = frac;
                        InfiniteTerrain.m_alphaMap[z, x, 1] = (1 - frac) * c;
                        InfiniteTerrain.m_alphaMap[z, x, 2] = 0;
                        InfiniteTerrain.m_alphaMap[z, x, 3] = 1 - frac - (1 - frac) * c;
                    }
                }
                else
                {
                    InfiniteTerrain.m_alphaMap[z, x, 0] = 1 - height / 500;
                    InfiniteTerrain.m_alphaMap[z, x, 1] = 0;
                    InfiniteTerrain.m_alphaMap[z, x, 2] = height / 500;
                    InfiniteTerrain.m_alphaMap[z, x, 3] = 0;

                    //InfiniteTerrain.m_alphaMap[z, x, 0] = 1 - height / 500;
                    //InfiniteTerrain.m_alphaMap[z, x, 1] = 0;
                    //InfiniteTerrain.m_alphaMap[z, x, 2] = height / 500;
                    //InfiniteTerrain.m_alphaMap[z, x, 3] = 1;
                }
            }
        }
        */
    }
}