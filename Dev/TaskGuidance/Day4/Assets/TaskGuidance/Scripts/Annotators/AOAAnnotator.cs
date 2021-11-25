using Microsoft.Azure.ObjectAnchors.Unity;
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
    /// An annotator that creates annotations around an Azure Object Anchor.
    /// </summary>
    public class AOAAnnotator : AnnotatorBase
    {
        #region Unity Inspector Variables
        [Header("Managers")]
        [Tooltip("The AOA manager that will locate objects.")]
        [SerializeField]
        private ObjectAnchorManager aoaManager;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <inheritdoc/>
        protected override Task CommitPlacementAsync()
        {
            throw new InvalidOperationException($"{nameof(AOAAnnotator)} does not support placing.");
        }

        /// <summary>
        /// Attaches the placemark to our located object, moving the placemark to the correct location.
        /// </summary>
        /// <param name="instance">
        /// The anchor to apply
        /// </param>
        private void AttachLocatedAnchor(IObjectAnchorsServiceEventArgs instance)
        {
            // Make sure we have a visual
            if (PlacemarkVisual == null) { InstantiatePlacemark(); }

            // Move it to the object location
            var location = instance.Location;
            if (!location.HasValue) { throw new InvalidOperationException("Object Anchor does not have a valid location."); }

            // Move the placemark to the object position and orientation
            PlacemarkVisual.transform.SetPositionAndRotation(location.Value.Position, location.Value.Orientation);

            // Notify that we're now located
            NotifyLocated();
        }

        /// <inheritdoc/>
        protected override Task StartLocatingPlacementAsync()
        {
            // Log
            this.Log($"Starting Object Anchor search...");

            // Start locating
            aoaManager.IsLocating = true;

            // Done
            return Task.CompletedTask;
        }
        #endregion // Internal Methods

        #region Overrides / Event Handlers
        private void AOAManager_ServiceEvent(object sender, ObjectAnchorManager.ObjectAnchorsServiceEventArgs e)
        {
            // Was an object located?
            if (e.Kind == ObjectAnchorManager.ObjectAnchorsServiceEventKind.Added)
            {
                // Was it the one we're looking for?
                // For now, just assume any object
                // if (args.Identifier == ObjectData.Id)
                if (true)
                {
                    // Log
                    this.Log($"Object located '{e.Args.ModelId}'.");

                    // Attach to the object
                    if (!IsLocated)
                    {
                        AttachLocatedAnchor(e.Args);
                    }
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

            // Subscribe to AOA events
            if (aoaManager != null)
            {
                aoaManager.ServiceEvent += AOAManager_ServiceEvent;
            }
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            // Pass to base to complete
            base.OnDisable();

            // Unsubscribe from AOA events
            if (aoaManager != null)
            {
                aoaManager.ServiceEvent -= AOAManager_ServiceEvent;
            }
        }

        protected override void Start()
        {
            // Pass on to base first
            base.Start();

            // Make sure we have an AOA Manager
            if (aoaManager == null)
            {
                this.LogError("No AOA Manager was assigned. This annotator will be disabled.");
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
                // AOA can always locate as long as it hasn't been located and we're not
                // in the Unity editor

                return ((!IsLocating) && (!IsLocating) && (!Application.isEditor));
            }
        }

        /// <inheritdoc/>
        public override bool CanPlace
        {
            get
            {
                // Azure Object Anchor can't be placed, only located.
                return false;
            }
        }
        #endregion // Public Properties
    }
}