using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia { get; private set; }

    public TMP_Text TextoScore;
    public GameObject PanelPausa;

    public bool EstaPausado { get; private set; }

    private int score;

    private void Awake()
    {
        Instancia = this;

        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void Start()
    {
        score = 0;
        EstaPausado = false;

        PanelPausa.SetActive(false);

        ActualizarScore();
    }

    public void SumarPunto()
    {
        score++;

        ActualizarScore();
    }

    private void ActualizarScore()
    {
        TextoScore.text = "SCORE: " + score;
    }

    public void Pausar()
    {
        EstaPausado = true;

        PanelPausa.SetActive(true);

        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public void Continuar()
    {
        EstaPausado = false;

        PanelPausa.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void Reiniciar()
    {
        RestaurarTiempo();

        StartCoroutine(ReiniciarEscena());
    }

    private IEnumerator ReiniciarEscena()
    {
        LoaderUtility.Deinitialize();

        yield return null;

        LoaderUtility.Initialize();

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name,
            LoadSceneMode.Single
        );
    }

    public void VolverAlMenu()
    {
        RestaurarTiempo();

        SceneManager.LoadScene(
            "MenuInicial",
            LoadSceneMode.Single
        );

        LoaderUtility.Deinitialize();
    }

    private void RestaurarTiempo()
    {
        EstaPausado = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public int ObtenerScore()
    {
        return score;
    }

    public void ReiniciarScore()
    {
        score = 0;

        if (TextoScore != null)
        {
            TextoScore.text = "SCORE: 0";
        }
    }
}