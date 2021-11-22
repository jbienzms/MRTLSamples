using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaskGuidance
{
    /// <summary>
    /// Handles visualizing all annotation data in the scene.
    /// </summary>
    public class AnnotationManager : MonoBehaviour
    {
        #region Member Variables
        public GameObject FakeRoot;

        [Tooltip("The container where annotation visuals will be created.")]
        [SerializeField]
        private Transform annotationContainer;

        [Tooltip("The prefab that will be used to represent an annotation.")]
        [SerializeField]
        private GameObject annotationPrefab;

        [Tooltip("The app data that should be visualized.")]
        [SerializeField]
        private AnnotationAppData appData;
        #endregion // Member Variables

        #region Member Variables
        /// <summary>
        /// Visualizes all of the annotations stored in the <see cref="AppData"/> property.
        /// </summary>
        private void VisualizeAnnotations()
        {
            // Make sure we have data
            if ((appData == null) || (appData.AnnotatedObjects == null) || (appData.AnnotatedObjects.Count < 1))
            {
                Debug.LogWarning($"{nameof(AnnotationManager)}: No annotations to visualize.");
                return;
            }

            // Visualize
            foreach (var ao in appData.AnnotatedObjects)
            {
                // What type of object
                switch (ao.ObjectType)
                {
                    // It's an azure spatial anchor
                    case AnnotatedObjectType.AzureSpatialAnchor:
                        VisualizeASA(ao);
                        break;
                    default:
                        Debug.LogError($"{nameof(AnnotationManager)}: Unknown object type '{ao.ObjectType}'.");
                        break;
                }
            }
        }

        /// <summary>
        /// Visualizes the annotations relative to the specified parent.
        /// </summary>
        /// <param name="annotations">
        /// The annotations to visualize.
        /// </param>
        /// <param name="targetParent">
        /// The parent of the annotations.
        /// </param>
        private void VisualizeAnnotations(List<AnnotationData> annotations, Transform targetParent)
        {
            // Render each annotation
            foreach (var annData in annotations)
            {
                // Visualize the annotation data
                AnnotationHelper.Visualize(annotationPrefab, annotationContainer, targetParent, annData);
            }
        }

        /// <summary>
        /// Visualizes all of the annotations on an Azure Spatial Anchor.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="AnnotatedObjectData"/> that represents the Azure Spatial Anchor.
        /// </param>
        private void VisualizeASA(AnnotatedObjectData obj)
        {
            // TODO: Load the Azure Spatial Achor

            // Visualize
            VisualizeAnnotations(obj.Annotations, FakeRoot.transform);
        }
        #endregion // Member Variables

        #region Unity Overrides
        // Start is called before the first frame update
        protected virtual void Start()
        {
            VisualizeAnnotations();
        }

        // Update is called once per frame
        protected virtual void Update()
        {

        }
        #endregion // Unity Overrides

        #region Public Properties
        /// <summary>
        /// Gets or sets the parent where annotations will be created.
        /// </summary>
        public Transform AnnotationContainer { get => annotationContainer; set => annotationContainer = value; }

        /// <summary>
        /// Gets or sets the prefab that will be used to represent an annotation.
        /// </summary>
        /// <remarks>
        /// The prefab must contain a <see cref="ToolTipConnector"/>
        /// </remarks>
        public GameObject AnnotationPrefab { get => annotationPrefab; set => annotationPrefab = value; }

        /// <summary>
        /// Gets or sets the app data that should be visualized.
        /// </summary>
        public AnnotationAppData AppData { get => appData; set => appData = value; }
        #endregion // Public Properties
    }
}