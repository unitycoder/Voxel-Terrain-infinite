    using UnityEngine;
    using System.Collections.Generic;
    using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Rendering;
using Unity.Collections;
using System;
using UnityEngine.UIElements;
using System.ComponentModel;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

namespace VoxelEngine {
    public enum VoxelType {
        stone = 0,
        Dirt = 1,
        Sand = 2,
        grass = 3,
        Iron = 4,
        coal = 5,
        nitrate = 6,
        Copper = 7,
        bedRock = 8,

    }



    public class VoxelChunk {
        public static Vector3Int[] neighbors = new Vector3Int[8]
{    new Vector3Int(1,0,0)
    ,new Vector3Int(1,0,1)
    ,new Vector3Int(0,0,1)
    ,new Vector3Int(-1,0,0)
    ,new Vector3Int(-1,0,1)
    ,new Vector3Int(0,0,-1)
    ,new Vector3Int(1,0,-1)
    ,new Vector3Int(-1,0,-1)

};
        public VoxelGO m_voxelGo;
        public Mesh.MeshDataArray meshData;
        public bool isFull;
        public bool keepData;
        public int TreeCount;
        //public WaterChunk Wchunk;
        public bool isUpperLevel;
        public parentObject parentObj;
        public bool canGrass;
        Quaternion plantRotation;
        public List<Vector3> voxelPos = new List<Vector3>();
        public List<float> voxelList = new List<float>();
        public bool CanSync;
        public bool CanSave;
        public bool HasMesh;
        public List<Vector3> Pos;
        public List<Matrix4x4>[] grassPos;
        public List<int> GrassIndex;
        public List<float> GrassDist;
        public List<int> GrassBiome;
        public Voxels[,,] Voxels;
        public NativeList<Vector3> verts;
        public NativeList<Color> colorsM;
        public NativeList<Vector2> Uvs;
        public NativeList<int> tris;
        public NativeList<Vector3> normals;
        public Vector3 m_pos;
        public static VoxelTerrainEngine generator;
        public bool hascollider;
        public string FileName;
        public string SaveName;
        public bool HasGrass;
        public bool HasTrees;
        public int size;
        public List<VTree> treelist;
        public List<GameObject> treeGameObject;
        public bool ChunkHasWater = true;
        public static int totalweights;
        public GameObject myGameObject;
        public Transform myTransform;
        public int POSX;
        public int POSZ;
        public int lod;
        public int posY;
        public bool chunkProccessed;
        public VoxelChunk( int x, int y, int z, int ChunkLod, bool upper)
        {

            keepData = false;
            isUpperLevel = upper;

            plantRotation = Quaternion.identity;
            //As we need some extra data to smooth the voxels and create the normals we need a extra 5 voxels
            //+1 one to create a seamless mesh. +2 to create smoothed normals and +2 to smooth the voxels
            //This is a little unoptimsed as it means some data is being generated that has alread been generated in other voxel chunks
            //but it is simpler as we dont need to access the data in the other voxels. You could try and copy the data
            //needed in for the other voxel chunks as a optimisation step
            //set voxel size and data
            chunkProccessed = false;
            POSX = x;
            POSZ = z;
            lod = ChunkLod;
            int multi = VoxelTerrainEngine.TriSize[lod];
            /// width /= multi;
            //length /= multi;
            // height /= multi;
            //set position so that voxel position matches world position if translated
            posY = y;
            m_pos.Set(x - 2.0f * multi, y - 2.0f * multi, z - 2.0f * multi);

  

            
            //set file name for voxel saver
            if (isUpperLevel == false)
            {
                FileName = String.Concat( "VoxelChunk " + m_pos);

                SaveName = String.Concat("Saved Chunks" + generator.m_surfaceSeed);


            } else
            {
                FileName = String.Concat("VoxelChunk Upper" + m_pos);

                SaveName = String.Concat("Saved Chunks Upper" + generator.m_surfaceSeed);


            }

        }


        public void RegenChunk(int ChunkLod, int x, int y, int z, bool upper)
        {


            keepData = false;
            isUpperLevel = upper;
            CanSave = false;
            HasMesh = false;
            POSX = x;
            posY = y;
            POSZ = z;
            chunkProccessed = false;
            TreeCount = 0;
            //height /= VoxelTerrainEngine.TriSize[ChunkLod];
            //Voxels = new byte[width + 5, height + 5, width + 5];
            //Biomes = new byte[width + 5, width + 5];
            //material = new byte[width + 5, height + 5, width + 5];
            // if (posY == 0)
            // Water = new byte[width + 5, height + 5, width + 5];

            plantRotation = Quaternion.identity;
            //As we need some extra data to smooth the voxels and create the normals we need a extra 5 voxels
            //+1 one to create a seamless mesh. +2 to create smoothed normals and +2 to smooth the voxels
            //This is a little unoptimsed as it means some data is being generated that has alread been generated in other voxel chunks
            //but it is simpler as we dont need to access the data in the other voxels. You could try and copy the data
            //needed in for the other voxel chunks as a optimisation step
            //set voxel size and data



            lod = ChunkLod;
            int multi = VoxelTerrainEngine.TriSize[lod];


            m_pos.Set(x-2.0f * multi, y-2.0f * multi, z-2.0f * multi);

            //set file name for voxel saver
            if (isUpperLevel == false)
            {
                FileName = String.Concat("VoxelChunk " + m_pos);

                SaveName = String.Concat("Saved Chunks" + generator.m_surfaceSeed);

            }
            else
            {
                FileName = String.Concat("VoxelChunk Upper" + m_pos);

                SaveName = String.Concat("Saved Chunks Upper" + generator.m_surfaceSeed);


            }
        }

        public void DeleteTree(int Tree) {
            treelist.RemoveAt(Tree);

            Rigidbody body = treeGameObject[Tree].GetComponent<Rigidbody>();
            if (body == null)
                body = treeGameObject[Tree].AddComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeRotationY;
            body.mass = 500;
            body.angularDrag = 0.25f;
            body.isKinematic = false;
            body.centerOfMass = new Vector3(1, 2,  0.5f);

            //treeGameObject[Tree].GetComponent<TreeHealth>().TreeFall();
            treeGameObject.RemoveAt(Tree);
            CanSave = true;
            CanSync = true;
        }

