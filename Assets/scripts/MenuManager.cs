using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class MenuManager : MonoBehaviour
{
    public GameObject PanelPrincipal;
    public GameObject PanelPersonajes;
    public GameObject PanelAbout;

    public TMP_Text TextoPersonajeSeleccionado;

    private readonly string[] nombresPersonajes =
    {
        "GALLINA",
        "CIERVO",
        "PERRO",
        "CABALLO",
        "GATO",
        "PINGÜINO",
        "TIGRE"
    };

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        MostrarPrincipal();
        ActualizarPersonajeSeleccionado();
    }

    public void IniciarJuego()
    {
        LoaderUtility.Initialize();

        SceneManager.LoadScene("comedor", LoadSceneMode.Single);
    }

    public void MostrarPrincipal()
    {
        PanelPrincipal.SetActive(true);
        PanelPersonajes.SetActive(false);
        PanelAbout.SetActive(false);
    }

    public void MostrarPersonajes()
    {
        PanelPrincipal.SetActive(false);
        PanelPersonajes.SetActive(true);
        PanelAbout.SetActive(false);

        ActualizarPersonajeSeleccionado();
    }

    public void MostrarAbout()
    {
        PanelPrincipal.SetActive(false);
        PanelPersonajes.SetActive(false);
        PanelAbout.SetActive(true);
    }

    public void SeleccionarPersonaje(int indice)
    {
        if (indice < 0 || indice >= nombresPersonajes.Length)
        {
            return;
        }

        DatosJuego.GuardarPersonajeSeleccionado(indice);

        ActualizarPersonajeSeleccionado();
    }

    private void ActualizarPersonajeSeleccionado()
    {
        int indice = DatosJuego.ObtenerPersonajeSeleccionado();

        TextoPersonajeSeleccionado.text =
            "SELECCIONADO: " + nombresPersonajes[indice];
    }

    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}