using Godot;
using System;
using System.Threading.Tasks;

public partial class LoadingScreen : Node
{
    [Export] private Label _loadingLabel;
    [Export] private AnimationPlayer _loadingAnimationPlayer;

    private Godot.Collections.Array _progress = new Godot.Collections.Array();
    private float _loadingStartValue = 0.0f;
    private string _targetScenePath;

    private ResourceLoader.ThreadLoadStatus _loadStatus;
    private PackedScene _loadedSceneResource;

    private const string MainMenuScenePath = "res://scenes/MainMenu.tscn";

    public static string SceneToLoad { get; set; }

    public override void _Ready()
    {
        //_loadingValue = (float)_progress[0];
        if (_loadingLabel == null)
        {
            GD.PrintErr("LoadingLabel is not assigned in the inspector.");
            GetTree().ChangeSceneToFile(MainMenuScenePath);

            return;
        }

        _targetScenePath = SceneToLoad;

        if (string.IsNullOrEmpty(SceneToLoad))
        {
            GD.PrintErr("SceneToLoad is not set. Please set it before loading the scene.");
            GetTree().ChangeSceneToFile(MainMenuScenePath);

            return;
        }
        StartLoadingScene();
    }

    public async Task CreateTimer(float seconds)
    {
        await ToSignal(GetTree().CreateTimer(seconds), "timeout");
    }

    public async override void _Process(double delta)
    {
        if (string.IsNullOrEmpty(_targetScenePath))
        {
            GD.PrintErr("Target scene path is not set. Cannot process loading.");
            GetTree().ChangeSceneToFile(MainMenuScenePath);
            return;
        }

        _loadStatus = ResourceLoader.LoadThreadedGetStatus(_targetScenePath, _progress);
        switch (_loadStatus)
        {
            case ResourceLoader.ThreadLoadStatus.InProgress:
                if (_progress.Count > 0)
                {
                    float progressValue = (float)_progress[0];

                    if (_loadingAnimationPlayer == null)
                        GD.PrintErr("LoadingAnimationPlayer is not assigned in the inspector.");
                    else
                        _loadingAnimationPlayer.Play("LoadingAnimation");

                    await CreateTimer(0.1f); // Simulate some delay for loading
                }
                break;
            case ResourceLoader.ThreadLoadStatus.Loaded:
                if (_loadingAnimationPlayer == null)
                    GD.PrintErr("LoadingAnimationPlayer is not assigned in the inspector.");
                else
                    _loadingAnimationPlayer.Play("LoadedAnimation");
                if (Input.IsAnythingPressed())
                {
                    var loadedScene = ResourceLoader.LoadThreadedGet(_targetScenePath) as PackedScene;
                    if (loadedScene != null)
                    {
                        GD.Print("Scene loaded successfully: " + _targetScenePath);
                        _targetScenePath = null;
                        GetTree().ChangeSceneToPacked(loadedScene);
                    }
                    else
                    {
                        GD.PrintErr($"Failed to load scene from path: {SceneToLoad}");
                        GetTree().ChangeSceneToFile(MainMenuScenePath);
                    }
                }
                break;
            case ResourceLoader.ThreadLoadStatus.Failed:
            case ResourceLoader.ThreadLoadStatus.InvalidResource:
                GD.PrintErr($"Failed to load scene from path: '{SceneToLoad}'. Status: {_loadStatus}");
                GetTree().ChangeSceneToFile(MainMenuScenePath);
                _targetScenePath = null;
                break;
        }
    }

    private void StartLoadingScene()
    {
        Error err = ResourceLoader.LoadThreadedRequest(_targetScenePath, "", false);
        if (err != Error.Ok)
        {
            GD.PrintErr($"Error starting load request for {_targetScenePath}: {err}");
            GetTree().ChangeSceneToFile(MainMenuScenePath);
            _targetScenePath = null;
            return;
        }
        _loadStatus = ResourceLoader.LoadThreadedGetStatus(_targetScenePath);
    }
}
