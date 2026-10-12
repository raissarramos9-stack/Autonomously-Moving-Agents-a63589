
using UnityEngine;
using UnityEngine.AI;

public class Bot4 : MonoBehaviour
{
    public GameObject target;

    private NavMeshAgent agent;
    private Drive4 ds;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (target == null)
        {
            Debug.LogError("Bot4: Tens de atribuir o Target no Inspector!", this);
            enabled = false;
            return;
        }

        ds = target.GetComponent<Drive4>();

        if (ds == null)
        {
            Debug.LogError("Bot4: O objeto Target não tem o script Drive!", this);
            enabled = false;
            return;
        }

        if (agent == null)
        {
            Debug.LogError("Bot4: Falta o componente NavMeshAgent!", this);
            enabled = false;
            return;
        }
    }

    void Seek(Vector3 location)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(location);
        }
    }

    void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - transform.position;
        Seek(transform.position - fleeVector);
    }

    void Pursue()
    {
        if (target == null || ds == null || agent == null)
            return;

        Vector3 targetDir = target.transform.position - transform.position;

        float relativeHeading = Vector3.Angle(
            transform.forward,
            transform.InverseTransformDirection(target.transform.forward)
        );

        float toTarget = Vector3.Angle(
            transform.forward,
            transform.InverseTransformDirection(targetDir)
        );

        if ((toTarget > 90.0f && relativeHeading < 20.0f) ||
            ds.currentSpeed < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }

        float totalSpeed = agent.speed + ds.currentSpeed;

        if (totalSpeed <= 0.01f)
        {
            Seek(target.transform.position);
            return;
        }

        float lookAhead = targetDir.magnitude / totalSpeed;

        Seek(target.transform.position +
             target.transform.forward * lookAhead);
    }

    void Evade()
    {
        if (target == null || ds == null || agent == null)
            return;

        Vector3 targetDir = target.transform.position - transform.position;
        float totalSpeed = agent.speed + ds.currentSpeed;

        if (totalSpeed <= 0.01f)
        {
            Flee(target.transform.position);
            return;
        }

        float lookAhead = targetDir.magnitude / totalSpeed;

        Flee(target.transform.position +
             target.transform.forward * lookAhead);
    }

    Vector3 wanderTarget = Vector3.zero;

    void Wander()
    {
        float wanderRadius = 10.0f;
        float wanderDistance = 10.0f;
        float wanderJitter = 1.0f;

        wanderTarget += new Vector3(
            Random.Range(-1.0f, 1.0f) * wanderJitter,
            0.0f,
            Random.Range(-1.0f, 1.0f) * wanderJitter
        );

        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget +
                              new Vector3(0.0f, 0.0f, wanderDistance);

        Vector3 targetWorld = transform.TransformPoint(targetLocal);

        Seek(targetWorld);
    }

    void Hide()
    {
        if (target == null || World4.Instance == null)
            return;

        GameObject[] hidingSpots = World4.Instance.GetHidingSpots();

        if (hidingSpots == null || hidingSpots.Length == 0)
            return;

        float dist = Mathf.Infinity;
        Vector3 chosenSpot = transform.position;

        foreach (GameObject spot in hidingSpots)
        {
            if (spot == null)
                continue;

            Vector3 hideDir =
                spot.transform.position - target.transform.position;

            Vector3 hidePos =
                spot.transform.position + hideDir.normalized * 10.0f;

            float currentDist = Vector3.Distance(transform.position, hidePos);

            if (currentDist < dist)
            {
                chosenSpot = hidePos;
                dist = currentDist;
            }
        }

        if (dist < Mathf.Infinity)
            Seek(chosenSpot);
    }

    void CleverHide()
    {
        if (target == null || World4.Instance == null)
            return;

        GameObject[] hidingSpots = World4.Instance.GetHidingSpots();

        if (hidingSpots == null || hidingSpots.Length == 0)
            return;

        float dist = Mathf.Infinity;
        Vector3 chosenSpot = transform.position;
        Vector3 chosenDir = Vector3.zero;
        GameObject chosenGO = null;

        foreach (GameObject spot in hidingSpots)
        {
            if (spot == null)
                continue;

            Vector3 hideDir =
                spot.transform.position - target.transform.position;

            Vector3 hidePos =
                spot.transform.position + hideDir.normalized * 10.0f;

            float currentDist = Vector3.Distance(transform.position, hidePos);

            if (currentDist < dist)
            {
                chosenSpot = hidePos;
                chosenDir = hideDir;
                chosenGO = spot;
                dist = currentDist;
            }
        }

        if (chosenGO == null)
            return;

        Collider hideCol = chosenGO.GetComponent<Collider>();

        if (hideCol == null)
        {
            Seek(chosenSpot);
            return;
        }

        Ray backRay = new Ray(chosenSpot, -chosenDir.normalized);

        if (hideCol.Raycast(backRay, out RaycastHit info, 100.0f))
        {
            Seek(info.point + chosenDir.normalized * 2.0f);
        }
        else
        {
            Seek(chosenSpot);
        }
    }

    bool CanSeeTarget()
    {
        if (target == null)
            return false;

        Vector3 rayToTarget = target.transform.position - transform.position;

        float lookAngle = Vector3.Angle(transform.forward, rayToTarget);

        if (lookAngle < 60.0f &&
            Physics.Raycast(transform.position, rayToTarget, out RaycastHit hit))
        {
            if (hit.transform.CompareTag("cop"))
                return true;
        }

        return false;
    }

    bool TargetCanSeeMe()
    {
        if (target == null)
            return false;

        Vector3 rayToTarget = transform.position - target.transform.position;

        float lookAngle = Vector3.Angle(
            target.transform.forward,
            rayToTarget
        );

        return lookAngle < 60.0f;
    }

    private bool coolDown = false;

    void BehaviourCooldown()
    {
        coolDown = false;
    }

    void Update()
    {
        if (target == null || ds == null || agent == null)
            return;

        if (!coolDown)
        {
            if (CanSeeTarget() && TargetCanSeeMe())
            {
                CleverHide();

                coolDown = true;
                Invoke(nameof(BehaviourCooldown), 5.0f);
            }
            else
            {
                Pursue();
            }
        }
    }

    void OnDisable()
    {
        CancelInvoke(nameof(BehaviourCooldown));
    }
}
