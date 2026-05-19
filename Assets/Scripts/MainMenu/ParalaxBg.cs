using UnityEngine;

public class ParalaxBg : MonoBehaviour
{
    [SerializeField] private float parallaxEffectMultiplier; 
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        float parallaxX = (mousePos.x - 0.5f) * parallaxEffectMultiplier;
        float parallaxY = (mousePos.y - 0.5f) * parallaxEffectMultiplier;

        transform.position = new Vector3(startPos.x + parallaxX, startPos.y + parallaxY, transform.position.z);
    }
}
