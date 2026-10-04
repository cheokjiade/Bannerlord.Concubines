using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace Concubines {
    public class FamilyMenuBehavior : CampaignBehaviorBase {
        public const string MenuId = "concubines_family";
        private const int MaxPickListSize = 12;

        public FamilyMenuBehavior(CampaignGameStarter starter) {
            AddMenus(starter);
        }

        public override void RegisterEvents() { }

        public override void SyncData(IDataStore dataStore) { }

        private void AddMenus(CampaignGameStarter starter) {
            starter.AddGameMenu(MenuId, "You attend to family matters.", null, GameMenu.MenuOverlayType.None, GameMenu.MenuFlags.None, null);

            starter.AddGameMenuOption(MenuId, "concubines_family_growup", "Make a child grow up.",
                args => HasChildBelow(Campaign.Current.Models.AgeModel.HeroComesOfAge),
                args => ShowGrowUpInquiry(Campaign.Current.Models.AgeModel.HeroComesOfAge, "Make a child grow up"),
                false, 0, false, null);

            starter.AddGameMenuOption(MenuId, "concubines_family_teenager", "Age a child into a teenager.",
                args => HasChildBelow(Campaign.Current.Models.AgeModel.BecomeTeenagerAge),
                args => ShowGrowUpInquiry(Campaign.Current.Models.AgeModel.BecomeTeenagerAge, "Age a child into a teenager"),
                false, 1, false, null);

            starter.AddGameMenuOption(MenuId, "concubines_family_clothing", "Manage your children's clothing.",
                args => PlayerClanChildren().Any(),
                args => GameMenu.SwitchToMenu("concubines_family_clothing"),
                false, 2, false, null);

            starter.AddGameMenuOption(MenuId, "concubines_family_legitimize", "Legitimize a bastard child.",
                args => BastardChildren().Any(),
                args => ShowLegitimizeInquiry(),
                false, 3, false, null);

            starter.AddGameMenuOption(MenuId, "concubines_family_leave", "Leave.",
                null, null, true, 4, false, null);

            starter.AddGameMenu("concubines_family_clothing", "How should your children dress?", null, GameMenu.MenuOverlayType.None, GameMenu.MenuFlags.None, null);
            starter.AddGameMenuOption("concubines_family_clothing", "concubines_family_clothing_gender", "Daughters dress like their mother; sons like their father.",
                null,
                args => {
                    ApplyOutfits(child => child.IsFemale ? (child.Mother ?? child.Father) : (child.Father ?? child.Mother));
                    GameMenu.SwitchToMenu(MenuId);
                }, false, 0, false, null);
            starter.AddGameMenuOption("concubines_family_clothing", "concubines_family_clothing_mother", "All children dress like their mother.",
                null,
                args => {
                    ApplyOutfits(child => child.Mother);
                    GameMenu.SwitchToMenu(MenuId);
                }, false, 1, false, null);
            starter.AddGameMenuOption("concubines_family_clothing", "concubines_family_clothing_father", "All children dress like their father.",
                null,
                args => {
                    ApplyOutfits(child => child.Father);
                    GameMenu.SwitchToMenu(MenuId);
                }, false, 2, false, null);
            starter.AddGameMenuOption("concubines_family_clothing", "concubines_family_clothing_back", "Back.",
                null, args => GameMenu.SwitchToMenu(MenuId), false, 3, false, null);

            starter.AddGameMenuOption("town", "concubines_family_town", "Attend to family matters.",
                null, args => GameMenu.SwitchToMenu(MenuId), false, 10, false, null);
        }

        private static bool HasChildBelow(float age) {
            return Hero.MainHero.Children.Any(c => c != null && c.IsAlive && c.Age < age);
        }

        private static IEnumerable<Hero> PlayerClanChildren() {
            return Hero.MainHero.Children.Where(c => c != null && c.IsAlive && c.Clan == Hero.MainHero.Clan);
        }

        private static IEnumerable<Hero> BastardChildren() {
            return Hero.MainHero.Children.Where(c => c != null && c.IsAlive && Bastards.StaticUtils.Utils.GetBastardFromHero(c) != null);
        }

        private static void ShowGrowUpInquiry(float threshold, string title) {
            List<InquiryElement> elements = Hero.MainHero.Children
                .Where(c => c != null && c.IsAlive && c.Age < threshold)
                .Take(MaxPickListSize)
                .Select(c => new InquiryElement(c, c.Name.ToString() + " (" + (int)c.Age + " years old)", null))
                .ToList();
            if (elements.Count == 0)
                return;

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(title, "Select a child.", elements, true, 1, 1, "Confirm", "Cancel",
                list => { foreach (InquiryElement element in list) GrowUp((Hero)element.Identifier, threshold); },
                null, null, false);
            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }

        private static void GrowUp(Hero child, float targetAge) {
            child.SetBirthDay(CampaignTime.Now - CampaignTime.Years(targetAge));
            Utils.PrintToMessages("{=FamilyChildGrown}{CHILD} is now {AGE} years old.", 255, 229, 204,
                ("CHILD", child.Name.ToString()), ("AGE", ((int)targetAge).ToString()));
        }

        private static void ApplyOutfits(Func<Hero, Hero> pickModel) {
            int count = 0;
            foreach (Hero child in PlayerClanChildren().ToList()) {
                Hero model = pickModel(child);
                if (model == null || model == child)
                    continue;
                child.BattleEquipment.FillFrom(model.BattleEquipment);
                child.CivilianEquipment.FillFrom(model.CivilianEquipment);
                count++;
            }
            Utils.PrintToMessages("{=FamilyChildrenDressed}{COUNT} of your children have been outfitted.", 255, 229, 204, ("COUNT", count.ToString()));
        }

        private static void ShowLegitimizeInquiry() {
            float cost = GetLegitimizeCost();
            List<InquiryElement> elements = BastardChildren()
                .Take(MaxPickListSize)
                .Select(c => new InquiryElement(c, c.Name.ToString() + " (" + (int)cost + " influence)", null))
                .ToList();
            if (elements.Count == 0)
                return;

            MultiSelectionInquiryData data = new MultiSelectionInquiryData("Legitimize a bastard child", "Select a child to legitimize.", elements, true, 1, 1, "Confirm", "Cancel",
                list => { foreach (InquiryElement element in list) Legitimize((Hero)element.Identifier, cost); },
                null, null, false);
            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }

        private static float GetLegitimizeCost() {
            float cost = Bastards.Settings.MCMSettings.Instance.LegitimizeInfluenceCost;
            if (!Bastards.StaticUtils.Utils.IsHeroKing(Hero.MainHero))
                cost *= 2f;
            return cost;
        }

        private static void Legitimize(Hero child, float cost) {
            if (Hero.MainHero.Clan.Influence < cost) {
                Utils.PrintToMessages("{=FamilyLegitimizeNoInfluence}You lack the influence to legitimize {CHILD}.", 255, 100, 100, ("CHILD", child.Name.ToString()));
                return;
            }
            Hero.MainHero.Clan.Influence -= cost;
            Bastards.StaticUtils.Utils.LegitimizeBastardFromHero(child);
            Utils.PrintToMessages("{=FamilyChildLegitimized}{CHILD} has been legitimized.", 255, 229, 204, ("CHILD", child.Name.ToString()));
        }
    }
}

