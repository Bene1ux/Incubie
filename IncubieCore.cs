using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using SharpDX;

namespace Incubie
{
    class IncubieCore : BaseSettingsPlugin<IncubSettings>
    {
        private int gemInventoryId;
        private int gemId;
        private bool foundGem;
        private bool foundIncub;
        private int maxIncubAvailableKills;
        private uint maxExperienceToNextLevel;
        private uint startingExperienceToNextLevel;
        private ushort startingIncubatorKills;
        private int incubatorInventoryId;
        private uint gemExperienceGained;
        private ushort killed;
        private float gemExpPerMonster;

        private Dictionary<string, ushort> incubatorMaxCapacities = new Dictionary<string, ushort>
        {
            {"Mysterious Incubator", 9030},
            {"Skittering Incubator", 9030},
            {"Fossilised Incubator", 9030},
            {"Fragmented Incubator", 9030},
            {"Abyssal Incubator", 9030},
            {"Geomancer's Incubator", 30030},
            {"Thaumaturge's Incubator", 15030},
            {"Time-Lost Incubator", 45030},
            {"Foreboding Incubator", 10530},
            {"Maddening Incubator", 12930},
            {"Obscured Incubator", 10530},
            {"Celestial Armoursmith's Incubator", 15030},
            {"Celestial Blacksmith's Incubator", 15030},
            {"Celestial Jeweller's Incubator", 21030}
        };

        public override void OnLoad()
        {
            Settings.Refresh.OnPressed = () =>
            {
                Refresh();
                DebugWindow.LogMsg("Refreshed gem and incubator info");
            };
            base.OnLoad();
        }

        public override void Render()
        {
            // if (GameController.IngameState.IngameUi.InventoryPanel.IsVisible)
            // {
            //     foreach (var inventory in GameController.IngameState.Data.ServerData.PlayerInventories)
            //     {
            //         var name = inventory.Inventory.Items.ElementAtOrDefault(0)?.GetComponent<Mods>()?.IncubatorName;
            //         if (name == null)
            //         {
            //             continue;
            //         }
            //
            //         var rect = inventory.Inventory.InventorySlotItems.ElementAtOrDefault(0)?.GetClientRect();
            //         if (rect != null)
            //         {
            //             Graphics.DrawBox(rect.Value, Color.Lime);
            //         }
            //     }
            // }
            gemExperienceGained = 0;
            if (foundGem)
            {
                var experienceToNextLevel = GameController.IngameState.Data.ServerData
                    .PlayerInventories[gemInventoryId].Inventory.Items[0]
                    .GetComponent<Sockets>().SocketedGems[gemId].GemEntity.GetComponent<SkillGem>()
                    .ExperienceToNextLevel;
                gemExperienceGained = startingExperienceToNextLevel - experienceToNextLevel;
                var gemMilExpGained = (float)gemExperienceGained / 1000000;
                var maxGemMilExp = (float) maxExperienceToNextLevel / 1000000;
                Graphics.DrawText($"{gemMilExpGained.ToString("")} / {maxGemMilExp.ToString("")}", new System.Numerics.Vector2(Settings.X, Settings.Y));
            }

            if (foundIncub)
            {
                var mods = GameController.IngameState.Data.ServerData
                    .PlayerInventories[incubatorInventoryId].Inventory
                    .Items[0].GetComponent<Mods>();
                if (mods.IncubatorName == null)
                {
                    foundIncub = false;
                    return;
                }

                var incubatorKills = (ushort)mods.IncubatorKills;
                killed = (ushort)(incubatorKills - startingIncubatorKills);
                Graphics.DrawText($"{killed} / {maxIncubAvailableKills}", new System.Numerics.Vector2(Settings.X, Settings.Y + 30));
                if (killed != 0 && Settings.ShowExpPerMonster)
                {
                    gemExpPerMonster = (float)gemExperienceGained / killed;
                    Graphics.DrawText(gemExpPerMonster.ToString(""), new System.Numerics.Vector2(Settings.X, Settings.Y+60));
                }
            }
        }

        private bool FindGem()
        {
            maxExperienceToNextLevel = 0;
            for (var i = 0; i < GameController.IngameState.Data.ServerData.PlayerInventories.Count; i++)
            {
                var inventory = GameController.IngameState.Data.ServerData.PlayerInventories[i];
                var item = inventory.Inventory.Items.ElementAtOrDefault(0);

                var gems = item?.GetComponent<Sockets>()?.SocketedGems;
                if (gems == null)
                {
                    continue;
                }

                for (var j = 0; j < gems.Count; j++)
                {
                    var gem = gems[j].GemEntity.GetComponent<SkillGem>();
                    if (gem.ExperienceToNextLevel > maxExperienceToNextLevel)
                    {
                        gemId = j;
                        gemInventoryId = i;
                        maxExperienceToNextLevel = gem.ExperienceToNextLevel;
                        startingExperienceToNextLevel = gem.ExperienceToNextLevel;
                    }
                }
            }
            LogMessage($"Max gem exp to next level is:{maxExperienceToNextLevel}");
            return maxExperienceToNextLevel > 0;
        }

        private bool FindIncub()
        {
            maxIncubAvailableKills = 0;
            for (var i = 0; i < GameController.IngameState.Data.ServerData.PlayerInventories.Count; i++)
            {
                var inventory = GameController.IngameState.Data.ServerData.PlayerInventories[i];
                var item = inventory.Inventory?.Items.ElementAtOrDefault(0);

                var mods = item?.GetComponent<Mods>();
                if (mods == null)
                {
                    continue;
                }

                var name = mods.IncubatorName;
                var kills = mods.IncubatorKills;
                if (name == null)
                {
                    continue;
                }

                if (incubatorMaxCapacities.ContainsKey(name))
                {
                    int incubAvailableKills = incubatorMaxCapacities[name] - kills;
                    if (incubAvailableKills > maxIncubAvailableKills)
                    {
                        maxIncubAvailableKills = incubAvailableKills;
                        incubatorInventoryId = i;
                        startingIncubatorKills = (ushort)kills;
                    }
                }
            }

            return maxIncubAvailableKills > 0;
        }

        public override void AreaChange(AreaInstance area)
        {
            //Thread.Sleep(Settings.PauseTime);
            //var players = GameController.Entities.Where(x => x.Type == ExileCore.Shared.Enums.EntityType.Player);
            //var enumerable = players.ToList();
            //DebugWindow.LogMsg($"Found {enumerable.Count} players", 20f);
            //if (enumerable.Count >= Settings.PartyCount)
            //{
            //    var names = enumerable.Select(player => player.GetComponent<Player>().PlayerName).ToList();
            //    File.WriteAllLines("partyinfo.txt", names);
            //}

            //if (killed > 0)
            //{
            //    DebugWindow.LogMsg($"#legion {gemExperienceGained},{killed},{gemExpPerMonster}", 20f);
            //}

            if (!area.IsHideout && !area.IsTown)
            {
                Refresh();
            }
            else
            {
                foundGem = false;
                foundIncub = false;
            }
        }

        private void Refresh()
        {
            foundGem = FindGem();
            foundIncub = FindIncub();
        }
    }
}