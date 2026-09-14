using TMPro;
using UnityEngine;

public class TextoPuntoFlotante : MonoBehaviour
{
    public float VelocidadSubida = 0.15f;
    public float Duracion = 1.2f;

    private TextMeshPro texto;
    private Camera camaraPrincipal;

    private float tiempoActual;

    private Color colorInicial;

    private void Awake()
    {
        texto = GetComponent<TextMeshPro>();

        camaraPrincipal = Camera.main;

        if (texto != null)
        {
            colorInicial = texto.color;
        }
    }

    private void Update()
    {
        transform.position +=
            Vector3.up *
            VelocidadSubida *
            Time.deltaTime;

        if (camaraPrincipal != null)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    transform.position -
                    camaraPrincipal.transform.position
                );
        }

        tiempoActual += Time.deltaTime;

        if (texto != null)
        {
            float alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    tiempoActual / Duracion
                );

            Color nuevoColor =
                colorInicial;

            nuevoColor.a =
                alpha;

            texto.color =
                nuevoColor;
        }

        if (tiempoActual >= Duracion)
        {
            Destroy(gameObject);
        }
    }
}