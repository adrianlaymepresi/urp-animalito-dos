using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scene-local visual preview. MenuManager and its persistent button callbacks
/// remain the only owners of the saved character selection.
/// </summary>
[DisallowMultipleComponent]
public sealed class PersonajePreviewManager : MonoBehaviour
{
    [System.Serializable]
    public sealed class PersonajeVisual
    {
        public string Nombre;
        public GameObject Prefab;
        public Vector3 Rotacion = new Vector3(0f, 145f, 0f);
        public Vector3 Desplazamiento = Vector3.zero;
        [Range(0.5f, 1.5f)] public float AjusteEscala = 1f;
        [Range(1f, 2f)] public float MargenExtra = 1f;
    }

    public GameObject PreviewRig;
    public Transform PreviewContainer;
    public Camera PreviewCamera;
    public RenderTexture PreviewTexture;
    public RawImage PreviewImage;
    public TMP_Text NombrePersonaje;
    public Button[] Botones;
    public PersonajeVisual[] Personajes;
    [Range(0, 31)] public int PreviewLayer = 8;
    [Range(1.05f, 1.5f)] public float MargenEncuadre = 1.2f;

    public int CurrentIndex { get; private set; } = -1;
    public GameObject CurrentModel { get; private set; }

    private Coroutine cambio;
    private bool refrescarSeleccion;
    private static readonly int Vert = Animator.StringToHash("Vert");
    private static readonly int State = Animator.StringToHash("State");

    private void OnEnable()
    {
        if (!Application.isPlaying) return;
        if (!ConfiguracionValida())
        {
            Debug.LogError("PersonajePreviewManager: faltan referencias del preview.", this);
            return;
        }
        foreach (Button boton in Botones)
            if (boton != null) boton.onClick.AddListener(SolicitarActualizacion);

        MostrarSeleccionGuardada();
    }

    private bool ConfiguracionValida()
    {
        return PreviewRig != null && PreviewContainer != null &&
               PreviewCamera != null && PreviewTexture != null &&
               PreviewImage != null && NombrePersonaje != null &&
               Botones != null && Personajes != null && Personajes.Length == 7;
    }

    private void SolicitarActualizacion()
    {
        // Read in LateUpdate so every original persistent callback has finished.
        refrescarSeleccion = true;
    }

    private void LateUpdate()
    {
        if (!refrescarSeleccion) return;
        refrescarSeleccion = false;
        MostrarSeleccionGuardada();
    }

    private void MostrarSeleccionGuardada()
    {
        int indice = DatosJuego.ObtenerPersonajeSeleccionado();
        if (indice < 0 || indice >= Personajes.Length)
            indice = DatosJuego.PersonajePorDefecto;

        if (CurrentIndex == indice && CurrentModel != null) return;
        if (cambio != null) StopCoroutine(cambio);
        RetirarModelo();
        CurrentIndex = indice;
        NombrePersonaje.text = Personajes[indice].Nombre;
        cambio = StartCoroutine(CrearPreview(indice));
    }

