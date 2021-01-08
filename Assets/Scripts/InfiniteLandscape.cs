using UnityEngine;
using System.Collections;

public class InfiniteLandscape : MonoBehaviour
{
	public static int RandomSeed = 5;
	public GameObject PlayerObject;

    public static float waterHeight = 50;
 
    public static float m_landScapeSize = 3072;

    // Amount of terrains used: 3 = 3x3:
	protected const int dim = 3;


	//  Following 2 values determine the starting position of player in world
	// Unit here is "terrain", not meters or such

	// Values need to divisable by 3 -  TODO fix this so that value can be any integer
	public static int initialGlobalIndexX = 333; //12;
	public static int initialGlobalIndexZ = 333; //999;

	protected bool patchIsFilling = false;
    protected int prevGlobalIndexX = -1;
    protected int prevGlobalIndexZ = -1;
    protected int curGlobalIndexX = initialGlobalIndexX + 1;
    protected int curGlobalIndexZ = initialGlobalIndexZ + 1;
    protected int prevLocalIndexX = -1;
    protected int prevLocalIndexZ = -1;
    protected int curLocalIndexX = 1;
    protected int curLocalIndexZ = 1;
    protected int prevCyclicIndexX = -1;
    protected int prevCyclicIndexZ = -1;
    protected int curCyclicIndexX = 1;
    protected int curCyclicIndexZ = 1;

    protected bool updateLandscape = false;

    protected bool UpdateIndexes()
    {

		int currentLocalIndexX = GetLocalIndex(PlayerObject.transform.position.x);
        int currentLocalIndexZ = GetLocalIndex(PlayerObject.transform.position.z);

        if (curLocalIndexX != currentLocalIndexX || curLocalIndexZ != currentLocalIndexZ)
        {

			Debug.LogError("----------------- UpdateIndexes -> changed -----------------");


			prevLocalIndexX = curLocalIndexX;
            curLocalIndexX = currentLocalIndexX;
            prevLocalIndexZ = curLocalIndexZ;
            curLocalIndexZ = currentLocalIndexZ;

            int dx = curLocalIndexX - prevLocalIndexX;
            int dz = curLocalIndexZ - prevLocalIndexZ;
            prevGlobalIndexX = curGlobalIndexX;
            curGlobalIndexX += dx;
            prevGlobalIndexZ = curGlobalIndexZ;
            curGlobalIndexZ += dz;

            prevCyclicIndexX = curCyclicIndexX;
            curCyclicIndexX = curGlobalIndexX % dim;
            prevCyclicIndexZ = curCyclicIndexZ;
            curCyclicIndexZ = curGlobalIndexZ % dim;

			Debug.Log("Entered new terrain at : " + curGlobalIndexX + "  " + curGlobalIndexZ);
			

			return true;
        }
        else return false;
    }

    protected int GetLocalIndex(float x)
    {
        return (Mathf.CeilToInt(x / m_landScapeSize));
    }

   
	private void Start() {
	}

    protected virtual void Update()
    {
        if (UpdateIndexes())
            updateLandscape = true;
        else
            updateLandscape = false;
    }
}
