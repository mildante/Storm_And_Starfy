using System.IO;
using NUnit.Framework;

public class NetworkFlowAssetTests
{
    private const string Level2ScenePath = "Assets/Scenes/Level2.unity";
    private const string FinishPanelScriptPath = "Assets/Scripts/UI/FinishPanel.cs";
    private const string ShipHelmScriptPath = "Assets/Scripts/Level/ShipHelmCheckpoint.cs";
    private const string ShipHelmPrefabPath = "Assets/Resources/ShipHelm.prefab";
    private const string LevelManagerScriptPath = "Assets/Scripts/Level/LevelManager.cs";
    private const string StoryPanelScriptPath = "Assets/Scripts/UI/StoryPanelController.cs";
    private const string StarCollectibleScriptPath = "Assets/Scripts/Level/StarCollectible.cs";
    private const string PlayerMovementScriptPath = "Assets/Scripts/Player/PlayerMovement.cs";
    private const string StarfyMovementScriptPath = "Assets/Scripts/Player/StarfyMovement.cs";
    private const string StormPrefabPath = "Assets/Resources/Storm.prefab";
    private const string StarfyPrefabPath = "Assets/Resources/Starfy.prefab";
    private const string MovingLeverScriptPath = "Assets/Scripts/Level/MovingLever.cs";
    private const string WaterDamageScriptPath = "Assets/Scripts/Level/WaterDamage.cs";
    private const string ButtonManagerScriptPath = "Assets/Scripts/UI/ButtonManager.cs";
    private const string PanelsPrefabPath = "Assets/Resources/Panels.prefab";

    [Test]
    public void Level2Scene_DoesNotContainStaticStarfyPrefabInstance()
    {
        string level2Scene = File.ReadAllText(Level2ScenePath);

        Assert.That(
            level2Scene,
            Does.Not.Contain("guid: fa6f2aa8df0b6eb41811c6807f070be0"),
            "Level2 should rely on NetworkPlayerSpawner instead of a scene-placed Starfy prefab.");
    }

    [Test]
    public void FinishPanel_ClassNameMatchesFileAndUsesLevelManagerRequests()
    {
        string finishPanel = File.ReadAllText(FinishPanelScriptPath);

        Assert.That(finishPanel, Does.Contain("public class FinishPanel"));
        Assert.That(finishPanel, Does.Not.Contain("public class VictoryPanel"));
        Assert.That(finishPanel, Does.Not.Contain("PhotonNetwork.LoadLevel"));
        Assert.That(finishPanel, Does.Contain("RequestRestartLevel"));
        Assert.That(finishPanel, Does.Contain("RequestReturnToMenu"));
    }

    [Test]
    public void ShipHelmCheckpoint_ClassNameMatchesFileAndPrefabComponent()
    {
        string shipHelmScript = File.ReadAllText(ShipHelmScriptPath);
        string shipHelmPrefab = File.ReadAllText(ShipHelmPrefabPath);

        Assert.That(shipHelmScript, Does.Contain("public class ShipHelmCheckpoint"));
        Assert.That(shipHelmScript, Does.Not.Contain("public class WheelCheckpoint"));
        Assert.That(shipHelmPrefab, Does.Contain("Assembly-CSharp::ShipHelmCheckpoint"));
    }

    [Test]
    public void LevelManager_DoesNotKeepLegacyExitDoorReference()
    {
        string levelManager = File.ReadAllText(LevelManagerScriptPath);

        Assert.That(levelManager, Does.Not.Contain("ExitDoor exitDoor"));
        Assert.That(levelManager, Does.Not.Contain("OpenDoor"));
    }

    [Test]
    public void StoryPanel_RequestsHostDrivenFinishInsteadOfLoadingSceneDirectly()
    {
        string storyPanel = File.ReadAllText(StoryPanelScriptPath);

        Assert.That(storyPanel, Does.Not.Contain("PhotonNetwork.LoadLevel"));
        Assert.That(storyPanel, Does.Contain("PhotonNetwork.IsMasterClient"));
        Assert.That(storyPanel, Does.Contain("RequestLoadNextLevel"));
    }

    [Test]
    public void LevelManager_HasHostDrivenLevel2TransitionRequest()
    {
        string levelManager = File.ReadAllText(LevelManagerScriptPath);

        Assert.That(levelManager, Does.Contain("NextLevelRequestEvent"));
        Assert.That(levelManager, Does.Contain("RequestLoadNextLevel"));
        Assert.That(levelManager, Does.Contain("LoadLevel2AsHost"));
        Assert.That(levelManager, Does.Contain("PhotonNetwork.LoadLevel(\"Level2\")"));
    }

