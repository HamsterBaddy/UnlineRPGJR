using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://youtu.be/zit45k6CUMk
public class Parallax : MonoBehaviour
{
	private float length, startpos;
	public GameObject cam;
	public float parallaxEffect;

    // Start is called before the first frame update
    void Start()
    {
		startpos = transform.position.x;
		length = GetComponent<SpriteRenderer>().bounds.size.x;
		Debug.Log(length);
    }

    // Update is called once per frame
    void Update()
    {
		float temp = (cam.transform.position.x * (1 - parallaxEffect));
		float dist = (cam.transform.position.x * parallaxEffect);

		transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

		if (temp > startpos + length) startpos += length;
		else if (temp < startpos - length) startpos -= length;
    }
}
