using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public enum battleStates
    {
        INIT,
        WAIT,
        ACTION,
        END
    }
    public battleStates battleState;

    public enum battleRanges
    {
        CLOSE,
        MID,
        LONG,
        EXTREME
    }
    public battleRanges battleRange = new battleRanges();

    public BattleUiManager battleUiManager;
    public TechDatabase moveDatabase;
    public PlayerMovement playerMovement;
    public GameObject player1;
    public GameObject player2;
    private BattleStats p1BattleStats;
    private BattleStats p2BattleStats;
    public GameObject p1Monster;
    public GameObject p2Monster;
    private Animator p1Animator;
    private Animator p2Animator;
    private AnimatorController p1AnimController;
    private AnimatorController p2AnimController;
    

    public GameObject vCams;
    public List<GameObject> vCamsList;

    [Header("Game Modifiers")]
    public float randomnessMod = 0.05f;
    public float statLevelMod = 0.04f;
    public float stageMaxX = 10f;
    public float stageMinX = -10f;
    public float timer = 60;

    [Header("Game Logic")]
    private Vector3 player1AfterTechPos;
    private Vector3 player2AfterTechPos;
    private bool isTechHitting;
    private bool isP1Walking;
    private bool isP2Walking;


    private void Start()
    {
        //subscribe
        battleUiManager.onMoveButtonClicked += TechInitiated;
        playerMovement.onMoveRightPressed += MoveRight;
        playerMovement.onMoveLeftPressed += MoveLeft;
        playerMovement.onMoveRightReleased += CheckForWalkAnim;
        playerMovement.onMoveLeftReleased += CheckForWalkAnim;
        battleState = battleStates.INIT;

        p1BattleStats = player1.GetComponent<BattleStats>();
        p2BattleStats = player2.GetComponent<BattleStats>();

        //instantiate monsters
        var p1Mon = Instantiate(Resources.Load("Monster Prefabs/Bear") as GameObject, p1Monster.transform);
        p1Mon.transform.parent = p1Monster.transform;
        p1Animator = p1Mon.GetComponent<Animator>();
        p1Mon.AddComponent<AnimatorController>();
        p1AnimController = p1Mon.GetComponent<AnimatorController>();

        var p2Mon = Instantiate(Resources.Load("Monster Prefabs/Dino") as GameObject, p2Monster.transform);
        p2Mon.transform.parent = p2Monster.transform;
        p2Animator = p2Mon.GetComponent<Animator>();
        p2Mon.AddComponent<AnimatorController>();
        p2AnimController = p2Mon.GetComponent<AnimatorController>();

        BattleStateChanged();
        StartCoroutine(ReadyCountdown());
        
    }
    private void Update()
    {
        //hit animations
        CheckForHitAnimation();


        switch (battleState)
        {
            case battleStates.WAIT:
                CheckForBattleEnd();
                StartCoroutine(StaminaRegenCoroutine());
                LifeStaminaValueCaps();
                TimerCountdown();
                //CheckForWalkAnim();


                //Debug.Log(battleRange);
                if (battleRange != DetermineRange(player1.transform.position.x, player2.transform.position.x))
                {
                    battleRange = DetermineRange(player1.transform.position.x, player2.transform.position.x);
                    BattleRangeChanged();
                }
                break;

            case battleStates.ACTION:
                CheckForBattleEnd();
                break;
        }
    }

    public void CheckForWalkAnim()
    {
        if (isP1Walking)
        {
            isP1Walking = false;
        }
    }

    public void CheckForHitAnimation()
    {
        if(p1AnimController.isHittingEnemy)
        {
            if (isTechHitting)
            {
                p2Animator.SetTrigger("Hit Light");
                p1AnimController.isHittingEnemy = false;

                //sfx
                Vector3 hitPos = p2Monster.transform.position;
                hitPos.x = p2Monster.transform.position.x - p2Monster.transform.localScale.x;
                hitPos.y = p2Monster.transform.position.y + p2Monster.transform.localScale.y;

                var hitSfx = Instantiate(Resources.Load("Sfx/Hit Effect") as GameObject, hitPos, Quaternion.identity);
                Destroy(hitSfx, 5f);
            }
            else
            {
                p2Animator.SetTrigger("Evade");
                p1AnimController.isHittingEnemy = false;
            }
        }
        if (p2AnimController.isHittingEnemy)
        {
            if(isTechHitting)
            {
                p1Animator.SetTrigger("Hit Light");
                p2AnimController.isHittingEnemy = false;

                //sfx
                Vector3 hitPos = p1Monster.transform.position;
                hitPos.x = p1Monster.transform.position.x - p1Monster.transform.localScale.x;
                hitPos.y = p1Monster.transform.position.y + p1Monster.transform.localScale.y;

                var hitSfx = Instantiate(Resources.Load("Sfx/Hit Effect") as GameObject, hitPos, Quaternion.identity);
                Destroy(hitSfx, 5f);
            }
            else
            {
                p1Animator.SetTrigger("Evade");
                p2AnimController.isHittingEnemy = false;
            }
        }
    }

    IEnumerator ReadyCountdown()
    {
        var seconds = 2.5f;
        ShowMessage("Ready", seconds);
        yield return new WaitForSeconds(seconds);
        yield return ReadyFight();
    }

    IEnumerator ReadyFight()
    {
        var seconds = 0.5f;
        ShowMessage("Fight", seconds);
        yield return new WaitForSeconds(seconds);
        battleState = battleStates.WAIT;
        BattleStateChanged();
    }

    public void TimerCountdown()
    {
        timer = timer - Time.deltaTime;
        TimerChanged(timer);

        if (timer <= 0)
        {
            timer = 0;
            battleState = battleStates.END;
            BattleStateChanged();

            StartCoroutine(TimerKnockout());
        }
    }

    IEnumerator TimerKnockout()
    {
        ShowRemainingLife();
        yield return new WaitForSeconds(3f);

        if (p2BattleStats.lifeCurrent > p1BattleStats.lifeCurrent)
        {
            ShowMessage("You Lose", 3);
            p1Animator.SetTrigger("Hit");
            p2Animator.SetTrigger("Backflip");
        }
        else if (p1BattleStats.lifeCurrent > p2BattleStats.lifeCurrent)
        {
            ShowMessage("You Win", 3);
            p1Animator.SetTrigger("Backflip");
            p2Animator.SetTrigger("Hit");
        }
    }

    public void CheckForBattleEnd()
    {
        if (p1BattleStats.lifeCurrent <= 0 || p2BattleStats.lifeCurrent <= 0)
        {
            StartCoroutine(KnockoutEnd());
        }


    }

    IEnumerator KnockoutEnd()
    {
        battleState = battleStates.END;
        BattleStateChanged();

        ShowMessage("K.O.", 3);
        yield return new WaitForSeconds(3f);
        if (p1BattleStats.lifeCurrent <= 0)
        {
            ShowMessage("You Lose", 3);
            p1Animator.SetTrigger("Dead");
            p2Animator.SetTrigger("Backflip");
        }
        else if (p2BattleStats.lifeCurrent <= 0)
        {
            ShowMessage("You Win", 3);
            p1Animator.SetTrigger("Backflip");
            p2Animator.SetTrigger("Dead");
        }
    }

    public void MoveRight()
    {
        if(battleState == battleStates.WAIT)
        {
            if((player1.transform.position.x < stageMaxX &&
                player1.transform.position.x < (player2.transform.position.x - player2.transform.localScale.x * 2f)))
            {
                player1.transform.Translate(Vector3.right * Time.deltaTime * 10);
                if(!isP1Walking)
                {
                    p1Animator.SetTrigger("Walk");
                    isP1Walking = true;
                }
            }
        }
    }
    public void MoveLeft()
    {
        if (battleState == battleStates.WAIT)
        {
            if(player1.transform.position.x > stageMinX)
            {
                player1.transform.Translate(Vector3.left * Time.deltaTime * 10);
                if (!isP1Walking)
                {
                    p1Animator.SetTrigger("Walk");
                    isP1Walking = true;
                }
            }
        }
    }

    public void LifeStaminaValueCaps()
    {
        if(p1BattleStats.lifeCurrent > p1BattleStats.lifeMax)
        {
            p1BattleStats.lifeCurrent = p1BattleStats.lifeMax;
        }
        if (p1BattleStats.lifeCurrent < 0)
        {
            p1BattleStats.lifeCurrent = 0;
        }

        if (p1BattleStats.staminaCurrent > p1BattleStats.staminaMax)
        {
            p1BattleStats.staminaCurrent = p1BattleStats.staminaMax;
        }
        if (p1BattleStats.staminaCurrent < 0)
        {
            p1BattleStats.staminaCurrent = 0;
        }

        if (p2BattleStats.lifeCurrent > p2BattleStats.lifeMax)
        {
            p2BattleStats.lifeCurrent = p2BattleStats.lifeMax;
        }
        if (p2BattleStats.lifeCurrent < 0)
        {
            p2BattleStats.lifeCurrent = 0;
        }

        if (p2BattleStats.staminaCurrent > p2BattleStats.staminaMax)
        {
            p2BattleStats.staminaCurrent = p2BattleStats.staminaMax;
        }
        if (p2BattleStats.staminaCurrent < 0)
        {
            p2BattleStats.staminaCurrent = 0;
        }
    }

    public void BattleCameraPickRandom()
    {
        if(vCamsList.Count <=0)
        {
            foreach (Transform child in vCams.transform)
            {
                vCamsList.Add(child.gameObject);
            }
        }

        int choice = UnityEngine.Random.Range(0, vCamsList.Count);
        foreach (var item in vCamsList)
        {
            item.SetActive(false);
        }
        vCamsList[choice].SetActive(true);
        //Debug.Log("Camera view" + choice);
    }

    public void PickBattleCamera(int choice)
    {
        if (vCamsList.Count <= 0)
        {
            foreach (Transform child in vCams.transform)
            {
                vCamsList.Add(child.gameObject);
            }
        }
        foreach (var item in vCamsList)
        {
            item.SetActive(false);
        }
        vCamsList[choice].SetActive(true);
    }

    public void TechInitiated(string move, GameObject attacker, GameObject defender)
    {
        //check state
        if (battleState == battleStates.WAIT)
        {
            StartCoroutine(TechStart(move, attacker, defender));
        }
    }

    IEnumerator TechStart(string move, GameObject attacker, GameObject defender)
    {
        Debug.Log("TechStart");
        //calculate everything
        BattleStats attackerStats = attacker.GetComponent<BattleStats>();
        BattleStats defenderStats = defender.GetComponent<BattleStats>();

        float totalLifeDamage = 0;
        float totalStaminaDamage = 0;
        float totalChanceToHit = 0;
        isTechHitting = false;

        Vector3 attackerTargetPos = attacker.transform.position;
        Vector3 defenderTargetPos = defender.transform.position;


        //check if stamina is enough
        if (battleUiManager.GetMoveCost(move) < attackerStats.staminaCurrent)
        {
            //state change
            battleState = battleStates.ACTION;
            BattleStateChanged();

            //camera switch
            BattleCameraPickRandom();
        }

        //subtract cost from attacker
        attackerStats.staminaCurrent -= GetMoveCost(move);

        //calculate total hit chance
        float accuracyBonus = 0;
        accuracyBonus = attackerStats.skill - defenderStats.speed;
        accuracyBonus = accuracyBonus / 50;
        accuracyBonus = accuracyBonus * statLevelMod * 100;
        totalChanceToHit = accuracyBonus + GetMoveHitChance(move);
        //add randomness
        totalChanceToHit = RandomFactor(totalChanceToHit);

        //create random chance to beat
        var chanceToBeat = UnityEngine.Random.Range(0, 100);

        //lock chances to 5% and 99%
        totalChanceToHit = Mathf.Clamp(totalChanceToHit, 1, 99);

        //check if move hits
        if (totalChanceToHit > chanceToBeat)
        {
            //move successful
            Debug.Log("HIT " + totalChanceToHit + " vs " + chanceToBeat);
            isTechHitting = true;

            //check if POW or INT move type
            if (GetMoveType(move) == "POW")
            {
                totalLifeDamage = CalculatePowDamage(move, attackerStats, defenderStats);
            }
            else if (GetMoveType(move) == "INT")
            {
                totalLifeDamage = CalculateIntDamage(move, attackerStats, defenderStats);
            }

            totalStaminaDamage = GetMoveStaminaDamage(move);

            //add randomness
            totalLifeDamage = RandomFactor(totalLifeDamage);
            totalStaminaDamage = RandomFactor(totalStaminaDamage);
        }
        else
        {
            //move missed
            Debug.Log("MISS " + totalChanceToHit + " vs " + chanceToBeat);
            isTechHitting = false;
        }

        yield return MoveToPositionsBeforeTech(move, attacker, defender);
        yield return AnimateTech(move, attacker, defender, totalChanceToHit);
        yield return ShowDamagesAfterTech(attacker, defender, totalLifeDamage, totalStaminaDamage, attackerStats, defenderStats);
        yield return MoveToPositionsAfterTech(attacker, defender, attackerTargetPos, defenderTargetPos);
        StartCoroutine(TechEnded());

        if (attacker.name == "Player 1")
        {
            p1Animator.SetTrigger("Ready");
        }
        else if (attacker.name == "Player 2")
        {
            p2Animator.SetTrigger("Ready");
        }
        Debug.Log("END");
    }

    IEnumerator MoveToPositionsBeforeTech(string move, GameObject attacker, GameObject defender)
    {
        Debug.Log("MoveToPositionsBeforeTech");
        yield return MoveToPositionsBeforeTechSetup(move, attacker, defender);
    }

    IEnumerator MoveToPositionsBeforeTechSetup(string move, GameObject attacker, GameObject defender)
    {
        var temp = defender.transform.position;

        if (attacker.name == "Player 1")
        {
            player1AfterTechPos = attacker.transform.position;
            player2AfterTechPos = defender.transform.position;

            temp.x = defender.transform.position.x - (defender.transform.localScale.x * 2f);

            p1Animator.SetTrigger("Walk");
        }
        else if (attacker.name == "Player 2")
        {
            player2AfterTechPos = attacker.transform.position;
            player1AfterTechPos = defender.transform.position;

            temp.x = defender.transform.position.x + (defender.transform.localScale.x * 2f);

            p2Animator.SetTrigger("Walk");
        }

        while (MoveToPositionsBeforeTechMover(attacker, temp))
        {
            yield return null;
        }
    }

    public bool MoveToPositionsBeforeTechMover(GameObject attacker, Vector3 defenderPos)
    {
        attacker.transform.position = Vector3.MoveTowards(attacker.transform.position, defenderPos,
            (Time.deltaTime * 10f * 3));
        
        return (attacker.transform.position != defenderPos);
    }

    IEnumerator AnimateTech(string move, GameObject attacker, GameObject defender, float totalChanceToHit)
    {
        Debug.Log("AnimateTech");

        ShowTechNameAndHit(attacker, move, totalChanceToHit);

        //attack animate
        if (attacker.name == "Player 1")
        {
            p1Animator.SetTrigger(move);

            p1AnimController.isAnimating = true;
            while (p1AnimController.isAnimating)
            {
                yield return null;

                if (p1Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
                {  //If normalizedTime is 0 to 1 means animation is playing, if greater than 1 means finished
                    p1AnimController.isAnimating = false;
                }
                else
                {
                    p1AnimController.isAnimating = true;
                }
            }


        }
        else if (attacker.name == "Player 2")
        {
            p2Animator.SetTrigger(move);

            p2AnimController.isAnimating = true;
            while (p2AnimController.isAnimating)
            {
                yield return null;

                if (p2Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
                {  //If normalizedTime is 0 to 1 means animation is playing, if greater than 1 means finished
                    p2AnimController.isAnimating = false;
                }
                else
                {
                    p2AnimController.isAnimating = true;
                }
            }


        }
    }

    IEnumerator ShowDamagesAfterTech(GameObject attacker, GameObject defender,
        float totalLifeDamage, float totalStaminaDamage, 
        BattleStats attackerStats, BattleStats defenderStats)
    {
        Debug.Log("ShowDamagesAfterTech");

        if(totalLifeDamage > 0 && totalStaminaDamage > 0)
        {
            //display damage
            AttackHit(defender, totalLifeDamage, totalStaminaDamage);

            LifeDamaged(defender, totalLifeDamage);
            StaminaDamaged(defender, totalStaminaDamage);
        }

        //apply damage
        defenderStats.lifeCurrent -= totalLifeDamage;
        defenderStats.staminaCurrent -= totalStaminaDamage;

        yield return null;
    }

    IEnumerator MoveToPositionsAfterTech(GameObject attacker, GameObject defender, Vector3 attackerTargetPos, Vector3 defenderTargetPos)
    {
        Debug.Log("MoveToPositionsAfterTech");
        if (attacker.name == "Player 1")
        {
            p1Animator.SetTrigger("Walk");
            if(!isTechHitting)
            {
                p2Animator.SetTrigger("Walk");
            }
        }
        else if (attacker.name == "Player 2")
        {
            p2Animator.SetTrigger("Walk");
            if (!isTechHitting)
            {
                p1Animator.SetTrigger("Walk");
            }
        }


        while (MoveToPositionsAfterTechMover(attacker, defender, attackerTargetPos, defenderTargetPos))
        {
            yield return null;
        }
         
    }
    public bool MoveToPositionsAfterTechMover(GameObject attacker, GameObject defender, Vector3 attackerTargetPos, Vector3 defenderTargetPos)
    {
        attacker.transform.position = Vector3.MoveTowards(attacker.transform.position, attackerTargetPos,
            (Time.deltaTime * 10f * 1));

        defender.transform.position = Vector3.MoveTowards(defender.transform.position, defenderTargetPos,
            (Time.deltaTime * 10f * 1));

        return (attacker.transform.position != attackerTargetPos); 
            //&& (defender.transform.position != defenderTargetPos);
    }

    IEnumerator TechEnded()
    {
        yield return new WaitForSeconds(1f);
        battleState = battleStates.WAIT;
        BattleStateChanged();
        PickBattleCamera(0);
        AttackFinished();
    }

    public float CalculatePowDamage(string move, BattleStats attacker, BattleStats defender)
    {
        float totalPowDamage = 0;
        float damageBonus = 0;
        //POWER TYPE
        //calculate damage
        damageBonus = attacker.power - defender.defense;
        damageBonus = damageBonus / 50;
        damageBonus = damageBonus * statLevelMod * GetMoveLifeDamage(move);
        
        totalPowDamage = damageBonus + GetMoveLifeDamage(move);
        Debug.Log("damageBonus " + damageBonus + "move " + GetMoveLifeDamage(move));

        //subtract damage from defender
        return totalPowDamage;
    }

    public float CalculateIntDamage(string move, BattleStats attacker, BattleStats defender)
    {
        float totalIntDamage = 0;
        float damageBonus = 0;
        //POWER TYPE
        //calculate damage
        damageBonus = attacker.intelligence - defender.resist;
        damageBonus = damageBonus / 50;
        damageBonus = damageBonus * statLevelMod * GetMoveLifeDamage(move);

        totalIntDamage = damageBonus + GetMoveLifeDamage(move);
        Debug.Log("damageBonus " + damageBonus + " move " + GetMoveLifeDamage(move));

        //subtract damage from defender
        return totalIntDamage;
    }

    IEnumerator WaitForActionToFinish()
    {
        yield return new WaitForSeconds(2f);
        battleState = battleStates.WAIT;
        BattleStateChanged();

        AttackFinished();
    }

    public float GetMoveHitChance(string move)
    {
        float returnValue = 0;
        foreach (var item in moveDatabase.techDatabase)
        {
            if (item.techName == move)
            {
                returnValue = item.hit;
            }
        }
        return returnValue;
    }

    public float GetMoveCost(string move)
    {
        float returnValue = 0;
        foreach (var item in moveDatabase.techDatabase)
        {
            if (item.techName == move)
            {
                returnValue = item.cost;
            }
        }
        return returnValue;
    }

    public float GetMoveLifeDamage(string move)
    {
        float returnValue = 0;
        foreach (var item in moveDatabase.techDatabase)
        {
            if (item.techName == move)
            {
                returnValue = item.lifeDamage;
            }
        }
        return returnValue;
    }
    public float GetMoveStaminaDamage(string move)
    {
        float returnValue = 0;
        foreach (var item in moveDatabase.techDatabase)
        {
            if (item.techName == move)
            {
                returnValue = item.staminaDamage;
            }
        }
        return returnValue;
    }

    public string GetMoveType(string move)
    {
        string returnValue = "";
        foreach (var item in moveDatabase.techDatabase)
        {
            if (item.techName == move)
            {
                returnValue = item.techDamage.ToString();
            }
        }
        return returnValue;
    }

    IEnumerator StaminaRegenCoroutine()
    {
        yield return new WaitForEndOfFrame();
        StaminaRegen(p1BattleStats);
        StaminaRegen(p2BattleStats);
    }
    public void StaminaRegen(BattleStats monster)
    {
        if(monster.staminaCurrent < monster.staminaMax)
        {
            float recovery = monster.staminaMax * 0.01f;
            monster.staminaCurrent += recovery * Time.deltaTime;

            if(monster.staminaCurrent >= monster.staminaMax)
            {
                monster.staminaCurrent = monster.staminaMax;
            }
        }
    }

    public float RandomFactor(float numberToAddRandom)
    {
        numberToAddRandom = (UnityEngine.Random.Range(-randomnessMod, randomnessMod) * numberToAddRandom) + numberToAddRandom;
        return numberToAddRandom;
    }

    public battleRanges DetermineRange(float player1Pos, float player2Pos)
    {
        battleRanges range = battleRanges.EXTREME;
        float distance = player2Pos - player1Pos;
        if(distance > 15)
        {
            range = battleRanges.EXTREME;
        }
        else if (distance > 10 && distance <= 15)
        {
            range = battleRanges.LONG;
        }
        else if (distance > 5 && distance <= 10)
        {
            range = battleRanges.MID;
        }
        else if (distance <= 5)
        {
            range = battleRanges.CLOSE;
        }
        return range;
    }


    public Action<GameObject, float> onLifeDamaged;
    public void LifeDamaged(GameObject defender, float damage)
    {
        if (onLifeDamaged != null)
        {
            onLifeDamaged(defender, damage);
        }
    }

    public Action<GameObject, float> onStaminaDamaged;
    public void StaminaDamaged(GameObject defender, float damage)
    {
        if (onStaminaDamaged != null)
        {
            onStaminaDamaged(defender, damage);
        }
    }

    public Action<GameObject, float, float> onAttackHit;
    public void AttackHit(GameObject defender, float lifeDamage, float staminaDamage)
    {
        if (onAttackHit != null)
        {
            onAttackHit(defender, lifeDamage, staminaDamage);
        }
    }

    public Action onAttackFinished;
    public void AttackFinished()
    {
        if (onAttackFinished != null)
        {
            onAttackFinished();
        }
    }

    public Action<battleRanges> onBattleRangeChanged;
    public void BattleRangeChanged()
    {
        if (onBattleRangeChanged != null)
        {
            onBattleRangeChanged(battleRange);
        }
    }

    public Action<battleStates> onBattleStateChanged;
    public void BattleStateChanged()
    {
        if (onBattleStateChanged != null)
        {
            onBattleStateChanged(battleState);
        }
    }

    public Action<string, float> onShowMessage;
    public void ShowMessage(string message, float duration)
    {
        if (onShowMessage != null)
        {
            onShowMessage(message, duration);
        }
    }

    public Action<float> onTimerChanged;
    public void TimerChanged(float timer)
    {
        if (onTimerChanged != null)
        {
            onTimerChanged(timer);
        }
    }

    public Action onBattleEnded;
    public void BattleEnded()
    {
        if (onBattleEnded != null)
        {
            onBattleEnded();
        }
    }

    public Action onShowRemainingLife;
    public void ShowRemainingLife()
    {
        if (onShowRemainingLife != null)
        {
            onShowRemainingLife();
        }
    }
    public Action<GameObject, string, float> onShowTechNameAndHit;
    public void ShowTechNameAndHit(GameObject attacker, string move, float totalChanceToHit)
    {
        if (onShowTechNameAndHit != null)
        {
            onShowTechNameAndHit(attacker, move, totalChanceToHit);
        }
    }
}
