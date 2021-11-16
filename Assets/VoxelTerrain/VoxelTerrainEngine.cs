using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using VoxelEngine.DoubleKeyDictionary;

using System.Net.Sockets;
using System.Collections.Concurrent;
using UnityEngine.UI;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using System.IO;

namespace VoxelEngine
{
    [BurstCompile]
    public struct BakeJob : IJobParallelFor
    {
        private int meshId;

        public BakeJob(int meshIds)
        {
            this.meshId = meshIds;
        }

        public void Execute(int index)
        {
            Physics.BakeMesh(meshId, false);
        }
    }


    public struct Voxels{
       public float VoxelValue ;
    public byte Material;
    public byte Biome;


        }
    public struct VTree
    {
        public Vector3 treePos;
        public int treeBiome;
        public int treeindex;
        
        public int treeHealth;
    }
public class VoxelTerrainEngine: MonoBehaviour 
 {
        bool poolsFilled = false;
        [HideInInspector]
        public Transform parent;
        int memoryInUse;
        int ChunkCount;
        int Arrayslength;
       int GoChunkslength;
        public bool DoNothing;
        int destroyN;
        float debugTimer;
        public GameObject babel;
        public float babelDistance;
        public float babelFrequency;

        public List<GameObject> babelsGO = new List<GameObject>();
        public List<Vector3> babels = new List<Vector3>();
        //public SetSpawnPosition spawner;
        public bool RenderGrass;
        public bool renderTrees;
        public bool CreateMeshes;
        public bool CreateWater;
        public ConcurrentQueue<parentObject> Destroyable = new ConcurrentQueue<parentObject>();
        public ConcurrentQueue<parentObject> ParentObjPool = new ConcurrentQueue<parentObject>();
        
       // public List<WaterChunk> waterChunks = new List<WaterChunk>();
        int TerrainsLoaded;
        public TextureFormat format;
        public ConcurrentBag<VoxelChunk> ChunkPool = new ConcurrentBag<VoxelChunk>();
        public ConcurrentBag<VoxelGO> GOChunkPool = new ConcurrentBag<VoxelGO>();
        //public ConcurrentQueue<WaterChunk> WChunkPool = new ConcurrentQueue<WaterChunk>();
        public ConcurrentBag<Voxels[,,]> arrays = new ConcurrentBag<Voxels[,,]>();
        public ConcurrentBag<Vector3[,,]> NormalsArr = new ConcurrentBag<Vector3[,,]>();
        public float offPos;
        public bool makeTerrain;
        public List<parentObject> children = new List<parentObject>();
        public float TerrainLodBias = 1.25f;
        public int GrassLayer;
        public int numberOfTextures;
        public Texture2D[] textures;
        public Texture2D[] texturesNormal;
        public int SnowLine;
		public int grassFrequency ;
		public int TreeFrequency ;
        public SurfaceNets surfaceNets;
		[Range(0,9)]
		public int minLod;
        [Range(0, 9)]
        public int maxLod;
        public float offset;
		//public GameObject [] enemys;
        //public GameObject [] insects;
  //public int maxInsects;
		public NoiseModule noise;
		public UnityEngine.Rendering.ShadowCastingMode shadows;
		public LayerMask mask;
		public Material m_material;
		public Material water;
        public MeshFactory meshFactory;
		public int distanceToload;
		[Range(0,200)]
		public float DetailDistance;
		public int MaxTrees;
		public int MaxGrass;
        public int MaxPlants;
        public int MaxRocks;
		public bool MakeCaves;
  public int [,] stuff;
        public GameObject[] lavaParticles;
        public GameObject []TreesGrass;
        public GameObject []TreesForest;
		public GameObject []TreesDesert;
		public GameObject []TreesTropics;
		public GameObject []TreesSnow;
		public GameObject []Rocks;
		public GameObject[] Grass;
		public GameObject[] GrassForest;
		public GameObject []GrassTropics;
		public GameObject[] GrassDesert;
		public GameObject[] GrassSnow;
        [HideInInspector]
        public Mesh[] MeshGrass;
        [HideInInspector]
        public Material[] MatGrass;
        [HideInInspector]
		public Mesh[] GrassMesh;
		[HideInInspector]
		public Material[] GrassMat;
		[HideInInspector]
		public Mesh[] GrassMeshForest;
		[HideInInspector]
		public Material[] GrassMatForest;
		[HideInInspector]
		public Mesh[] GrassMeshTropics;
		[HideInInspector]
		public Material[] GrassMatTropics;
		[HideInInspector]
		public Mesh[] GrassMeshDesert;
		[HideInInspector]
		public Material[] GrassMatDesert;
		[HideInInspector]
		public Mesh[] GrassMeshSnow;
		[HideInInspector]
		public Material[] GrassMatSnow;
		public float []GrassWeights;
		Vector2 Campos;
		[HideInInspector]
		public Transform Parent;
		[HideInInspector]
  public Dictionary<Vector3Int, parentObject>m_voxelChunk;
        public static int sleepTime = 0;
  public int m_surfaceSeed = 4;
        public int m_voxelWidth = 32;
		public PerlinNoise m_surfacePerlin;
		public Thread [] Threads;
        public static VoxelTerrainEngine Generator;
		[HideInInspector]
		public Vector3 ParentPos;
        [HideInInspector]
        public bool CanContinue;
        public float time = 0;
  public List<Vector3> PlayerGrass = new List<Vector3>();
        public List<Vector2> Player = new List<Vector2>();
        public ConcurrentQueue<parentObject>ToDestroy;
        public ConcurrentQueue<parentObject>VoxelChunks;
        public ConcurrentQueue<parentObject> AllocateVoxelChunks = new ConcurrentQueue<parentObject>();
        public ConcurrentQueue<parentObject> JobMakeMesh = new ConcurrentQueue<parentObject>();
        //Edited chunks
        public ConcurrentQueue<VoxelChunk> EAllocateVoxelChunks = new ConcurrentQueue<VoxelChunk>();
        public ConcurrentQueue<VoxelChunk> EJobMakeMesh = new ConcurrentQueue<VoxelChunk>();
        public ConcurrentQueue<VoxelChunk> EVoxelChunks = new ConcurrentQueue<VoxelChunk>();
        public ConcurrentQueue<VoxelChunk> EVoxelVertChunks = new ConcurrentQueue<VoxelChunk>();
       // public ConcurrentQueue<WaterChunk> EWaterVertChunks = new ConcurrentQueue<WaterChunk>();
        //public ConcurrentQueue<WaterChunk> EVoxelWaterChunks = new ConcurrentQueue<WaterChunk>();

