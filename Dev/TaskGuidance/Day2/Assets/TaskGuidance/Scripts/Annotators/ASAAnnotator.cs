using Microsoft.MixedReality.Toolkit.Physics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TaskGuidance
{
    /// <summary>
    /// An annotator that creates annotations around an Azure Spatial Anchor.
    /// </summary>
    public class ASAAnnotator : AnnotatorBase
    {
        #region Unity Inspector Variables
        [Tooltip("The prefab that will be used to represent the center of the ASA Anchor.")]
        [SerializeField]
        private GameObject anchorPrefab;
        #endregion // Unity Inspector Variables

        #region Overrides / Event Handlers
        protected override Task<bool> TryPlaceVisualAsync(FocusDetails focus)
        {
            // Create the visual to represent the center of the anchor
            if (anchorPrefab != null)
            {
                // Use the prefab
                ObjectVisual = GameObject.Instantiate(anchorPrefab);
            }
            else
            {
                // Create a small sphere to represent
                ObjectVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ObjectVisual.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }

            // Move it to the focus point and orientation
            ObjectVisual.transform.position = focus.Point;
            ObjectVisual.transform.rotation = Quaternion.LookRotation(focus.Normal * -1f, Vector3.up);

            // It's now placed
            IsVisualPlaced = true;
            return Task.FromResult(true);
        }
        #endregion // Overrides / Event Handlers
    }
}