        //some optimisations could be done like combining meshes
        ///method for rendering grass 
        public void RenderGrass()
        {


                //if we have grass render render it otherwise ignore
                if(grassPos!=null)
                for (int g = 0; g < grassPos.Length; g++)
                {
                    if (canGrass && grassPos[g].Count > 0 )
                    {

                        Graphics.DrawMeshInstanced(generator.MeshGrass[g], 0, generator.MatGrass[g], grassPos[g], null, UnityEngine.Rendering.ShadowCastingMode.Off, true, generator.GrassLayer);

                    }
                }
            if (GrassIndex != null)
                for (int i = 0; i < GrassIndex.Count; i++)
                {
                    if (canGrass && GrassDist[i] < generator.DetailDistance )
                    {
                        if (GrassBiome[i] == 0)
                            Graphics.DrawMesh(generator.GrassMesh[GrassIndex[i]], Pos[i], plantRotation, generator.GrassMat[GrassIndex[i]], generator.GrassLayer, null, 0,null,false,true);
                        else if (GrassBiome[i] == 1)
                            Graphics.DrawMesh(generator.GrassMeshForest[GrassIndex[i]], Pos[i], plantRotation, generator.GrassMatForest[GrassIndex[i]], generator.GrassLayer, null, 0, null, false, true);
                        else if (GrassBiome[i] == 2)
                            Graphics.DrawMesh(generator.GrassMeshTropics[GrassIndex[i]], Pos[i], plantRotation, generator.GrassMatTropics[GrassIndex[i]], generator.GrassLayer, null, 0, null, false, true);
                        else if (GrassBiome[i] == 3)
                            Graphics.DrawMesh(generator.GrassMeshSnow[GrassIndex[i]], Pos[i], plantRotation, generator.GrassMatSnow[GrassIndex[i]], generator.GrassLayer, null, 0, null, false, true);
                        else if (GrassBiome[i] == 4)
                            Graphics.DrawMesh(generator.GrassMeshDesert[GrassIndex[i]], Pos[i], plantRotation, generator.GrassMatDesert[GrassIndex[i]], generator.GrassLayer,null, 0, null, false, true);
                    }
                }
            }


        public bool CheckForVoxelEdits()
        {

            if (voxelPos.Count > 0 && Voxels!=null && chunkProccessed)
            {
                bool hasVox = false;
                for (int i = 0; i < voxelPos.Count; i++)
                {
                    SetVoxels(voxelPos[i], voxelList[i], out hasVox);
                }
                voxelPos.Clear();
                voxelList.Clear();
                return true;
            }else return false;
        }
        //this basically allows you set the value where 0 is full and 255 is empty
        public bool SetVoxels(Vector3 voxelpos, float value,out bool hasVoxels) {

            int Length = generator.m_voxelWidth + 5;
            if (voxelpos.y <= 5)
            {
                hasVoxels = false;
                return false;
            }
            if (voxelpos.y >= (VoxelTerrainEngine.TriSize[generator.maxLod]*generator.m_voxelWidth * 2)-5)
            {
                hasVoxels = false;
                return false;
            }
            if (lod > 0)
            {
                hasVoxels = false;
                return false;
            }
            hasVoxels = Voxels != null;
                voxelpos = (voxelpos-m_pos);
            //rounds off the vector3s to nearest integer as voxels are integer values
            int voxelpositionX = (int)voxelpos.x;

            int voxelpositionY = (int)voxelpos.y;

            int voxelpositionZ = (int)voxelpos.z;


            //neccesary checks to ensure we are not checking out of bounds voxels
            if (voxelpositionX < Length
             && voxelpositionZ < Length && voxelpositionX >= 0
             && voxelpositionZ >= 0 && voxelpositionY < Length
             && voxelpositionY >= 0)
            {
                if (chunkProccessed)
                {
                    if (Voxels != null)
                    {
                        float vox = Voxels[voxelpositionX, voxelpositionY, voxelpositionZ].VoxelValue;

                            vox += value;
                        if (vox > 120)
                            vox = 255;
                        Voxels[voxelpositionX, voxelpositionY, voxelpositionZ].VoxelValue = vox;
                        

                    }
                    else
                    {

                        voxelPos.Add(new Vector3(voxelpositionX, voxelpositionY, voxelpositionZ) + m_pos);
                        voxelList.Add(value);

                        keepData = true;
                        

                    }
                }
                else
                {
                    voxelPos.Add(new Vector3(voxelpositionX, voxelpositionY, voxelpositionZ) + m_pos);
                    voxelList.Add(value);


                }
                CanSave = true;
                CanSync = true;
                //tell the engine to regenerate the terrain based on new voxel data

                //send back to tell the engine its got a voxel at this coordinate


                return true;



            }
            //send back to tell the engine it hasnt got voxel at this coordinate
            else
            {
                hasVoxels = false;
                return false;
            }

        }

        //this basically allows you set the value where 0 is full and 255 is empty
        public float GetVoxelsValue(Vector3 voxelpos) {

            voxelpos -= m_pos;
            voxelpos = new Vector3(voxelpos.x / VoxelTerrainEngine.TriSize[lod], voxelpos.y / VoxelTerrainEngine.TriSize[lod], voxelpos.z / VoxelTerrainEngine.TriSize[lod]);

            int voxelpositionX = Mathf.RoundToInt(voxelpos.x);
            int voxelpositionY = Mathf.RoundToInt(voxelpos.y);
            int voxelpositionZ = Mathf.RoundToInt(voxelpos.z);

            int Length = generator.m_voxelWidth + 5;
            if (voxelpos.y <= 5)
            {
                return -1;
            }
            if (voxelpos.y >= (VoxelTerrainEngine.TriSize[generator.maxLod] * generator.m_voxelWidth * 2) - 5)
            {
                return -1;
            }
            //neccesary checks to ensure we are not checking out of bounds voxels
            if (Voxels!=null && voxelpositionX < Length
             && voxelpositionZ < Length && voxelpositionX >= 0
             && voxelpositionZ >= 0 && voxelpositionY < Length
             && voxelpositionY >= 0 )
            {

                float vox = Voxels[voxelpositionX, voxelpositionY, voxelpositionZ].VoxelValue;

                //tell the engine to regenerate the terrain based on new voxel data

                //send back to tell the engine its got a voxel at this coordinate
                return vox;


            }
            else return -1;

        }

