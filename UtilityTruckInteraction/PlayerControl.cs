using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTruckInteraction
{
    using Rage;
    using Rage.Native;
    using System.Windows.Forms;

    internal static class PlayerControl
    {
        public static BucketController ActiveBucket { get; set; }
        private static Vehicle nearVeh;
        private static uint pressStart = 0;
        private static uint lastBucketHelp = 0;
        private static bool hasShownHelpInBucket = false;

        internal static void Process()
        {
            BucketController.CleanupInvalidEntries();

            if(ActiveBucket != null)
            {
                if(ActiveBucket.Truck)
                {
                    Vehicle truck = ActiveBucket.Truck;

                    if (Game.IsControlPressed(0, Settings.Controls.RaiseBucket))
                    {
                        ActiveBucket.Raise();
                        ActiveBucket.PlayerControlled = true;
                    }
                    else if (Game.IsControlPressed(0, Settings.Controls.LowerBucket))
                    {
                        ActiveBucket.Lower();
                        ActiveBucket.PlayerControlled = true;
                    }
                    else if (Game.IsControlPressed(0, Settings.Controls.RotateBoomLeft))
                    {
                        ActiveBucket.RotateLeft();
                        ActiveBucket.PlayerControlled = true;
                    }
                    else if (Game.IsControlPressed(0, Settings.Controls.RotateBoomRight))
                    {
                        ActiveBucket.RotateRight();
                        ActiveBucket.PlayerControlled = true;
                    }
                    else if ((ActiveBucket.BoomTime > 0 || ActiveBucket.RotationTime > 0)
                      && Game.IsControlJustPressed(0, Settings.Controls.ResetBucket))
                    {
                        ActiveBucket.Reset();
                    }
                    else if (ActiveBucket.PlayerControlled && (ActiveBucket.InBoomMode || ActiveBucket.InRotationMode))
                    {
                        ActiveBucket.Stop();
                        ActiveBucket.PlayerControlled = false;
                    }
                    else if (Game.IsControlJustPressed(0, Settings.Controls.EnterExitInteractionMode))
                    {
                        string name = truck.Model.GetVehicleDisplayName();
                        string license = truck.LicensePlate;
                        ActiveBucket.Stop();
                        ActiveBucket.PlayerControlled = false;
                        ActiveBucket = null;
                        nearVeh = null;
                        Game.DisplayNotification($"Deactivated ~b~Utility Truck Interaction~w~ on {name} ({license})");
                        Game.HideHelp();
                        while (Game.IsControlPressed(0, Settings.Controls.EnterExitInteractionMode)) GameFiber.Sleep(5);
                        return;
                    }
                    else 
                    {
                        ActiveBucket.PlayerControlled = false;
                        bool playerNearBucket = ActiveBucket.IsPlayerNearBucket;
                        if (playerNearBucket)
                        {
                            Game.DisplayHelp(ActiveBucket.HelpInfo, 10);
                            Game.DisableControlAction(0, Settings.Controls.ClimbIntoBucket, true);
                        }

                        // if (playerNearBucket && Game.IsControlPressed(0, GameControl.Jump))
                        if (playerNearBucket && NativeFunction.Natives.IS_DISABLED_CONTROL_PRESSED<bool>(0, (int)Settings.Controls.ClimbIntoBucket))
                        {
                            uint n1 = Game.DisplayNotification("Climbing into bucket, please wait...");
                            if (ActiveBucket.ClimbIntoBucket(Game.LocalPlayer.Character))
                            {
                                Game.DisplayNotification("~g~Climbed into bucket");
                                Game.DisplayHelp(ActiveBucket.HelpInfo, 10000);
                                hasShownHelpInBucket = true;
                            }
                            else
                            {
                                Game.DisplayNotification("~r~Could not climb into bucket");
                            }
                            Game.RemoveNotification(n1);
                        }
                        else if (Game.GameTime - lastBucketHelp > 2000)
                        {
                            lastBucketHelp = Game.GameTime;
                            if (ActiveBucket.IsEntityInBucket(Game.LocalPlayer.Character))
                            {
                                if (!hasShownHelpInBucket)
                                {
                                    Game.DisplayHelp(ActiveBucket.HelpInfo, 10000);
                                    hasShownHelpInBucket = true;
                                }
                            }
                            else
                            {
                                hasShownHelpInBucket = false;
                            }
                        }
                    } 
                } else
                {
                    ActiveBucket = null;
                    Game.HideHelp();
                }
            } else
            {
                Ped p = Game.LocalPlayer.Character;
                foreach (Vehicle veh in p.GetNearbyVehicles(8))
                {
                    if(BucketController.IsEligibleVehicle(veh))
                    {
                        string help = $"Press and hold {Settings.Controls.EnterExitInteractionMode.GetControlHelpText()} to control the bucket on the ~b~{veh.Model.GetVehicleDisplayName()}~w~ ({veh.LicensePlate})";
                        if (veh != nearVeh)
                        {
                            Game.DisplayHelp(help, 10000);
                            nearVeh = veh;
                            pressStart = 0;
                        }
                        if(Game.IsControlPressed(0, Settings.Controls.EnterExitInteractionMode))
                        {
                            if (pressStart == 0)
                            {
                                pressStart = Game.GameTime;
                                Utils.ShowLoadingSpinner($"Starting utility truck interaction on {veh.Model.GetVehicleDisplayName()}...");
                            }
                            float pct = (Game.GameTime - pressStart) * 1.0f / 3000f;
                            if(pct >= 1)
                            {
                                ActiveBucket = BucketController.GetBucketController(veh);
                                nearVeh = null;
                                pressStart = 0;
                                Utils.ClearLoadingSpinner();
                                Game.DisplayHelp(ActiveBucket.HelpInfo, 30000);
                            } else
                            {
                                Game.DisplayHelp(help, 1000);
                            }
                        } else if(pressStart > 0)
                        {
                            pressStart = 0;
                            Utils.ClearLoadingSpinner();
                        }
                        return;
                    }
                    nearVeh = null;
                }
                GameFiber.Sleep(1000);
            }
        }
    }
}
