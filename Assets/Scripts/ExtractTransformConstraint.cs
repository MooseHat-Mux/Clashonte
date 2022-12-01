using UnityEngine.Animations.Rigging;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Animation Rigging/Extract Transform Constraint")]
public class ExtractTransformConstraint : RigConstraint<ExtractTransformConstraintJob,
    ExtractTransformConstraintData, ExtractTransformConstraintJobBinder>
{

}