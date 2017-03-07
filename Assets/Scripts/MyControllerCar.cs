using UnityEngine;
using System.Collections;

public class MyControllerCar : MonoBehaviour {

    [SerializeField] private WheelCollider[] _whellColliders = new WheelCollider[4];
    [SerializeField] private Transform[] _wheelTransforms = new Transform[4];
    [SerializeField] private float _maximumSteerAngle = 25f; //da 25 fino a 45-50 (più alta velocità meno sterzo in teoria)
    [SerializeField] private float _torqueOverAllWheels = 2500f;
    [SerializeField] private float _reverseTorque = 500f;
    [SerializeField] private float maxHandbrakeTorque; //potenza freno massima, setto in start
    [SerializeField] private float _downforce = 100f; //non toccare
    [SerializeField] private float _topSpeed = 150f;
    [SerializeField] private float _slipLimit = 0.3f; //limite slittamento
    [SerializeField] private float _brakeTorque = 20000f; //sempre molto alto, più della torque dell'ordine della decina

    private Quaternion[] _initWheelRotations;
    private float _steerAngle;
    private float _currentTorque;
    private Rigidbody _rigidbody;
    public float CurrentSpeed { get { return _rigidbody.velocity.magnitude * 2.23693629f; } } //grande dubbio, ricontrolla su internet

    void Start()
    {
        maxHandbrakeTorque = float.MaxValue;
        _rigidbody = GetComponent<Rigidbody>();
        _currentTorque = 0;
    }

    //Posizione transform e collider wheels
    private void SetPositionWheels(int numberofwheels)
    {
        for (int i = 0; i < numberofwheels; i++)
        {
            Quaternion quat;
            Vector3 position;
            _whellColliders[i].GetWorldPose(out position, out quat);
            _wheelTransforms[i].position = position;
            _wheelTransforms[i].rotation = quat;
        }

    }


    public void Move(float steering, float accel, float footbrake, float handbrake)
    {

        SetPositionWheels(4); //se moto 2 per esempio

        //clamp valori sterzo, accelerazione, accelerazione e freno
        steering = Mathf.Clamp(steering, -1, 1);
        accel = Mathf.Clamp(accel, 0, 1);
        footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
        handbrake = Mathf.Clamp(handbrake, 0, 1);

        //setto alla 0 e alla 1 (frontali) lo sterzo
        _steerAngle = steering * _maximumSteerAngle;
        _whellColliders[0].steerAngle = _steerAngle;
        _whellColliders[1].steerAngle = _steerAngle;

        Driving(accel, footbrake); // passo sempre accelerazione, footbrake è il valore inverso
        CapSpeed(); //Velocità, se setti gui sfruttala

        //Se premo "Jump" setto freno, la 2 e la 3 sono le ruote posteriori
        if (handbrake > 0f)
        {
            float stop = handbrake * maxHandbrakeTorque;
            _whellColliders[2].brakeTorque = stop;
            _whellColliders[3].brakeTorque = stop;
        }

        AddDownForce();
        TractionControl();
    }

