﻿using Verse;
using UnityEngine;
using System;

namespace VFE_Settlers.Settings {
    internal class ModSettings : Verse.ModSettings {
        #region Fields
        private static readonly int minReward = 20;
        private static readonly int maxReward = 200;
        #endregion Fields
        #region Properties
        public int MinReward = minReward;
        public int MaxReward = maxReward;
        #endregion Properties

        #region Methods
        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref MinReward, "MinReward", minReward);
            Scribe_Values.Look(ref MaxReward, "MaxReward", maxReward);
        }
        internal void Reset() {
            MinReward = minReward;
            MaxReward = maxReward;
        }
        #endregion Methods
    }
    internal static class SettingsHelper {
        public static ModSettings LatestVersion;
        public static void Reset() {
            LatestVersion.Reset();
        }
    }
    public class ModMain : Mod {

        private ModSettings settings;
        public ModMain(ModContentPack content) : base(content) {
            settings = GetSettings<ModSettings>();
            SettingsHelper.LatestVersion = settings;
        }
        public override string SettingsCategory() {
            return "Settlers";
        }
        public static Vector2 scrollPosition = Vector2.zero;


        public override void DoSettingsWindowContents(Rect inRect) {
            try {
                inRect.yMin += 20;
                inRect.yMax -= 20;
                Listing_Standard list = new Listing_Standard();
                Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
                Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, inRect.height * 2);
                Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
                list.Begin(rect2);
                if (list.ButtonText("DefaultSettings".Translate())) {
                    settings.Reset();
                };
                list.Label("MinReward".Translate(settings.MinReward * 100));
                settings.MinReward = (int)Mathf.Round(list.Slider(settings.MinReward, 10, 100));
                list.Label("MaxReward".Translate(settings.MaxReward * 100));
                settings.MaxReward = (int)Mathf.Round(list.Slider(settings.MaxReward, settings.MinReward, 250));

                list.End();
                Widgets.EndScrollView();
                settings.Write();

            }
            catch (Exception ex) {
                Log.Message(ex.Message);
            }
        }
    }
}
