using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

[BepInPlugin("com.nikt.wizardgamespeedrunintegrator", "Wizard Game Livesplit Integration Tool", "0.1.0")]
public class WizardGameSpeedrunIntegrator : BaseUnityPlugin{
    private static float inGameTimer;
    private static float realLifeTime;

    public static int currentWave = 0;
    
    private static readonly MethodInfo CheckMove = AccessTools.Method(typeof(PlayerMovement), "IsPlayerMoving");
    
    private MemoryMappedFile mmf;
    private MemoryMappedViewAccessor accessor;
    private static SpeedrunState state = new SpeedrunState();

    void Awake(){
        Harmony.CreateAndPatchAll(typeof(WizardGameSpeedrunIntegrator));

        mmf = MemoryMappedFile.CreateOrOpen("WizardGameSpeedrun", Marshal.SizeOf<SpeedrunState>());
        accessor = mmf.CreateViewAccessor();
    }

    void Update(){
        accessor.Write(0, ref state);
    }

    [HarmonyPatch(typeof(PlayerMovement), "Update")]
    [HarmonyPrefix]
    static void MoveDetection(PlayerMovement __instance){
        if (!state.isRunStarted){
            Debug.Log($"#- Test, {CheckMove?.Invoke(__instance, null)}");
            if ((bool)CheckMove.Invoke(__instance, null)){
                state.isRunStarted = true;
            }
        }
    }

    //[HarmonyPatch(typeof(GameManager), "Start")]
    //[HarmonyPrefix]
    //static void StartDetection(){
    //    state = new SpeedrunState();
    //}
    
    [HarmonyPatch(typeof(HubShield), "OnTriggerEnter")]
    [HarmonyPrefix]
    static void EndDetection(HubShield __instance){
        if (state.bossFinished){
            Debug.Log("#- Hub ReEntered detected");
            state.gameFinished = true;
        }
    }
    
    [HarmonyPatch(typeof(HubShield), "UnlockHub")]
    [HarmonyPrefix]
    static void OnHubOpen(HubShield __instance){
        Debug.Log("#- Hub Opened detected");
        state.bossFinished = true;
    }

    [HarmonyPatch(typeof(WaveManager), "StartNewWave")]
    [HarmonyPrefix]
    static void StartWaveDetection(WaveManager __instance){
        Debug.Log($"#- Started a sub wave, current {currentWave}");
        currentWave++;
        switch (currentWave){
            case 1:
                state.wave1started = true;
                break;
            case 2:
                state.wave2started = true;
                break;
            case 3:
                state.wave3started = true;
                break;
            case 4:
                state.wave4started = true;
                break;
            case 5:
                state.wave5started = true;
                break;
            default:
                Debug.LogError("huh");
                break;
        }
    }
    
    [HarmonyPatch(typeof(SpawnerCoreShield), "ShieldBreak")]
    [HarmonyPrefix]
    static void StartWaveDetection(SpawnerCoreShield __instance){
        Debug.Log($"#- Finished a sub wave, current {currentWave}");
        switch (currentWave){
            case 1:
                state.wave1finished = true;
                break;
            case 2:
                state.wave2finished = true;
                break;
            case 3:
                state.wave3finished = true;
                break;
            case 4:
                state.wave4finished = true;
                break;
            case 5:
                state.wave5finished = true;
                break;
            default:
                Debug.LogError("#- huh");
                break;
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct SpeedrunState
{
    public bool isRunStarted;

    public bool wave1started;
    public bool wave1finished;
    public bool wave2started;
    public bool wave2finished;
    public bool wave3started;
    public bool wave3finished;
    public bool wave4started;
    public bool wave4finished;
    public bool wave5started;
    public bool wave5finished;

    public bool bossStart;
    public bool bossFinished;
    public bool gameFinished;
}