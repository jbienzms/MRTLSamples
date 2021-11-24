using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.Azure.RemoteRendering;
using Microsoft.Azure.RemoteRendering.Unity;
using Quaternion = UnityEngine.Quaternion;
using System.Globalization;

#if UNITY_WSA
using UnityEngine.XR.ARFoundation;
#endif

// ask Unity to automatically append an ARRServiceUnity component when a RemoteRendering script is attached
[RequireComponent(typeof(ARRServiceUnity))]
public class RemoteRenderingManager : MonoBehaviour
{
    #region Constants
    private readonly string LastSessionIdKey = "Microsoft.Azure.RemoteRendering.Quickstart.LastSessionId";
    #endregion // Constants

    #region Member Variables
    private ARRServiceUnity arrService = null;
    private string sessionId = null;
    #endregion // Member Variables

    #region Unity Inspector Variables
    [Header("Authentication")]
    [SerializeField]
    [Tooltip("The domain to use when authenticating with the Remote Rendering service.")]
    private string accountDomain = "";
    public string AccountDomain => accountDomain;

    [SerializeField]
    [Tooltip("The Account ID provided by the Remtoe Rendering service portal.")]
    private string accountId = "";
    public string AccountId => accountId;

    [SerializeField]
    [Tooltip("The Account Key provided by the Remote Rendering service portal.")]
    private string accountKey = "";
    public string AccountKey => accountKey;


    [Header("Connection")]
    [SerializeField]
    [Tooltip("The domain to use when connecting to the Remote Rendering service.")]
    private string remoteRenderingDomain = "";
    public string RemoteRenderingDomain => remoteRenderingDomain;


    [Header("Session")]
    [SerializeField]
    [Tooltip("The maximum time a session can run in hours.")]
    private uint maxLeaseTimeHours = 0;
    public uint MaxLeaseTimeHours => maxLeaseTimeHours;

    [SerializeField]
    [Tooltip("The maximum time a session can run in minutes.")]
    private uint maxLeaseTimeMinutes = 10;
    public uint MaxLeaseTimeMinutes => maxLeaseTimeMinutes;

    [SerializeField]
    [Tooltip("The size of the ARR VM to creatge.")]
    private RenderingSessionVmSize vmSize = RenderingSessionVmSize.Standard;
    public RenderingSessionVmSize VMSize => vmSize;
    #endregion // Unity Inspector Variables

    #region Internal Methods
    /// <summary>
    /// Connects to a the ARR service.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> that represents the operation.
    /// </returns>
    private async Task ConnectAsync()
    {
        // Make sure the service object is initalized
        EnsureServiceInitialized();

        // Don't connect if already connected
        if (arrService.CurrentActiveSession?.ConnectionStatus != ConnectionStatus.Disconnected)
        {
            return;
        }

        // Attempt to connect
        ConnectionStatus res = await arrService.CurrentActiveSession.ConnectAsync(new RendererInitOptions());

        // If not connected, fail task
        if (!arrService.CurrentActiveSession.IsConnected)
        {
            throw new InvalidOperationException("ARR service failed to connect.");
        }
    }

    /// <summary>
    /// Initialzies and configures the ARR service object.
    /// </summary>
    private void EnsureServiceInitialized()
    {
        if (arrService.Client != null)
        {
            // early out if the front-end has been created before
            return;
        }

        // initialize the ARR service with our account details.
        // Trim the strings in case they have been pasted into the inspector with trailing whitespace
        SessionConfiguration sessionConfiguration = new SessionConfiguration();
        sessionConfiguration.AccountKey = accountKey.Trim();
        sessionConfiguration.AccountId = accountId.Trim();
        sessionConfiguration.RemoteRenderingDomain = remoteRenderingDomain.Trim();
        sessionConfiguration.AccountDomain = accountDomain.Trim();

        arrService.Initialize(sessionConfiguration);
    }

    /// <summary>
    /// Logs the specified error.
    /// </summary>
    /// <param name="message">
    /// The message to log.
    /// </param>
    private void LogError(string message)
    {
        Debug.LogError(message);
    }

    /// <summary>
    /// Logs the specified message.
    /// </summary>
    /// <param name="message">
    /// The message to log.
    /// </param>
    private void LogMessage(string message)
    {
        Debug.Log(message);
    }

    /// <summary>
    /// Logs a sessions tatus.
    /// </summary>
    /// <param name="sessionProperties">
    /// The session properties to log.
    /// </param>
    private void LogSessionStatus(RenderingSessionProperties sessionProperties)
    {
        Debug.Log($"Session '{sessionProperties.Id}' is {sessionProperties.Status}. Size={sessionProperties.Size}" +
                  (!string.IsNullOrEmpty(sessionProperties.Hostname) ? $", Hostname='{sessionProperties.Hostname}'" : "") +
                  (!string.IsNullOrEmpty(sessionProperties.Message) ? $", Message='{sessionProperties.Message}'" : ""));
    }

