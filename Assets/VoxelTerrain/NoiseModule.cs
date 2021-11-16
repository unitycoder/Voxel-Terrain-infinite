using UnityEngine;
using System.Collections;
using VoxelEngine;
[SerializeField]
public class NoiseModule : MonoBehaviour
{
	// Use this for initialization
	public float valueX;
	public float Frequency;
	public float FrequencyThreeD;
	public float CaveFrequency;
	public float GroundFrequency;
	public int oct;
	public int caveOct;
	public int GroundOct;
	public float amplitude;
	public float AmplitudeThreeD;
	public float caveAmplitude;
	public float GroundAmplitude;
	public bool check;
	public float biomeFrequency;
	public float biomeAmp;
	public float SampleMountains(float x, float z, PerlinNoise perlin)
	{
		//This creates the noise used for the mountains. It used something called 
		//domain warping. Domain warping is basically offseting the position used for the noise by
		//another noise value. It tends to create a warped effect that looks nice.
		//Clamp noise to 0 so mountains only occur where there is a positive value
		//The last value (32.0f) is the amp that defines (roughly) the maximum mountaion height
		//Change this to create high/lower mountains

		return perlin.FractalNoise2D(x, z, oct, Frequency, amplitude);
	}

	public float SampleGround(float x, float z, PerlinNoise perlin)
	{
		//This creates the noise used for the ground.
		//The last value (8.0f) is the amp that defines (roughly) the maximum 
		return -Mathf.Abs(perlin.FractalNoise2D(x, z, GroundOct, GroundFrequency, GroundAmplitude));
	}
	//not cave noise just normal noise now as it needed a noise with another seed
	public float SampleCaves(float x, float z, PerlinNoise perlin)
	{
		//larger caves (A higher frequency will also create larger caves). It is unitless, 1 != 1m
		return perlin.FractalNoise2D(x, z, caveOct, CaveFrequency, caveAmplitude);

	}
	public float Sample3d(float x, float y, float z)
	{
		//larger caves (A higher frequency will also create larger caves). It is unitless, 1 != 1m

		return VoronoiNoise.FractalNoise3D(x, y, z, oct, Frequency, amplitude, VoxelTerrainEngine.Generator.m_surfaceSeed); ;

	}

	public float FillVoxel2d(float x, float z, PerlinNoise noise)
	{
		//float startTime = Time.realtimeSinceStartup;

		//Creates the data the mesh is created form. Fills m_voxels with values between -1 and 1 where
		//-1 is a soild voxel and 1 is a empty voxel.


		float HT = SampleGround(x, z, noise) - SampleMountains(x, z, noise);
		//if(HT<valueX)
		// HT *= SampleCaves(x,z,noise);
		//else HT /= SampleCaves(x, z, noise);
		if (check)
			Debug.Log(HT);





		return HT;


	}
	public float FillVoxel3d(float x, float y, float z, PerlinNoise noise)
	{
		//float startTime = Time.realtimeSinceStartup;

		//Creates the data the mesh is created form. Fills m_voxels with values between -1 and 1 where
		//-1 is a soild voxel and 1 is a empty voxel.


		float HT = noise.FractalNoise3D(x, y, z, 1, FrequencyThreeD, AmplitudeThreeD);

		//else HT /= SampleCaves(x, z, noise);
		if (check)
			Debug.Log(HT);





		return HT;


	}
}
