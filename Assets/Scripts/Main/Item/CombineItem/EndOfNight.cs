using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� ���ڶ�
/// </summary>
public class EndOfNight : BaseItem
{
    private bool useSkill;
    private ItemAttribute itemAttribute = new ItemAttribute();

    public override void InitItemSkill()
    {
        foreach (ItemAttribute iAttribute in ItemAttributes)
        {
            if (iAttribute.ItemAttributeType == ItemAttributeType.AD_Speed)
            {
                itemAttribute = iAttribute;
            }
        }
    }

    public override void InitTargetObject(GameObject targetChampion)
    {
        if (targetChampion == null)
            return;

        InitItemSkill();

 
        if(!useSkill)
            CheckChampionHp();
    }

    public override void ResetItem()
    {
        Debug.Log("Reset Item");
        useSkill = false;
        itemAttribute.AttributeValue = 0;
    }

    

    private void CheckChampionHp()
    {
        Debug.Log($"Use Skill : {useSkill}");

        if (EquipChampion == null || EquipChmpionBase == null)
        {
            return;
        }
            

        if (EquipChmpionBase.Display_CurHp / EquipChmpionBase.Display_MaxHp <= 0.6f)
        {
            itemAttribute.AttributeValue = 0.15f;
            CoroutineHelper.StartCoroutine(ChangeTagTemporarily(EquipChmpionBase.gameObject, "CantSelectChampion", "Champion", 1f));

        }
    }

    private IEnumerator ChangeTagTemporarily(GameObject equipChampion, string temporaryTag, string originalTag, float delay)
    {
        equipChampion.tag = temporaryTag;
        
        yield return new WaitForSeconds(delay);

        equipChampion.tag = originalTag;
        useSkill = true;

        Debug.Log($"Use Skill End : {useSkill}");
    }
}