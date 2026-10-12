using UnityEngine;
using UnityEngine.AI;

public class Bot4 : MonoBehaviour {

    public GameObject target;

    NavMeshAgent agent;
    Drive4 ds;

    void Start() {

        agent = GetComponent<NavMeshAgent>();
        ds = target.GetComponent<Drive4>();
    }

    void Seek(Vector3 location) {

        agent.SetDestination(location);
    }

    void Flee(Vector3 location) {

        Vector3 fleeVector = location - this.transform.position;
        agent.SetDestination(this.transform.position - fleeVector);
    }

    void Pursue() {

        Vector3 targetDir = target.transform.position - this.transform.position;

        // float relativeHeading = Vector3.Angle(this.transform.forward, this.transform.TransformVector(target.transform.forward));
        // float toTarget = Vector3.Angle(this.transform.forward, this.transform.TransformVector(targetDir));
        float relativeHeading = Vector3.Angle(this.transform.forward, this.transform.InverseTransformVector(target.transform.forward));
        float toTarget = Vector3.Angle(this.transform.forward, this.transform.InverseTransformVector(targetDir));

        if ((toTarget > 90.0f && relativeHeading < 20.0f) || ds.currentSpeed < 0.01f) {

            Seek(target.transform.position);
            return;
        }

        float lookAhead = targetDir.magnitude / (agent.speed + ds.currentSpeed);
        Seek(target.transform.position + target.transform.forward * lookAhead);
    }

    void Evade() {

        Vector3 targetDir = target.transform.position - this.transform.position;
        float lookAhead = targetDir.magnitude / (agent.speed + ds.currentSpeed);
        Flee(target.transform.position + target.transform.forward * lookAhead);
    }

    Vector3 wanderTarget = Vector3.zero;

    void Wander() {

        float wanderRadius = 10.0f;
        float wanderDistance = 10.0f;
        float wanderJitter = 1.0f;

        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter,
        0.0f,
        Random.Range(1.0f, 1.0f) * wanderJitter);

        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0.0f, 0.0f, wanderDistance);
        //Vector3 targetWorld = this.gameObject.transform.InverseTransformVector(targetLocal);
        Vector3 targetWorld = this.gameObject.transform.TransformVector(targetLocal);

        Seek(targetWorld);
    }

    void Hide() {

        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;

        for (int i = 0; i < World4.Instance.GetHidingSpots().Length; ++i) {

            Vector3 hideDir = World4.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 hidePos = World4.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 10.0f;

            if (Vector3.Distance(this.transform.position, hidePos) < dist) {

                chosenSpot = hidePos;
                dist = Vector3.Distance(this.transform.position, hidePos);
            }
        }

        Seek(chosenSpot);
    }

    void CleverHide() {

        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDir = Vector3.zero;
        GameObject chosenGO = World4.Instance.GetHidingSpots()[0];

        for (int i = 0; i < World4.Instance.GetHidingSpots().Length; ++i) {

            Vector3 hideDir = World4.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 hidePos = World4.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 10.0f;

            if (Vector3.Distance(this.transform.position, hidePos) < dist) {

                chosenSpot = hidePos;
                chosenDir = hideDir;
                chosenGO = World4.Instance.GetHidingSpots()[i];
                dist = Vector3.Distance(this.transform.position, hidePos);
            }
        }

        Collider hideCol = chosenGO.GetComponent<Collider>();
        Ray backRay = new Ray(chosenSpot, -chosenDir.normalized);
        RaycastHit info;
        float distance = 100.0f;
        hideCol.Raycast(backRay, out info, distance);

        Seek(info.point + chosenDir.normalized * 2.0f);
    }

    bool CanSeeTarget() {

        RaycastHit raycastInfo;
        Vector3 rayToTarget = target.transform.position - this.transform.position;
        float lookAngle = Vector3.Angle(this.transform.forward, rayToTarget);

        if (lookAngle < 60.0f && Physics.Raycast(this.transform.position, rayToTarget, out raycastInfo)) {

            if (raycastInfo.transform.gameObject.tag == "cop") {

                return true;
            }
        }
        return false;
    }

    bool CanSeeMe() {

        Vector3 rayToTarget = this.transform.position - target.transform.position;
        float lookAngle = Vector3.Angle(target.transform.forward, rayToTarget);

        if (lookAngle < 60.0f ) return true;
     
        return false;
    }

    bool coolDown = false;

    void BehaviourCooldown() {

        coolDown = false;
    }

    bool TargetInRange() {

        if (Vector3.Distance(this.transform.position, target.transform.position) < 10.0f) return true;
        return false;
    }

    void Update() {

        // Seek(target.transform.position);
        // Flee(target.transform.position);
        // Pursue();
        // Evade();
        // Wander();
        // Hide();
        // if (CanSeeTarget()) CleverHide();

        if (!coolDown) {

            if (!TargetInRange()) {

                Wander();
            }
            else if (CanSeeTarget() && CanSeeMe()) {

                CleverHide();
                coolDown = true;
                Invoke("BehaviourCooldown", 5.0f);
            } else {

                Pursue();
            }
        }   
    }
}
