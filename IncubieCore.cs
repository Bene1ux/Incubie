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
        private int MinExperienceToNextLevel => Settings.MonsterCount * 5400;
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
                    if (gem.ExperienceToNextLevel < MinExperienceToNextLevel)
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

                DebugWindow.LogMsg($"#Incubator: \"{name}\" Exists: {incubatorMaxCapacities.ContainsKey(name)}", 20f);
                if (!incubatorMaxCapacities.ContainsKey(name) ||
                    incubatorMaxCapacities.ContainsKey(name) &&
                    incubatorMaxCapacities[name] - Settings.MonsterCount < kills)
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
            Thread.Sleep(Settings.PauseTime);
            var players = GameController.Entities.Where(x => x.Type == ExileCore.Shared.Enums.EntityType.Player);
            var enumerable = players.ToList();
            DebugWindow.LogMsg($"Found {enumerable.Count} players", 20f);
            if (enumerable.Count >= Settings.PartyCount)
            {
                var names = enumerable.Select(player => player.GetComponent<Player>().PlayerName).ToList();
                File.WriteAllLines("partyinfo.txt", names);
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