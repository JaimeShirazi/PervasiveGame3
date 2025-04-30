using UnityEngine;

public class SkyMovement : MonoBehaviour
{
    [SerializeField] private Transform lightSource;
    private void OnEnable()
    {
        InputHandler.OnTimeUpdate += OnTimeUpdate;
    }
    private void OnDisable()
    {
        InputHandler.OnTimeUpdate -= OnTimeUpdate;
    }
    void OnTimeUpdate(float time)
    {
        lightSource.eulerAngles = new Vector3(TimeUtils.GetSkyAngle(), -90, -90);



        lightSource.position = new Vector3(Mathf.Cos(TimeUtils.GetSkyAngle() * Mathf.Deg2Rad), Mathf.Sin(TimeUtils.GetSkyAngle() * Mathf.Deg2Rad), 0);
    }
}
