using System.Collections.Generic;
using UnityEngine;

public class CastleCreator : MonoBehaviour
{

	// GameObject names tell on what sides there are walls:

	[SerializeField] private GameObject HelperObject;

	[SerializeField] private GameObject Floor;
	public GameObject[] Wall_Normal_Parts;
	[SerializeField] private GameObject Wall_High_Normal;
	[SerializeField] private GameObject Wall_High_entrance;

	private CastleData m_data; // for storing the root object for destroying the castle

	private enum NeighbourType
	{
		SameLevel,
		Higher,
		Lower,
		Empty,
		EmptyAccessable
	}

	public enum Direction
	{
		N,
		W,
		S,
		E
	}


	public int m_castleSize = 2;

	public int exitAmount = 4;

	public float maxExitHeightDifference = 4;


	public int stepSize = 20; // the y step size of terrain pieces
	public int castleTileSize = 100;
	public CastleTile[,] CastleMap;

	public List<District> Districts;

	public void CreateCastle(CastleData data)
	{
		Debug.Log("Creating Castle to " + data.mapX + "  " + data.mapZ);

		// TODO make castle parameters depend on the terrain features: size, y step size, max y height of a castle tile 


		m_data = data;

		CastleMap = new CastleTile[m_castleSize, m_castleSize];
		// map terrain coordinateds to 2d castlePiecesArray
		// analyze terrain, put 	
		SetTileHeight(data.coordX, data.coordZ);
		InstantiateCastleTiles();


		// todo process
		/*

		set tile heights
		find valid tiles (no too big height differences)
		for each tile, if valid, look for same height neighbours recursively, 
		
		set district, - if tile not in recursive yet, loop, create new district,  if visited, skip, otherwise, get recursion loops "this district"

		At what point districtAccesses? 

		mark as districtset, 
		put to district instance

		Now each tile should have district set and each district in Districts should have list of tiles it consists of.

		*/
	}



	/*
	int distance(Tile a, Tile b)
	{
		int dx = a.getX() - b.getX();
		int dy = a.getY() - b.getY();
		return Math.abs(dx) + Math.abs(dy);
	}
	*/

	public void SetTileHeight(float mapX, float mapZ)
	{
		// sample the given area, set the FloorHeight
		float startX = mapX;
		float startZ = mapZ;

		for (int i = 0; i < m_castleSize; i++)
		{
			for (int k = 0; k < m_castleSize; k++)
			{
				CastleMap[i, k] = new CastleTile();
				RaycastHit hit;

				// Sample all corners, pick highest value

				// Sample hit center of the tile, so offset for corners

				float highestCornerValue = 0;

				float iVal = startX + (i * castleTileSize);
				float zVal = startZ + k * castleTileSize;
				float halfTile = castleTileSize / 2;

				// TODO loop:
				if (Physics.Raycast(new Vector3(iVal + halfTile, 1000, zVal + halfTile), -Vector3.up, out hit))
				{
					if (highestCornerValue < hit.point.y)
						highestCornerValue = hit.point.y;
				}
				if (Physics.Raycast(new Vector3(iVal + halfTile, 1000, zVal - halfTile), -Vector3.up, out hit))
				{
					if (highestCornerValue < hit.point.y)
						highestCornerValue = hit.point.y;
				}
				if (Physics.Raycast(new Vector3(iVal - halfTile, 1000, zVal + halfTile), -Vector3.up, out hit))
				{
					if (highestCornerValue < hit.point.y)
						highestCornerValue = hit.point.y;
				}
				if (Physics.Raycast(new Vector3(iVal - halfTile, 1000, zVal - halfTile), -Vector3.up, out hit))
				{
					if (highestCornerValue < hit.point.y)
						highestCornerValue = hit.point.y;
				}

				if (Physics.Raycast(new Vector3(iVal, 1000, zVal), -Vector3.up, out hit))
				{
					CastleMap[i, k].TileCenterHeight = hit.point.y;
				}

				// Make castle parts y pos be in Steps
				float floorHeight = (Mathf.Ceil(highestCornerValue / stepSize)) * stepSize;

				CastleMap[i, k].FloorHeight = floorHeight; // stored for comparing neighbours

				CastleMap[i, k].position = new Vector3(startX + (i * castleTileSize), floorHeight, (startZ + k * castleTileSize));
			}
		}
	}

