using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class DistrictMapper : MonoBehaviour
{
	// Two-dimensional array.

	public int heightStepSize = 10;
	private static int arraySize = 4;
    private float[,] depths = new float[arraySize, arraySize];
    private CityTile[,] m_cityTiles = new CityTile[arraySize, arraySize];
    private List<ADistrict> districts = new List<ADistrict>() ;
    public GameObject prefab;

	public GameObject DistrictAccessPrefab;

	[SerializeField] public List<Color> colors;
	// Start is called before the first frame update
	void Start()
	{
		CreateDistricts();
	}
	

	public void CreateDistricts()
        {
		// populate depths:
        for (int i = 0; i < arraySize; i++)
        {
            for (int k = 0; k < arraySize; k++)
            {
               float height = Random.Range(0.0f, 30.0f);
               m_cityTiles[i,k] = new CityTile();
               // 0,10,20.... to height
               m_cityTiles[i,k].height = (Mathf.FloorToInt(height / 10.0f)) *10;

				m_cityTiles[i, k].worldPosition = new Vector3 (i * 10, m_cityTiles[i, k].height / 10, k * 10);

	}
        }
        // iterate and set up districts
        for (int i = 0; i < arraySize; i++)
        {
            for (int k = 0; k < arraySize; k++)
            {
                if (!m_cityTiles[i, k].visited)
                {
                    m_cityTiles[i, k].visited = true;
                    if (m_cityTiles[i, k].district == null)
                    {
                        var newDistrict = new ADistrict();
                        m_cityTiles[i, k].district = newDistrict;
                        newDistrict.name = "District_created_in_" + i + "_" + k;
                        newDistrict.height = m_cityTiles[i, k].height;

						newDistrict.districtAccesses = new List<ADistrictAccess>();

						districts.Add(newDistrict);
                       
					   
                            //newDistrict.color = colors[districts.Count];
                       
                    }
                    // look for visited neighbours by flood fill! with same depth - if found, set to same district
                    // if none found, create a new district
                    checkNeighbours(i,k);
                }
            }
        }

		// Iterate throug tiles and set up district accesses

		for (int y = 0; y < arraySize; y++)
		{
			for (int x = 0; x < arraySize; x++)
			{

				//top
				FindPossibleDistrictAccess(x, y - 1, m_cityTiles[x, y],0);
				// left
				FindPossibleDistrictAccess(x - 1, y, m_cityTiles[x, y],90);
				// right
				FindPossibleDistrictAccess(x + 1, y, m_cityTiles[x, y],270);
				// bottom
				FindPossibleDistrictAccess(x, y + 1, m_cityTiles[x, y],180);

				// compare height to neighbours
				// if suitable save possible district access in district
			}
		}

		// Visualization

		Debug.Log("Passed");
        for (int i = 0; i < arraySize; i++)
        {
            for (int k = 0; k < arraySize; k++)
            {
                Debug.Log("Tile " +  i + " / "+  k  +  " District: " + m_cityTiles[i, k].district.name);
                GameObject visualization = Instantiate(prefab, new Vector3(i * 10, m_cityTiles[i, k].district.height/10, k * 10), Quaternion.identity);
                visualization.GetComponent<Renderer>().material.color = m_cityTiles[i, k].district.color;
            }
        }
        foreach (var district in districts)
        {
            Debug.Log("District: " + district.name + "  " + district.height);
		}

		foreach (var district in districts)
		{
			foreach(var access in district.districtAccesses)
			{
				var position = access.myCityTile.worldPosition;

				GameObject visualization = Instantiate(DistrictAccessPrefab, position, Quaternion.identity);
				visualization.transform.Rotate(0, access.rotation, 0);
			}
		}
    }



    private void checkNeighbours(int x,  int y)
    {
        //top
        FindAndSetSameDistrictNeighbours(x,y - 1,  m_cityTiles[x, y]);
        // left
        FindAndSetSameDistrictNeighbours(x-1, y,  m_cityTiles[x, y]);
        // right
        FindAndSetSameDistrictNeighbours(x+1, y,  m_cityTiles[x, y]);
        // bottom
        FindAndSetSameDistrictNeighbours(x, y+1,  m_cityTiles[x, y]);
    }


	private void FindPossibleDistrictAccess(int x, int y, CityTile currentCheckedTile, int rotation)
	{
		var currentCheckedDistrict = currentCheckedTile.district;
		if (validPosition(x, y))
		{
			var tile = m_cityTiles[x, y];
			{
				if (tile.height == currentCheckedDistrict.height + heightStepSize)
				{

					// check if there already is similar access
					bool alreadyExists = false;

					foreach(var item in currentCheckedDistrict.districtAccesses)
					{
						if (item.accessedDistrict == m_cityTiles[x, y].district)
						{
							// already exists
							alreadyExists = true;
						}
					}

					if(! alreadyExists)
					{
						var access = new ADistrictAccess();
						access.myDistrict = currentCheckedDistrict;
						access.accessedDistrict = m_cityTiles[x, y].district;
						access.myCityTile = currentCheckedTile;
						access.rotation = rotation;
						currentCheckedDistrict.districtAccesses.Add(access);
					}
				}
			}
		}
	}
    private void FindAndSetSameDistrictNeighbours(int x, int y, CityTile currentCheckedTile)
    {
		var currentCheckedDistrict = currentCheckedTile.district;
		if (validPosition(x, y))
        {
            var tile = m_cityTiles[x, y];
            if (!tile.visited)
            {
                if (tile.height == currentCheckedDistrict.height)
                {
					// Same Height -> same district
					tile.visited = true;
                    tile.district = currentCheckedDistrict;
                    checkNeighbours(x,y);
                }
			}
        }
    }



    private bool validPosition(int x, int y)
    {
        return !(x < 0 || x >= arraySize || y < 0 || y >= arraySize);
    }
    // Update is called once per frame
    void Update()
    {
    }
}
public class CityTile
{
    public int height;
    public ADistrict district;
    public bool visited; // when creating districts

	public bool valid;

	public Vector3 worldPosition;

}

public class ADistrictAccess
{
	public ADistrict accessedDistrict;
	public ADistrict myDistrict; // Needed?

	public CityTile myCityTile; // Needed?

	public int rotation; // or make n,w,s,e enum 

}

public class ADistrict
{
    public string name;
    public int height;
    public Color color;
	public List<ADistrictAccess> districtAccesses;
}