    private IEnumerator CrearPreview(int indice)
    {
        PersonajeVisual visual = Personajes[indice];
        if (visual.Prefab == null)
        {
            Debug.LogError("No hay prefab de preview para " + visual.Nombre, this);
            yield break;
        }

        PreviewCamera.enabled = false;
        PreviewImage.enabled = false;
        PreviewContainer.gameObject.SetActive(false);

        // Inactive parent prevents AnimalBehaviour.Awake from running during Instantiate.
        GameObject modelo = Instantiate(visual.Prefab, PreviewContainer, false);
        CurrentModel = modelo;
        modelo.name = "Preview_" + visual.Nombre;
        modelo.SetActive(false);

        foreach (Transform pieza in modelo.GetComponentsInChildren<Transform>(true))
            pieza.gameObject.layer = PreviewLayer;

        foreach (MonoBehaviour comportamiento in modelo.GetComponentsInChildren<MonoBehaviour>(true))
        {
            comportamiento.enabled = false;
            Destroy(comportamiento);
        }
        foreach (AudioSource audio in modelo.GetComponentsInChildren<AudioSource>(true))
        {
            audio.enabled = false;
            Destroy(audio);
        }
        foreach (Collider collider in modelo.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
            Destroy(collider);
        }
        foreach (Rigidbody cuerpo in modelo.GetComponentsInChildren<Rigidbody>(true))
        {
            cuerpo.isKinematic = true;
            cuerpo.detectCollisions = false;
            Destroy(cuerpo);
        }

        // Allow deferred component destruction to finish BEFORE activating the clone.
        yield return null;
        if (modelo == null || CurrentModel != modelo) yield break;

        PreviewRig.SetActive(true);
        modelo.transform.localPosition = Vector3.zero;
        modelo.transform.localRotation = Quaternion.Euler(visual.Rotacion);
        modelo.transform.localScale = Vector3.one;
        modelo.SetActive(true);
        PreviewContainer.gameObject.SetActive(true);

        foreach (Animator animator in modelo.GetComponentsInChildren<Animator>(true))
        {
            animator.enabled = true;
            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.SetFloat(Vert, 0f);
            animator.SetFloat(State, 0f);
            animator.Update(0f);
        }
        foreach (Renderer renderer in modelo.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }

        Encuadrar(modelo, visual);
        if (!PreviewTexture.IsCreated()) PreviewTexture.Create();
        PreviewCamera.targetTexture = PreviewTexture;
        PreviewImage.texture = PreviewTexture;
        PreviewImage.enabled = true;
        PreviewCamera.enabled = true;
        cambio = null;
    }

    private void Encuadrar(GameObject modelo, PersonajeVisual visual)
    {
        Renderer[] renderers = modelo.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;
        Bounds bounds = CalcularBounds(renderers);
        float altura = Mathf.Max(bounds.size.y, 0.01f);
        modelo.transform.localScale = Vector3.one * (2.4f / altura) * visual.AjusteEscala;
        bounds = CalcularBounds(renderers);
        modelo.transform.position += PreviewContainer.position + visual.Desplazamiento - bounds.center;
        bounds = CalcularBounds(renderers);

        float aspecto = (float)PreviewTexture.width / PreviewTexture.height;
        PreviewCamera.aspect = aspecto;
        PreviewCamera.transform.SetPositionAndRotation(
            PreviewContainer.position + new Vector3(0f, 0f, -8f), Quaternion.identity);
        // Bounds include the rig's full imported extents, providing room for Idle motion.
        PreviewCamera.orthographicSize = Mathf.Max(
            bounds.extents.y + Mathf.Abs(visual.Desplazamiento.y),
            (bounds.extents.x + Mathf.Abs(visual.Desplazamiento.x)) / aspecto
        ) * MargenEncuadre * visual.MargenExtra;
    }

    private static Bounds CalcularBounds(Renderer[] renderers)
    {
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private void RetirarModelo()
    {
        if (CurrentModel != null)
        {
            // Hide immediately; deferred Destroy can never leave two active animals.
            CurrentModel.SetActive(false);
            Destroy(CurrentModel);
            CurrentModel = null;
        }
    }

    private void OnDisable()
    {
        if (!Application.isPlaying) return;
        if (Botones != null)
            foreach (Button boton in Botones)
                if (boton != null) boton.onClick.RemoveListener(SolicitarActualizacion);
        StopAllCoroutines();
        cambio = null;
        refrescarSeleccion = false;
        RetirarModelo();
        CurrentIndex = -1;
        if (PreviewCamera != null) PreviewCamera.enabled = false;
        if (PreviewContainer != null) PreviewContainer.gameObject.SetActive(false);
        if (PreviewRig != null) PreviewRig.SetActive(false);
        if (PreviewImage != null) PreviewImage.enabled = false;
        if (PreviewTexture != null && PreviewTexture.IsCreated()) PreviewTexture.Release();
    }
}
