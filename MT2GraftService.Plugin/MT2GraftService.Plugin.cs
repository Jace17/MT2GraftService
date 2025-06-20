using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MT2GraftService.Plugin
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger = new(MyPluginInfo.PLUGIN_GUID);
        public void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            // Uncomment if you need harmony patches, if you are writing your own custom effects.
            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(SaveManager), "GetMerchantGoodsAtDistance")]
    public class AddGraftService
    {
        public static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("AddGraftService");
        public static void Postfix(SaveManager __instance, List<MerchantGoodState> __result)
        {
            if (__result.FindAll(x => x.IsService).Count > 3)
            {
                Log.LogInfo("Too many services. Not adding graft service.");
                return; // If there are already 4 services, do not add the graft service
            }

            GrantableRewardData grantableRewardData = __instance.GetAllGameData().FindRewardDataByName("LazarusLeagueLabGraftReward");
            if (grantableRewardData != null)
            {
                Traverse.Create(grantableRewardData).Field("_isServiceMerchantReward").SetValue(true); // Set the _isServiceMerchantReward field to true
                Traverse.Create(grantableRewardData).Field("_merchantServiceIndex").SetValue(4); // Set the _merchantServiceIndex field to 4
                Traverse.Create(grantableRewardData).Field("costs").SetValue(new int[] { 50, 100, 150, 200, 250 });


                MerchantGoodState merchantGoodState = new MerchantGoodState(grantableRewardData, __instance, false, null);
                Traverse.Create(merchantGoodState).Field("seen").SetValue(true); // Set the seen field to true
                __result.Add(merchantGoodState); // Add the new MerchantGoodState to the list
            }
        }
    }

    [HarmonyPatch(typeof(RewardData), "RewardTitle", MethodType.Getter)]
    public class OverrideRewardTitle
    {
        public static bool Prefix(RewardData __instance, ref string __result)
        {
            if (__instance.name == "LazarusLeagueLabGraftReward")
            {
                __result = "Graft Equipment";
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RewardData), "RewardDescription", MethodType.Getter)]
    public class OverrideRewardDescription
    {
        public static bool Prefix(RewardData __instance, ref string __result)
        {
            if (__instance.name == "LazarusLeagueLabGraftReward")
            {
                __result = "Graft an equipment from your deck to a unit.";
                return false;
            }
            return true;
        }
    }
}
