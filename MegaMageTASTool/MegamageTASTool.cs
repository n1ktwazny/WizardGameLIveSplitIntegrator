using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

[BepInPlugin("com.nikt.megamagetastool", "Mega Mage TAS Tool", "0.1.0")]
public class MegaMageTASTool : BaseUnityPlugin{
    private static ConfigEntry<bool> TASWaves;
    private static ConfigEntry<bool> TASSubWaves;

    static private PlayerMovement _playerMovement;

    void Awake(){
        Harmony.CreateAndPatchAll(typeof(MegaMageTASTool));

        TASWaves = Config.Bind("TAS", "Wave RNG Manipulation", false, "Forces 3 waves until bigboy");
        TASSubWaves = Config.Bind("TAS", "Sub-wave RNG Manipulation", false, "Forces minimum amount of enemies in subwave");
    }
    
    //TAS Wave limit
    [HarmonyPatch(typeof(WaveManager), "Start")]
    [HarmonyPostfix]
    static void TASWaveOverride(WaveManager __instance){
        if(TASWaves.Value){
            AccessTools.Field(typeof(WaveManager), "_maxSubWaves").SetValue(__instance, 
            AccessTools.Field(typeof(WaveManager), "_minSubWaves").GetValue(__instance));
        }
    }

    //TAS Subwave enemy amounts 
    [HarmonyPatch(typeof(WaveManager), "Start")]
    [HarmonyPostfix]
    static void TASSubWaveOverride(WaveManager __instance){
        if(TASSubWaves.Value){
            AccessTools.Field(typeof(WaveManager), "_maxEnemyQuota").SetValue(__instance, 
            AccessTools.Field(typeof(WaveManager), "_minEnemyQuota").GetValue(__instance));

            AccessTools.Field(typeof(WaveManager), "_timeToSpawnMax").SetValue(__instance, 
            AccessTools.Field(typeof(WaveManager), "_timeToSpawnMin").GetValue(__instance));
        }
    }
}