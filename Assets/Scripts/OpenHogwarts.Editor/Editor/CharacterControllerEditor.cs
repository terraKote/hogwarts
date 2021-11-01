using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;

namespace OpenHogwarts.Editor
{
    [EditorTool("Edit Character Controller", typeof(CharacterController))]
    public class CharacterControllerTool : EditorTool
    {
        private CapsuleBoundsHandle capsuleBoundsHandle = new CapsuleBoundsHandle();
        private int axis = 1;

        public override void OnToolGUI(EditorWindow window)
        {
            CharacterController characterController = (CharacterController)target;

            using (new Handles.DrawingScope(Matrix4x4.TRS(characterController.transform.position, characterController.transform.rotation, Vector3.one)))
            {
                CopyColliderPropertiesToHandle(characterController);
                EditorGUI.BeginChangeCheck();

                capsuleBoundsHandle.DrawHandle();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(characterController, $"Modify {ObjectNames.NicifyVariableName(target.GetType().Name)}");
                    CopyHandlePropertiesToCollider(characterController);
                }
            }

            base.OnToolGUI(window);
        }

        private void CopyColliderPropertiesToHandle(CharacterController characterController)
        {
            capsuleBoundsHandle.center = TransformColliderCenterToHandleSpace(characterController.transform, characterController.center);

            float radiusScaleFactor;
            Vector3 sizeScale = GetCapsuleColliderHandleScale(characterController.transform.lossyScale, axis, out radiusScaleFactor);

            capsuleBoundsHandle.height = capsuleBoundsHandle.radius = 0f;
            capsuleBoundsHandle.height = characterController.height * Mathf.Abs(sizeScale[axis]);
            capsuleBoundsHandle.radius = characterController.radius * radiusScaleFactor;

            capsuleBoundsHandle.heightAxis = CapsuleBoundsHandle.HeightAxis.Y;
        }

        private void CopyHandlePropertiesToCollider(CharacterController characterController)
        {
            characterController.center = TransformHandleCenterToColliderSpace(characterController.transform, capsuleBoundsHandle.center);

            float radiusScaleFactor;
            Vector3 sizeScale = GetCapsuleColliderHandleScale(characterController.transform.lossyScale, axis, out radiusScaleFactor);
            sizeScale = InvertScaleVector(sizeScale);

            // only apply changes to collider radius/height if scale factor from transform is non-zero
            if (radiusScaleFactor != 0f)
                characterController.radius = capsuleBoundsHandle.radius / radiusScaleFactor;

            if (sizeScale[axis] != 0f)
                characterController.height = capsuleBoundsHandle.height * Mathf.Abs(sizeScale[axis]);
        }

        private static Vector3 TransformColliderCenterToHandleSpace(Transform colliderTransform, Vector3 colliderCenter)
        {
            return Handles.inverseMatrix * (colliderTransform.localToWorldMatrix * colliderCenter);
        }

        private static Vector3 TransformHandleCenterToColliderSpace(Transform colliderTransform, Vector3 handleCenter)
        {
            return colliderTransform.localToWorldMatrix.inverse * (Handles.matrix * handleCenter);
        }

        private static Vector3 GetCapsuleColliderHandleScale(Vector3 lossyScale, int capsuleDirection, out float radiusScaleFactor)
        {
            radiusScaleFactor = 0f;

            for (int axis = 0; axis < 3; ++axis)
            {
                if (axis != capsuleDirection)
                    radiusScaleFactor = Mathf.Max(radiusScaleFactor, Mathf.Abs(lossyScale[axis]));
            }

            for (int axis = 0; axis < 3; ++axis)
            {
                if (axis != capsuleDirection)
                    lossyScale[axis] = Mathf.Sign(lossyScale[axis]) * radiusScaleFactor;
            }

            return lossyScale;
        }

        private static Vector3 InvertScaleVector(Vector3 scaleVector)
        {
            for (int axis = 0; axis < 3; ++axis)
                scaleVector[axis] = scaleVector[axis] == 0f ? 0f : 1f / scaleVector[axis];

            return scaleVector;
        }
    }

    [CustomEditor(typeof(CharacterController))]
    public class CharacterControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty slopeLimit;
        private SerializedProperty stepOffset;
        private SerializedProperty skinWidth;
        private SerializedProperty minMoveDistance;

        private CharacterController characterController;
        private bool drawSkinWidthGizmo;
        private int gizmoResolution = 5;

        private void OnEnable()
        {
            slopeLimit = serializedObject.FindProperty("m_SlopeLimit");
            stepOffset = serializedObject.FindProperty("m_StepOffset");
            skinWidth = serializedObject.FindProperty("m_SkinWidth");
            minMoveDistance = serializedObject.FindProperty("m_MinMoveDistance");

            characterController = (CharacterController)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.EditorToolbarForTarget(EditorGUIUtility.TrTempContent("Edit Collider"), target);

            bool previousValue = drawSkinWidthGizmo;
            drawSkinWidthGizmo = EditorGUILayout.Toggle(new GUIContent("Draw Skin Width Gizmo"), drawSkinWidthGizmo);

            if (previousValue != drawSkinWidthGizmo) SceneView.RepaintAll();

            if (GUILayout.Button("Recenter"))
            {
                characterController.center = new Vector3(0, characterController.center.y, 0);
                SceneView.RepaintAll();
            }

            EditorGUILayout.PropertyField(slopeLimit);
            EditorGUILayout.PropertyField(stepOffset);
            EditorGUILayout.PropertyField(skinWidth);
            EditorGUILayout.PropertyField(minMoveDistance);
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (!drawSkinWidthGizmo) return;

            Handles.color = Color.cyan;
            Vector3 offset = new Vector3(characterController.center.x, 0, characterController.center.z);

            for (int i = 0; i < gizmoResolution; i++)
            {
                Handles.DrawWireDisc(characterController.transform.position - offset + Vector3.Lerp(Vector3.zero, new Vector3(0, characterController.height, 0), (float)i / gizmoResolution), Vector3.up, skinWidth.floatValue);
            }
        }
    }
}
