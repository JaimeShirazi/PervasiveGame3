using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GardenManager : MonoBehaviour
{
    [SerializeField] private float groundLevel = -3.6f;

    [SerializeField] private GameObject plantPrefab;
    [SerializeField] private GameObject mushroomPrefab;

    private List<PlantGrowth> plants = new();

    private List<PlantGrowth> pendingDead = new();

    private List<float[]> newSeedsQueue = new();

    private float cooldownTimer;
    private float mushroomCooldownTimer = 3;
    void Start()
    {
        newSeedsQueue = new() { new float[] { 0 } };
        SpawnNewPlants();
    }
    private void Update()
    {
        for (int i = 0; i < plants.Count; i++)
        {
            if (plants[i].Health <= 0)
            {
                pendingDead.Add(plants[i]);
            }
            else
            {
                if (!plants[i].UpdatePlant())
                {
                    if (!plants[i].CreatedSeeds)
                    {
                        newSeedsQueue.Add(plants[i].CreateSeeds());
                    }
                }
            }
        }

        for (int i = 0; i < pendingDead.Count; i++)
        {
            plants.Remove(pendingDead[i]);
            Destroy(pendingDead[i].gameObject);
        }
        pendingDead.Clear();

        cooldownTimer = Mathf.Max(0, cooldownTimer - Time.deltaTime);
        mushroomCooldownTimer = Mathf.Max(0, mushroomCooldownTimer - Time.deltaTime * 0.6f);

        SpawnNewPlants();
        SpawnNewMushroom();
    }
    public void SpawnNewPlants()
    {
        if (newSeedsQueue.Count <= 0) return;

        int mostOffshoots = newSeedsQueue[0].Length;
        for (int i = 0; i < newSeedsQueue.Count; i++)
        {
            if (newSeedsQueue[i].Length > mostOffshoots) mostOffshoots = newSeedsQueue[i].Length;
        }
        List<float> totalOffshoots = new();

        for (int i = 0; i < mostOffshoots; i++)
        {
            for (int j = 0; j < newSeedsQueue.Count; j++)
            {
                if (i <= newSeedsQueue[j].Length - 1)
                {
                    totalOffshoots.Add(newSeedsQueue[j][i]);
                }
            }
        }

        for (int i = 0; i < Mathf.Min(totalOffshoots.Count, Mathf.Max(4, newSeedsQueue.Count)); i++)
        {
            if (cooldownTimer <= 4)
            {
                plants.Insert(0, Instantiate(plantPrefab, new Vector3(totalOffshoots[i], groundLevel, 0), Quaternion.identity).GetComponent<PlantGrowth>());
                cooldownTimer++;
            }
        }

        newSeedsQueue.Clear();
    }
    public void SpawnNewMushroom()
    {
        if (!TimeUtils.ShouldGrowMushrooms()) return;

        if (mushroomCooldownTimer <= 0)
        {
            Instantiate(mushroomPrefab, new Vector3(Random.Range(-4.4f, 4.4f), groundLevel, 0), Quaternion.identity);
            mushroomCooldownTimer++;
        }
    }
}
