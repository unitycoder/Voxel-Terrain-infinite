using UnityEngine;
using System.Collections;
using VoxelEngine;
namespace VoxelEngine{
public interface IGenerator{
	
		float FillVoxel2d(float x,float z, Vector3 m_pos);
}
}
