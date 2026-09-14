using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RetoManager : MonoBehaviour
{
    public static RetoManager Instancia;

    [Header("Configuracion")]
    public float DuracionReto = 30f;

    [Header("Referencias")]
    public AnimalManager AnimalManager;
    public GameObject BotonDash;
    public Button BotonReto;

    [Header("Interfaz")]
    public GameObject TextoTiempoObjeto;
    public TMP_Text TextoTiempo;

    public GameObject PanelResultadoReto;
    public TMP_Text TextoResultado;

    [Header("Audio")]
    public AudioClip SonidoSegundo;
    public AudioClip SonidoFinReto;

    private AudioSource audioSource;

    private bool retoActivo;
    private float tiempoRestante;

    public bool RetoActivo
    {
        get
        {
            return retoActivo;
        }
    }

    private void Awake()
    {
        Instancia = this;

        audioSource =
            gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        /*
         * Esto permite escuchar el sonido final
         * incluso cuando congelamos el juego.
         */
        audioSource.ignoreListenerPause = true;
    }

    private void Start()
    {
        retoActivo = false;

        if (TextoTiempoObjeto != null)
        {
            TextoTiempoObjeto.SetActive(false);
        }

        if (PanelResultadoReto != null)
        {
            PanelResultadoReto.SetActive(false);
        }

        if (BotonDash != null)
        {
            BotonDash.SetActive(true);
        }
    }

    public void IniciarReto()
    {
        if (retoActivo)
        {
            return;
        }

        if (AnimalManager == null ||
            AnimalManager.Animal == null)
        {
            Debug.Log(
                "Primero debes colocar el animal."
            );

            return;
        }

        StartCoroutine(
            EjecutarReto()
        );
    }

    private IEnumerator EjecutarReto()
    {
        retoActivo = true;

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.ReiniciarScore();

            /*
             * Cierra el menu de pausa y
             * devuelve el juego a funcionamiento normal.
             */
            GameManager.Instancia.Continuar();
        }

        if (BotonDash != null)
        {
            BotonDash.SetActive(false);
        }

        if (BotonReto != null)
        {
            BotonReto.interactable = false;
        }

        if (TextoTiempoObjeto != null)
        {
            TextoTiempoObjeto.SetActive(true);
        }

        tiempoRestante =
            DuracionReto;

        int segundoAnterior =
            Mathf.CeilToInt(tiempoRestante);

        ActualizarTextoTiempo(
            segundoAnterior
        );

        while (tiempoRestante > 0f)
        {
            if (GameManager.Instancia != null &&
                GameManager.Instancia.EstaPausado)
            {
                yield return null;
                continue;
            }

            tiempoRestante -=
                Time.deltaTime;

            int segundoActual =
                Mathf.CeilToInt(
                    tiempoRestante
                );

            if (segundoActual <
                segundoAnterior)
            {
                segundoAnterior =
                    segundoActual;

                if (segundoActual > 0)
                {
                    ReproducirSonidoSegundo();
                }
            }

            ActualizarTextoTiempo(
                segundoActual
            );

            yield return null;
        }

        FinalizarReto();
    }

    private void ActualizarTextoTiempo(
        int segundos
    )
    {
        if (TextoTiempo == null)
        {
            return;
        }

        segundos =
            Mathf.Max(
                segundos,
                0
            );

        TextoTiempo.text =
            segundos.ToString();
    }

    private void ReproducirSonidoSegundo()
    {
        if (SonidoSegundo == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            SonidoSegundo
        );
    }

    private void FinalizarReto()
    {
        retoActivo = false;

        int scoreFinal = 0;

        if (GameManager.Instancia != null)
        {
            scoreFinal =
                GameManager.Instancia.ObtenerScore();
        }

        if (TextoTiempo != null)
        {
            TextoTiempo.text =
                "0";
        }

        if (SonidoFinReto != null)
        {
            audioSource.PlayOneShot(
                SonidoFinReto
            );
        }

        if (TextoResultado != null)
        {
            TextoResultado.text =
                "COMISTE " +
                scoreFinal +
                " ALIMENTOS\n" +
                "EN 30 SEGUNDOS";
        }

        if (PanelResultadoReto != null)
        {
            PanelResultadoReto.SetActive(
                true
            );
        }

        Time.timeScale =
            0f;

        AudioListener.pause =
            true;
    }

    public void CerrarResultado()
    {
        Time.timeScale =
            1f;

        AudioListener.pause =
            false;

        /*
         * El usuario pidio volver al juego
         * como si hubiera pulsado Reiniciar.
         */
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.Reiniciar();
        }
    }
}