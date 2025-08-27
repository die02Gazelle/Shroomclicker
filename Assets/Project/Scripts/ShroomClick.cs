using ShroomClicker;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShroomClick : MonoBehaviour
{
    public TextMeshProUGUI shroomCountText;

    public AudioSource audioSource;
    public AudioClip clickSound;

    private ShroomData data;

    public void InitWithData(ShroomData initData)
    {
        data = initData;
    }

    private void Update()
    {
        // Mouse handling for PC
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            CheckClick(pos);
        }

        // Touch handling for Mobile
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    Vector2 pos = Camera.main.ScreenToWorldPoint(touch.position.ReadValue());
                    CheckClick(pos);
                }
            }

        }
    }

    void CheckClick(Vector2 pos)
    {
        Collider2D col = Physics2D.OverlapPoint(pos);
        if (col != null && col.gameObject == gameObject)
        {
            SaveLoadManager.Instance.data.shrooms += SaveLoadManager.Instance.data.multiplier;

            audioSource.PlayOneShot(clickSound);

            UIManager.Instance.UpdateShrooms(SaveLoadManager.Instance.data.shrooms);
        }
    }

}
