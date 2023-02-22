using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    public interface ITPoseProvider
    {
        /// <summary>
        /// * world は ModelRoot を意図していることに注意
        /// </summary>
        Vector3 HipTPoseWorldPosition { get; }

        /// <summary>
        /// * world は ModelRoot を意図していることに注意
        /// * bone 無いときはどう振る舞うべきか
        /// </summary>
        /// <param name="bone"></param>
        /// <returns></returns>
        Quaternion GetBoneTPoseWorldRotation(HumanBodyBones bone);

        /// <summary>
        /// ボーンを親ボーンとセットで列挙する</returns>
        /// </summary>
        /// <returns>hips の場合は parent は HumanBodyBones.LastBone で null を表す</returns>
        IEnumerable<(HumanBodyBones Head, HumanBodyBones Parent)> EnumerateBones();
    }
}
