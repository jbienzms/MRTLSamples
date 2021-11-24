using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using Microsoft.MixedReality.Toolkit.Physics;
using System;
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
        #region Unity Inspector Variables
        [Tooltip("The ASA manager that will create and locate anchors.")]
        [SerializeField]
        private SpatialAnchorManager asaManager;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <inheritdoc/>
        protected override async Task CommitPlacementAsync()
        {
            // We can only do ASA things on a real device and not in the editor
            if (Application.isEditor)
            {
                this.Log("Can't create ASA anchor in Unity Editor.");
            }
            else
            {
                // Log
                this.Log("Creating a new anchor...");

                // Make sure we have a valid ASA sesion
                await EnsureASASessionAsync();

                // Add a cloud native anchor to the visual
                CloudNativeAnchor cna = PlacemarkVisual.AddComponent<CloudNativeAnchor>();

                // Convert the native anchor format to cloud anchor format
                await cna.NativeToCloud();

                // Set to auto expire 1 month from now
                cna.CloudAnchor.Expiration = DateTime.UtcNow.AddMonths(1);

                // Save the anchor
                await asaManager.CreateAnchorAsync(cna.CloudAnchor);

                // Store the ID of the anchor that was created so we can load the anchor again later
                ObjectData.Id = cna.CloudAnchor.Identifier;

                // Log
                this.Log($"Anchor created with ID '{ObjectData.Id}'.");
            }
        }

        /// <summary>
        /// Ensures that we have a valid active ASA session
        /// </summary>
        private async Task EnsureASASessionAsync()
        {
            // If an ASA session hasn't been started yet, start one now
            if (!asaManager.IsSessionStarted)
            {
                this.Log("Starting a new ASA session...");
                await asaManager.StartSessionAsync();
            }
        }

        /// <summary>
        /// Attaches the located anchor to our placemark, moving the placemark to the correct location.
        /// </summary>
        /// <param name="anchor">
        /// The anchor to apply
        /// </param>
        private void AttachLocatedAnchor(CloudSpatialAnchor anchor)
        {
            // Make sure we have a visual
            if (PlacemarkVisual == null) { InstantiatePlacemark(); }

            // Add a cloud native anchor to the visual
            CloudNativeAnchor cna = PlacemarkVisual.AddComponent<CloudNativeAnchor>();

            // Convert the cloud anchor format to the native anchor format, which moves the visual
            // to the right location
            cna.CloudToNative(anchor);

            // Notify that we're now located
            NotifyLocated();
        }

        /// <inheritdoc/>
        protected override async Task StartLocatingPlacementAsync()
        {
            // Log
            this.Log($"Starting a search for anchor '{ObjectData.Id}'.");

            // Make sure we have a valid ASA sesion
            await EnsureASASessionAsync();

            // Create a criteria that will search for the Anchor ID we care about
            AnchorLocateCriteria criteria = new AnchorLocateCriteria()
            {
                Identifiers = new string[] { ObjectData.Id }
            };

            // Start a watcher to locate the criteria above
            asaManager.Session.CreateWatcher(criteria);
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
                    this.Log($"Anchor located '{args.Identifier}'.");

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
                this.LogError("No ASA Manager was assigned. This annotator will be disabled.");
                enabled = false;
            }
        }
        #endregion // Unity Overrides

        #region Public Properties
        /// <inheritdoc/>
        public override bool CanLocate
        {
            get
            {
                // If base says no, then no
                if (!base.CanLocate) { return false; }

                // ASA can't locate in the Unity editor
                return (!Application.isEditor);
            }
        }
        #endregion // Public Properties
    }
}