using UnityEngine;
using System.Collections;

public class EndGame : MonoBehaviour {

    void Update()
    {
        if (Time.timeScale==0)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Application.LoadLevel("testmacchina");
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        Time.timeScale = 0;
        //GUI fine gioco
    }
}
