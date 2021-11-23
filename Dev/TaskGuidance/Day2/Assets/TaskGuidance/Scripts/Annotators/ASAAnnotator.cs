using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using Microsoft.MixedReality.Toolkit.Physics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TaskGuidance
{
    /// <summary>
    /// An annotator that creates annotations around an Azure Spatial Anchor.
    /// </summary>
    public class ASAAnnotator : AnnotatorBase
    {
        #region Nested Types
        /// <summary>
        /// The differnet states for ASA in this annotator.
        /// </summary>
        private enum ASAState
        {
            /// <summary>
            /// The ASA anchor hasn't been placed.
            /// </summary>
            NotPlaced,
            /// <summary>
            /// The ASA anchor is being located.
            /// </summary>
            Locating,

            /// <summary>
            /// The ASA anchor has been located.
            /// </summary>
            Located
        }
        #endregion // Nested Types

        #region Member Variables
        private ASAState asaState;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [Tooltip("The prefab that will be used to represent the center of the ASA Anchor.")]
        [SerializeField]
        private GameObject anchorPrefab;

        [Tooltip("The ASA manager that will create and locate anchors.")]
        [SerializeField]
        private SpatialAnchorManager asaManager;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Ensures that we have a valid active ASA session
        /// </summary>
        private async Task EnsureASASessionAsync()
        {
            // If an ASA session hasn't been started yet, start one now
            if (!asaManager.IsSessionStarted)
            {
                Debug.Log($"{nameof(ASAAnnotator)}: Starting a new ASA session...");
                await asaManager.StartSessionAsync();
            }
        }

        /// <summary>
        /// Creates a visual to represent the center of the anchor.
        /// </summary>
        /// <returns>
        /// The visual.
        /// </returns>
        private void InstantiateVisual()
        {
            // Create the visual to represent the center of the anchor
            if (anchorPrefab != null)
            {
                // Use the prefab
                ObjectVisual = GameObject.Instantiate(anchorPrefab);
            }
            else
            {
                // Create a small sphere to represent
                ObjectVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ObjectVisual.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
        }

        /// <summary>
        /// Attaches the specified anchor to our visual, moving the visual to the correct location.
        /// </summary>
        /// <param name="anchor">
        /// The anchor to apply
        /// </param>
        private void AttachLocatedAnchor(CloudSpatialAnchor anchor)
        {
            // Make sure we have a visual
            if (ObjectVisual == null) { InstantiateVisual(); }

            // Add a cloud native anchor to the visual
            CloudNativeAnchor cna = ObjectVisual.AddComponent<CloudNativeAnchor>();

            // Convert the cloud anchor format to the native anchor format, which moves the visual
            // to the right location
            cna.CloudToNative(anchor);

            // It's now placed
            asaState = ASAState.Located;
            IsVisualPlaced = true;
        }
        #endregion // Internal Methods

        #region Overrides / Event Handlers
        private void ASAManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            // Was it just now located?
            if (args.Status == LocateAnchorStatus.Located)
            {
                // Was it the one we're looking for?
                if (args.Identifier == ObjectData.Id)
                {
                    // Log
                    Debug.Log($"{nameof(ASAAnnotator)}: Anchor located: '{args.Identifier}'.");

                    // Attach the anchor, but use the UI thread to do it
                    UnityDispatcher.InvokeOnAppThread(()=> AttachLocatedAnchor(args.Anchor));
                }
            }
        }
        #endregion // Overrides / Event Handlers

        #region Unity Overrides
        /// <inheritdoc/>
        protected override void OnEnable()
        {
            // Pass to base first
            base.OnEnable();

            // Subscribe to ASA events
            if (asaManager != null)
            {
                asaManager.AnchorLocated += ASAManager_AnchorLocated;
            }
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            // Pass to base to complete
            base.OnDisable();

            // Unsubscribe from ASA events
            if (asaManager != null)
            {
                asaManager.AnchorLocated -= ASAManager_AnchorLocated;
            }
        }

        protected override void Start()
        {
            // Pass on to base first
            base.Start();

            // Make sure we have an ASA Manager
            if (asaManager == null)
            {
                Debug.LogError($"{nameof(ASAAnnotator)}: No ASA Manager was assigned. This annotator will be disabled.");
                enabled = false;
            }
        }
        #endregion // Unity Overrides

        #region Public Methods
        /// <inheritdoc/>
        public override async Task<bool> TryLoadAsync()
        {
            // We can only try to locate the anchor if it isn't already placed or being located
            if (asaState != ASAState.NotPlaced)
            {
                Debug.LogWarning($"{nameof(ASAAnnotator)}: Attemting to load but state is {asaState}.");
                return false;
            }

            // If we don't have an anchor ID to load, there's nothing to do
            if (string.IsNullOrEmpty(ObjectData.Id)) { return false; }

            // If we're running in the Unity Editor, ASA functions won't work
            if (Application.isEditor)
            {
                Debug.Log($"{nameof(ASAAnnotator)}: Can't load ASA data in Unity Editor.");
                return false;
            }

            // Make sure we have a valid ASA sesion
            await EnsureASASessionAsync();

            // Change our internal state to know that we're trying to locate the anchor
            asaState = ASAState.Locating;

            // Create a criteria that will search for the Anchor ID we care about
            AnchorLocateCriteria criteria = new AnchorLocateCriteria()
            {
                Identifiers = new string[] { ObjectData.Id }
            };

            // Log
            Debug.Log($"{nameof(ASAAnnotator)}: Starting a search for anchor '{ObjectData.Id}'.");

            // Start a watcher to locate the criteria above
            asaManager.Session.CreateWatcher(criteria);

            // Loading
            return true;
        }

        /// <inheritdoc/>
        public override async Task<bool> TryPlaceVisualAsync(FocusDetails focus)
        {
            // We can only place the visual if we haven't already placed it and aren't trying to
            // locate an existing anchor
            if (asaState != ASAState.NotPlaced)
            {
                Debug.LogWarning($"{nameof(ASAAnnotator)}: Attemting to place visual but state is {asaState}.");
                return false;
            }

            // Make sure we have a visual to place
            if (ObjectVisual == null) { InstantiateVisual(); }

            // Move the visual to where the user tapped and orientate it correctly
            ObjectVisual.transform.position = focus.Point;
            ObjectVisual.transform.rotation = Quaternion.LookRotation(focus.Normal * -1f, Vector3.up);

            // We can only do ASA things on a real device and not in the editor
            if (Application.isEditor)
            {
                Debug.Log($"{nameof(ASAAnnotator)}: Can't create ASA anchor in Unity Editor.");
            }
            else
            {
                // Make sure we have a valid ASA sesion
                await EnsureASASessionAsync();

                // Add a cloud native anchor to the visual
                CloudNativeAnchor cna = ObjectVisual.AddComponent<CloudNativeAnchor>();

                // Convert the native anchor format to cloud anchor format
                await cna.NativeToCloud();

                // Log
                Debug.Log($"{nameof(ASAAnnotator)}: Creating a new anchor...");

                // Save the anchor
                await asaManager.CreateAnchorAsync(cna.CloudAnchor);

                // Store the ID of the anchor that was created so we can load the anchor again later
                ObjectData.Id = cna.CloudAnchor.Identifier;

                // Log
                Debug.Log($"{nameof(ASAAnnotator)}: Anchor created with ID '{ObjectData.Id}'.");
            }

            // It's now placed
            asaState = ASAState.Located;
            IsVisualPlaced = true;
            return true;
        }
        #endregion // Public Methods
    }
}