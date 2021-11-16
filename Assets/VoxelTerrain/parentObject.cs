using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelEngine;
using VoxelEngine.DoubleKeyDictionary;
using System.Linq;
using System.IO;
using Unity.Collections;
using UnityEngine.Rendering;
using Unity.Jobs;

using System;
using System.Threading;
using UnityEditor;
using Unity.Collections.LowLevel.Unsafe;
namespace VoxelEngine
{

    public class parentObject
    {
        public static Vector3Int[] mask = new Vector3Int[8] {
        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 1, 0),
        new Vector3Int(1, 0, 1),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 1, 1)
    };
        public static Vector3[] neighbor = new Vector3[6] {
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1),
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, -1, 0),

    };
        public enum infrontOrBehind
        {
            infront = 0,
            behind = 1,
            none = 2,
        }
        int maxlod;
        public infrontOrBehind frontorback;
        public int Posx;
        public int Posy;
        public int Posz;
        Vector3Int ParentPos;
        public parentObject Parent;
        public Dictionary<Vector3Int, VoxelChunk> chunks;
        public Dictionary<Vector3Int, VoxelChunk> Upperchunks;
        public Dictionary<Vector3Int, parentObject> childrenI;
        public List<VoxelChunk> chunksE = new List<VoxelChunk>(8);
        //public List<WaterChunk> waterChunks = new List<WaterChunk>();
        public List<VoxelChunk> UpperchunksE = new List<VoxelChunk>(8);
        public List<parentObject> children;
        public Queue<VoxelChunk> destroyChunk = new Queue<VoxelChunk>(8);
        public VoxelTerrainEngine generator;
        int Chunksize;
        public bool hasChunks;
        public bool Cangen;
        public bool isChecking;
        public int level;
        public bool isDestroyed;
        public parentObject(int xx, int yy, int zz, VoxelTerrainEngine gen, int size, int Lev)
        {
            level = Lev;
            generator = gen;
            children = new List<parentObject>();
            Chunksize = size;
            maxlod = gen.maxLod;
            chunks = new Dictionary<Vector3Int, VoxelChunk>(8);
            Upperchunks = new Dictionary<Vector3Int, VoxelChunk>(8);
            childrenI = new Dictionary<Vector3Int, parentObject>(8);
            Posx = xx;
            Posy = yy;
            Posz = zz;
            ParentPos = new Vector3Int(xx, yy, zz);
        }
        public void regen(int xx, int yy, int zz, VoxelTerrainEngine gen, int size, int Lev)
        {
            level = Lev;
            generator = gen;
            children.Clear();
            Chunksize = size;
            maxlod = gen.maxLod;
            chunks.Clear();
            Upperchunks.Clear();
            Posx = xx;
            Posy = yy;
            Posz = zz;
            ParentPos = new Vector3Int(xx, yy, zz);
        }
        public bool FindChunk(Vector3 chunkpos, out VoxelChunk chunk)
        {
            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].FindChunk(chunkpos, out chunk))
                    {
                        return true;

                    }
                }

            }
            else
            {
                if (level == 0)
                {
                    for (int i = 0; i < chunksE.Count; i++)
                    {
                        if ((chunksE[i].m_pos - chunkpos).magnitude < 1)
                        {
                            chunk = chunksE[i];
                            return true;
                        }
                    }
                }
                chunk = null;
                return false;

            }
            chunk = null;
            return false;

        }

        public bool FindChunkPos(Vector3 chunkpos, out VoxelChunk chunk)
        {

            int Length = generator.m_voxelWidth * VoxelTerrainEngine.TriSize[(level - 1)];
            if (UpperchunksE.Count > 0)
            {
                for (int i = 0; i < UpperchunksE.Count; i++)
                {
                    Vector3 p = (chunkpos - UpperchunksE[i].m_pos);
                    if (p.x < Length && p.z < Length && p.x >= 0 && p.z >= 0 && p.y < Length && p.y >= 0)
                    {
                        chunk = UpperchunksE[i];
                        return true;
                    }
                }
            }
            chunk = null;
            return false;
        }

        public bool FindTeeeChunk(Vector3 chunkpos, out VoxelChunk chunk)
        {

            if (UpperchunksE.Count > 0)
            {

                for (int i = 0; i < UpperchunksE.Count; i++)
                {
                    if ((UpperchunksE[i].m_pos - chunkpos).magnitude < 1)
                    {
                        chunk = UpperchunksE[i];
                        return true;
                    }
                }
                chunk = null;
                return false;

            }
            chunk = null;
            return false;

        }
        /*public bool SetWaterVoxel(List<Vector3> hitpoint, int value)
        {
            int C = waterChunks.Count;
            bool IsSet = false;
            List<WaterChunk> WChunks = new List<WaterChunk>();
            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].SetWaterVoxel(hitpoint, value))
                        IsSet = true;


                }

            }
            else
            {
                for (int c = 0; c < C; c++)
                {

                    if (waterChunks[c] != null && waterChunks[c].lod == generator.minLod)
                    {
                        for(int i =0;i < hitpoint.Count;i++)
                        if (waterChunks[c].SetWaterVoxels(hitpoint[i], value))
                        {
                            if (!WChunks.Contains(waterChunks[c])) 
                            WChunks.Add(waterChunks[c]);

                            IsSet = true;
                        }
                    }
                }
                for(int i = 0;i < WChunks.Count; i++)
                  {
                      if (!generator.IEWaterVertChunks.Contains(WChunks[i]) && WChunks[i].Water != null && WChunks[i].chunkProccessed)
                      {
                          generator.IEWaterVertChunks.Enqueue(WChunks[i]);
                      }
                      else if (!generator.IEVoxelWaterChunks.Contains(WChunks[i]) && WChunks[i].Water == null && WChunks[i].chunkProccessed)
                      {
                          generator.IEVoxelWaterChunks.Enqueue(WChunks[i]);
                      }
                  }

            }
            return IsSet;
        }*/

        public bool SetVoxel(Vector3 Pos, float value)
        {
            bool hasVoxels = false;
            bool hasSet = false;
            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].SetVoxel(Pos, value))
                        hasSet = true;


                }

            }
            else
            {
                if (level == 0)
                {
                    for (int i = 0; i < chunksE.Count; i++)
                    {
                        if (chunksE[i].SetVoxels(Pos, value, out hasVoxels))
                        {
                            hasSet = true;

                            if (hasVoxels)
                            {
    
                                generator.EVoxelVertChunks.Enqueue(chunksE[i]);
                            }
                            else if (hasVoxels == false && !generator.EVoxelChunks.Contains(chunksE[i]))
                                generator.EVoxelChunks.Enqueue(chunksE[i]);
                        }

                    }
                }
            }
            return hasSet;
        }

        public bool SetVoxel(List<Vector3> Pos, List<float> value)
        {
            bool hasVoxels = false;
            bool hasSet = false;
            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].SetVoxel(Pos, value))
                        hasSet = true;


                }

            }
            else
            {
                if (level == 0)
                {
                    List<VoxelChunk> Chunks = new List<VoxelChunk>();
                    List<bool> ChunksB = new List<bool>();
                    for (int i = 0; i < chunksE.Count; i++)
                    {
                        for (int t = 0; t < Pos.Count; t++)
                            if (chunksE[i].SetVoxels(Pos[t], value[t], out hasVoxels))
                            {
                                hasSet = true;

                                if (!Chunks.Contains(chunksE[i]))
                                {
                                    Chunks.Add(chunksE[i]);
                                    ChunksB.Add(hasVoxels);
                                }
                            }

                    }
                    for (int i = 0; i < Chunks.Count; i++)
                    {

                        if (ChunksB[i])
                            generator.EVoxelVertChunks.Enqueue(Chunks[i]);
                        else if (ChunksB[i] == false && !generator.EVoxelChunks.Contains(Chunks[i]))
                            generator.EVoxelChunks.Enqueue(Chunks[i]);
                    }
                }
            }
            return hasSet;
        }
        public float GetVoxelValue(Vector3 hitpoint)
        {
            float N = -1.0f;
            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].GetVoxelValue(hitpoint);
                }
            }
            else if (level == 0)
            {
                int C = chunksE.Count;

                for (int c = 0; c < C; c++)
                {

                    if (chunksE[c] != null && chunksE[c].lod < 1)
                    {
                        N = chunksE[c].GetVoxelsValue(hitpoint);
                        if (N > 0)
                        {
                            return N;
                        }
                    }
                }

            }
            return N;
        }
        public int FindVoxelType(Vector3 hitpoint)
        {
            int type = 0;

            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    type = children[i].FindVoxelType(hitpoint);
                    if (type <= 8 && type > -1)
                    {
                        return type;
                    }
                }
            }
            else if (level == 0)
            {
                int C = chunksE.Count;

                for (int c = 0; c < C; c++)
                {

                    if (chunksE[c] != null && chunksE[c].lod < 1)
                    {
                        type = chunksE[c].FindVoxelType(hitpoint);
                        if (type <= 8 && type > -1)
                        {
                            return type;
                        }

                    }

                }
            }
            return -1;

        }
        public void SetVoxelType(Vector3 hitpoint,byte type)
        {

            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                   children[i].SetVoxelType(hitpoint,type);

                }
            }
            else if (level == 0)
            {
                int C = chunksE.Count;

                for (int c = 0; c < C; c++)
                {

                    if (chunksE[c] != null && chunksE[c].lod < 1)
                    {
                        chunksE[c].SetVoxelType(hitpoint,type);
 

                    }

                }
            }

        }
        public int GetBiome(Vector3 hitpoint)
        {
            int N = -1;
            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].GetBiome(hitpoint);
                }
            }
            else if (level == 0)
            {
                int C = chunksE.Count;

                for (int c = 0; c < C; c++)
                {

                    if (chunksE[c] != null && chunksE[c].lod < 1)
                    {
                        N = chunksE[c].GetBiome(hitpoint);
                        if (N > 0)
                        {
                            return N;
                        }
                    }
                }

            }
            return N;
        }
        public void draw()
        {
            if (children.Count == 0)
            {
                int offset = Chunksize;

                if (level == 0)
                    Gizmos.color = new Color(255, 0, 0, 0.55f);
                else if (level == 1)
                    Gizmos.color = new Color(0, 255, 0, 0.5f);
                else if (level == 2)
                    Gizmos.color = new Color(0, 0, 255, 0.5f);
                else if (level == 3)
                    Gizmos.color = new Color(255, 0, 255, 0.5f);
                else if (level == 4)
                    Gizmos.color = new Color(255, 255, 255, 0.5f);
                else if (level == 5)
                    Gizmos.color = new Color(255, 125, 125, 1.0f);



                for (int i = 0; i < chunksE.Count; i++)
                {
                    if (generator.CanContinue)
                    {

                        Gizmos.DrawCube(chunksE[i].m_pos + (new Vector3(8,8,8) * VoxelTerrainEngine.TriSize[level]), new Vector3(16, 16, 16)*VoxelTerrainEngine.TriSize[level]);
                    }
                }
            }
            else if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].draw();
                }
            }
        }
        public void Destroy()
        {
            while (destroyChunk.Count > 0)
            {
                VoxelChunk chunk = destroyChunk.Dequeue();
                if (chunk != null && chunk.isUpperLevel == false)
                {
                    if (chunk.chunkProccessed) //&& chunk.Wchunk.chunkProccessed)
                    {
                        chunk.Destroy();
                        //chunk.Wchunk.Destroy();
                        //to be fixed
                        chunksE.Remove(chunk);
                        chunks.Remove(new Vector3Int(chunk.POSX, chunk.posY, chunk.POSZ));
                        //.Remove(chunk.Wchunk);
                        if (!generator.ChunkPool.Contains(chunk))
                            generator.ChunkPool.Add(chunk);
                        //if (!generator.WChunkPool.Contains(chunk.Wchunk))
                        //   generator.WChunkPool.Enqueue(chunk.Wchunk);
                    }

                    else if (chunk.chunkProccessed == false)// || chunk.Wchunk.chunkProccessed == false)
                    {
                        destroyChunk.Enqueue(chunk);
                        if (!generator.ToDestroy.Contains(this))
                            generator.ToDestroy.Enqueue(this);
                        break;
                    }

                }
                
                else if (chunk != null && chunk.isUpperLevel && chunk.chunkProccessed)
                {
                    chunk.Destroy();
                    Upperchunks.Remove(new Vector3Int(chunk.POSX, chunk.posY, chunk.POSZ));
                    UpperchunksE.Remove(chunk);
                    if (!generator.ChunkPool.Contains(chunk))
                        generator.ChunkPool.Add(chunk);
                }
               

            }



        }
        public void SaveVoxels()
        {
            if (UpperchunksE.Count > 0)
            {
                for (int c = 0; c < UpperchunksE.Count; c++)
                {

                    if (UpperchunksE[c] != null)
                    {
                        UpperchunksE[c].SaveVoxels();

                    }
                }
            }
            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].SaveVoxels();
                }
            }
            else if (level == 0)
            {
                int C = chunksE.Count;

                for (int c = 0; c < C; c++)
                {

                    if (chunksE[c] != null)
                    {
                        chunksE[c].SaveVoxels();

                    }
                }

            }
        }
        public bool CheckChildren()
        {
            int C = chunksE.Count;
            if (C > 0)
            {
                for (int i = 0; i < C; i++)
                {

                    if (chunksE[i].chunkProccessed == false)//|| chunksE[i].isDestroyed == false && chunksE[i].Wchunk.chunkProccessed == false)
                    {
                        return false;
                    }
                }
            }
            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].CheckChildren() == false)
                        return false;

                }

            }

            return true;

        }
        public bool CheckProccesed()
        {
            int C = chunksE.Count;
            if (C > 0)
            {
                for (int i = 0; i < C; i++)
                {

                    if (chunksE[i].chunkProccessed == false)//|| chunksE[i].isDestroyed == false && chunksE[i].Wchunk.chunkProccessed == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool CheckFrontOrback()
        {

            if (frontorback == infrontOrBehind.infront)
            {
               return CheckChildren();
            }
            else if (frontorback == infrontOrBehind.behind)
            {
              return  checkParent();
            }

            return true;

        }
        public bool checkParent()
        {
            if (chunksE.Count > 0)
            {

                return CheckProccesed();
            }
            if (Parent != null)
            {
                return Parent.checkParent();
            }

            return true;
        }
        public void DestroyChildren(bool destroyUpper)
        {
            
            if (destroyUpper)
            {
                for (int t = 0; t < UpperchunksE.Count; t++)
                    destroyChunk.Enqueue(UpperchunksE[t]);
                Upperchunks.Clear();
                UpperchunksE.Clear();
            }
            if (chunksE.Count > 0)
            {
                for (int i = 0; i < chunksE.Count; i++)
                {
                    chunksE[i].Clear();
                    if (!destroyChunk.Contains(chunksE[i]))
                        destroyChunk.Enqueue(chunksE[i]);
                }
                chunksE.Clear();
                chunks.Clear();
            }
            if (destroyChunk.Count > 0 && !generator.ToDestroy.Contains(this))
                generator.ToDestroy.Enqueue(this);
            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].frontorback = infrontOrBehind.behind;
                    children[i].DestroyChildren(destroyUpper);
                }
            }
        }
        public void checklod()
        {
            bool madeChunks = false;
            if (isChecking == false && isDestroyed == false)
            {
                isChecking = true;
                bool b = CheckChildren();
                if (b == true)
                    checkParent();
                if (b == true && children.Count > 0)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        if (children[i].isChecking)
                        {
                            b = false;
                            break;
                        }
                        else children[i].checklod();
                    }

                }

                Cangen = b;
                if (Cangen)
                {

                    Vector3 pos = new Vector3(Posx, Posy, Posz);
                    int nlod = 0;
                    int cSize = Chunksize / 2;
                    int[] Plod = new int[generator.Player.Count];
                    //gaps in terrain when paging
                    for (int p = 0; p < generator.PlayerGrass.Count; p++)
                    {
                        nlod = 0;
                        Vector3 Ppos = generator.PlayerGrass[p];


                        Ppos.Set(Ppos.x, Ppos.y, Ppos.z);
                        Ppos /= Chunksize;
                        Ppos.Set(Mathf.RoundToInt(Ppos.x), Mathf.RoundToInt(Ppos.y), Mathf.RoundToInt(Ppos.z));
                        Ppos *= Chunksize;
                        Ppos.Set(Ppos.x - cSize, Ppos.y - cSize, Ppos.z - cSize);

                        int distx = (int)Mathf.Abs(Ppos.x - pos.x);
                        int disty = (int)Mathf.Abs(Ppos.y - pos.y);
                        int distz = (int)Mathf.Abs(Ppos.z - pos.z);
                        int dist;

                        if (distx > distz)
                            dist = distx;
                        else
                            dist = distz;

                        if (disty > dist)
                            dist = disty;
                        for (int t = generator.minLod; t < VoxelTerrainEngine.TriSize.Length; t++)
                        {
                            if (dist > VoxelTerrainEngine.LodDist[t] * generator.TerrainLodBias)
                            {

                                nlod = t;

                            }
                        }
                        Plod[p] = nlod;
                    }
                    int lod = generator.maxLod;
                    for (int i = 0; i < Plod.Length; i++)
                        if (Plod[i] < lod)
                            lod = Plod[i];

                    nlod = lod;
                    if (nlod < level && children.Count == 0)
                    {
                        subdevide();
                    }
                    else if (nlod == level && children.Count > 0)
                    {

                        Combine();
                    }
                    if (level == maxlod && UpperchunksE.Count == 0 && generator.makeTerrain )
                    {
                        GenerateDChunks();
                      madeChunks = true;
                    }
                    if (chunksE.Count == 0 && nlod == level && children.Count == 0 && generator.makeTerrain)
                    {

                        GenerateChunks(level);
                        madeChunks = true;
                    }
                    if (madeChunks)
                        generator.VoxelChunks.Enqueue(this);
                }
            }
            isChecking = false;
        }

        public void CreateVoxels()
        {
            bool hasStuff = false;
            if (generator.CreateMeshes)
            {
                int C = chunksE.Count;

                for (int i = 0; i < C; i++)
                {

                    VoxelChunk chunk = chunksE[i];
       
                        if (chunk.Createvoxels())
                        {

                            // if(generator.CreateMeshes)
                            chunk.CreateVertices();
                            if (chunk.lod > 0)
                                chunk.ClearData();
                            if (chunk.verts.IsCreated)
                            {
                                hasStuff = true;

                            }
                            else
                            {
                                chunk.HasGrass = true;
                                chunk.Clear();
                                chunk.chunkProccessed = true;


                            }
                            if (!generator.CreateMeshes)
                                chunk.chunkProccessed = true;
                        }





                        else
                        {
                            chunk.HasGrass = true;
                            chunk.Clear();
                            chunk.chunkProccessed = true;


                        }

                }
                C = UpperchunksE.Count;
                //upperLevel
                for (int i = 0; i < C; i++)
                {

                    VoxelChunk chunk = UpperchunksE[i];

                        if (chunk.Createvoxels())
                        {
                            //if (generator.CreateMeshes)
                            chunk.CreateVertices();
                            chunk.ClearData();
                            if (chunk.verts.IsCreated || chunk.TreeCount > 0)
                            {
                                hasStuff = true;

                            }
                            else
                            {
                                chunk.HasGrass = true;
                                chunk.Clear();
                                chunk.chunkProccessed = true;


                            }
                            if (!generator.CreateMeshes)
                                chunk.chunkProccessed = true;
                        }
                        else
                        {
                            chunk.HasGrass = true;
                            chunk.Clear();
                            chunk.chunkProccessed = true;


                        }




                }
                    if (hasStuff && generator.CreateMeshes)
                    generator.AllocateVoxelChunks.Enqueue(this);
            }
        }

        public void AllocateMeshes()
        {
            int C = chunksE.Count;
            for (int i = 0; i < C; i++)
            {
                VoxelChunk myChunk = chunksE[i];
                if (myChunk.size>0 && myChunk.chunkProccessed == false)
                {
                    myChunk.AllocateMesh();
                    //Debug.Log("AllocateMesh " + myChunk.m_pos + " " + level);
                    //chunksE[i].mesh.UploadMeshData(false);

                }

            }
            generator.JobMakeMesh.Enqueue(this);
        }
        public void JobMakeMeshes()
        {
            int C = chunksE.Count;
            for (int i = 0; i < C; i++)
            {
                VoxelChunk myChunk = chunksE[i];
                if (myChunk.size>0 && myChunk.chunkProccessed == false)
                {
                    //Debug.Log("JobMakeMesh " + myChunk.m_pos + " " + level);
                    myChunk.CreateMyMesh();
                    //chunksE[i].mesh.UploadMeshData(false);

                }

            }
            generator.MeshChunks.Enqueue(this);
        }
        public void CreateMeshes()
        {
            int C = chunksE.Count;
            for (int i = 0; i < C; i++)
            {
                VoxelChunk myChunk = chunksE[i];
                if (myChunk.size > 0 && myChunk.chunkProccessed == false)
                {

                    myChunk.CreateMesh();
                   // Debug.Log("CreatedMesh " + myChunk.m_pos + " " + level);
                    int meshId = 0;

                    myChunk.m_voxelGo.m_MeshCollider.enabled = false;
                    meshId = myChunk.m_voxelGo.mesh.GetInstanceID();


                    // This spreads the expensive operation over all cores.
                    var job = new BakeJob(meshId);
                    JobHandle h = job.Schedule(1, 1);
                    h.Complete();
                    // Now instantiate colliders on the main thread.

                    myChunk.m_voxelGo.m_MeshCollider.sharedMesh = myChunk.m_voxelGo.mesh;

                    myChunk.m_voxelGo.m_MeshCollider.enabled = true;

                    //chunksE[i].mesh.UploadMeshData(false);

                }
                else
                {
                    myChunk.HasGrass = true;
                    myChunk.Clear();
                    myChunk.chunkProccessed = true;


                }
            }
            C = UpperchunksE.Count;
            for (int i = 0; i < C; i++)
            {
                VoxelChunk myChunk = UpperchunksE[i];
                if (myChunk.size>0 && myChunk.chunkProccessed == false || myChunk.TreeCount > 0 && myChunk.chunkProccessed == false)
                {

                    if (myChunk.chunkProccessed == false)
                        myChunk.CreateMesh();
                }
                else
                {
                    myChunk.HasGrass = true;
                    myChunk.Clear();
                    myChunk.chunkProccessed = true;


                }

            }

        }


        public void RenderGrass()
        {
            if (level == 0)
            {
                for (int i = 0; i < chunksE.Count; i++)
                {
                    if (chunksE[i].canGrass)
                        chunksE[i].RenderGrass();
                }
            }
            else
            {
                for (int i = 0; i < children.Count; i++)
                {

                    children[i].RenderGrass();
                }
            }
        }

        public void CheckGrassDist()
        {
            if (level == 0)
            {
                for (int i = 0; i < chunksE.Count; i++)
                {
                    if (chunksE[i].Pos != null && chunksE[i].Pos.Count > 0)
                    {
                        for (int G = 0; G < chunksE[i].Pos.Count; G++)
                        {
                            if (generator.CanContinue)
                            {
                                chunksE[i].GrassDist[G] = (generator.PlayerGrass[0] -  chunksE[i].Pos[G]).magnitude;

                            }
                            else break;
                        }

                        float d = (generator.PlayerGrass[0] - chunksE[i].m_pos).magnitude;
                        if (d < generator.DetailDistance * 1.25f)
                            chunksE[i].canGrass = true;
                        else chunksE[i].canGrass = false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].CheckGrassDist();
                }
            }
        }
        public void GenerateChunks(int TLOd)
        {
            for (int i = 0; i < mask.Length; i++)
            {
                if (generator.CanContinue)
                {
                    Vector3Int pos = generator.m_voxelWidth * VoxelTerrainEngine.TriSize[TLOd]* mask[i];
                    pos.Set(pos.x + ParentPos.x, pos.y + ParentPos.y, pos.z + ParentPos.z);

                    if (!chunks.ContainsKey(pos))
                    {
                       VoxelChunk chunk = generator.CreateChunk(pos, TLOd, false);
                        chunk.parentObj = this;
                        chunks.Add(pos, chunk);

                        chunksE.Add(chunk);
                        if (generator.CreateMeshes == false)
                            chunk.chunkProccessed = true;
                    }
                    //else Debug.Log("Problem Here");
                }
                else break;
            }

        }
        public void GenerateDChunks()
        {
            int cSize = generator.m_voxelWidth * VoxelTerrainEngine.TriSize[maxlod - 1];
            for (int x = 0; x < Chunksize; x += cSize)
            {
                for (int y = 0; y < Chunksize; y += cSize)
                {
                    for (int z = 0; z < Chunksize; z += cSize)
                    {
                        if (generator.CanContinue)
                        {
                            Vector3Int pos = new Vector3Int(Posx + x, Posy + y, Posz + z);
                            if (!Upperchunks.ContainsKey(pos))
                            {
                              VoxelChunk chunk =  generator.CreateChunk(pos, maxlod - 1, true);
                                chunk.parentObj = this;
                                Upperchunks.Add(pos, chunk);

                                //Pobj.waterChunks.Add(WChunk);
                                // Chunk.Wchunk = WChunk;
                                //set flag on chunk
                                UpperchunksE.Add(chunk);
                                if (generator.CreateMeshes == false)
                                    chunk.chunkProccessed = true;
                            }
                            //else Debug.Log("Problem Here");
                        }
                        else break;
                    }
                }
            }

        }
        public void Combine()
        {
            DestroyChildren(false);
            children.Clear();
            childrenI.Clear();
        }
        public void DestroyChunks()
        {
            frontorback = infrontOrBehind.infront;
            if (chunksE.Count > 0)
            {
                for (int t = 0; t < chunksE.Count; t++)
                {
                    chunksE[t].Clear();

                    destroyChunk.Enqueue(chunksE[t]);

                }
            }
            if (chunksE.Count > 0)
            {
                chunksE.Clear();
                chunks.Clear();
                
            }
            if (destroyChunk.Count > 0 && !generator.ToDestroy.Contains(this))
                generator.ToDestroy.Enqueue(this);
        }
        public void subdevide()
        {

            for (int i = 0; i < mask.Length; i++)
            {
                Vector3Int pos = new Vector3Int(Posx + ((Chunksize / 2) * mask[i].x), Posy + ((Chunksize / 2) * mask[i].y), Posz + ((Chunksize / 2) * mask[i].z));
                //subdeviding to much
                //use dictionary
                if (generator.CanContinue)
                {
                    if (!childrenI.ContainsKey(pos) && Chunksize > (generator.m_voxelWidth * VoxelTerrainEngine.TriSize[generator.minLod]) * 2)
                    {
                        parentObject pobj = new parentObject(pos.x, pos.y, pos.z, generator, Chunksize / 2, level - 1)
                        {
                            Parent = this
                        };
                        childrenI.Add(pos, pobj);
                        pobj.checklod();
                        children.Add(pobj);
                        
                    }
                }
                else break;
            }
            DestroyChunks();
           

        }
    }
}
