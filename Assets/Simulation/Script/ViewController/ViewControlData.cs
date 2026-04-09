using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Starscape.Simulation
{
    [Serializable]
    public class ViewControlData
    {
        public enum Type
        {
            [LabelText("固定视角")]
            FIXED,
        }

        public string Id;
        public Camera Camera;
        public Type ControlType;
        public bool IsBackToPlayer = true;

        /// <summary>
        /// 相机固定位置
        /// </summary>
        [BoxGroup("固定视角")]
        [ShowIf("ControlType", Type.FIXED)]
        public Transform FixedPosition;
        /// <summary>
        /// 相机固定观察目标
        /// </summary>
        [BoxGroup("固定视角")]
        [ShowIf("ControlType", Type.FIXED)]
        public Transform FixedLookAt;
    }
}
