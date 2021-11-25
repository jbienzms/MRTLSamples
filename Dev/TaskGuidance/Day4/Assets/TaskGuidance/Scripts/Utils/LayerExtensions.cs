using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Helpers for working with layers.
/// </summary>
static public class LayerExtensions
{
    /// <summary>
    /// Determines if the specified layer is found in the layer mask.
    /// </summary>
    /// <param name="mask">
    /// The <see cref="LayerMask"/> to search.
    /// </param>
    /// <param name="layer">
    /// The layer to search for in the mask.
    /// </param>
    /// <returns>
    /// <c>true</c> if the layer is in the mask; otherwise <c>false</c>.
    /// </returns>
    static public bool Contains(this LayerMask mask, int layer)
    {
        return ((mask.value & (1 << layer)) != 0);
    }

    /// <summary>
    /// Gets a <see cref="LayerMask"/> that contains all of the existing named layers.
    /// </summary>
    /// <param name="layerNames">
    /// The names of the layers to find.
    /// </param>
    /// <returns>
    /// An array with the names of only the found layers.
    /// </returns>
    static public string[] ExistingLayers(params string[] layerNames)
    {
        // Place to keep all found layers
        List<string> foundLayers = new List<string>();

        // Check each one
        foreach (string layerName in layerNames)
        {
            if (LayerExists(layerName))
            {
                foundLayers.Add(layerName);
            }
        }

        // Return only found
        return foundLayers.ToArray();
    }

    /// <summary>
    /// Gets a <see cref="LayerMask"/> that contains all of the existing named layers.
    /// </summary>
    /// <param name="layerNames">
    /// The names of the layers to find.
    /// </param>
    /// <returns>
    /// A <see cref="LayerMask"/> with only the found layers.
    /// </returns>
    static public LayerMask ExistingLayersMask(params string[] layerNames)
    {
        // Narrow to only existing
        string[] existing = ExistingLayers(layerNames);

        // Make sure at least one is found
        if (existing.Length < 1) { return 0; }

        // Return the mask
        return LayerMask.GetMask(existing);
    }

    /// <summary>
    /// Returns the named layer if found; otherwise returns the default.
    /// </summary>
    /// <param name="layerName">
    /// The name of the layer to use if found.
    /// </param>
    /// <param name="defaultLayer">
    /// The layer to use if not found.
    /// </param>
    /// <returns>
    /// The layer.
    /// </returns>
    static public int ExistingOrDefault(string layerName, int defaultLayer)
    {
        // Try to get the named layer
        int namedLayer = LayerMask.NameToLayer(layerName);

        // Which to use?
        return (namedLayer > -1 ? namedLayer : defaultLayer);
    }

    /// <summary>
    /// Determines if the named layer exists.
    /// </summary>
    /// <param name="layerName">
    /// The name of the layer to check.
    /// </param>
    /// <returns>
    /// <c>true</c> if the named layer exists; otherwise <c>false</c>.
    /// </returns>
    static public bool LayerExists(string layerName)
    {
        return (LayerMask.NameToLayer(layerName) > -1);
    }
}