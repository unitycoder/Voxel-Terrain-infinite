using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using VoxelEngine;
public class SurfaceNets
{

	public byte threshold =120;	



	/*
	 * Give each voxel a position, an index, a corner mask, and an edge mask.
	 * isOnSurface basically asks if our mesh goes through this voxel
	 */


	public struct sample
	{
		public Vector3 point;
		public byte value;
	}

	// create an array of voxels that'll fill our sample space


	// Define the edge table and cube edges with a set size
	private int[] edgeTable = new int[256];
	private int[] cubeEdges = new int[24];

	/* 
	 * Set up the 8 corners of each voxel like this:
	 * Very important that this is set up the same way as the cube edges
	 * 
	 *  y         z
	 *  ^        /     
	 *  |
	 *    6----7
	 *   /|   /|
	 *  4----5 |
	 *  | 2--|-3
	 *  |/   |/
	 *  0----1   --> x
	 * 
	 */

	private Vector3[] voxelCornerOffsets = new Vector3[8] {
		new Vector3(0,0,0),		// 0
		new Vector3(1,0,0), 	// 1
		new Vector3(0,1,0), 	// 2
		new Vector3(1,1,0), 	// 3
		new Vector3(0,0,1), 	// 4
		new Vector3(1,0,1), 	// 5
		new Vector3(0,1,1), 	// 6
		new Vector3(1,1,1)  	// 7
	};



	/*
	 * Build an intersection table. This is a 2^(cube config) -> 2^(edge config) map
	 * There is only one entry for each possible cube configuration
	 * and the output is a 12-bit vector enumerating all edges
	 * crossing the 0-level
	 */

	void GenerateIntersectionTable()
	{

		for (int i = 0; i < 256; ++i)
		{
			int em = 0;
			for (int j = 0; j < 24; j += 2)
			{
				var a = Convert.ToBoolean(i & (1 << cubeEdges[j]));
				var b = Convert.ToBoolean(i & (1 << cubeEdges[j + 1]));
				em |= a != b ? (1 << (j >> 1)) : 0;
			}
			edgeTable[i] = em;
		}
	}

	/* 
	 * Utility function to build a table of possible edges for a cube with each
	 * pair of points representing one edge i.e. [0,1,0,2,0,4,...] would be the 
	 * edges from points 0 to 1, 0 to 2, and 0 to 4 respectively:
	 * 
	 *  y         z
	 *  ^        /     
	 *  |
	 *    6----7
	 *   /|   /|
	 *  4----5 |
	 *  | 2--|-3
	 *  |/   |/
	 *  0----1   --> x
	 * 
	 */

	void GenerateCubeEdgesTable()
	{
		int k = 0;
		for (int i = 0; i < 8; ++i)
		{
			for (int j = 1; j <= 4; j <<= 1)
			{
				int p = i ^ j;
				if (i <= p)
				{
					cubeEdges[k++] = i;
					cubeEdges[k++] = p;
				}
			}
		}
	}
	public SurfaceNets()
    {
		GenerateCubeEdgesTable();
		GenerateIntersectionTable();
	}
	/*
	 * Not necessary but let's draw a wire cube mesh around our sample space just
	 * to help us visualize it
	 */
	public struct Voxel
	{
		public int vertexIndex;
		public Vector3 vertexPosition;
		public bool isOnSurface;
		public int voxelEdgeMask;
		public int cornerMask;
	}


	// Set up the size of each list of voxels, verticies, triangles
	/*void Awake()
	{
		voxels = new Voxel[(int)sampleResolution.x, (int)sampleResolution.y, (int)sampleResolution.z];
		vertices = new List<Vector3>();
		newVertices = new List<Vector3>();
		triangles = new List<int>();
	}

	
	 * this doens't have to implement at run time, you can make 
	 * a custom editor for Unity but we won't get into that here
	 
	void Start()
	{
		GenerateCubeEdgesTable();
		GenerateIntersectionTable();
		CalculateVertexPositions();
		GenerateFieldMesh();
	}
	*/
	/*
	 * SampleField: Takes in a vector3 from a voxel and looks at every child object
	 * and returns the smallest distance from each possible object.
	 */

	






