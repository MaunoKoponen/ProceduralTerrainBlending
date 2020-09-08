using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public static class PatchManager
{
    private static int terrainPatchRes = 96;
    private static int splatDetailPatchRes = 32;
    private static int treePatchRes = 8;
	private static int tempCounter = 0;

	public class TerrainInfo
    {
		public bool HasHills; // temp solution, need betterr way to query landmass types of a terrain

		public Vector3 newPosition;
		public int globalX;
		public int globalZ;
		public Terrain terrain;

		public int landmassTypes;  //  1,2,4,8... 

		public TerrainInfo(int globX, int globZ, Terrain ter, Vector3 newPos)
        {
            newPosition = newPos;
            globalX = globX;
            globalZ = globZ;
            terrain = ter;
			string key = globalX.ToString() + "_" + globalZ.ToString();
			landmassTypes = InfiniteTerrain.GetOrAssignLandMassTypes(key);
			SetParameters();
		}


		private void SetParameters()
		{
				if (((landmassTypes & 1) > 0)) // hills
				HasHills = true;
		}
    }

    public static Queue<IPatch> patchQueue = new Queue<IPatch>();
    private static List<TerrainInfo> patchList = new List<TerrainInfo>();

    public static void AddTerrainInfo(int globX, int globZ, Terrain terrain, Vector3 pos)
    {

		string xName = globX.ToString();
		string zName = globZ.ToString();
		terrain.name = xName + "_" + zName;
		
		//Debug.Log("Adding new terrainInfo to PatchList: globX: " + globX + " globZ: " + globZ);
        patchList.Add(new TerrainInfo(globX, globZ, terrain, pos));
    }

    public static void MakePatches()
    {
        foreach (TerrainInfo tI in patchList)
        {
            for (int i = 0; i < terrainPatchRes; i++)
			{
				patchQueue.Enqueue(new TerrainPatch(tI.globalX, tI.globalZ, tI.terrain,
					InfiniteTerrain.m_heightMapSize * i / terrainPatchRes, InfiniteTerrain.m_heightMapSize * (i + 1) / terrainPatchRes, tI.newPosition, tI));
			}
                
        }
        
        foreach (TerrainInfo tI in patchList)
        {
            for (int i = 0; i < splatDetailPatchRes; i++)
            {
				patchQueue.Enqueue(new SplatDetailPatch(tI.globalX, tI.globalZ, tI.terrain,
					InfiniteTerrain.m_alphaMapSize * i / splatDetailPatchRes, InfiniteTerrain.m_alphaMapSize * (i + 1) / splatDetailPatchRes, tI));
			}    
        }
       
        foreach (TerrainInfo tI in patchList)
            for (int i = 0; i < treePatchRes; i++)
			{
				patchQueue.Enqueue(new TreePatch(tI.globalX, tI.globalZ, tI.terrain,
					InfiniteTerrain.numOfTreesPerTerrain * i / treePatchRes, InfiniteTerrain.numOfTreesPerTerrain * (i + 1) / treePatchRes, tI));	
			}
                
        patchList.Clear();
    }
}
