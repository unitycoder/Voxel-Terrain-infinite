using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;
// Tagging component for use with the LocalNavMeshBuilder
// Supports mesh-filter and terrain - can be extended to physics and/or primitives
[DefaultExecutionOrder(-200)]
public class NavMeshSourceTag : MonoBehaviour
{
    // Global containers for all active mesh/terrain tags
    public static List<MeshFilter> m_Meshes = new List<MeshFilter>();
    public static bool Changed;
    public NavMeshBuildSource Source;
    bool hasSource;
    public Mesh mesh;
    public Transform myTransform;
    public int area = 0;
    public bool isNormal;
    public void Awake()
    {
        if (isNormal)
        {
            myTransform = transform;
            if(mesh==null)
            mesh = GetComponent<MeshFilter>().sharedMesh;
            hasSource = true;
            Make();
        }
    }
    public void Make()
    {

        StartCoroutine(AddMesh());

    }

        public IEnumerator AddMesh()
    {
        yield return new WaitForEndOfFrame();
        if (mesh != null && !hasSource)
        {

            Source = new NavMeshBuildSource();
            Source.shape = NavMeshBuildSourceShape.Mesh;
            yield return new WaitForEndOfFrame();
            hasSource = true;
            Source.sourceObject = mesh;
            Source.transform = myTransform.localToWorldMatrix;
            Source.area = area;
            yield return new WaitForEndOfFrame();
            LocalNavMeshBuilder.m_Sources.Add(Source);
        }
        else if (hasSource)
        {
            Source.sourceObject = mesh;
            Source.transform = myTransform.localToWorldMatrix;
            yield return new WaitForEndOfFrame();
            LocalNavMeshBuilder.m_Sources.Add(Source);
        }
        Changed = true;
    }
    public void UnMake()
    {
        if (hasSource)
        {
            LocalNavMeshBuilder.m_Sources.Remove(Source);
        }
 

    }


}
