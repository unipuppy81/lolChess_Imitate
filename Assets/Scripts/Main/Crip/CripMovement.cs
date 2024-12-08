using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MapGenerator;

public class CripMovement : MonoBehaviour
{
    public float moveInterval = 2f; // 이동 간격
    private float moveTimer;
    private MapGenerator.MapInfo playerMapInfo;

    [SerializeField]private HexTile targetTile;
    private Vector3 targetPosition;
    public float moveSpeed = 2f; // 이동 속도

    private float fixedYPosition; // 고정된 y-좌표

    // 이전 타일을 추적하기 위한 변수
    private HexTile lastTile;

    public Crip crip;

    void Start()
    {
        crip = GetComponent<Crip>();

        moveTimer = moveInterval;

        // 크립이 속한 맵 정보를 가져옵니다.
        playerMapInfo = gameObject.GetComponent<Crip>().PlayerMapInfo;
        crip.CurrentTile = GetCurrentTile();

        targetTile = null;

        fixedYPosition = transform.position.y;

        Vector3 startPosition = transform.position;
        startPosition.y = fixedYPosition;
        transform.position = startPosition;

        lastTile = crip.CurrentTile;

        transform.rotation = Quaternion.Euler(0,180,0);

        crip.OnCurrentTileChanged += OnTileChanged;
    }

    void Update()
    {
        if (targetTile == null)
        {
            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0f)
            {
                crip.PlayAnimation("Walk");
                MoveRandomly();
                moveTimer = moveInterval;
            }
        }
        else
        {
            // 현재 위치에서 타겟 위치로 이동
            //Vector3 currentPosition = transform.position;
            //Vector3 desiredPosition = Vector3.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);

            // y-좌표를 고정합니다.
            //desiredPosition.y = fixedYPosition;

            //transform.position = desiredPosition;

            // 이동 중에 위치 기반으로 타일을 감지하고 상태를 업데이트
            //pdateTileUnderCrip();
        }

        HexTile tile = GetTileUnderCrip();
        if(tile != null && crip.CurrentTile != tile && crip.CurrentTile != targetTile)
        {
            crip.CurrentTile = tile;
        }

    }

    private void MoveRandomly()
    { 
        if (playerMapInfo == null)
            return;

        // 모든 비어있는 타일 가져오기
        List<HexTile> unoccupiedTiles = new List<HexTile>();
        foreach (var tileEntry in playerMapInfo.HexDictionary)
        {
            HexTile tile = tileEntry.Value;
            if (!tile.isOccupied && tile.championOnTile.Count == 0)
            {
                unoccupiedTiles.Add(tile);
            }
        }

        if (unoccupiedTiles.Count > 0)
        {
            HexTile nextTile;

            do
            {
                nextTile = unoccupiedTiles[Random.Range(0, unoccupiedTiles.Count)];
            }
            while (nextTile.championOnTile.Count > 0);

            // 목표 타일과 위치 설정
            targetTile = nextTile;

            // 목표 위치의 y-좌표를 고정합니다.
            Vector3 tilePosition = nextTile.transform.position;
            tilePosition.y = fixedYPosition;
            targetPosition = tilePosition;

            // 진행 방향 계산
            Vector3 direction = (targetPosition - transform.position).normalized;

            // 오브젝트를 방향으로 회전
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = targetRotation;
            }

            HexTile currentTile = GetTileUnderCrip();
            if (currentTile != null)
            {
                currentTile.championOnTile.Remove(gameObject);
            }

            nextTile.championOnTile.Add(gameObject);
            StartCoroutine(MoveTo(targetPosition));
        }
    }

    private IEnumerator MoveTo(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            // 위치 이동
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * moveSpeed);
            yield return null;
        }

        // 도착 후 정확한 위치로 보정
        transform.position = targetPosition;
    }

    void UpdateTileUnderCrip()
    {
        HexTile hitTile = GetTileUnderCrip();
        if (hitTile != null)
        {
            // 현재 타일과 다르다면
            if (hitTile != crip.CurrentTile)
            {
                // 이전 타일에서 크립 제거
                if (crip.CurrentTile != null)
                {
                    crip.CurrentTile.championOnTile.Remove(this.gameObject);
                }

                // 새로운 타일에 크립 추가
                if (!hitTile.championOnTile.Contains(this.gameObject))
                {
                    hitTile.championOnTile.Add(this.gameObject);
                }

                // 현재 타일 업데이트
                crip.CurrentTile = hitTile;
                crip.CurrentTile = crip.CurrentTile;
                crip.transform.SetParent(crip.CurrentTile.transform);
            }
            // 현재 타일과 같다면 아무 작업도 하지 않음
        }
        else
        {
            // 아래에 타일이 감지되지 않는 경우, 현재 타일에서 크립 제거
            if (crip.CurrentTile != null)
            {
                crip.CurrentTile.championOnTile.Remove(this.gameObject);
                crip.CurrentTile = null;
            }
        }
    }

    HexTile GetTileUnderCrip()
    {
        if (playerMapInfo == null)
            return null;

        // 현재 위치에서 가장 가까운 타일 찾기
        float minDistance = float.MaxValue;
        HexTile nearestTile = null;
        foreach (var tileEntry in playerMapInfo.HexDictionary)
        {
            HexTile tile = tileEntry.Value;
            float dist = Vector3.Distance(transform.position, tile.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestTile = tile;
            }
        }
        return nearestTile;
    }

    public HexTile GetCurrentTile()
    {
        return GetTileUnderCrip();
    }
    public HexTile GetTargetTile()
    {
        return targetTile;
    }

    public void OnTileChanged(HexTile cur ,HexTile next)
    {
        if(cur != null)
        {
            cur.championOnTile.Remove(gameObject);
        }

        if(next != null)
        {
            next.championOnTile.Add(gameObject);
            gameObject.transform.SetParent(next.transform);
        }
    }

}
