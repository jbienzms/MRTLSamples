using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaskGuidance
{
    /// <summary>
    /// A helper class for creating annotations.
    /// </summary>
    static public class AnnotationHelper
    {
        #region Member Variables
        static private int created = 0;
        #endregion // Member Variables

        #region Public Methods
        /// <summary>
        /// Creates a visual representation of an annotation.
        /// </summary>
        /// <param name="prefab">
        /// The prefab that will be used to represent the annotation. This prefab must include <see cref="ToolTip"/> and <see cref="ToolTipConnector"/>.
        /// </param>
        /// <param name="annotationParent">
        /// The container where the annotation will be created.
        /// </param>
        /// <param name="targetParent">
        /// The container where the target will be created.
        /// </param>
        /// <param name="text">
        /// The text of the prefab.
        /// </param>
        /// <param name="offset">
        /// The local offset within <paramref name="targetParent"/> where the target will be placed.
        /// </param>
        /// <returns>
        /// The created <see cref="GameObject"/> that represents the annotation.
        /// </returns>
        static private GameObject Visualize(GameObject prefab, Transform annotationParent, Transform targetParent, string text, Vector3 offset)
        {
            // Validate parameters
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            if (annotationParent == null) throw new ArgumentNullException(nameof(annotationParent));
            if (targetParent == null) throw new ArgumentNullException(nameof(targetParent));

            // Create a suffix for the annotation and target game objects
            string suffix = (created++).ToString();

            // Create an empty game object where the target of the tooltip should be
            GameObject target = new GameObject();

            // Give the target a name
            target.name = $"Target-{suffix}";

            // Move the target to where it should be relative to its parent
            target.transform.localPosition = targetParent.position + offset;

            // Make it a child of the parent
            target.transform.SetParent(targetParent, worldPositionStays: true);

            // Create an instance of the annotation prefab in the annotation container
            GameObject annot = GameObject.Instantiate(prefab, annotationParent);

            // Give it a name
            annot.name = $"Annotation-{suffix}";

            // Find components
            ToolTip toolTip = annot.GetComponentInChildren<ToolTip>();
            ToolTipConnector connector = annot.GetComponentInChildren<ToolTipConnector>();
            if (toolTip == null) { throw new InvalidOperationException($"{nameof(prefab)} doesn't incldue a {nameof(ToolTip)}."); }
            if (connector == null) { throw new InvalidOperationException($"{nameof(prefab)} doesn't incldue a {nameof(ToolTipConnector)}."); }

            // Configure the tooltip
            toolTip.ToolTipText = text;

            // Configure the tooltip connector
            connector.ConnectorFollowingType = ConnectorFollowType.PositionAndYRotation;
            connector.Target = target;


            // Return the newly created annotation
            return annot;
        }

        /// <summary>
        /// Creates a visual representation of the specified annotation.
        /// </summary>
        /// <param name="prefab">
        /// The prefab that will be used to represent the annotation. This prefab must include <see cref="ToolTip"/> and <see cref="ToolTipConnector"/>.
        /// </param>
        /// <param name="annotationParent">
        /// The container where the annotation will be created.
        /// </param>
        /// <param name="targetParent">
        /// The container where the target will be created.
        /// </param>
        /// <param name="annotation">
        /// The annotation to represent with a visual.
        /// </param>
        /// <returns>
        /// The created <see cref="GameObject"/> that represents the annotation.
        /// </returns>
        static public GameObject Visualize(GameObject prefab, Transform annotationParent, Transform targetParent, AnnotationData annotation)
        {
            // Validate parameters
            if (prefab == null) throw new ArgumentNullException(nameof(annotation));

            // Use other version
            return Visualize(prefab, annotationParent, targetParent, annotation.Text, annotation.Offset);
        }
        #endregion // Public Methods
    }
}