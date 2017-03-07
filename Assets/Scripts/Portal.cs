using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {
        //attiva i cubi dell'ultimo giro
        GameObject.Find("Manager").SendMessage("LastLap", SendMessageOptions.DontRequireReceiver);
    }
}
