using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TaskGuidance
{
    /// <summary>
    /// Handles saving and loading annotations as well as interacting with annotators.
    /// </summary>
    public class AnnotationManager : InputSystemGlobalHandlerListener, IMixedRealityInputActionHandler
    {
        #region Member Variables
        private bool dataLoaded;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [Header("Annotators")]
        [Tooltip("The app data that should be visualized.")]
        [SerializeField]
        private AnnotationAppData appData;

        [Tooltip("The annotator for ASA.")]
        [SerializeField]
        private ASAAnnotator asaAnnotator;

        [Tooltip("The annotator for ARR.")]
        [SerializeField]
        private ARRAnnotator arrAnnotator;

        [Tooltip("The annotator for AOA.")]
        [SerializeField]
        private AOAAnnotator aoaAnnotator;

        [Header("Content")]
        [Tooltip("The input action to switch to ASA.")]
        [SerializeField]
        private MixedRealityInputAction asaAction;

        [Tooltip("The input action to switch to ARR.")]
        [SerializeField]
        private MixedRealityInputAction arrAction;

        [Tooltip("The input action to switch to AOA.")]
        [SerializeField]
        private MixedRealityInputAction aoaAction;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Activates the specified annotator.
        /// </summary>
        /// <param name="annotator">
        /// The type of annotator to activate.
        /// </param>
        private void ActivateAnnotator(AnnotatorType annotator)
        {
            // Log
            this.Log($"Switching to {annotator} annotator.");

            // Placeholder
            AnnotatorBase newAnnotator = null;

            // Get the new annotator and disable the others
            switch (annotator)
            {
                case AnnotatorType.AzureSpatialAnchor:
                    newAnnotator = asaAnnotator;
                    arrAnnotator.enabled = false;
                    aoaAnnotator.enabled = false;
                    break;
                case AnnotatorType.AzureRemoteRender:
                    newAnnotator = arrAnnotator;
                    asaAnnotator.enabled = false;
                    aoaAnnotator.enabled = false;
                    break;
                case AnnotatorType.AzureObjectAnchor:
                    newAnnotator = aoaAnnotator;
                    asaAnnotator.enabled = false;
                    arrAnnotator.enabled = false;
                    break;
                default:
                    this.LogWarning($"Unknown annotator {annotator}");
                    break;
            }

            // Enable the new annotator
            newAnnotator.enabled = true;

            // If can locate, then locate (but don't wait for it to complete)
            if (newAnnotator.CanLocate)
            {
                var t = newAnnotator.StartLocatingAsync();
            }
        }

        /// <summary>
        /// Handles the specified input action, switching to the proper annotator.
        /// </summary>
        /// <param name="action">
        /// The action to handle.
        /// </param>
        private void HandleInputAction(MixedRealityInputAction action)
        {
            if (action == asaAction)
            {
                ActivateAnnotator(AnnotatorType.AzureSpatialAnchor);
            }
            else if (action == arrAction)
            {
                ActivateAnnotator(AnnotatorType.AzureRemoteRender);
            }
            else if (action == aoaAction)
            {
                ActivateAnnotator(AnnotatorType.AzureObjectAnchor);
            }
        }

        /// <summary>
        /// Loads annotation data for each annotator from storage. If data isn't found, an empty
        /// data set will be crated.
        /// </summary>
        private void LoadOrCreateData()
        {
            // Attempt to load from storage
            AppData = DataStore.LoadObject<AnnotationAppData>("Annotations");

            // If no data was loaded from storage, create a new data set
            if ((appData == null) || (appData.AnnotatedObjects == null))
            {
                // Create default app data
                appData = new AnnotationAppData();
            }

            // Make sure we have data for each type of annotator and try to locate it
            foreach (AnnotatorType objectType in Enum.GetValues(typeof(AnnotatorType)))
            {
                // See if there's already data for the locator
                AnnotatedObjectData data = appData.AnnotatedObjects.Where(o => o.Annotator == objectType).FirstOrDefault();

                // If not, create and add data for this locator
                if (data == null)
                {
                    data = new AnnotatedObjectData() { Annotator = objectType };
                    appData.AnnotatedObjects.Add(data);
                }

                // Now, get the annotator for the type of data
                AnnotatorBase annotator = null;
                switch (objectType)
                {
                    // It's an azure spatial anchor
                    case AnnotatorType.AzureSpatialAnchor:
                        annotator = asaAnnotator;
                        break;

                    // It's an azure spatial anchor
                    case AnnotatorType.AzureRemoteRender:
                        annotator = arrAnnotator;
                        break;

                    // It's an azure object anchor
                    case AnnotatorType.AzureObjectAnchor:
                        annotator = aoaAnnotator;
                        break;

                    default:
                        Debug.LogError($"{nameof(AnnotationManager)}: Unknown object type '{objectType}'.");
                        continue;
                }

                // Send the data to the annotator
                annotator.ObjectData = data;

                // If possible to locate the annotator, start locating
                if ((annotator.enabled) && (annotator.CanLocate))
                {
                    // But don't wait for it to complete here
                    var t = annotator.StartLocatingAsync();
                }
            }

            this.LogRaw("READY! Say 'Spatial Anchor Mode', 'Remote Render Mode' or 'Object Anchor Mode' to begin.");
        }

        /// <summary>
        /// Saves annotation data to storage.
        /// </summary>
        private void SaveData()
        {
            // Make sure we have data
            if (appData != null)
            {
                DataStore.SaveObject(appData, "Annotations");
            }
        }

        /// <summary>
        /// Subscribe to events from the annotators.
        /// </summary>
        private void SubscribeEvents()
        {
            asaAnnotator.AnnotationAdded += Annotator_AnnotationAdded;
            arrAnnotator.AnnotationAdded += Annotator_AnnotationAdded;
            aoaAnnotator.AnnotationAdded += Annotator_AnnotationAdded;
        }

        /// <summary>
        /// Unsubscribe from events from the annotators.
        /// </summary>
        private void UnsubscribeEvents()
        {
            asaAnnotator.AnnotationAdded -= Annotator_AnnotationAdded;
            arrAnnotator.AnnotationAdded -= Annotator_AnnotationAdded;
            aoaAnnotator.AnnotationAdded -= Annotator_AnnotationAdded;
        }
        #endregion // Internal Methods

        #region Overrides / Event Handlers
        /// <summary>
        /// Called whenever an annotator adds an annotation.
        /// </summary>
        private void Annotator_AnnotationAdded(object sender, System.EventArgs e)
        {
            // Every time an annotation is added, save the data to storage
            SaveData();
        }

        protected override void RegisterHandlers()
        {
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
        }

        protected override void UnregisterHandlers()
        {
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
        }
        #endregion // Overrides / Event Handlers

        #region IMixedRealityInputActionHandler
        void IMixedRealityInputActionHandler.OnActionStarted(BaseInputEventData eventData)
        {
            HandleInputAction(eventData.MixedRealityInputAction);
        }

        void IMixedRealityInputActionHandler.OnActionEnded(BaseInputEventData eventData)
        {

        }
        #endregion // IMixedRealityInputActionHandler

        #region Unity Overrides
        protected override void OnEnable()
        {
            base.OnEnable();
            SubscribeEvents();
        }

        protected override void OnDisable()
        {
            UnsubscribeEvents();
            base.OnDisable();
        }

        protected virtual void Update()
        {
            // HACK: Wait until the first frame to load data
            // This gives AR Foundation time to start
            if (!dataLoaded)
            {
                dataLoaded = true;
                LoadOrCreateData();
            }
        }
        #endregion // Unity Overrides

        #region Public Properties
        /// <summary>
        /// Gets or sets the app data that should be visualized.
        /// </summary>
        public AnnotationAppData AppData { get => appData; set => appData = value; }

        /// <summary>
        /// Gets or sets the annotator for AOA.
        /// </summary>
        public AOAAnnotator AOAAnnotator { get => aoaAnnotator; set => aoaAnnotator = value; }

        /// <summary>
        /// Gets or sets the annotator for ASA.
        /// </summary>
        public ASAAnnotator ASAAnnotator { get => asaAnnotator; set => asaAnnotator = value; }

        /// <summary>
        /// Gets or sets the annotator for ARR.
        /// </summary>
        public ARRAnnotator ARRAnnotator { get => arrAnnotator; set => arrAnnotator = value; }
        #endregion // Public Properties
    }
}