using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;

    [Header("Particles")]
    [SerializeField] private ParticleSystem hitParticle;
    [SerializeField] private ParticleSystem winParticle;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayHitEffect(Vector3 position)
    {
        if (!hitParticle) return;

        ParticleSystem p =
            Instantiate(hitParticle, position, Quaternion.identity);
        p.Play();
        Destroy(p.gameObject, 2f);
    }

    public void PlayWinEffect()
    {
        if (!winParticle) return;
        winParticle.Play();
    }
}
