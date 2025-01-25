using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EdTech
{
    [Serializable]
    [CreateAssetMenu(menuName = "EdTech/SOSpriteMap")]
    public class SOSpriteMap : ScriptableObject
    {
        public SpriteMapItem[] Items;
    }
}
