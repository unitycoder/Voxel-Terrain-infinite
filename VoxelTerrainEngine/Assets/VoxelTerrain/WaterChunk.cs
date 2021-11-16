using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using VoxelEngine;
using System.Linq;
//using UnityStandardAssets.Water;
using System.Xml.Schema;
using System.Threading;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEngine.AI;

public class WaterChunk
{
    public NavMeshSourceTag sourceTag;
    public NavMeshModifier mod;
    public bool meshDisposed;
    public Mesh.MeshDataArray outputMesh;
    public bool CanSave;
    public bool HasMesh;
    public bool chunkProccessed;
    public string FileName;
    public string SaveName;
    public Transform parent;
    public byte[,,] Water;
    public GameObject m_meshW;
    public List<Vector3> Waterverts = new List<Vector3>();
    public List<int> WaterTris = new List<int>();
    public List<Vector3> WaterNormals = new List<Vector3>();
    public Mesh meshW;
    MeshRenderer meshrenderW;
    MeshFilter meshfilterW;
    public Transform Wtransform;
    public bool HasWater;
    public Vector3 m_pos;
    public VoxelChunk chunk;
    public VoxelTerrainEngine generator;
    public int size;
    public bool keepData;
    public List<Vector3> voxelPos = new List<Vector3>();
    public List<byte> voxelList = new List<byte>();
    public int POSX;
    public int POSZ;
    public int lod;
    public int posY;
    public bool hasFull;
    public WaterChunk(Vector3 pos, int width, int x, int y, int z, int ChunkLod,VoxelChunk Cchunk)
    {
        meshDisposed = true;
        keepData = false;
        hasFull = false;
        generator = VoxelTerrainEngine.Generator;
        POSX = x;
        POSZ = z;
        lod = ChunkLod;
        int multi = VoxelTerrainEngine.TriSize[lod];
        /// width /= multi;
        //length /= multi;
        // height /= multi;
        if (Water == null)
            Water = generator.RequestArray(width + 5);
        //set position so that voxel position matches world position if translated
        posY = y;
        pos = new Vector3(pos.x, y, pos.z);
        m_pos = pos - (new Vector3(2f, 2f, 2f) * multi);
        chunk = Cchunk;

            FileName = "WaterChunk " + m_pos;

            SaveName = "Saved Chunks" + generator.m_surfaceSeed;


    }

    public void Initialize()
    {
        if (!meshDisposed) outputMesh.Dispose();
        outputMesh = Mesh.AllocateWritableMeshData(1);
        meshDisposed = false;
        

    }
    public void RegenChunk(int ChunkLod, int width, Vector3 pos, VoxelChunk Cchunk)
    {
        Waterverts.Clear();
        WaterTris.Clear();
        WaterNormals.Clear();
        keepData = false;
        hasFull = false;
        CanSave = false;
        chunkProccessed = false;
        POSX = (int)pos.x;
        posY = (int)pos.y;
        POSZ = (int)pos.z;

        if (Water == null)
            Water = generator.RequestArray(width + 5);

        lod = ChunkLod;
        int multi = VoxelTerrainEngine.TriSize[lod];
        int widthLength = width;


        m_pos = pos - (new Vector3(2f, 2f, 2f) * multi);
        chunk = Cchunk;

        FileName = "WaterChunk " + m_pos;

        SaveName = "Saved Chunks" + generator.m_surfaceSeed;
    }

