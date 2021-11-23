using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaskGuidance
{
    /// <summary>
    /// Handles saving and loading annotations as well as interacting with annotators.
    /// </summary>
    public class AnnotationManager : MonoBehaviour
    {
        #region Member Variables
        [Tooltip("The app data that should be visualized.")]
        [SerializeField]
        private AnnotationAppData appData;

        [Tooltip("The annotator for ASA.")]
        [SerializeField]
        private ASAAnnotator asaAnnotator;
        #endregion // Member Variables

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
                        // Add one for ASA
                        new AnnotatedObjectData() { ObjectType = AnnotatedObjectType.AzureSpatialAnchor}
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

                    default:
                        Debug.LogError($"{nameof(AnnotationManager)}: Unknown object type '{annotatedObject.ObjectType}'.");
                        continue;
                }

                // Set the data
                annotator.ObjectData = annotatedObject;

                // Attempt to load and visualize (it's OK if it fails)
                var t = annotator.TryLoadAndVisualizeAsync();
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
            LoadData();
        }

        protected virtual void OnEnable()
        {
            SubscribeEvents();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeEvents();
        }
        #endregion // Unity Overrides

        #region Public Properties
        /// <summary>
        /// Gets or sets the app data that should be visualized.
        /// </summary>
        public AnnotationAppData AppData { get => appData; set => appData = value; }

        /// <summary>
        /// Gets or sets the annotator for ASA.
        /// </summary>
        public ASAAnnotator ASAAnnotator { get => asaAnnotator; set => asaAnnotator = value; }
        #endregion // Public Properties
    }
}