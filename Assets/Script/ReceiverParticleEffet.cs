using UnityEngine;

public class ReceiverParticleEffet : MonoBehaviour
{
    [SerializeField] private Material mat_particle;

    [Header("Settings")]
    [SerializeField] private Color color = Color.white;
    [SerializeField] private int burstCount = 30;
    [SerializeField] private float lifetime = 0.6f;
    [SerializeField] private float startSpeed = 3.5f;
    [SerializeField] private float startSize = 0.35f;
    [SerializeField] private float shapeRadius = 0.3f;

    private ParticleSystem _ps;
    private bool _isLooping;

    private void Awake()
    {
        _ps = gameObject.AddComponent<ParticleSystem>();
        _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Setup();
    }

    private void Setup()
    {
        ParticleSystem.MainModule main = _ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 1f;
        main.startLifetime = lifetime;
        main.startSpeed = startSpeed;
        main.startSize = startSize;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Callback;

        ParticleSystem.EmissionModule emission = _ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

        ParticleSystem.ShapeModule shape = _ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = shapeRadius;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = _ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = gradient;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = _ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ParticleSystemRenderer psRenderer = GetComponent<ParticleSystemRenderer>();
        psRenderer.material = mat_particle;
        psRenderer.material.EnableKeyword("_EMISSION");
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        psRenderer.alignment = ParticleSystemRenderSpace.View;
    }
    
    public void Play()
    {
        _isLooping = true;
        PlayOnce();
    }

    public void StopLoop()
    {
        _isLooping = false;
    }

    private void PlayOnce()
    {
        _ps.Clear();
        _ps.Play();
    }

    private void OnParticleSystemStopped()
    {
        if (_isLooping)
        {
            PlayOnce();
        }
    }
}
