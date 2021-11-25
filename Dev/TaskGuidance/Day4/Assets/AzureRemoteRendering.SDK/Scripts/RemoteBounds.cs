// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Azure.RemoteRendering;
using Microsoft.Azure.RemoteRendering.Unity;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class RemoteBounds : MonoBehaviour
{
    #region Member Variables
    private BoxCollider boxCollider;
    private Entity entity;
    #endregion // Member Variables

    #region Internal Methods
    /// <summary>
    /// Gets the remote bounds and applies them locally.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> that represents the operation.
    /// </returns>
    private async Task ApplyBoundsAsync()
    {
        // If we don't have the entity, try and get it
        if (entity == null)
        {
            RemoteEntitySyncObject syncObject = GetComponent<RemoteEntitySyncObject>();
            if (syncObject != null) { entity = syncObject.Entity; }
        }

        // If we still don't have the entity, nothing to do
        if (entity == null) { return; }

        // Get remote bounds
        var remoteBounds = await entity.QueryLocalBoundsAsync();

        // Convert to unity
        var unityBounds = remoteBounds.toUnity();

        // Update the bounding box
        var localBounds = BoundsBoxCollider.bounds;
        localBounds.center = unityBounds.center;
        localBounds.min = unityBounds.min;
        localBounds.max = unityBounds.max;
    }
    #endregion // Internal Methods

    #region Unity Overrides
    protected virtual void Start()
    {
        // Attempt to apply the bounds, don't worry if it fails
        var t = ApplyBoundsAsync();
    }
    #endregion // Unity Overrides

    #region Public Properties
    /// <summary>
    /// Gets the BoxCollider that represents the bounds.
    /// </summary>
    public BoxCollider BoundsBoxCollider
    {
        get
        {
            if (boxCollider == null)
            {
                boxCollider = GetComponent<BoxCollider>();
                if (boxCollider == null)
                {
                    boxCollider = this.gameObject.AddComponent<BoxCollider>();
                }
            }
            return boxCollider;
        }
    }
    #endregion // Public Properties
}