    [Test]
    public void StarCollectible_SynchronizesCollectionFromLocalStormOnly()
    {
        string starCollectible = File.ReadAllText(StarCollectibleScriptPath);

        Assert.That(starCollectible, Does.Contain("IOnEventCallback"));
        Assert.That(starCollectible, Does.Contain("PhotonNetwork.RaiseEvent"));
        Assert.That(starCollectible, Does.Contain("CompareTag(\"Storm\")"));
        Assert.That(starCollectible, Does.Contain("playerView == null || playerView.IsMine"));
        Assert.That(starCollectible, Does.Not.Contain("Destroy(gameObject)"));
    }

    [Test]
    public void StormPrefab_UsesSlowLowJumpMovementStats()
    {
        string stormPrefab = File.ReadAllText(StormPrefabPath);

        Assert.That(stormPrefab, Does.Contain("Assembly-CSharp::PlayerMovement"));
        Assert.That(stormPrefab, Does.Contain("speed: 2"));
        Assert.That(stormPrefab, Does.Contain("jumpForce: 4"));
    }

    [Test]
    public void StarfyPrefab_UsesFastHighJumpMovementStats()
    {
        string starfyPrefab = File.ReadAllText(StarfyPrefabPath);

        Assert.That(starfyPrefab, Does.Contain("Assembly-CSharp::StarfyMovement"));
        Assert.That(starfyPrefab, Does.Contain("speed: 5"));
        Assert.That(starfyPrefab, Does.Contain("jumpForce: 7"));
    }

    [Test]
    public void StormAttack_RequestsNetworkEnemyDeath()
    {
        string playerMovement = File.ReadAllText(PlayerMovementScriptPath);

        Assert.That(playerMovement, Does.Contain("pig.RequestDie()"));
        Assert.That(playerMovement, Does.Contain("kingPig.RequestDie()"));
    }

    [Test]
    public void PlayerMovement_GroundsCharactersOnTopOfEachOther()
    {
        string playerMovement = File.ReadAllText(PlayerMovementScriptPath);
        string starfyMovement = File.ReadAllText(StarfyMovementScriptPath);

        Assert.That(playerMovement, Does.Contain("IsStandingOnOtherPlayer()"));
        Assert.That(playerMovement, Does.Contain("CompareTag(\"Storm\")"));
        Assert.That(playerMovement, Does.Contain("CompareTag(\"Starfy\")"));

        Assert.That(starfyMovement, Does.Contain("IsStandingOnOtherPlayer()"));
        Assert.That(starfyMovement, Does.Contain("CompareTag(\"Storm\")"));
        Assert.That(starfyMovement, Does.Contain("CompareTag(\"Starfy\")"));
    }

    [Test]
    public void MovingLever_UsesPhotonRpcAndLocalPlayerInput()
    {
        string movingLever = File.ReadAllText(MovingLeverScriptPath);

        Assert.That(movingLever, Does.Contain("MonoBehaviourPun"));
        Assert.That(movingLever, Does.Contain("[PunRPC]"));
        Assert.That(movingLever, Does.Contain("photonView.RPC"));
        Assert.That(movingLever, Does.Contain("GetComponentInParent<PhotonView>"));
        Assert.That(movingLever, Does.Contain("platform.MoveToState(isOn)"));
    }

    [Test]
    public void WaterDamage_TracksDamageDelayPerPlayer()
    {
        string waterDamage = File.ReadAllText(WaterDamageScriptPath);

        Assert.That(waterDamage, Does.Contain("Dictionary<GameObject, Coroutine>"));
        Assert.That(waterDamage, Does.Contain("damageCoroutines[player]"));
        Assert.That(waterDamage, Does.Contain("damageCoroutines.Remove(player)"));
        Assert.That(waterDamage, Does.Contain("OnTriggerStay2D"));
        Assert.That(waterDamage, Does.Contain("health.TakeDamage()"));
        Assert.That(waterDamage, Does.Not.Contain("health.KillInstantly()"));
    }

    [Test]
    public void Level1Water_UsesFilledCompositeTrigger()
    {
        string level1Scene = File.ReadAllText("Assets/Scenes/Level1.unity");

        Assert.That(level1Scene, Does.Contain("m_EditorClassIdentifier: Assembly-CSharp::WaterDamage"));
        Assert.That(level1Scene, Does.Contain("m_CompositeGameObject: {fileID: 899309640}"));
        Assert.That(level1Scene, Does.Contain("m_GeometryType: 1"));
    }

    [Test]
    public void LegacyNextButton_IsDetachedFromObsoleteLoadNextLevel()
    {
        string buttonManager = File.ReadAllText(ButtonManagerScriptPath);
        string panelsPrefab = File.ReadAllText(PanelsPrefabPath);

        Assert.That(buttonManager, Does.Not.Contain("LoadNextLevel"));
        Assert.That(panelsPrefab, Does.Not.Contain("m_MethodName: LoadNextLevel"));
    }
}
