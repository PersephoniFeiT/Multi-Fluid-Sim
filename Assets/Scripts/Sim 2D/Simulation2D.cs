using UnityEngine;
using Unity.Mathematics;

public class Simulation2D : MonoBehaviour
{
    public event System.Action SimulationStepCompleted;

    [Header("Simulation Settings")]
    public float timeScale = 1;
    public bool fixedTimeStep;
    public int iterationsPerFrame;
    public float gravity;
    //float collisionDamping = 0.95f;
    public float smoothingRadius = 2;
    //float targetDensity;
    //float pressureMultiplier;
    //float nearPressureMultiplier;
    //float viscosityStrength;
    public Vector2 boundsSize;
    public Vector2 obstacleSize;
    public Vector2 obstacleCentre;

    [Header("Interaction Settings")]
    public float interactionRadius;
    public float interactionStrength;

    [Header("References")]
    public ComputeShader compute;
    //public ParticleSpawner spawner;
    public ParticleDisplay2D display;
    [SerializeField]
    public FluidMedium[] fluidMedia;

    // Buffers
    public ComputeBuffer positionBuffer { get; private set; }
    public ComputeBuffer velocityBuffer { get; private set; }
    public ComputeBuffer densityBuffer { get; private set; }
    public ComputeBuffer fluidMediaIndeces;
    public ComputeBuffer fluidMediaProfiles;
    ComputeBuffer predictedPositionBuffer;
    ComputeBuffer spatialIndices;
    ComputeBuffer spatialOffsets;
    GPUSort gpuSort;

    // Kernel IDs
    const int externalForcesKernel = 0;
    const int spatialHashKernel = 1;
    const int densityKernel = 2;
    const int pressureKernel = 3;
    const int viscosityKernel = 4;
    const int updatePositionKernel = 5;

    // State
    bool isPaused;
    ParticleSpawner.ParticleSpawnData spawnData;
    bool pauseNextFrame;

    public int numParticles { get; private set; }


    void Start()
    {
        // Load settings from fluid medium
        //collisionDamping = fluidMedium.collisionDamping;
        //targetDensity = fluidMedium.targetDensity;
        //pressureMultiplier = fluidMedium.pressureMultiplier;
        //nearPressureMultiplier = fluidMedium.nearPressureMultiplier;
        //viscosityStrength = fluidMedium.viscosityStrength;

        
        //spawnData = fluidMedium.spawner.GetSpawnData();
        /*for(int i = 0; i < fluidMedia.Length; i++){
            ParticleSpawner.ParticleSpawnData newSpawnData = new ParticleSpawner.ParticleSpawnData(spawnData.positions.Length + fluidMedium[i].spawner.particleCount);
            spawnData.positions.CopyTo(newSpawnData.positions, 0);
            spawnData.velocities.CopyTo(newSpawnData.velocities, 0);
            fluidMedia[i].spawner.GetSpawnData().positions.CopyTo(newSpawnData.positions, spawnData.positions.Length);
            fluidMedia[i].spawner.GetSpawnData().velocities.CopyTo(newSpawnData.velocities, spawnData.velocities.Length);
            spawnData = newSpawnData;
        }*/

        //numParticles = 0;

        //for(int i = 0; i < fluidMedia.Lenght; i++)




        //numParticles = spawnData.positions.Length;


        Debug.Log("Controls: Space = Play/Pause, R = Reset, LMB = Attract, RMB = Repel");

        float deltaTime = 1 / 60f;
        Time.fixedDeltaTime = deltaTime;

        

        // Create buffers
        //positionBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        //predictedPositionBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        //velocityBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        //densityBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        //spatialIndices = ComputeHelper.CreateStructuredBuffer<uint3>(numParticles);
        //spatialOffsets = ComputeHelper.CreateStructuredBuffer<uint>(numParticles);
        //fluidMediaIndeces = ComputeHelper.CreateStructuredBuffer<uint>(numParticles);
        //fluidMediaProfiles = ComputeHelper.CreateStructuredBuffer<FluidMediumProfile>(fluidMedia.Length);

        // Set buffer data
        InitializeBufferData(fluidMedia);

        // Init compute
        ComputeHelper.SetBuffer(compute, positionBuffer, "Positions", externalForcesKernel, updatePositionKernel);
        ComputeHelper.SetBuffer(compute, predictedPositionBuffer, "PredictedPositions", externalForcesKernel, spatialHashKernel, densityKernel, pressureKernel, viscosityKernel);
        ComputeHelper.SetBuffer(compute, spatialIndices, "SpatialIndices", spatialHashKernel, densityKernel, pressureKernel, viscosityKernel);
        ComputeHelper.SetBuffer(compute, spatialOffsets, "SpatialOffsets", spatialHashKernel, densityKernel, pressureKernel, viscosityKernel);
        ComputeHelper.SetBuffer(compute, densityBuffer, "Densities", densityKernel, pressureKernel, viscosityKernel);
        ComputeHelper.SetBuffer(compute, velocityBuffer, "Velocities", externalForcesKernel, pressureKernel, viscosityKernel, updatePositionKernel);
        ComputeHelper.SetBuffer(compute, fluidMediaIndeces, "fluidMediaIndeces", pressureKernel, viscosityKernel, updatePositionKernel);
        ComputeHelper.SetBuffer(compute, fluidMediaProfiles, "fluidMediaProfiles", pressureKernel, viscosityKernel, updatePositionKernel);

        compute.SetInt("numParticles", numParticles);

        gpuSort = new();
        gpuSort.SetBuffers(spatialIndices, spatialOffsets);


        // Init display
        display.Init(this);
    }

