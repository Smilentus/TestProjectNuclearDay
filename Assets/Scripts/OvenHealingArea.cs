using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenHealingArea : MonoBehaviour
{
    private HealthModule playerHealthModule;
    public float HealValue = 1f;

    private void Start()
    {
        StartCoroutine(HealingAction());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerHealthModule = collision.GetComponent<HealthModule>();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (playerHealthModule == null)
        {
            if (collision.CompareTag("Player"))
            {
                playerHealthModule = collision.GetComponent<HealthModule>();
            }    
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerHealthModule = null;
        }
    }

    private IEnumerator HealingAction()
    {
        while(true)
        {
            if (playerHealthModule != null)
            {
                playerHealthModule.AddHealth(HealValue);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
