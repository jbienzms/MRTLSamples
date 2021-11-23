using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TaskGuidance
{
    /// <summary>
    /// Creates annotations around an Azure Spatial Anchor.
    /// </summary>
    public class ASAAnnotator : AnnotatorBase
    {
        protected override Task<bool> TryPlaceVisualAsync(Vector3 position)
        {
            ObjectVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ObjectVisual.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            ObjectVisual.transform.position = position;

            IsVisualPlaced = true;
            return Task.FromResult(true);
        }
    }
}