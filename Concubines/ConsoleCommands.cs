using Concubines.Extensions;
using Concubines.Models;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
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
