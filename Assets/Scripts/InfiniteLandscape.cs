using UnityEngine;
using System.Collections;

public class InfiniteLandscape : MonoBehaviour
{
	public static int RandomSeed = 40; // lowland and easy hills 38; //ver good, varied landmasses: 37;// mountains, some lakes: 35;//small islands 29; // lots of plains, mounntains in distance: 28;// bumpy mountains: 27;// good:26;

	public GameObject PlayerObject;

    public static float waterHeight = 50;

    public static float m_landScapeSize = 3072;

    protected const int dim = 3;
    public static int initialGlobalIndexX = 9003; 
	
	// TODO: There is a bug somewhere that breaks things if x and z starting coordinate is not same
	public static int initialGlobalIndexZ = 9003;

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
