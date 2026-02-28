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

    public static int currentWave = 1;
    
    private static readonly MethodInfo CheckMove = AccessTools.Method(typeof(PlayerMovement), "IsPlayerMoving");
    private static readonly MethodInfo ResetScene = AccessTools.Method(typeof(SceneManager), "RestartScene");
    
    private MemoryMappedFile mmf;
    private MemoryMappedViewAccessor accessor;
    private static SpeedrunState state = new SpeedrunState();


    static private PlayerMovement _playerMovement;

    void Awake(){
        Harmony.CreateAndPatchAll(typeof(WizardGameSpeedrunIntegrator));

        mmf = MemoryMappedFile.CreateOrOpen("WizardGameSpeedrun", Marshal.SizeOf<SpeedrunState>());
        accessor = mmf.CreateViewAccessor();
        ResetNumbers();
    }

    void Update(){
        accessor.Write(0, ref state);
        if (Input.GetKeyDown(KeyCode.R)||Input.GetKeyDown(KeyCode.P)){
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