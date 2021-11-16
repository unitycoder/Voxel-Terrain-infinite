using UnityEngine;
using System.Collections;
using System.Threading;
using System.Collections.Generic;

public class bendPlant : MonoBehaviour {
	public Terrain[]terrains;
	public AudioClip clip;
	public Thread thread;
	bool check;
	public Vector3 playerPosition;
	public Transform player;
	public List<GameObject> triggers = new List<GameObject>();
	public List<Vector3> pos = new List<Vector3>();
	public List<Vector3> soundpos = new List<Vector3>();
	bool play;
	public int current;
	public float dist;
	public GameObject trigger;
	public List<int> number = new List<int>();
	void Start(){
		check = true;
		player = Camera.main.transform;
			thread = new Thread(DistanceCheck);
		thread.IsBackground = true;
		thread.Priority = System.Threading.ThreadPriority.Lowest;
		thread.Start();
	}

	void Update(){
		if(triggers.Count<soundpos.Count)
			for(int i = 0; i < soundpos.Count-triggers.Count;i++){
			if(triggers.Count<soundpos.Count){
				GameObject gameo = Instantiate(trigger,Vector3.zero,Quaternion.identity)as GameObject;
			triggers.Add(gameo);
		}
		}
	playerPosition = player.position;
		for(int i = 0;i < soundpos.Count;i++){
		triggers[i].transform.position = soundpos[i];
		if(number[i]/50000>0)
				triggers[i].transform.localScale = terrains[number[i]/50000].terrainData.treePrototypes[terrains[number[i]/50000].terrainData.GetTreeInstance(number[i]/((50000*(number[i]/50000))/50000)-50000).prototypeIndex].prefab.GetComponent<MeshFilter>().sharedMesh.bounds.size/2 ;
		else 
				triggers[i].transform.localScale = terrains[number[i]/50000].terrainData.treePrototypes[terrains[number[i]/50000].terrainData.GetTreeInstance(number[i]).prototypeIndex].prefab.GetComponent<MeshFilter>().sharedMesh.bounds.size/2 ;
	}
	}
	void DistanceCheck(){
		Debug.Log("started");
		while(check){
			for(int i =0;i < pos.Count;i++){
				current = i;
				if(Vector3.Distance(pos[i],playerPosition+(Vector3.down*2))<=dist){
					if(!soundpos.Contains(pos[i])){
					soundpos.Add( pos[i]);
					number.Add(i);
					break;
					}
					}

			}
			for(int i = 0;i < soundpos.Count;i++){
				if(Vector3.Distance(soundpos[i],playerPosition+(Vector3.down*2))>dist+5)
					soundpos.Remove(soundpos[i]);
					number.Remove(i);
			}
		}
	}

	void OnApplicationQuit(){
		check = false;
		if(thread!=null)
		thread.Abort();
	}

}