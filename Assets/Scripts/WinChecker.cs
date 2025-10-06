using TMPro;

using UnityEngine;

public class WinChecker : MonoBehaviour
{
    public TMP_Text toggle;
    public Cauldron cauldron;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (TrophyHandler.trophiesCollected.Count >= 24 && cauldron.completedPotionSlots.Count >= 12)
        {
            toggle.enabled = true;
        }
    }
}
