using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DatosJuego
{
    private const string ClavePersonaje = "PersonajeSeleccionado";

    public const int PersonajePorDefecto = 4;

    public static void GuardarPersonajeSeleccionado(int indice)
    {
        PlayerPrefs.SetInt(ClavePersonaje, indice);
        PlayerPrefs.Save();
    }

    public static int ObtenerPersonajeSeleccionado()
    {
        return PlayerPrefs.GetInt(ClavePersonaje, PersonajePorDefecto);
    }
}