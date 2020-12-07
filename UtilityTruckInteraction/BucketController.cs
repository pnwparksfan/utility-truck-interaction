using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTruckInteraction
{
    using Rage;
    using Rage.Native;

    internal class BucketController
    {
        private static AnimationDictionary boomDict = "va_utillitruck";
        private static string boomAnim = "crane";
        private static AnimationDictionary rotDict = "v_boomtruck";
        private static string rotAnim = "rotate_crane_base";

        private static Dictionary<Model, bool> eligibleModels = new Dictionary<Model, bool>();
        private static Dictionary<Vehicle, BucketController> controllerCache = new Dictionary<Vehicle, BucketController>();

        public Vehicle Truck { get; }
        private float origTopSpeed;
        private bool origEngineOn;
        public bool Stopped { get; private set; } = true;
        public bool PlayerControlled { get; internal set; } = false;
        public float BoomTime { get; private set; } = -1f;
        public float RotationTime { get; private set; } = -1f;
        private HashSet<Entity> attachments = new HashSet<Entity>();
        private HashSet<Ped> attachedPeds = new HashSet<Ped>();
        public Entity[] AttachedProps => attachments.ToArray();
        private int? soundID = null;

        public static bool IsEligibleVehicle(Vehicle veh)
        {
            if (!veh) return false;

            if (eligibleModels.TryGetValue(veh.Model, out bool eligible)) return eligible;

            eligible = veh.HasBones("arm_1", "arm_2", "bucket");
            eligibleModels[veh.Model] = eligible;
            return eligible;
        }

        public static BucketController GetBucketController(Vehicle truck)
        {
            if (truck && IsEligibleVehicle(truck))
            {
                if (controllerCache.TryGetValue(truck, out var controller))
                {
                    return controller;
                }
                return new BucketController(truck);
            }

            return null;
        }

        private BucketController(Vehicle truck)
        {
            this.Truck = truck;
            origTopSpeed = truck.TopSpeed;
            origEngineOn = truck.IsEngineOn;
            controllerCache.Add(truck, this);
            if(Settings.Bucket.EnableCollisionProps)
            {
                enableAttachments();
            }
            
            Update();
        }

        public string HelpInfo
        {
            get
            {
                if (!Truck) return "";
                
                string info = "~b~Utility Truck Interaction~w~\n\n";
                info += Settings.Controls.RaiseBucket.GetControlHelpText();
                info += " Raise bucket\n";
                info += Settings.Controls.LowerBucket.GetControlHelpText();
                info += " Lower bucket\n";
                info += Settings.Controls.RotateBoomLeft.GetControlHelpText();
                info += " Rotate left\n";
                info += Settings.Controls.RotateBoomRight.GetControlHelpText();
                info += " Rotate right\n";
                info += Settings.Controls.ResetBucket.GetControlHelpText();
                info += " Reset\n";
                info += Settings.Controls.EnterExitInteractionMode.GetControlHelpText();
                info += " Exit";
                if (Truck.HasBones("leg_rr", "leg_rl", "leg_fr", "leg_fl")) {
                    info += "\n~INPUT_VEH_ROOF~ Deploy legs (hold in driver's seat)";
                }
                if (IsPlayerNearBucket)
                {
                    info += "\n";
                    info += Settings.Controls.ClimbIntoBucket.GetControlHelpText();
                    info += " Enter bucket";
                }
                return info;
            }
        }


        public bool AttachmentsEnabled
        {
            get => attachments.Count > 0;

            set
            {
                if (value) enableAttachments();
                else disableAttachments();
            }
        }

        private void enableAttachments()
        {
            if (!Truck) return;

            disableAttachments();

            (string model, string bone, Vector3 offset, Rotator rotation)[] objs = new (string, string, Vector3, Rotator)[]
            {
                ("prop_crate_06a", "bucket", new Vector3(0, -0.36f, -0.86f), new Rotator(0, 0, 90)),
                ("prop_skate_rail", "arm_1", new Vector3(0, 3, 0.06f), Rotator.Zero),
                ("prop_skate_rail", "arm_2", new Vector3(0, -3.4f, 0.1f), Rotator.Zero),
            };

            foreach (var item in objs)
            {
                if (Truck && Truck.HasBone(item.bone) && new Model(item.model).IsValid)
                {
                    Rage.Object o = new Rage.Object(item.model, Truck.GetOffsetPositionUp(-20f));
                    if (!o) continue;
                    o.MakePersistent();
                    o.AttachWithCollision(Truck, item.bone, item.offset, item.rotation, true);
                    o.IsVisible = false;
                    attachments.Add(o);
                }
            }
        }

        private void disableAttachments()
        {
            foreach (var item in attachments)
            {
                if (item) item.Delete();
            }
            attachments.Clear();
        }

        private void SetMode(Func<bool> mode, Func<float> time, AnimationDictionary dict, string anim, bool continuous, bool force, bool attachBucketPeds = true)
        {
            if (Truck)
            {
                Update();
                if (!mode() || force)
                {
                    if (force) Truck.StopEntityAnim(dict, anim);
                    Truck.PlayEntityAnimation(dict, anim, continuous, !continuous);
                    GameFiber.Sleep(5);
                    Utils.YieldWhileLimit(() => !mode(), 2000);
                }

                if (Truck && time() > 0)
                {
                    Truck.SetAnimTime(dict, anim, time());
                }
                Stopped = false;
                if (Settings.Misc.AutoAttachOnMove) SetBucketPedsAttached(true);
                PlaySound();
            }

        }

        private void StartRotMode(bool continuous = true) => SetMode(() => InRotationMode, () => RotationTime, rotDict, rotAnim, continuous, !continuous);
        private void StartBoomMode() => SetMode(() => InBoomMode, () => BoomTime, boomDict, boomAnim, false, false);

        public void Raise()
        {
            StartBoomMode();

            if (Truck)
            {
                Truck.SetAnimSpeed(boomDict, boomAnim, Settings.Bucket.RaiseSpeed);
            }
        }

        public void Lower()
        {
            StartBoomMode();

            if (Truck)
            {
                Truck.SetAnimSpeed(boomDict, boomAnim, -1 * Settings.Bucket.LowerSpeed);
            }
        }

        public void RotateRight() => RotateRight(true);
        public void RotateRight(bool continuous)
        {
            StartRotMode(continuous);

            if (Truck)
            {
                Truck.SetAnimSpeed(rotDict, rotAnim, -Settings.Bucket.RotateSpeed);
            }
        }

        public void RotateLeft() => RotateLeft(true);
        public void RotateLeft(bool continuous)
        {
            StartRotMode(continuous);

            if (Truck)
            {
                Truck.SetAnimSpeed(rotDict, rotAnim, Settings.Bucket.RotateSpeed);
            }
        }

        public void Stop()
        {
            if (Truck)
            {
                if (InBoomMode) Truck.SetAnimSpeed(boomDict, boomAnim, 0f);
                if (InRotationMode) Truck.SetAnimSpeed(rotDict, rotAnim, 0f);
                Stopped = true;
                SetBucketPedsAttached(false);
                StopSound();
                Update();
            }
        }


        private void Reset(bool instant)
        {
            if (!instant)
            {
                Reset();
            } else
            {
                Stop();
                Update();

                Truck.SetAnimSpeed(rotDict, rotAnim, 0);
                Truck.SetAnimTime(rotDict, rotAnim, 0);
                Truck.SetAnimSpeed(boomDict, boomAnim, 0);
                Truck.SetAnimTime(boomDict, boomAnim, 0);

                Update();
            }
        }

        public void Reset()
        {
            Stop();
            Update();

            if (RotationTime > 0)
            {
                if (Truck && RotationTime > 0.5f) RotateLeft(false);
                else if (Truck) RotateRight(false);

                Utils.YieldWhileLimit(() => Truck && !(Truck.HasFinishedAnim(rotDict, rotAnim) || Truck.GetAnimTime(rotDict, rotAnim) == 0f), 10000);
                StopSound();
                GameFiber.Sleep(500);
            }

            Lower();
            Utils.YieldWhileLimit(() => Truck && !(Truck.HasFinishedAnim(boomDict, boomAnim) || Truck.GetAnimTime(boomDict, boomAnim) == 0f), 10000);
            Stop();

        }

        private void Update()
        {
            if (!Truck) return;

            if (InBoomMode)
            {
                BoomTime = Truck.GetAnimTime(boomDict, boomAnim);
            }

            if (InRotationMode)
            {
                RotationTime = Truck.GetAnimTime(rotDict, rotAnim);
            }

            if(Settings.Misc.EnableEngine)
            {
                if (BoomTime != 0 || RotationTime != 0)
                {
                    Truck.IsEngineOn = true;
                }
                else
                {
                    Truck.IsEngineOn = Truck.HasDriver || origEngineOn;
                }
            }
            
            if (Settings.Misc.LimitSpeed > 0)
            {
                if (BoomTime > 0.2f || (RotationTime > 0.1f && RotationTime < 0.9f))
                {
                    Truck.TopSpeed = 5f;
                }
                else
                {
                    Truck.TopSpeed = origTopSpeed;
                }
            }
            

            if (AttachmentsEnabled)
            {
                foreach (var item in attachments)
                {
                    if (!item)
                    {
                        Game.LogTrivial($"Attachment disappeared, respawning");
                        enableAttachments();
                        break;
                    }
                }
            }
        }

        internal void SetBucketPedsAttached(bool attached)
        {
            if (!Truck) return;

            if (!attached)
            {
                foreach (Ped ped in attachedPeds)
                {
                    if (ped && ped.IsAttachedTo(Truck))
                    {
                        if (ped.IsPlayingAnim(ped.IsMale ? "move_m@generic" : "move_f@generic", "idle"))
                        {
                            ped.Tasks.ClearSecondary();
                        }
                        ped.DetachWithCollision();
                        Game.LogTrivialDebug($"Detached ped {ped.Model.Name} ({ped.Handle.Value.ToString("x8")})");
                    }
                }
                attachedPeds.Clear();
            } else
            {
                Quaternion bucketQ = Truck.GetBoneOrientation("bucket");
                Vector3 bucketPos = Truck.GetBonePosition("bucket");
                int bucketBone = Truck.GetBoneIndex("bucket");

                foreach (Ped ped in GetPedsInBucket())
                {
                    if (ped && !attachedPeds.Contains(ped) && !ped.IsAttachedToAnyEntity())
                    {
                        if (!ped.IsPlayingAnyScriptedAnim())
                        {
                            ped.Tasks.PlayAnimation(ped.IsMale ? "move_m@generic" : "move_f@generic", "idle", 10f, AnimationFlags.Loop | AnimationFlags.SecondaryTask);
                        }

                        attachedPeds.Add(ped);
                        Rotator rotOffset = (ped.Orientation * Quaternion.Invert(bucketQ)).ToRotation();
                        Vector3 worldOffset = ped.Position - bucketPos;
                        Vector4 rotatedPosition = Vector3.Transform(worldOffset, Quaternion.Invert(bucketQ));
                        Vector3 posOffset = new Vector3(rotatedPosition.X, rotatedPosition.Y, rotatedPosition.Z);

                        ped.AttachWithCollision(Truck, bucketBone, posOffset, rotOffset, true);

                        // Game.LogTrivial($"Original offset: {worldOffset}");
                        // Game.LogTrivial($"Rotated offset: {rotatedPosition}");
                        Game.LogTrivialDebug($"Attached ped {ped.Model.Name} ({ped.Handle.Value.ToString("x8")}) to bucket with offset {posOffset} and rotation {rotOffset}");
                    }
                }
            }
        }

        public bool InBoomMode => Truck && (Truck.IsPlayingAnim(boomDict, boomAnim) || Truck.HasFinishedAnim(boomDict, boomAnim));
        public bool InRotationMode => Truck && (Truck.IsPlayingAnim(rotDict, rotAnim) || Truck.HasFinishedAnim(rotDict, rotAnim));
        public bool IsBoomMoving => Truck && !Stopped && Truck.IsPlayingAnim(boomDict, boomAnim) && !Truck.HasFinishedAnim(boomDict, boomAnim);
        public bool IsRotating => Truck && !Stopped && Truck.IsPlayingAnim(rotDict, rotAnim) && !Truck.HasFinishedAnim(rotDict, rotAnim);


        public Vector3 GetBucketPosition
        {
            get
            {
                if (!Truck) return default;

                Vector3 bucketBonePos = Truck.GetBonePosition("bucket");
                Quaternion bucketQ = Truck.GetBoneOrientation("bucket");
                return MathHelper.GetOffsetPosition(bucketBonePos, bucketQ, Settings.Bucket.BucketOffset);
            }
        }

        public bool IsEntityInBucket(Entity entity)
        {
            return Truck && entity &&
                Utils.IsPositionInBox(GetBucketPosition, Truck.GetBoneOrientation("bucket"), Settings.Bucket.BucketSize, entity.Position);
        }

        public Ped[] GetPedsInBucket()
        {
            List<Ped> peds = new List<Ped>();
            foreach (Ped ped in World.GetAllPeds())
            {
                if (IsEntityInBucket(ped)) peds.Add(ped);
            }
            return peds.ToArray();
        }

        public bool CanClimbIntoBucket(Ped ped)
        {
            return ped && Truck && ped.DistanceTo(Truck.RearPosition) < 20 &&
                BoomTime < 0.05f && (RotationTime > 0.95f || RotationTime < 0.05f) &&
                !IsEntityInBucket(ped);
        }

        public bool IsPlayerNearBucket
        { 
            get 
            {
                Ped p = Game.LocalPlayer.Character;
                if (!CanClimbIntoBucket(p)) return false;

                Vector3 pos = Game.LocalPlayer.Character.Position;
                Vector3 offset = Truck.GetPositionOffset(pos) - Truck.GetPositionOffset(Truck.RearPosition);
                
                return 
                    !p.IsStandingOnVehicle(Truck) &&
                    Math.Abs(offset.X) < Settings.Bucket.BucketSize.X * 0.6f && 
                    -offset.Y > 0 &&
                    -offset.Y < Settings.Bucket.ClimbZoneDist && 
                    pos.Z > Truck.BelowPosition.Z && 
                    pos.Z < GetBucketPosition.Z;
            }
        }

        public bool ClimbIntoBucket(Ped ped)
        {
            if (!CanClimbIntoBucket(ped)) return false;

            Truck.MakePersistent();
            float startZ = (World.GetGroundZ(Truck.RearPosition, false, true) ?? Truck.BelowPosition.Z) + (ped.Position.Z - ped.BelowPosition.Z);
            Vector3 start = new Vector3(Truck.RearPosition.X, Truck.RearPosition.Y, startZ);
            start += Truck.ForwardVector * -0.5f;
            // var f = GameFiber.ExecuteNewWhile(() => { Debug.DrawSphere(start, 0.5f, System.Drawing.Color.Blue); }, () => ped);
            ped.Tasks.GoStraightToPosition(start, 1f, Truck.Heading, 1.5f, 15000).WaitForCompletion();

            ped.CollisionIgnoredEntity = Truck;
            // Vector3 start = ped.Position;
            Rage.Task t = ped.Tasks.PlayAnimation("move_climb", "standclimbup_220_high", 5f, AnimationFlags.None);
            while(ped && Truck && t.IsActive)
            {
                ped.SetPositionNoOffset(Vector3.Lerp(start, GetBucketPosition, ped.GetAnimTime("move_climb", "standclimbup_220_high")));
                NativeFunction.Natives.SET_PED_CAN_LEG_IK(ped, false);
                NativeFunction.Natives.SET_PED_CAN_ARM_IK(ped, false);
                NativeFunction.Natives.SET_PED_CAN_TORSO_IK(ped, false);
                GameFiber.Yield();
            }

            if(ped && Truck)
            {
                ped.Position = GetBucketPosition;
                ped.Heading = Truck.Heading;
                ped.CollisionIgnoredEntity = null;
            }

            return IsEntityInBucket(ped);
            // if (f.IsAlive) f.Abort();
        }

        private void PlaySound()
        {
            if(Truck)
            {
                if (!soundID.HasValue)
                {
                    NativeFunction.Natives.REQUEST_AMBIENT_AUDIO_BANK("Crane", 0);
                    NativeFunction.Natives.REQUEST_SCRIPT_AUDIO_BANK("Container_Lifter", 0);
                    if (!NativeFunction.Natives.IS_AUDIO_SCENE_ACTIVE<bool>("DOCKS_HEIST_USING_CRANE"))
                    {
                        NativeFunction.Natives.Start_Audio_Scene("DOCKS_HEIST_USING_CRANE");
                    }
                    soundID = NativeFunction.Natives.GET_SOUND_ID<int>();
                    Game.LogTrivialDebug($"Assigned sound ID {soundID}");
                }

                if(NativeFunction.Natives.HAS_SOUND_FINISHED<bool>(soundID.Value))
                {
                    NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(soundID.Value, "Move_U_D", Truck, "CRANE_SOUNDS", 0, 0);
                }
                
            }
        }

        private void StopSound()
        {
            if(soundID.HasValue)
            {
                NativeFunction.Natives.STOP_SOUND(soundID.Value);
                NativeFunction.Natives.RELEASE_SOUND_ID(soundID.Value);
                Game.LogTrivialDebug($"Released sound ID {soundID}");
                soundID = null;
            }
        }

        private static StaticFinalizer finalizer = new StaticFinalizer(() => 
        {
            Game.HideHelp();
            Utils.ClearLoadingSpinner();

            foreach (var controller in controllerCache.Values)
            {
                controller.Stop();
                controller.StopSound();
                controller.disableAttachments();
                
                if(Settings.Misc.AutoReset)
                {
                    controller.Reset(true);
                    if (controller.Truck)
                    {
                        if (Settings.Misc.LimitSpeed > 0) controller.Truck.TopSpeed = controller.origTopSpeed;

                        if ((controller.InBoomMode && controller.RotationTime > 0) ||
                        (controller.InRotationMode && controller.BoomTime > 0))
                        {
                            controller.Truck.Repair();
                        }
                    }
                }
            }
        });

        internal static void CleanupInvalidEntries()
        {
            foreach (var controller in controllerCache.Values.ToArray())
            {
                if (!controller.Truck)
                {
                    controller.disableAttachments();
                    controller.StopSound();
                    controllerCache.Remove(controller.Truck);
                }
            }
        }
    }
}
