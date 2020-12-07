using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTruckInteraction
{
    using Rage;
    
    internal static class Settings
    {
        public static InitializationFile INI = new InitializationFile(@"Plugins\UtilityTruckInteraction.ini");

        public static class Controls
        {
            public static GameControl RaiseBucket { get; } = INI.ReadEnum("Controls", "RaiseBucket", GameControl.VehicleFlySelectTargetRight);
            public static GameControl LowerBucket { get; } = INI.ReadEnum("Controls", "LowerBucket", GameControl.VehicleFlySelectTargetLeft);
            public static GameControl RotateBoomLeft { get; } = INI.ReadEnum("Controls", "RotateBoomLeft", GameControl.VehicleFlyRollLeftOnly);
            public static GameControl RotateBoomRight { get; } = INI.ReadEnum("Controls", "RotateBoomRight", GameControl.VehicleFlyRollRightOnly);
            public static GameControl ResetBucket { get; } = INI.ReadEnum("Controls", "ResetBucket", GameControl.VehicleFlyAttackCamera);
            public static GameControl EnterExitInteractionMode { get; } = INI.ReadEnum("Controls", "EnterExitInteractionMode", GameControl.VehicleDuck);
            public static GameControl ClimbIntoBucket { get; } = INI.ReadEnum("Controls", "ClimbIntoBucket", GameControl.Jump);
        }

        public static class Bucket
        {
            private static float OffsetX { get; } = INI.ReadSingle("Bucket", "OffsetX", 0f);
            private static float OffsetY { get; } = INI.ReadSingle("Bucket", "OffsetY", -0.35f);
            private static float OffsetZ { get; } = INI.ReadSingle("Bucket", "OffsetZ", -0.3f);
            private static float SizeX { get; } = INI.ReadSingle("Bucket", "SizeX", 1.1f);
            private static float SizeY { get; } = INI.ReadSingle("Bucket", "SizeY", 0.7f);
            private static float SizeZ { get; } = INI.ReadSingle("Bucket", "SizeZ", 1.5f);

            public static Vector3 BucketOffset { get; } = new Vector3(OffsetX, OffsetY, OffsetZ);
            public static Vector3 BucketSize { get; } = new Vector3(SizeX, SizeY, SizeZ);

            public static float RaiseSpeed { get; } = INI.ReadSingle("Bucket", "RaiseSpeed", 0.3f);
            public static float LowerSpeed { get; } = INI.ReadSingle("Bucket", "LowerSpeed", 0.25f);
            public static float RotateSpeed { get; } = INI.ReadSingle("Bucket", "RotateSpeed", 0.15f);
            public static float ClimbZoneDist { get; } = INI.ReadSingle("Bucket", "ClimbZoneDist", 2.0f);
            public static bool EnableCollisionProps { get; } = INI.ReadBoolean("Bucket", "EnableCollisionProps", true);
        }

        public static class Misc
        {
            public static bool EnableEngine { get; } = INI.ReadBoolean("Misc Options", "EnableEngine", true);
            public static float LimitSpeed { get; } = INI.ReadSingle("Misc Options", "LimitSpeed", 5f);
            public static bool AutoReset { get; } = INI.ReadBoolean("Misc Options", "AutoReset", true);
            public static bool AutoAttachOnMove { get; } = INI.ReadBoolean("Misc Options", "AutoAttachOnMove", true);
        }
    }
}