    public bool FindWaterVoxels(Vector3 voxelpos)
    {
        bool isIn = true;
        bool isSet = false;
        int w = generator.m_voxelWidth + 5;
        if (voxelpos.y <= 5 && voxelpos.y >= (VoxelTerrainEngine.TriSize[generator.maxLod] * generator.m_voxelWidth * 2) - 5)
        {
            isIn = false;
        }
        voxelpos = voxelpos - m_pos;
        //rounds off the vector3s to nearest integer as voxels are integer values
        int voxelpositionX = Mathf.RoundToInt(voxelpos.x);

        int voxelpositionY = Mathf.RoundToInt(voxelpos.y);

        int voxelpositionZ = Mathf.RoundToInt(voxelpos.z);

        //neccesary checks to ensure we are not checking out of bounds voxels


        if (isIn && voxelpositionX < w && voxelpositionY < w
         && voxelpositionZ < w && voxelpositionX >= 0 && voxelpositionY >= 0
         && voxelpositionZ >= 0)
        {
            if (chunkProccessed)
            {

                if (Water != null)
                {

                    if (Water[voxelpositionX, voxelpositionY, voxelpositionZ] < 20)
                        isSet = true;

                }
                else if (hasFull)
                    isSet = true;

                //tell the engine to regenerate the terrain based on new voxel data

                //send back to tell the engine its got a voxel at this coordinate
            }
        }
        return isSet;
    }
    public bool SetWaterVoxels(Vector3 voxelpos, int value)
    {
        bool isIn = true;
        bool isSet = false;
        int w = generator.m_voxelWidth + 5;
        if (voxelpos.y <= 5 && voxelpos.y >= (VoxelTerrainEngine.TriSize[generator.maxLod] * generator.m_voxelWidth * 2) - 5)
        {
            isIn = false;
        }
        voxelpos = voxelpos - m_pos;
        //rounds off the vector3s to nearest integer as voxels are integer values
        int voxelpositionX = Mathf.RoundToInt(voxelpos.x);

        int voxelpositionY = Mathf.RoundToInt(voxelpos.y);

        int voxelpositionZ = Mathf.RoundToInt(voxelpos.z);

        //neccesary checks to ensure we are not checking out of bounds voxels


        if (isIn && voxelpositionX < w && voxelpositionY < w
         && voxelpositionZ < w && voxelpositionX >= 0 && voxelpositionY >= 0
         && voxelpositionZ >= 0)
        {
            if (chunkProccessed)
            {
                int vox;
                vox = value;
                vox = Mathf.Clamp(vox, 0, 255);
                if (Water != null)
                {
                    if (Water[voxelpositionX, voxelpositionY, voxelpositionZ] != vox)
                    {
                        Water[voxelpositionX, voxelpositionY, voxelpositionZ] = (byte)vox;

                        if (voxelpositionX + 1 < w)
                            Water[voxelpositionX + 1, voxelpositionY, voxelpositionZ] = (byte)vox;

                        if (voxelpositionX - 1 >= 0)
                            Water[voxelpositionX - 1, voxelpositionY, voxelpositionZ] = (byte)vox;

                        if (voxelpositionZ - 1 >= 0)
                            Water[voxelpositionX, voxelpositionY, voxelpositionZ - 1] = (byte)vox;

                        if (voxelpositionZ + 1 < w)
                            Water[voxelpositionX, voxelpositionY, voxelpositionZ + 1] = (byte)vox;

                        if (voxelpositionZ - 1 >= 0 && voxelpositionX - 1 > 0)
                            Water[voxelpositionX - 1, voxelpositionY, voxelpositionZ - 1] = (byte)vox;

                        if (voxelpositionX + 1 < w && voxelpositionZ + 1 < w)
                            Water[voxelpositionX + 1, voxelpositionY, voxelpositionZ + 1] = (byte)vox;

                        if (voxelpositionX - 1 >= 0 && voxelpositionZ + 1 < w)
                            Water[voxelpositionX - 1, voxelpositionY, voxelpositionZ + 1] = (byte)vox;

                        if (voxelpositionZ - 1 >= 0 && voxelpositionX + 1 < w)
                            Water[voxelpositionX + 1, voxelpositionY, voxelpositionZ - 1] = (byte)vox;
                        isSet = true;
                        hasFull = true;
                        CanSave = true;

                    }


                }
                else
                {
                    voxelPos.Add(new Vector3(voxelpositionX, voxelpositionY, voxelpositionZ) + m_pos);
                    voxelList.Add((byte)value);
                    keepData = true;
                    hasFull = true;
                    isSet = true;
                }
            }
            else
            {
                voxelPos.Add(new Vector3(voxelpositionX, voxelpositionY, voxelpositionZ) + m_pos);
                voxelList.Add((byte)value);
                keepData = true;
                hasFull = true;
                isSet = true;

            }
            HasWater = true;
            chunk.ChunkHasWater = true;
            //tell the engine to regenerate the terrain based on new voxel data

            //send back to tell the engine its got a voxel at this coordinate
        }
        return isSet;
    }
    public void CheckWater()
    {
        
        int w = generator.m_voxelWidth + 5;
        Vector3 P = new Vector3(0, 0, 0);
        List<Vector3> pos = new List<Vector3>();
        if (HasWater == true && chunk.Voxels != null && chunk.chunkProccessed)
        {
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < w; y++)
                {
                    for (int z = 0; z < w; z++)
                    {
   
                            float s = chunk.Voxels[x, y, z].VoxelValue;
                            
                                if (s > 100)
                                {
                                    P.Set(x + m_pos.x, y + m_pos.y, z + m_pos.z);
                                    if (y + 1 < w && Water[x, y + 1, z] <= 0 && Water[x, y, z] > 0)
                                    {



                                pos.Add(P);

                                    }
                                    else if (x - 1 > 0 && x + 1 < w && Water[x + 1, y, z] <= 0 && Water[x - 1, y, z] <= 0 && Water[x, y, z] > 0)
                                    {

                                pos.Add(P);
                            }
                                    else if (z - 1 > 0 && z + 1 < w && Water[x, y, z + 1] <= 0 && Water[x, y, z - 1] <= 0 && Water[x, y, z] > 0)
                                    {

                                pos.Add(P);
                            }

                                    //****

                                    else if (x - 1 > 0 && Water[x - 1, y, z] <= 0 && Water[x, y, z] > 0)
                                    {

                                pos.Add(P);
                            }
                                    else if (x + 1 < w && Water[x + 1, y, z] <= 0 && Water[x, y, z] > 0)
                                    {

                                pos.Add(P);
                            }
                                    else if (z - 1 > 0 && Water[x, y, z - 1] <= 0 && Water[x, y, z] > 0)
                                    {

                                pos.Add(P);
                            }
                                    else if (z + 1 < w && Water[x, y, z + 1] <= 0 && Water[x, y, z] > 0)
                                    {

                                        pos.Add(P);

                                    }
                                }

                    }
                }
            }
            //if(pos.Count>0)
            //generator.SetWater(pos,0);
        }

    }
    public void CreatewaterMesh()
    {
        size = Waterverts.Count;
       
        if (size > 0)
        {
            if (m_meshW == null)
            {

                    m_meshW = new GameObject("Water Mesh " + POSX
                     + " " + posY + " " + POSZ + " Lod" + lod);

                    // m_mesh.AddComponent<NavMeshSourceTag>();
                
                m_meshW.tag = "water";
                parent = generator.transform;
                m_meshW.layer = parent.gameObject.layer;
                m_meshW.isStatic = parent.gameObject.isStatic;
                Wtransform = m_meshW.transform;
                Wtransform.parent = parent;
                
                Wtransform.localScale = Vector3.one;
                
            }
            Wtransform.localPosition = m_pos;
            if (lod > 0)
                Clear();


            if (meshW == null)
            {
                meshW = new Mesh();
                meshW.MarkDynamic();


                if (!meshrenderW)
                    meshrenderW = m_meshW.AddComponent<MeshRenderer>();
                if (!meshfilterW)
                    meshfilterW = m_meshW.AddComponent<MeshFilter>();
                
                meshrenderW.sharedMaterial = generator.water;
            }
            else
            {
                meshfilterW.mesh = null;
                meshW.Clear();
            }



            Mesh.ApplyAndDisposeWritableMeshData(outputMesh, meshW, MeshUpdateFlags.Default);
            meshDisposed = true;
            meshW.Optimize();
            meshW.RecalculateBounds();
            //meshW.UploadMeshData(false);
            meshfilterW.sharedMesh = meshW;

            meshrenderW.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (m_meshW != null && m_meshW.activeSelf == false)
                m_meshW.SetActive(true);
            if (!sourceTag)
            {
                sourceTag = m_meshW.AddComponent<NavMeshSourceTag>();
                sourceTag.mesh = meshW;
                sourceTag.myTransform = Wtransform;
                sourceTag.area = 1;
                sourceTag.Make();
            }
            else
            {
                sourceTag.UnMake();
                sourceTag.mesh = meshW;
                sourceTag.myTransform = Wtransform;

                sourceTag.Make();
            }
        }
        else
        {

            if(hasFull==false)
            HasWater = false;
            if (lod > 0)
                Clear();
        }
        if (hasFull)

            HasWater = true;
        
        
        chunkProccessed = true;
    }
    public void LoadVoxels()
    {
        
        byte[] Wat = File.ReadAllBytes(SaveName + "/" + FileName + "/" + FileName + "Water" + ".dat");
            for (int x = 0; x < Water.GetLength(0); x++)
        {

            for (int y = 0; y < Water.GetLength(1); y++)
            {

                for (int z = 0; z < Water.GetLength(2); z++)
                {

                    Water[x, y, z] = Wat[x + y * Water.GetLength(0) + z * Water.GetLength(0) * Water.GetLength(1)];

                }
            }
        }
    }
    public void SaveVoxels()
    {
        //flatten array of 3d voxels
        if (lod == 0)
        {
            byte[] Wat = new byte[Water.GetLength(0) * Water.GetLength(1) * Water.GetLength(2)];
            for (int x = 0; x < Water.GetLength(0); x++)
            {

                for (int y = 0; y < Water.GetLength(1); y++)
                {

                    for (int z = 0; z < Water.GetLength(2); z++)
                    {

                        Wat[x + y * Water.GetLength(0) + z * Water.GetLength(0) * Water.GetLength(1)] = Water[x, y, z];
                    }
                }
            }
            //if no directory exists create one
            if (!Directory.Exists(SaveName + "/" + FileName))

                Directory.CreateDirectory(SaveName + "/" + FileName);

            File.WriteAllBytes(SaveName + "/" + FileName + "/" + FileName + "Water" + ".dat", Wat);





        }
    }
    public void CreateWaterVertices()
    {
        try
        {
            Waterverts.Clear();
            WaterTris.Clear();
            WaterNormals.Clear();
            int H = Water.GetLength(0);
            Vector3[,,] m_normals = new Vector3[H, H, H];
            generator.surfaceNets.CalculateVertexPositions(Water, Waterverts, WaterTris,m_pos, 1, 1, lod, false,chunk, m_normals);
            int WSize = Waterverts.Count;
            //generator.meshFactory.CalculateNormals(Water, WNormals, WSize, WVerts, lod);
            // = generator.meshFactory.CalculateNormals(Water, lod);
            int multi = VoxelTerrainEngine.TriSize[lod];
            //create colors
            for (int i = 0; i < WSize; i++)
            {

                Vector3 vert = Waterverts[i];
                vert = new Vector3(vert.x, vert.y, vert.z) / multi;
               // WaterNormals.Add(MeshFactory.TriLinearInterpNormal(vert, m_normals));
            }
            int T = WaterTris.Count;
            var data = outputMesh[0];

            data.SetVertexBufferParams(WSize, new VertexAttributeDescriptor(VertexAttribute.Position), new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1));
            data.SetIndexBufferParams(T, IndexFormat.UInt32);


            NativeArray<Vector3> NVerts;
            NVerts = data.GetVertexData<Vector3>(stream: 0);
            NativeArray<Vector3> NNormals;

            NNormals = data.GetVertexData<Vector3>(stream: 1);

            NativeArray<int> NTris;
            NTris = data.GetIndexData<int>();

            int vCount = NVerts.Length;


            for (int i = 0; i < vCount; i++)
            {
                NVerts[i] = Waterverts[i];
                NNormals[i] = WaterNormals[i];

            }
            for (int i = 0; i < T; i++)
            {
                NTris[i] = WaterTris[i];
            }
            var sm = new SubMeshDescriptor(0, T, MeshTopology.Triangles);
            sm.firstVertex = 0;
            sm.vertexCount = size;
            data.subMeshCount = 1;

            data.SetSubMesh(0, sm);
        }
        catch (Exception e)
        {
            Debug.LogError(e.StackTrace + e.Message + e.Source);
        }
    }
    public bool CheckForVoxelEdits()
    {
        if (voxelPos.Count > 0 && Water != null && chunkProccessed)
        {
            for (int i = 0; i < voxelPos.Count; i++)
            {
                SetWaterVoxels(voxelPos[i], voxelList[i]);
            }
            voxelPos.Clear();
            voxelList.Clear();
            return true;
        }
        else return false;
    }
    public bool CreateWaterVoxels()
    {
        int w = generator.m_voxelWidth+5;
        int waterheight = generator.SnowLine;
        bool CHasVox = chunk.Voxels != null;
        
        if (File.Exists(SaveName + "/" + FileName + "/" + FileName + "Water" + ".dat")) {
            if (Water == null)
            {
                Water = generator.RequestArray(w);
            }
            LoadVoxels();
            hasFull = true;
            keepData = true;
        }
        else if(chunk.ChunkHasWater==true)
        {
            if (Water == null && keepData)
            {
                Water = generator.RequestArray(w);
                hasFull = true;
            }

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < w; y++)
                {
                    for (int z = 0; z < w; z++)
                    {
                        int worldX = (int)((x * VoxelTerrainEngine.TriSize[lod]) + m_pos.x);
                        int worldY = (int)((y * VoxelTerrainEngine.TriSize[lod]) + m_pos.y);
                        int worldZ = (int)((z * VoxelTerrainEngine.TriSize[lod]) + m_pos.z);
                        if (CHasVox && chunk.Voxels[x, y, z].VoxelValue > 100 && worldY <= waterheight|| CHasVox == false && chunk.isFull == false && worldY <= waterheight)
                        {
                            if (lod > 0)
                            {
                                if (worldY < waterheight)
                                    Water[x, y, z] = 0;
                                else if (worldY == waterheight)
                                    Water[x, y, z] = (byte)(110);
                            }
                            else Water[x, y, z] = 0;
                            hasFull = true;
                        }
                        else Water[x, y, z] = 255;
                    }
                }
            }
            
        }

        return hasFull;
    }
    public void Clear()
    {
        if (keepData == false)
        {
            Water = null;
        }
        Waterverts.Clear();
        WaterTris.Clear();
        WaterNormals.Clear();
    }
    public void Destroy()
    {


        chunkProccessed = false;
        if (m_meshW != null)
        {
            //generator.waterChunks.Remove(this);
            
            m_meshW.SetActive(false);

        }
        if (!meshDisposed)
            outputMesh.Dispose();
        meshDisposed = true;
        if (meshW != null)
            meshW.Clear();
        if (CanSave)
            SaveVoxels();
            Waterverts.Clear();
    WaterTris.Clear();
    WaterNormals.Clear();
 
        Water = null;
        if (meshW != null)
            GameObject.Destroy(meshW);
    }

}
