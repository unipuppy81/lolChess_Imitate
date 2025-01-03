using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class ItemFrame : ObjectPoolable
{
    [SerializeField] private MeshRenderer mesh;
    [SerializeField] private Sprite itemImge;
    [SerializeField] private ItemBlueprint itemBlueprint;


    #region Property
    public ItemBlueprint ItemBlueprint => itemBlueprint;
    #endregion


    public void Init(ItemBlueprint item)
    {
        if(item == null)
        {
            Debug.Log("item Null");
        }

        mesh = GetComponent<MeshRenderer>();
        mesh.material = item.Material;
        itemImge = item.Icon;
        itemBlueprint = item;
    }

    #region UI
    public void ShowItemInfoUI()
    {
        var itemInfoUIPopup = Manager.UI.ShowPopup<UIPopupItemDetail>();

        Vector2 mousePosition = Input.mousePosition;
        itemInfoUIPopup.SetPosition(mousePosition);

        UIPopupItemDetail uiItemDetail = itemInfoUIPopup.GetComponent<UIPopupItemDetail>();
        uiItemDetail.SetItemData(itemBlueprint);
    }

    public void HideItemInfoUI()
    {
        Manager.UI.ClosePopup();
    }
    #endregion
}
