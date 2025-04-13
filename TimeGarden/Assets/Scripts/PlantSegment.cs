using UnityEngine;

public class PlantSegment : MonoBehaviour
{
    private float growthRate;
    private int segmentEnergy;
    private Vector2 startPos;
    private bool allowGrowth = false;

    public Vector2 endPoint;
    public GameObject childBranch;
    private GameObject parentBranch;

    private float sunAngle;
    private float growthAngle;
    private Vector2 plusVector;

    public bool test;

    void Awake()
    {
        startPos = transform.position;
        endPoint = startPos;

        Vector2 vectorDelta = (Vector2)SunMovement.instance.gameObject.transform.position - startPos;
        sunAngle = Mathf.Atan2(vectorDelta.x, vectorDelta.y);
    }

    //Method called by parent to setup the segment with necessary info. AngleWeighting is calculated by the parent as the "direction the growth is moving" more or less. Measured in radians, with 0 being directly up.
    //AngleWeighting will sometimes be 90 deg off from what you'd expect, this is done when a branch is chosen to be an offshoot.
    void Setup(int SegmentEnergy, float GrowthRate, float AngleWeighting, GameObject Parent)
    {
        growthRate = GrowthRate;
        segmentEnergy = SegmentEnergy;
        parentBranch = Parent;        

        // ---- Setup growth angle ----
        float randomOffset = Random.Range(-0.4f, 0.4f)/segmentEnergy; //Wobbles a bit more later on in growth.
        growthAngle = AngleWeighting + randomOffset;
        plusVector = new Vector2(Mathf.Sin(growthAngle), Mathf.Cos(growthAngle));

        allowGrowth = true;
    }

    void CreateNew(Vector2 initialPos, int newEnergy, float newGrowthRate)
    {
        //Create new segment.
    }

    // Update is called once per frame
    void Update()
    {
        //Testing
        if (test)
        {
            Setup(5, 1f, 0f, gameObject);
            test = false;
        }

        if (allowGrowth){
            endPoint += plusVector * growthRate * Time.deltaTime;
            transform.position = endPoint;
            if ((endPoint - startPos).magnitude > 0.5f)
            {
                allowGrowth = false;

                CreateNew(endPoint, segmentEnergy - 1, growthRate);
            }
        }


    }
}
