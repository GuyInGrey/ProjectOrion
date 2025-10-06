using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Torchlight : MonoBehaviour
{

    [SerializeField] float _maxIntensity = 80.0f;
    [SerializeField] float _minIntensity = 20.0f;
    [SerializeField] float _flickerSpeed = 10f; // Adjust this to control flickering speed
    [SerializeField] float _intensityVariation = 20.0f; // Adjust this for intensity variation

    [SerializeField] SpriteRenderer rendererToSetColor;

    Light2D _light;
    private float currentIntensity;
    private float targetIntensity;

    //[SerializeField] Color _color = new(.9216f, .5686f, .1882f);
    //[SerializeField] Color _undertone = new(.9647f, .8980f, .4588f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _light = GetComponent<Light2D>();
        currentIntensity = _light.intensity;
        if (rendererToSetColor != null)
        {
            rendererToSetColor.color = _light.color;
        }
        StartCoroutine(Flicker());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator Flicker()
    {
        while (true)
        {
            // Randomly determine target intensity
            targetIntensity = Random.Range(_minIntensity, _maxIntensity);

            // Add some variation to the intensity
            targetIntensity += Random.Range(-_intensityVariation, _intensityVariation);

            // Smoothly change the intensity
            while (Mathf.Abs(currentIntensity - targetIntensity) > 0.1f)
            {
                currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * _flickerSpeed);
                _light.intensity = currentIntensity;
                yield return null;
            }
            yield return null;
        }
    }
}
