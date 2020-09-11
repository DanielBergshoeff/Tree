using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityStandardAssets.Characters.ThirdPerson;

public class WallClimber : MonoBehaviour
{
    public float MoveSpeed = 3f;
    public float ClimbForce = 1f;
    public float SmallestEdge = 0.25f;
    public float MaxAngle = 30f;
    public float MinDistance = 0.1f;
    public float CoolDown = 0.15f;
    public float ClimbRange = 2f;
    public float JumpForce = 1f;
    public float ClimbJumpForce = 300f;
    public float BounceForce = 300f;
    public ClimbingType CurrentType = ClimbingType.Walking;

    public StickType CurrentStickType;

    public Transform HandTransform;
    public Animator MyAnimator;
    public Rigidbody MyRigidbody;
    public Vector3 VerticalHandOffset;
    public Vector3 HorizontalHandOffset;
    public Vector3 FallHandOffset;
    public Vector3 RaycastPosition;

    public LayerMask SpotLayer;
    public LayerMask CurrentSpotLayer;
    public LayerMask CheckLayerForObstacle;
    public LayerMask CheckLayersReachable;

    private Vector3 targetPoint;
    private Vector3 targetNormal;

    private Vector2 moveDir;
    private Vector2 rightStickDir;

    private float lastTime;
    private float beginDistance;
    private Vector3 m_GroundNormal;
    private bool m_IsGrounded = false;


    private Quaternion oldRotation;

    // Update is called once per frame
    void Update() {
        GroundCheck();
        if (CurrentType == ClimbingType.Walking || CurrentType == ClimbingType.Falling || CurrentType == ClimbingType.Jumping || CurrentType == ClimbingType.CheckingForClimbStart) {
            if(Time.time - lastTime > 0.3f)
                Move();
        }

        if (CurrentType == ClimbingType.Walking && moveDir.y > 0f)
            StartClimbing();

        if (CurrentType == ClimbingType.Climbing)
            Climb();

        UpdateStats();

        if (CurrentType == ClimbingType.ClimbingTowardsPoint || CurrentType == ClimbingType.ClimbingTowardsPlateau) 
            MoveTowardsPoint();

        if (CurrentType == ClimbingType.Jumping || CurrentType == ClimbingType.Falling)
            Jumping();
    }

