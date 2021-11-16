using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelEngine;
using UnityEngine.AI;
public class VoxelGO : MonoBehaviour
{
    // Start is called before the first frame update
    public VoxelChunk chunk;
    public NavMeshSourceTag sourceTag;
    public MeshCollider m_MeshCollider;
    public GameObject myGameObject;
    public Transform m_Transform;
    public Vector3 m_pos;
    public Mesh mesh;
    public bool CanRender;
    public static int layer;
    public void Update()
    {
        if (CanRender)
        {
            Graphics.DrawMesh(mesh, m_pos, Quaternion.identity,VoxelTerrainEngine.Generator.m_material, layer, null, 0, null, true, true);
        }
    }
}
