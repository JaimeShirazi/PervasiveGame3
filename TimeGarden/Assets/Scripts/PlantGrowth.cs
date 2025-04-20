using UnityEngine;
using System.Collections.Generic;

public class PlantGrowth : MonoBehaviour
{
    public enum GrowthResponse
    {
        Growing, Stopped, Dead, Unborn
    }
    public abstract class BasePlantSegment
    {
        public abstract Vector2 Position { get; }
        public abstract float GrowthRate { get; }
        public abstract int SegmentEnergy { get; }
        protected abstract float GrowthAngle { get; }

        /// <returns>Whether or not this segment is still growing.</returns>
        public abstract GrowthResponse Grow(float amount);
        /// <returns>(angle of main child, angle of offshoot child)</returns>
        public (float, float) GetChildAngles()
        {
            float sunAngle = TimeUtils.GetPlantAngle();

            float mainAngle = Mathf.Clamp(
                Mathf.Lerp(GrowthAngle, sunAngle, 0.2f),
                -Mathf.PI / 2,
                Mathf.PI / 2);

            float offshootAngle = GrowthAngle + (sunAngle - GrowthAngle) * 0.5f + Random.value switch
            {
                > 0.5f => Mathf.PI / 2f,
                _ => - Mathf.PI / 2f
            };

            return (mainAngle, offshootAngle);
        }
    }
    public class PlantSegment : BasePlantSegment
    {
        public override Vector2 Position => position;
        public override float GrowthRate => growthRate;
        public override int SegmentEnergy => segmentEnergy;
        protected override float GrowthAngle => growthAngle;
        public BasePlantSegment Previous => previous;
        private BasePlantSegment previous;
        private Vector2 position;
        private float growthRate;
        protected int segmentEnergy;
        private float growthAngle;
        private Vector2 dir;

        /// <param name="AngleWeighting">The "direction the growth is moving" more or less. Measured in radians, with 0 being directly up. AngleWeighting will sometimes be 90 deg off from what you'd expect, this is done when a branch is chosen to be an offshoot.</param>
        public PlantSegment(BasePlantSegment previous, float AngleWeighting)
        {
            this.previous = previous;
            position = previous.Position;
            growthRate = previous.GrowthRate;
            segmentEnergy = previous.SegmentEnergy - 1;

            // ---- Setup growth angle ----
            float randomOffset = Random.Range(-0.4f, 0.4f) / Mathf.Max(segmentEnergy, 1); //Wobbles a bit more later on in growth.
            randomOffset *= 3f;
            growthAngle = AngleWeighting + randomOffset;
            dir = new Vector2(Mathf.Sin(growthAngle), Mathf.Cos(growthAngle));
        }
        
        public override GrowthResponse Grow(float amount)
        {
            if (segmentEnergy <= 0) return GrowthResponse.Dead;
            position += dir * growthRate * amount;

            float magnitude = (position - previous.Position).magnitude;

            if (amount > 0)
            {
                if (magnitude > 0.3f) return GrowthResponse.Stopped;
                else return GrowthResponse.Growing;
            }
            else
            {
                if (Vector2.Dot(dir, position - previous.Position) < 0) return GrowthResponse.Unborn;
                else return GrowthResponse.Growing;
            }
        }
    }
    public class OffshootPlantSegment : PlantSegment
    {
        public struct OffshootPositionData
        {
            public Vector2 Before;
            public Vector2 From;
            public Vector2 To;
        }
        public OffshootPositionData GetOffshootData()
        {
            if (Previous is PlantSegment plantSegment)
            {
                return new OffshootPositionData
                {
                    Before = plantSegment.Previous.Position,
                    From = plantSegment.Position,
                    To = Position
                };
            }
            else
            {
                return new OffshootPositionData
                {
                    Before = Previous.Position,
                    From = Vector2.Lerp(Previous.Position, Position, 0.5f),
                    To = Position
                };
            }
        }
        /// <param name="AngleWeighting">The "direction the growth is moving" more or less. Measured in radians, with 0 being directly up. AngleWeighting will sometimes be 90 deg off from what you'd expect, this is done when a branch is chosen to be an offshoot.</param>
        public OffshootPlantSegment(BasePlantSegment previous, float AngleWeighting) : base(previous, AngleWeighting)
        {
            segmentEnergy = 1;
        }
    }
    public class RootPlantSegment : BasePlantSegment
    {
        public override Vector2 Position => position;
        public override float GrowthRate => growthRate;
        public override int SegmentEnergy => segmentEnergy;
        protected override float GrowthAngle => 0;
        private Vector2 position;
        private float growthRate;
        private int segmentEnergy;
        public RootPlantSegment(Vector2 position, float growthRate, int segmentEnergy)
        {
            this.position = position;
            this.growthRate = growthRate;
            this.segmentEnergy = segmentEnergy + 1;
        }
        public override GrowthResponse Grow(float _) => GrowthResponse.Stopped;

    }

