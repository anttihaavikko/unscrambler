using System;
using UnityEngine;

namespace AnttiStarterKit.Utils
{
    public class DisableAfter : MonoBehaviour
    {
        [SerializeField] private float delay = 1f;

        private void Start()
        {
            Invoke(nameof(DoDisable), delay);
        }

        private void DoDisable()
        {
            gameObject.SetActive(false);
        }
    }
}