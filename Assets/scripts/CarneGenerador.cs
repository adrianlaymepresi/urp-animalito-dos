using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
public class CarneGenerador : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public SuperficieManager SuperficieManager;
    public CarneBehaviour Carne;
    public GameObject CarnePrefab;

    // Genera un punto aleatorio dentro de un triángulo definido por dos vectores
    public static Vector3 RandomInTriangle(Vector3 v1, Vector3 v2)
    {
        float u = Random.Range(0.0f, 1.0f);
        float v = Random.Range(0.0f, 1.0f);

        if (v + u > 1)
        {
            v = 1 - v;
            u = 1 - u;
        }

        return (v1 * u) + (v2 * v);
    }

    // Encuentra un punto aleatorio dentro de un triángulo de la malla del plano
    public static Vector3 FindRandomLocation(ARPlane plane)
    {
        var mesh = plane.GetComponent<ARPlaneMeshVisualizer>().mesh;
        var triangles = mesh.triangles;
        var vertices = mesh.vertices;

        if (triangles.Length < 3)
        {
            // El plano aún no tiene geometría suficiente
            return plane.transform.position;
        }

        // Elegir un triángulo completo al azar
        int triIndex = Random.Range(0, triangles.Length / 3); // cantidad de triángulos
        int i1 = triangles[triIndex * 3];
        int i2 = triangles[triIndex * 3 + 1];
        int i3 = triangles[triIndex * 3 + 2];

        // Obtener sus vértices locales metodo de track triangular 1,2,3
        Vector3 v1 = vertices[i1];
        Vector3 v2 = vertices[i2];
        Vector3 v3 = vertices[i3];

        // Escoger un punto aleatorio dentro del triángulo -- ojo con lod vslores 
        Vector3 randomInTriangle = RandomInTriangle(v2 - v1, v3 - v1) + v1;

        // Convertir a coordenadas de mundo
        return plane.transform.TransformPoint(randomInTriangle);
    }

    public void GenerarCarne(ARPlane plane)
    {
        var carneClone = GameObject.Instantiate(CarnePrefab);
        carneClone.transform.position = FindRandomLocation(plane);

        Carne = carneClone.GetComponent<CarneBehaviour>();
    }

    private void Update()
    {
        var lockedPlane = SuperficieManager.LockedPlane;
        if (lockedPlane != null)
        {
            if (Carne == null)
            {
                GenerarCarne(lockedPlane);
            }

            if (Carne != null)
            {
                // Alinear el paquete con la altura del plano bloqueado
                Vector3 carnePosition = Carne.transform.position;
                carnePosition = new Vector3(carnePosition.x, lockedPlane.center.y, carnePosition.z);
                Carne.transform.position = carnePosition;
            }
        }
    }
}
