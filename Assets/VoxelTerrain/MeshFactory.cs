using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine.Rendering;
using Unity.Collections;
namespace VoxelEngine
{
	public class MeshFactory
	{
		public static VoxelTerrainEngine generator;
		public static PerlinNoise SurfacePerlin;
		public static MarchingCubes MarchingCubes;
		public static float m_surfaceLevel;

		//calculate normals for the mesh 
		public void CalculateNormals(Voxels[,,] m_voxels, int lod, int size, NativeList<Vector3> normals, NativeList<Vector3> Verts)
		{

			if (m_voxels != null)
			{

				Vector3[,,] m_normals;
				//This calculates the normal of each voxel. If you have a 3d array of data
				//the normal is the derivitive of the x, y and z axis.
				//Normally you need to flip the normal (*-1) but it is not needed in this case.
				//If you dont call this function the normals that Unity generates for a mesh are used.


				int width = generator.m_voxelWidth + 5;
				m_normals = generator.RequestNormalArray(width);
				int multi = VoxelTerrainEngine.TriSize[lod];

				for (int x = 1; x < width - 1; x++)
				{
					for (int z = 1; z < width - 1; z++)
					{
						for (int y = 1; y < width - 1; y++)
						{

							float dx = m_voxels[x + 1, y, z].VoxelValue - m_voxels[x - 1, y, z].VoxelValue;
							float dy = m_voxels[x, y + 1, z].VoxelValue - m_voxels[x, y - 1, z].VoxelValue;
							float dz = m_voxels[x, y, z + 1].VoxelValue - m_voxels[x, y, z - 1].VoxelValue;

							m_normals[x, y, z].Set(dx, dy, dz);
							Vector3 pos = Vector3.Normalize(m_normals[x, y, z]);
							m_normals[x, y, z].Set(pos.x, pos.y, pos.z);
						}
					}
				}



				for (int i = 0; i < size; i++)
				{
					Vector3 pos = Verts[i] / multi;
					int x = (int)pos.x;
					int y = (int)pos.y;
					int z = (int)pos.z;

					float fx = pos.x - x;
					float fy = pos.y - y;
					float fz = pos.z - z;

					Vector3 x0 = m_normals[x, y, z] * (1.0f - fx) + m_normals[x + 1, y, z] * fx;
					Vector3 x1 = m_normals[x, y, z + 1] * (1.0f - fx) + m_normals[x + 1, y, z + 1] * fx;

					Vector3 x2 = m_normals[x, y + 1, z] * (1.0f - fx) + m_normals[x + 1, y + 1, z] * fx;
					Vector3 x3 = m_normals[x, y + 1, z + 1] * (1.0f - fx) + m_normals[x + 1, y + 1, z + 1] * fx;

					Vector3 z0 = x0 * (1.0f - fz) + x1 * fz;
					Vector3 z1 = x2 * (1.0f - fz) + x3 * fz;

					normals.Add(z0 * (1.0f - fy) + z1 * fy);
				}
				generator.NormalsArr.Add(m_normals);
			}

		}

		//interpolate normals so normals are smoothed





		public bool createWater(byte[,,] m_Water, byte[,,] m_voxels, int lod, Vector3 m_pos)
		{
			int multi = VoxelTerrainEngine.TriSize[lod];
			int w = m_voxels.GetLength(0);
			int h = m_voxels.GetLength(1);
			int l = m_voxels.GetLength(2);
			for (int x = 0; x < w; x++)
			{

				for (int z = 0; z < l; z++)
				{
					for (int y = 0; y < h; y++)
					{
						if ((y * multi) + m_pos.y < generator.SnowLine)
						{

							byte water = 255;

							if (m_voxels[x, y, z] > 125)
								water = 0;
							if (m_voxels[x, y, z] <= 125)
								water = 255;

							m_Water[x, y, z] = water;
						}
						else m_Water[x, y, z] = 255;
					}
				}
			}
			return true;

		}


