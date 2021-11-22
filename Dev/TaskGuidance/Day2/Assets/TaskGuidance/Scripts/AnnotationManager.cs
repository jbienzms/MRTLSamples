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

        [Tooltip("The container where annotations will be created.")]
        [SerializeField]
        private Transform annotationContainer;

        [Tooltip("The prefab that will be used to represent an annotation.")]
        [SerializeField]
        private GameObject annotationPrefab;

        [Tooltip("The annotation data that should be visualized.")]
        [SerializeField]
        private AnnotationData annotations;
        #endregion // Member Variables

        #region Member Variables
        /// <summary>
        /// Visualizes all of the annotations stored in the <see cref="Annotations"/> property.
        /// </summary>
        private void VisualizeAnnotations()
        {
            // Make sure we have data
            if ((annotations == null) || (annotations.AnnotatedObjects == null) || (annotations.AnnotatedObjects.Count < 1))
            {
                Debug.LogWarning($"{nameof(AnnotationManager)}: No annotations to visualize.");
                return;
            }

            // Visualize
            foreach (var ao in annotations.AnnotatedObjects)
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
        /// <param name="parent">
        /// The parent of the annotations.
        /// </param>
        private void VisualizeAnnotations(List<Annotation> annotations, Transform parent)
        {
            // Keep track of how many annotaitons we've visualized on this parent
            int annotationCount=0;

            // Render each annotation
            foreach (var annotation in annotations)
            {
                // Increment the annotation count
                annotationCount++;

                // Create an empty game object where the target of the tooltip should be
                GameObject target = new GameObject();

                // Give the target a name
                target.name = $"Target-{annotationCount}";

                // Make it a child of the parent
                target.transform.SetParent(parent, worldPositionStays: false);

                // Move the target to where it should be relative to its parent
                target.transform.localPosition = annotation.Offset;

                // Create an instance of the annotation prefab in the annotation container
                GameObject prefab = GameObject.Instantiate(annotationPrefab, annotationContainer);

                // Give it a name
                prefab.name = $"Annotation-{annotationCount}";

                // Find the tooltip connector
                ToolTipConnector connector = prefab.GetComponentInChildren<ToolTipConnector>();
                if (connector == null)
                {
                    Debug.LogError($"{nameof(AnnotationManager)}: Annotation prefab doesn't incldue a {nameof(ToolTipConnector)}.");
                    return;
                }

                // Configure the tooltip connector
                connector.ConnectorFollowingType = ConnectorFollowType.PositionAndYRotation;
                connector.Target = target;
            }
        }

        /// <summary>
        /// Visualizes all of the annotations on an Azure Spatial Anchor.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="AnnotatedObject"/> that represents the Azure Spatial Anchor.
        /// </param>
        private void VisualizeASA(AnnotatedObject obj)
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
        /// Gets or sets the container where annotations will be created.
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
        /// Gets or sets the annotation data that should be visualized.
        /// </summary>
        public AnnotationData Annotations { get => annotations; set => annotations = value; }
        #endregion // Public Properties
    }
}