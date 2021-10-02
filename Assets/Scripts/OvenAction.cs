using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.EventSystems;

public class OvenAction : MonoBehaviour, IPointerClickHandler
{
    public Sprite activeSprite;
    public GameObject healingArea;

    public bool canActivateOven = false;
    private bool isPlayerAround = false;
    private bool isOvenActive = false;

    public AudioClip FurnaceBurning;
    public AudioClip FurnaceOpening;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isPlayerAround && !isOvenActive && canActivateOven)
        {
            StartCoroutine(InteractWithOven());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerInteractArea") && canActivateOven)
        {
            isPlayerAround = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerInteractArea") && canActivateOven && !isPlayerAround)
        {
            isPlayerAround = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerInteractArea") && canActivateOven)
        {
            isPlayerAround = false;
        }
    }

    private IEnumerator InteractWithOven()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        playerController.InteractWithObject();

        yield return new WaitForSpineAnimationComplete(playerController.trackEntry);

        isOvenActive = true;
        this.GetComponent<SpriteRenderer>().sprite = activeSprite;
        this.transform.GetChild(0).gameObject.SetActive(true);
        StartCoroutine(OvenSounds());
    }

    private IEnumerator OvenSounds()
    {
        this.GetComponent<AudioSource>().clip = FurnaceOpening;
        this.GetComponent<AudioSource>().loop = false;
        this.GetComponent<AudioSource>().Play();
        healingArea.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        this.GetComponent<AudioSource>().clip = FurnaceBurning;
        this.GetComponent<AudioSource>().loop = true;
        this.GetComponent<AudioSource>().Play();
    }
}