        //this basically allows you set the value where 0 is full and 255 is empty
        public int GetBiome(Vector3 voxelpos) {

            if (m_voxelGo!=null)
                voxelpos = m_voxelGo.m_Transform.InverseTransformPoint(voxelpos);
            voxelpos = new Vector3(voxelpos.x / VoxelTerrainEngine.TriSize[lod], voxelpos.y, voxelpos.z / VoxelTerrainEngine.TriSize[lod]);

            //rounds off the vector3s to nearest integer as voxels are integer values
            int voxelpositionX = Mathf.RoundToInt(voxelpos.x);
            int voxelpositionY = Mathf.RoundToInt(voxelpos.y);
            int voxelpositionZ = Mathf.RoundToInt(voxelpos.z);
            int Length = generator.m_voxelWidth + 5;
            //neccesary checks to ensure we are not checking out of bounds voxels
            if (Voxels != null && voxelpositionX < Length && voxelpositionY < Length && voxelpositionY >=0 && voxelpositionZ < Length && voxelpositionX >= 0 && voxelpositionZ >= 0) {

                int vox = Voxels[voxelpositionX, voxelpositionY, voxelpositionZ].Biome;

                //tell the engine to regenerate the terrain based on new voxel data

                //send back to tell the engine its got a voxel at this coordinate
                return vox;


            }
            else return -1;

        }

        //cant remember what this does but it looks like it basically checks if theres a voxel there?
        public bool CheckVoxels(Vector3 chunkpos) {

            if (m_voxelGo != null && (m_voxelGo.m_Transform.position - chunkpos).magnitude <= 0.1f)
                return true;
            else return false;

        }

        public void AddPlant(Vector3 voxelPos, int id) {

            for (int i = 0; i < Pos.Count; i++) {
                if ((voxelPos - Pos[i]).magnitude <= 2) {
                    Pos.Add(voxelPos);
                    GrassDist.Add(0.0f);
                    GrassIndex.Add(id);
                    CanSave = true;
                    CanSync = true;
                    break;
                }
            }
        }

