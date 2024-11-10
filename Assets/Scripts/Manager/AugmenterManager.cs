using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmenterManager
{
    #region ���� �� ������Ƽ

    private AugmenterBlueprint augmenterBlueprint;


    public AugmenterBlueprint AugmenterBlueprint => augmenterBlueprint;
    #endregion


    #region �ʱ�ȭ ����

    public void Init(AugmenterBlueprint augmenterBlueprint) 
    {
        this.augmenterBlueprint = augmenterBlueprint;
    }

    #endregion

    #region ���� ���� �� ���� ����
    public List<AugmenterData> GetSilverAugmenters()
    {
        return augmenterBlueprint.GetRandomSilverAugments();
    }
    public List<AugmenterData> GetGoldAugmenters()
    {
        return augmenterBlueprint.GetRandomGoldAugments();
    }
    public List<AugmenterData> GetPlatinumAugmenters()
    {
        return augmenterBlueprint.GetRandomPlatinumAugments();
    }

    public void SetAugmenter(UserData user, AugmenterData augData)
    {
        user.UserAugmenter.Add(augData);
    }
    #endregion

    #region ���� ���� ����

    public void ApplySelectAugmenter(UserData user)
    {
        foreach(var aug in user.UserAugmenter)
        {
            Debug.Log($"UserAugmenter Count : {user.UserAugmenter.Count}");
            aug.BaseAugmenter.ApplyNow(user);
        }
    }


    public void ApplyStartRoundAugmenter(UserData user)
    {
        foreach (var aug in user.UserAugmenter)
        {
            aug.BaseAugmenter.ApplyStartRound(user);
        }
    }

    public void ApplyEndRoundAugmenter(UserData user)
    {
        foreach (var aug in user.UserAugmenter)
        {
            aug.BaseAugmenter.ApplyEndRound(user);
        }
    }
    #endregion
}