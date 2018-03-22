using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteerSeekColl : MonoBehaviour
{
    public Transform _target;
    public int _maxSpeed;
    public int _maxAcceleration;
    public int _maxBrakeForce;
    public Vector3 _currentVelocity = Vector3.zero;
    public Vector3 _targetVelocity;
    public float _targetSpeed;
    public Vector3 _steeringAll = Vector3.zero;
    public float _max_See_Ahead = 8;
    public float _max_Avoid_Force = 10;

    public float _targetRadius;  //Holds the radius for arriving at the target
    public float _slowRadius;  //Holds the radius for beginning to slow down

    public float distance;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = transform.position + _currentVelocity * Time.deltaTime;

        _steeringAll = getSteering()._linear + getCollideAvoid()._linear;
        _currentVelocity = _currentVelocity + _steeringAll * Time.deltaTime; // Vt = Vo +a.t

        if (_currentVelocity.magnitude > _maxSpeed)
        {
            _currentVelocity = _currentVelocity.normalized * _maxSpeed;

        }


    }

    public SteeringData getSteering()
    {
        SteeringData _SteeringOut = new SteeringData();
        _SteeringOut._linear = _target.position - transform.position; //#direction

        distance = _SteeringOut._linear.magnitude;

        if (distance > _slowRadius)
        {

            _targetSpeed = _maxSpeed;
        }

        else if (distance <= _targetRadius)
        {
            _targetSpeed = 0;

        }

        else
        {
            _targetSpeed = _maxSpeed * distance / _slowRadius;
        }

        _targetVelocity = _SteeringOut._linear.normalized * _targetSpeed;
        _SteeringOut._linear = (_targetVelocity - _currentVelocity);

        if (_targetSpeed < _maxSpeed)  //jika melambat gunakan brakeForce
        {
            _SteeringOut._linear = _SteeringOut._linear.normalized; // normalize membuat resultan vektor = 1.
            _SteeringOut._linear *= _maxBrakeForce;
        }
        else
        {
            if (_SteeringOut._linear.magnitude > _maxAcceleration)
            {
                _SteeringOut._linear = _SteeringOut._linear.normalized; // normalize membuat resultan vektor = 1.
                _SteeringOut._linear *= _maxAcceleration;
            }
        }

        // this.transform.eulerAngles = getNewOrientation(this.transform.eulerAngles, _KinematicOut._velocity); //rotation menggunakan euler angle
        _SteeringOut._angular = Vector3.zero; //orientasi belum kita hitung
        return _SteeringOut;
    }

    public SteeringData getCollideAvoid()
    {
        Vector3 ahead = this.transform.position + _currentVelocity.normalized * _max_See_Ahead * 0.5f;

        Circle _ClosestObsCircle = findMostThreateningObstacle();
        SteeringData _SteeringOut = new SteeringData();

        //lengkapi .... yang dibawah ini

        if (_ClosestObsCircle != null)
        {
            _SteeringOut._linear = ahead - _ClosestObsCircle._center;
            if (_SteeringOut._linear.magnitude > _max_Avoid_Force)
            {
                _SteeringOut._linear = _SteeringOut._linear.normalized;
                _SteeringOut._linear *= _max_Avoid_Force;
            }
        }
        else { _SteeringOut._linear = Vector3.zero; }
        return _SteeringOut;
    }

    public Vector3 getNewOrientation(Vector3 _currentOrientation, Vector3 _velocity)
    {
        //Make sure we have a velocity
        if (_velocity.magnitude > 0) //length didapat dri fungsi magnitude (mendapatkan resultan)
        {
            //Calculate orientation using an arc tangent of the velocity components.
            float radian = Mathf.Atan2(_velocity.x, _velocity.z); //dalam radian, tidak perlu minus karena rotasi unity searah jarum jam
            float angle = radian * (180 / Mathf.PI);  // untuk mengganti ke angle
            Vector3 newOrientation = new Vector3(0, angle, 0);
            return newOrientation;
        }

        //Otherwise use the current orientation
        else
            return _currentOrientation;

    }



    public bool lineIntersectsCircle(Vector3 _ahead, Vector3 _ahead2, Circle obstacle)
    {
        // the property "center" of the obstacle is a Vector3D.

        float distance1 = (obstacle._center - _ahead).magnitude;
        float distance2 = (obstacle._center - _ahead2).magnitude;

        return (distance1 <= obstacle._radius) || (distance2 <= obstacle._radius);
    }

    public Circle findMostThreateningObstacle()
    {
        Vector3 ahead = this.transform.position + _currentVelocity.normalized * _max_See_Ahead;
        Vector3 ahead2 = this.transform.position + _currentVelocity.normalized * _max_See_Ahead * 0.5f;
        Circle mostThreatening = null;
        //cek semua objek yg perlu dihindari
        foreach (GameObject _obstacle in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            Circle CircleObs = _obstacle.GetComponent<Circle>();
            bool _isCollide = lineIntersectsCircle(ahead, ahead2, CircleObs);

            //cek collision yang paling dekat
            if (_isCollide)
            {
                if (mostThreatening == null)
                {
                    mostThreatening = CircleObs;
                    Debug.Log(mostThreatening.gameObject);
                }
                else
                {
                    Debug.Log(GameObject.FindGameObjectsWithTag("Obstacle").Length);
                    float distanceCurrent = (this.transform.position - mostThreatening._center).magnitude;
                    float distanceCek = (this.transform.position - CircleObs._center).magnitude;
                    mostThreatening = distanceCek < distanceCurrent ? CircleObs : mostThreatening;
                }
            }
        }
        return mostThreatening;
    }
}
