using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 *  Extension of Bounded that makes sure no
 * part of the camera leaves the Boundary.
 */
[RequireComponent(typeof(Camera))]
public class BoundedCamera : Bounded
{
    private Camera _camera {
        get {
            if (_wasCached) return _cachedCamera;

            _wasCached = true;
            _cachedCamera = GetComponent<Camera>();
            return _cachedCamera;
        }
    }

    private Camera _cachedCamera;
    private bool _wasCached;


    // Gets the extent of this camera at the z of the given Boundary, for a camera with perspective.
    private Vector2 PerspectiveExtents {
        get {
            float z = by.Bounds.center.z;
            Vector3 min = _camera.ViewportToWorldPoint(new Vector3(0, 0, z));
            Vector3 max = _camera.ViewportToWorldPoint(new Vector3(1, 1, z));

            return new Vector2((max.x - min.x) / 2, (max.y - min.y) / 2);
        }
    }

    protected override float GetHorizontalExtent() {
        if (_camera.orthographic) {
            return _camera.orthographicSize * _camera.aspect;
        }

        return PerspectiveExtents.x;
    }

    protected override float GetVerticalExtent() {
        if (_camera.orthographic) {
            return _camera.orthographicSize;
        }

        return PerspectiveExtents.y;
    }
}