    private void Driving(float accel, float footbrake)
    {

        float torque;
        torque = accel * (_currentTorque / 4f);
        for (int i = 0; i < 4; i++) //assunte ruote 4
        {
            _whellColliders[i].motorTorque = torque; //4 ruote motrici, molto più stabile che solo posteriore
        }

        for (int i = 0; i < 4; i++)
        {
            if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, _rigidbody.velocity) < 50f) //Angle restituisce angolo in gradi tra i 2 vettori
            {
                _whellColliders[i].brakeTorque = _brakeTorque * footbrake;
            }
            else if (footbrake > 0) //vuol dire che stai andando in retro
            {
                _whellColliders[i].brakeTorque = 0f;
                _whellColliders[i].motorTorque = -_reverseTorque * footbrake;
            }
        }
    }

    private void CapSpeed()
    {
        float speed = _rigidbody.velocity.magnitude;
        speed *= 3.6f;
        if (speed > _topSpeed)
            _rigidbody.velocity = (_topSpeed / 3.6f) * _rigidbody.velocity.normalized;
    }

    //INTERNET per dare più grip...
    private void AddDownForce()
    {
        _whellColliders[0].attachedRigidbody.AddForce(-transform.up * _downforce * _whellColliders[0].attachedRigidbody.velocity.magnitude);
    }

    //controllo trazione, riduce potenza ruota se l'auto gira troppo
    private void TractionControl()
    {
        WheelHit wheelHit;
        for (int i = 0; i < 4; i++)
        {
            _whellColliders[i].GetGroundHit(out wheelHit);

            AdjustTorque(wheelHit.forwardSlip); //slittamento preumatico nella forward
        }

    }

    private void AdjustTorque(float forwardSlip)
    {
        if (forwardSlip >= _slipLimit && _currentTorque >= 0)
        {
            _currentTorque -= 10;
        }
        else
        {
            _currentTorque += 10;
            if (_currentTorque > _torqueOverAllWheels)
            {
                _currentTorque = _torqueOverAllWheels;
            }
        }
    }


    /*
     * PARTE FACOLTATIVA:
     * -SteerHelper per aiuto nelle curve ad alta velocità specialmente, ovviamente da richiamare nella Move. Valore migliore 0.644
     *  [NELLA DOCS: [Range(0, 1)] [SerializeField] private float SteerHelper; // 0 is raw physics , 1 the car will grip in the direction it is facing]
     * 
     * -Nella Start() _whellColliders[0].attachedRigidbody.centerOfMass = centreOfMassOffset;
     *  [SerializeField] private Vector3 centreOfMassOffset; //o invariato o leggermente più basso per stabilità
     * 
     * -Tolte le marce, se servono riprendere Bozza2
     */

    /*
    private void SteerHelper()
    {
        for (int i = 0; i < 4; i++)
        {
            WheelHit wheelhit;
            _whellColliders[i].GetGroundHit(out wheelhit);
            if (wheelhit.normal == Vector3.zero)
                return;
        }

        if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
        {
            float turnadjust = (transform.eulerAngles.y - m_OldRotation) * SteerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
        }
        m_OldRotation = transform.eulerAngles.y;
    }*/
}

/*
        BACKUP
 
 
 */