    private void GroundCheck() {
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, 0.3f)) {
            m_GroundNormal = hitInfo.normal;
            m_IsGrounded = true;
            MyAnimator.applyRootMotion = true;
        }
        else {
            m_IsGrounded = false;
            m_GroundNormal = Vector3.up;
            MyAnimator.applyRootMotion = false;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.CompareTag("Mushroom")) {
            MyRigidbody.AddForce(Vector3.up * BounceForce);
        }
    }
    

    private void OnRightStick(InputValue value) {
        rightStickDir = value.Get<Vector2>();

        switch (CurrentStickType) {
            case StickType.Normal:
                if (rightStickDir.y < -0.5f && CurrentType != ClimbingType.Walking && CurrentType != ClimbingType.Jumping && CurrentType != ClimbingType.Falling) {
                    CurrentStickType = StickType.Jumping;
                }
                return;

            case StickType.Jumping:
                if (rightStickDir.y > -0.1f && Mathf.Abs(rightStickDir.x) + Mathf.Abs(rightStickDir.y) > 0.9f) {
                    CurrentStickType = StickType.Normal;
                    MyRigidbody.isKinematic = false;
                    MyRigidbody.AddForce((transform.right * rightStickDir.x + transform.up * Mathf.Clamp(rightStickDir.y + 0.5f, 0f, 1f)) * ClimbJumpForce);
                    CurrentType = ClimbingType.Jumping;
                }
                return;
        }
    }

    private void Move() {
        MyAnimator.SetFloat("Speed", moveDir.magnitude);

        if (moveDir.magnitude < 0.1f) {
            return;
        }

        float zMove = moveDir.y;

        if (CurrentType != ClimbingType.Walking)
            zMove = 0f;

        Vector3 targetDir = new Vector3(moveDir.x, 0f, zMove);
        targetDir = Camera.main.transform.TransformDirection(targetDir);
        targetDir.y = 0.0f;

        transform.position = transform.position + targetDir * Time.deltaTime * MoveSpeed;

        if (CurrentType != ClimbingType.Walking)
            return;

        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDir, Time.deltaTime * 10f, 0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    private void Jump() {
        if (!m_IsGrounded)
            return;

        MyRigidbody.AddForce(Vector3.up * JumpForce);
    }

    private void OnJump() {
        Jump();
    }

    private void OnMove(InputValue value) {
        moveDir = value.Get<Vector2>();
    }

    public void CheckForClimbStart() {
        RaycastHit hit;

        Vector3 dir = transform.forward - transform.up / 0.8f;

        if(!Physics.Raycast(transform.position + transform.rotation * RaycastPosition, dir, 1.6f) && !Input.GetButton("Jump")) {
            CurrentType = ClimbingType.CheckingForClimbStart;
            if (Physics.Raycast(transform.position + new Vector3(0, 1.1f, 0), -transform.up, out hit, 1.6f, SpotLayer))
                FindSpot(hit, CheckingType.Falling);
        }
    }

    public void CheckForPlateau() {
        RaycastHit hit;

        Vector3 dir = transform.up + transform.forward / 2f;

        if(!Physics.Raycast(HandTransform.position + transform.rotation * VerticalHandOffset, dir, out hit, 1.5f, SpotLayer)) {
            CurrentType = ClimbingType.ClimbingTowardsPlateau;

            if(Physics.Raycast(HandTransform.position + dir * 1.5f, -Vector3.up, out hit, 1.7f, SpotLayer)) {
                targetPoint = HandTransform.position + dir * 1.5f;
            }
            else {
                targetPoint = HandTransform.position + dir * 1.5f - transform.rotation * new Vector3(0, -0.2f, 0.25f);
            }

            targetNormal = -transform.forward;

            MyAnimator.SetBool("Hanging", true);
        }
    }

    public void UpdateStats() {
        if(CurrentType != ClimbingType.Walking && m_IsGrounded && CurrentType != ClimbingType.ClimbingTowardsPoint) {
            CurrentType = ClimbingType.Walking;
            MyRigidbody.isKinematic = false;
            MyAnimator.SetBool("Hanging", false);
        }

        if (CurrentType == ClimbingType.Walking && !m_IsGrounded)
            CurrentType = ClimbingType.Jumping;

        if(CurrentType == ClimbingType.Walking && (moveDir.y != 0 || moveDir.x != 0))
            CheckForClimbStart();
    }

    public void StartClimbing() {
        if(Physics.Raycast(transform.position + transform.rotation * RaycastPosition, transform.forward, 0.4f, SpotLayer) && Time.time - lastTime > CoolDown && CurrentType == ClimbingType.Walking) {
            if(CurrentType == ClimbingType.Walking) {
                MyRigidbody.AddForce(transform.up * JumpForce);
            }

            lastTime = Time.time;
        }
    }

    public void Jumping() {
        if(MyRigidbody.velocity.y < 0f && CurrentType != ClimbingType.Falling) {
            CurrentType = ClimbingType.Falling;
            oldRotation = transform.rotation;
        }

        if(MyRigidbody.velocity.y > 0f && CurrentType != ClimbingType.Jumping) {
            CurrentType = ClimbingType.Jumping;
        }

        if(CurrentType == ClimbingType.Jumping) {
            CheckForSpots(HandTransform.position + FallHandOffset, -transform.up, 0.1f, CheckingType.Normal);
        }
        if(CurrentType == ClimbingType.Falling) {
            CheckForSpots(HandTransform.position + FallHandOffset + transform.rotation * new Vector3(0.02f, -0.6f, 0f), -transform.up, 0.4f, CheckingType.Normal);
            transform.rotation = oldRotation;
        }
    }

    public void Climb() {
        if(Time.time - lastTime > CoolDown && CurrentType == ClimbingType.Climbing) {
            if(moveDir.y > 0.5f) {
                CheckForSpots(HandTransform.position + transform.rotation * VerticalHandOffset + transform.up * ClimbRange, -transform.up, ClimbRange, CheckingType.Normal);


                if(CurrentType != ClimbingType.ClimbingTowardsPoint)
                    CheckForPlateau();
            }
            else if (moveDir.y < -0.5f) {
                CheckForSpots(HandTransform.position - transform.rotation * (VerticalHandOffset + new Vector3(0f, 0.3f, 0f)), -transform.up, ClimbRange, CheckingType.Normal);


                if(CurrentType != ClimbingType.ClimbingTowardsPoint) {
                    MyRigidbody.isKinematic = false;
                    CurrentType = ClimbingType.Falling;
                    oldRotation = transform.rotation;
                }
            }
            else if (moveDir.x != 0f && moveDir.x > 0.5f || moveDir.x < -0.5f) {
                CheckForSpots(HandTransform.position + transform.rotation * HorizontalHandOffset, transform.right * moveDir.x - transform.up / 3.5f, ClimbRange / 2, CheckingType.Normal);

                if(CurrentType != ClimbingType.ClimbingTowardsPoint)
                    CheckForSpots(HandTransform.position + transform.rotation * HorizontalHandOffset, transform.right * moveDir.x - transform.up / 1.5f, ClimbRange / 3f, CheckingType.Normal);

                if (CurrentType != ClimbingType.ClimbingTowardsPoint)
                    CheckForSpots(HandTransform.position + transform.rotation * HorizontalHandOffset, transform.right * moveDir.x - transform.up / 6f, ClimbRange / 1.5f, CheckingType.Normal);

                if (CurrentType != ClimbingType.ClimbingTowardsPoint) {
                    int hor = 0;

                    if (moveDir.x < 0f) {
                        hor = -1;
                    }
                    if (moveDir.x > 0f) {
                        hor = 1;
                    }

                    CheckForSpots(HandTransform.position + transform.rotation * HorizontalHandOffset + transform.right * hor * SmallestEdge / 4, transform.forward - transform.up * 2f, ClimbRange / 3f, CheckingType.Turning);
                    if (CurrentType != ClimbingType.ClimbingTowardsPoint) {
                        CheckForSpots(HandTransform.position + transform.rotation * HorizontalHandOffset + transform.right * 0.2f, transform.forward - transform.up * 2f + transform.right * hor /1.5f, ClimbRange / 3f, CheckingType.Turning);
                    }
                }
            }

        }
    }

    public void CheckForSpots(Vector3 spotLocation, Vector3 direction, float range, CheckingType checkingType) {
        bool foundSpot = false;

        RaycastHit hit;
        if(Physics.Raycast(spotLocation + transform.right * SmallestEdge / 2f, direction, out hit, range, SpotLayer)) {
            if(Vector3.Distance(HandTransform.position, hit.point) > MinDistance) {
                foundSpot = true;

                FindSpot(hit, checkingType);
            }
        }

        if (!foundSpot) {
            if (Physics.Raycast(spotLocation - transform.right * SmallestEdge / 2f, direction, out hit, range, SpotLayer)) {
                if (Vector3.Distance(HandTransform.position, hit.point) > MinDistance) {
                    foundSpot = true;

                    FindSpot(hit, checkingType);
                }
            }
        }

        if (!foundSpot) {
            if (Physics.Raycast(spotLocation + transform.right * SmallestEdge / 2f + transform.forward * SmallestEdge, direction, out hit, range, SpotLayer)) {
                if (Vector3.Distance(HandTransform.position, hit.point) - SmallestEdge / 1.5f > MinDistance) {
                    foundSpot = true;

                    FindSpot(hit, checkingType);
                }
            }
        }

        if (!foundSpot) {
            if (Physics.Raycast(spotLocation - transform.right * SmallestEdge / 2f + transform.forward * SmallestEdge, direction, out hit, range, SpotLayer)) {
                if (Vector3.Distance(HandTransform.position, hit.point) - SmallestEdge / 1.5f > MinDistance) {
                    foundSpot = true;

                    FindSpot(hit, checkingType);
                }
            }
        }
    }

    public void FindSpot(RaycastHit h, CheckingType type) {
        if(Vector3.Angle(h.normal, Vector3.up) < MaxAngle) {
            RayInfo ray = new RayInfo();

            if(type == CheckingType.Normal) {
                ray = GetClosestPoint(h.transform, h.point + new Vector3(0, -0.01f, 0), transform.forward / 2.5f);
            }
            else  if (type == CheckingType.Turning) {
                ray = GetClosestPoint(h.transform, h.point + new Vector3(0, -0.01f, 0), transform.forward / 2.5f - transform.right * moveDir.x);
            }
            else if (type == CheckingType.Falling) {
                ray = GetClosestPoint(h.transform, h.point + new Vector3(0, -0.01f, 0), -transform.forward / 2.5f);
            }

            targetPoint = ray.Point;
            targetNormal = ray.Normal;

            if (ray.CanGoToPoint) {
                if(CurrentType != ClimbingType.Climbing && CurrentType != ClimbingType.ClimbingTowardsPoint) {
                    MyRigidbody.isKinematic = true;
                    m_IsGrounded = false;
                }

                CurrentType = ClimbingType.ClimbingTowardsPoint;

                beginDistance = Vector3.Distance(transform.position, (targetPoint - transform.rotation * HandTransform.localPosition));
            }
        }
    }

    public RayInfo GetClosestPoint(Transform trans, Vector3 pos, Vector3 dir) {
        RayInfo currray = new RayInfo();

        RaycastHit hit;
        int oldLayer = trans.gameObject.layer;

        trans.gameObject.layer = 8;

        if (Physics.Raycast(pos - dir, dir, out hit, dir.magnitude * 2f, CurrentSpotLayer)) {
            currray.Point = hit.point;
            currray.Normal = hit.normal;

            if (!Physics.Linecast(HandTransform.position + transform.rotation * new Vector3(0, 0.05f, -0.05f), currray.Point + new Vector3(0, 0.5f, 0), out hit, CheckLayersReachable)) {
                if (!Physics.Linecast(currray.Point - Quaternion.Euler(new Vector3(0f, 90f, 0f)) * currray.Normal * 0.35f + 0.1f * currray.Normal, currray.Point + Quaternion.Euler(new Vector3(0f, 90f, 0f)) * currray.Normal * 0.35f + 0.1f * currray.Normal, out hit, CheckLayerForObstacle)) // character width = 0.35f change if necessary
                {
                    if (!Physics.Linecast(currray.Point + Quaternion.Euler(new Vector3(0f, 90f, 0f)) * currray.Normal * 0.35f + 0.1f * currray.Normal, currray.Point - Quaternion.Euler(new Vector3(0f, 90f, 0f)) * currray.Normal * 0.35f + 0.1f * currray.Normal, out hit, CheckLayerForObstacle)) // character width = 0.35f change if necessary
                    {
                        currray.CanGoToPoint = true;
                    }
                    else {
                        currray.CanGoToPoint = false;
                    }
                }
                else {
                    currray.CanGoToPoint = false;
                }
            }
            else {
                currray.CanGoToPoint = false;
            }

            trans.gameObject.layer = oldLayer;
            return currray;
        }
        else {
            trans.gameObject.layer = oldLayer;
            return currray;
        }
    }

    public void MoveTowardsPoint() {
        transform.position = Vector3.Lerp(transform.position, (targetPoint - transform.rotation * HandTransform.localPosition), Time.deltaTime * ClimbForce);

        Quaternion lookRotation = Quaternion.LookRotation(-targetNormal);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * ClimbForce);

        float distance = Vector3.Distance(transform.position, (targetPoint - transform.rotation * HandTransform.localPosition));
        float percent = -9f * (beginDistance - distance) / beginDistance;

        MyAnimator.SetBool("Hanging", true);

        if (distance <= 0.01f && CurrentType == ClimbingType.ClimbingTowardsPoint) {
            transform.position = targetPoint - transform.rotation * HandTransform.localPosition;
            transform.rotation = lookRotation;
            lastTime = Time.time;
            CurrentType = ClimbingType.Climbing;
        }

        if (distance <= 0.25f && CurrentType == ClimbingType.ClimbingTowardsPlateau) {
            transform.position = targetPoint - transform.rotation * HandTransform.localPosition;
            transform.rotation = lookRotation;
            lastTime = Time.time;
            CurrentType = ClimbingType.Walking;

            MyRigidbody.isKinematic = false;
        }
    }
}

[System.Serializable]
public class RayInfo
{
    public Vector3 Point;
    public Vector3 Normal;
    public bool CanGoToPoint;
}


[System.Serializable]
public enum ClimbingType
{
    Walking,
    Jumping,
    Falling,
    Climbing,
    ClimbingTowardsPoint,
    ClimbingTowardsPlateau,
    CheckingForClimbStart
}

[System.Serializable]
public enum CheckingType
{
    Normal,
    Turning,
    Falling
}

[System.Serializable]
public enum StickType
{
    Normal,
    Jumping
}
