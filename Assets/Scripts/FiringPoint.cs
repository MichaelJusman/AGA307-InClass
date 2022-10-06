using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiringPoint : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 1000f;
    public GameObject hitSparks;
    public LineRenderer laser;
    


    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            FireRigidProjectile();
        }

        if(Input.GetButtonDown("Fire2"))
        {
            FireRaycast();
        }

    }

    void FireRigidProjectile()
    {
        //Create a reference to held our instantiated object
        GameObject projectileInstance = Instantiate(projectilePrefab, transform.position, transform.rotation);



        //Get the rigidbody attached to the [rpjectile and add force to it
        projectileInstance.GetComponent<Rigidbody>().AddForce(transform.forward * projectileSpeed);
    }

    void FireRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.Log(hit.collider.name + " at point " + hit.point + " which was " + hit.distance + " units away ");
            laser.SetPosition(0, transform.position);
            laser.SetPosition(1, hit.point);
            GameObject party = Instantiate(hitSparks, hit.point, hit.transform.rotation);
           
            Destroy(party, 2);
        }
    }
}
