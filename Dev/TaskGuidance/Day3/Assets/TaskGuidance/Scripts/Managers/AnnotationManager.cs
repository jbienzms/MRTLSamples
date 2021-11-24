using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TaskGuidance
{
    /// <summary>
    /// Handles saving and loading annotations as well as interacting with annotators.
    /// </summary>
    public class AnnotationManager : MonoBehaviour
    {
        #region Member Variables
        private bool dataLoaded;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [Tooltip("The app data that should be visualized.")]
        [SerializeField]
        private AnnotationAppData appData;

        [Tooltip("The annotator for ARR.")]
        [SerializeField]
        private ARRAnnotator arrAnnotator;

        [Tooltip("The annotator for ASA.")]
        [SerializeField]
        private ASAAnnotator asaAnnotator;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Loads annotation data from storage.
        /// </summary>
        private void LoadData()
        {
            // Attempt to load from storage
            AppData = DataStore.LoadObject<AnnotationAppData>("Annotations");

            // Make sure we have data
            if ((appData == null) || (appData.AnnotatedObjects == null) || (appData.AnnotatedObjects.Count < 1))
            {
                Debug.LogWarning($"{nameof(AnnotationManager)}: No annotations loaded. Creating default data.");
                // Create default app data
                appData = new AnnotationAppData()
                {
                    // Create a list of annotated objects
                    AnnotatedObjects = new List<AnnotatedObjectData>()
                    {
                        // Add one for each annotator type
                        new AnnotatedObjectData() { ObjectType = AnnotatedObjectType.AzureSpatialAnchor},
                        new AnnotatedObjectData() { ObjectType = AnnotatedObjectType.AzureRemoteRender}
                    }
                };
            }

            // Visualize
            foreach (var annotatedObject in appData.AnnotatedObjects)
            {
                // Which annotator should we use?
                AnnotatorBase annotator = null;

                // Get the annotator for the type of object
                switch (annotatedObject.ObjectType)
                {
                    // It's an azure spatial anchor
                    case AnnotatedObjectType.AzureSpatialAnchor:
                        annotator = asaAnnotator;
                        break;

                    // It's an azure spatial anchor
                    case AnnotatedObjectType.AzureRemoteRender:
                        annotator = arrAnnotator;
                        break;

                    default:
                        Debug.LogError($"{nameof(AnnotationManager)}: Unknown object type '{annotatedObject.ObjectType}'.");
                        continue;
                }

                // Set the data
                annotator.ObjectData = annotatedObject;

                // If possible to locate, start locating
                if ((annotator.enabled) && (annotator.CanLocate))
                {
                    // Start locating, but don't wait for it to complete here
                    var t = annotator.StartLocatingAsync();
                }
            }
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
        }

        /// <summary>
        /// Unsubscribe from events from the annotators.
        /// </summary>
        private void UnsubscribeEvents()
        {
            asaAnnotator.AnnotationAdded -= Annotator_AnnotationAdded;
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
        #endregion // Overrides / Event Handlers

        #region Unity Overrides
        // Start is called before the first frame update
        protected virtual void Start()
        {

        }

        protected virtual void OnEnable()
        {
            SubscribeEvents();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeEvents();
        }

        protected virtual void Update()
        {
            // HACK: Wait until the first frame to load data
            // This gives AR Foundation time to start
            if (!dataLoaded)
            {
                dataLoaded = true;
                LoadData();
            }
        }
        #endregion // Unity Overrides

        #region Public Properties
        /// <summary>
        /// Gets or sets the app data that should be visualized.
        /// </summary>
        public AnnotationAppData AppData { get => appData; set => appData = value; }

        /// <summary>
        /// Gets or sets the annotator for ARR.
        /// </summary>
        public ARRAnnotator ARRAnnotator { get => arrAnnotator; set => arrAnnotator = value; }

        /// <summary>
        /// Gets or sets the annotator for ASA.
        /// </summary>
        public ASAAnnotator ASAAnnotator { get => asaAnnotator; set => asaAnnotator = value; }
        #endregion // Public Properties
    }
}