	public void InstantiateCastleTiles()
	{

		for (int i = 0; i < m_castleSize; i++)
		{
			for (int k = 0; k < m_castleSize; k++)
			{
				CastleMap[i, k].OkToInstantiate = DoInstantiate(i, k);
			}
		}

		GameObject aParent = Instantiate(HelperObject, CastleMap[0, 0].position, Quaternion.identity);
		//aParent.name = CastleMap[0, 0].position.x.ToString() + "_" + CastleMap[0, 0].position.z.ToString();

		aParent.name = "Castle_" + m_data.mapX + "_" + m_data.mapZ;

		// store root object for destroying
		m_data.rootGameObject = aParent;

		for (int i = 0; i < m_castleSize; i++)
		{
			for (int k = 0; k < m_castleSize; k++)
			{
				if (CastleMap[i, k].FloorHeight < InfiniteLandscape.waterHeight)
					continue;

				if (CastleMap[i, k].OkToInstantiate == false)
					continue;

				GameObject tile;
				tile = Instantiate(Floor, CastleMap[i, k].position, Quaternion.identity, aParent.transform);
				tile.name = "Floor_" + i + "_" + k;

				GameObject wallNormal = Wall_Normal_Parts[Random.Range(0, 2)];


				if (RightNeighbourType(i, k) == NeighbourType.EmptyAccessable)
					Instantiate(Wall_High_entrance, CastleMap[i, k].position, Quaternion.Euler(0, 180, 0), aParent.transform);
				if (RightNeighbourType(i, k) == NeighbourType.Empty)
				{					
					Instantiate(Wall_High_Normal, CastleMap[i, k].position, Quaternion.Euler(0, 180, 0), aParent.transform);
				}
				if (RightNeighbourType(i, k) == NeighbourType.Lower)
				{
					Instantiate(wallNormal, CastleMap[i, k].position, Quaternion.Euler(0, 180, 0), aParent.transform);
				}

				if (LeftNeighbourType(i, k) == NeighbourType.EmptyAccessable)
					Instantiate(Wall_High_entrance, CastleMap[i, k].position, Quaternion.Euler(0, 0, 0), aParent.transform);
				if (LeftNeighbourType(i, k) == NeighbourType.Empty)
				{
					Instantiate(Wall_High_Normal, CastleMap[i, k].position, Quaternion.Euler(0, 0, 0), aParent.transform);
				}
				if (LeftNeighbourType(i, k) == NeighbourType.Lower)
				{
					Instantiate(wallNormal, CastleMap[i, k].position, Quaternion.Euler(0, 0, 0), aParent.transform);
				}

				if (TopNeighbourType(i, k) == NeighbourType.EmptyAccessable)
					Instantiate(Wall_High_entrance, CastleMap[i, k].position, Quaternion.Euler(0, 90, 0), aParent.transform);
				if (TopNeighbourType(i, k) == NeighbourType.Empty)
				{
					Instantiate(Wall_High_Normal, CastleMap[i, k].position, Quaternion.Euler(0, 90, 0), aParent.transform);
				}
				if (TopNeighbourType(i, k) == NeighbourType.Lower)
				{
					Instantiate(wallNormal, CastleMap[i, k].position, Quaternion.Euler(0, 90, 0), aParent.transform);
				}

				if (BottomNeighbourType(i, k) == NeighbourType.EmptyAccessable)
					Instantiate(Wall_High_entrance, CastleMap[i, k].position, Quaternion.Euler(0, 270, 0), aParent.transform);
				if (BottomNeighbourType(i, k) == NeighbourType.Empty)
				{
					Instantiate(Wall_High_Normal, CastleMap[i, k].position, Quaternion.Euler(0, 270, 0), aParent.transform);
				}
				if (BottomNeighbourType(i, k) == NeighbourType.Lower)
				{
					Instantiate(wallNormal, CastleMap[i, k].position, Quaternion.Euler(0, 270, 0), aParent.transform);
				}	
			}
		}
	}

	

