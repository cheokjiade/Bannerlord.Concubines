using BetterCore.Utils;
using Concubines.Settings;
using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Concubines {
    public class Concubines : MBSubModuleBase {
        public static Random Random = new();
        public static MCMSettings Settings { get; private set; }
        public static string ModName { get; private set; } = "Concubines";

        private bool isInitialized = false;
        private bool isLoaded = false;

        //FIRST
        protected override void OnSubModuleLoad() {
            try {
                base.OnSubModuleLoad();

                if (isInitialized)
                    return;

                Harmony h = new("Bannerlord.Windwhistle." + ModName);

                h.PatchAll();

                isInitialized = true;
            } catch (Exception e) {
                NotifyHelper.WriteError(ModName, "OnSubModuleLoad threw exception " + e);
            }
        }

        //SECOND
        protected override void OnBeforeInitialModuleScreenSetAsRoot() {
            try {
                base.OnBeforeInitialModuleScreenSetAsRoot();

                if (isLoaded)
                    return;

                ModName = base.GetType().Assembly.GetName().Name;

                Settings = MCMSettings.Instance ?? throw new NullReferenceException("Settings are null");

                NotifyHelper.WriteMessage(ModName + " Loaded.", MsgType.Good);


                isLoaded = true;
            } catch (Exception e) {
                NotifyHelper.WriteError(ModName, "OnBeforeInitialModuleScreenSetAsRoot threw exception " + e);
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter) {
            if (game.GameType is Campaign) {
                CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarter;

                campaignStarter.AddBehavior(new ConcubineCampaignBehavior(campaignStarter));
                campaignStarter.AddBehavior(new FamilyMenuBehavior(campaignStarter));
            }
        }
    }
}