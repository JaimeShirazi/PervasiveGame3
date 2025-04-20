using System;
using UnityEngine;

public class MushroomLogic : MonoBehaviour
{
    public static event Action<float> OnMushroomUpdate = (_) => { };

    private float health = 3f;
    void Update()
    {
        if (!TimeUtils.ShouldGrowMushrooms()) health -= Time.deltaTime;

        OnMushroomUpdate.Invoke(transform.position.x);

        if (health < 0) Destroy(gameObject);
    }
}
