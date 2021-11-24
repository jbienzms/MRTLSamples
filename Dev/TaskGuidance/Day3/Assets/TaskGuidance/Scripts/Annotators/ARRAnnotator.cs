using Microsoft.Azure.RemoteRendering.Unity;
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
    /// An annotator that creates annotations around an Azure Remote Rendering model.
    /// </summary>
    public class ARRAnnotator : AnnotatorBase
    {
        #region Member Variables
        private GameObject modelRoot;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [Header("Managers")]
        [Tooltip("The ASA manager that will create and locate anchors.")]
        [SerializeField]
        private RemoteRenderingManager arrManager;

        [Header("Content")]
        [SerializeField]
        [Tooltip("The name of the model to annotate.")]
        private string modelName = "builtin://Engine";
        public string ModelName => modelName;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <inheritdoc/>
        protected override async Task CommitPlacementAsync()
        {
            // ARR doesn't actually save the placement, but now that we're placed we can connect
            // and load our model.

            // Log
            this.Log($"Connecting to ARR session...");

            // Either resume the last session or start a new one
            await arrManager.ResumeLastOrCreateSessionAsync();

            // Log
            this.Log($"Loading model '{modelName}'...");

            // Load our model and create related Unity components
            modelRoot = await arrManager.LoadModelAsync(modelName, UnityCreationMode.CreateUnityComponents);
        }

        /// <inheritdoc/>
        protected override Task StartLocatingPlacementAsync()
        {
            throw new InvalidOperationException($"{nameof(ARRAnnotator)} does not support locating.");
        }
        #endregion // Internal Methods

        #region Unity Overrides
        protected override void Start()
        {
            // Pass on to base first
            base.Start();

            // Make sure we have an ASA Manager
            if (arrManager == null)
            {
                this.LogError("No ARR Manager was assigned. This annotator will be disabled.");
                enabled = false;
            }

            // Make sure we have a model to annotate
            if (string.IsNullOrEmpty(modelName))
            {
                this.LogError("No model to annotate. This annotator will be disabled.");
                enabled = false;
            }
        }
        #endregion // Unity Overrides

        #region Public Properties
        /// <inheritdoc/>
        public override bool CanAnnotate
        {
            get
            {
                // We can't annotate if ARR model hasn't been loaded
                if (modelRoot == null) { return false; }

                // Let the base class decide
                return base.CanAnnotate;
            }
        }

        /// <inheritdoc/>
        public override bool CanLocate
        {
            get
            {
                // ARR can never locate, it can only be placed
                return false;
            }
        }
        #endregion // Public Properties
    }
}