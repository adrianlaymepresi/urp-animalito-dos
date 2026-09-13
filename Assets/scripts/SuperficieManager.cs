using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SuperficieManager : MonoBehaviour
{
    public ARPlaneManager PlaneManager;
    public ARRaycastManager RaycastManager;
    public ARPlane LockedPlane;

    // marca que hemos bloqueado un plano (para empezar a filtrar)
    private bool isLocked = false;

    private void Start()
    {
        if (PlaneManager == null)
            PlaneManager = GetComponent<ARPlaneManager>();
    }

    public void LockPlane(ARPlane keepPlane)
    {
        if (PlaneManager == null)
            PlaneManager = GetComponent<ARPlaneManager>();

        if (keepPlane == null) return;

        // Desactivar todos los planos excepto el seleccionado (una vez)
        var arPlane = keepPlane.GetComponent<ARPlane>();
        foreach (var plane in PlaneManager.trackables)
        {
            if (plane != arPlane)
                plane.gameObject.SetActive(false);
        }

        LockedPlane = arPlane;
        isLocked = true;

        // NOTA: no nos suscribimos a events por compatibilidad con varias versiones.
        // Nuevos planos se desactivarán en Update() (polling).
    }

    private void Update()
    {
        if (!isLocked || LockedPlane == null || PlaneManager == null) return;

        // 1) Desactiva cualquier plano nuevo que aparezca (efecto "lock")
        foreach (var plane in PlaneManager.trackables)
        {
            if (plane != LockedPlane && plane.gameObject.activeSelf)
                plane.gameObject.SetActive(false);
        }

        // 2) Gestiona subsumption (dos posibilidades según la versión de ARFoundation)
        Type planeType = typeof(ARPlane);

        // Intentamos primero encontrar 'subsumedById' (TrackableId) — versiones más recientes lo exponen
        PropertyInfo subsumedByIdProp = planeType.GetProperty(
            "subsumedById",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
        );

        if (subsumedByIdProp != null)
        {
            object val = subsumedByIdProp.GetValue(LockedPlane);
            if (val is TrackableId subsumedId && subsumedId != TrackableId.invalidId)
            {
                // Si hay un TrackableId padre, obtenlo del PlaneManager
                var newPlane = PlaneManager.GetPlane(subsumedId);
                if (newPlane != null && newPlane != LockedPlane)
                {
                    LockedPlane = newPlane;
                }
            }

            return; // ya tratamos este caso
        }

        // Si no existe subsumedById, intentamos 'subsumedBy' (ARPlane) — versiones antiguas
        PropertyInfo subsumedByProp = planeType.GetProperty(
            "subsumedBy",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
        );

        if (subsumedByProp != null)
        {
            object subsumedObj = subsumedByProp.GetValue(LockedPlane);
            if (subsumedObj is ARPlane parentPlane && parentPlane != null && parentPlane != LockedPlane)
            {
                LockedPlane = parentPlane;
            }
        }

        // Si ninguna propiedad existe, no hacemos nada (la versión de AR Foundation no soporta subsumption).
    }
}
