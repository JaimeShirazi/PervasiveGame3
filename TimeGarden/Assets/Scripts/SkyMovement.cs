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
    }
}