	/** 
	 * calculate the postion of each vertex. this sets up our 3 dimensional grid of voxels
	 * while also sampling each voxel
	 **/
	//public void CreateVertices(byte[,,] voxels, List<Vector3> verts, List<int> index, Vector3 m_pos, int start, int end, int lod, bool isUpperLevel, VoxelChunk chunk)
	public void CalculateVertexPositions(byte[,,] voxelsB,List<Vector3>Verts,List<int>Tris, Vector3 worldpos,int start,int end,int lod,bool s , VoxelChunk chunk,Vector3[,,] m_normals)
	{
		Voxel[,,] voxels;
	int size = VoxelTerrainEngine.Generator.m_voxelWidth+5;
		voxels = new Voxel[size, size, size];
		List<Vector3> vertices = new List<Vector3>();
		int multi = VoxelTerrainEngine.TriSize[lod];
		sample[] samples = new sample[8];
		for (int x = 0; x < size-1; x++)
		{
			for (int y = 0; y < size -1; y++)
			{
				for (int z = 0; z < size -1; z++)
				{
					if (x > 0 && y > 0 && z > 0)
					{
						float dx = voxelsB[x + 1, y, z] - voxelsB[x - 1, y, z];
						float dy = voxelsB[x, y + 1, z] - voxelsB[x, y - 1, z];
						float dz = voxelsB[x, y, z + 1] - voxelsB[x, y, z - 1];

						m_normals[x, y, z] = Vector3.Normalize(new Vector3(dx, dy, dz));
					}
					// default values.
					voxels[x, y, z].isOnSurface = false;
					voxels[x, y, z].voxelEdgeMask = 0;
					voxels[x, y, z].vertexIndex = -1;
					voxels[x, y, z].cornerMask = 0;

					int cornerMask = 0;

					// sample the 8 corners for the voxel and create a corner mask
					
					for (int i = 0; i < 8; i++)
					{
						var offset = voxelCornerOffsets[i];
						var pos = new Vector3(x,y,z)+ voxelCornerOffsets[i];
						var sample = voxelsB[x+(int)offset.x, y + (int)offset.y, z + (int)offset.z];
						samples[i].value = sample;
						samples[i].point = pos;
						cornerMask |= ((sample >= threshold) ? (1 << i) : 0);
					}

					//Check for early termination if cell does not intersect boundary
					if (cornerMask == 0 || cornerMask == 0xff)
					{
						continue;
					}

					// get edgemask from table using our corner mask
					int edgeMask = edgeTable[cornerMask];
					int edgeCrossings = 0;
					var vertPos = Vector3.zero;

					for (int i = 0; i < 12; ++i)
					{

						//Use edge mask to check if it is crossed
						if (!((edgeMask & (1 << i)) > 0))
						{
							continue;
						}

						//If it did, increment number of edge crossings
						++edgeCrossings;

						//Now find the point of intersection
						int e0 = cubeEdges[i << 1];
						int e1 = cubeEdges[(i << 1) + 1];
						float g0 = samples[e0].value;
						float g1 = samples[e1].value;
						float t = (threshold - g0) / (g1 - g0);

						vertPos += Vector3.Lerp(samples[e0].point, samples[e1].point, t);
					}
					vertPos /= edgeCrossings;

					voxels[x, y, z].vertexPosition = vertPos;
					voxels[x, y, z].isOnSurface = true;
					voxels[x, y, z].voxelEdgeMask = edgeMask;
					voxels[x, y, z].vertexIndex = vertices.Count;
					voxels[x, y, z].cornerMask = cornerMask;
					vertices.Add(vertPos);
				}
			}
		}
		ComputeMesh(voxels,Verts, Tris,lod);
	}