    public bool CreatedSeeds => createdSeeds;
    public const float MAX_HEALTH = 6f;
    public float Health { get; private set; } = MAX_HEALTH;

    private List<BasePlantSegment> segments;
    private List<OffshootPlantSegment> offshoots = new();
    private HashSet<OffshootPlantSegment> growingOffshoots = new();

    private bool createdSeeds;

    public Vector2[] GetMainPlantPoints()
    {
        Vector2[] points = new Vector2[segments.Count];
        for (int i = 0; i < segments.Count; i++)
        {
            points[i] = segments[i].Position;
        }
        return points;
    }
    public OffshootPlantSegment.OffshootPositionData[] GetOffshoots()
    {
        OffshootPlantSegment.OffshootPositionData[] offshootsData = new OffshootPlantSegment.OffshootPositionData[offshoots.Count];
        for (int i = 0;i < offshootsData.Length; i++)
        {
            offshootsData[i] = offshoots[i].GetOffshootData();
        }
        return offshootsData;
    }

    void Awake()
    {
        segments = new()
        {
            new RootPlantSegment(Vector2.zero, 0.5f, 16)
        };
    }
    private void OnEnable()
    {
        InputHandler.OnTimeUpdate += OnTimeUpdate;
        MushroomLogic.OnMushroomUpdate += OnMushroomUpdate;
    }
    private void OnDisable()
    {
        InputHandler.OnTimeUpdate -= OnTimeUpdate;
        MushroomLogic.OnMushroomUpdate -= OnMushroomUpdate;
    }

    void OnTimeUpdate(float delta)
    {
        if (delta < 0)
        {
            GrowMainSegment(Mathf.Min(delta * 0.1f, 0));
        }
    }
    void OnMushroomUpdate(float xPos)
    {
        if (!createdSeeds) return;

        float weight = Mathf.InverseLerp(2f, 0.1f, Mathf.Abs(xPos - transform.position.x));

        Health -= weight * Time.deltaTime;
    }

    bool GrowMainSegment(float delta)
    {
        switch (segments[^1].Grow(delta))
        {
            case GrowthResponse.Growing:
                break;
            case GrowthResponse.Stopped:
                (float, float) angles = segments[^1].GetChildAngles();

                segments.Add(new PlantSegment(segments[^1], angles.Item1));

                if (segments.Count > 2)
                {
                    if (Random.value > 0.9f) //Check for making extra offshoot branch
                    {
                        offshoots.Add(new OffshootPlantSegment(segments[^2], angles.Item2));
                        growingOffshoots.Add(offshoots[^1]);
                    }
                }
                break;
            case GrowthResponse.Dead:
                return false;
            case GrowthResponse.Unborn:
                List<OffshootPlantSegment> offshootsFromSegment = new();
                foreach (OffshootPlantSegment segment in offshoots)
                {
                    if (segment.Previous == segments[^1])
                    {
                        offshootsFromSegment.Add(segment);
                    }
                }
                foreach (OffshootPlantSegment segment in offshootsFromSegment)
                {
                    offshoots.Remove(segment);
                    if (growingOffshoots.Contains(segment))
                    {
                        growingOffshoots.Remove(segment);
                    }
                }
                segments.Remove(segments[^1]);
                GrowMainSegment(-0.1f); //Make sure to reverse the previous segment by enough so that it doesn't grow back in one frame tick
                break;
        }
        return true;
    }
    public bool UpdatePlant()
    {
        if (!TimeUtils.ShouldGrowPlants()) return true;

        //Grow the main plant
        if (!GrowMainSegment(Time.deltaTime))
        {
            return false;
        }

        //Grow the offshoots
        foreach (OffshootPlantSegment offshoot in offshoots)
        {
            if (growingOffshoots.Contains(offshoot))
            {
                switch (offshoot.Grow(Time.deltaTime))
                {
                    case GrowthResponse.Growing:
                        break;
                    case GrowthResponse.Stopped:
                        growingOffshoots.Remove(offshoot);
                        break;
                    case GrowthResponse.Dead:
                        break;
                    case GrowthResponse.Unborn:
                        break;
                }
            }
        }

        return true;
    }
    public float[] CreateSeeds()
    {
        createdSeeds = true;
        transform.position -= Vector3.forward;
        float[] newSeeds = new float[offshoots.Count + 1];
        newSeeds[0] = segments[^1].Position.x + transform.position.x;
        for (int i = 0; i < offshoots.Count; i++)
        {
            newSeeds[i + 1] = offshoots[^1].Position.x + transform.position.x;
        }
        return newSeeds;
    }
}
