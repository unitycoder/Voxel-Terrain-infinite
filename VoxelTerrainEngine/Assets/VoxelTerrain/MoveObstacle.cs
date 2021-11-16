using UnityEngine;
using System.Collections;

public class MoveObstacle : MonoBehaviour {
	// Use this for initialization
	
	// Update is called once per frame
	void Update () {

		Shader.SetGlobalVector("_Obstacle",new Vector4(transform.position.x,transform.position.y,transform.position.z,0));
	}
}
