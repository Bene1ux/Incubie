using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using SharpDX;

namespace Incubie
{
    class IncubieCore : BaseSettingsPlugin<IncubSettings>
    {
        private int gemInventoryId;
        private int gemId;
        private bool foundGem;
        private bool foundIncub;
        private const int MIN_EXP_TO_NEXT_LEVEL = 53000000;
        private uint startingExperienceToNextLevel;
        private ushort startingIncubatorKills;
        private int incubatorInventoryId;
        private uint gemExperienceGained;
        private ushort killed;
        private float gemExpPerMonster;

        private Dictionary<string, ushort> incubatorMaxCounts = new Dictionary<string, ushort>
        {
            {"Fragmented Incubator", 5000},
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

        public override void Render()
        {
            if (!Settings.MyCheckboxOption)
            {
                return;
            }

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
                Graphics.DrawText(gemMilExpGained.ToString(""), new Vector2(200, 200), Color.White, 30);
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
                if (killed != 0)
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
            DebugWindow.LogMsg($"#legion {gemExperienceGained},{killed},{gemExpPerMonster}", 20f);
            if (!area.IsHideout && !area.IsTown && !area.HasWaypoint)
            {
                foundGem = FindGem();
                foundIncub = FindIncub();
            }
            else
            {
                foundGem = false;
                foundIncub = false;
            }
        }
    }
}