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

    public AnimalBehaviour Animal;

    private ConfiguracionPersonaje configuracionActual;

    private void Start()
    {
        int indice = DatosJuego.ObtenerPersonajeSeleccionado();

        if (indice < 0 || indice >= Personajes.Length)
        {
            indice = DatosJuego.PersonajePorDefecto;
        }

        configuracionActual = Personajes[indice];

        if (CarneGenerador != null)
        {
            CarneGenerador.CarnePrefab = configuracionActual.AlimentoPrefab;
        }
    }

    private void Update()
    {
        if (GameManager.Instancia != null &&
            GameManager.Instancia.EstaPausado)
        {
            return;
        }

        if (Animal == null &&
            WasTapped() &&
            Reticle.CurrentPlane != null)
        {
            var objetoAnimal =
                Instantiate(configuracionActual.AnimalPrefab);

            Animal =
                objetoAnimal.GetComponent<AnimalBehaviour>();

            Animal.Reticle = Reticle;

            Animal.transform.position =
                Reticle.transform.position;

            Animal.ConfigurarSonidos(
                configuracionActual.SonidoAnimal,
                SonidoComiendo
            );

            SuperficieManager.LockPlane(
                Reticle.CurrentPlane
            );
        }
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

        var touch = Input.GetTouch(0);

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