using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
[CreateAssetMenu(fileName = "New Craft Action", menuName = "Abilities/CraftAction")]
public class CraftActionAbility : DirectAbility {

    public override bool CanUseOn(GameObject target, BaseCharacter source) {
        return true;
    }

}

}