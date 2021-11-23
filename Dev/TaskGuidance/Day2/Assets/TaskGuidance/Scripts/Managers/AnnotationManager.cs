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

        #region Member Variables
        /// <summary>
        /// Visualizes all of the annotations stored in the <see cref="AppData"/> property.
        /// </summary>
        private void LoadData()
        {
            // Make sure we have data
            if ((appData == null) || (appData.AnnotatedObjects == null) || (appData.AnnotatedObjects.Count < 1))
            {
                Debug.LogWarning($"{nameof(AnnotationManager)}: No annotations to visualize.");
                return;
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
        #endregion // Member Variables

        #region Unity Overrides
        // Start is called before the first frame update
        protected virtual void Start()
        {
            LoadData();
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