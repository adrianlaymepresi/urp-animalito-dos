using System;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class ConfiguracionPersonaje
{
    public string Nombre;
    public GameObject AnimalPrefab;
    public GameObject AlimentoPrefab;
    public AudioClip SonidoAnimal;
}

public class AnimalManager : MonoBehaviour
{
    public ReticleBehaviour Reticle;

    public SuperficieManager SuperficieManager;

    public CarneGenerador CarneGenerador;

    public ConfiguracionPersonaje[] Personajes;

    public AudioClip SonidoComiendo;

    [Header("Efectos al comer")]
    public GameObject EfectoComerPrefab;

    public GameObject TextoPuntoPrefab;

    public AnimalBehaviour Animal;

    private ConfiguracionPersonaje configuracionActual;

    private void Start()
    {
        if (Personajes == null ||
            Personajes.Length == 0)
        {
            return;
        }

        int indice =
            DatosJuego.ObtenerPersonajeSeleccionado();

        if (indice < 0 ||
            indice >= Personajes.Length)
        {
            indice =
                DatosJuego.PersonajePorDefecto;
        }

        configuracionActual =
            Personajes[indice];

        if (CarneGenerador != null)
        {
            CarneGenerador.CarnePrefab =
                configuracionActual.AlimentoPrefab;
        }
    }

    private void Update()
    {
        if (GameManager.Instancia != null &&
            GameManager.Instancia.EstaPausado)
        {
            return;
        }

        if (configuracionActual == null)
        {
            return;
        }

        if (Animal == null &&
            WasTapped() &&
            Reticle.CurrentPlane != null)
        {
            GameObject objetoAnimal =
                Instantiate(
                    configuracionActual.AnimalPrefab
                );

            Animal =
                objetoAnimal.GetComponent<AnimalBehaviour>();

            Animal.Reticle =
                Reticle;

            Animal.transform.position =
                Reticle.transform.position;

            Animal.ConfigurarSonidos(
                configuracionActual.SonidoAnimal,
                SonidoComiendo
            );

            Animal.ConfigurarEfectos(
                EfectoComerPrefab,
                TextoPuntoPrefab
            );

            SuperficieManager.LockPlane(
                Reticle.CurrentPlane
            );
        }
    }

    public void EjecutarDash()
    {
        if (RetoManager.Instancia != null &&
            RetoManager.Instancia.RetoActivo)
        {
            return;
        }

        if (GameManager.Instancia != null &&
            GameManager.Instancia.EstaPausado)
        {
            return;
        }

        if (Animal == null)
        {
            return;
        }

        if (CarneGenerador == null)
        {
            return;
        }

        if (CarneGenerador.Carne == null)
        {
            return;
        }

        Animal.IniciarDash(
            CarneGenerador.Carne.transform
        );
    }

    private bool WasTapped()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            return true;
        }

        if (Input.touchCount == 0)
        {
            return false;
        }

        Touch touch =
            Input.GetTouch(0);

        if (touch.phase != TouchPhase.Began)
        {
            return false;
        }

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(
                touch.fingerId
            ))
        {
            return false;
        }

        return true;
    }
}