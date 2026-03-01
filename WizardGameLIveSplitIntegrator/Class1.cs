using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

[BepInPlugin("com.nikt.wizardgamespeedrunintegrator", "Wizard Game Livesplit Integration Tool", "0.1.0")]
public class WizardGameSpeedrunIntegrator : BaseUnityPlugin{
    private static ConfigEntry<bool> TASWaves;
    private static ConfigEntry<bool> TASSubWaves;
    private static ConfigEntry<bool> PReset;
    private static float inGameTimer;
    private static float realLifeTime;

    public static int currentWave = 1;
    
    private static readonly MethodInfo CheckMove = AccessTools.Method(typeof(PlayerMovement), "IsPlayerMoving");
    private static readonly MethodInfo ResetScene = AccessTools.Method(typeof(SceneManager), "RestartScene");
    
    private MemoryMappedFile mmf;
    private MemoryMappedViewAccessor accessor;
    private static SpeedrunState state = new SpeedrunState();


    static private PlayerMovement _playerMovement;

    void Awake(){
        Harmony.CreateAndPatchAll(typeof(WizardGameSpeedrunIntegrator));

        PReset = Config.Bind("Options", "Reset on P", true, "Resets when pressing the letter P on thy keyboard");
        TASWaves = Config.Bind("TAS", "Wave RNG Manipulation", false, "Forces 3 waves until bigboy");
        TASSubWaves = Config.Bind("TAS", "Sub-wave RNG Manipulation", false, "Forces minimum amount of enemies in subwave");

        mmf = MemoryMappedFile.CreateOrOpen("WizardGameSpeedrun", Marshal.SizeOf<SpeedrunState>());
        accessor = mmf.CreateViewAccessor();
        ResetNumbers();
    }

    void Update(){
        accessor.Write(0, ref state);
        if (Input.GetKeyDown(KeyCode.R)||(Input.GetKeyDown(KeyCode.P)&& PReset.Value)){

            ResetNumbers();
            ResetScene.Invoke(SceneManager.Instance, null);
        }
    }

    static void ResetNumbers(){
        state.isRunStarted = 0;

        currentWave = 1;
        state.wave1started = 0;
        state.wave1finished = 0;
        state.wave2started = 0;
        state.wave2finished = 0;
        state.wave3started = 0;
        state.wave3finished = 0;
        state.wave4started = 0;
        state.wave4finished = 0;
        state.wave5started = 0;
        state.wave5finished = 0;

        state.gameFinished = 0;
    }

    [HarmonyPatch(typeof(SceneManager), "RestartScene")]
    [HarmonyPrefix]
    static void RestartDetect(){
        ResetNumbers();
    }

    [HarmonyPatch(typeof(WaveManager), "Start")]
    [HarmonyPrefix]
    static void GameStart(){
        ResetNumbers();
    }
    // Reset logic ^

    //Timer start
    [HarmonyPatch(typeof(PlayerMovement), "Update")]
    [HarmonyPrefix]
    static void MoveDetection(PlayerMovement __instance){
        if(_playerMovement == null){
            _playerMovement = __instance;
        }
        if (state.isRunStarted != 1){
            Debug.Log($"#- Test, {CheckMove?.Invoke(__instance, null)}");
            if ((bool)CheckMove.Invoke(__instance, null)){
                state.isRunStarted = 1;
            }else{
                ResetNumbers();
            }
        }
    }
    

    [HarmonyPatch(typeof(HubShield), "OnTriggerEnter")]
    [HarmonyPrefix]
    static void EndDetection(HubShield __instance){
        if (state.hubOpened == 1){
            Debug.Log("#- Hub ReEntered detected");
            state.gameFinished = 1;
            state.isRunStarted = 0;
        }
    }
    
    [HarmonyPatch(typeof(HubShield), "UnlockHub")]
    [HarmonyPrefix]
    static void OnHubOpen(HubShield __instance){
        Debug.Log("#- Hub Opened detected");
        state.wave1started = 1;
        state.wave1finished = 1;
        state.wave2started = 1;
        state.wave2finished = 1;
        state.wave3started = 1;
        state.wave3finished = 1;
        state.wave4started = 1;
        state.wave4finished = 1;
        state.wave5started = 1;
        state.wave5finished = 1;
        state.hubOpened = 1;
    }

    [HarmonyPatch(typeof(SpawnerZone), "OnTriggerEnter")]
    [HarmonyPrefix]
    static void StartWaveDetection(SpawnerZone __instance){
        Debug.Log($"#- Started a sub wave, current {currentWave}");
        switch (currentWave){
            case 1:
                state.wave1started = 1;
                break;
            case 2:
                state.wave2started = 1;
                break;
            case 3:
                state.wave3started = 1;
                break;
            case 4:
                state.wave4started = 1;
                break;
            case 5:
                state.wave5started = 1;
                break;
            default:
                Debug.LogError("huh");
                break;
        }


    }
    
    [HarmonyPatch(typeof(WaveManager), "SubWaveEnded")]
    [HarmonyPostfix]
    static void EndWaveDetection(SpawnerCoreShield __instance){
        Debug.Log($"#- Finished a sub wave, current {currentWave}");
        switch (currentWave){
            case 1:
                state.wave1finished = 1;
                break;
            case 2:
                state.wave2finished = 1;
                break;
            case 3:
                state.wave3finished = 1;
                break;
            case 4:
                state.wave4finished = 1;
                break;
            case 5:
                state.wave5finished = 1;
                break;
            default:
                Debug.LogError("#- huh");
                break;
        }

        currentWave++;
    }

    //TAS RNG manipulation
    
    [HarmonyPatch(typeof(WaveManager), "Start")]
    [HarmonyPostfix]
    static void TASWaveOverride(WaveManager __instance){
        if(TASWaves.Value){
            AccessTools.Field(typeof(WaveManager), "_maxSubWaves").SetValue(__instance, 
            AccessTools.Field(typeof(WaveManager), "_minSubWaves").GetValue(__instance));
        }
    }

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

[StructLayout(LayoutKind.Sequential)]
public struct SpeedrunState{
    public byte isRunStarted;  // 0
    public byte wave1started;  // 1
    public byte wave1finished; // 2
    public byte wave2started;  // 3
    public byte wave2finished; // 4
    public byte wave3started;  // 5
    public byte wave3finished; // 6
    public byte wave4started;  // 7
    public byte wave4finished; // 8
    public byte wave5started;  // 9
    public byte wave5finished; // 10
    public byte hubOpened;     // 11
    public byte gameFinished;  // 12
}