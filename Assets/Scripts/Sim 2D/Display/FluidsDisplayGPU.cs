using UnityEngine;

public abstract class FluidsDisplayGPU : MonoBehaviour
{
    public Mesh mesh;
    public Shader shader;
    public float scale;
    //public Gradient colourMap;
    public int gradientResolution;

    protected Material material;
    protected ComputeBuffer argsBuffer;
    protected Bounds bounds;
    protected bool needsUpdate;

    public abstract void Init(Simulation2D sim);
    public abstract void UpdateSettings();

    public void LateUpdate()
    {
        if (shader != null)
        {
            UpdateSettings();
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
        }
    }

    void OnValidate()
    {
        needsUpdate = true;
    }

    void OnDestroy()
    {
        ComputeHelper.Release(argsBuffer);
        
    }    
}