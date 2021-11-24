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
    /// The state of the annotator loading.
    /// </summary>
    public enum AnnotatorLoadState
    {
        /// <summary>
        /// The annotator has not loaded the object to annotate.
        /// </summary>
        NotLoaded,

        /// <summary>
        /// The annotator is loading the object to annotate.
        /// </summary>
        Loading,

        /// <summary>
        /// The annotator has loaded the object to annotate.
        /// </summary>
        Loaded
    };

    /// <summary>
    /// Base class for a manager that performs annotations.
    /// </summary>
    public abstract class AnnotatorBase : InputSystemGlobalHandlerListener, IMixedRealityPointerHandler
    {
        #region Member Variables
        private bool isLocated;
        private bool isLocating;
        private bool isPlaced;
        private bool isPlacing;
        private GameObject placemarkVisual;
        private bool storedAnnotationsVisualized;
        #endregion // Member Variables

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

        [Tooltip("The prefab that will be used to represent the placement of the annotator.")]
        [SerializeField]
        private GameObject placemarkPrefab;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Creates the visual to represent the placement of the annotator.
        /// </summary>
        protected void InstantiatePlacemark()
        {
            // Is there a prefab?
            if (placemarkPrefab != null)
            {
                // Use the prefab
                PlacemarkVisual = GameObject.Instantiate(placemarkPrefab);
            }
            else
            {
                // Create a small sphere to represent
                PlacemarkVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                PlacemarkVisual.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
        }

        /// <summary>
        /// Notifies subscribers that an annotation has been added.
        /// </summary>
        protected void NotifyAnnotationAdded()
        {
            OnAnnotationAdded();
            AnnotationAdded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Notifies subscribers that the annotator has been located in the physical world.
        /// </summary>
        protected void NotifyLocated()
        {
            isLocating = false;
            isLocated = true;
            OnLocated();
            Located?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Notifies subscribers that the annotator has been placed in the physical world.
        /// </summary>
        protected void NotifyPlaced()
        {
            isPlacing = false;
            isPlaced = true;
            OnPlaced();
            Placed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Saves the placement of the annotator.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        protected abstract Task SavePlacementAsync();

        /// <summary>
        /// Starts locating the placement of the annotator.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        protected abstract Task StartLocatingPlacementAsync();

        /// <summary>
        /// Attempts to place the annotator or add an annotation at the specified focus point.
        /// </summary>
        /// <param name="focus">
        /// The focus point where the annotator or annotation should be placed.
        /// </param>
        private void TryPlaceOrAnnotate(FocusDetails focus)
        {
            // Try to place first
            if (CanPlace)
            {
                // Kick off the process to place, but don't wait for it to complete here
                var t = PlaceAsync(focus);
            }
            // Next, try to annotate
            else if (CanAnnotate)
            {
                AddAnnotation(focus);
            }
        }

        /// <summary>
        /// Visualzies all of the annotations stored in <see cref="ObjectData"/>.
        /// </summary>
        protected virtual void VisualizeStoredAnnotations()
        {
            // Validate
            if (objectData == null) { throw new InvalidOperationException($"Attempted to call {nameof(VisualizeStoredAnnotations)} but no object data."); }
            if ((!IsPlaced) && (!IsLocated)) { throw new InvalidOperationException($"Attempted to call {nameof(VisualizeStoredAnnotations)} but annotator has not been placed or located."); }

            // If this has already been done, don't do it again
            if (storedAnnotationsVisualized) { return; }

            // Render each annotation
            foreach (var annData in objectData.Annotations)
            {
                // Visualize the annotation data
                AnnotationVisualizer.Visualize(annotationPrefab, annotationContainer, placemarkVisual.transform, annData);
            }

            // Stored annotations have been visualized
            storedAnnotationsVisualized = true;
        }
        #endregion // Internal Methods

        #region Overridables / Event Triggers
        /// <summary>
        /// Called when an annotation has been added.
        /// </summary>
        protected virtual void OnAnnotationAdded() {}

        /// <summary>
        /// Called when the annotator has been located in the physical world.
        /// </summary>
        protected virtual void OnLocated()
        {
            // If we haven't visualizeds stored annotations yet, visualize them now
            if (!storedAnnotationsVisualized) { VisualizeStoredAnnotations(); }
        }

        /// <summary>
        /// Called when the annotator has been placed in the physical world.
        /// </summary>
        protected virtual void OnPlaced()
        {
            // If we haven't visualizeds stored annotations yet, visualize them now
            if (!storedAnnotationsVisualized) { VisualizeStoredAnnotations(); }
        }
        #endregion // Overridables / Event Triggers

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
            // Make sure we have focus details
            if (eventData?.Pointer?.Result?.Details == null) { return; }

            // Get the details of the pointer click
            FocusDetails focus = eventData.Pointer.Result.Details;

            // If there is no focused object under the pointer (including the surface mesh) then ignore
            if (focus.Object == null) { return; }

            // Attempt to place or annotate
            TryPlaceOrAnnotate(focus);
        }
        #endregion // IMixedRealityPointerHandler Members

        #region Public Methods
        /// <summary>
        /// Attempts to add an annotation at the specified position.
        /// </summary>
        /// <param name="focus">
        /// The focus area where the object should be placed.
        /// </param>
        public virtual void AddAnnotation(FocusDetails focus)
        {
            // Makes sure we can annotate
            if (!CanAnnotate) { throw new InvalidOperationException($"{this.name}: {nameof(AddAnnotation)} called but {nameof(CanAnnotate)} is false."); }

            // Calculate the world rotation of the nomral
            Quaternion focusRot = Quaternion.LookRotation(focus.Normal);

            // Create the annotation data
            AnnotationData annData = new AnnotationData()
            {
                // Just basic text for now
                Text = "Annotation",

                // The focus point relative to the visual gives us the offset
                Offset = placemarkVisual.transform.InverseTransformPoint(focus.Point),

                // Calculate the offset from visual forward to the focus forward
                Direction = Quaternion.Inverse(placemarkVisual.transform.rotation) * focusRot
            };

            // Add the annoation data to the annotated object data
            ObjectData.Annotations.Add(annData);

            // Visualize the annotation data
            AnnotationVisualizer.Visualize(annotationPrefab, annotationContainer, placemarkVisual.transform, annData);

            // Notify
            NotifyAnnotationAdded();
        }

        /// <summary>
        /// Attempts to place the visual to be annotated at the specified focus point.
        /// </summary>
        /// <param name="focus">
        /// The focus point where the visual should be placed.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public async Task PlaceAsync(FocusDetails focus)
        {
            // Makes sure we can place
            if (!CanPlace) { throw new InvalidOperationException($"{this.name}: {nameof(PlaceAsync)} called but {nameof(CanPlace)} is false."); }

            // Placing
            isPlacing = true;

            // Make sure any errors during placing or saving don't keep the isPlacing flag stuck on
            try
            {
                // If we don't have a visual yet, create it
                if (placemarkVisual == null) { InstantiatePlacemark(); }

                // Move the visual to where the user tapped and orientate it correctly
                placemarkVisual.transform.position = focus.Point;
                placemarkVisual.transform.rotation = Quaternion.LookRotation(focus.Normal, Vector3.up);

                // Log
                this.Log($"Saving placement...");

                // Save the placement
                await SavePlacementAsync();

                // Notify that we've been placed
                NotifyPlaced();
            }
            catch (Exception ex)
            {
                // Log
                this.LogError($"Error saving placement, {ex.Message}.");

                // Rethrow
                throw;
            }
            finally
            {
                // No longer placing
                isPlacing = false;
            }
        }

        /// <summary>
        /// Starts the process of locating the annotator in the real world.
        /// </summary>
        /// <returns>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public Task StartLocatingAsync()
        {
            // Makes sure we can locate
            if (!CanLocate) { throw new InvalidOperationException($"{this.name}: {nameof(StartLocatingAsync)} called but {nameof(CanLocate)} is false."); }

            // Locating
            isLocating = true;

            // Make sure any errors starting locating don't keep the isLocating flag stuck on
            try
            {
                // Log
                this.Log($"Starting to locate.");

                // Start locating
                return StartLocatingAsync();
            }
            catch (Exception ex)
            {
                // Log
                this.LogError($"Error starting to locate, {ex.Message}.");

                // Rethrow
                throw;
            }
            finally
            {
                // No longer locating
                isLocating = false;
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
        /// Gets a value that indicates if annotations can be added to the visual.
        /// </summary>
        /// <remarks>
        /// Generally annotations can be added as long as the visual has been placed or located.
        /// </remarks>
        public virtual bool CanAnnotate
        {
            get
            {
                return ((IsPlaced) || (IsLocated));
            }
        }

        /// <summary>
        /// Gets a value that indicates if the visual to be annotated can be located.
        /// </summary>
        /// <remarks>
        /// Usually the visual can be loacted if it hasn't already been located and if the
        /// <see cref="ObjectData"/> has a valid <see cref="AnnotatedObjectData.Id">Id</see>.
        /// For example, in ASA ObjectData.ID is the ID of the anchor that was previously
        /// placed.
        /// </remarks>
        public virtual bool CanLocate
        {
            get
            {
                // Only if we're not already located or locating and we have a unique ID
                return ((!IsLocated) && (!IsLocating) && (!string.IsNullOrEmpty(objectData?.Id)));
            }
        }

        /// <summary>
        /// Gets a value that indicates if the annotator can be placed.
        /// </summary>
        /// <remarks>
        /// By default the annotator can only be placed once and can't be placed again once it's
        /// been located.
        /// </remarks>
        public virtual bool CanPlace
        {
            get
            {
                // Only if we haven't already been placed, aren't being placed, and haven't been located
                return ((!IsPlaced) && (!IsPlacing) && (!IsLocated));
            }
        }

        /// <summary>
        /// Gets a value that indicates if the annotator has been located in the real world.
        /// </summary>
        /// <remarks>
        /// With ASA, for example, this would indicate that the ASA anchor has been located.
        /// </remarks>
        public bool IsLocated => isLocated;

        /// <summary>
        /// Gets a value that indicates if the annotator is currently being located.
        /// </summary>
        public bool IsLocating => isLocating;

        /// <summary>
        /// Gets a value that indicates if the annotator is currently being placed.
        /// </summary>
        public bool IsPlacing => isPlacing;

        /// <summary>
        /// Gets a value that indicates if the annotator has been placed.
        /// </summary>
        public bool IsPlaced => isPlaced;

        /// <summary>
        /// Gets or sets the data about the object where annotations will be stored on disk.
        /// </summary>
        public AnnotatedObjectData ObjectData { get => objectData; set => objectData = value; }

        /// <summary>
        /// Gets or sets the visual representing the placement of the annotator.
        /// </summary>
        public GameObject PlacemarkVisual { get => placemarkVisual; set => placemarkVisual = value; }
        #endregion // Public Properties

        #region Public Events
        /// <summary>
        /// Raised when an annotation has been added.
        /// </summary>
        public event EventHandler AnnotationAdded;

        /// <summary>
        /// Raised when the annotator has been located in the physical world.
        /// </summary>
        public event EventHandler Located;

        /// <summary>
        /// Raised when the annotator has been placed in the physical world.
        /// </summary>
        public event EventHandler Placed;
        #endregion // Public Events
    }
}