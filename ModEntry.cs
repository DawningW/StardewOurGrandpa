using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace StardewOurGrandpa;

public class ModEntry : Mod
{
    public static ModEntry Instance { get; private set; } = null!;

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.PatchAll();
    }

    [HarmonyPatch]
    public class GrandpaEvaluationPatch
    {
        [HarmonyPatch(typeof(Utility), nameof(Utility.getGrandpaScore))]
        static bool Prefix(ref int __result)
        {
            List<Farmer> farmers = new(Game1.getAllFarmers());
            if (farmers.Count == 1) return true;

            int score = 0;

            if (Game1.player.totalMoneyEarned >= 50000) score++;
            if (Game1.player.totalMoneyEarned >= 100000) score++;
            if (Game1.player.totalMoneyEarned >= 200000) score++;
            if (Game1.player.totalMoneyEarned >= 300000) score++;
            if (Game1.player.totalMoneyEarned >= 500000) score++;
            if (Game1.player.totalMoneyEarned >= 1000000) score += 2;

            if (farmers.Exists(farmer => farmer.achievements.Contains(5))) score++;

            if (farmers.Exists(farmer => farmer.hasSkullKey)) score++;

            // bool hasCommunityCenter = Game1.isLocationAccessible("CommunityCenter");
            bool hasCommunityCenter = Utility.HasAnyPlayerSeenEvent("191393"); // fix bug with DedicatedServer Mod
            if (hasCommunityCenter || farmers.Exists(farmer => farmer.hasCompletedCommunityCenter())) score++;
            if (hasCommunityCenter) score += 2;

            if (farmers.Exists(farmer => farmer.isMarriedOrRoommates() && Utility.getHomeOfFarmer(farmer).upgradeLevel >= 2)) score++;

            if (farmers.Exists(farmer => farmer.hasRustyKey)) score++;

            if (farmers.Exists(farmer => farmer.achievements.Contains(26))) score++;
            if (farmers.Exists(farmer => farmer.achievements.Contains(34))) score++;

            int numberOfFriendsWithinThisRange = farmers.Max(farmer => Utility.getNumberOfFriendsWithinThisRange(farmer, 1975, 999999));
            if (numberOfFriendsWithinThisRange >= 5) score++;
            if (numberOfFriendsWithinThisRange >= 10) score++;

            int level = farmers.Max(farmer => farmer.Level);
            if (level >= 15) score++;
            if (level >= 25) score++;

            if (farmers.Exists(farmer => farmer.mailReceived.Contains("petLoveMessage"))) score++;

            Instance.Monitor.Log($"Grandpa's evaluation score: {score}", LogLevel.Debug);
            __result = score;
            return false;
        }
    }
}
