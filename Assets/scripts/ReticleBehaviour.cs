using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ReticleBehaviour : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject Child;
    public SuperficieManager SuperficieManager;

    public ARPlane CurrentPlane;

    // Start is called before the first frame update
    private void Start()
    {
        Child = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        SuperficieManager.RaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinBounds);

        CurrentPlane = null;
        ARRaycastHit? hit = null;
        if (hits.Count > 0)
        {
            // If you don't have a locked plane already...
            var lockedPlane = SuperficieManager.LockedPlane;
            hit = lockedPlane == null
                // ... use the first hit in `hits`.
                ? hits[0]
                // Otherwise use the locked plane, if it's there.
                : hits.SingleOrDefault(x => x.trackableId == lockedPlane.trackableId);
        }
        if (hit.HasValue)
            {
                CurrentPlane = SuperficieManager.PlaneManager.GetPlane(hit.Value.trackableId);
                // Move this reticle to the location of the hit.
                transform.position = hit.Value.pose.position;
            }
            Child.SetActive(CurrentPlane != null);




    }
}