		//function to create the voxel noises and caves etc.
		public bool CreateVoxels(ref Voxels[,,] m_voxels, Vector3 m_pos, NoiseModule noiseModule, int lod, VoxelChunk chunk, bool isFull)
		{
			//float startTime = Time.realtimeSinceStartup;
			bool hasEmpty = false;
			bool hasVoxels = false;
			isFull = true;
			//Creates the data the mesh is created form. Fills m_voxels with values between -1 and 1 where
			//-1 is a soild voxel and 1 is a empty voxel.
			if (chunk.Voxels != null)
			{
				int w = generator.m_voxelWidth + 5;
				int h = generator.m_voxelWidth + 5;
				int l = generator.m_voxelWidth + 5;
				float worldX;
				float worldZ;
				float worldY;
				int multi = VoxelTerrainEngine.TriSize[lod];
				float ht;
				///matarial minus one
				int MTM = 0;
				float VTM = 0;
				int mh = (generator.m_voxelWidth * VoxelTerrainEngine.TriSize[generator.maxLod]);
				mh *= 2;
				mh -= 5;
				float HT = 0;
				if (noiseModule != null)
				{

					for (int x = 0; x < w; x++)
					{

						for (int z = 0; z < l; z++)
						{

							worldX = (x * multi) + m_pos.x;
							worldZ = (z * multi) + m_pos.z;
							//float D = (SimplexNoise.Generate(worldX * 1.0f / 700.0f, 0f, worldZ * 1.0f / 950.0f));
							//worldX += (D * 40.0f);
							//worldZ += (D * 60.0f);
							float biome = generator.m_surfacePerlin.FractalNoise2D(worldX, worldZ, 1, noiseModule.biomeFrequency, noiseModule.biomeAmp);
							if (biome > 0.5f)
								biome = 0.5f;
							else if (biome < -0.5f)
								biome = -0.5f;
							biome += 0.5f;
							biome *= 5;
							/*if (x >= 1)
							{

								Biomes[x - 1, z] = Biomes[x, z];


							}

							if (z >= 1)
							{
								Biomes[x , z - 1] = Biomes[x, z];

							}
							if (x >= 1 && z >= 1)
							{
								Biomes[x - 1, z - 1] = Biomes[x, z];

							}*/
							ht = 0.0f;
							ht = generator.noise.FillVoxel2d(worldX, worldZ, SurfacePerlin);


							for (int y = 0; y < h; y++)
							{

								float MT = 0.0f;
								worldY = (y * multi) + m_pos.y;
								//worldY += (SimplexNoise.Generate(worldZ * 1.0f / 950.0f, worldY * 1.0f / 190.0f, worldX * 1.0f / 850.0f) * 33.0f);
								if (worldY > 5 && worldY < mh)
								{
									HT = (ht + worldY) - generator.offset;
									//HT += generator.noise.FillVoxel3d(worldX, worldY, worldZ, SurfacePerlin);
									int b = Mathf.RoundToInt(biome);

									if (worldY > generator.SnowLine)
										m_voxels[x, y, z].Biome = (byte)3;
									else if (b != 3)
										m_voxels[x, y, z].Biome = (byte)b;
									else m_voxels[x, y, z].Biome = (byte)0;


									HT /= 10.0f;
									if (HT > 0.5f)
										HT = 0.5f;
									else if (HT < -0.5f)
										HT = -0.5f;

									HT += 0.5f;

									//MT = HT;
									HT *= 255.0f;
									//MT *= 7;
									//MT = (int)MT;






									if (y > 0)
									{
										MTM = m_voxels[x, y - 1, z].Material;
										VTM = m_voxels[x, y - 1, z].VoxelValue;
									}





									////implement algorithm where it changes direction randomly using x y z to traverse array in a cave way

									if (MT == 4.0f || MT == 5.0 || MT == 6.0f || MT == 7.0f)
										MT = 0;



									if (generator.MakeCaves)
									{
										float cave = 0.0f;

										cave = SimplexNoise.Generate(worldX * 1.0f / 420.0f, worldY * 1.0f / 90.0f, worldZ * 1.0f / 590.0f);//(generator.m_surfacePerlin.FractalNoise3D(worldX, worldY , worldZ, 1, 30, 1.0f));
										if (m_voxels[x, y, z].Biome == 5 && HT <= 70.0f)
											cave += SimplexNoise.Generate(worldX * 1.0f / 60.0f, worldY * 1.0f / 30.0f, worldZ * 1.0f / 60.0f);//(generator.m_surfacePerlin.FractalNoise3D(worldX, worldY , worldZ, 1, 25, 1.0f));

										cave /= 6;
										if (cave > 0.5f)
											cave = 0.5f;
										else if (cave < -0.5f)
											cave = -0.5f;
										cave += 0.5f;
										cave *= 210.0f;
										HT += cave;

									}
									if (chunk.keepData == false)
									{
										if (y > 0 && MTM != 8 && HT > 145)
										{

											if (VTM <= 145.0f)
												m_voxels[x, y - 1, z].Material = (int)VoxelType.grass;
										}
										if (y > 0 && MTM != 8 && MT == (float)VoxelType.grass && HT < 145.0f)
											if (VTM <= 145.0f)
												m_voxels[x, y - 1, z].Material = (int)VoxelType.Dirt;

									}

									if (HT < 180.0f)
									{
										float m = SimplexNoise.Generate(worldX * 1.0f / 60, worldY * 1.0f / 60, worldZ * 1.0f / 60);
										if (y > 0 && m * 2 > 1.5f && m < 2.0f)
										{
											HT = 0.0f;
											MT = 0.0f;
											m_voxels[x, y - 1, z].Material = 0;
											m_voxels[x, y - 1, z].VoxelValue = 0;
										}
										if (MT == 0.0f && HT < 40.0f)
										{
											if (m > 0.5f)
												m = 0.5f;
											else if (m < -0.5f)
												m = -0.5f;
											m += 0.5f;
											if (worldY < generator.offset / 3)
												m *= 7.0f;
											else if (worldY < generator.offset / 2)
												m *= 6.0f;
											else if (worldY < generator.offset)
												m *= 5.0f;




											m = Mathf.Clamp(m, 0.0f, 7.0f);
											MT = m;
											//MT = Mathf.Round(MT);
											if (Mathf.RoundToInt(MT) == (int)VoxelType.grass)
												MT = 0.0f;
										}

									}





								}
								if ((y * multi) + m_pos.y >= mh - 1)
								{
									HT = 255.0f;
									if (hasVoxels)
										hasEmpty = true;
									MT = (float)VoxelType.grass;
									m_voxels[x, y - 1, z].Material = (byte)(int)MT;
								}
								if ((y * multi) + m_pos.y <= 6.0f)
								{
									MT = 8.0f;
									HT = 0.0f;
									if (hasEmpty)
										hasVoxels = true;
								}
								if (HT > 123.0f)
									isFull = false;
								if (HT > 123.0f)
									hasEmpty = true;
								if (HT <= 125.0f)
									hasVoxels = true;



								m_voxels[x, y, z].VoxelValue = HT;
								m_voxels[x, y, z].Material = (byte)Mathf.RoundToInt(MT);


							}
						}
					}


				}
				else Debug.LogError("Noise Module Not detected Please add a noise module script to you terrain engine");
			}
			else Debug.Log("error");


			if (hasVoxels && hasEmpty)
				return true;
			else return false;

		}

	}
}
