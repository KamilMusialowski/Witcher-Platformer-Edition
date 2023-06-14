using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Resultbar : MonoBehaviour
{
    [SerializeField] private Text ScoreText;
    [SerializeField] private Result PlayerResult;

    private void Start()
    {
        ScoreText.text = PlayerResult.currentScore.ToString();
    }

    private void Update()
    {
        ScoreText.text = PlayerResult.currentScore.ToString();
    }
}
