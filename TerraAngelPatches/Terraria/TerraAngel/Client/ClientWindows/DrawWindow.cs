﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework;
using TerraAngel.Cheat;
using TerraAngel.Cheat.Cringes;
using Terraria;
using TerraAngel.Utility;
using TerraAngel.WorldEdits;
using Terraria.ID;
using TerraAngel;

namespace TerraAngel.Client.ClientWindows
{
    public class DrawWindow : ClientWindow
    {
        public override bool IsToggleable => false;
        public override bool DefaultEnabled => true;
        public override bool IsEnabled { get => true; }

        public override void Draw(ImGuiIOPtr io)
        {
            ImGui.Begin("DRAWWINDOW", ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);
            ImGui.PushClipRect(System.Numerics.Vector2.Zero, io.DisplaySize, false);
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            if (!Main.gameMenu)
            {
                if (!Main.mapFullscreen)
                {
                    ESPBoxesCringe espBoxes = CringeManager.GetCringe<ESPBoxesCringe>();
                    ESPTracersCringe espTracers = CringeManager.GetCringe<ESPTracersCringe>();
                    if (espBoxes.Enabled || espTracers.Enabled)
                    {
                        Vector2 localPlayerCenter = Util.WorldToScreen(Main.LocalPlayer.Center);
                        for (int i = 0; i < 255; i++)
                        {
                            if (Main.player[i].active)
                            {
                                Player currentPlayer = Main.player[i];
                                if (espBoxes.Enabled)
                                {
                                    Vector2 minScreenPos = Util.WorldToScreen(currentPlayer.TopLeft);
                                    Vector2 maxScreenPos = Util.WorldToScreen(currentPlayer.BottomRight);
                                    if (currentPlayer.whoAmI == Main.myPlayer)
                                    {
                                        drawList.AddRect(minScreenPos.ToNumerics(), maxScreenPos.ToNumerics(), espBoxes.LocalPlayerColor.PackedValue);
                                    }
                                    else
                                    {
                                        drawList.AddRect(minScreenPos.ToNumerics(), maxScreenPos.ToNumerics(), espBoxes.OtherPlayerColor.PackedValue);
                                    }
                                }

                                if (espTracers.Enabled)
                                {
                                    if (currentPlayer.whoAmI != Main.myPlayer)
                                    {
                                        Vector2 otherPlayerCenter = Util.WorldToScreen(currentPlayer.Center);

                                        drawList.AddLine(localPlayerCenter.ToNumerics(), otherPlayerCenter.ToNumerics(), espTracers.TracerColor.PackedValue);
                                    }
                                }
                            }
                        }
                    }

                    WorldEdit worldEdit = ClientLoader.MainRenderer.CurrentWorldEdit;
                    worldEdit?.DrawPreviewInWorld(io, drawList);
                }
                else
                {
                    WorldEdit worldEdit = ClientLoader.MainRenderer.CurrentWorldEdit;
                    worldEdit?.DrawPreviewInMap(io, drawList);

                    
                }

                {
                    WorldEdit worldEdit = ClientLoader.MainRenderer.CurrentWorldEdit;
                    if (worldEdit != null)
                    {
                        Vector2 mousePos = Util.ScreenToWorld(Input.InputSystem.MousePosition) / 16f;

                        if (Main.mapFullscreen)
                            mousePos = Util.ScreenToWorldFullscreenMap(Input.InputSystem.MousePosition) / 16f;

                        mousePos = new Vector2(MathF.Floor(mousePos.X), MathF.Floor(mousePos.Y));

                        if (worldEdit.RunEveryFrame)
                        {
                            if (Input.InputSystem.MiddleMouseDown)
                            {
                                worldEdit.Edit(mousePos);
                            }
                        }
                        else if (Input.InputSystem.MiddleMousePressed)
                        {
                            worldEdit.Edit(mousePos);
                        }
                    }
                }

                if (CringeManager.GetCringe<ShowTileSectionsCringe>().Enabled)
                {
                    if (CringeManager.LoadedTileSections != null)
                    {
                        for (int xs = 0; xs < Main.maxSectionsX; xs++)
                        {
                            for (int ys = 0; ys < Main.maxSectionsY; ys++)
                            {
                                Color col = new Color(1f, 1, 0f);
                                if (!CringeManager.LoadedTileSections[xs, ys])
                                {
                                    col = new Color(1f, 0f, 0f);
                                }

                                Vector2 worldCoords = new Vector2(xs * 200 * 16, ys * 150 * 16);
                                Vector2 worldCoords2 = new Vector2((xs + 1) * 200 * 16, (ys + 1) * 150 * 16);

                                if (Main.mapFullscreen)
                                {
                                    drawList.AddRect(Util.WorldToScreenFullscreenMap(worldCoords).ToNumerics(), Util.WorldToScreenFullscreenMap(worldCoords2).ToNumerics(), col.PackedValue);
                                }
                                else
                                {
                                    drawList.AddRect(Util.WorldToScreen(worldCoords).ToNumerics(), Util.WorldToScreen(worldCoords2).ToNumerics(), col.PackedValue);
                                }
                            }
                        }
                    }
                }
            }

            ImGui.PopClipRect();
            ImGui.End();
        }
    }
}