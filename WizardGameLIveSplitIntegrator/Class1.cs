using BepInEx;
using HarmonyLib;
using UnityEngine;

[BepInPlugin("com.nikt.wizardgamespeedrunintegrator", "Wizard Game Livesplit integration tool", "0.1.0")]
public class WizardGameSpeedrunIntegrator : BaseUnityPlugin
{
    private static bool isRunStarted;
    
    private static bool wave1started;
    private static bool wave1finished;
    
    private static bool wave2started;
    private static bool wave2finished;
    
    private static bool wave3started;
    private static bool wave3finished;
    
    private static bool wave4started;
    private static bool wave4finished;
    
    private static bool wave5started;
    private static bool wave5finished;
    
    private static bool bossStart;
    private static bool bossFinished;
    
    private static bool gameFinished;

    public static int currentWave = 0;
    
    void Awake(){
        Harmony.CreateAndPatchAll(typeof(WizardGameSpeedrunIntegrator));
    }

    [HarmonyPatch(typeof(GameManager), "Start")]
    [HarmonyPrefix]
    static void StartDetection(GameManager __instance){
        isRunStarted = true;
    }
    
    [HarmonyPatch(typeof(HubShield), "OnTriggerEnter")]
    [HarmonyPrefix]
    static void EndDetection(HubShield __instance){
        if (bossFinished){
            gameFinished = true;
        }
    }
    
    

    [HarmonyPatch(typeof(WaveManager), "StartNewWave")]
    [HarmonyPrefix]
    static void StartWaveDetection(WaveManager __instance)
    {
        Debug.Log("Started a new sub wave");
        switch (currentWave){
            case 1:
                wave1started = true;
                break;
            case 2:
                wave2started = true;
                break;
            case 3:
                wave3started = true;
                break;
            case 4:
                wave4started = true;
                break;
            case 5:
                wave5started = true;
                break;
            default:
                Debug.LogError("huh");
                break;
        }
    }
    
    [HarmonyPatch(typeof(SpawnerCoreShield), "ShieldBreak")]
    [HarmonyPrefix]
    static void StartWaveDetection(SpawnerCoreShield __instance){
        Debug.Log("Finished a sub wave");
        currentWave++;
        switch (currentWave){
            case 1:
                wave1finished = true;
                break;
            case 2:
                wave2finished = true;
                break;
            case 3:
                wave3finished = true;
                break;
            case 4:
                wave4finished = true;
                break;
            case 5:
                wave5finished = true;
                break;
            default:
                Debug.LogError("huh");
                break;
        }
    }
    //SpawnerCoreShield  ShieldBreak
}