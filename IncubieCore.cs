using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
        private const int MIN_EXP_TO_NEXT_LEVEL = 50000000;
        private uint startingExperienceToNextLevel;
        private ushort startingIncubatorKills;
        private int incubatorInventoryId;
        private uint gemExperienceGained;
        private ushort killed;
        private float gemExpPerMonster;

        private Dictionary<string, ushort> incubatorMaxCounts = new Dictionary<string, ushort>
        {
            {"Mysterious Incubator", 1},
            {"Skittering Incubator", 1},
            {"Fossilised Incubator", 1},
            {"Fragmented Incubator", 1},
            {"Abyssal Incubator", 1},
            {"Geomancer's Incubator", 21000},
            {"Thaumaturge's Incubator", 5000},
            {"Time-Lost Incubator", 33000},
            {"Foreboding Incubator", 1000},
            {"Maddening Incubator", 2000},
            {"Obscured Incubator", 1000},
            {"Celestial Armoursmith's Incubator", 3000},
            {"Celestial Blacksmith's Incubator", 3000},
            {"Celestial Jeweller's Incubator", 8000}
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

            if (foundGem)
            {
                var experienceToNextLevel = GameController.IngameState.Data.ServerData
                    .PlayerInventories[gemInventoryId].Inventory.Items[0]
                    .GetComponent<Sockets>().SocketedGems[gemId].GemEntity.GetComponent<SkillGem>()
                    .ExperienceToNextLevel;
                gemExperienceGained = startingExperienceToNextLevel - experienceToNextLevel;
                var gemMilExpGained = (float) gemExperienceGained / 1000000;
                Graphics.DrawText(gemMilExpGained.ToString(""), new Vector2(200, 200));
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

                var incubatorKills = (ushort) mods.IncubatorKills;
                killed = (ushort) (incubatorKills - startingIncubatorKills);
                Graphics.DrawText(killed.ToString(), new Vector2(200, 230));
                if (killed != 0 && Settings.ShowExpPerMonster)
                {
                    gemExpPerMonster = (float) gemExperienceGained / killed;
                    Graphics.DrawText(gemExpPerMonster.ToString(""), new Vector2(200, 260));
                }
            }
        }

        private bool FindGem()
        {
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
                    var gem = gems[0].GemEntity.GetComponent<SkillGem>();
                    if (gem.ExperienceToNextLevel < MIN_EXP_TO_NEXT_LEVEL)
                    {
                        continue;
                    }

                    gemId = j;
                    gemInventoryId = i;
                    startingExperienceToNextLevel = gem.ExperienceToNextLevel;
                    return true;
                }
            }

            return false;
        }

        private bool FindIncub()
        {
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

                DebugWindow.LogMsg($"#Incubator: \"{name}\" Exists: {incubatorMaxCounts.ContainsKey(name)}", 20f);
                if (!incubatorMaxCounts.ContainsKey(name) ||
                    incubatorMaxCounts.ContainsKey(name) && incubatorMaxCounts[name] < kills)
                {
                    continue;
                }

                incubatorInventoryId = i;
                startingIncubatorKills = (ushort) kills;
                return true;
            }

            return false;
        }

        public override void AreaChange(AreaInstance area)
        {
            var players = GameController.Entities.Where(x => x.Type == ExileCore.Shared.Enums.EntityType.Player);
            var enumerable = players.ToList();
            DebugWindow.LogMsg($"Found {enumerable.Count} players", 20f);
            if (enumerable.Count >= 6)
            {
                var names = enumerable.Select(player => player.GetComponent<Player>().PlayerName).ToList();
                File.AppendAllLines("partyinfo.txt", names);
            }
            
            if (killed > 0)
            {
                DebugWindow.LogMsg($"#legion {gemExperienceGained},{killed},{gemExpPerMonster}", 20f);
            }

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