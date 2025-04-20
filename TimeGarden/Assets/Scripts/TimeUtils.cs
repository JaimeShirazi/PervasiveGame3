using UnityEngine;

public static class TimeUtils
{
    //Returns the angle of the forward vector of the sun in degrees where 0 = sun pointing left (clockwise)
    public static float GetSkyAngle() => 292.5f - (InputHandler.Time * 15f);
    public static float GetSunAngle()
    {
        float time = InputHandler.Time;
        while (time < 0) time += 24;
        return Mathf.Lerp(-Mathf.PI / 2f, Mathf.PI / 2f, Mathf.InverseLerp(7.5f, 19.5f, time % 24));
    }
    public static float GetPlantAngle()
    {
        float skyAngle = GetSkyAngle();
        skyAngle += 90f;
        while (skyAngle < 0) skyAngle += 360;
        skyAngle %= 360f;

        skyAngle = Mathf.InverseLerp(90f, 270f, skyAngle);

        return Mathf.Lerp(Mathf.PI / 2f, -Mathf.PI / 2f, skyAngle);
    }
    public static bool ShouldGrowPlants()
    {
        float skyAngle = GetSkyAngle();
        while (skyAngle < 0) skyAngle += 360;
        skyAngle %= 360f;

        return skyAngle < 180;
    }
    public static bool ShouldGrowMushrooms() => !ShouldGrowPlants();
}
