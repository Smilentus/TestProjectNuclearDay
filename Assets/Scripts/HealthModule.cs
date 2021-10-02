using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthModule : MonoBehaviour
{
    public float maxHealth = 100;
    public float health = 100;

    private Image healthBarImage;

    private void Start()
    {
        healthBarImage = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();
        UpdateHealthUI();
    }

    public void AddHealth(float value)
    {
        health += value;

        if (health >= maxHealth)
        {
            health = maxHealth;
            FindObjectOfType<Fader>().StartFadeIn();
        }

        UpdateHealthUI();
    }

    public void TakeHealth(float value)
    {
        health -= value;

        if (health <= 0)
        {
            health = 0;
            OnTakenDamageEvent(null);
        }

        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        healthBarImage.fillAmount = health / maxHealth;
    }

    protected virtual void OnTakenDamageEvent(EventArgs e)
    {
        EventHandler<EventArgs> handler = OnHealthBelowZeroHandler;
        if (handler != null)
        {
            handler(this, e);
        }
    }

    public event EventHandler<EventArgs> OnHealthBelowZeroHandler;
}

