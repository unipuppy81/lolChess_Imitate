using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour
{
    private Camera mainCamera;
    private ItemFrame draggedItem;
    private Vector3 offset;


    #region Unity Flow
    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }

        if (draggedItem != null)
        {
            HandleDragging();
            if (Input.GetMouseButtonUp(0))
            {
                HandleDrop();
            }
        }
    }


    #endregion

    #region Item Click Logic
    private void HandleLeftClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject clickedObject = hit.collider.gameObject;

            if (clickedObject.CompareTag("Item"))
            {
                draggedItem = clickedObject.GetComponent<ItemFrame>();
                offset = draggedItem.transform.position - GetMouseWorldPosition();
            }
            else
            {
                if (draggedItem != null)
                {
                    HandleDrop();
                }
            }
        }
    }

    private void HandleDragging()
    {
        if (draggedItem != null)
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition() + offset;
            draggedItem.transform.position = new Vector3(mouseWorldPos.x, draggedItem.transform.position.y, mouseWorldPos.z);
        }
    }

    private void HandleDrop()
    {
        if (draggedItem != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // 겹치는 아이템 확인
                Collider[] hitColliders = Physics.OverlapSphere(hit.point, 1f);

                foreach (Collider collider in hitColliders)
                {
                    if (collider.CompareTag("Item") && collider.transform != draggedItem.transform)
                    {
                        ItemFrame targetItemFrame = collider.GetComponent<ItemFrame>();

                        if (targetItemFrame != null)
                        {
                            string combinedItemName = Manager.Item.ItemCombine(draggedItem.ItemBlueprint.ItemId, targetItemFrame.ItemBlueprint.ItemId);

                            if (combinedItemName == "error")
                            {
                                Debug.Log("아이템 조합 실패");
                                return;
                            }

                            // 조합 성공 시 아이템 생성
                            Manager.Item.CreateItem(combinedItemName, new Vector3(0, 0, 0));

                            // 기존 아이템 파괴
                            Destroy(targetItemFrame.gameObject);
                            Destroy(draggedItem.gameObject);
                            break;
                        }
                    }
                }
            }

            // 드래그가 끝나면 null로 설정
            draggedItem = null;
        }
    }
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCamera.WorldToScreenPoint(Vector3.zero).z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }
    #endregion
}
