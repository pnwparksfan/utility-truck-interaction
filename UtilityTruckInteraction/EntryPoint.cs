[assembly: Rage.Attributes.Plugin("Utility Truck Interaction", Description = "Interact with utility truck lift buckets", Author = "PNWParksFan", PrefersSingleInstance = true)]

namespace UtilityTruckInteraction
{
    using Rage;
    using Rage.Attributes;
    using System.Drawing;
    using System.Reflection;

    public class EntryPoint
    {

        private static void Main()
        {
            Game.LogTrivial("Loading Utility Truck Interaction by PNWParksFan");
            Game.LogTrivial("Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            GameFiber.ExecuteWhile(PlayerControl.Process, () => true);
        }

        private static BucketController controller => PlayerControl.ActiveBucket;

        [ConsoleCommand(Name = "RaiseBucket", Description = "Raise current active utility bucket until stopped. No effect if no active bucket.")]
        private static void Raise()
        {
            controller?.Raise();
        }

        [ConsoleCommand(Name = "LowerBucket", Description = "Lower current active utility bucket until stopped. No effect if no active bucket.")]
        private static void Lower()
        {
            controller?.Lower();
        }

        [ConsoleCommand(Name = "RotateBucketRight", Description = "Rotate clockwise current active utility truck boom until stopped. No effect if no active bucket.")]
        private static void RotateRight()
        {
            controller?.RotateRight();
        }

        [ConsoleCommand(Name = "RotateBucketLeft", Description = "Rotate counterclockwise current active utility truck boom until stopped. No effect if no active bucket.")]
        private static void RotateLeft()
        {
            controller?.RotateLeft();
        }

        [ConsoleCommand(Name = "StopBucketMovement", Description = "Stop the current active utility truck bucket/boom from moving. No effect if no active bucket.")]
        private static void StopBoom()
        {
            controller?.Stop();
        }

        [ConsoleCommand(Name = "ResetBucket", Description = "Reset the current active utility bucket/boom to its default retracted position. No effect if no active bucket.")]
        private static void ResetBoom()
        {
            Game.LogTrivial("Started resetting boom");
            controller?.Reset();
            Game.LogTrivial("Finished resetting boom");
        }

        [ConsoleCommand(Name = "SetBucketPropAttachmentsEnabled", Description = "Enable/disable invisible attached props that add collisions to the bucket and boom on the current active utility truck. No effect if no active bucket.")]
        private static void SetBucketAttachments([ConsoleCommandParameter(Name = "PropsEnabled", Description = "true to enable props, false to disable")] bool enabled)
        {
            if (controller != null) controller.AttachmentsEnabled = enabled;
        }

        private static GameFiber debugFiber;
        [ConsoleCommand(Name = "ShowBucketDebugLines", Description = "Enable/disable debug lines showing the position of the utility truck arm and bucket")]
        private static void DebugBones([ConsoleCommandParameter(Name = "Enabled")] bool enabled)
        {
            debugFiber?.Abort();
            if (enabled)
            {
                debugFiber = GameFiber.ExecuteNewWhile(() =>
                {
                    Vehicle truck = controller.Truck;
                    Vector3 a1 = truck.GetBonePosition("arm_1");
                    Vector3 a2 = truck.GetBonePosition("arm_2");
                    Vector3 rb = truck.GetBonePosition("rotating_base");
                    Vector3 bk = truck.GetBonePosition("bucket");

                    Quaternion bq = truck.GetBoneOrientation("bucket");
                    Vector3 bucketBox = controller.GetBucketPosition;
                    Debug.DrawWireBox(bucketBox, bq, Settings.Bucket.BucketSize, Color.Purple);

                    Debug.DrawSphere(a1, 0.3f, Color.Red);
                    Debug.DrawSphere(a2, 0.3f, Color.Green);
                    Debug.DrawSphere(rb, 0.3f, Color.Orange);
                    Debug.DrawSphere(bk, 0.3f, Color.Blue);

                    Debug.DrawLine(rb, a1, Color.HotPink);
                    Debug.DrawLine(a1, a2, Color.HotPink);
                    Debug.DrawLine(a2, bk, Color.HotPink);

                    Vector3 py1 = Game.LocalPlayer.Character.GetOffsetPositionFront(1f);
                    Vector3 py2 = Game.LocalPlayer.Character.GetOffsetPositionFront(-1f);
                    Vector3 pz1 = Game.LocalPlayer.Character.GetOffsetPositionUp(1f);
                    Vector3 pz2 = Game.LocalPlayer.Character.GetOffsetPositionUp(-1f);
                    Vector3 px1 = Game.LocalPlayer.Character.GetOffsetPositionRight(1f);
                    Vector3 px2 = Game.LocalPlayer.Character.GetOffsetPositionRight(-1f);

                    Color c = controller.IsEntityInBucket(Game.LocalPlayer.Character) ? Color.Green : Color.Red;
                    Debug.DrawLine(py1, py2, c);
                    Debug.DrawLine(px1, px2, c);
                    Debug.DrawLine(pz1, pz2, c);

                }, () => controller != null && controller.Truck);
            }
        }

#if DEBUG
        [ConsoleCommand]
        private static void GetPedsInBucket()
        {
            if (controller != null && controller.Truck)
            {
                foreach(var ped in controller.GetPedsInBucket())
                {
                    Game.LogTrivial($"Ped {ped.Model.Name} ({ped.Handle.Value.ToString("x8")}) in bucket");
                }
            }
        }

        [ConsoleCommand]
        private static void AttachPedsInBucket(bool attached)
        {
            controller?.SetBucketPedsAttached(attached);
        }

        [ConsoleCommand]
        private static void ClimbIntoBucket()
        {
            if(controller != null)
            {
                bool success = controller.ClimbIntoBucket(Game.LocalPlayer.Character);
                Game.DisplayNotification(success ? "~g~climbed into bucket" : "~r~failed to climb into bucket");
            }
            
        }
#endif
    }
}
