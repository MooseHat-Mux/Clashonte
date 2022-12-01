using System.Collections;
using Freya;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class ProceduralManager : MonoBehaviour
{
    [Header("Physic Settings")]
    public bool grabbed;
    public bool punching;
    public bool kicking;
    public bool grabbing;
    public bool leftSide;

    public float punchAngle;
    public float strikeSpeed;
    public float armLength;
    public float legLength;
    public float hitDuration;
    public float strikeWiggleRadius;

    public AnimationCurve punchCurve;
    public AnimationCurve kickCurve;
    public AnimationCurve throwCurve;

    [Space]

    [Header("Body References")]
    public HealthScript healthController;
    public Rigidbody[] ragdollBodies;

    [Space]

    [Header("IK References")]
    public Transform currentTarget;
    public Transform defaultHeadTarget;
    public Transform defaultBodyTarget;
    public Transform defaultHipTarget;

    public MultiAimConstraint headIk;
    public MultiAimConstraint bodyIk;
    public TwoBoneIKConstraint leftArm;
    public TwoBoneIKConstraint rightArm;
    public TwoBoneIKConstraint leftLeg;
    public TwoBoneIKConstraint rightLeg;

    [SerializeField] private ExtractTransformConstraint extractConstraintHandL;
    [SerializeField] private ExtractTransformConstraint extractConstraintHandR;
    [SerializeField] private ExtractTransformConstraint extractConstraintFootL;
    [SerializeField] private ExtractTransformConstraint extractConstraintFootR;

    private const float threshold = 0.1f;

    public void OnPunch(InputValue value)
    {
        if (!grabbed && !punching && !kicking)
        {
            if (value.isPressed)
            {
                StartCoroutine(GiveLove());
            }
        }
    }

    public void OnKick(InputValue value)
    {
        if (!grabbed)
        {
            if (value.isPressed)
            {
                StartCoroutine(GiveStrength());
            }
        }
    }

    public void OnThrow(InputValue value)
    {
        if (!grabbed)
        {
            if (value.isPressed)
            {
                StartCoroutine(GivePower());
            }
        }
    }

    IEnumerator GiveLove()
    {
        punching = true;
        if (currentTarget == null)
        {
            currentTarget = defaultHeadTarget;
        }

        Transform guidedTarget;
        if (leftSide)
        {
            guidedTarget = leftArm.data.target;
            leftArm.weight = 1;
        }
        else
        {
            guidedTarget = rightArm.data.target;
            rightArm.weight = 1;
        }

        Bezier3D trajec = GetArmPath();
        float t = 0;
        while (t < 1)
        {
            Vector3 targetPos = trajec.Eval(t);
            Vector3 vrot = Quaternion.LookRotation(targetPos - guidedTarget.position, Vector3.up).eulerAngles;
            vrot.x += punchAngle;
            guidedTarget.rotation = Quaternion.Euler(vrot);
            guidedTarget.position = targetPos;

            t += Time.deltaTime * strikeSpeed
                * punchCurve.Evaluate(1.0f - (Vector3.Distance(guidedTarget.position, currentTarget.position) / armLength));

            yield return null;
        }

        punching = false;
        while (!punching && t > 0)
        {
            Vector3 targetPos = trajec.Eval(t);
            guidedTarget.position = targetPos;

            t -= Time.deltaTime * strikeSpeed;

            yield return null;
        }

        leftArm.weight = 0;
        rightArm.weight = 0;
    }

    IEnumerator GiveStrength()
    {
        yield return null;
    }

    IEnumerator GivePower()
    {
        yield return null;
    }

    IEnumerator EndLove()
    {
        yield return null;
    }

    IEnumerator EnableHit()
    {
        yield return new WaitForSeconds(hitDuration);
    }

    public void EnableRagdoll()
    {

    }

    private Bezier3D GetArmPath()
    {
        Bezier3D trajectory;
        leftSide = Freya.Random.Value < 0.5f;
        if (leftSide)
        {
            trajectory = new Bezier3D(new Vector3[]
            {
               extractConstraintHandL.data.position,
               Freya.Random.InUnitSphere * strikeWiggleRadius + leftArm.data.mid.position,
               currentTarget.position
            });
        }
        else
        {
            trajectory = new Bezier3D(new Vector3[]
            {
               extractConstraintHandR.data.position,
               Freya.Random.InUnitSphere * strikeWiggleRadius + leftArm.data.mid.position,
               currentTarget.position
            });
        }

        return trajectory;
    }

    private Bezier3D GetLegPath()
    {
        Bezier3D trajectory;
        leftSide = Freya.Random.Value < 0.5f;
        if (leftSide)
        {
            trajectory = new Bezier3D(new Vector3[]
            {
               extractConstraintHandL.data.position,
               Freya.Random.InUnitSphere * strikeWiggleRadius + leftArm.data.mid.position,
               currentTarget.position
            });
        }
        else
        {
            trajectory = new Bezier3D(new Vector3[]
            {
               extractConstraintHandR.data.position,
               Freya.Random.InUnitSphere * strikeWiggleRadius + leftArm.data.mid.position,
               currentTarget.position
            });
        }

        return trajectory;
    }
}