       // public ConcurrentQueue<WaterChunk> IEWaterVertChunks = new ConcurrentQueue<WaterChunk>();
       // public ConcurrentQueue<WaterChunk> IEVoxelWaterChunks = new ConcurrentQueue<WaterChunk>();

        public ConcurrentQueue<VoxelChunk> EMeshChunks = new ConcurrentQueue<VoxelChunk>();
        //public ConcurrentQueue<WaterChunk> EMeshWaterChunks = new ConcurrentQueue<WaterChunk>();
        public ConcurrentQueue<parentObject>MeshChunks;
       // public ConcurrentQueue<WaterChunk> WMeshChunks = new ConcurrentQueue<WaterChunk>();
		[HideInInspector]
		public bool cansave;
		public List<Transform> target;
        public Thread VoxelGenerator;
		public static materialType type;
  public Camera mainCam;
        public float GCTIme;
        public ConcurrentQueue<VoxelChunk> ChunksNetworked = new ConcurrentQueue<VoxelChunk>();
        public static int [] TriSize = new int[10]{
			1,2,4,8,16,32,64,128,256,512
		};
        public static int[] LodDist = new int[10]{
            32,64,128,256,512,1024,2048,4096,8192,16384
        };
        

        public void Start(){
            parent = transform;
            /////////////
            if (DoNothing==false)
            {
                VoxelGO.layer = gameObject.layer;

                //surfaceNets = new SurfaceNets();
                Texture2DArray textureArray = new Texture2DArray(textures[0].width, textures[0].height, numberOfTextures, textures[0].format, true, false);

                for (int i = 0; i < textures.Length; i++)
                {

                    Graphics.CopyTexture(textures[i], 0, textureArray, i);
                }
                textureArray.Apply(false, true);
                m_material.SetTexture("_MainTex", textureArray);

                Texture2DArray textureArrayN = new Texture2DArray(texturesNormal[0].width, texturesNormal[0].height, numberOfTextures, texturesNormal[0].format, true, true);
                for (int i = 0; i < texturesNormal.Length; i++)
                {

                    Graphics.CopyTexture(texturesNormal[i], 0, textureArrayN, i);
                    //textureArray.SetPixels32(texturesNormal[i].GetPixels32(0), i, 0);
                }
                textureArrayN.Apply(false, true);

                m_material.SetTexture("_MainTexNorm", textureArrayN);


                /////////////
                type = GetComponent<materialType>();
                float weights = 0;
                for (int t = 0; t < GrassWeights.Length; t++)
                    weights += GrassWeights[t];
                VoxelChunk.totalweights = Mathf.RoundToInt(weights);


                AddTarget(Camera.main.transform);
                int BCount = PlayerPrefs.GetInt("babelsCount");
              /*  for (int i = 0; i < BCount; i++)
                {
                    Vector3 pos = new Vector3(PlayerPrefs.GetFloat("babelsPosx" + i),
                    PlayerPrefs.GetFloat("babelsPosy" + i),
                    PlayerPrefs.GetFloat("babelsPosz" + i));
                    GameObject Bab = Instantiate(babel, pos, Quaternion.identity, Parent);
                    babelsGO.Add(Bab);



                }*/
                mainCam = Camera.main;



                    EngineStart();
                    Debug.Log("started Client" + " " + m_surfaceSeed);
            }
            else { Generator = this; sleepTime = 1; }

        }
        public void EngineStart(){

            sleepTime = 0;
   ToDestroy = new ConcurrentQueue<parentObject>();
            VoxelChunks = new ConcurrentQueue<parentObject>();
            m_voxelChunk = new Dictionary<Vector3Int, parentObject>();
            MeshChunks = new ConcurrentQueue<parentObject>();
            Threads = new Thread[1];


            GrassMesh = new Mesh[Grass.Length];
            GrassMat = new Material[Grass.Length];
            MatGrass = new Material[5];
            MeshGrass = new Mesh[5];
            for (int i = 0; i < Grass.Length; i++) {
                GrassMesh[i] = Grass[i].GetComponent<MeshFilter>().sharedMesh;
                GrassMat[i] = Grass[i].GetComponent<MeshRenderer>().sharedMaterial;
                if (i == 0) {
                    MatGrass[0] = GrassMat[i];
                    MeshGrass[0] = GrassMesh[i];
            }
            }
            ////////////////

            GrassMeshForest = new Mesh[Grass.Length];
            GrassMatForest = new Material[Grass.Length];

            for(int i =0;i < Grass.Length;i++){
                GrassMeshForest[i] = GrassForest[i].GetComponent<MeshFilter>().sharedMesh;
                GrassMatForest[i] = GrassForest[i].GetComponent<MeshRenderer>().sharedMaterial;
                if (i == 0)
                {
                    MatGrass[1] = GrassMatForest[i];
                    MeshGrass[1] = GrassMeshForest[i];
                }
            }
            ////////////////////

            GrassMeshTropics = new Mesh[Grass.Length];
            GrassMatTropics = new Material[Grass.Length];

            for(int i =0;i < Grass.Length;i++){
                GrassMeshTropics[i] = GrassTropics[i].GetComponent<MeshFilter>().sharedMesh;
                GrassMatTropics[i] = GrassTropics[i].GetComponent<MeshRenderer>().sharedMaterial;
                if (i == 0)
                {
                    MatGrass[2] = GrassMatTropics[i];
                    MeshGrass[2] = GrassMeshTropics[i];
                }
            }
            ////////////////////
  
            GrassMeshDesert = new Mesh[Grass.Length];
            GrassMatDesert = new Material[Grass.Length];

            for(int i =0;i < Grass.Length;i++){
                GrassMeshDesert[i] = GrassDesert[i].GetComponent<MeshFilter>().sharedMesh;
                GrassMatDesert[i] = GrassDesert[i].GetComponent<MeshRenderer>().sharedMaterial;
                if (i == 0)
                {
                    MatGrass[4] = GrassMatDesert[i];
                    MeshGrass[4] = GrassMeshDesert[i];
                }
            }
            ////////////////////

            GrassMeshSnow = new Mesh[Grass.Length];
            GrassMatSnow = new Material[Grass.Length];

            for(int i =0;i < Grass.Length;i++){
                GrassMeshSnow[i] = GrassSnow[i].GetComponent<MeshFilter>().sharedMesh;
                GrassMatSnow[i] = GrassSnow[i].GetComponent<MeshRenderer>().sharedMaterial;
                if (i == 0)
                {
                    MeshGrass[3] = GrassMeshSnow[i];
                    MatGrass[3] = GrassMatSnow[i];
                    
                }
            }



            Generator = this;

            Parent = transform;


            VoxelChunk.generator = Generator;


            MeshFactory.generator = Generator;



            MeshFactory.MarchingCubes = new MarchingCubes();
            meshFactory = new MeshFactory();
            ParentPos = Parent.position;

            MarchingCubes.SetTarget(125);

            MarchingCubes.SetWindingOrder(0, 1, 2);
            FillPoolsVoxelGo();
            StartCoroutine(StartPerlinNoise());

            CanContinue = true;
            Shader.WarmupAllShaders();
            StartCoroutine(StartThreads());

        }

