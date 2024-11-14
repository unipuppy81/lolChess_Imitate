using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MapGenerator;

public class AIPlayer
{
    private Player aiPlayerComponent; // AI 플레이어의 Player 컴포넌트
    private UserData aiUserData;      // AI 플레이어의 UserData
    private GameDataBlueprint gameDataBlueprint;


    public Player AiPlayerComponent => aiPlayerComponent;

    public void InitAIPlayer(Player player, GameDataBlueprint gameData)
    {
        aiPlayerComponent = player;
        aiUserData = player.UserData;
        gameDataBlueprint = gameData;
    }


    public void PerformActions(Player aiPlayer)
    {
        BuyChampions(aiPlayer); //챔피언 구매
        // 필요에 따라 추가 행동 (레벨업, 리롤 등)
        DecideAndPlaceChampions(); // 챔피언 배치
    }

    #region 챔피언구매로직
    private void BuyChampions(Player aiPlayer)
    {
        //int level = aiUserData.Level;

        // 챔피언 구매 로직
        List<string> shopChampionList = GetRandomChampions(1);
        string selectedChampionName = shopChampionList[Random.Range(0, shopChampionList.Count)];
        
        // 챔피언 블루프린트 가져오기
        ChampionBlueprint championBlueprint = Manager.Asset.GetBlueprint(selectedChampionName) as ChampionBlueprint;

        InstantiateAIChampion(championBlueprint, aiPlayer);

        /* 골드 체크 후 구매
        int championCost = championBlueprint.ChampionCost;
        if (aiUserData.GetGold() >= championCost)
        {
            aiUserData.SetGold(aiUserData.GetGold() - championCost);

            // 챔피언 인스턴스 생성 및 배치
            InstantiateAIChampion(championBlueprint);
        }*/
    }

    private void InstantiateAIChampion(ChampionBlueprint cBlueprint, Player aiPlayer)
    {
        // AI 플레이어의 맵 정보를 가져옵니다.
        MapGenerator.MapInfo aiMapInfo = aiUserData.MapInfo;

        if (aiMapInfo == null)
        {
            Debug.LogWarning($"AI 플레이어 {aiUserData.UserName}의 MapInfo를 찾을 수 없습니다.");
            return;
        }

        // 빈 타일 찾기
        HexTile emptyTile = FindEmptyRectTile(aiMapInfo);

        if (emptyTile != null)
        {
            GameObject newChampionObject = Manager.Asset.InstantiatePrefab(cBlueprint.ChampionInstantiateName);
            newChampionObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            GameObject frame = Manager.Asset.InstantiatePrefab("ChampionFrame");
            frame.transform.SetParent(newChampionObject.transform, false);
            newChampionObject.transform.position = emptyTile.transform.position + new Vector3(0, 0.5f, 0);

            newChampionObject.transform.SetParent(emptyTile.transform);
            emptyTile.championOnTile.Add(newChampionObject);

            ChampionBase cBase = newChampionObject.GetComponent<ChampionBase>();
            ChampionFrame cFrame = frame.GetComponentInChildren<ChampionFrame>();

            cBase.SetChampion(cBlueprint, aiPlayer);
            cBase.InitChampion(cFrame);

            //Manager.Champion.SettingNonBattleChampion(Manager.User.User1_Data);

            // AI 플레이어의 챔피언 리스트에 추가
            //aiUserData.NonBattleChampionObject.Add(newChampionObject);
            Manager.Champion.SettingNonBattleChampion(aiUserData);
        }
        else
        {
            Debug.LogWarning($"AI 플레이어 {aiUserData.UserName}의 맵에 빈 타일이 없습니다.");
        }
    }

    private HexTile FindEmptyRectTile(MapGenerator.MapInfo mapInfo)
    {
        foreach (var tileEntry in mapInfo.RectDictionary)
        {
            HexTile tile = tileEntry.Value;
            if (!tile.isOccupied)
            {
                return tile;
            }
        }
        return null;
    }

    // 챔피언 선택 로직 (기존 UIShopPanel에서 가져온 메서드 활용)
    private List<string> GetRandomChampions(int level)
    {
        List<string> selectedChampions = new List<string>();

        ChampionRandomData currentData = gameDataBlueprint.ChampionRandomDataList[level - 1];

        for (int i = 0; i < 5; i++)
        {
            int costIndex = GetCostIndex(currentData.Probability);
            ChampionData costChampionData = GetChampionDataByCost(costIndex + 1);
            if (costChampionData != null && costChampionData.Names.Length > 0)
            {
                string selectedChampion = costChampionData.Names[Random.Range(0, costChampionData.Names.Length)];
                selectedChampions.Add(selectedChampion);
            }
        }

        return selectedChampions;
    }

