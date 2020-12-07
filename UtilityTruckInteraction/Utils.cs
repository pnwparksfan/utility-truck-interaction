using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTruckInteraction
{
    using Rage;
    using Rage.Native;

    internal static class Utils
    {
        public static void PlayEntityAnimation(this Entity entity, AnimationDictionary dict, string anim, bool loop = false, bool stayInEndFrame = true, float blend = 8f, bool onlyIfNotPlaying = true)
        {
            dict.LoadAndWait();
            if(entity && (!onlyIfNotPlaying || !IsPlayingAnim(entity, dict, anim)))
            {
                NativeFunction.Natives.PLAY_ENTITY_ANIM(entity, anim, dict.Name, blend, loop, stayInEndFrame, 0, 0f, 0);
            }
        }

        public static void StopEntityAnim(this Entity entity, AnimationDictionary dict, string anim)
        {
            NativeFunction.Natives.STOP_ENTITY_ANIM(entity, anim, dict.Name, 0f);
        }

        public static bool IsPlayingAnim(this Entity entity, AnimationDictionary dict, string anim)
        {
            return NativeFunction.Natives.IS_ENTITY_PLAYING_ANIM<bool>(entity, dict.Name, anim, 3);
        }

        public static bool HasFinishedAnim(this Entity entity, AnimationDictionary dict, string anim)
        {
            return NativeFunction.Natives.HAS_ENTITY_ANIM_FINISHED<bool>(entity, dict.Name, anim, 3);
        }

        public static void SetAnimSpeed(this Entity entity, AnimationDictionary dict, string anim, float speed)
        {
            NativeFunction.Natives.SET_ENTITY_ANIM_SPEED(entity, dict.Name, anim, speed);
        }

        public static void SetAnimTime(this Entity entity, AnimationDictionary dict, string anim, float time)
        {
            NativeFunction.Natives.SET_ENTITY_ANIM_CURRENT_TIME(entity, dict.Name, anim, time);
        }

        public static float GetAnimTime(this Entity entity, AnimationDictionary dict, string anim)
        {
            return NativeFunction.Natives.GET_ENTITY_ANIM_CURRENT_TIME<float>(entity, dict.Name, anim);
        }

        public static float GetAnimDuration(AnimationDictionary dict, string anim)
        {
            return NativeFunction.Natives.GET_ANIM_DURATION<float>(dict.Name, anim);
        }

        public static bool IsPlayingAnyScriptedAnim(this Ped ped)
        {
            return NativeFunction.Natives.GET_IS_TASK_ACTIVE<bool>(ped, 134); // CTaskScriptedAnimation
        }

        public static bool HasBones(this Entity entity, params string[] bones)
        {
            foreach (var bone in bones)
            {
                if (!entity.HasBone(bone)) return false;
            }
            return true;
        }

        public static void AttachWithCollision(this Entity object1, Entity object2, int boneIndex, Vector3 position, Rotator rotation, bool collision)
        {
            NativeFunction.Natives.ATTACH_ENTITY_TO_ENTITY(object1, object2, boneIndex, position.X, position.Y, position.Z, rotation.Pitch, rotation.Roll, rotation.Yaw, true, true, collision, true, 2, true);
        }

        public static void AttachWithCollision(this Entity object1, Entity object2, string boneName, Vector3 position, Rotator rotation, bool collision)
        {
            object1.AttachWithCollision(object2, object2.GetBoneIndex(boneName), position, rotation, collision);
        }

        public static bool IsAttachedTo(this Entity from, Entity to)
        {
            return NativeFunction.Natives.IS_ENTITY_ATTACHED_TO_ENTITY<bool>(from, to);
        }

        public static bool IsAttachedToAnyObject(this Entity entity) => NativeFunction.Natives.IS_ENTITY_ATTACHED_TO_ANY_OBJECT<bool>(entity);
        public static bool IsAttachedToAnyVehicle(this Entity entity) => NativeFunction.Natives.IS_ENTITY_ATTACHED_TO_ANY_VEHICLE<bool>(entity);
        public static bool IsAttachedToAnyPed(this Entity entity) => NativeFunction.Natives.IS_ENTITY_ATTACHED_TO_ANY_PED<bool>(entity);
        public static bool IsAttachedToAnyEntity(this Entity entity) => entity.IsAttachedToAnyObject() || entity.IsAttachedToAnyVehicle() || entity.IsAttachedToAnyPed();


        public static void DetachWithCollision(this Entity object1)
        {
            NativeFunction.Natives.DETACH_ENTITY(object1, true, false);
        }

        public static void SetPositionNoOffset(this Entity entity, Vector3 position)
        {
            NativeFunction.Natives.SET_ENTITY_COORDS_NO_OFFSET(entity, position, 1, 1, 1, 0);
        }

        public static bool YieldWhileLimit(Func<bool> predicate, int limit)
        {
            uint start = Game.GameTime;
            while(predicate())
            {
                if (Game.GameTime - start > limit) return false;
                GameFiber.Yield();
            }
            return true;
        }

        public static void ShowLoadingSpinner(string text, int type = 0)
        {
            NativeFunction.Natives.xABA17D7CE615ADBF("STRING"); // BEGIN_TEXT_COMMAND_BUSYSPINNER_ON or _SET_LOADING_PROMPT_TEXT_ENTRY
            NativeFunction.Natives.x6C188BE134E074AA(text); // ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME
            NativeFunction.Natives.xBD12F8228410D9B4(type); // END_TEXT_COMMAND_BUSYSPINNER_ON or _SHOW_LOADING_PROMPT
        }

        public static void ClearLoadingSpinner()
        {
            NativeFunction.Natives.x10D373323E5B9C0D(); // BUSYSPINNER_OFF or _REMOVE_LOADING_PROMPT
        }

        public static string GetLabelText(string labelName)
        {
            return NativeFunction.Natives.x7B5280EBA9840C72<string>(labelName);
        }

        public static string GetVehicleMake(this Model model)
        {
            // _GET_MAKE_NAME_FROM_VEHICLE_MODEL
            string label = NativeFunction.Natives.xF7AF4F159FF99F97<string>(model);
            return GetLabelText(label);
        }

        public static string GetVehicleDisplayName(this Model model)
        {
            string label = NativeFunction.Natives.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL<string>(model);
            return GetLabelText(label);
        }

        /*
        public static string GetControlHelpText(int controlGroup, GameControl control)
        {
            string code = NativeFunction.Natives.GET_CONTROL_INSTRUCTIONAL_BUTTON<string>(controlGroup, (int)control, 1);
            return $"~{code}~";
        }
        */

        public static bool IsPositionInBox(Vector3 boxCenter, Quaternion boxOrientation, Vector3 boxSize, Vector3 position)
        {
            Vector3 V = position - boxCenter;
            boxOrientation.GetAxes(out Vector3 X_loc, out Vector3 Y_loc, out Vector3 Z_loc);

            float px = V.ProjectOnTo(X_loc).Length();
            float py = V.ProjectOnTo(Y_loc).Length();
            float pz = V.ProjectOnTo(Z_loc).Length();

            return (
                px * 2 < boxSize.X &&
                py * 2 < boxSize.Y &&
                pz * 2 < boxSize.Z
            );
        }
    }
}
