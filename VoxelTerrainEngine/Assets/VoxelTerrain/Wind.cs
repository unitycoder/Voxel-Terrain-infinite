using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class Wind : MonoBehaviour {
	public WindZone Zone;

	// Use this for initialization
	void Start () {
	Zone = GetComponent<WindZone>();
	}
	
	// Update is called once per frame
	void Update () {
		Shader.SetGlobalVector("windSpeed",new Vector4(Zone.windMain ,Zone.windTurbulence,Zone.windPulseMagnitude,Zone.windPulseFrequency ));
		Shader.SetGlobalVector("WindDirection",Zone.transform.forward);

	}
}
