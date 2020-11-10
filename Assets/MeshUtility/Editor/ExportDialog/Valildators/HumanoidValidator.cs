using System.Collections.Generic;
using System.Linq;
using MeshUtility.M17N;
using UnityEngine;

namespace MeshUtility.Validators
{
    public static class HumanoidValidator
    {
        public enum ValidationMessages
        {
            [LangMsg(Languages.ja, "回転・拡大縮小もしくはWeightの無いBlendShapeが含まれています。正規化が必用です。Setting の PoseFreeze を有効にしてください")]
            [LangMsg(Languages.en, " Normalization is required. There are nodes (child GameObject) where rotation and scaling or blendshape without bone weight are not default. Please enable PoseFreeze")]
            ROTATION_OR_SCALEING_INCLUDED_IN_NODE,

            [LangMsg(Languages.ja, "正規化済みです。Setting の PoseFreeze は不要です")]
            [LangMsg(Languages.en, "Normalization has been done. PoseFreeze is not required")]
            IS_POSE_FREEZE_DONE,

            [LangMsg(Languages.ja, "ExportRootに Animator がありません")]
            [LangMsg(Languages.en, "No Animator in ExportRoot")]
            NO_ANIMATOR,

            [LangMsg(Languages.ja, "Z+ 向きにしてください")]
            [LangMsg(Languages.en, "The model needs to face the positive Z-axis")]
            FACE_Z_POSITIVE_DIRECTION,

            [LangMsg(Languages.ja, "ExportRootの Animator に Avatar がありません")]
            [LangMsg(Languages.en, "No Avatar in ExportRoot's Animator")]
            NO_AVATAR_IN_ANIMATOR,

            [LangMsg(Languages.ja, "ExportRootの Animator.Avatar が不正です")]
            [LangMsg(Languages.en, "Animator.avatar in ExportRoot is not valid")]
            AVATAR_IS_NOT_VALID,

            [LangMsg(Languages.ja, "ExportRootの Animator.Avatar がヒューマノイドではありません。FBX importer の Rig で設定してください")]
            [LangMsg(Languages.en, "Animator.avatar is not humanoid. Please change model's AnimationType to humanoid")]
            AVATAR_IS_NOT_HUMANOID,

            [LangMsg(Languages.ja, "Jaw(顎)ボーンが含まれています。意図していない場合は設定解除をおすすめします。FBX importer の rig 設定から変更できます")]
            [LangMsg(Languages.en, "Jaw bone is included. It may not what you intended. Please check the humanoid avatar setting screen")]
            JAW_BONE_IS_INCLUDED,
        }

        public static bool HasRotationOrScale(GameObject root)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>())
            {
                if (t.localRotation != Quaternion.identity)
                {
                    return true;
                }
                if (t.localScale != Vector3.one)
                {
                    return true;
                }
            }

            return false;
        }

        static Vector3 GetForward(Transform l, Transform r)
        {
            if (l == null || r == null)
            {
                return Vector3.zero;
            }
            var lr = (r.position - l.position).normalized;
            return Vector3.Cross(lr, Vector3.up);
        }

        public static IReadOnlyList<MeshExportInfo> MeshInformations;
        public static bool EnableFreeze;

        public static IEnumerable<Validation> Validate(GameObject ExportRoot)
        {
            if (!ExportRoot)
            {
                yield break;
            }

            if (MeshInformations != null)
            {
                if (HasRotationOrScale(ExportRoot) || MeshInformations.Any(x => x.ExportBlendShapeCount > 0 && !x.HasSkinning))
                {
                    // 正規化必用
                    if (EnableFreeze)
                    {
                        // する
                        yield return Validation.Info("PoseFreeze checked. OK");
                    }
                    else
                    {
                        // しない
                        yield return Validation.Warning(ValidationMessages.ROTATION_OR_SCALEING_INCLUDED_IN_NODE.Msg());
                    }
                }
                else
                {
                    // 不要
                    if (EnableFreeze)
                    {
                        // する
                        yield return Validation.Warning(ValidationMessages.IS_POSE_FREEZE_DONE.Msg());
                    }
                    else
                    {
                        // しない
                        yield return Validation.Info("Root OK");
                    }
                }
            }

            //
            // animator
            //
            var animator = ExportRoot.GetComponent<Animator>();
            if (animator == null)
            {
                yield return Validation.Critical(ValidationMessages.NO_ANIMATOR.Msg());
                yield break;
            }

            // avatar
            var avatar = animator.avatar;
            if (avatar == null)
            {
                yield return Validation.Critical(ValidationMessages.NO_AVATAR_IN_ANIMATOR.Msg());
                yield break;
            }
            if (!avatar.isValid)
            {
                yield return Validation.Critical(ValidationMessages.AVATAR_IS_NOT_VALID.Msg());
                yield break;
            }
            if (!avatar.isHuman)
            {
                yield return Validation.Critical(ValidationMessages.AVATAR_IS_NOT_HUMANOID.Msg());
                yield break;
            }
            // direction
            {
                var l = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                var r = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                var f = GetForward(l, r);
                if (Vector3.Dot(f, Vector3.forward) < 0.8f)
                {
                    yield return Validation.Critical(ValidationMessages.FACE_Z_POSITIVE_DIRECTION.Msg());
                    yield break;
                }
            }

            var jaw = animator.GetBoneTransform(HumanBodyBones.Jaw);
            if (jaw != null)
            {
                yield return Validation.Warning(ValidationMessages.JAW_BONE_IS_INCLUDED.Msg());
            }
            else
            {
                yield return Validation.Info("Animator OK");
            }
        }
    }
}