		public void AddTarget(Transform m_target ){
			target.Add(m_target);
   
            
            Vector3 ppos = m_target.position;
            PlayerGrass.Add(ppos);
            int w = m_voxelWidth * TriSize[maxLod];
            w *= 2;
            ppos /= w;
            ppos.Set(Mathf.RoundToInt(ppos.x), Mathf.RoundToInt(ppos.y), Mathf.RoundToInt(ppos.z));
            ppos *= w;
            Player.Add(new Vector2(ppos.x, ppos.z));

        }
        public IEnumerator StartPerlinNoise()
        {

            yield return new WaitForSeconds(1);

            m_surfacePerlin = new PerlinNoise(m_surfaceSeed);
            UnityEngine.Random.InitState(m_surfaceSeed);
            MeshFactory.SurfacePerlin = m_surfacePerlin;
            SimplexNoise.Seed = m_surfaceSeed;
        }
		//start all threads
		public IEnumerator StartThreads(){
			yield return new WaitForSeconds(4);
			VoxelGenerator = new Thread(GenerateTerrains);
			VoxelGenerator.IsBackground = false;
   VoxelGenerator.Priority = System.Threading.ThreadPriority.Lowest;
			VoxelGenerator.Start();


			for(int i =0;i < Threads.Length;i++){	
				Threads[i] = new Thread(GenerateVoxels);
				Threads[i].IsBackground = false;
    Threads[i].Priority = System.Threading.ThreadPriority.Lowest ;
				Threads[i].Start();
    Debug.Log("started Thread");
			}

		}
		//raycast for the voxel
		//basically just a normal raycast but with better targetting for the voxels
		public static bool RaycastVoxels(Ray ray, out RaycastHit hitinfo,float distance,LayerMask mask ){
            RaycastHit hit;

            if (Physics.Raycast(ray.origin, ray.direction, out hit, distance,mask))
            {
                Vector3 p = hit.point + ray.direction/2;

                int x = Mathf.RoundToInt(p.x);
                int y = Mathf.RoundToInt(p.y);
                int z = Mathf.RoundToInt(p.z);




                if (hit.collider.tag == Generator.transform.tag)
                {
                    hit.point = new Vector3(x, y, z);
                }




                hitinfo = hit;

                return true;
            }

            else
            {

                hitinfo = hit;

                return false;

            }

        }
		//raycast for the voxel
		//basically just a normal raycast but with better targetting for the voxels
		public static bool RaycastVoxels(Ray ray, out RaycastHit hitinfo,float distance ){

            RaycastHit hit;

            if (Physics.Raycast(ray.origin, ray.direction, out hit, distance))
            {
                Vector3 p = hit.point + ray.direction / 2;

                int x = Mathf.RoundToInt(p.x);
                int y = Mathf.RoundToInt(p.y);
                int z = Mathf.RoundToInt(p.z);



                if (hit.collider.tag == Generator.transform.tag)
                {
                    hit.point = new Vector3(x, y, z);
                }




                hitinfo = hit;

                return true;
            }

            else
            {

                hitinfo = hit;

                return false;

            }

        }


