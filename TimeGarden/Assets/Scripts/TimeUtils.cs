using UnityEngine;

public static class TimeUtils
{
    //Returns the angle of the forward vector of the sun in degrees where 0 = sun pointing left (clockwise)
    public static float GetSkyAngle()
    {
        float angle = 292.5f - (InputHandler.Time * 15f);
        while (angle < 0)
        {
            angle += 360f;
        }
        return angle % 360f;
    }
    public static float GetSunAngle()
    {
        float time = InputHandler.Time;
        while (time < 0) time += 24;
        return Mathf.Lerp(-Mathf.PI / 2f, Mathf.PI / 2f, Mathf.InverseLerp(7.5f, 19.5f, time % 24));
    }
}
