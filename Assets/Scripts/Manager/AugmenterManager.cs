using System.Collections.Generic;

public class AugmenterManager
{
    #region 변수 및 프로퍼티

    private AugmenterBlueprint augmenterBlueprint;

    public AugmenterBlueprint AugmenterBlueprint => augmenterBlueprint;
    #endregion


    #region 초기화 로직

    public void Init(AugmenterBlueprint augmenterBlueprint) 
    {
        this.augmenterBlueprint = augmenterBlueprint;
    }

    #endregion

    #region 증강 선택 및 세팅 로직
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

    public void SetAugmenter(UserData user, AugmenterData augData, bool closePopup = true)
    {
        user.UserAugmenter.Add(augData);

        if (closePopup && Manager.UI.CheckPopupStack())
            Manager.UI.CloseAllPopupUI();

        user.MapInfo.PlayerAugBox.GetComponent<AugmenterCube>().Init(user.UserAugmenter);
    }
    #endregion

    #region 증강 적용 로직

    public void ApplyFirstAugmenter(UserData user, BaseAugmenter aug)
    {
        aug.ApplyNow(user);
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
    public void ApplyWheneverAugmenter(UserData user)
    {
        foreach (var aug in user.UserAugmenter)
        {
            aug.BaseAugmenter.ApplyWhenever(user);
        }
    }

    public void ApplyRerollAugmenter(UserData user) 
    {
        foreach (var aug in user.UserAugmenter)
        {
            aug.BaseAugmenter.ApplyReroll(user);
        }
    }

    public void ApplyLevelUpAugmenter(UserData user)
    {
        foreach (var aug in user.UserAugmenter)
        {
            aug.BaseAugmenter.ApplyLevelUp(user);
        }
    }
    #endregion
}
