using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidMedium : MonoBehaviour
{
    [Header("Fluid Medium Properties")]
    [Range(0, 1)] public float collisionDamping = 0.95f;
    public float targetDensity;
    public float pressureMultiplier;
    public float nearPressureMultiplier;
    public float viscosityStrength;

    [Header("References")]
    public ParticleSpawner spawner;
    public ParticleDisplay2D display;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
