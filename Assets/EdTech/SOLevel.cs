using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EdTech
{
    [Serializable]
    [CreateAssetMenu(menuName = "EdTech/SOLevel")]
    public class SOLevel : ScriptableObject
    {
        public EdLevel LevelDef;
    }
}