/* BOZZA1
    [SerializeField] private WheelCollider[] m_WheelColliders = new WheelCollider[4]; //4 wheelcollider
    [SerializeField] private GameObject[] m_WheelMeshes = new GameObject[4]; //4 meshes ruote
    [SerializeField] private Vector3 m_CentreOfMassOffset; //centro di massa
    [SerializeField] private float m_MaximumSteerAngle; //sterzo
    [Range(0, 1)] [SerializeField] private float m_SteerHelper; // 0 is raw physics , 1 the car will grip in the direction it is facing
    [SerializeField] private float m_FullTorqueOverAllWheels; 
    [SerializeField] private float m_ReverseTorque;
    [SerializeField] private float m_MaxHandbrakeTorque;
    [SerializeField] private float m_Downforce = 100f;
    [SerializeField] private float m_Topspeed = 200;
    [SerializeField] private static int NoOfGears = 5;
    [SerializeField] private float m_RevRangeBoundary = 1f;
    [SerializeField] private float m_SlipLimit;
    [SerializeField] private float m_BrakeTorque;

    private Quaternion[] m_WheelMeshLocalRotations;
    private Vector3 m_Prevpos, m_Pos;
    private float m_SteerAngle;
    private int m_GearNum;
    private float m_GearFactor;
    private float m_OldRotation;
    private float m_CurrentTorque;
    private Rigidbody m_Rigidbody;
    private const float k_ReversingThreshold = 0.01f;

    public bool Skidding { get; private set; }
    public float BrakeInput { get; private set; }
    public float CurrentSteerAngle { get { return m_SteerAngle; } }
    public float CurrentSpeed { get { return m_Rigidbody.velocity.magnitude * 2.23693629f; } }
    public float MaxSpeed { get { return m_Topspeed; } }
    public float Revs { get; private set; }
    public float AccelInput { get; private set; }

    // Use this for initialization
    void Start () 
    {
        m_WheelMeshLocalRotations = new Quaternion[4];
        //settaggio rotazione ruote dalle transform
        for (int i = 0; i < 4; i++)
        {
            m_WheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
        }
        //settaggio centro di massa
        m_WheelColliders[0].attachedRigidbody.centerOfMass = m_CentreOfMassOffset;

        m_MaxHandbrakeTorque = float.MaxValue;

        m_Rigidbody = GetComponent<Rigidbody>();
        m_CurrentTorque = m_FullTorqueOverAllWheels - m_FullTorqueOverAllWheels;
    }

    private void SetPositionWheels(int numberofwheels)
    {
        for (int i = 0 ; i < numberofwheels; i++)
        {
            Quaternion quat;
            Vector3 position;
            m_WheelColliders[i].GetWorldPose(out position, out quat);
            m_WheelMeshes[i].transform.position = position;
            m_WheelMeshes[i].transform.rotation = quat;
        }

    }
    public void Move(float steering, float accel, float footbrake, float handbrake)
    {

        SetPositionWheels(4); //se moto 2 per esempio

        //clamp input values
        steering = Mathf.Clamp(steering, -1, 1);
        AccelInput = accel = Mathf.Clamp(accel, 0, 1);
        BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
        handbrake = Mathf.Clamp(handbrake, 0, 1);

        //Set the steer on the front wheels.
        //Assuming that wheels 0 and 1 are the front wheels.
        m_SteerAngle = steering * m_MaximumSteerAngle;
        m_WheelColliders[0].steerAngle = m_SteerAngle; //0 e 1 frontali, 3 e 4 posteriori
        m_WheelColliders[1].steerAngle = m_SteerAngle;

        SteerHelper();
        ApplyDrive(accel, footbrake);
        CapSpeed();

        //Set the handbrake.
        //Assuming that wheels 2 and 3 are the rear wheels.
        if (handbrake > 0f)
        {
            var hbTorque = handbrake * m_MaxHandbrakeTorque;
            m_WheelColliders[2].brakeTorque = hbTorque;
            m_WheelColliders[3].brakeTorque = hbTorque;
        }


        CalculateRevs();
        GearChanging();

        AddDownForce();
        TractionControl();
    }

    private void SteerHelper()
    {
        for (int i = 0; i < 4; i++)
        {
            WheelHit wheelhit;
            m_WheelColliders[i].GetGroundHit(out wheelhit);
            if (wheelhit.normal == Vector3.zero)
                return; // wheels arent on the ground so dont realign the rigidbody velocity
        }

        // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
        if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
        {
            var turnadjust = (transform.eulerAngles.y - m_OldRotation) * m_SteerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
        }
        m_OldRotation = transform.eulerAngles.y;
    }

    private void ApplyDrive(float accel, float footbrake)
    {

        float thrustTorque;
        thrustTorque = accel * (m_CurrentTorque / 4f);
        for (int i = 0; i < 4; i++)
        {
            m_WheelColliders[i].motorTorque = thrustTorque;
        }

        for (int i = 0; i < 4; i++)
        {
            if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, m_Rigidbody.velocity) < 50f)
            {
                m_WheelColliders[i].brakeTorque = m_BrakeTorque * footbrake;
            }
            else if (footbrake > 0)
            {
                m_WheelColliders[i].brakeTorque = 0f;
                m_WheelColliders[i].motorTorque = -m_ReverseTorque * footbrake;
            }
        }
    }

    private void CalculateRevs()
    {
        // calculate engine revs (for display / sound)
        // (this is done in retrospect - revs are not used in force/power calculations)
        CalculateGearFactor();
        var gearNumFactor = m_GearNum / (float)NoOfGears;
        var revsRangeMin = ULerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
        var revsRangeMax = ULerp(m_RevRangeBoundary, 1f, gearNumFactor);
        Revs = ULerp(revsRangeMin, revsRangeMax, m_GearFactor);
    }

    private void CalculateGearFactor()
    {
        float f = (1 / (float)NoOfGears);
        // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
        // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
        var targetGearFactor = Mathf.InverseLerp(f * m_GearNum, f * (m_GearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));
        m_GearFactor = Mathf.Lerp(m_GearFactor, targetGearFactor, Time.deltaTime * 5f);
    }
    // unclamped version of Lerp, to allow value to exceed the from-to range
    private static float ULerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }
    // simple function to add a curved bias towards 1 for a value in the 0-1 range
    private static float CurveFactor(float factor)
    {
        return 1 - (1 - factor) * (1 - factor);
    }

    private void GearChanging()
    {
        float f = Mathf.Abs(CurrentSpeed / MaxSpeed);
        float upgearlimit = (1 / (float)NoOfGears) * (m_GearNum + 1);
        float downgearlimit = (1 / (float)NoOfGears) * m_GearNum;

        if (m_GearNum > 0 && f < downgearlimit)
        {
            m_GearNum--;
        }

        if (f > upgearlimit && (m_GearNum < (NoOfGears - 1)))
        {
            m_GearNum++;
        }
    }
    // this is used to add more grip in relation to speed
    private void AddDownForce()
    {
        m_WheelColliders[0].attachedRigidbody.AddForce(-transform.up * m_Downforce *
                                                     m_WheelColliders[0].attachedRigidbody.velocity.magnitude);
    }

    // crude traction control that reduces the power to wheel if the car is wheel spinning too much
    private void TractionControl()
    {
        WheelHit wheelHit;
                // loop through all wheels
                for (int i = 0; i < 4; i++)
                {
                    m_WheelColliders[i].GetGroundHit(out wheelHit);

                    AdjustTorque(wheelHit.forwardSlip);
                }
                
    }

    private void AdjustTorque(float forwardSlip)
    {
        if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0)
        {
            m_CurrentTorque -= 10;
        }
        else
        {
            m_CurrentTorque += 10;
            if (m_CurrentTorque > m_FullTorqueOverAllWheels)
            {
                m_CurrentTorque = m_FullTorqueOverAllWheels;
            }
        }
    }

    private void CapSpeed()
    {
        float speed = m_Rigidbody.velocity.magnitude;
                speed *= 3.6f;
                if (speed > m_Topspeed)
                    m_Rigidbody.velocity = (m_Topspeed / 3.6f) * m_Rigidbody.velocity.normalized;
    }
     * */



