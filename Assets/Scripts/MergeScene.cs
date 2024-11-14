using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeScene : MonoBehaviour
{
    /// <summary>
    /// 게임 시작 변수
    /// </summary>
    public static bool GameStart;

    /// <summary>
    /// 전투 시작 변수
    /// </summary>
    public static bool BatteStart;



    [SerializeField] private UISceneMain mainScene;
    [SerializeField] private GameDataBlueprint gameDataBlueprint;
    [SerializeField] private SymbolDataBlueprint symbolDataBlueprint;
    [SerializeField] private AugmenterBlueprint augmenterBlueprint;
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] 
    public int Level = 1;

    private void Start()
    {
        Manager.Asset.LoadAllAsync((count, totalCount) =>
        {
            GameStart = false;

            if (count >= totalCount)
            {
                gameDataBlueprint = Manager.Asset.GetBlueprint("GameDataBlueprint") as GameDataBlueprint;
                symbolDataBlueprint = Manager.Asset.GetBlueprint("SymbolDataBlueprint") as SymbolDataBlueprint;
                augmenterBlueprint = Manager.Asset.GetBlueprint("AugmenterBlueprint") as AugmenterBlueprint;
                GameStart = true;
                BatteStart = false;

                Manager.Game.InitGameManager();
                Manager.User.Init();

                mainScene.InitPanel(gameDataBlueprint, symbolDataBlueprint);
                mapGenerator.InitMapGenerator(gameDataBlueprint);

                Manager.User.InitMap(mapGenerator);
                Manager.Champion.Init(gameDataBlueprint);

                Manager.Stage.InitStage(Manager.Game.PlayerListObject, mapGenerator, gameDataBlueprint);
                Manager.UserHp.InitializeHealthBars();
                
                Manager.Item.Init();
                Manager.Synerge.Init(symbolDataBlueprint);

            }
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            //Manager.Item.CreateItem("B020", new Vector3(0, 0, 0));
            AugmenterData aData = augmenterBlueprint.GetAugmentByName("삼총사");
            Manager.Augmenter.SetAugmenter(Manager.User.GetHumanUserData(), aData);
        }
        else if(Input.GetKeyDown(KeyCode.X)) 
        {
            ChampionBlueprint cBlueprint = Manager.Asset.GetBlueprint("ChampionBlueprint_Seraphine") as ChampionBlueprint;

            GameObject newChampionObject = Manager.Asset.InstantiatePrefab(cBlueprint.ChampionInstantiateName);
            newChampionObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            newChampionObject.tag = "Enemy";

            GameObject frame = Manager.Asset.InstantiatePrefab("ChampionFrame");

            frame.transform.SetParent(newChampionObject.transform, false);
            newChampionObject.transform.position = new Vector3(0, 5, 0);

            ChampionBase cBase = newChampionObject.GetComponent<ChampionBase>();
            ChampionFrame cFrame = frame.GetComponentInChildren<ChampionFrame>();

            Player player = Manager.Game.PlayerListObject[0].GetComponent<Player>();
            cBase.SetChampion(cBlueprint, player);
            cBase.InitChampion(cFrame);
        }
        else if(Input.GetKeyDown(KeyCode.N)) //전투 시작 
        {
            Manager.Synerge.ApplySynergy(Manager.User.GetHumanUserData());
        }
        else if(Input.GetKeyDown(KeyCode.M)) // 전투 종료
        {
            Manager.Synerge.UnApplySynergy(Manager.User.GetHumanUserData());

            foreach(var champion in Manager.User.GetHumanUserData().BattleChampionObject)
            {
                ChampionBase cBase = champion.GetComponent<ChampionBase>();

                if(cBase != null)
                {
                    cBase.ResetChampionStats();
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.J)) // 증강 시작
        {
            Manager.Augmenter.ApplyFirstAugmenter(Manager.User.GetHumanUserData());
            Manager.Augmenter.ApplyStartRoundAugmenter(Manager.User.GetHumanUserData());
        }
        else if (Input.GetKeyDown(KeyCode.K)) // 증강 종료
        {
            Manager.Augmenter.ApplyEndRoundAugmenter(Manager.User.GetHumanUserData());

            foreach(var obj in Manager.User.GetHumanUserData().TotalChampionObject)
            {
                ChampionBase cBase = obj.GetComponent<ChampionBase>();
                cBase.ResetChampionStats();
            }
        }
        else if (Input.GetKeyDown(KeyCode.L)) // 증강 원할때
        {
            Manager.Augmenter.ApplyWheneverAugmenter(Manager.User.GetHumanUserData());

        }
    }
}
