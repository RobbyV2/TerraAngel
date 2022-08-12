﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TerraAngel.Hooks;
using TerraAngel.Input;
using TerraAngel.Utility;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace TerraAngel.Cheat.Cringes
{
    public class AutoFishCringe : Cringe
    {
        public override string Name => "Auto-Fish";

        public override CringeTabs Tab => CringeTabs.AutomationCringes;

        public ref bool AcceptItems => ref ClientLoader.Config.AutoFishAcceptItems;
        public ref bool AcceptAllItems => ref ClientLoader.Config.AutoFishAcceptAllItems;
        public ref bool AcceptQuestFish => ref ClientLoader.Config.AutoFishAcceptQuestFish;
        public ref bool AcceptCrates => ref ClientLoader.Config.AutoFishAcceptCrates;
        public ref bool AcceptNormal => ref ClientLoader.Config.AutoFishAcceptNormal;
        public ref bool AcceptCommon => ref ClientLoader.Config.AutoFishAcceptCommon;
        public ref bool AcceptUncommon => ref ClientLoader.Config.AutoFishAcceptUncommon;
        public ref bool AcceptRare => ref ClientLoader.Config.AutoFishAcceptRare;
        public ref bool AcceptVeryRare => ref ClientLoader.Config.AutoFishAcceptVeryRare;
        public ref bool AcceptLegendary => ref ClientLoader.Config.AutoFishAcceptLegendary;
        public ref bool AcceptNPCs => ref ClientLoader.Config.AutoFishAcceptNPCs;
        private ref int frameCountRandomizationMin => ref ClientLoader.Config.AutoFishFrameCountRandomizationMin;
        private ref int frameCountRandomizationMax => ref ClientLoader.Config.AutoFishFrameCountRandomizationMax;

        private int frameCountBeforeActualPullFish = 0;
        private int frameCountBeforeActualCast = 0;
        private bool wantPullFish = false;
        private bool wantToReCast = false;

        private bool hasSpecialPosition = false;
        private Vector2 specialPosition = Vector2.Zero;

        public override void DrawUI(ImGuiIOPtr io)
        {
            base.DrawUI(io);

            if (ImGui.CollapsingHeader("Auto-Fish settings"))
            {
                ImGui.Indent(20f);
                ImGui.Checkbox("Accept Items", ref AcceptItems);
                ImGui.Checkbox("Accept All Items", ref AcceptAllItems);
                if (AcceptItems && !AcceptAllItems)
                {
                    ImGui.Checkbox("Accept Quest Fish", ref AcceptQuestFish);
                    ImGui.Checkbox("Accept Crates", ref AcceptCrates);
                    ImGui.Checkbox("Accept Normal", ref AcceptNormal);
                    ImGui.Checkbox("Accept Common", ref AcceptCommon);
                    ImGui.Checkbox("Accept Uncommon", ref AcceptUncommon);
                    ImGui.Checkbox("Accept Rare", ref AcceptRare);
                    ImGui.Checkbox("Accept Very Rare", ref AcceptVeryRare);
                    ImGui.Checkbox("Accept Legendary", ref AcceptLegendary);
                }
                ImGui.Checkbox("Accept NPCs", ref AcceptNPCs);

                ImGui.SliderInt("Randomization Min", ref frameCountRandomizationMin, 0, 120);
                ImGui.SliderInt("Randomization Max", ref frameCountRandomizationMax, frameCountRandomizationMin, frameCountRandomizationMin + 120);
                ImGui.Checkbox("Use specific mouse position", ref hasSpecialPosition); ImGui.SameLine(); ImGui.TextDisabled("*Press Ctrl+Alt to select specific cast position");
                ImGui.Unindent(20f);
            }
        }

        public override void Update()
        {
            base.Update();
            if (frameCountRandomizationMax < frameCountRandomizationMin)
                frameCountRandomizationMax = frameCountRandomizationMin;
            if (Enabled)
            {
                if (hasSpecialPosition)
                {
                    ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
                    drawList.AddCircleFilled(Util.WorldToScreen(specialPosition).ToNumerics(), 10f, Color.Red.PackedValue);

                    if (InputSystem.KeyCtrl && InputSystem.KeyAlt)
                    {
                        specialPosition = Main.MouseWorld;
                    }
                }

                if (wantPullFish)
                {
                    frameCountBeforeActualPullFish--;

                    if (frameCountBeforeActualPullFish <= 0)
                    {
                        try
                        {
                            bool canUse = (bool)HookUtil.GetMethod(typeof(Player), "ItemCheck_CheckFishingBobbers").Invoke(Main.LocalPlayer, new object[] { true });
                            wantPullFish = false;
                            wantToReCast = true;
                            frameCountBeforeActualCast = Main.rand.Next(frameCountRandomizationMin, frameCountRandomizationMax) + 50;
                        }
                        catch (Exception ex)
                        {

                        }

                        ClientLoader.Console.WriteLine("Pulled fish");
                    }
                }
                if (wantToReCast)
                {
                    if (Main.projectile.Any(x => (x.bobber && x.owner == Main.myPlayer && x.active)))
                        return;

                    frameCountBeforeActualCast--;

                    if (frameCountBeforeActualCast <= 0)
                    {
                        Item heldItem = Main.LocalPlayer.HeldItem;

                        if (heldItem.fishingPole > 0)
                        {
                            Main.LocalPlayer.Fishing_GetBait(out int baitPower, out int baitType);

                            if (baitPower > 0 && baitType > 0)
                            {
                                Main.LocalPlayer.controlUseItem = true;
                                int mx = Main.mouseX;
                                int my = Main.mouseY;
                                if (hasSpecialPosition)
                                {
                                    Main.mouseX = (int)specialPosition.X - (int)Main.screenPosition.X;
                                    Main.mouseY = (int)specialPosition.Y - (int)Main.screenPosition.Y;
                                }
                                Main.LocalPlayer.ItemCheck(Main.LocalPlayer.HeldItem.useStyle);
                                NetMessage.SendData(MessageID.PlayerControls, number: Main.myPlayer);

                                Main.mouseX = mx;
                                Main.mouseY = my;

                            }
                        }
                        wantToReCast = false;
                    }

                }

            }
        }

        public void FishingCheck(Projectile bobber, FishingAttempt fish)
        {
            if (bobber.owner == Main.myPlayer && Enabled)
            {
                bool wantToCatch = false;
                if (fish.rolledItemDrop > 0)
                {
                    ClientLoader.Console.WriteLine($"Fish: {Utility.Util.ItemFields[fish.rolledItemDrop].Name}");
                    if (AcceptItems)
                    {
                        if (!fish.crate && fish.questFish == -1 && !fish.common && !fish.uncommon && !fish.rare && !fish.veryrare && !fish.legendary && AcceptNormal)
                        {
                            wantToCatch = true;
                        }

                        if (AcceptQuestFish && fish.questFish != -1)
                        {
                            wantToCatch = true;
                        }

                        if (AcceptCrates && fish.crate)
                        {
                            wantToCatch = true;
                        }

                        if (AcceptCommon && fish.common)
                        {
                            wantToCatch = true;
                        }

                        if (AcceptUncommon && fish.uncommon)
                        {
                            wantToCatch = true;
                        }

                        if (AcceptRare && fish.rare)
                        {
                            wantToCatch = true;
                        }

                        if (AcceptVeryRare && fish.veryrare)
                        {
                            wantToCatch = true;
                        }

                        if (AcceptLegendary && fish.legendary)
                        {
                            wantToCatch = true;
                        }

                    }
                }

                if (fish.rolledEnemySpawn > 0)
                {
                    ClientLoader.Console.WriteLine($"NPC: {Utility.Util.NPCFields[fish.rolledEnemySpawn].Name}");
                    if (AcceptNPCs)
                    {
                        wantToCatch = true;
                    }
                }

                if (wantToCatch)
                {
                    wantPullFish = true;
                    frameCountBeforeActualPullFish = Main.rand.Next(frameCountRandomizationMin, frameCountRandomizationMax);
                }
            }
        }
    }
}
