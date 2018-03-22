using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekMove : MonoBehaviour
{

    public Transform parent;
    public int _maxAcceleration;
    public int _maxSpeed;
    public Vector3 _currentVelocity = Vector3.zero;
    public Vector3 _steeringAll = Vector3.zero;
    public float _max_See_Ahead = 8;
    public float _max_Avoid_Force = 8;

    Vector3 _offset;
    Vector3 _target;

    // Use this for initialization
    void Start()
    {
        _offset = transform.position - parent.position;
        _target = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = parent.position + _offset;
        targetPosition.y = _target.y;
        _target += targetPosition - _target;

        this.transform.position = transform.position + _currentVelocity * Time.deltaTime;

        //kerjakan yang dibawah ini
        _steeringAll = getSteering()._linear + getCollideAvoid()._linear;
        this.transform.position = transform.position + _steeringAll * Time.deltaTime;
    }

    public SteeringData getSteering()
    {
        SteeringData _SteeringOut = new SteeringData();
        _SteeringOut._linear = _target - transform.position;
        _SteeringOut._linear = _SteeringOut._linear.normalized;
        _SteeringOut._linear *= _maxAcceleration;
        _SteeringOut._angular = Vector3.zero;
        return _SteeringOut;
    }

    public SteeringData getCollideAvoid()
    {
        Vector3 ahead = this.transform.position + _currentVelocity.normalized * _max_See_Ahead * 0.5f;

        Circle _ClosestObsCirle = findMostThreateningObstacle();
        SteeringData _SteeringOut = new SteeringData();

        //lengkapi .... yang dibawah ini

        if (_ClosestObsCirle != null)
        {
            _SteeringOut._linear = ahead - _ClosestObsCirle._center;
            if (_SteeringOut._linear.magnitude > _max_Avoid_Force)
            {
                _SteeringOut._linear = _SteeringOut._linear.normalized;
                _SteeringOut._linear *= _max_Avoid_Force;

            }
        }
        else { _SteeringOut._linear = Vector3.zero; }
        return _SteeringOut;
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

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(_target, 1.0f);
    }
}
