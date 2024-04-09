using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Rigidbody theRB;
    public float maxSpeed;

    public float fowardAccel, reverseAccel;
    public float turnStrength = 180;

    private float speedInput = 0F;
    private float turnInput;

    private bool grounded;
    private float dragOnGround;

    public Transform groundRayPoint, groundRayPoint2;
    public LayerMask whatIsGround;
    public float groundRayLength = 0.75f;
    public float gravityMod = 10f;
    public float carDragOnGround = 3f;
    public float angularDragOnGround = 3f;

    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn = 30f;

    public ParticleSystem[] dustTrail;
    public float maxEmission = 50f, emissionFadeSpeed = 30f;
    private float emissionRate;

    public AudioSource engineSound;
    public AudioSource skidSound;
    public float skidFade = 2f;




    // Start is called before the first frame update
    void Start()
    {
        theRB.transform.parent = null; // unparents the physics and model

    }

    // Update is called once per frame
    void Update()
    {
        if (!LapTracker.instance.raceStarting)

        {

            speedInput = 0f;

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetToTrack();
            }

            if (Input.GetAxis("Vertical") > 0)
            {
                speedInput = Input.GetAxis("Vertical") * fowardAccel;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                speedInput = Input.GetAxis("Vertical") * reverseAccel;
            }

            turnInput = Input.GetAxis("Horizontal");



            

            
        }

    }

    private void LateUpdate()
    {
        //Turning the Wheels

        UpdateWheelRotation();

        // transform.position = theRB.position; //moves the model of the car to match the physics sphere 

        //control the tire particle emissions
        UpdateParticleEffect();
        UpdateSoundEffect();
    }

    private void UpdateSoundEffect()
    {
        if (engineSound != null)
        {
            engineSound.pitch = 1f + ((theRB.velocity.magnitude / maxSpeed) * 2.5f);
        }

        if (skidSound != null)
        {
            if (Mathf.Abs(turnInput) > 0.5f)
            {
                skidSound.volume = 0.25f;
            }
            else
            {
                skidSound.volume = Mathf.MoveTowards(skidSound.volume, 0f, skidFade * Time.deltaTime);
            }
        }
    }

    private void UpdateParticleEffect()
    {
        emissionRate = Mathf.MoveTowards(emissionRate, 0f, emissionFadeSpeed * Time.deltaTime);

        if (grounded && (Mathf.Abs(turnInput) > 0.5f || (theRB.velocity.magnitude < maxSpeed * 0.5f && theRB.velocity.magnitude != 0)))
        {
            emissionRate = maxEmission;
        }

        if (theRB.velocity.magnitude <= 1)
        {
            emissionRate = 0;
        }

        for (int i = 0; i < dustTrail.Length; i++)
        {
            var emissionModule = dustTrail[i].emission;
            emissionModule.rateOverTime = emissionRate;
        }
    }

    private void UpdateWheelRotation()
    {
        leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn), rightFrontWheel.localRotation.eulerAngles.z);
    }

    private void FixedUpdate()
    {
        grounded = false;

        RaycastHit hit;
        Vector3 normalTarget = Vector3.zero;

        if (Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, whatIsGround))
        {
            grounded = true;
            normalTarget = hit.normal;
            
        }


        if (Physics.Raycast(groundRayPoint2.position, -transform.up, out hit, groundRayLength, whatIsGround))
        {
            grounded = true;
            normalTarget = (normalTarget + hit.normal) / 2;
        }


        if (grounded)
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, normalTarget) * transform.rotation;
        }
     

        // This accelerates the car 

        if ((grounded) & !LapTracker.instance.raceOver)
        {
            theRB.drag = carDragOnGround;
            theRB.angularDrag = angularDragOnGround;
            theRB.AddForce(transform.forward * speedInput * 100f);
        }
        else
        {
            theRB.drag = 0.1f;
            theRB.AddForce(-Vector3.up * gravityMod * 100f);
        }

        if (theRB.velocity.magnitude > maxSpeed)
        {
            theRB.velocity = theRB.velocity.normalized;
        }


        //Debug.Log(theRB.velocity.magnitude);
        //Debug.Log(grounded);

        //Move the Model 

        transform.position = theRB.position;

        if (grounded && Input.GetAxis("Horizontal") != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed), 0f));
        }
    }

    void ResetToTrack()
    {
        Transform pointToGoTo = LapTracker.instance.precedingGate;
        
        transform.position = pointToGoTo.position;
        transform.rotation = LapTracker.instance.precedingGate.rotation;
     
        theRB.transform.position = pointToGoTo.position;
        theRB.velocity = Vector3.zero;
        speedInput = 0f;
        turnInput = 0F;
    }
}
