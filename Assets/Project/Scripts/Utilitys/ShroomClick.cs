using ShroomClicker;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShroomClick : MonoBehaviour
{
    public TextMeshProUGUI shroomCountText;

    public Canvas mainCanvas;

    public AudioSource audioSource;
    public AudioClip clickSound;

    private ShroomData data;

    // CPS / Click Mode
    private Queue<float> clickTimes = new Queue<float>();
    private const float cpsWindow = 1f;
    private const float cpsThreshold = 10f;
    public bool clickModeActive = false;
    private float clickModeTimer = 0f;
    private const float deactivateDelay = 2f;

    private Vector2 shroomColNormal = new Vector2(4f, 3.5f);
    private Vector2 shroomColClickMode = new Vector2(10f, 15f);

    public void InitWithData(ShroomData initData)
    {
        data = initData;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        UpdateClickMode(deltaTime);

        // Mouse handling for PC
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            CheckClick(ray);
        }

        // Touch handling for Mobile
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    Vector2 touchPos = touch.position.ReadValue();
                    Ray ray = Camera.main.ScreenPointToRay(touchPos);

                    CheckClick(ray);
                }
            }

        }
    }

    void CheckClick(Ray ray)
    {
        RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit2D.collider != null)
        {
            if (hit2D.collider.CompareTag("Shroom"))
            {
                SaveLoadManager.Instance.data.shrooms += SaveLoadManager.Instance.data.multiplier;

                audioSource.PlayOneShot(clickSound);

                UIManager.Instance.UpdateShrooms(SaveLoadManager.Instance.data.shrooms);

                RegisterClick();
            }
        }
    }

    void RegisterClick()
    {
        float now = Time.time;

        clickTimes.Enqueue(now);

        while (clickTimes.Count > 0 && now - clickTimes.Peek() > cpsWindow)
            clickTimes.Dequeue();
    }

    void UpdateClickMode(float deltaTime)
    {
        float now = Time.time;

        while (clickTimes.Count > 0 && now - clickTimes.Peek() > cpsWindow)
            clickTimes.Dequeue();

        float currentCPS = clickTimes.Count / cpsWindow;

        if (currentCPS >= cpsThreshold)
        {
            clickModeActive = true;
            clickModeTimer = 0f;

            BoxCollider2D shroomCollider = GetComponent<BoxCollider2D>();
            shroomCollider.size = shroomColClickMode;

            mainCanvas.GetComponent<GraphicRaycaster>().enabled = false;
        }
        else if (clickModeActive)
        {
            clickModeTimer += deltaTime;

            if (clickModeTimer >= deactivateDelay)
            {
                clickModeActive = false;
                clickModeTimer = 0f;

                BoxCollider2D shroomCollider = GetComponent<BoxCollider2D>();
                shroomCollider.size = shroomColNormal;

                mainCanvas.GetComponent<GraphicRaycaster>().enabled = true;
            }
        }
    }
}
