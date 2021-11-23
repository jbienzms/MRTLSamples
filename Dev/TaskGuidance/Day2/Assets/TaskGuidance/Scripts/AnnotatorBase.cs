using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TaskGuidance
{
    /// <summary>
    /// Base class for a manager that performs annotations.
    /// </summary>
    public class AnnotatorBase : InputSystemGlobalHandlerListener, IMixedRealityPointerHandler
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

        #region Overrides / Event Handlers
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
                Offset = position - objectVisual.transform.position
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
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        protected virtual Task<bool> TryPlaceVisualAsync(Vector3 position)
        {
            return Task.FromResult(false);
        }
        #endregion // Overrides / Event Handlers

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

        async void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            // See if we have a point where the click happened
            Vector3? point = eventData?.Pointer?.Result?.Details.Point;

            // If we have a point, try to place or add an annotation
            if (point != null)
            {
                // Is our visual placed (or located)?
                if (!IsVisualPlaced)
                {
                    // No. Try and place it.
                    await TryPlaceVisualAsync(point.Value);

                    // If placed, visualize
                    if (IsVisualPlaced)
                    {
                        Visualize();
                    }
                }
                else
                {
                    // Yes, already placed. Just add another annotation.
                    TryAddAnnotation(point.Value);
                }
            }
        }
        #endregion // IMixedRealityPointerHandler Members


        #region Public Methods
        /// <summary>
        /// Attempts to load the annotated object into the scene.
        /// </summary>
        /// <returns>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public Task<bool> TryLoadAsync()
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Attempts to load the annotated object into the scene.
        /// </summary>
        /// <returns>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public async Task<bool> TryLoadAndVisualizeAsync()
        {
            // Wait for load
            bool loaded = await TryLoadAsync();

            // If not loaded, nothing else to do
            if (!loaded) { return false; }

            // Visualize
            Visualize();

            // Done!
            return true;
        }

        /// <summary>
        /// Visualzies all of the annotations for the object.
        /// </summary>
        public virtual void Visualize()
        {
            if (objectData == null) { throw new InvalidOperationException($"Attempted to call {nameof(Visualize)} but no object data."); }
            if (!IsVisualPlaced) { throw new InvalidOperationException($"Attempted to call {nameof(Visualize)} but visual has not been placed."); }

            // Render each annotation
            foreach (var annData in objectData.Annotations)
            {
                // Visualize the annotation data
                AnnotationHelper.Visualize(annotationPrefab, annotationContainer, objectVisual.transform, annData);
            }
        }
        #endregion // Public Methods

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