    void FixedUpdate()
    {
        if (fixedTimeStep)
        {
            RunSimulationFrame(Time.fixedDeltaTime);
        }
    }

    void Update()
    {
        // Run simulation if not in fixed timestep mode
        // (skip running for first few frames as deltaTime can be disproportionaly large)
        if (!fixedTimeStep && Time.frameCount > 10)
        {
            RunSimulationFrame(Time.deltaTime);
        }

        if (pauseNextFrame)
        {
            isPaused = true;
            pauseNextFrame = false;
        }

        HandleInput();
    }

    void RunSimulationFrame(float frameTime)
    {
        if (!isPaused)
        {
            //obstacleCentre.x -= 0.01f;
            float timeStep = frameTime / iterationsPerFrame * timeScale;

            UpdateSettings(timeStep);

            for (int i = 0; i < iterationsPerFrame; i++)
            {
                RunSimulationStep();
                SimulationStepCompleted?.Invoke();
            }

        }
    }

    void RunSimulationStep()
    {
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: externalForcesKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: spatialHashKernel);
        gpuSort.SortAndCalculateOffsets();
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: densityKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: pressureKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: viscosityKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: updatePositionKernel);

    }

    void UpdateSettings(float deltaTime)
    {
        compute.SetFloat("deltaTime", deltaTime);
        compute.SetFloat("gravity", gravity);
        compute.SetFloat("collisionDamping", fluidMedium.collisionDamping);
        compute.SetFloat("smoothingRadius", smoothingRadius);
        compute.SetFloat("targetDensity", fluidMedium.targetDensity);
        compute.SetFloat("pressureMultiplier", fluidMedium.pressureMultiplier);
        compute.SetFloat("nearPressureMultiplier", fluidMedium.nearPressureMultiplier);
        compute.SetFloat("viscosityStrength", fluidMedium.viscosityStrength);
        compute.SetVector("boundsSize", boundsSize);
        compute.SetVector("obstacleSize", obstacleSize);
        compute.SetVector("obstacleCentre", obstacleCentre);

        compute.SetFloat("Poly6ScalingFactor", 4 / (Mathf.PI * Mathf.Pow(smoothingRadius, 8)));
        compute.SetFloat("SpikyPow3ScalingFactor", 10 / (Mathf.PI * Mathf.Pow(smoothingRadius, 5)));
        compute.SetFloat("SpikyPow2ScalingFactor", 6 / (Mathf.PI * Mathf.Pow(smoothingRadius, 4)));
        compute.SetFloat("SpikyPow3DerivativeScalingFactor", 30 / (Mathf.Pow(smoothingRadius, 5) * Mathf.PI));
        compute.SetFloat("SpikyPow2DerivativeScalingFactor", 12 / (Mathf.Pow(smoothingRadius, 4) * Mathf.PI));

        // Mouse interaction settings:
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isPullInteraction = Input.GetMouseButton(0);
        bool isPushInteraction = Input.GetMouseButton(1);
        float currInteractStrength = 0;
        if (isPushInteraction || isPullInteraction)
        {
            currInteractStrength = isPushInteraction ? -interactionStrength : interactionStrength;
        }

        compute.SetVector("interactionInputPoint", mousePos);
        compute.SetFloat("interactionInputStrength", currInteractStrength);
        compute.SetFloat("interactionInputRadius", interactionRadius);
    }

    void InitializeBufferData(FluidMedium[] media)
    {
        // Initialize numParticles and profiles data
        numParticles = 0;
        FluidMedium.FluidMediumProfile[] profilesData = new FluidMedium.FluidMediumProfile[media.Length];
        
        for(int i = 0; i < media.Length; i++){
            numParticles += media[i].spawner.particleCount;
            profilesData[i] = media[i].GetProfile();
        }
        
        // Initialize per particle data
        uint[] mediaIndecesData = new uint[numParticles];
        float2[] allPoints = new float2[numParticles];
        float2[] allVelocities = new float2[numParticles];

        int particleBufferIndex = 0;
        for(int i = 0; i < media.Length; i++){
            for(int j = 0; j < media[i].spawner.particleCount; j++){
                mediaIndecesData[particleBufferIndex + j] = (uint)i;
                allPoints[particleBufferIndex + j] = media[i].spawner.GetSpawnData().positions[j];
                allVelocities[particleBufferIndex + j] = media[i].spawner.GetSpawnData().velocities[j];
            }
            particleBufferIndex += media[i].spawner.particleCount;
        }

        
        // Create buffers
        fluidMediaProfiles = ComputeHelper.CreateStructuredBuffer<FluidMediumProfile>(fluidMedia.Length);
        positionBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        predictedPositionBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        velocityBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        densityBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        spatialIndices = ComputeHelper.CreateStructuredBuffer<uint3>(numParticles);
        spatialOffsets = ComputeHelper.CreateStructuredBuffer<uint>(numParticles);
        fluidMediaIndeces = ComputeHelper.CreateStructuredBuffer<uint>(numParticles);

        // Set buffer data
        fluidMediaProfiles.SetData(profilesData);

        fluidMediaIndeces.SetData(mediaIndecesData);
        positionBuffer.SetData(allPoints);
        predictedPositionBuffer.SetData(allPoints);
        velocityBuffer.SetData(allVelocities);

        /*float2[] allPoints = new float2[spawnData.positions.Length];
        System.Array.Copy(spawnData.positions, allPoints, spawnData.positions.Length);
    
        positionBuffer.SetData(allPoints);
        predictedPositionBuffer.SetData(allPoints);
        velocityBuffer.SetData(spawnData.velocities);

        for(int i = 0; i < allPoints.Length; i++){
            
        }

        for(int i = 0; i < media.lenght; i++){
            fluidMediaProfiles[i] = media[i].GetProfile();
        }*/
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPaused = !isPaused;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            isPaused = false;
            pauseNextFrame = true;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            isPaused = true;
            // Reset positions, the run single frame to get density etc (for debug purposes) and then reset positions again
            SetInitialBufferData(spawnData);
            RunSimulationStep();
            SetInitialBufferData(spawnData);
        }
    }


    void OnDestroy()
    {
        ComputeHelper.Release(positionBuffer, predictedPositionBuffer, velocityBuffer, densityBuffer, spatialIndices, spatialOffsets);
    }


    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.4f);
        Gizmos.DrawWireCube(Vector2.zero, boundsSize);
        Gizmos.DrawWireCube(obstacleCentre, obstacleSize);

        if (Application.isPlaying)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            bool isPullInteraction = Input.GetMouseButton(0);
            bool isPushInteraction = Input.GetMouseButton(1);
            bool isInteracting = isPullInteraction || isPushInteraction;
            if (isInteracting)
            {
                Gizmos.color = isPullInteraction ? Color.green : Color.red;
                Gizmos.DrawWireSphere(mousePos, interactionRadius);
            }
        }

    }
}
