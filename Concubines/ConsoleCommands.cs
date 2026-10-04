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
                    building.LevelUp();
                    building.LevelUp();
                    building.LevelUp();
                }
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
            foreach (Hero child in Hero.MainHero.Children)
            {
                //&& Hero.MainHero.Children.Contains(child.Mother)
                if (child.IsAlive && !child.IsChild && child.IsKnownToPlayer && child.Clan == Hero.MainHero.Clan && child.Age >= (float)Campaign.Current.Models.AgeModel.HeroComesOfAge)
                {
                    Bastards.StaticUtils.Utils.LegitimizeBastardFromHero(child);
                    if (child.Mother.IsAlive)
                    {
                        child.BattleEquipment.FillFrom(child.Mother.BattleEquipment);
                        child.CivilianEquipment.FillFrom(child.Mother.CivilianEquipment);
                    }
                }
            }
            Hero.MainHero.SetTraitLevel(DefaultTraits.Mercy, 2);
            Hero.MainHero.SetTraitLevel(DefaultTraits.Valor, 2);
            Hero.MainHero.SetTraitLevel(DefaultTraits.Generosity, 2);
            Hero.MainHero.SetTraitLevel(DefaultTraits.Calculating, 2);
            Hero.MainHero.SetTraitLevel(DefaultTraits.Honor, 2);
            return "we good.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("debug_cheat_concubines", "concubines")]
        private static string DebugConcubinesLoveMe(List<string> args)
        {
            foreach (Hero concubine in ConcubineList.GetFor(Hero.MainHero).Concubines.Keys.ToList())
            {
                if (concubine.IsAlive)
                {
                    foreach (CharacterAttribute characterAttribute in Attributes.All)
                    {
                        if (concubine.GetAttributeValue(characterAttribute) < 20)
                        {
                            concubine.HeroDeveloper.AddAttribute(characterAttribute, 20 - concubine.GetAttributeValue(characterAttribute), false);
                        }
                    }
                    foreach (SkillObject skillObject in Skills.All)
                    {
                        concubine.AddSkillXp(skillObject, 10000000);
                    }
                    concubine.SetTraitLevel(DefaultTraits.Mercy, 2);
                    concubine.SetTraitLevel(DefaultTraits.Valor, 2);
                    concubine.SetTraitLevel(DefaultTraits.Generosity, 2);
                    concubine.SetTraitLevel(DefaultTraits.Calculating, 2);
                    concubine.SetTraitLevel(DefaultTraits.Honor, 2);
                    concubine.SetPersonalRelation(Hero.MainHero, 100);
                    concubine.SetBirthDay(CampaignTime.Now - CampaignTime.Years(18f));
                }
            }
            foreach (Hero child in Hero.MainHero.Children)
            {
                if (child.IsAlive && child.IsChild && child.IsKnownToPlayer && child.Clan == Hero.MainHero.Clan && child.Age < (float)Campaign.Current.Models.AgeModel.HeroComesOfAge)
                {
                    child.SetBirthDay(CampaignTime.Now - CampaignTime.Years(18f));
                    foreach (CharacterAttribute characterAttribute in Attributes.All)
                    {
                        if (child.GetAttributeValue(characterAttribute) < 20)
                        {
                            child.HeroDeveloper.AddAttribute(characterAttribute, 20 - child.GetAttributeValue(characterAttribute), false);
                        }
                    }
                    foreach (SkillObject skillObject in Skills.All)
                    {
                        child.AddSkillXp(skillObject, 10000000);
                    }
                    child.SetPersonalRelation(Hero.MainHero, 100);
                    child.SetTraitLevel(DefaultTraits.Mercy, 2);
                    child.SetTraitLevel(DefaultTraits.Valor, 2);
                    child.SetTraitLevel(DefaultTraits.Generosity, 2);
                    child.SetTraitLevel(DefaultTraits.Calculating, 2);
                    child.SetTraitLevel(DefaultTraits.Honor, 2);
                    child.Culture = child.Mother.Culture;
                    child.UpdatePlayerGender(true);
                    TextObject name = NameGenerator.Current.GenerateHeroFirstName(child);
                    child.SetName(name, name);
                    //child.BattleEquipment.GetEquipmentFromSlot(EquipmentIndex.Weapon1).
                    child.BattleEquipment.FillFrom(child.Mother.BattleEquipment);
                    child.CivilianEquipment.FillFrom(child.Mother.CivilianEquipment);
                }
            }

            return "we good.";
        }
    }

}
