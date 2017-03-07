using UnityEngine;
using System.Collections;

public class MyCarController : MonoBehaviour 
{
    //Collider ruote
    public WheelCollider _wheelFR;
    public WheelCollider _wheelFL;
    public WheelCollider _wheelBR;
    public WheelCollider _wheelBL;
    //Transform ruote
    public Transform _wheelFRTrans;
    public Transform _wheelFLTrans;
    public Transform _wheelBRTrans;
    public Transform _wheelBLTrans;

    public float _maxTorque = 400f; //potenza motore

    private float _speed = 0; //velocità macchina
    private float _currentSteer = 0; //sterzo
    private float _distance = 0f; //distanza percorsa in m

    //variabili private per funzionamento movimento
    private readonly float _highsteer = 40; //sterzo ad alte velocità
    private readonly float _lowsteer = 50; //sterzo a basse velocità
    private readonly float _decelleration = 10f; //potenza decelerazione

	void Start () 
    {
        //gameObject.GetComponent<Rigidbody>().centerOfMass += new Vector3(0f, -0.05f, 0f);//new Vector3(0f, -0.2f, 0f); //per migliore stabilità //gameObject.GetComponent<Transform>().localPosition; 
	}
	
	// Update is called once per frame
	void Update () 
    {
        
        //movimento grafico ruote
        _wheelFLTrans.Rotate(_wheelFL.rpm / 60f * 360f * Time.deltaTime, 0, 0);
        _wheelFRTrans.Rotate(_wheelFR.rpm / 60f * 360f * Time.deltaTime, 0, 0);
        _wheelBLTrans.Rotate(_wheelBL.rpm / 60f * 360f * Time.deltaTime, 0, 0);
        _wheelBRTrans.Rotate(_wheelBR.rpm / 60f * 360f * Time.deltaTime, 0, 0);
	}
    void FixedUpdate()
    {
         frenata(); //senza la collisione rallenta di pochissimo la velocità
         Speed = gameObject.GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
         Speed = Mathf.Round(Speed); //per non vedere decimali

            _wheelBR.motorTorque = _maxTorque * Input.GetAxis("Vertical"); //trazione posteriore
            _wheelBL.motorTorque = _maxTorque * Input.GetAxis("Vertical");
            _wheelFR.motorTorque = _maxTorque * Input.GetAxis("Vertical"); //trazione posteriore
            _wheelFL.motorTorque = _maxTorque * Input.GetAxis("Vertical");
         //_currentSteer = 50;
            _currentSteer = 20;
            if (Input.GetAxis("Horizontal") != 0f)
            {
                //_currentSteer = Mathf.Lerp(_lowsteer, _highsteer, Speed / 50); //se _speed 0 _lowsteer altrimenti _highsteer, ad alte velocità meno sterzo ruote
                _currentSteer *= Input.GetAxis("Horizontal");
                _wheelFR.steerAngle = _currentSteer;
                _wheelFL.steerAngle = _currentSteer;
            }
         Distance += (Speed / 3.6f) * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
    }

    void frenata()
    {
        //decelerazione
        if (Input.GetButton("Vertical") == false) //non tocchi freccia su o giù
        {
            if (Input.GetButton("Jump") == true) //premo anche lo spazio (più frenata perchè si aggiungono anche le frontali)
            {
                _wheelFL.brakeTorque = _decelleration; //piccolo slittamento
                _wheelFL.brakeTorque = _decelleration;
            }
            _wheelBL.brakeTorque = _decelleration;
            _wheelBR.brakeTorque = _decelleration;
        }
        else
        {
            _wheelBL.brakeTorque = 0;
            _wheelBR.brakeTorque = 0;
            _wheelFL.brakeTorque = 0;
            _wheelFL.brakeTorque = 0;
        }
    }
    //accessor
    public float Distance
    {
        get { return _distance; }
        set { _distance = value; }
    }
    public float Speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    public void OnGUI()
    {
        // Tachimetro
        string msg = "Speed: " + Speed.ToString("f0") + "Km/H";

        GUILayout.BeginArea(new Rect(Screen.width - 200 - 32, 32, 200, 40), GUI.skin.window);
        GUILayout.Label(msg);
        GUILayout.EndArea();
    }
}