        public bool HasPlant(Vector3 voxelPos) {

            if (Pos!=null && Pos.Count > 0)
            {
                for (int i = 0; i < Pos.Count; i++)
                {
                    if ((voxelPos - Pos[i]).magnitude <= 1.5f)
                    {

                        return true;

                    }
                }
            }
            if(grassPos!=null)
                for (int g = 0; g < grassPos.Length; g++)
                    for (int i = 0; i < grassPos[g].Count; i++)
                {
                    if ((voxelPos - (Vector3)grassPos[g][i].GetColumn(3)).magnitude <= 1.5f)
                    {

                        return true;

                    }
                }
            
            return false;
        }
        public int DestroyPlant(Vector3 voxelPos) {
            for (int i = 0; i < Pos.Count; i++)
            {
                if ((voxelPos - Pos[i]).magnitude <= 1.5f)
                {
                    Pos.RemoveAt(i);
                    GrassDist.RemoveAt(i);
                    int t = GrassIndex[i];
                    GrassIndex.RemoveAt(i);
                    
                    CanSave = true;
                    CanSync = true;
                    return t;

                }
 

            }
            for(int g =0;g < grassPos.Length;g++)
            for (int i = 0; i < grassPos[g].Count; i++)
            {
                if ((voxelPos - (Vector3)grassPos[g][i].GetColumn(3)).magnitude <= 1.5f)
                {
                    grassPos[g].RemoveAt(i);

                        CanSave = true;
                    CanSync = true;
                    return 0;

                }
            }
            return -1;
        }
        public int findPlant(Vector3 voxelPos) {
            for (int i = 0; i < Pos.Count; i++)
            {
                if ((voxelPos - Pos[i]).magnitude <= 1)
                {
                    return i;
                }
            }
            for (int g = 0; g < grassPos.Length; g++)
                for (int i = 0; i < grassPos[g].Count; i++)
                {
                    if ((voxelPos - (Vector3)grassPos[g][i].GetColumn(3)).magnitude <= 1)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Destroy() {
            if (isUpperLevel == false)
            {
                canGrass = false;

                if (m_voxelGo != null )
                {
                    m_voxelGo.myGameObject.SetActive(false);
                    m_voxelGo.CanRender = false;
                    m_voxelGo.mesh.Clear(false);
                    m_voxelGo.sourceTag.UnMake();
                    m_voxelGo.chunk = null;
                    generator.GOChunkPool.Add(m_voxelGo);

                }
                HasMesh = false;
                m_voxelGo = null;
                HasGrass = false;
                HasTrees = false;
                if (GrassDist != null)
                {
                    GrassDist = null;
                    GrassIndex = null;
                    GrassBiome = null;
                    Pos = null;
                    for (int i = 0; i < grassPos.Length; i++)
                        grassPos[i] = null;
                }
                
                


            }
            if (isUpperLevel)
            {
                if (CanSave)
                    SaveVoxels();
                if (treeGameObject!=null)
                while (treeGameObject.Count > 0)
                {
                    GameObject T = treeGameObject[0];
                    treeGameObject.RemoveAt(0);
                    GameObject.DestroyImmediate(T);
                }
                treeGameObject = null;
                treelist = null;

                if(myGameObject != null)
                GameObject.DestroyImmediate(myGameObject);
            }

            if (verts.IsCreated)
                verts.Dispose();
            if (tris.IsCreated)
                tris.Dispose();
            if (colorsM.IsCreated)
                colorsM.Dispose();
            if (Uvs.IsCreated)
                Uvs.Dispose();
            if (normals.IsCreated)
                normals.Dispose();
            if (Voxels != null )
            {
                generator.arrays.Add(Voxels);
                Voxels = null;
            }
            }
        public void Clear()
        {


            if (verts.IsCreated)
                verts.Dispose();
            if (tris.IsCreated)
                tris.Dispose();
            if (colorsM.IsCreated)
                colorsM.Dispose();
            if (Uvs.IsCreated)
                Uvs.Dispose();
            if (normals.IsCreated)
                normals.Dispose();
            if (CanSave)
                SaveVoxels();
            HasGrass = false;
            canGrass = false;
            if (GrassDist != null)
            {
                GrassDist = null;
                GrassIndex = null;
                GrassBiome = null;
                Pos = null;
                for (int i = 0; i < grassPos.Length; i++)
                    grassPos[i] = null;
            }
            if (Voxels != null)
            {
                generator.arrays.Add(Voxels);
                Voxels = null;
            }
        }
        public void ClearData()
        {
            if (Voxels != null )
            {
                generator.arrays.Add(Voxels);
                Voxels = null;
            }
        }
        ///this is to find the type of voxel thats there eg: iron , oil or gold
        ///can have up to 8 types which will be in shader
        public int FindVoxelType(Vector3 VoxelPos) {
            VoxelPos -= m_pos;
            VoxelPos = new Vector3(VoxelPos.x / VoxelTerrainEngine.TriSize[lod], VoxelPos.y / VoxelTerrainEngine.TriSize[lod], VoxelPos.z / VoxelTerrainEngine.TriSize[lod]);

            int type = 0;
            int voxelpositionX = Mathf.RoundToInt(VoxelPos.x);
            int voxelpositionY = Mathf.RoundToInt(VoxelPos.y);
            int voxelpositionZ = Mathf.RoundToInt(VoxelPos.z);

            int Length = generator.m_voxelWidth + 5;
            if (VoxelPos.y <= 5)
            {
                return -1;
            }
            if (VoxelPos.y >= (VoxelTerrainEngine.TriSize[generator.maxLod] * generator.m_voxelWidth * 2) - 5)
            {
                return -1;
            }
            //neccesary checks to ensure we are not checking out of bounds voxels
            if (Voxels!=null && voxelpositionX < Length
             && voxelpositionZ < Length && voxelpositionX >= 0
             && voxelpositionZ >= 0 && voxelpositionY < Length
             && voxelpositionY >= 0)
            {

                type = Voxels[voxelpositionX, voxelpositionY, voxelpositionZ].Material;

                return type;
            }
            else return -1;

        }

        ///this is to find the type of voxel thats there eg: iron , oil or gold
        ///can have up to 8 types which will be in shader
        ///still needs work id like to have separate array that holds the types in it but it uses to much memory
        ///still dont quite understand why it uses so much memory
        public void SetVoxelType(Vector3 VoxelPos, byte type) {
            VoxelPos -= m_pos;
            VoxelPos = new Vector3(VoxelPos.x / VoxelTerrainEngine.TriSize[lod], VoxelPos.y / VoxelTerrainEngine.TriSize[lod], VoxelPos.z / VoxelTerrainEngine.TriSize[lod]);

            int voxelpositionX = Mathf.RoundToInt(VoxelPos.x);
            int voxelpositionY = Mathf.RoundToInt(VoxelPos.y);
            int voxelpositionZ = Mathf.RoundToInt(VoxelPos.z);

            int Length = generator.m_voxelWidth + 5;
            bool can = true;
            if (VoxelPos.y <= 5)
            {
                can = false;
            }
            if (VoxelPos.y >= (VoxelTerrainEngine.TriSize[generator.maxLod] * generator.m_voxelWidth * 2) - 5)
            {
                can = false;
            }
            //neccesary checks to ensure we are not checking out of bounds voxels
            if (can && Voxels != null && voxelpositionX < Length
             && voxelpositionZ < Length && voxelpositionX >= 0
             && voxelpositionZ >= 0 && voxelpositionY < Length
             && voxelpositionY >= 0) {
                Voxels[voxelpositionX, voxelpositionY, voxelpositionZ].Material = type;
                CanSave = true;
                CanSync = true;
            }
        }
        //threaded mesh creation 
        // basically just creates the triangles and vertices on a thread then adds them to a mesh
        //in the main thread
        public bool Createvoxels() {
            if (Voxels == null)
                Voxels = generator.RequestArray3d(generator.m_voxelWidth + 5);
            if (File.Exists(SaveName + "/" + FileName + "/" + FileName + "Tree" + ".dat"))
                TreeCount = 1;
            else TreeCount = 0;

            bool hasVoxels = false;
            if (lod == 0 && !File.Exists(SaveName + "/" + FileName + "/" + FileName + "dat" + ".dat") || lod > 0 && isUpperLevel ==false|| isUpperLevel && TreeCount ==0)
            {
                if (keepData)
                {
                    if (Voxels == null)
                        Voxels = generator.RequestArray3d(generator.m_voxelWidth + 5);
                }
                hasVoxels = generator.meshFactory.CreateVoxels( ref Voxels, m_pos, generator.noise, lod, this, isFull);
                if (keepData)
                    hasVoxels = true;
            }
            else if(isUpperLevel==false)
            {
                LoadVoxels();
                CanSave = true;
                hasVoxels = true;
                keepData = true;
            }
            else if(isUpperLevel && TreeCount > 0)
                hasVoxels = true;
            return hasVoxels;

        }

        public void CreateVertices() {
            try
            {




               
                //create the verts
                int multi = VoxelTerrainEngine.TriSize[lod];
                bool hasVerts = false;
                if (TreeCount == 0 && isUpperLevel || isUpperLevel == false)
                    hasVerts = MeshFactory.MarchingCubes.CreateVertices ( m_pos, 1, 1, lod, isUpperLevel,this);


                //store the size so as to avoid garbage creation
                if (verts.IsCreated)
                    size = verts.Length;
                else size = 0;
                //create normals


                if (size > 0)
                {
                    normals = new NativeList<Vector3>(size,Allocator.Persistent);
                    generator.meshFactory.CalculateNormals(Voxels, lod, size, normals, verts);
                }

                if (size > 0 && lod==0)
                {
                    if (HasGrass == false)
                        spawnstuff();
                }
                if (isUpperLevel && size > 0 && HasTrees == false  || isUpperLevel && TreeCount > 0)
                {
                    SpawnTrees();
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e.StackTrace + e.Message + e.Source);
            }

        }

  
        void SpawnTrees()
        {
            int MT = generator.MaxTrees;

            treelist = new List<VTree>(MT);
            treeGameObject = new List<GameObject>();

            if (TreeCount == 0 && generator.MaxTrees > 0)
            {

                int TLength = generator.TreesGrass.Length;
                int T = 0;



                if (size > 0)
                    for (int t = 0; t < size; t++)
                    {
                        Vector3 vert = verts[t] + m_pos;
                        float xyz = SimplexNoise.Generate(vert.z * 1.0f / 45, vert.y * 1.0f / 60, vert.x * 1.0f / 30) ;
                        float vf = SimplexNoise.Generate((vert.x + (xyz*300)) * 1.0f / generator.TreeFrequency, (vert.y +( xyz*120)) * 1.0f / generator.TreeFrequency, (vert.z + (xyz*220)) * 1.0f / generator.TreeFrequency);

                        if (vf < -0.5f)
                            vf = -0.5f;

                        if (vf > 0.5f)
                            vf = 0.5f;

                        vf += 0.5f;
                        if (xyz < -0.5f)
                            xyz = -0.5f;

                        if (xyz > 0.5f)
                            xyz = 0.5f;

                        xyz += 0.5f;

                        float B = SimplexNoise.Generate((vert.x + xyz) * 2.0f / generator.babelFrequency, (vert.y + xyz) * 2.0f / generator.babelFrequency, (vert.z + xyz) * 2.0f / generator.babelFrequency);

                        if (B < -0.5f)
                            B = -0.5f;

                        if (B > 0.5f)
                            B = 0.5f;

                        B += 0.5f;
                        int BF = 0;
                        BF = Mathf.Clamp((int)(B * verts.Length), 0, verts.Length - 1);
                        float Surf = Vector3.Dot(Vector3.up, normals[BF]);
                        bool toclose = false;
                        if (Surf <= 1.0f && Surf > 0.9f)
                        {
                            
                                Vector3 P = new Vector3((int)verts[BF].x, (int)verts[BF].y, (int)verts[BF].z)+ m_pos;
                            int x = (int)(verts[BF].x / VoxelTerrainEngine.TriSize[lod]);
                            int y = (int)(verts[BF].x / VoxelTerrainEngine.TriSize[lod]);
                            int z = (int)(verts[BF].x / VoxelTerrainEngine.TriSize[lod]);
                            if (Voxels[x, y, z].Material == (int)VoxelType.grass)
                            {
                                if (generator.babels.Count > 0)
                                {
                                    for (int i = 0; i < generator.babels.Count; i++)
                                    {


                                        if ((generator.babels[i] - P).magnitude < generator.babelDistance)
                                        {
                                            toclose = true;
                                            break;

                                        }
                                    }
                                    if (toclose == false) generator.babels.Add(P);
                                }
                                else generator.babels.Add(P);
                            }

                        }
                            if (HasTrees == false && T < MT)
                        {


                            int v = 0;
                            v = Mathf.Clamp((int)(vf * size), 0, size - 1);
                            float Sur = Vector3.Dot(Vector3.up, normals[v]);



                            int num = 0;
                            num = Mathf.RoundToInt(xyz * (TLength - 1));
                            Vector3 vec = new Vector3((int)verts[v].x, (int)verts[v].y, (int)verts[v].z);
                            Vector3 Tvec = new Vector3((int)verts[t].x, (int)verts[t].y, (int)verts[t].z);
                            bool isClose = false;
                            if (treelist.Count > 0 )
                            {
                               
                                    for (int i = 0; i < treelist.Count; i++)
                                    {

                                    if ((treelist[i].treePos - (Tvec + m_pos + Vector3.down)).magnitude < 15)
                                        {
                                            isClose = true;
                                            break;
                                        }

                                    }

                            }
                            for (int i = 0; i < generator.babels.Count; i++)
                            {

                                if ((generator.babels[i] - (vec+m_pos)).magnitude < 30)
                                {
                                    isClose = true;
                                    break;

                                }
                            }
                            if (vf > 0.0f && vf < 0.1f && isClose == false )
                            {
                                
                                    int x = (int)(Tvec.x / VoxelTerrainEngine.TriSize[lod]);
                                    int y = (int)(Tvec.y / VoxelTerrainEngine.TriSize[lod]);
                                    int z = (int)(Tvec.z / VoxelTerrainEngine.TriSize[lod]);
                                    if (Voxels[x, y, z].Material == (int)VoxelType.grass)
                                    {
                                        VTree tree;
                                        tree.treePos = (Tvec + m_pos) + Vector3.down;
                                        tree.treeindex = num;

                                        tree.treeBiome = Voxels[x, y, z].Biome;
                                        tree.treeHealth = 100;
                                        treelist.Add(tree);
                                        T++;
                                    }
                            }

                        }
                        else break;
                    }
            }
            
            else if (TreeCount > 0 && isUpperLevel)
            {

                using (FileStream file = File.Open(SaveName + "/" + FileName + "/" + FileName + "Tree" + ".dat",FileMode.Open))
                {
                    using (BinaryReader reader = new BinaryReader(file))
                    {
                        //hasgrass
                        int Count = reader.ReadInt32();
                        reader.ReadBoolean();

                        for (int i = 0; i < Count; i++)
                        {
                            VTree tree;
                            tree.treeindex = reader.ReadInt32();
                            Vector3 pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                            tree.treePos = pos;

                            tree.treeHealth = reader.ReadInt32();
                            tree.treeBiome = reader.ReadInt32();
                            treelist.Add(tree);

                        }
                    }
                }
            }

            HasTrees = true;
        }
        void spawnstuff() {
            int MG = generator.MaxGrass;

            int V = size;
            int weight;
            int G = 0;
            
            if (lod > 0)
                MG *=  VoxelTerrainEngine.TriSize[lod] * 4;
            grassPos = new List<Matrix4x4>[5];
            for (int i = 0; i < grassPos.Length; i++)
                grassPos[i] = new List<Matrix4x4>(MG);
            Pos = new List<Vector3>(generator.MaxPlants);
            GrassIndex = new List<int>(generator.MaxPlants);
            GrassBiome = new List<int>(generator.MaxPlants);
            GrassDist = new List<float>(generator.MaxPlants);

            if (!File.Exists(SaveName + "/" + FileName + "/" + FileName + "grass" + ".dat") && generator.MaxGrass>0) {


                float xyz;
                float vf;
                Vector3 vert = new Vector3(0,0,0);
                for (int i = 0; i < V; i++) {
     
                    vert.Set(verts[i].x+ m_pos.x, verts[i].y + m_pos.y, verts[i].z+ m_pos.z) ;
                    xyz = SimplexNoise.Generate(vert.z * 1.0f / generator.grassFrequency*5, vert.y * 1.0f / generator.grassFrequency*10, vert.x * 1.0f / generator.grassFrequency*8) * 20;
                    vf = SimplexNoise.Generate((vert.x+xyz) * 1.0f / generator.grassFrequency, (vert.y + xyz) * 1.0f / generator.grassFrequency, (vert.z + xyz) * 1.0f / generator.grassFrequency);


                    if (vf < -0.5f)
                        vf = -0.5f;

                    if (vf > 0.5f)
                        vf = 0.5f;

                    vf += 0.5f;



                    int v = 0;
                    v = Mathf.Clamp((int)(vf * V), 0, V - 1);
                    Vector3 pos = verts[v];
                    int x = Mathf.RoundToInt(pos.x / VoxelTerrainEngine.TriSize[lod]);
                    int y = Mathf.RoundToInt(pos.y / VoxelTerrainEngine.TriSize[lod]);
                    int z = Mathf.RoundToInt(pos.z / VoxelTerrainEngine.TriSize[lod]);

                    if (HasGrass == false  && Pos.Count < generator.MaxPlants)
                    {

                       

                        int GI = 0;

                        float TWeights = 0;
                        //basic weighting of grass which can be set up on Terrain script
                        //for each grass values range is 0.0f to 10.0f
                        weight = Mathf.Clamp((int)(vf * totalweights), 0, totalweights);
                        for (int t = 0; t < generator.GrassWeights.Length; t++) {
                 
                            if (TWeights <= weight) {

                                TWeights += generator.GrassWeights[t];

                                if (TWeights > weight) {
                                    GI = t;
                                    break; }

                            }
                        }
                        
                        
                        if (Voxels[x,y,z].Material==(int)VoxelType.grass)
                            if (Pos.Contains(pos + m_pos) == false && Uvs[v].x == 0 )
                            {
                                

                                int b = Voxels[x, y, z].Biome;
                                if (b != 5)
                                {
                                    if (!Pos.Contains(pos))
                                    {
                                        Pos.Add(pos + m_pos);
                                        GrassIndex.Add(GI);


                                        GrassBiome.Add(b);
                                        GrassDist.Add(0.0f);
                                    }
                                }

                                
                            }


                    }
                   

                    if (Voxels[x, y, z].Material == (int)VoxelType.grass)
                        if (G < MG && Uvs[v].x == 0 && Uvs[v].y != 5 && Uvs[v].y != 4 )
                        {


                            int b = Voxels[x, y, z].Biome;

                            if (b != 5)
                            {
                                Matrix4x4 m = Matrix4x4.TRS(pos + m_pos, Quaternion.identity, Vector3.one);

                                // GPos.Add(pos + m_pos);
                                if (!grassPos[b].Contains(m))
                                {
                                    grassPos[b].Add(m);
                                    G++;
                                }
                            }
                    }
                    if (Pos.Count >= generator.MaxPlants && G >= MG)
                        break;
                }
                    HasGrass = true;

            }
            else if (File.Exists(SaveName + "/" + FileName + "/" + FileName + "grass" + ".dat"))
            {
                using (FileStream file = File.Open(SaveName + "/" + FileName + "/" + FileName + "grass" + ".dat",FileMode.Open))
                {
                    using (BinaryReader reader = new BinaryReader(file))
                    {
                        //hasgrass
                        bool hasStuff = reader.ReadBoolean();
                          int GCount = reader.ReadInt32();
                        
                        for (int i = 0; i < GCount; i++)
                        {

                            GrassIndex.Add(reader.ReadInt32());

                            //Rot.Add(VoxelSaver.GetFloat("Rot"+i));
                            Vector3 pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                            pos -= m_pos;
                            int b = Voxels[(int)(pos.x / VoxelTerrainEngine.TriSize[lod]), (int)(pos.y / VoxelTerrainEngine.TriSize[lod]), (int)(pos.z / VoxelTerrainEngine.TriSize[lod])].Biome;

                            Pos.Add(pos + m_pos);

                            GrassBiome.Add(b);
                            GrassDist.Add(1000.0f);
                        }

                        for (int g = 0; g < grassPos.Length; g++)
                        {
                            int Count = reader.ReadInt32();
                            for (int i = 0; i < Count; i++)
                            {

                                    Vector3 pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                                    pos -= m_pos;
                                    int b = Voxels[(int)(pos.x / VoxelTerrainEngine.TriSize[lod]), (int)(pos.y / VoxelTerrainEngine.TriSize[lod]), (int)(pos.z / VoxelTerrainEngine.TriSize[lod])].Biome;
                                    grassPos[b].Add(Matrix4x4.TRS(pos + m_pos, Quaternion.identity, Vector3.one));
                            }
                        }
                    }
                }


                HasGrass = true;



            }


        }

        //save voxels this all works perfectly i dont think it needs much explaining only thing that could 
        //be done is some sort of compression although i couldnt find any compression methods in unity
        public void SaveVoxels()
        {
            //flatten array of 3d voxels
            if (lod == 0 && Voxels != null && CanSave)
            {
                //byte[] voxel = new byte[Voxels.GetLength(0) * Voxels.GetLength(1) * Voxels.GetLength(2)];
                //byte[] mats = new byte[Voxels.GetLength(0) * Voxels.GetLength(1) * Voxels.GetLength(2)];
                //byte[] bio = new byte[Voxels.GetLength(0) * Voxels.GetLength(1) * Voxels.GetLength(2)];
                if (!Directory.Exists(SaveName + "/" + FileName))

                    Directory.CreateDirectory(SaveName + "/" + FileName);
                using (FileStream file = File.Create(SaveName + "/" + FileName + "/" + FileName + "dat" + ".dat"))
                {
                    using (BinaryWriter writer = new BinaryWriter(file))
                    {
                        for (int x = 0; x < Voxels.GetLength(0); x++)
                        {

                            for (int y = 0; y < Voxels.GetLength(1); y++)
                            {

                                for (int z = 0; z < Voxels.GetLength(2); z++)
                                {

                                    writer.Write(Voxels[x, y, z].VoxelValue);
                                    writer.Write(Voxels[x, y, z].Material);
                                    writer.Write(Voxels[x, y, z].Biome);
                                    // File.Write(SaveName + "/" + FileName + "/" + FileName + "mat" + ".dat", Voxels[x, y, z].Material);
                                    //File.Write(SaveName + "/" + FileName + "/" + FileName + "biome" + ".dat", Voxels[x, y, z].Biome);
                                }
                                // voxel[x + y * Voxels.GetLength(0) + z * Voxels.GetLength(0) * Voxels.GetLength(1)] = (byte)Voxels[x, y, z].VoxelValue;
                                //mats[x + y * Voxels.GetLength(0) + z * Voxels.GetLength(0) * Voxels.GetLength(1)] = (byte)Voxels[x, y, z].Material;
                                // bio[x + y * Voxels.GetLength(0) + z * Voxels.GetLength(0) * Voxels.GetLength(1)] = (byte)Voxels[x,y, z].Biome;
                            }
                        }

                    }
                }
                using (FileStream file = File.Create(SaveName + "/" + FileName + "/" + FileName + "grass" + ".dat"))
                {
                    using (BinaryWriter writer = new BinaryWriter(file))
                    {
                        //hasgrass
                        writer.Write(true);
                        if (GrassIndex != null)
                            writer.Write(GrassIndex.Count);
                        else writer.Write(0);


                        if (GrassIndex != null && GrassIndex.Count > 0)
                        {
                            for (int i = 0; i < GrassIndex.Count; i++)
                            {
                                writer.Write(GrassIndex[i]);
                                writer.Write(Pos[i].x);
                                writer.Write(Pos[i].y);
                                writer.Write(Pos[i].z);


                            }

                        }
                        if (grassPos != null)
                            for (int g = 0; g < grassPos.Length; g++) {
                                writer.Write(grassPos[g].Count);
                                for (int i = 0; i < grassPos[g].Count; i++)
                                {
                                    if (grassPos[g].Count > 0)
                                    {
                                        writer.Write(grassPos[g][i].GetColumn(3).x);
                                        writer.Write(grassPos[g][i].GetColumn(3).y);
                                        writer.Write(grassPos[g][i].GetColumn(3).z);
                                    }
                                }
                            }
                    }
                }
            }
            if (isUpperLevel && CanSave)
            {
                if (!Directory.Exists(SaveName + "/" + FileName))

                    Directory.CreateDirectory(SaveName + "/" + FileName);
                using (FileStream file = File.Create(SaveName + "/" + FileName + "/" + FileName + "Tree" + ".dat"))
                {
                    using (BinaryWriter writer = new BinaryWriter(file))
                    {
                        //hasgrass
                        writer.Write(treelist.Count);
                        writer.Write(true);

                        for (int i = 0; i < treelist.Count; i++)
                        {
                            
                            writer.Write(treelist[i].treeindex);
                            
                            writer.Write(treelist[i].treePos.x);
                            writer.Write(treelist[i].treePos.y);
                            writer.Write(treelist[i].treePos.z);

                            writer.Write(treelist[i].treeHealth);

                            writer.Write(treelist[i].treeBiome);
                        }
                    }
                }
            }
        }
        //load the voxels if directory exists

        public void LoadVoxels()
        {
            using (FileStream file = File.Open(SaveName + "/" + FileName + "/" + FileName + "dat" + ".dat",FileMode.Open))
            {
                using (BinaryReader writer = new BinaryReader(file))
                {
                    for (int x = 0; x < Voxels.GetLength(0); x++)
                    {

                        for (int y = 0; y < Voxels.GetLength(1); y++)
                        {

                            for (int z = 0; z < Voxels.GetLength(2); z++)
                            {

                                Voxels[x, y, z].VoxelValue = writer.ReadSingle();
                                Voxels[x, y, z].Material = writer.ReadByte();
                                Voxels[x, y, z].Biome = writer.ReadByte();
                                // writer.Write(Voxels[x, y, z].Material);
                                //writer.Write(Voxels[x, y, z].Biome);
                                // File.Write(SaveName + "/" + FileName + "/" + FileName + "mat" + ".dat", Voxels[x, y, z].Material);
                                //File.Write(SaveName + "/" + FileName + "/" + FileName + "biome" + ".dat", Voxels[x, y, z].Biome);
                            }
                            // voxel[x + y * Voxels.GetLength(0) + z * Voxels.GetLength(0) * Voxels.GetLength(1)] = (byte)Voxels[x, y, z].VoxelValue;
                            //mats[x + y * Voxels.GetLength(0) + z * Voxels.GetLength(0) * Voxels.GetLength(1)] = (byte)Voxels[x, y, z].Material;
                            // bio[x + y * Voxels.GetLength(0) + z * Voxels.GetLength(0) * Voxels.GetLength(1)] = (byte)Voxels[x,y, z].Biome;
                        }
                    }
                }
            }
            CanSync = true;
        }

        //main thread mesh assigning 
        //assigns meshes vertices , triangles and colors to mesh 
        public void CreateMyMesh()
        {

            int T = tris.Length;
            var m = meshData[0];
            m.SetVertexBufferParams(size, new VertexAttributeDescriptor(VertexAttribute.Position, stream: 0), new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1), new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4, 2), new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 3));
            m.SetIndexBufferParams(T, IndexFormat.UInt32);