    private int GetCostIndex(float[] probabilities)
    {
        float[] cumulativeProbabilities = new float[probabilities.Length];
        cumulativeProbabilities[0] = probabilities[0];

        for (int i = 1; i < probabilities.Length; i++)
        {
            cumulativeProbabilities[i] = cumulativeProbabilities[i - 1] + probabilities[i];
        }

        float randomValue = Random.Range(0f, 1f);

        for (int i = 0; i < cumulativeProbabilities.Length; i++)
        {
            if (randomValue < cumulativeProbabilities[i])
            {
                return i;
            }
        }

        return probabilities.Length - 1;
    }

    private ChampionData GetChampionDataByCost(int cost)
    {
        foreach (ChampionData data in gameDataBlueprint.ChampionDataList)
        {
            if (data.Cost == cost)
            {
                return data;
            }
        }
        return null;
    }
    #endregion

    #region 챔피언 배치 로직
    private void DecideAndPlaceChampions()
    {
        // 현재 배틀필드에 배치된 챔피언 수 확인
        int currentBattleChampions = aiUserData.BattleChampionObject.Count;

        // AI 플레이어의 레벨에 따른 최대 배치 가능 챔피언 수
        int maxBattleChampions = 1; //aiUserData.Level; // 임시로 6레벨 줌

        // 배치 가능한 슬롯 수 계산
        int availableSlots = maxBattleChampions - currentBattleChampions;

        if (availableSlots <= 0)
        {
            // 배치 가능한 슬롯이 없으면 반환
            return;
        }

        // RectTile에 있는 챔피언들 중에서 배치할 챔피언 선택
        List<GameObject> championsOnBench = new List<GameObject>(aiUserData.NonBattleChampionObject);

        // 배치 가능한 수만큼 챔피언 선택
        for (int i = 0; i < availableSlots && championsOnBench.Count > 0; i++)
        {
            // 랜덤 챔피언 선택
            int randomIndex = Random.Range(0, championsOnBench.Count);
            GameObject championToPlace = championsOnBench[randomIndex];

            PlaceChampionOnHexTile(championToPlace);
            championsOnBench.RemoveAt(randomIndex);
        }
    }

    private void PlaceChampionOnHexTile(GameObject champion)
    {
        // AI 플레이어의 맵 정보를 가져옵니다.
        MapGenerator.MapInfo aiMapInfo = aiUserData.MapInfo;

        if (aiMapInfo == null)
        {
            Debug.LogWarning($"AI 플레이어 {aiUserData.UserName}의 MapInfo를 찾을 수 없습니다.");
            return;
        }

        // 빈 HexTile 찾기
        HexTile emptyTile = FindEmptyHexTile(aiMapInfo);

        if (emptyTile != null)
        {
            // 챔피언의 현재 타일 정보 가져오기
            HexTile currentTile = champion.transform.parent.GetComponent<HexTile>();
            if (currentTile != null)
            {
                currentTile.championOnTile.Remove(champion);
            }

            // 챔피언의 위치와 부모를 업데이트
            champion.transform.position = emptyTile.transform.position + new Vector3(0, 0.5f, 0);
            champion.transform.SetParent(emptyTile.transform);

            // 타일 상태 업데이트
            emptyTile.championOnTile.Add(champion);

            // AI 플레이어의 리스트 업데이트
            //aiUserData.NonBattleChampionObject.Remove(champion);
            //aiUserData.BattleChampionObject.Add(champion);
            Manager.Champion.SettingBattleChampion(aiUserData);
        }
        else
        {
            Debug.LogWarning($"AI 플레이어 {aiUserData.UserName}의 HexTile에 빈 타일이 없습니다.");
        }
    }

    private HexTile FindEmptyHexTile(MapGenerator.MapInfo mapInfo)
    {
        List<HexTile> availableTiles = new List<HexTile>();

        // 조건에 맞는 타일들을 리스트에 추가합니다.
        foreach (var tileEntry in mapInfo.HexDictionary)
        {
            HexTile tile = tileEntry.Value;
            if (!tile.isOccupied && tile.CompareTag("PlayerTile"))
            {
                availableTiles.Add(tile);
            }
        }

        // 리스트가 비어있지 않으면 랜덤한 타일을 반환합니다.
        if (availableTiles.Count > 0)
        {
            int randomIndex = Random.Range(0, availableTiles.Count);
            return availableTiles[randomIndex];
        }

        // 조건에 맞는 타일이 없으면 null을 반환합니다.
        return null;
    }
    #endregion