	public void ComputeMesh(Voxel[,,] voxels, List<Vector3>verts,List<int> tris,int lod)
	{
		// set the size of our buffer
		int[] buffer = new int[4096];
		int multi = VoxelTerrainEngine.TriSize[lod];
		// get the width, height, and depth of the sample space for our nested for loops
		int width = voxels.GetUpperBound(0) - 1;
		int height = voxels.GetUpperBound(1) - 1;
		int depth = voxels.GetUpperBound(2) - 1;

		int n = 0;
		int[] pos = new int[3];
		int[] R = new int[] { 1, width + 1, (width + 1) * (height + 1) };
		float[] grid = new float[8];
		int bufferNumber = 1;

		// resize the buffer if it's not big enough
		if (R[2] * 2 > buffer.Length)
			buffer = new int[R[2] * 2];

		for (pos[2] = 0; pos[2] < depth - 1; pos[2]++, n += width, bufferNumber ^= 1, R[2] = -R[2])
		{
			var bufferIndex = 1 + (width + 1) * (1 + bufferNumber * (height + 1));

			for (pos[1] = 0; pos[1] < height - 1; pos[1]++, n++, bufferIndex += 2)
			{
				for (pos[0] = 0; pos[0] < width - 1; pos[0]++, n++, bufferIndex++)
				{
					// get the corner mask we calculated earlier
					var mask = voxels[pos[0], pos[1], pos[2]].cornerMask;

					// Early Termination Check
					if (mask == 0 || mask == 0xff)
					{
						continue;
					}

					// get edge mask
					var edgeMask = edgeTable[mask];

					var vertex = new Vector3();
					var edgeIndex = 0;

					//For Every Cube Edge
					for (var i = 0; i < 12; i++)
					{
						//Use Edge Mask to Check if Crossed
						if (!Convert.ToBoolean(edgeMask & (1 << i)))
						{
							continue;
						}

						//If So, Increment Edge Crossing #
						edgeIndex++;

						//Find Intersection Point
						var e0 = cubeEdges[i << 1];
						var e1 = cubeEdges[(i << 1) + 1];
						var g0 = grid[e0];
						var g1 = grid[e1];
						var t = g0 - g1;
						if (Math.Abs(t) > 1e-16)
							t = g0 / t;
						else
							continue;

						//Interpolate Vertices, Add Intersections
						for (int j = 0, k = 1; j < 3; j++, k <<= 1)
						{
							var a = e0 & k;
							var b = e1 & k;
							if (a != b)
								vertex[j] += Convert.ToBoolean(a) ? 1f - t : t;
							else
								vertex[j] += Convert.ToBoolean(a) ? 1f : 0;
						}
					}

					//Average Edge Intersections, Add to Coordinate
					var s = 1f / edgeIndex;
					for (var i = 0; i < 3; i++)
					{
						vertex[i] = pos[i] + s * vertex[i];
					}
					vertex = voxels[pos[0], pos[1], pos[2]].vertexPosition;

					//Add Vertex to Buffer, Store Pointer to Vertex Index
					buffer[bufferIndex] = verts.Count;
					verts.Add(vertex* multi);

					//Add Faces (Loop Over 3 Base Components)
					for (var i = 0; i < 3; i++)
					{
						//First 3 Entries Indicate Crossings on Edge
						if (!Convert.ToBoolean(edgeMask & (1 << i)))
						{
							continue;
						}

						//i - Axes, iu, iv - Ortho Axes
						var iu = (i + 1) % 3;
						var iv = (i + 2) % 3;

						//Skip if on Boundary
						if (pos[iu] == 0 || pos[iv] == 0)
							continue;

						//Otherwise, Look Up Adjacent Edges in Buffer
						var du = R[iu];
						var dv = R[iv];

						//Flip Orientation Depending on Corner Sign
						if (Convert.ToBoolean(mask & 1))
						{
							tris.Add(buffer[bufferIndex]);
							tris.Add(buffer[bufferIndex - du - dv]);
							tris.Add(buffer[bufferIndex - du]);
							tris.Add(buffer[bufferIndex]);
							tris.Add(buffer[bufferIndex - dv]);
							tris.Add(buffer[bufferIndex - du - dv]);
						}
						else
						{
							tris.Add(buffer[bufferIndex]);
							tris.Add(buffer[bufferIndex - du - dv]);
							tris.Add(buffer[bufferIndex - dv]);
							tris.Add(buffer[bufferIndex]);
							tris.Add(buffer[bufferIndex - du]);
							tris.Add(buffer[bufferIndex - du - dv]);
						}
					}

				}
			}
		}

	}




}