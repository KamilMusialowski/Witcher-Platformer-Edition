using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class responsible for displaying players health.
/// </summary>
public class Healthbar : MonoBehaviour
{
    /// <summary>
    /// Players Health class object field.
    /// </summary>
    [SerializeField] private Health playerHealth;
    /// <summary>
    /// "Black" hearts from GUI - representing inactive lives.
    /// </summary>
    [SerializeField] private Image totalhealthbar;
    /// <summary>
    /// "Active" player hearts from GUI.
    /// </summary>
    [SerializeField] private Image currenthealthbar;

    /// <summary>
    /// Initialization of Player Health GUI.
    /// </summary>
    private void Start()
    {
        totalhealthbar.fillAmount = playerHealth.maxbarHealth/10;
    }
    /// <summary>
    /// Method for update Player Health on GUI.
    /// </summary>
    private void Update()
    {
        currenthealthbar.fillAmount = playerHealth.currentHealth/10;
    }
}
