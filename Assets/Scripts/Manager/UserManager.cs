using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserManager
{
    public UserData User1_Data;
    private List<UserData> userDatas; 

    public void Init()
    {
        userDatas = new List<UserData>();

        GameObject obj = GameObject.Find("User");

        if (obj == null)
            return;

        
        for(int i = 1; i<= 8; i++)
        {
            GameObject newPlayer = Manager.Asset.InstantiatePrefab("Player", obj.transform);
            PlayerData pData = Manager.Asset.GetBlueprint($"PlayerData_{i}") as PlayerData;
            Player player = newPlayer.GetComponent<Player>();
            player.PlayerData = pData;

            Manager.Game.PlayerListObject[i - 1] = newPlayer;
        }
        


        for (int i = 0; i < obj.transform.childCount; i++)
        {
            UserData user = new UserData();
            user.InitUserData(10, "박태영", 0);
            Transform child = obj.transform.GetChild(i);
            Player player = child.GetComponent<Player>();

            player.InitPlayer(user);
            userDatas.Add(user);
        }

        User1_Data = new UserData();
        User1_Data.InitUserData(10, "박태영", 0);
    }

    public bool CheckChamipon(UserData user, string championName)
    {
        return user.BattleChampionObject.Any(championObject =>
        {
            if (championObject == null)
                return false;

            ChampionBase championBase = championObject.GetComponent<ChampionBase>();
            return championBase != null && championBase.ChampionName == championName;
        });
    }

    public UserData GetUserData()
    {
        return User1_Data;
    }

    public void ClearSynergy(UserData user)
    {
        user.ChampionSynergies_Line.Clear();
        user.ChampionSynergies_Job.Clear();
        user.ChampionSynergies.Clear();
    }

    public void InitMap(MapGenerator mapGenerator)
    {
        foreach(var user in userDatas)
        {
            user.MapInfo = mapGenerator.mapInfos.FirstOrDefault(mapInfo => mapInfo.mapId == user.UserId);
            user.SugarCraftPosition = user.MapInfo.SugarcraftPosition;
            user.PortalPosition = user.MapInfo.PortalPosition;
        }
        User1_Data.MapInfo = mapGenerator.mapInfos.FirstOrDefault(mapInfo => mapInfo.mapId == User1_Data.UserId);
        User1_Data.SugarCraftPosition = User1_Data.MapInfo.SugarcraftPosition;
        User1_Data.PortalPosition = User1_Data.MapInfo.PortalPosition;

    }
}


[System.Serializable]
public class UserData
{
    [SerializeField] private int gold;
    [SerializeField] private string userName;
    [SerializeField] private int userHealth;
    [SerializeField] private int userId;
    [SerializeField] private List<GameObject> totalChampionObject;
    [SerializeField] private List<GameObject> battleChampionObject;
    [SerializeField] private List<GameObject> nonBattleChamiponObject;
    [SerializeField] private List<ItemBlueprint> totalItemBlueprint;
    [SerializeField] private List<UserSynergyData> userSynergyData;
    [SerializeField] private MapGenerator.MapInfo mapInfo;
    [SerializeField] private List<Transform> sugarCraftPosition;
    [SerializeField] private List<Transform> portalPosition;
    [SerializeField] private PlayerType playerType;
    [SerializeField] private List<AugmenterData> userAugmenter;


    private Dictionary<string, int> synergies_Line;
    private Dictionary<string, int> synergies_Job;
    private Dictionary<string, int> totalSynergies;
    private Dictionary<string, HashSet<string>> championSynergies_Line;
    private Dictionary<string, HashSet<string>> championSynergies_Job;

    private Dictionary<string, SynergyData> championSynergies;


    #region Property

    public void SetGold(int gold)
    {
        this.gold = gold;
    }

    public int GetGold() { return gold; }

    public int UserHealth
    {
        get { return userHealth; }
        set { userHealth = value; }
    }

    public int UserId
    {
        get { return userId; }
        set { userId = value; } 
    }

    public string UserName
    {
        get { return userName; }
        set { userName = value; }
    }


    public List<GameObject> TotalChampionObject
    {
        get { return totalChampionObject; }
        set { totalChampionObject = value; }
    }
    public List<GameObject> BattleChampionObject
    {
        get { return battleChampionObject; }
        set { battleChampionObject = value; }
    }

    public List<GameObject> NonBattleChampionObject
    {
        get { return nonBattleChamiponObject; }
        set {  nonBattleChamiponObject = value; }
    }

    public List<UserSynergyData> UserSynergyData
    {
        get { return userSynergyData; }
        set { userSynergyData = value; }
    }

    public PlayerType PlayerType
    {
        get { return playerType; }
        set { playerType = value; }
    }

    public Dictionary<string, int> Synergies_Line
    {
        get { return synergies_Line; }
        set { synergies_Line = value; }
    }
    public Dictionary<string, int> Synergies_Job
    {
        get { return synergies_Job; }
        set { synergies_Job = value; }
    }

    public Dictionary<string, int> TotalSynergies
    {
        get { return totalSynergies; }
        set { totalSynergies = value; }
    }
    public Dictionary<string, HashSet<string>> ChampionSynergies_Line
    {
        get { return championSynergies_Line; }
        set { championSynergies_Line = value; }
    }

    public Dictionary<string, HashSet<string>> ChampionSynergies_Job
    {
        get { return championSynergies_Job; }
        set { championSynergies_Job = value; }
    }

    public Dictionary<string, SynergyData> ChampionSynergies
    {
        get { return championSynergies; }
        set {  championSynergies = value;}
    }
    public MapGenerator.MapInfo MapInfo
    { 
        get { return mapInfo; } 
        set {  mapInfo = value; }
    }

    public List<Transform> SugarCraftPosition
    {
        get { return sugarCraftPosition; }
        set {  sugarCraftPosition = value; }
    }

    public List<Transform> PortalPosition 
    { 
        get { return portalPosition; }
        set { portalPosition = value; }
    }


    public List<AugmenterData> UserAugmenter
    {
        get { return userAugmenter; }
        set { userAugmenter = value; }
    }


    #endregion

    #region Init
    public void InitUserData(int _gold, string _userName, int _userId)
    {
        totalChampionObject = new List<GameObject>();
        battleChampionObject = new List<GameObject>();
        nonBattleChamiponObject = new List<GameObject>();
        totalItemBlueprint = new List<ItemBlueprint>();
        synergies_Line = new Dictionary<string, int>();
        synergies_Job = new Dictionary<string, int>();
        championSynergies_Line = new Dictionary<string, HashSet<string>>();
        championSynergies_Job = new Dictionary<string, HashSet<string>>();
        championSynergies = new Dictionary<string, SynergyData>();
        sugarCraftPosition = new List<Transform>();
        portalPosition = new List<Transform>();
        userAugmenter = new List<AugmenterData>();

        gold = _gold;
        userName = _userName;
        userId = _userId;
        totalItemBlueprint.Clear();
        synergies_Line.Clear();
        
    }

    
    #endregion
    

}

[System.Serializable]
public class SynergyData
{
    public List<string> Lines { get; private set; }
    public List<string> Jobs { get; private set; }

    public SynergyData()
    {
        Lines = new List<string>();
        Jobs = new List<string>();
    }
}

[System.Serializable]
public class UserSynergyData
{
    [SerializeField] string synergyName;
    [SerializeField] int synergyCount;


    public string SynergyName
    {
        get { return synergyName; }
        set { synergyName = value; }
    }

    public int SynergyCount
    {
        get { return synergyCount; }
        set { synergyCount = value; }
    }
}