    /// <summary>
    /// Logs the status of the specified session.
    /// </summary>
    /// <param name="session">
    /// The session to log.
    /// </param>
    private async void LogSessionStatus(RenderingSession session)
    {
        if (session != null)
        {
            var sessionProperties = await session.GetPropertiesAsync();
            LogSessionStatus(sessionProperties.SessionProperties);
        }
        else
        {
            var sessionProperties = arrService.LastProperties;
            LogMessage($"Session ended: Id={sessionProperties.Id}");
        }
    }

    /// <summary>
    /// Called when the manager initializes in order to configure the service.
    /// </summary>
    protected virtual void LoadConfiguration()
    {
        // Attempt to load configuration from config resource if present.
        RemoteRenderingConfig config = Resources.Load<RemoteRenderingConfig>("RemoteRenderingConfig");
        if (config != null)
        {
            // Authentication
            accountDomain = config.AccountDomain;
            accountId = config.AccountId;
            accountKey = config.AccountKey;

            // Connection
            remoteRenderingDomain = config.RemoteRenderingDomain;
        }
    }
    #endregion // Internal Methods

    #region Overrides / Event Handlers
    private void ARRService_OnSessionStatusChanged(ARRServiceUnity service, RenderingSession session)
    {
        LogSessionStatus(session);
    }
    #endregion // Overrides / Event Handlers

    #region Unity Overrides
    /// <inheritdoc/>
    protected virtual void Awake()
    {
        LoadConfiguration();

        // initialize Azure Remote Rendering for use in Unity:
        // it needs to know which camera is used for rendering the scene
        RemoteUnityClientInit clientInit = new RemoteUnityClientInit(Camera.main);
        RemoteManagerUnity.InitializeManager(clientInit);

        // Get the ARRServiceUnity component
        arrService = GetComponent<ARRServiceUnity>();

        // Subscribe to session events
        if (arrService != null)
        {
            arrService.OnSessionStatusChanged += ARRService_OnSessionStatusChanged;
        }
    }

    /// <inheritdoc/>
    protected virtual void LateUpdate()
    {
        // The session must have its runtime pump updated.
        // The update will push messages to the server, receive messages, and update the frame-buffer with the remotely rendered content.
        arrService?.CurrentActiveSession?.Connection.Update();
    }

    /// <inheritdoc/>
    protected virtual void OnDestroy()
    {
        Disconnect();

        if (arrService != null)
        {
            arrService.OnSessionStatusChanged -= ARRService_OnSessionStatusChanged;
        }

        RemoteManagerStatic.ShutdownRemoteRendering();
    }
    #endregion // Unity Overrides

