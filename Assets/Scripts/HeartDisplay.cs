using System.Collections.Generic;
using AnttiStarterKit.Animations;
using UnityEngine;

public class HeartDisplay : MonoBehaviour
{
    [SerializeField] private List<Transform> hearts;

    private int lives = 10;

    public void LoseLives(int amount)
    {
        if (amount <= 0) return;
        
        var next = Mathf.Max(0, lives - amount);
        var cur = 0;
        
        for (var i = lives - 1; i >= next; i--)
        {
            Tweener.ScaleToQuad(hearts[i], Vector3.zero, 0.3f, cur * 0.1f);
            cur++;
        }
            
        lives = next;
    }
}