    #region 챔피언 슬롯 리롤 로직
    private void ReRollChampions(Player aiPlayer)
    {
        // 챔피언 슬롯 리롤 비용: 2골드
        int rerollCost = 2;

        if (aiUserData.UserGold >= rerollCost)
        {
            // 골드 소모
            aiUserData.UserGold -= rerollCost;
            //UIManager.Instance.UpdateUserGoldUI(aiUserData); // UI 업데이트 (필요 시)

            // 현재 챔피언 슬롯 비우기
            foreach (var champion in aiUserData.BattleChampionObject)
            {
                HexTile currentTile = champion.transform.parent.GetComponent<HexTile>();
                if (currentTile != null)
                {
                    currentTile.championOnTile.Remove(champion);
                    // 챔피언을 벤치로 이동
                    aiUserData.NonBattleChampionObject.Add(champion);
                    champion.transform.SetParent(null);
                }
            }
            aiUserData.BattleChampionObject.Clear();

            // 새로운 챔피언 리스트 생성
            List<string> newShopChampionList = GetRandomChampions(aiUserData.UserLevel);

            // 새로운 챔피언들을 슬롯에 배치
            foreach (var championName in newShopChampionList)
            {
                ChampionBlueprint championBlueprint = Manager.Asset.GetBlueprint(championName) as ChampionBlueprint;
                BuyChampions(aiPlayer); // 새로운 챔피언 구매 및 배치
            }

            Debug.Log($"AI {aiUserData.UserName}가 2 골드를 사용하여 챔피언 슬롯을 리롤했습니다. 남은 골드: {aiUserData.UserGold}");
        }
        else
        {
            Debug.Log($"AI {aiUserData.UserName}에게는 챔피언 슬롯을 리롤할 충분한 골드가 없습니다.");
        }
    }
    #endregion

    #region 경험치 구매 로직
    private void BuyExperience(Player aiPlayer)
    {
        // 경험치 구매 비용: 4골드당 4 EXP
        int experienceCost = 4;
        int experienceAmount = 4;

        if (aiUserData.UserGold >= experienceCost)
        {
            // 골드 소모
            aiUserData.UserGold -= experienceCost;
            //UIManager.Instance.UpdateUserGoldUI(aiUserData); // UI 업데이트 (필요 시)

            // 경험치 추가
            Manager.Level.AddExperience(aiUserData, experienceAmount);
            // 또는 LevelManager.Instance.AddExperience(aiUserData, experienceAmount);

            Debug.Log($"AI {aiUserData.UserName}가 {experienceCost} 골드를 사용하여 {experienceAmount} EXP를 구매했습니다. 남은 골드: {aiUserData.UserGold}");
        }
        else
        {
            Debug.Log($"AI {aiUserData.UserName}에게는 경험치를 구매할 충분한 골드가 없습니다.");
        }
    }
    #endregion

    #region AI 행동 우선순위 및 조건 설정
    /*private bool ShouldBuyExperience()
    {
        // 다음 레벨로의 경험치가 일정 이하일 때 구매
        int nextLevelExp = Manager.Level.GetNextLevelExp(aiUserData);
        return aiUserData.UserExp + 4 >= nextLevelExp; // 4 EXP를 추가할 때 레벨업이 될 경우
    }*/

    private bool ShouldReRollChampions()
    {
        // 현재 보유한 시너지의 개수가 원하는 수준보다 낮을 때
        int currentSynergyCount = GetCurrentSynergyCount();
        return currentSynergyCount < desiredSynergyThreshold && aiUserData.UserGold >= 2;
    }

    private int GetCurrentSynergyCount()
    {
        // 현재 AI의 챔피언 슬롯에 있는 시너지의 개수를 반환
        // 예시로 단순히 현재 시너지 수를 반환하도록 구현
        return aiUserData.ChampionSynergies_Line.Count + aiUserData.ChampionSynergies_Job.Count;
    }

    private int desiredSynergyThreshold = 3; // 원하는 시너지 수 설정
    #endregion

}
