﻿using Emberpoint.Core.GameObjects.Abstracts;
using Emberpoint.Core.GameObjects.Map;

namespace Emberpoint.Core.GameObjects.Blueprints.Objects
{
    public class BasementBlueprint : Blueprint<EmberCell>
    {
        public override Blueprint<EmberCell> StairsUpBlueprint { get { return new GroundFloorBlueprint(); } }
    }
}
