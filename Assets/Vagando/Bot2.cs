using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bot2 : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;
    Drive2 ds;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Start()
{
    agent = GetComponent<NavMeshAgent>();

    if (agent == null)
        Debug.LogError("❌ BOT2 NÃO TEM NAV MESH AGENT!");

    if (target == null)
    {
        Debug.LogError("❌ BOT2 NÃO TEM TARGET!");
        return;
    }

    ds = target.GetComponent<Drive2>();

    if (ds == null)
        Debug.LogError("❌ O TARGET NÃO TEM O SCRIPT DRIVE!");
    else
        Debug.Log("✅ TARGET E DRIVE ENCONTRADOS!");
}

    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - this.transform.position;
        agent.SetDestination(this.transform.position - fleeVector);
    }

    void Pursue()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;

        float relativeHeading = Vector3.Angle(
            this.transform.forward,
            this.transform.TransformVector(target.transform.forward)
        );

        float toTarget = Vector3.Angle(
            this.transform.forward,
            this.transform.TransformVector(targetDir)
        );

        if ((toTarget > 90 && relativeHeading < 20) || ds.currentSpeed < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }

        float lookAhead = targetDir.magnitude / (agent.speed + ds.currentSpeed);

        Seek(target.transform.position + target.transform.forward * lookAhead);
    }

    void Evade()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;

        float lookAhead = targetDir.magnitude / (agent.speed + ds.currentSpeed);

        Flee(target.transform.position + target.transform.forward * lookAhead);
    }

    Vector3 wanderTarget = Vector3.zero;

    void Wander()
    {
        float wanderRadius = 10;
        float wanderDistance = 10;
        float wanderJitter = 1;

        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter,
                                     0,
                                     Random.Range(-1.0f,1.0f) * wanderJitter);
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);
        Vector3 targetWorld = this.gameObject.transform.InverseTransformVector(targetLocal);

        Seek(targetWorld);
    }

    // Update is called once per frame
    void Update()
    {
        Evade();
    }
}
