using System.Collections.Generic;
using UnityEngine;

public class PlantVisual : MonoBehaviour
{
    [SerializeField] private PlantGrowth plant;
    [SerializeField] private LineRenderer line;
    [SerializeField] private GameObject linePrefab;

    private List<LineRenderer> offshootLines = new();
    private bool alive = true;
    public void SetAliveState(bool state)
    {
        line.material.SetFloat("_Growing", state ? 1 : 0);
        for (int i = 0; i < offshootLines.Count; i++)
        {
            offshootLines[i].material.SetFloat("_Growing", state ? 1 : 0);
        }

        UpdatePositions();
        alive = state;
    }
    public void SetHealth(float value)
    {
        line.material.SetFloat("_Health", value);
        for (int i = 0; i < offshootLines.Count; i++)
        {
            offshootLines[i].material.SetFloat("_Health", value);
        }
    }
    void Update()
    {
        if (plant.CreatedSeeds == alive)
        {
            SetAliveState(!plant.CreatedSeeds);
        }

        if (!alive)
        {
            SetHealth(plant.Health / PlantGrowth.MAX_HEALTH);
            return;
        }

        UpdatePositions();
    }
    void UpdatePositions()
    {
        Vector2[] points = plant.GetMainPlantPoints();
        line.positionCount = points.Length;
        for (int i = 0; i < points.Length; i++)
        {
            line.SetPosition(i, new Vector3(points[i].x, points[i].y, 0));
        }

        PlantGrowth.OffshootPlantSegment.OffshootPositionData[] data = plant.GetOffshoots();
        while (offshootLines.Count < data.Length)
        {
            GameObject newLine = Instantiate(linePrefab, transform);
            offshootLines.Add(newLine.GetComponent<LineRenderer>());
        }
        for (int i = offshootLines.Count - 1; i > data.Length - 1; i--)
        {
            Destroy(offshootLines[i]);
            offshootLines.RemoveAt(i);
        }
        for (int i = 0; i < offshootLines.Count; i++)
        {
            offshootLines[i].positionCount = 3;
            offshootLines[i].SetPositions(new Vector3[]
            {
                data[i].Before,
                data[i].From,
                data[i].To,
            });
        }
    }
}