	// check if the tile is in the border tile af array, or if the height difference will be too
	// much so wall tile cant cover the space completely 
	bool DoInstantiate(int i, int k)
	{
		if (k + 1 >= m_castleSize || CastleMap[i, k + 1] == null)
			return false;
		if (CastleMap[i, k].FloorHeight - CastleMap[i, k + 1].FloorHeight > stepSize * 1.2f)
			return false;

		if (k == 0 || CastleMap[i, k - 1] == null)
			return false;
		if (CastleMap[i, k].FloorHeight - CastleMap[i, k - 1].FloorHeight > stepSize * 1.2f)
			return false;

		if (i + 1 >= m_castleSize || CastleMap[i + 1, k] == null)
			return false;
		if (CastleMap[i, k].FloorHeight - CastleMap[i + 1, k].FloorHeight > stepSize * 1.2f)
			return false;

		if (i == 0 || CastleMap[i - 1, k] == null)
			return false;
		if (CastleMap[i, k].FloorHeight - CastleMap[i - 1, k].FloorHeight > stepSize * 1.2f)
			return false;

		return true;
	}


	NeighbourType TopNeighbourType(int i, int k)
	{
		if (k + 1 >= m_castleSize || CastleMap[i, k + 1] == null)
			return NeighbourType.Empty; // failsafe

		if (CastleMap[i, k + 1].OkToInstantiate == false)
		{
			if (CastleMap[i, k].FloorHeight - CastleMap[i, k + 1].TileCenterHeight < maxExitHeightDifference)
				return NeighbourType.EmptyAccessable;
			else
				return NeighbourType.Empty;  // the neighboring tile will not be created, we need wall on this side
		}
		if (CastleMap[i, k].FloorHeight > CastleMap[i, k + 1].FloorHeight)
			return NeighbourType.Lower;

		return NeighbourType.SameLevel;
	}

	NeighbourType BottomNeighbourType(int i, int k)
	{
		if (k == 0 || CastleMap[i, k - 1] == null)
			return NeighbourType.Empty; // failsafe

		if (CastleMap[i, k - 1].OkToInstantiate == false)
		{
			if (CastleMap[i, k].FloorHeight - CastleMap[i, k - 1].FloorHeight < maxExitHeightDifference)
				return NeighbourType.EmptyAccessable;  
			else
				return NeighbourType.Empty;  // the neighboring tile will not be created, we need wall on this side
		}
			

		if (CastleMap[i, k].FloorHeight > CastleMap[i, k - 1].FloorHeight)
			return NeighbourType.Lower;

		return NeighbourType.SameLevel;
	}

	NeighbourType LeftNeighbourType(int i, int k)
	{
		if (i == 0 || CastleMap[i - 1, k] == null)
			return NeighbourType.Empty; // failsafe


		if (CastleMap[i-1, k].OkToInstantiate == false)
		{
			if (CastleMap[i, k].FloorHeight - CastleMap[i-1, k].FloorHeight < maxExitHeightDifference)
				return NeighbourType.EmptyAccessable;
			else
				return NeighbourType.Empty;  // the neighboring tile will not be created, we need wall on this side
		}

		if (CastleMap[i, k].FloorHeight > CastleMap[i - 1, k].FloorHeight)
			return NeighbourType.Lower;

		return NeighbourType.SameLevel;
	}

	NeighbourType RightNeighbourType(int i, int k)
	{
		if (i + 1 >= m_castleSize || CastleMap[i + 1, k] == null)
			return NeighbourType.Empty; // failsafe

		if (CastleMap[i + 1, k].OkToInstantiate == false)
		{
			if (CastleMap[i, k].FloorHeight - CastleMap[i + 1, k].FloorHeight < maxExitHeightDifference)
				return NeighbourType.EmptyAccessable;
			else
				return NeighbourType.Empty;  // the neighboring tile will not be created, we need wall on this side
		}
		if (CastleMap[i, k].FloorHeight > CastleMap[i + 1, k].FloorHeight)
			return NeighbourType.Lower;

		return NeighbourType.SameLevel;
	}
}

public class CastleTile
{
	District district;

	List<DistrictAccess> districtAccesses;

	public float FloorHeight;
	public float TileCenterHeight;
	public Vector3 position;
	public bool OkToInstantiate;
	public GameObject CastleGameObject;
}

// Neighbouring tiles that are in same height.
// has 1 (or 2) entraces to neighbouring districts (which always are ramps)

//Note that there  can be separate districts in same height

public class District 
{

	List<CastleTile> tiles;

	List<District> neighbourDistricts;
	// or..
	List<DistrictAccess> districtAccesses;

}

// access to other district
// since there can be several accesses to different districts in one tile, the direction needs to be stored
public class DistrictAccess
{
	public CastleCreator.Direction direction;
	public CastleTile tile;

	public District accessedDistrict;
	public District myDistrict;

}