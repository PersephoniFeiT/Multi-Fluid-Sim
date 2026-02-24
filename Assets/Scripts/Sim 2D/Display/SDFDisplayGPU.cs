using UnityEngine;

public class SDFDisplayGPU : FluidsDisplayGPU{
    public override void Init(Simulation2D sim)
    {
        material = new Material(shader);
        material.SetBuffer("Positions2D", sim.positionBuffer);
        material.SetBuffer("DensityData", sim.densityBuffer);
        material.SetBuffer("FluidMediaIndeces", sim.fluidMediaIndeces);
        material.SetBuffer("FluidMediaProfiles", sim.fluidMediaProfiles);
        material.SetFloat("_GlobalScale", scale);

        argsBuffer = ComputeHelper.CreateArgsBuffer(mesh, sim.positionBuffer.count);

        bounds = new Bounds(Vector3.zero, Vector3.one * 10000);
    }

    public override void UpdateSettings()
    {
        if (needsUpdate)
        {
            needsUpdate = false;
        }
    }
}


