using ChampionOwnedStates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MapGenerator;

public class Crip : MonoBehaviour
{
    [SerializeField] private HexTile currentTile;

    [SerializeField] private CripObjectData cripData;

    [SerializeField]private int currentHP;

    public Action<HexTile, HexTile> OnCurrentTileChanged;

    public HexTile CurrentTile
    {
        get => currentTile;
        set
        {
            if(currentTile != value)
            {
                OnCurrentTileChanged?.Invoke(currentTile, value);
                currentTile = value;
            }
        }
    }




    public MapGenerator.MapInfo PlayerMapInfo;
    public bool IsDie;
    [SerializeField] private HexTile targetTile;

    public Animator anim;
    //[SerializeField] private ItemTile itemtile;


    public int CurrentHp
    {
        get { return currentHP; }
        set { currentHP = value; }
    }


    public void InitCrip()
    {
        IsDie = false;
        currentHP = cripData.HP;
        anim = GetComponentInChildren<Animator>();
    }


    void Start()
    {
        IsDie = false;
        currentHP = cripData.HP;
        anim = GetComponentInChildren<Animator>();
    }



    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            IsDie = true;
            OnDeath();
            //Death();
        }
    }

    public void OnDeath()
    {
        //GenerateItem(); 아이템 생성
        GameObject obj = Manager.ObjectPool.GetGo("Capsule");
        obj.transform.position = this.transform.position + new Vector3(0, 1f, 0);
        Capsule cap = obj.GetComponent<Capsule>();

        int randomGold = UnityEngine.Random.Range(0, 7);

        string itemId = Manager.Item.NormalItem[UnityEngine.Random.Range(0, Manager.Item.NormalItem.Count)].ItemId;
        List<string> randomItems = new List<string>();
        randomItems.Add(itemId);

        List<string> championList = new List<string>();

        cap.InitCapsule(PlayerMapInfo.playerData.UserData, randomGold, randomItems, championList);

        if (PlayerMapInfo != null)
        {
            Player playerComponent = PlayerMapInfo.playerData;
            if (playerComponent != null)
            {
                playerComponent.UserData.CripObjectList.Remove(gameObject);
            }
        }
        HexTile targetTile = gameObject.GetComponent<CripMovement>().GetTargetTile();
        if (targetTile != null)
        {
            targetTile.championOnTile.Remove(gameObject);
        }

        PlayAnimation("Die");
        Invoke("DestroyObj", 0.5f);

    }

    private void DestroyObj()
    {
        Destroy(gameObject);
    }

    public void Death()
    {
        //GenerateItem(); 아이템 생성

        if (PlayerMapInfo != null)
        {
            Player playerComponent = PlayerMapInfo.playerData;
            if (playerComponent != null)
            {
                playerComponent.UserData.CripObjectList.Remove(this.gameObject);
            }
        }
        HexTile targetTile = gameObject.GetComponent<CripMovement>().GetTargetTile();
        if(targetTile != null)
        {
            targetTile.championOnTile.Remove(this.gameObject);
            Debug.Log("크립제거");
        }

        PlayAnimation("Die");
        Invoke("DestroyObj", 0.5f);
    }

    public void PlayAnimation(string animationName)
    {
        if (anim != null)
        {
            anim.Play(animationName);
        }
        else
        {
            Debug.LogError("Animator is not assigned.");
        }
    }
    void GenerateItem()
    {
        // 크립이 속한 맵의 ItemTile을 찾습니다.
        ItemTile itemTile = FindItemTileInMap();

        if (itemTile != null)
        {
            itemTile.GenerateItem();
        }
        else
        {
            Debug.LogWarning("ItemTile을 찾을 수 없습니다.");
        }
    }

    ItemTile FindItemTileInMap()
    {
        if (PlayerMapInfo != null)
        {
            foreach (var itemTile in PlayerMapInfo.ItemTile)
            {
                if(itemTile.TileType1 == ItemOwner.Player)
                {
                    return itemTile;
                }
            }
            
        }
        return null;
    }
}
