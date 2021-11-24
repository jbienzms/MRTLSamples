using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Physics;
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
            // Make sure we have focus details
            if (eventData?.Pointer?.Result?.Details == null) { return; }

            // Get the details of the pointer click
            FocusDetails focus = eventData.Pointer.Result.Details;

            // If there is no focused object under the pointer (including the surface mesh) then ignore
            if (focus.Object == null) { return; }

            // Is our visual placed (or located)?
            if (!IsVisualPlaced)
            {
                // No. Try and place it.
                await TryPlaceVisualAsync(focus);

                // If placed, visualize
                if (IsVisualPlaced)
                {
                    Visualize();
                }
            }
            else
            {
                // Yes, already placed. Try to add another annotation.
                if (TryAddAnnotation(focus))
                {
                    // Notify that an annotation was added.
                    AnnotationAdded?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        #endregion // IMixedRealityPointerHandler Members

        #region Public Methods
        /// <summary>
        /// Attempts to add an annotation at the specified position.
        /// </summary>
        /// <param name="focus">
        /// The focus area where the object should be placed.
        /// </param>
        public virtual bool TryAddAnnotation(FocusDetails focus)
        {
            // Can't add an annotation if the object isn't placed
            if (!IsVisualPlaced) { return false; }

            // Calculate the world rotation of the nomral
            Quaternion focusRot = Quaternion.LookRotation(focus.Normal);

            // Create the annotation data
            AnnotationData annData = new AnnotationData()
            {
                // Just basic text for now
                Text = "Annotation",

                // The focus point relative to the visual gives us the offset
                Offset = objectVisual.transform.InverseTransformPoint(focus.Point),

                // Calculate the offset from visual forward to the focus forward
                Direction = Quaternion.Inverse(objectVisual.transform.rotation) * focusRot
            };

            // Add the annoation data to the annotated object
            ObjectData.Annotations.Add(annData);

            // Visualize the annotation data
            AnnotationVisualizer.Visualize(annotationPrefab, annotationContainer, objectVisual.transform, annData);

            // Success
            return true;
        }

        /// <summary>
        /// Attempts to load the annotated object into the scene.
        /// </summary>
        /// <returns>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public virtual Task<bool> TryLoadAsync()
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
        /// Attempts to place the object at the specified position.
        /// </summary>
        /// <param name="focus">
        /// The focus area where the object should be placed.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public virtual Task<bool> TryPlaceVisualAsync(FocusDetails focus)
        {
            return Task.FromResult(false);
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
                AnnotationVisualizer.Visualize(annotationPrefab, annotationContainer, objectVisual.transform, annData);
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

        #region Public Events
        /// <summary>
        /// Raised when an annotation has been added.
        /// </summary>
        public event EventHandler AnnotationAdded;
        #endregion // Public Events
    }
}