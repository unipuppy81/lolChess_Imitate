using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChampionAttackController : MonoBehaviour
{
    #region 변수 & 프로퍼티
    private ChampionBase cBase;

    // State : Move
    private IEnumerator findCoroutine;
    private IEnumerator moveCoroutine;

    // State : Attack
    private IEnumerator attackCoroutine;

    [SerializeField] private GameObject targetChampion;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private bool isUseSkill;

    private float attack_Speed;
    private float attack_Range;

    public float realAttackRange;
    public Player EnemyPlayer;


    [SerializeField] private HexTile nextTile = new HexTile();
    [SerializeField] private HexTile curTile = new HexTile();




    [SerializeField] List<HexTile> path = new List<HexTile>();

    public bool IsAttack; // 전체 체크 용

    [SerializeField] private bool attackLogic;
    [SerializeField] private bool isAttackRange;
    public bool IsUseSkill()
    {
        return isUseSkill;
    }

    public bool CanAttack(GameObject target)
    {
        if(target == null) return false;

        return Vector3.Distance(gameObject.transform.position, target.transform.position) <= realAttackRange;
    }

    public GameObject TargetChampion => targetChampion;
    public List<HexTile> Path
    {
        get { return path; }
        set { path = value; }
    }

    #endregion

    #region 초기화
    public void Init(ChampionBase championBase, float _atk_Speed, float _atk_Range, float _curMana, float _maxMana)
    {
        cBase = championBase;
        attack_Speed = _atk_Speed;
        attack_Range = _atk_Range;

        realAttackRange = cBase.Attack_Range * 3f;

        IsAttack = false;
        attackLogic = false;
        isUseSkill = false;

    }

    public void EndBattle()
    {
        StopAllCoroutines();

        findCoroutine = null;
        moveCoroutine = null;
        attackCoroutine = null;

        EnemyPlayer = null;
        targetChampion = null;
        attackLogic = false;

        path.Clear();

        cBase.ChampionStateController.ChangeState(ChampionState.Idle, cBase);
    }
    #endregion

    #region 탐색 로직

    public void FindPathToTarget()
    {
        SetTargetEnemy();
    }

    private void SetTargetEnemy()
    {
        if (EnemyPlayer == null || EnemyPlayer.UserData.BattleChampionObject.Count <= 0)
        {
            Debug.Log("SetTargetEnemy: EnemyPlayer가 null이거나 적 챔피언이 없습니다.");
            return;
        }

        int aliveCount = 0;
        GameObject closestChampion = null;
        float minDistance = float.MaxValue;

        Vector3 currentPosition = transform.position;

        foreach (var champion in EnemyPlayer.UserData.BattleChampionObject)
        {
            if (champion == null)
                continue;

            ChampionBase cBase = champion.GetComponent<ChampionBase>();
            if (cBase.ChampionHpMpController.IsDie())
                continue; // 사망한 챔피언 무시

            aliveCount++;

            // 거리 계산
            float distance = Vector3.Distance(currentPosition, champion.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestChampion = champion;
            }
        }

        if (aliveCount == 0)
        {
            cBase.ChampionStateController.ChangeState(ChampionState.Idle, cBase); 
            return;
        }

        targetChampion = closestChampion;

        if (targetChampion == null)
        {
            cBase.ChampionStateController.ChangeState(ChampionState.Idle, cBase);
            return;
        }

        Manager.Stage.SetNearestTile(gameObject);
        curTile = Manager.Stage.GetParentTileInHex(gameObject);

        path = Manager.Stage.FindShortestPath(gameObject, targetChampion);

        StopAllCoroutines(); 
        StartCoroutine(StartMoveAndCheck());
    }
    #endregion

    #region 이동 로직

    private IEnumerator StartMoveAndCheck()
    {
        if (targetChampion == null)
            yield break;

        ChampionBase tcBase = targetChampion.GetComponent<ChampionBase>();

        if (tcBase.ChampionHpMpController.IsDie())
        {
            StopAllCoroutines();
            cBase.ChampionStateController.ChangeState(ChampionState.Idle, cBase);
            cBase.ChampionStateController.ChangeState(ChampionState.Move, cBase);
            yield break;
        }

        while (targetChampion != null && MergeScene.BatteStart && !tcBase.ChampionHpMpController.IsDie())
        {
            HexTile curTile = Manager.Stage.FindNearestTile(gameObject, cBase.BattleStageIndex);

            path = Manager.Stage.FindShortestPath(gameObject, targetChampion);

            if (path == null || path.Count == 0)
                yield break;

            nextTile = path[0];
            yield return StartCoroutine(MoveOneStepAlongPath(curTile));

            if (CanAttack(targetChampion))
            {
                cBase.ChampionStateController.ChangeState(ChampionState.Attack, cBase);
                yield break;
            }
        }
    }

   
    private IEnumerator MoveOneStepAlongPath(HexTile curTile)
    {
        if (path == null || path.Count == 0)
            yield break;

        nextTile = path[0];

        Vector3 targetPosition = nextTile.transform.position;

        curTile.championOnTile.Remove(gameObject);
        nextTile.championOnTile.Add(gameObject);
        gameObject.transform.SetParent(nextTile.transform);


        yield return StartCoroutine(MoveTo(targetPosition));

        float stoppingDistance = 0.1f; 

        if (Vector3.Distance(transform.position, targetPosition) <= stoppingDistance)
        {
 
            path.RemoveAt(0);
        }
    }

    private IEnumerator MoveTo(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, cBase.Champion_Speed  * 5f * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
        
    }
    #endregion

    #region 공격 로직

    public void AttackLogic()
    {
        if(!attackLogic)
        {
            attackCoroutine = AttackRoutine();
            StartCoroutine(attackCoroutine);
        }
    }

    public void AttackLogicStop()
    {
        if(attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    private IEnumerator AttackRoutine()
    {
        if (attackLogic)
            yield break;

        attackLogic = true;



        while (targetChampion != null)
        {
            ChampionBase tcBase = targetChampion.GetComponent<ChampionBase>();

            if (tcBase.ChampionHpMpController.IsDie())
            {
                cBase.ChampionStateController.ChangeState(ChampionState.Move, cBase);
                yield break;
            }

            Vector3 directionToTarget = (targetChampion.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

            while (Quaternion.Angle(transform.rotation, lookRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
                yield return null;
            }

            cBase.UpdateStatWithItem(cBase.EquipItem);


            if (cBase.ChampionHpMpController.IsManaFull())
            {
                Debug.Log("스킬 사용");
                cBase.ChampionHpMpController.UseSkillMana();
                //CoroutineHelper.StartCoroutine(UseSkillCoroutine());
            }
            else if (!cBase.ChampionHpMpController.IsManaFull())
            {
                CreateNormalAttack(targetChampion);
                cBase.ChampionHpMpController.NormalAttackMana();
            }

            yield return new WaitForSeconds(cBase.Champion_Atk_Spd);
        }

        attackLogic = false;
    }

    public void CreateNormalAttack(GameObject target)
    {
        if (target == null)
            return;

        if (cBase.Attack_Range > 1)
        {
            // 원거리 공격 - 발사체 생성
            isAttackRange = true;

            GameObject projectileObject = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            NormalProjectile projectile = projectileObject.GetComponent<NormalProjectile>();

            if (projectile != null)
            {
                projectile.SetTarget(target, ChampionDamageSet());
            }
        }
        else 
        {
            // 근접 공격
            isAttackRange = false;
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget <= 1.5f)
            {
                ChampionBase targetHealth = target.GetComponent<ChampionBase>();
                if (targetHealth != null)
                {
                    PlayMeleeAttackAnimation();
                    targetHealth.ChampionHpMpController.TakeDamage(ChampionDamageSet()); 
                }
            }
        }
    }
    private void PlayMeleeAttackAnimation()
    {
        // 근접 공격 애니메이션 재생 로직 (예: animator.SetTrigger("MeleeAttack"))
    }

    private int ChampionDamageSet()
    {
        return cBase.Champion_TotalDamage;
    }

    private IEnumerator UseSkillCoroutine()
    {
        Debug.Log(" 스킬 사용 ");
        isUseSkill = true;
        cBase.ChampionHpMpController.UseSkillMana();

        yield return new WaitForSeconds(3.0f);

        Debug.Log(" 스킬 끝 ");
        isUseSkill = false;
    }
    #endregion

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, realAttackRange);
    }
}
