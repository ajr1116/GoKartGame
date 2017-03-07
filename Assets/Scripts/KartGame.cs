using UnityEngine;
using System.Collections;

public class KartGame : MonoBehaviour {

    //blocco percorsi
    public GameObject _cubeLastLap;
    public GameObject _cubeFirstLap;

	void Start () 
    {
        _cubeLastLap.SetActive(false);
        _cubeFirstLap.SetActive(true);
	}
	
    void LastLap()
    {
        _cubeLastLap.SetActive(true);
        _cubeFirstLap.SetActive(false);
    }
}
