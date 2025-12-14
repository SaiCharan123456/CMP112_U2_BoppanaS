using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject highlight; // Shows which slot is active

    public void SetIcon(Sprite icon)
    {
        if (icon != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    public void SetSelected(bool selected)
    {
        highlight.SetActive(selected);
    }

    public void Clear()
    {
        iconImage.enabled = false;
        highlight.SetActive(false);
    }

}
