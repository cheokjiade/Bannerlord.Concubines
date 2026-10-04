using Concubines.Extensions;
using Concubines.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Concubines {
    public class ConsoleCommands {
        [CommandLineFunctionality.CommandLineArgumentFunction("debug_clear_npc_concubines", "concubines")]
        private static string DebugClearNPCConcubines(List<string> args) {
            foreach (ConcubineList data in ConcubineCampaignBehavior.Instance.ConcubineData.ToList()) {
                Hero heroFor = data.Hero;
                if (heroFor == Hero.MainHero)
                    continue;

                foreach (Hero concubine in data.Concubines.Keys.ToList())
                    concubine.DisposeAsConcubine();

                if (!ConcubineCampaignBehavior.Instance.ConcubineData.Contains(data))
                    Utils.PrintToMessages(heroFor.Name.ToString() + " CLEARED");
            }

            return "we good.";
        }


        [CommandLineFunctionality.CommandLineArgumentFunction("debug_clear_bad_governors", "concubines")]
        private static string DebugClearBadGovernors(List<string> args) {
            foreach (Hero hero in Campaign.Current.AliveHeroes) {
                Town? governorTown = hero.GovernorOf;
                if (governorTown != null) {
                    if (hero.Clan != governorTown.OwnerClan)
                        ChangeGovernorAction.RemoveGovernorOf(hero);
                }
            }

            return "we good.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("upgrade_all_owned_towns", "concubines")]
        private static string UpgradeOwnedTowns(List<string> args)
        {
            foreach(Town town in Hero.MainHero.Clan.Fiefs)
            {
                foreach (Building building in town.Buildings)
                {
                    for (int i = building.CurrentLevel; i < BuildingType.MaxLevel; i++)
                        building.LevelUp();
                }
                if (town.CurrentBuilding != null)
                    town.CurrentBuilding.BuildingProgress += 2000f;

            }

            return "we good.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("visit_all_settlements", "concubines")]
        private static string VisitAllSettlements(List<string> args)
        {
            foreach (Settlement s in Settlement.All)
            {
                s.HasVisited = true;
            }
            
            return "we good.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("my_children_equip_follow_mother", "concubines")]
        private static string DebugChildrenEquipFollowMother(List<string> args)
        {
            float heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
            foreach (Hero child in Hero.MainHero.Children)
            {
                if (child.IsAlive && !child.IsChild && child.IsKnownToPlayer && child.Clan == Hero.MainHero.Clan && child.Age >= heroComesOfAge)
                {
                    Bastards.StaticUtils.Utils.LegitimizeBastardFromHero(child);
                    EquipLikeMother(child);
                }
            }
            MaxOutTraits(Hero.MainHero);
            return "we good.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("debug_cheat_concubines", "concubines")]
        private static string DebugConcubinesLoveMe(List<string> args)
        {
            foreach (Hero concubine in ConcubineList.GetFor(Hero.MainHero).Concubines.Keys.ToList())
            {
                if (concubine.IsAlive)
                {
                    MaxOutSkillsAttributesTraits(concubine);
                    concubine.SetPersonalRelation(Hero.MainHero, 100);
                    concubine.SetBirthDay(CampaignTime.Now - CampaignTime.Years(18f));
                }
            }
            float heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
            foreach (Hero child in Hero.MainHero.Children)
            {
                if (child.IsAlive && child.IsChild && child.IsKnownToPlayer && child.Clan == Hero.MainHero.Clan && child.Age < heroComesOfAge)
                {
                    child.SetBirthDay(CampaignTime.Now - CampaignTime.Years(18f));
                    MaxOutSkillsAttributesTraits(child);
                    child.SetPersonalRelation(Hero.MainHero, 100);
                    if (child.Mother != null)
                        child.Culture = child.Mother.Culture;
                    TextObject name = NameGenerator.Current.GenerateHeroFirstName(child);
                    child.SetName(name, name);
                    EquipLikeMother(child);
                }
            }

            return "we good.";
        }

[CommandLineFunctionality.CommandLineArgumentFunction("family", "concubines")]
        private static string OpenFamilyMenu(List<string> args)
        {
            GameMenu.ActivateGameMenu(FamilyMenuBehavior.MenuId);
            return "opened the family menu.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("come_of_age", "concubines")]
        private static string ComeOfAge(List<string> args)
        {
            if (args.Count < 1)
            {
                string listing = string.Join(", ", Hero.MainHero.Children
                    .Where(c => c != null && c.IsAlive)
                    .Select(c => c.FirstName.ToString() + " (" + (int)c.Age + ")"));
                return "Usage: concubines.come_of_age <name> [teen]. Children: " + (listing.Length == 0 ? "none" : listing);
            }

            bool teen = args.Count > 1 && string.Equals(args[1], "teen", StringComparison.OrdinalIgnoreCase);
            float threshold = teen ? Campaign.Current.Models.AgeModel.BecomeTeenagerAge : Campaign.Current.Models.AgeModel.HeroComesOfAge;
            Hero match = Hero.MainHero.Children.FirstOrDefault(c => c != null && c.IsAlive &&
                (c.FirstName.ToString().IndexOf(args[0], StringComparison.OrdinalIgnoreCase) >= 0 ||
                 c.Name.ToString().IndexOf(args[0], StringComparison.OrdinalIgnoreCase) >= 0));
            if (match == null)
                return "No matching child found.";
            if (match.Age >= threshold)
                return match.FirstName + " is already " + (int)match.Age + " years old.";
            match.SetBirthDay(CampaignTime.Now - CampaignTime.Years(threshold));
            return match.FirstName + " is now " + (int)threshold + " years old.";
        }

        private static void MaxOutTraits(Hero hero)
        {
            hero.SetTraitLevel(DefaultTraits.Mercy, 2);
            hero.SetTraitLevel(DefaultTraits.Valor, 2);
            hero.SetTraitLevel(DefaultTraits.Generosity, 2);
            hero.SetTraitLevel(DefaultTraits.Calculating, 2);
            hero.SetTraitLevel(DefaultTraits.Honor, 2);
        }

        private static void MaxOutSkillsAttributesTraits(Hero hero)
        {
            foreach (CharacterAttribute characterAttribute in Attributes.All)
            {
                if (hero.GetAttributeValue(characterAttribute) < 20)
                {
                    hero.HeroDeveloper.AddAttribute(characterAttribute, 20 - hero.GetAttributeValue(characterAttribute), false);
                }
            }
            foreach (SkillObject skillObject in Skills.All)
            {
                hero.AddSkillXp(skillObject, 10000000);
            }
            MaxOutTraits(hero);
        }

        private static void EquipLikeMother(Hero child)
        {
            if (child.Mother == null || !child.Mother.IsAlive)
                return;
            child.BattleEquipment.FillFrom(child.Mother.BattleEquipment);
            child.CivilianEquipment.FillFrom(child.Mother.CivilianEquipment);
        }
    }

}
