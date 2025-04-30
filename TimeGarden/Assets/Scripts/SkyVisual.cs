using UnityEngine;

public class SkyVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer sky;
    void Update()
    {
        sky.material.SetFloat("_GameTime", InputHandler.Time);
    }
}
