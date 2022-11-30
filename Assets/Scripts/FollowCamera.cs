using UnityEngine;
using System;
using FastPolygons.Manager;

public class FollowCamera : MonoBehaviour
{
    private Transform car;
    public event EventHandler OnFollowCar;

    private void Awake()
    {
        OnFollowCar += FollowCamera_FollowCar;
    }

    private void FollowCamera_FollowCar(object sender, EventArgs e)
    {
        car = GameObject.FindGameObjectWithTag("Player").transform;

        if (car == null)
        {
            return;
        }
    }

    private void Update()
    {
        OnFollowCar?.Invoke(this, EventArgs.Empty);
    }

    private void FixedUpdate()
    {
        if(car != null)
        {
            Vector3 dir = car.transform.position - transform.position;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, 2 * Time.deltaTime);
            OnFollowCar -= FollowCamera_FollowCar;
        }
    }
}