    #region Public Methods
    /// <summary>
    /// Connects to a new ARR Session.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> that represents the operation.
    /// </returns>
    public async Task CreateNewSessionAsync()
    {
        try
        {
            // Connect first
            await ConnectAsync();

            // Start a new session
            RenderingSessionProperties props = await arrService.StartSession(new RenderingSessionCreationOptions(VMSize, (int)MaxLeaseTimeHours, (int)MaxLeaseTimeMinutes));

            // Make sure it started
            if (props.Status != RenderingSessionStatus.Ready)
            {
                SessionGeneralContext context = new SessionGeneralContext()
                {
                    ErrorMessage = "New rendering session failed to enter the ready state."
                };
                throw new RRSessionException(context);
            }

            // Store session ID for later resume
            LastSessionId = arrService.CurrentActiveSession.SessionUuid;
        }
        catch (RRSessionException sessionException)
        {
            LogError($"Error connecting to runtime: {sessionException.Context.ErrorMessage}");
            throw;
        }
        catch (RRException generalException)
        {
            LogError($"General error connecting to runtime: {generalException.ErrorCode}");
            throw;
        }
        catch (Exception ex)
        {
            LogError($"Unknown error connecting to runtime: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Loads the specified model and returns a <see cref="GameObject"/> that represents it.
    /// </summary>
    /// <param name="modelName">
    /// The name of the model to load.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that yields the loaded model.
    /// </returns>
    public async Task<GameObject> LoadModelAsync(string modelName, UnityCreationMode creationMode)
    {
        // Validate
        if ((arrService == null) || (!arrService.CurrentActiveSession.IsConnected)) { throw new InvalidOperationException($"{nameof(LoadModelAsync)} called but no active session."); }

        // Create a root object to parent a loaded model to.
        Entity modelEntity = arrService.CurrentActiveSession.Connection.CreateEntity();

        // Get the game object representation of this entity.
        GameObject modelEntityGO = modelEntity.GetOrCreateGameObject(UnityCreationMode.DoNotCreateUnityComponents);

        // Ensure the entity will sync translations with the server.
        var sync = modelEntityGO.GetComponent<RemoteEntitySyncObject>();
        sync.SyncEveryFrame = true;

        // Hide the scene tree until the model is loaded and we had time to get the AABB and recenter the model.
        var stateOverride = modelEntityGO.CreateArrComponent<ARRHierarchicalStateOverrideComponent>(arrService.CurrentActiveSession);
        stateOverride.RemoteComponent.HiddenState = HierarchicalEnableState.ForceOn;

        // Load the model and report progress
        var loadModelParams = new LoadModelFromSasOptions(modelName, modelEntity);
        var loadModelResult = await arrService.CurrentActiveSession.Connection.LoadModelFromSasAsync(loadModelParams, (float progress) =>
        {
            LogMessage($"Loading Model: {progress.ToString("P2", CultureInfo.InvariantCulture)}");
        });

        // Create the root and sync on every frame
        var rootGO = loadModelResult.Root.GetOrCreateGameObject(creationMode);
        rootGO.GetComponent<RemoteEntitySyncObject>().SyncEveryFrame = true;

        // Model is loaded and recentered. We can show the model now.
        stateOverride.RemoteComponent.HiddenState = HierarchicalEnableState.InheritFromParent;

        // Return the created object
        return rootGO;
    }

    /// <summary>
    /// Resumes the last session if possible. Otherwise, creates a new session.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> that represents the operation.
    /// </returns>
    public Task ResumeLastOrCreateSessionAsync()
    {
        // Get the last ID
        string lastId = LastSessionId;

        // If we have a valid last ID, try to use it
        if (!string.IsNullOrEmpty(lastId))
        {
            return ResumeOrCreateSessionAsync(lastId);
        }
        else
        {
            return CreateNewSessionAsync();
        }

    }

    /// <summary>
    /// Resumes the specified session if possible. Otherwise, creates a new session.
    /// </summary>
    /// <param name="sessionId">
    /// The ID of the session to attempt to resume.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the operation.
    /// </returns>
    public async Task ResumeOrCreateSessionAsync(string sessionId)
    {
        // Validate
        if (string.IsNullOrEmpty(sessionId)) { throw new ArgumentException(nameof(sessionId)); }

        // The resume task will fail with a RRSessionException if the session can't be resumed
        try
        {
            // First try to resume the session
            await ResumeSessionAsync(sessionId);
        }
        catch (RRSessionException)
        {
            // Next try to create
            await CreateNewSessionAsync();
        }
    }

    /// <summary>
    /// Resumes the specified ARR session.
    /// </summary>
    /// <param name="sessionId">
    /// The ID of the session to resume.
    /// </param>
    public async Task ResumeSessionAsync(string sessionId)
    {
        // Validate
        if (string.IsNullOrEmpty(sessionId)) { throw new ArgumentException(nameof(sessionId)); }

        try
        {
            // Make sure the service object is initalized
            EnsureServiceInitialized();

            // Connect first
            await ConnectAsync();

            // Open the existing session
            RenderingSessionProperties props = await arrService.OpenSession(sessionId);

            // Make sure it's ready
            if (props.Status != RenderingSessionStatus.Ready)
            {
                SessionGeneralContext context = new SessionGeneralContext()
                {
                    ErrorMessage = "$Rendering session { sessionId } not available."
                };
                throw new RRSessionException(context);
            }

            // Store session ID for later resume
            LastSessionId = arrService.CurrentActiveSession.SessionUuid;
        }
        catch (RRSessionException sessionException)
        {
            LogError($"Error resuming session: {sessionException.Context.ErrorMessage}");
            throw;
        }
        catch (RRException generalException)
        {
            LogError($"General error resuming session: {generalException.ErrorCode}");
            throw;
        }
        catch (Exception ex)
        {
            LogError($"Unknown error resuming session: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Disconnects from any active ARR session.
    /// </summary>
    public void Disconnect()
    {
        if (IsConnected)
        {
            arrService.CurrentActiveSession.Disconnect();
        }
    }
    #endregion // Public Methods

    #region Public Properties
    /// <summary>
    /// Gets a value that indicates if currently connected to an ARR session.
    /// </summary>
    public bool IsConnected
    {
        get
        {
            return (arrService?.CurrentActiveSession?.ConnectionStatus == ConnectionStatus.Connected);
        }
    }

    /// <summary>
    /// Gets or sets the last session id so we can try to resume on future app runs.
    /// </summary>
    public string LastSessionId
    {
        get
        {
            #if UNITY_EDITOR
            sessionId = UnityEditor.EditorPrefs.GetString(LastSessionIdKey);
            #else
            sessionId = PlayerPrefs.GetString(LastSessionIdKey);
            #endif
            return sessionId;
        }

        set
        {
            #if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetString(LastSessionIdKey, value);
            #else
            PlayerPrefs.SetString(LastSessionIdKey, value);
            #endif
            sessionId = value;
        }
    }
    #endregion // Public Properties
}
