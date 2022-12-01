using StarterAssets;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IK_Animator : MonoBehaviour
{
    [Header("Animation Settings")]
    public bool grounded;
    public bool animating;
    public float GroundDistance;
    public LayerMask groundLayer;
    public AnimationCurve leftFootUp;
    public AnimationCurve rightFootUp;
    public AnimationCurve punchL;
    public AnimationCurve punchR;

    [Space]

    [Header("References")]
    public Animator anim;
    public Rig IKrig;

    public MultiAimConstraint headAim;
    public MultiAimConstraint bodyAim;
    public TwoBoneIKConstraint leftFootIK;
    public TwoBoneIKConstraint rightFootIK;
    public TwoBoneIKConstraint handIK;

    [Space]

    [SerializeField] private ExtractTransformConstraint extractConstraintPelvis;

    [Space]

    [SerializeField] private ExtractTransformConstraint extractConstraintL;
    [SerializeField] private ExtractTransformConstraint extractConstraintR;

    [Space]
    [Header("Transforms")]
    public Transform generalTarget;

    [Tooltip("Not Actual Pelvis Bone, Substitute in Pelvis MultiPosition Constraint (Source Object 0)")]
    [SerializeField] private Transform pelvis;

    [Space]

    [Tooltip("Transform Facing Forward of Left Foot")]
    [SerializeField] private Transform footForwardL;
    [Tooltip("Transform Facing Forward of Left Foot")]
    [SerializeField] private Transform footForwardR;

    [Space]

    [Header("Presets")]
    [SerializeField] private float footOffset;

    [Tooltip("Pelvis Offset Based on Specific Model, Adjust in Start Of Game")]
    [SerializeField] private float pelvisOffset;

    [Space]

    //limits for adjusting pelvis
    [SerializeField] private float maxStepHeight;
    [SerializeField] private float minStepHeight;

    [Space]

    //blend/lerp speed
    [SerializeField] private float pelvisMoveSpeed;
    [SerializeField] private float feetIkSpeed;

    //transform created as child of footForward to adjust for relative rotation 
    private Transform _footPlacementL;
    private Transform _footPlacementR;

    //for blending/lerping since values get reset every frame in animation cycle
    private Vector3 _lastPelvisPosition;

    private float animationTime;
    //just the y component
    private float _lastIkPositionL;
    private float _lastIkPositionR;

    private Quaternion _lastIkRotationL;
    private Quaternion _lastIkRotationR;

    private void Start()
    {
        //Create Child Transforms for relative Rotation IK and assign to initial foot rotation to get rotation offset
        GameObject footPlacementObjL = new GameObject("FootPlacementL");
        _footPlacementL = footPlacementObjL.transform;
        _footPlacementL.SetParent(footForwardL);
        _footPlacementL.localPosition = Vector3.zero;
        _footPlacementL.rotation = leftFootIK.data.tip.rotation;

        GameObject footPlacementObjR = new GameObject("FootPlacementR");
        _footPlacementR = footPlacementObjR.transform;
        _footPlacementR.SetParent(footForwardR);
        _footPlacementR.localPosition = Vector3.zero;
        _footPlacementR.rotation = rightFootIK.data.tip.rotation;

        _lastIkPositionL = extractConstraintL.data.position.y;
        _lastIkPositionR = extractConstraintL.data.position.y;

        _lastIkRotationL = extractConstraintL.data.rotation;
        _lastIkRotationR = extractConstraintL.data.rotation;
    }

    private void Update()
    {
        //Get all original Bone Positions
        Vector3 pelvisPosition = extractConstraintPelvis.data.position;
        pelvisPosition.y += pelvisOffset;

        Vector3 bonePositionL = extractConstraintL.data.position;
        Vector3 bonePositionR = extractConstraintR.data.position;

        Quaternion boneRotationL = extractConstraintL.data.rotation;
        Quaternion boneRotationR = extractConstraintR.data.rotation;

        GroundCheck(bonePositionL, bonePositionR);

        IKrig.weight = grounded ? 1f : 0f;

        if (!grounded)
        {
            _lastPelvisPosition = pelvisPosition;

            _lastIkPositionL = bonePositionL.y;
            _lastIkPositionR = bonePositionR.y;

            _lastIkRotationL = boneRotationL;
            _lastIkRotationR = boneRotationR;

            return;
        }

        FootRig(bonePositionL, bonePositionR, boneRotationL, boneRotationR, pelvisPosition);
    }

    private void GroundCheck(Vector3 leftFootPos, Vector3 rightFootPos)
    {
        //Feet Check
        Vector3 originL = leftFootPos;
        originL.y += maxStepHeight;
        float maxDistance = maxStepHeight * 2f;

        bool leftHit = Physics.Raycast(originL, Vector3.down, out RaycastHit hitL, maxDistance, groundLayer);

        //right Foot Raycast
        Vector3 originR = rightFootPos;
        originR.y += maxStepHeight;

        bool rightHit = Physics.Raycast(originR, Vector3.down, out RaycastHit hitR, maxDistance, groundLayer);

        //Leg displacement
        //float delta = hitL.point.y - hitR.point.y;

        grounded = (rightHit || leftHit);
    }

    private void FootRig(Vector3 leftFootPos, Vector3 rightFootPos, Quaternion leftFootRot, Quaternion rightFootRot, Vector3 pelvisPos)
    {
        Color checkGround = Color.red;

        //Feet IK
        Vector3 originL = leftFootPos;
        originL.y += maxStepHeight;
        float maxDistance = maxStepHeight * 2f;

        bool leftHit = Physics.Raycast(originL, Vector3.down, out RaycastHit hitL, maxDistance, groundLayer);
        if (leftHit)
        {
            checkGround = Color.green;
            Debug.DrawLine(originL, hitL.point, checkGround);
        }
        else
        {
            Debug.DrawRay(originL, Vector3.down * maxDistance, checkGround);
        }

        //right Foot Raycast
        Vector3 originR = rightFootPos;

        originR.y += maxStepHeight;

        bool rightHit = Physics.Raycast(originR, Vector3.down, out RaycastHit hitR, maxDistance, groundLayer);
        if (rightHit)
        {
            checkGround = Color.green;
            Debug.DrawLine(originR, hitR.point, checkGround);
        }
        else
        {
            Debug.DrawRay(originR, Vector3.down * maxDistance, checkGround);
        }

        bool hit = leftHit && rightHit;

        //displacement between legs
        float delta = hitL.point.y - hitR.point.y;

        //distance between legs
        float offset = Mathf.Abs(delta);

        bool adjustPelvis = offset <= maxStepHeight && offset >= minStepHeight && hit;

        if (adjustPelvis)
        {
            //move pelvis down (always down)
            pelvisPos.y -= offset;

            //re-adjust right foot for pelvis movement
            if (delta < 0)
            {
                rightFootPos.y += offset;

                //rotation R
                rightFootRot = SolveRotation(hitR.normal, footForwardR, ref _footPlacementR);
            }
            else if (delta > 0)
            {
                leftFootPos.y += offset;

                //rotation L
                leftFootRot = SolveRotation(hitL.normal, footForwardL, ref _footPlacementL);
            }
        }

        //Apply lerped pelvis readjustment, foot ik position and rotation

        //pelvis
        AdjustPelvis(pelvisPos);

        //IK
        float t = feetIkSpeed * Time.deltaTime;

        //ik position
        ApplyIkPosition(ref _lastIkPositionL, ref leftFootIK, leftFootPos, t);
        ApplyIkPosition(ref _lastIkPositionR, ref rightFootIK, rightFootPos, t);

        //ik rotation
        SetIkRotationWeight();
        ApplyIkRotation(ref _lastIkRotationL, ref leftFootIK, leftFootRot, t);
        ApplyIkRotation(ref _lastIkRotationR, ref rightFootIK, rightFootRot, t);
    }

    private void AdjustPelvis(Vector3 pelvisPosition)
    {
        _lastPelvisPosition = Vector3.Lerp(_lastPelvisPosition, pelvisPosition, pelvisMoveSpeed * Time.deltaTime);

        pelvis.position = _lastPelvisPosition;
    }

    private void ApplyIkPosition(ref float lastIkPosition, ref TwoBoneIKConstraint ikConstraint, Vector3 bonePosition, float t)
    {
        //ik position R
        lastIkPosition = Mathf.Lerp(lastIkPosition, bonePosition.y, t);

        bonePosition.y = lastIkPosition + footOffset;

        ikConstraint.data.target.position = bonePosition;
    }

    private void ApplyIkRotation(ref Quaternion lastIkRotation, ref TwoBoneIKConstraint ikConstraint, Quaternion boneRotation, float t)
    {
        lastIkRotation = Quaternion.Lerp(lastIkRotation, boneRotation, t);

        ikConstraint.data.target.rotation = lastIkRotation;
    }

    private void SetIkRotationWeight()
    {
        //float clipLength = anim.GetCurrentAnimatorStateInfo(0).length;
        animationTime = Mathf.Repeat(animationTime + Time.deltaTime, 1);

        //Get and Set Ik rotation weight
        float weightL = 1f - leftFootUp.Evaluate(animationTime);
        float weightR = 1f - rightFootUp.Evaluate(animationTime);

        leftFootIK.data.targetRotationWeight = weightL;
        rightFootIK.data.targetRotationWeight = weightR;
    }

    private Quaternion SolveRotation(Vector3 normal, Transform footForward, ref Transform footPlacement)
    {
        Vector3 localNormal = transform.InverseTransformDirection(normal);

        footForward.localRotation = Quaternion.FromToRotation(Vector3.up, localNormal);

        return footPlacement.rotation;
    }
}
