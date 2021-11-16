using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoxelEngine;
public class addDeletevoxel : MonoBehaviour
{

	Ray ray;
	public LayerMask mask;
	public bool islocked;
	public GameObject block;
	public Camera cam;
	public int Distance;
	public int type;
	// Use this for initialization
	void Start()
	{
		cam = Camera.main;
		block = Instantiate(block, Vector3.zero, Quaternion.Euler(0, 0, 0)) as GameObject;
		block.SetActive(true);
	}

	// Update is called once per frame
	void Update()
	{
		ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
		RaycastHit hit;
		

		
		
				
				if (VoxelTerrainEngine.RaycastVoxels(ray, out hit, 5))
				{
			block.transform.position = hit.point + hit.normal;

			
			if (Input.GetButtonDown("Fire1"))
			{
				List<float> values = new List<float>();
				List<Vector3> positions = new List<Vector3>();
					Vector3 pos = hit.point;
					for (int x = (int)(pos.x - Distance); x < (int)(pos.x + Distance); x++)
					{
						for (int y = (int)(pos.y - Distance); y < (int)(pos.y + Distance); y++)
						{
							for (int z = (int)(pos.z - Distance); z < (int)(pos.z + Distance); z++)
							{
								Vector3 mpos = new Vector3(x, y, z);
								float d = (mpos - pos).magnitude;
								if (d <= Distance)
								{
										positions.Add(mpos);
								values.Add(Mathf.SmoothStep(0.0f,255.0f, (float)Distance/ d));

								}
							}
						}
					}
					VoxelTerrainEngine.SetVoxels(positions, values);
				
				}
		


		}
		if (Input.GetButtonDown("Fire2"))
		{
			if(Physics.Raycast(ray,out hit,10))
            {
                if (hit.collider.CompareTag("tree"))
                {
					Vector3 pos = hit.transform.position;
					VoxelChunk chunk = VoxelTerrainEngine.Generator.FindTreeChunk(hit.transform.parent.position);
					int T = chunk.treelist.Count;

					for (int i = 0; i < T; i++)
					{
						if ((chunk.treelist[i].treePos - pos).magnitude <= 0.1f)
						{
							VTree tree = chunk.treelist[i];
							tree.treeHealth = 0;
							//tree.treeHealth = 0;
							chunk.treelist[i] = tree;
							Debug.Log(chunk.treelist[i].treeHealth);
							chunk.DeleteTree(i);
							break;
						}

					}
				}
            }
		}
		}
}

