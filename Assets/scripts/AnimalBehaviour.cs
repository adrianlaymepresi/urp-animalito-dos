using System.Collections;
using UnityEngine;

public class AnimalBehaviour : MonoBehaviour
{
    public ReticleBehaviour Reticle;

    public float Speed = 1.2f;
    public float VelocidadDash = 2f;

    private Animator animator;

    private static readonly int Vert =
        Animator.StringToHash("Vert");

    private static readonly int State =
        Animator.StringToHash("State");

    private AudioSource audioSourceAnimal;
    private AudioSource audioSourceComiendo;

    private AudioClip sonidoAnimal;
    private AudioClip sonidoComiendo;

    private Coroutine rutinaSonidoAnimal;

    private bool estaEnDash = false;

    private GameObject efectoComerPrefab;
    private GameObject textoPuntoPrefab;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        audioSourceAnimal =
            gameObject.AddComponent<AudioSource>();

        audioSourceComiendo =
            gameObject.AddComponent<AudioSource>();

        ConfigurarAudioSource(audioSourceAnimal);
        ConfigurarAudioSource(audioSourceComiendo);
    }

    private void ConfigurarAudioSource(
        AudioSource audioSource
    )
    {
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;
    }

    public void ConfigurarSonidos(
        AudioClip nuevoSonidoAnimal,
        AudioClip nuevoSonidoComiendo
    )
    {
        sonidoAnimal =
            nuevoSonidoAnimal;

        sonidoComiendo =
            nuevoSonidoComiendo;

        if (rutinaSonidoAnimal != null)
        {
            StopCoroutine(
                rutinaSonidoAnimal
            );
        }

        if (sonidoAnimal != null)
        {
            rutinaSonidoAnimal =
                StartCoroutine(
                    ReproducirSonidoAnimal()
                );
        }
    }

    public void ConfigurarEfectos(
        GameObject nuevoEfectoComer,
        GameObject nuevoTextoPunto
    )
    {
        efectoComerPrefab =
            nuevoEfectoComer;

        textoPuntoPrefab =
            nuevoTextoPunto;
    }

    private IEnumerator ReproducirSonidoAnimal()
    {
        while (true)
        {
            audioSourceAnimal.PlayOneShot(
                sonidoAnimal
            );

            yield return new WaitForSeconds(
                sonidoAnimal.length
            );

            yield return new WaitForSeconds(
                2f
            );
        }
    }

    private void Update()
    {
        if (GameManager.Instancia != null &&
            GameManager.Instancia.EstaPausado)
        {
            return;
        }

        if (estaEnDash)
        {
            return;
        }

        if (Reticle == null)
        {
            return;
        }

        Vector3 trackingPosition =
            Reticle.transform.position;

        float distancia =
            Vector3.Distance(
                trackingPosition,
                transform.position
            );

        if (distancia < 0.1f)
        {
            ActivarIdle();

            return;
        }

        ActivarWalk();

        Vector3 direccion =
            trackingPosition -
            transform.position;

        direccion.y =
            0f;

        if (direccion.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRotation =
                Quaternion.LookRotation(
                    direccion
                );

            transform.rotation =
                Quaternion.Lerp(
                    transform.rotation,
                    lookRotation,
                    Time.deltaTime * 10f
                );
        }

        Vector3 destino =
            trackingPosition;

        destino.y =
            transform.position.y;

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                destino,
                Speed * Time.deltaTime
            );
    }

    public void IniciarDash(
        Transform objetivo
    )
    {
        if (estaEnDash)
        {
            return;
        }

        if (objetivo == null)
        {
            return;
        }

        StartCoroutine(
            EjecutarDash(objetivo)
        );
    }

    private IEnumerator EjecutarDash(
        Transform objetivo
    )
    {
        estaEnDash =
            true;

        ActivarRun();

        while (objetivo != null)
        {
            if (GameManager.Instancia != null &&
                GameManager.Instancia.EstaPausado)
            {
                yield return null;
                continue;
            }

            Vector3 destino =
                objetivo.position;

            destino.y =
                transform.position.y;

            float distancia =
                Vector3.Distance(
                    transform.position,
                    destino
                );

            if (distancia <= 0.02f)
            {
                break;
            }

            Vector3 direccion =
                destino -
                transform.position;

            direccion.y =
                0f;

            if (direccion.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRotation =
                    Quaternion.LookRotation(
                        direccion
                    );

                transform.rotation =
                    Quaternion.Lerp(
                        transform.rotation,
                        lookRotation,
                        Time.deltaTime * 15f
                    );
            }

            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    destino,
                    VelocidadDash *
                    Time.deltaTime
                );

            yield return null;
        }

        estaEnDash =
            false;

        ActualizarAnimacionDespuesDelDash();
    }

    private void ActualizarAnimacionDespuesDelDash()
    {
        if (Reticle == null)
        {
            ActivarIdle();

            return;
        }

        float distancia =
            Vector3.Distance(
                Reticle.transform.position,
                transform.position
            );

        if (distancia < 0.1f)
        {
            ActivarIdle();
        }
        else
        {
            ActivarWalk();
        }
    }

    private void ActivarIdle()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat(
            Vert,
            0f
        );

        animator.SetFloat(
            State,
            0f
        );
    }

    private void ActivarWalk()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat(
            Vert,
            1f
        );

        animator.SetFloat(
            State,
            0f
        );
    }

    private void ActivarRun()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat(
            Vert,
            1f
        );

        animator.SetFloat(
            State,
            1f
        );
    }

    private void MostrarEfectosComer(
        Vector3 posicion
    )
    {
        if (efectoComerPrefab != null)
        {
            GameObject efecto =
                Instantiate(
                    efectoComerPrefab,
                    posicion,
                    Quaternion.identity
                );

            Destroy(
                efecto,
                3f
            );
        }

        if (textoPuntoPrefab != null)
        {
            Vector3 posicionTexto =
                posicion +
                Vector3.up * 0.15f;

            Instantiate(
                textoPuntoPrefab,
                posicionTexto,
                Quaternion.identity
            );
        }
    }

    private void OnTriggerEnter(
        Collider other
    )
    {
        var alimento =
            other.GetComponentInParent<CarneBehaviour>();

        if (alimento == null)
        {
            return;
        }

        Vector3 posicionEfecto =
            alimento.transform.position;

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.SumarPunto();
        }

        if (sonidoComiendo != null)
        {
            audioSourceComiendo.PlayOneShot(
                sonidoComiendo
            );
        }

        MostrarEfectosComer(
            posicionEfecto
        );

        Destroy(
            alimento.gameObject
        );
    }
}