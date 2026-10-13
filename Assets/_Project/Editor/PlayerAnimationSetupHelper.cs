using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace TheLastEmber.Editor
{
    [InitializeOnLoad]
    public static class PlayerAnimationSetupHelper
    {
        private const string AnimFolder = "Assets/_Project/Art/Animations/Player/";
        private const string ControllerPath = "Assets/_Project/Art/Animations/Player/PlayerAnimatorController.controller";

        static PlayerAnimationSetupHelper()
        {
            EditorApplication.delayCall += AutoBuildIfMissing;
        }

        private static void AutoBuildIfMissing()
        {
            if (!System.IO.File.Exists(ControllerPath))
            {
                BuildAnimatorController();
            }
        }

        [MenuItem("The Last Ember/Animation/Build Player Animator Controller")]
        public static void BuildAnimatorController()
        {
            // 1. Tạo hoặc tải Animator Controller
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }

            // Xóa parameters cũ và tạo mới
            controller.parameters = new AnimatorControllerParameter[0];
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            // Tìm clip trong các file FBX
            AnimationClip idleClip = LoadClipFromFbx(AnimFolder + "Sheathing Sword.fbx");
            AnimationClip runClip = LoadClipFromFbx(AnimFolder + "Run With Sword.fbx");
            AnimationClip attackClip = LoadClipFromFbx(AnimFolder + "Stable Sword Inward Slash.fbx");

            if (runClip == null) runClip = LoadClipFromFbx(AnimFolder + "Strut Walking.fbx");
            if (attackClip == null) attackClip = LoadClipFromFbx(AnimFolder + "One Hand Sword Combo.fbx");

            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
            rootStateMachine.states = new ChildAnimatorState[0];

            // Tạo các State
            AnimatorState idleState = rootStateMachine.AddState("Idle");
            idleState.motion = idleClip;

            AnimatorState runState = rootStateMachine.AddState("Run");
            runState.motion = runClip;

            AnimatorState attackState = rootStateMachine.AddState("Attack");
            attackState.motion = attackClip;

            rootStateMachine.defaultState = idleState;

            // Transition: Idle -> Run
            AnimatorStateTransition toRun = idleState.AddTransition(runState);
            toRun.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
            toRun.hasExitTime = false;
            toRun.duration = 0.15f;

            // Transition: Run -> Idle
            AnimatorStateTransition toIdle = runState.AddTransition(idleState);
            toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
            toIdle.hasExitTime = false;
            toIdle.duration = 0.15f;

            // Transition: AnyState -> Attack
            AnimatorStateTransition toAttack = rootStateMachine.AddAnyStateTransition(attackState);
            toAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
            toAttack.hasExitTime = false;
            toAttack.duration = 0.08f;

            // Transition: Attack -> Idle
            AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 0.85f;
            attackToIdle.duration = 0.2f;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green>[Animation Setup]</color> Đã tạo xong PlayerAnimatorController!");
        }

        private static AnimationClip LoadClipFromFbx(string fbxPath)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            foreach (Object a in assets)
            {
                if (a is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                {
                    return clip;
                }
            }
            return null;
        }
    }
}
