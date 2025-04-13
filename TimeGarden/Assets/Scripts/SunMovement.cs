using UnityEngine;
using UnityEngine.UIElements;

public class SunMovement : MonoBehaviour
{
    [SerializeField, Range(-Mathf.PI/2, Mathf.PI/2)] //Radians
    public float angle;
    private float initialY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        float x = initialY * Mathf.Tan(angle);
        transform.position = new Vector2(x, initialY);
    }
}