            NativeArray<Vector3> NVerts;
            NVerts = meshData[0].GetVertexData<Vector3>(0);

            NativeArray<Vector3> NNormals;
            NNormals = meshData[0].GetVertexData<Vector3>(1);

            NativeArray<Color> NColors;
            NColors = meshData[0].GetVertexData<Color>(2);

            NativeArray<Vector2> NUVs;
            NUVs = meshData[0].GetVertexData<Vector2>(3);




            NativeArray<int> NTris;
            NTris = m.GetIndexData<int>();

            int vCount = NVerts.Length;



            for (int i = 0; i < size; i++) {
                NVerts[i] = verts[i];
                NNormals[i] = normals[i];
                NColors[i] = colorsM[i];
                NUVs[i] = Uvs[i];

            }
            for (int i = 0; i < T; i++)
            {

                NTris[i] = tris[i];
            }
            var sm = new SubMeshDescriptor(0, T, MeshTopology.Triangles);
            sm.firstVertex = 0;
            sm.vertexCount = size;
           
            m.subMeshCount = 1;

            m.SetSubMesh(0, sm);
            NVerts.Dispose();
            NColors.Dispose();
            NUVs.Dispose();
            NNormals.Dispose();
            NTris.Dispose();

            verts.Dispose();
            tris.Dispose();
            normals.Dispose();
            colorsM.Dispose();
            Uvs.Dispose();
        }
        public void AllocateMesh()
        {
            meshData = Mesh.AllocateWritableMeshData(1);
        }
        public void CreateMesh()
        {
            
            if (isUpperLevel == false)
            {
                
  
                    //store in colors 

                    //mark mesh as dynamic not sure how well this works?
                    //mesh.MarkDynamic();


                if (size > 0)
                {
                    if (m_voxelGo == null)
                    {
                        m_voxelGo = generator.RequestVoxelGo();
                        m_voxelGo.chunk = this;


                    }
                    else m_voxelGo.sourceTag.UnMake();
                    // m_mesh.AddComponent<NavMeshSourceTag>();



                    //


                    m_voxelGo.m_Transform.localPosition = m_pos;

                    if (m_voxelGo.mesh == null)
                    {

                        m_voxelGo.mesh = new Mesh();
                        m_voxelGo.mesh.MarkDynamic();
                    }
                    else
                    {
                        m_voxelGo.mesh.Clear(false);


                    }
                    Mesh.ApplyAndDisposeWritableMeshData(meshData, m_voxelGo.mesh, MeshUpdateFlags.Default);
                    m_voxelGo.mesh.RecalculateBounds();
                    m_voxelGo.myGameObject.SetActive(true);
                    m_voxelGo.m_pos = m_pos;
                    m_voxelGo.CanRender = true;
                    m_voxelGo.sourceTag.mesh = m_voxelGo.mesh;
                    m_voxelGo.sourceTag.myTransform = m_voxelGo.m_Transform;

                    m_voxelGo.sourceTag.Make();
                    // mesh.SetVertices(verts);
                    //mesh.SetTriangles(tris, 0);
                    //mesh.SetNormals(normals);
                    //mesh.SetColors(colorsM);
                    // mesh.SetUVs(0, Uvs);




                    //mesh.SetColors(colorsM);
                    //mesh.SetUVs(0,Uvs);

                    // mesh.Optimize();
                    //mesh.MarkModified();
                    // m_voxelGo.m_meshfilter.mesh = m_voxelGo.mesh;
                    //col.sharedMesh = mesh;



                    if (size > 0)
                        HasMesh = true;

                    else HasMesh = false;
                    //mesh.UploadMeshData(false);
                    if (Pos!=null && Pos.Count > 0)
                        canGrass = true;





                    //set flag so engine knows it can draw mesh now



                    if (lod > 0)
                        Clear();
                }
            }

            if (isUpperLevel)
            {

                if (treelist.Count > 0 && treeGameObject.Count == 0)
                {

                    if (myGameObject == null)
                    {
                        myGameObject = new GameObject();
                        myTransform = myGameObject.transform;
                        myTransform.parent = generator.transform;
                        myTransform.position = m_pos;
                        myGameObject.tag = generator.tag;
                        myGameObject.layer = generator.gameObject.layer;
                        myTransform.parent = generator.transform;
                    }
                    
                    //add plant placer component to mesh object
                    for (int i = 0; i < treelist.Count; i++)
                    {

                        GameObject gameo = null;
                        if (treelist[i].treeBiome == 0)
                            gameo = GameObject.Instantiate(generator.TreesGrass[treelist[i].treeindex], treelist[i].treePos, Quaternion.identity);
                        else if (treelist[i].treeBiome == 1)
                            gameo = GameObject.Instantiate(generator.TreesForest[treelist[i].treeindex], treelist[i].treePos, Quaternion.identity);
                        else if (treelist[i].treeBiome == 2)
                            gameo = GameObject.Instantiate(generator.TreesTropics[treelist[i].treeindex], treelist[i].treePos, Quaternion.identity);
                        else if (treelist[i].treeBiome == 3)
                            gameo = GameObject.Instantiate(generator.TreesSnow[treelist[i].treeindex], treelist[i].treePos, Quaternion.identity);
                        else if (treelist[i].treeBiome == 4)
                            gameo = GameObject.Instantiate(generator.TreesDesert[treelist[i].treeindex], treelist[i].treePos, Quaternion.identity);
                        else if (treelist[i].treeBiome == 5)
                            gameo = GameObject.Instantiate(generator.lavaParticles[treelist[i].treeindex], treelist[i].treePos + Vector3.up, Quaternion.identity);

                        gameo.isStatic = false;
                        treeGameObject.Add(gameo);
                        gameo.transform.parent = myTransform;
                    }
                    if (treeGameObject.Count > 0)
                        HasTrees = true;
                }

                /* if(Voxels!=null)
                generator.arrays.Enqueue(Voxels);
                if (material != null)
                    generator.arrays.Enqueue(material);
                if (Biomes != null)
                    generator.arrays.Enqueue(Biomes);*/
                if (Voxels!=null)
                    generator.arrays.Add(Voxels);
                Voxels = null;
                if (verts.IsCreated)
                {
                    verts.Dispose();
                    tris.Dispose();
                    normals.Dispose();
                }

            }
            chunkProccessed = true;
        }


        //CreateMeshJob ****************************************
        
    }
}