/* BOZZA2

    [SerializeField] private WheelCollider[] _whellColliders = new WheelCollider[4];
    [SerializeField] private Transform[] _wheelTransforms = new Transform[4];
    [SerializeField] private float _maximumSteerAngle = 25; //da 25 fino a 45-50 (più alta velocità meno sterzo in teoria)
    [SerializeField] private float _torqueOverAllWheels;
    [SerializeField] private float _reverseTorque;
    [SerializeField] private float maxHandbrakeTorque; //potenza freno massima
    [SerializeField] private float _downforce = 100f; //non toccare
    [SerializeField] private float _topSpeed = 150;
    [SerializeField] private static int NoOfGears = 5; //da levare???
    [SerializeField] private float _revRangeBoundary = 1f; //da levare???
    [SerializeField] private float _slipLimit;
    [SerializeField] private float _brakeTorque;

    private Quaternion[] _initWheelRotations;
    //private Vector3 m_Prevpos, m_Pos;
    private float _steerAngle;
    private int m_GearNum; //forse levare
    private float m_GearFactor; //forse levare
    //private float m_OldRotation;
    private float _currentTorque;
    private Rigidbody _rigidbody;
    //private const float k_ReversingThreshold = 0.01f;

    public bool Skidding { get; private set; }
    public float BrakeInput { get; private set; }
    public float CurrentSteerAngle { get { return _steerAngle; } }
    public float CurrentSpeed { get { return _rigidbody.velocity.magnitude * 2.23693629f; } }
    public float MaxSpeed { get { return _topSpeed; } }
    public float Revs { get; private set; }
    public float AccelInput { get; private set; }

    // Use this for initialization
    void Start()
    {
        _initWheelRotations = new Quaternion[4];
        //settaggio rotazione ruote dalle transform
        for (int i = 0; i < 4; i++)
        {
            _initWheelRotations[i] = _wheelTransforms[i].localRotation;
        }

        maxHandbrakeTorque = float.MaxValue;

        _rigidbody = GetComponent<Rigidbody>();
        _currentTorque = _torqueOverAllWheels - _torqueOverAllWheels;
    }

    private void SetPositionWheels(int numberofwheels)
    {
        for (int i = 0; i < numberofwheels; i++)
        {
            Quaternion quat;
            Vector3 position;
            _whellColliders[i].GetWorldPose(out position, out quat);
            _wheelTransforms[i].position = position;
            _wheelTransforms[i].rotation = quat;
        }

    }
    public void Move(float steering, float accel, float footbrake, float handbrake)
    {

        SetPositionWheels(4); //se moto 2 per esempio

        //clamp input values
        steering = Mathf.Clamp(steering, -1, 1);
        AccelInput = accel = Mathf.Clamp(accel, 0, 1);
        BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
        handbrake = Mathf.Clamp(handbrake, 0, 1);

        //Set the steer on the front wheels.
        //Assuming that wheels 0 and 1 are the front wheels.
        _steerAngle = steering * _maximumSteerAngle;
        _whellColliders[0].steerAngle = _steerAngle; //0 e 1 frontali, 3 e 4 posteriori
        _whellColliders[1].steerAngle = _steerAngle;

        ApplyDrive(accel, footbrake);
        CapSpeed();

        //Set the handbrake.
        //Assuming that wheels 2 and 3 are the rear wheels.
        if (handbrake > 0f)
        {
            var hbTorque = handbrake * maxHandbrakeTorque;
            _whellColliders[2].brakeTorque = hbTorque;
            _whellColliders[3].brakeTorque = hbTorque;
        }


        CalculateRevs();
        GearChanging();

        AddDownForce();
        TractionControl();
    }

    private void ApplyDrive(float accel, float footbrake)
    {

        float thrustTorque;
        thrustTorque = accel * (_currentTorque / 4f);
        for (int i = 0; i < 4; i++)
        {
            _whellColliders[i].motorTorque = thrustTorque;
        }

        for (int i = 0; i < 4; i++)
        {
            if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, _rigidbody.velocity) < 50f)
            {
                _whellColliders[i].brakeTorque = _brakeTorque * footbrake;
            }
            else if (footbrake > 0)
            {
                _whellColliders[i].brakeTorque = 0f;
                _whellColliders[i].motorTorque = -_reverseTorque * footbrake;
            }
        }
    }

    private void CalculateRevs()
    {
        // calculate engine revs (for display / sound)
        // (this is done in retrospect - revs are not used in force/power calculations)
        CalculateGearFactor();
        var gearNumFactor = m_GearNum / (float)NoOfGears;
        var revsRangeMin = ULerp(0f, _revRangeBoundary, CurveFactor(gearNumFactor));
        var revsRangeMax = ULerp(_revRangeBoundary, 1f, gearNumFactor);
        Revs = ULerp(revsRangeMin, revsRangeMax, m_GearFactor);
    }

    private void CalculateGearFactor()
    {
        float f = (1 / (float)NoOfGears);
        // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
        // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
        var targetGearFactor = Mathf.InverseLerp(f * m_GearNum, f * (m_GearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));
        m_GearFactor = Mathf.Lerp(m_GearFactor, targetGearFactor, Time.deltaTime * 5f);
    }
    // unclamped version of Lerp, to allow value to exceed the from-to range
    private static float ULerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }
    // simple function to add a curved bias towards 1 for a value in the 0-1 range
    private static float CurveFactor(float factor)
    {
        return 1 - (1 - factor) * (1 - factor);
    }

    private void GearChanging()
    {
        float f = Mathf.Abs(CurrentSpeed / MaxSpeed);
        float upgearlimit = (1 / (float)NoOfGears) * (m_GearNum + 1);
        float downgearlimit = (1 / (float)NoOfGears) * m_GearNum;

        if (m_GearNum > 0 && f < downgearlimit)
        {
            m_GearNum--;
        }

        if (f > upgearlimit && (m_GearNum < (NoOfGears - 1)))
        {
            m_GearNum++;
        }
    }
    // this is used to add more grip in relation to speed
    private void AddDownForce()
    {
        _whellColliders[0].attachedRigidbody.AddForce(-transform.up * _downforce *
                                                     _whellColliders[0].attachedRigidbody.velocity.magnitude);
    }

    // crude traction control that reduces the power to wheel if the car is wheel spinning too much
    private void TractionControl()
    {
        WheelHit wheelHit;
        // loop through all wheels
        for (int i = 0; i < 4; i++)
        {
            _whellColliders[i].GetGroundHit(out wheelHit);

            AdjustTorque(wheelHit.forwardSlip);
        }

    }

    private void AdjustTorque(float forwardSlip)
    {
        if (forwardSlip >= _slipLimit && _currentTorque >= 0)
        {
            _currentTorque -= 10;
        }
        else
        {
            _currentTorque += 10;
            if (_currentTorque > _torqueOverAllWheels)
            {
                _currentTorque = _torqueOverAllWheels;
            }
        }
    }

    private void CapSpeed()
    {
        float speed = _rigidbody.velocity.magnitude;
        speed *= 3.6f;
        if (speed > _topSpeed)
            _rigidbody.velocity = (_topSpeed / 3.6f) * _rigidbody.velocity.normalized;
    }


    /*
     * PARTE FACOLTATIVA:
     * -SteerHelper per aiuto nelle curve ad alta velocità specialmente, ovviamente da richiamare nella Move. Valore migliore 0.644
     *  [NELLA DOCS: [Range(0, 1)] [SerializeField] private float SteerHelper; // 0 is raw physics , 1 the car will grip in the direction it is facing]
     * 
     * -Nella Start() _whellColliders[0].attachedRigidbody.centerOfMass = centreOfMassOffset;
     *  [SerializeField] private Vector3 centreOfMassOffset; //o invariato o leggermente più basso per stabilità
     * 
     * 
     */

    /*
    private void SteerHelper()
    {
        for (int i = 0; i < 4; i++)
        {
            WheelHit wheelhit;
            _whellColliders[i].GetGroundHit(out wheelhit);
            if (wheelhit.normal == Vector3.zero)
                return;
        }

        if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
        {
            float turnadjust = (transform.eulerAngles.y - m_OldRotation) * SteerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
        }
        m_OldRotation = transform.eulerAngles.y;
    }*/


