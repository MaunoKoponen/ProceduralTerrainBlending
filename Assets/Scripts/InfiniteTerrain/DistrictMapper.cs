using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class DistrictMapper : MonoBehaviour
{
    // Two-dimensional array.
    private static int arraySize = 4;
    private float[,] depths = new float[arraySize, arraySize];
    private CityTile[,] m_cityTiles = new CityTile[arraySize, arraySize];
    private List<ADistrict> districts = new List<ADistrict>() ;
    public GameObject prefab;
    [SerializeField] public List<Color> colors;
    // Start is called before the first frame update
    void Start()
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
                        districts.Add(newDistrict);
                        //if (colors.Count < districts.Count)
                        //{
                            newDistrict.color = colors[districts.Count];
                        //}
                    }
                    // look for visited neighbours by flood fill! with same depth - if found, set to same district
                    // if none found, create a new district
                    checkNeighbours(i,k);
                }
            }
        }
        // visualization
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
    }

    private void checkNeighbours(int x,  int y)
    {
        //top
        FindAndSetSameDistrictNeighbours(x,y - 1,  m_cityTiles[x, y].district);
        // left
        FindAndSetSameDistrictNeighbours(x-1, y,  m_cityTiles[x, y].district);
        // right
        FindAndSetSameDistrictNeighbours(x+1, y,  m_cityTiles[x, y].district);
        // bottom
        FindAndSetSameDistrictNeighbours(x, y+1,  m_cityTiles[x, y].district);
    }

    private void FindAndSetSameDistrictNeighbours(int x, int y, ADistrict currentCheckedDistrict)
    {
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
				else
				{
					// Different Height, possible districtAccess
					// TODO....

					// 


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

}
public class ADistrict
{
    public string name;
    public int height;
    public Color color;
}