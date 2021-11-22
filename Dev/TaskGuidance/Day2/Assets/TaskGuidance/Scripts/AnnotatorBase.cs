using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaskGuidance
{
    /// <summary>
    /// Base class for a manager that performs annotations.
    /// </summary>
    public class AnnotatorBase : BaseInputHandler, IMixedRealityPointerHandler
    {
        #region Unity Inspector Variables
        [Tooltip("The container where annotations visuals will be created in the scene.")]
        [SerializeField]
        private Transform annotationContainer;

        [Tooltip("The prefab that will be used to represent an annotation.")]
        [SerializeField]
        private GameObject annotationPrefab;

        [Tooltip("The data about the object where annotations will be stored on disk.")]
        [SerializeField]
        private AnnotatedObjectData objectData;

        [Tooltip("The visual representation of the object being annotated.")]
        [SerializeField]
        private GameObject objectVisual;
        #endregion // Unity Inspector Variables

        /// <summary>
        /// Attempts to add an annotation at the specified position.
        /// </summary>
        /// <param name="position">
        /// The position where the annotation should be created.
        /// </param>
        protected virtual bool TryAddAnnotation(Vector3 position)
        {
            // Can't add an annotation if the object isn't placed
            if (!IsVisualPlaced) { return false; }

            // Create the annotation data
            AnnotationData annData = new AnnotationData()
            {
                Text = "Annotation",
                Offset = objectVisual.transform.position - position
            };

            // Add the annoation data to the annotated object
            ObjectData.Annotations.Add(annData);

            // Visualize the annotation data
            AnnotationHelper.Visualize(annotationPrefab, annotationContainer, objectVisual.transform, annData);

            // Success
            return true;
        }

        /// <summary>
        /// Attempts to place the object at the specified position.
        /// </summary>
        /// <param name="position">
        /// The position where the object should be placed.
        /// </param>
        protected virtual void TryPlaceObject(Vector3 position)
        {

        }

        #region Overrides / Event Handlers
        protected override void RegisterHandlers()
        {
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
        }

        protected override void UnregisterHandlers()
        {
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
        }
        #endregion // Overrides / Event Handlers


        #region IMixedRealityPointerHandler Members
        void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData)
        {

        }

        void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData)
        {

        }

        void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
        {

        }

        void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            // See if we have a point where the click happened
            Vector3? point = eventData?.Pointer?.Result?.Details.Point;

            // If we have a point, try to place or add an annotation
            if (point != null)
            {
                if (!IsVisualPlaced)
                {
                    TryPlaceObject(point.Value);
                }
                else
                {
                    TryAddAnnotation(point.Value);
                }
            }
        }
        #endregion // IMixedRealityPointerHandler Members

        #region Public Properties
        /// <summary>
        /// Gets or sets the container where annotations visuals will be created in the scene.
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
        /// Gets or sets the data about the object where annotations will be stored on disk.
        /// </summary>
        public AnnotatedObjectData ObjectData { get => objectData; set => objectData = value; }

        /// <summary>
        /// Gets or sets the visual representation of the object being annotated.
        /// </summary>
        public GameObject ObjectVisual { get => objectVisual; set => objectVisual = value; }

        /// <summary>
        /// Gets a value that indicates if the visual representing the object has been placed.
        /// </summary>
        public bool IsVisualPlaced { get; protected set; }
        #endregion // Public Properties
    }
}