using System.Collections;
using UnityEngine;

public class AnimalBehaviour : MonoBehaviour
{
    public ReticleBehaviour Reticle;

    public float Speed = 1.2f;

    private AudioSource audioSourceAnimal;
    private AudioSource audioSourceComiendo;

    private AudioClip sonidoAnimal;
    private AudioClip sonidoComiendo;

    private Coroutine rutinaSonidoAnimal;

    private void Awake()
    {
        audioSourceAnimal =
            gameObject.AddComponent<AudioSource>();

        audioSourceComiendo =
            gameObject.AddComponent<AudioSource>();

        ConfigurarAudioSource(audioSourceAnimal);
        ConfigurarAudioSource(audioSourceComiendo);
    }

    private void ConfigurarAudioSource(AudioSource audioSource)
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
        sonidoAnimal = nuevoSonidoAnimal;
        sonidoComiendo = nuevoSonidoComiendo;

        if (rutinaSonidoAnimal != null)
        {
            StopCoroutine(rutinaSonidoAnimal);
        }

        if (sonidoAnimal != null)
        {
            rutinaSonidoAnimal =
                StartCoroutine(ReproducirSonidoAnimal());
        }
    }

    private IEnumerator ReproducirSonidoAnimal()
    {
        while (true)
        {
            audioSourceAnimal.PlayOneShot(sonidoAnimal);

            yield return new WaitForSeconds(
                sonidoAnimal.length
            );

            yield return new WaitForSeconds(2f);
        }
    }

    private void Update()
    {
        if (GameManager.Instancia != null &&
            GameManager.Instancia.EstaPausado)
        {
            return;
        }

        var trackingPosition =
            Reticle.transform.position;

        if (Vector3.Distance(
                trackingPosition,
                transform.position
            ) < 0.1f)
        {
            return;
        }

        var lookRotation =
            Quaternion.LookRotation(
                trackingPosition -
                transform.position
            );

        transform.rotation =
            Quaternion.Lerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * 10f
            );

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                trackingPosition,
                Speed * Time.deltaTime
            );
    }

    private void OnTriggerEnter(Collider other)
    {
        var alimento =
            other.GetComponentInParent<CarneBehaviour>();

        if (alimento != null)
        {
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

            Destroy(alimento.gameObject);
        }
    }
}