		public int FindVoxelType(Vector3 hitpoint){

            int C = children.Count;
            int Value;
            for (int c = 0; c < C; c++)
            {

                if (children[c] != null)
                {
                    Value = children[c].FindVoxelType(hitpoint);
                    if (Value > -1)
                        return Value;

                }
            }
            return -1;


        }
		public static void SetVoxelType(Vector3 voxel,byte value){

			Generator.SetM_VoxelType(voxel,value);

		}

		public void SetM_VoxelType(Vector3 hitpoint,byte type){

			int C = children.Count;
			for(int c = 0;c < C;c++){

				if(children[c]!=null){
                    children[c].SetVoxelType(hitpoint,type);


				}

			}


		}

		public VoxelChunk FindChunk(Vector3 pos){
            VoxelChunk chunk;
			for(int i = 0; i < children.Count; i++){
				if(children[i].FindChunk(pos,out chunk)){
					return chunk;
				}

			}
			return null;
		}
        public VoxelChunk FindTreeChunk(Vector3 pos)
        {
            VoxelChunk chunk;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].FindTeeeChunk (pos, out chunk))
                {
                    return chunk;
                }

            }
            return null;
        }

        public VoxelChunk FindChunkPos(Vector3 pos)
        {
            VoxelChunk chunk;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].FindChunkPos(pos, out chunk))
                {
                    return chunk;
                }

            }
            return null;
        }
        //check if voxel exists and to what chunk it belongs to 
        //just VoxelTerrainEngine.raycastvoxels to raycast for a
        //voxel then check the hit.point against this
        //should return a chunk
        /*
        public bool CheckVoxels(Vector3 HitPoint,out VoxelChunk chunk){

			int C = ActiveChunks.Count;

			chunk = null;

			for(int c = 0;c < C;c++){

				if(ActiveChunks[c].CheckVoxels(HitPoint)){

					chunk = ActiveChunks[c];

				}
			}
			if(chunk!=null)

				return true;

			else 

				return false;
		}*/

        static void SetDistance(int Distance){
			Generator.distanceToload = Distance;
		}

		static void SetDetailDistance(float Distance){
			Generator.DetailDistance = Distance;
		}
		//method for saving changes to terrain at runtime
		public void SaveTerrains(){
			cansave = true;
		}

		public void SaveTerrain(){
			cansave = false;
            for(int i =0;i < children.Count; i++)
                {
                    children[i].SaveVoxels();
                }
                

        
            PlayerPrefs.SetInt("babelsCount", babels.Count);
            for (int i = 0;i < babels.Count;i++)
            {

                PlayerPrefs.SetFloat("babelsPosx"+i, babels[i].x);
                PlayerPrefs.SetFloat("babelsPosy"+i, babels[i].y);
                PlayerPrefs.SetFloat("babelsPosz"+i, babels[i].z);

            }
            
            }
        /*
        public bool CheckBounds(int lod , int x,int z,bool zopen ,bool xopen)
        {
            VoxelChunk chunk;
            int widthLength = m_voxelWidth * lod;
            for(int i = 0;i < ActiveChunks.Count; i++)
            { chunk = ActiveChunks[i];
               if ((TriSize[ chunk.lod] * m_voxelWidth)+ chunk.POSX -( x + widthLength) <=0)
                {
                    xopen = false;
                    if ((TriSize[chunk.lod] * m_voxelWidth) + chunk.POSZ - (z + widthLength) <= 0)
                    {
                        zopen = false;

                    }
                    return false;

                }
                
            }
            return true;
        }*/
		//static method for setting voxel values 
		//can be used for adding or removing voxels
		//once this method is called the chunk its on will
		//be automatically located and set to re create the mesh using using new voxel data
		public static bool SetVoxels(Vector3 voxel,float value){

            if (Generator.SetVoxel(voxel, value))
                return true;
            else return false;

		}

        public static bool SetVoxels(List<Vector3> voxel, List<float> value)
        {

            if (Generator.SetVoxel(voxel, value))
                return true;
            else return false;

        }
        //static method for setting voxel values 
        //can be used for adding or removing voxels
        //once this method is called the chunk its on will
        //be automatically located and set to re create the mesh using using new voxel data
        public static float GetVoxelsValue(Vector3 voxel){
                return Generator.GetVoxelValue(voxel);

		}
       /* public static bool GetWater(Vector3 voxel)
        {

            return Generator.Get_Water(voxel);

        }*/

        public static int GetBiome(Vector3 voxel){
                return Generator.Get_Biome(voxel);
		}

        public int Get_Biome(Vector3 hitpoint)
        {

            int C = children.Count;
            int Value;
            for (int c = 0; c < C; c++)
            {

                if (children[c] != null)
                {
                    Value = children[c].GetBiome(hitpoint);
                    if (Value > -1)
                        return Value;

                }
            }
            return -1;
        }
        public float GetVoxelValue(Vector3 hitpoint){

			int C = children.Count;
            float Value;
			for(int c = 0;c < C;c++){

				if( children[c]!=null)
                {
                    Value = children[c].GetVoxelValue(hitpoint);
                    if (Value > -1.0f)
                        return Value;

                }
			}
			return -1.0f;
		}
       /* public bool Get_Water(Vector3 hitpoint)
        {

            int C = waterChunks.Count;

            for (int c = 0; c < C; c++)
            {

                if (waterChunks[c] != null && waterChunks[c].lod < 1)
                {
                    
                    if (waterChunks[c].FindWaterVoxels(hitpoint))
                    {
                        return true;
                    }
                }
            }
            return false;
        }*/

        public bool FindSavedChunkInfo(Vector3 m_pos,Voxels[,,]m_voxels)
        {
            VoxelChunk chunk = FindChunk(m_pos);
            if (chunk.lod == 0 && chunk.Voxels != null)
            {
                m_voxels = chunk.Voxels;
                return true;
            }
            string FileName = String.Concat("VoxelChunk " + m_pos);

            string SaveName = String.Concat("Saved Chunks" + m_surfaceSeed);
           

           if (Directory.Exists(SaveName + "/" + FileName))
            {
                m_voxels = RequestArray3d(m_voxelWidth + 5);
                LoadVoxels(SaveName, FileName, m_voxels);
                return true;
            }
            return false;

        }
        public void LoadVoxels(string SaveName,string FileName, Voxels[,,] m_voxels)
        {
            using (FileStream file = File.Open(SaveName + "/" + FileName + "/" + FileName + "dat" + ".dat", FileMode.Open))
            {
                using (BinaryReader writer = new BinaryReader(file))
                {
                    for (int x = 0; x < m_voxels.GetLength(0); x++)
                    {

                        for (int y = 0; y < m_voxels.GetLength(1); y++)
                        {

                            for (int z = 0; z < m_voxels.GetLength(2); z++)
                            {

                                m_voxels[x, y, z].VoxelValue = writer.ReadSingle();
                                m_voxels[x, y, z].Material = writer.ReadByte();
                                m_voxels[x, y, z].Biome = writer.ReadByte();
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
        }
        
        public bool SetVoxel(List<Vector3> hitpoint, List<float> value)
        {
            bool hasSet = false;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].SetVoxel(hitpoint, value))
                    hasSet = true;

            }
            return hasSet;


        }
        public bool SetVoxel(Vector3 hitpoint,float value){
            bool hasSet = false ;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].SetVoxel(hitpoint, value))
                    hasSet = true;

            }
            return hasSet;


		}
  /*public bool SetWater(List<Vector3> hitpoint,int value){
            
   

            bool hasSet = false;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].SetWaterVoxel (hitpoint, value))
                    hasSet = true;

            }
            return hasSet;
   
  }*/

		public static int  FindMaterial(Vector3 voxel){

			int type = 0;
                type = Generator.FindVoxelType(voxel);
            if (type > -1)
                return type;
            else return 0;
		}
        public void OnDrawGizmos()
        {
            if (CanContinue == true && children.Count>0 )
            {
                for (int i = 0; i < children.Count; i++)
                {
                        if(children[i].isDestroyed==false)
                    children[i].draw();
                }

            }
        }


        public void Create()
        {
     

                
                
              
 

            if (CreateMeshes && MeshChunks.TryDequeue(out parentObject chunk))
            {
                TerrainsLoaded++;
                chunk.CreateMeshes();
                GC.Collect();

            }
            VoxelChunk mchunk;

                  /*  if (CreateWater && WMeshChunks.TryDequeue(out WaterChunk WChunk))
                    {
                        if (WChunk.hasFull)
                        {

                            WChunk.CreatewaterMesh();

                        }
                        else
                        {

                            WChunk.chunkProccessed = true;
                        }

                        if (!waterChunks.Contains(WChunk) && WChunk.lod == 0)
                        {
                            waterChunks.Add(WChunk);
                        }
                        else if (waterChunks.Contains(WChunk) && WChunk.lod != 0)
                        {
                            waterChunks.Remove(WChunk);

                        }
                }*/


                if (MeshChunks.Count == 0 && VoxelChunks.Count==0) { 
                    sleepTime = 1;

                }




            
        }

        void Update()
        {
            if (DoNothing == false)
            {

                GCTIme += Time.smoothDeltaTime;
                if (GCTIme > 5)
                {
                    GC.Collect(0,GCCollectionMode.Forced,true);
                    GCTIme = 0;
                }

                ChunkCount = ChunkPool.Count;
               // if (CreateMeshes && babels.Count != babelsGO.Count && babels.Count > babelsGO.Count)
               // {
                //    GameObject Bab = Instantiate(babel, babels[babels.Count - 1], quaternion.identity, Parent);
                //    babelsGO.Add(Bab);
                //}
                if (CanContinue && children.Count > 0)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        children[i].RenderGrass();
                    }
                }
                parentObject pObj;
                //allocate Meshes
                if (AllocateVoxelChunks.TryDequeue(out pObj))
                {
                    pObj.AllocateMeshes();
                   
                }
                Arrayslength = arrays.Count;
                GoChunkslength = GOChunkPool.Count;
        VoxelChunk Voxelchunk;
                //Create Edited Meshes
                while (EMeshChunks.TryDequeue(out Voxelchunk))
                {
                    Voxelchunk.CreateMesh();
                    if (Voxelchunk.isUpperLevel == false)
                    {
                        int meshId = 0;

                        Voxelchunk.m_voxelGo.m_MeshCollider.enabled = false;
                        meshId = Voxelchunk.m_voxelGo.mesh.GetInstanceID();


                        // This spreads the expensive operation over all cores.
                        var job = new BakeJob(meshId);
                        JobHandle h = job.Schedule(1, 1);
                        h.Complete();
                        Voxelchunk.m_voxelGo.m_MeshCollider.sharedMesh = Voxelchunk.m_voxelGo.mesh;

                        Voxelchunk.m_voxelGo.m_MeshCollider.enabled = true;
                    }

                }
                //allocate EditedMesh
                while(EAllocateVoxelChunks.TryDequeue(out Voxelchunk))
                {
                    Voxelchunk.AllocateMesh();
                    EJobMakeMesh.Enqueue(Voxelchunk);
                }

                //WaterChunk Waterchunk;
                /* while (IEVoxelWaterChunks.TryDequeue(out Waterchunk))
                 {
                     Waterchunk.Initialize();
                     EVoxelWaterChunks.Enqueue(Waterchunk);
                 }
                 while (IEWaterVertChunks.TryDequeue(out Waterchunk))
                 {
                     Waterchunk.Initialize();
                     EWaterVertChunks.Enqueue(Waterchunk);
                 }*/
                parentObject chunk;



                /*         while (EMeshWaterChunks.TryDequeue(out WaterChunk Wchunk))
                         {
                             Wchunk.CreatewaterMesh();
                             if (!waterChunks.Contains(Wchunk) && Wchunk.lod == 0)
                             {
                                 waterChunks.Add(Wchunk);
                             }
                             else if (waterChunks.Contains(Wchunk) && Wchunk.lod != 0)
                             {
                                 waterChunks.Remove(Wchunk);

                             }
                         }*/
    
                    while (Destroyable.TryDequeue(out chunk))
                        chunk.Destroy();
                Shader.SetGlobalFloat("Distance", DetailDistance);
                for (int i = 0; i < target.Count; i++)
                {
  
                    PlayerGrass[i] = new Vector3(target[i].position.x, target[i].position.y, target[i].position.z);
                    Vector3 ppos = PlayerGrass[i];
                    int w = m_voxelWidth * TriSize[maxLod];
                    w *= 2;
                    ppos /= w;
                    ppos = new Vector3( Mathf.RoundToInt(ppos.x), Mathf.RoundToInt(ppos.y), Mathf.RoundToInt(ppos.z));
                    ppos *= w;
                    Player[i] = new Vector2(ppos.x, ppos.z);

                }
                if (debugTimer >= 8)
                {
                    Create();
                }
                else
                    debugTimer += Time.deltaTime;

            }
        }


        //method to destroy object 
        //had to do this as destroy can only be called from the main thread
        void EngineDestroy(Vector3Int POS)
        {
            if (m_voxelChunk.ContainsKey(POS))
            {
                m_voxelChunk[POS].SaveVoxels();


                ToDestroy.Enqueue(m_voxelChunk[POS]);
                m_voxelChunk.Remove(POS);

            }
        }

       /* void checkWater()
        {
            if (waterChunks.Count > 0)
            {
                for (int i = 0; i < waterChunks.Count; i++)
                {

                    if (waterChunks[i].HasWater && waterChunks[i].lod == 0 && waterChunks[i].chunkProccessed)
                        waterChunks[i].CheckWater();

                   
                }
            }
            Thread.Sleep(1);
        }*/
        void CheckChunks()
        {
            try
            {
                int distToload = Mathf.Clamp(distanceToload, 2, 6);
                if (children.Count > 0)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        children[i].CheckGrassDist();


                    }
                }
                destroyN = ToDestroy.Count;
                Thread.Sleep(2);
                if (children.Count > 0)
                {
                    float d;
                    
                    List<parentObject> Dchunks = new List<parentObject>();
                    d = distToload * ((m_voxelWidth * TriSize[maxLod])*2);
                    for (int i = 0; i < children.Count; i++)
                    {
                        if (CanContinue)
                        {

                            bool isClose = false;
                            parentObject chunk = children[i];
                            for (int p = 0; p < Player.Count; p++)
                            {

                                
                                Vector2 pPos = Player[p];
                               
                                Vector2 chunkpos = new Vector2(chunk.Posx, chunk.Posz);
                                float dist = Vector2.Distance(chunkpos, pPos);

                                if (dist <= d*1.15f)
                                {
                                    isClose = true;

                                }


                            }
                            if (chunk.isDestroyed == false && chunk.isChecking == false && chunk.CheckFrontOrback())
                            {


                                if (chunk.Cangen && chunk.isDestroyed == false && isClose == false)
                                {
                                    chunk.isDestroyed = true;

                                    chunk.DestroyChildren(true);
                                    Dchunks.Add(chunk);
                                    Debug.Log("Destroyed");

                                }
                                else if (chunk.isDestroyed == false)
                                {
                                    chunk.checklod();
                                }
                            }
                        }
                        else break;
                    }
                    Thread.Sleep(2);
                    if (Dchunks.Count > 0)
                    {
                        while( Dchunks.Count > 0)
                        {
                            if (CanContinue)
                            {
                                children.Remove(Dchunks[0]);
                                m_voxelChunk.Remove(new Vector3Int( Dchunks[0].Posx, Dchunks[0].Posy, Dchunks[0].Posz));
                                Dchunks.RemoveAt(0);
                            }
                            else break;
                        }
                    }
                    if(Dchunks.Count>0)
                    Thread.Sleep(2);
                    while (ToDestroy.Count > 0)
                    {
                        parentObject pobj;
                        if (ToDestroy == null)
                            Debug.LogError("Fucked");
                        if (ToDestroy.Count > 0 && ToDestroy.TryPeek(out pobj) && pobj == null)
                            ToDestroy.TryDequeue(out  pobj);

                        else if (ToDestroy.Count > 0 && ToDestroy.TryDequeue(out pobj))
                        {
                            if(pobj.Cangen && pobj.CheckFrontOrback())
                            Destroyable.Enqueue(pobj);
                            else
                            ToDestroy.Enqueue(pobj);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e.StackTrace + e.Message + e.Source);
            }
        }


            void GenerateVoxels(){
			while (CanContinue)
			{	
				try
				{


 
                    //Create the voxel data

                    parentObject Pchunk;


                    if (VoxelChunks.TryDequeue(out Pchunk))
                    {

                        Pchunk.CreateVoxels();

                    }

                    Thread.Sleep(2);
                    VoxelChunk chunk;
                    //edited Chunk
                    if (EVoxelChunks.TryDequeue(out chunk))
                    {
                        if(chunk.chunkProccessed)
                        {
                            chunk.chunkProccessed = false;
                        Debug.Log("Edited made Voxels and Verts");
                            chunk.chunkProccessed = false;
                        if (chunk.Createvoxels())
                        {
                            chunk.CheckForVoxelEdits();
                               

                        }

                            if (chunk.size > 0)
                            {

                                EVoxelVertChunks.Enqueue(chunk);
                            }
                            else
                            {
                                chunk.chunkProccessed = true;
                            }
                    }
                    else
                    {
                            EVoxelChunks.Enqueue(chunk);
                    }
                    }
                 /*   if (EVoxelWaterChunks.TryDequeue(out Wchunk))
                    {
                        if (!Wchunk.chunkProccessed)
                        {
                            if (Wchunk.CreateWaterVoxels())
                            {
                                Wchunk.chunkProccessed = false;
                                Wchunk.CheckForVoxelEdits();


                                Wchunk.CreateWaterVertices();

                                if (Wchunk.hasFull)
                                    EMeshWaterChunks.Enqueue(Wchunk);

                            }else Wchunk.chunkProccessed = true;
                        }
                        else if (!Wchunk.chunkProccessed)
                        {
                            EVoxelWaterChunks.Enqueue(Wchunk);
                        }

                    }*/
                 if(JobMakeMesh.TryDequeue(out Pchunk))
                    {
                        Pchunk.JobMakeMeshes();
                    }
                    if (EJobMakeMesh.TryDequeue(out chunk))
                    {
                        chunk.CreateMyMesh();
                        EMeshChunks.Enqueue(chunk);
                    }
                    Thread.Sleep(2);
                    if (EVoxelVertChunks.TryDequeue(out chunk))
                    {

                        if (chunk.chunkProccessed)
                        {
                            chunk.chunkProccessed = false;
                            chunk.CheckForVoxelEdits();
                            chunk.CreateVertices();
                            if (chunk.size > 0)
                                EAllocateVoxelChunks.Enqueue(chunk);
                            else
                                chunk.chunkProccessed = true;
                        }
                        else
                        {
                            EVoxelVertChunks.Enqueue(chunk);
                        }
                        
                    }

                    /*if (EWaterVertChunks.TryDequeue(out Wchunk))
                    {
                        
                        if (Wchunk.hasFull && Wchunk.chunkProccessed) {
                            Wchunk.chunkProccessed = false;
                            Wchunk.CheckForVoxelEdits();
                            Wchunk.CreateWaterVertices();
                            EMeshWaterChunks.Enqueue(Wchunk);
                        }
                        else if(!Wchunk.chunkProccessed)
                        {
                            EWaterVertChunks.Enqueue(Wchunk);
                        }
                        
                    }*/


                }
				catch (Exception e){
					Debug.LogError(e.StackTrace+e.Message+e.Source);
				}
			}
		}


        public Voxels[,,] RequestArray3d(int size)
        {
            Voxels[,,] array;
            if (arrays.Count > 0)
            {
                if (arrays.TryTake(out array))
                    return array;
                else
                {
                    while (arrays.TryTake(out array) == false)
                    {

                    }
                    return array;
                }
            }
            else
            {
                array = new Voxels[size, size, size];
                    Debug.Log("Added new Array" + arrays.Count);
            }
            return array;

        }
        public byte[,,] RequestArray(int size)
        {
            byte[,,] array;
            array = new byte[size, size, size];
            return array;

        }
        public Vector3[,,] RequestNormalArray(int size)
        {
            Vector3[,,] array;
            if (NormalsArr.TryTake(out array))
            {
                return array;
               
            }
            else
            {
                Debug.Log("created new normalArray");
                array = new Vector3[size, size, size];
                for(int x = 0;x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        for (int z = 0; z < size; z++)
                        {
                            array[x, y, z] = new Vector3(0, 0, 0);
                        }
                    }
                }

                return array;
            }
            

        }


        public VoxelGO RequestVoxelGo()
        {
            VoxelGO Go;

            if (GOChunkPool.TryTake(out Go) && poolsFilled)
                return Go;
            else
            {
                GameObject gameo = new GameObject();
                Go = gameo.AddComponent<VoxelGO>();

                Go.m_MeshCollider = gameo.AddComponent<MeshCollider>();
                Go.myGameObject = gameo;
                Go.m_Transform = gameo.transform;
                Go.m_Transform.parent = parent;
                Go.sourceTag = gameo.AddComponent<NavMeshSourceTag>();
                Go.tag = parent.tag;
                Go.mesh = new Mesh();
                Go.mesh.MarkDynamic();
                gameo.SetActive(false);
                if (poolsFilled)
                    Debug.Log("Added new Go" + GOChunkPool.Count);
            }
            return Go;

        }
        public VoxelChunk CreateChunk(Vector3Int Pos,int lod,bool UpperLevel)
        {


            //check if the chunk already exists if not create a chunk with x *y * z of voxels



                //set variables for chunk creation
                VoxelChunk Chunk;
                // WaterChunk WChunk;


                if (ChunkPool.TryTake(out Chunk))
                    Chunk.RegenChunk(lod,Pos.x, Pos.y, Pos.z, UpperLevel);
                else
                {
                    memoryInUse++;

                    Chunk = new VoxelChunk(Pos.x, Pos.y, Pos.z, lod, UpperLevel);

                }

            return Chunk;

            
            }
        public parentObject RequestParentObj(int x, int y, int z, int size, int Lev)
        {
            parentObject ParentObj;
            if (ParentObjPool.TryDequeue(out ParentObj))
                ParentObj.regen(x, y, z,this, size, Lev);
            else
            {

                ParentObj = new parentObject(x, y, z,this, size, Lev);

            }
            return ParentObj;
        }
        public void FillPoolsVoxelGo()
        {
            VoxelGO Go;
            Debug.Log("Filled Pools");
            for (int i = 0; i < 500; i++)
            {
                GameObject gameo = new GameObject();
                Go = gameo.AddComponent<VoxelGO>();
                Go.m_MeshCollider = gameo.AddComponent<MeshCollider>();
                Go.myGameObject = gameo;
                Go.m_Transform = gameo.transform;
                Go.m_Transform.parent = parent;
                Go.sourceTag = gameo.AddComponent<NavMeshSourceTag>();
                Go.tag = parent.tag;
                Go.mesh = new Mesh();
                Go.mesh.MarkDynamic();
                gameo.SetActive(false);
                GOChunkPool.Add(Go);
            }

            Debug.Log("Filled Pools" + GOChunkPool.Count);

        }
        public void FillPools()
        {
            for (int i = 0; i < 200; i++)
            {
                Voxels[,,] array = new Voxels[m_voxelWidth+5, m_voxelWidth+5, m_voxelWidth+5];
                arrays.Add(array);

            }
            Debug.Log("Filled Pools" + arrays.Count);
            FillChunkpool();

        }
        public void FillChunkpool()
        {
            for (int i = 0; i < 2500; i++)
            {
                VoxelChunk chunk = new VoxelChunk( 0, 0, 0, 0, false);
                ChunkPool.Add(chunk);
            }
            Debug.Log("Filled Chunk Pools" + ChunkPool.Count);
            poolsFilled = true;

        }




        //main method for calling chunk creation some optimizations could be done here
        void GenerateTerrains()
        {
            while (CanContinue)
            {
                try
                {
                    if (!poolsFilled)
                        FillPools();
                    if (cansave == true)
                        SaveTerrain();
                        CheckChunks();
                    //canCheck = false;
                    int lod = minLod;
                    //basic routine for creating chunks 
                    int widthLength = m_voxelWidth * TriSize[maxLod];
                    widthLength *= 2;


                    Thread.Sleep(1);
                    int distToload = Mathf.Clamp(distanceToload, 2, 6);
                    for (int i = 0; i < distToload * widthLength; i += widthLength)
                    {
                        if (CanContinue)
                        {


                            for (int p = 0; p < Player.Count; p++)
                            {
                                Vector2 Ppos = Player[p];
                                //set player positon relative to chunks basically rounding to chunk size
                                for (int x = (int)Ppos.x - i; x < (int)Ppos.x + i; x += widthLength)
                                {
                                    if (CanContinue)
                                    {
                                        for (int z = (int)Ppos.y - i; z < (int)Ppos.y + i; z += widthLength)
                                        {
                                            if (CanContinue)
                                            {
                                                //this is for the voxel editing at runtime 
                                                //if theres any new chunks that need to be updated they are added here




                                                //distance checking
                                                Vector3Int POS = new Vector3Int(x, 0, z);
                                                if (!m_voxelChunk.ContainsKey(POS))
                                                {
                                                    parentObject pObj = new parentObject(x, 0, z, this, widthLength, maxLod);
                                                    pObj.checklod();
                                                    m_voxelChunk.Add(POS, pObj);
                                                    children.Add(pObj);

                                                }



                                                /*     else if(m_voxelChunk[x,z].HasMesh && m_voxelChunk[x,z].chunkProccessed==true){

                   if(lod!=m_voxelChunk[x,z].lod){
                    m_voxelChunk[x,z].RegenChunk(lod,m_voxelWidth , m_voxelHeight,0);
                    m_voxelChunk[x,z].chunkY1.RegenChunk(lod,m_voxelWidth , m_voxelHeight,1);
                   RegenChunks.Enqueue( m_voxelChunk[x,z]);
                    RegenChunks.Enqueue(m_voxelChunk[x,z].chunkY1);

                                                          }
                 }*/



                                            }
                                            else break;
                                        }
                                    }
                                    else break;
                                }
                            }
                        }
                        else break;
                    }

                    /*     time += 0.01f;
                     if (time >= 4)
                     {
                         checkWater();
                         time = 0;
                     }*/
                    Thread.Sleep(2);
                    //canCheck = true;


                }

                catch (Exception e)
                {
                    Debug.LogError(e.StackTrace + e.Message);
                }
            }
        }
        public void OnApplicationQuit(){
   CanContinue = false;
            Exit();
  }
		public void Exit(){
			//set flag to stop generation
            SaveTerrain();
			CanContinue=false;
            try
            {
                for (int i = 0; i < Threads.Length; i++)
                {
                    Threads[i].Interrupt();
                    Threads[i].Abort();
                    Threads[i].Join();
                    
                    Debug.Log("thread aborted " + Threads[i].IsAlive);
                }

                VoxelGenerator.Interrupt();
                VoxelGenerator.Abort();
                VoxelGenerator.Join();
                
                Debug.Log("Thread Ended " + VoxelGenerator.IsAlive);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
            
            
		}

	}
}