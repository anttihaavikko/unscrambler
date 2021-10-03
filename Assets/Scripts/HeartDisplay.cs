using System.Collections.Generic;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Managers;
using UnityEngine;
using AnttiStarterKit.Extensions;

public class HeartDisplay : MonoBehaviour
{
    [SerializeField] private List<Transform> hearts;
    [SerializeField] private Camera cam;

    private int lives = 10;

    public void LoseLives(int amount)
    {
        if (amount <= 0) return;
        
        var next = Mathf.Max(0, lives - amount);
        var cur = 0;
        
        for (var i = lives - 1; i >= next; i--)
        {
            var pos = cam.ScreenToWorldPoint(hearts[i].transform.position).WhereZ(0);
            var delay = cur * 0.1f;
            Tweener.ScaleToQuad(hearts[i], Vector3.zero, 0.3f, delay);
            this.StartCoroutine(() => EffectManager.AddEffect(2, pos), delay + 0.15f);
            cur++;
        }
            
        lives = next;
    }
}