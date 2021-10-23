using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleUiManager : MonoBehaviour
{
    public TechDatabase moveDatabase;
    public BattleManager battleManager;

    [Header("Player1 Panel")]
    public GameObject player1;
    public Slider p1LifeSlider;
    public Slider p1StaminaSlider;
    public Text p1LifeCurrentText;
    public Text p1LifeMaxText;
    public Text p1StaminaCurrentText;
    public Text p1StaminaMaxText;
    public Text p1MonsterName;
    public GameObject p1ClosePanelGroup;
    public GameObject p1MidPanelGroup;
    public GameObject p1LongPanelGroup;
    public GameObject p1ExtremePanelGroup;
    public GameObject p1TechPanels;
    public GameObject p1TechButtonPrefab;
    private BattleStats p1BattleStats;

    [Header("Player2 Panel")]
    public GameObject player2;
    public Slider p2LifeSlider;
    public Slider p2StaminaSlider;
    public Text p2LifeCurrentText;
    public Text p2LifeMaxText;
    public Text p2StaminaCurrentText;
    public Text p2StaminaMaxText;
    public Text p2MonsterName;
    public GameObject p2ClosePanelGroup;
    public GameObject p2MidPanelGroup;
    public GameObject p2LongPanelGroup;
    public GameObject p2ExtremePanelGroup;
    public GameObject p2TechPanels;
    public GameObject p2TechButtonPrefab;
    private BattleStats p2BattleStats;

    [Header("Player1 Move Panel")]
    public Text p1MoveNameText;
    public Text p1MoveHitChanceText;
    public Text p1LifeDamageText;
    public Text p1StaminaDamageText;

    [Header("Player2 Move Panel")]
    public Text p2MoveNameText;
    public Text p2MoveHitChanceText;
    public Text p2LifeDamageText;
    public Text p2StaminaDamageText;

    [Header("Game UI")]
    public GameObject moveActivatedUi;
    public GameObject extraUi;
    public GameObject rangePanels;
    public GameObject closeRangePanel;
    public GameObject midRangePanel;
    public GameObject longRangePanel;
    public GameObject extremeRangePanel;
    public Text battleStatePanelText;
    public Text battleTimer;
    public Text p1RemainingLife;
    public Text p2RemainingLife;


    private void Start()
    {
        //subscribe
        battleManager.onLifeDamaged += LifeDamageOccured;
        battleManager.onStaminaDamaged += StaminaDamageOccured;
        battleManager.onAttackHit += ShowMoveDamages;
        battleManager.onAttackFinished += DisableAllMoveActivatedUi;
        battleManager.onBattleRangeChanged += DisplayMovePanelsBasedOnRange;
        battleManager.onShowMessage += DisplayMessage;
        battleManager.onTimerChanged += ShowTimer;
        battleManager.onShowRemainingLife += ShowRemainingLifeUi;
        battleManager.onShowTechNameAndHit += ShowMoveNameAndHitChance;
        battleManager.onBattleStateChanged += BattleStateChanged;

        p1BattleStats = player1.GetComponent<BattleStats>();
        p2BattleStats = player2.GetComponent<BattleStats>();

        PopulateLifePanelInformation();
        PopulateTechPanels();

        DisableAlExtraUi();
        DisableAllMoveActivatedUi();
    }

    private void Update()
    {
        PopulateLifePanelInformation();
    }

    public void DisableAlExtraUi()
    {
        foreach (Transform child in extraUi.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void ShowRemainingLifeUi()
    {
        StartCoroutine(ShowRemainingLifeUiCoroutine());
    }
    IEnumerator ShowRemainingLifeUiCoroutine()
    {
        p1RemainingLife.gameObject.SetActive(true);
        p2RemainingLife.gameObject.SetActive(true);
        p1RemainingLife.text = Mathf.RoundToInt((p1BattleStats.lifeCurrent / p1BattleStats.lifeMax) * 100) + "%";
        p2RemainingLife.text = Mathf.RoundToInt((p2BattleStats.lifeCurrent / p2BattleStats.lifeMax) * 100) + "%";
        yield return new WaitForSeconds(3f);
        p1RemainingLife.gameObject.SetActive(false);
        p2RemainingLife.gameObject.SetActive(false);
    }

    public void ShowTimer(float timer)
    {
        battleTimer.text = Mathf.RoundToInt(timer) + "";
    }


    public void DisplayMessage(string message, float messageDuration)
    {
        StartCoroutine(DisplayMessageForSeconds(message, messageDuration));
    }

    IEnumerator DisplayMessageForSeconds(string message, float messageDuration)
    {
        battleStatePanelText.gameObject.SetActive(true);
        battleStatePanelText.text = message;
        yield return new WaitForSeconds(messageDuration);
        battleStatePanelText.gameObject.SetActive(false);
    }

    public void PopulateLifePanelInformation()
    {
        p1LifeSlider.value = p1BattleStats.lifeCurrent / p1BattleStats.lifeMax;
        p1StaminaSlider.value = p1BattleStats.staminaCurrent / p1BattleStats.staminaMax;
        p1LifeCurrentText.text = Mathf.RoundToInt(p1BattleStats.lifeCurrent) + "";
        p1LifeMaxText.text = Mathf.RoundToInt(p1BattleStats.lifeMax) + "";
        p1StaminaCurrentText.text = Mathf.RoundToInt(p1BattleStats.staminaCurrent) + "";
        p1StaminaMaxText.text = Mathf.RoundToInt(p1BattleStats.staminaMax) + "";
        p1MonsterName.text = p1BattleStats.monsterName;

        p2LifeSlider.value = p2BattleStats.lifeCurrent / p2BattleStats.lifeMax;
        p2StaminaSlider.value = p2BattleStats.staminaCurrent / p2BattleStats.staminaMax;
        p2LifeCurrentText.text = Mathf.RoundToInt(p2BattleStats.lifeCurrent) + "";
        p2LifeMaxText.text = Mathf.RoundToInt(p2BattleStats.lifeMax) + "";
        p2StaminaCurrentText.text = Mathf.RoundToInt(p2BattleStats.staminaCurrent) + "";
        p2StaminaMaxText.text = Mathf.RoundToInt(p2BattleStats.staminaMax) + "";
        p2MonsterName.text = p2BattleStats.monsterName;
    }
    
    public void PopulateTechPanels()
    {
        PopulateTechPanelBasedOnRange(p1ClosePanelGroup, TechContent.techRanges.CLOSE, p1BattleStats);
        PopulateTechPanelBasedOnRange(p1MidPanelGroup, TechContent.techRanges.MID, p1BattleStats);
        PopulateTechPanelBasedOnRange(p1LongPanelGroup, TechContent.techRanges.LONG, p1BattleStats);
        PopulateTechPanelBasedOnRange(p1ExtremePanelGroup, TechContent.techRanges.EXTREME, p1BattleStats);

        PopulateTechPanelBasedOnRange(p2ClosePanelGroup, TechContent.techRanges.CLOSE, p2BattleStats);
        PopulateTechPanelBasedOnRange(p2MidPanelGroup, TechContent.techRanges.MID, p2BattleStats);
        PopulateTechPanelBasedOnRange(p2LongPanelGroup, TechContent.techRanges.LONG, p2BattleStats);
        PopulateTechPanelBasedOnRange(p2ExtremePanelGroup, TechContent.techRanges.EXTREME, p2BattleStats);

        //hide after populating
        p1TechPanels.SetActive(false);
        p2TechPanels.SetActive(false);
        rangePanels.SetActive(false);
    }

    public void PopulateTechPanelBasedOnRange(GameObject groupPanel, TechContent.techRanges range, BattleStats battleStats)
    {
        //clear menu before populating
        foreach (Transform child in groupPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (string tech in battleStats.movesKnown)
        {
            foreach (var item in moveDatabase.techDatabase)
            {
                if (tech == item.techName && item.techRange == range)
                {
                    //instantiate button template in organizer
                    GameObject techPanelItem = Instantiate(p1TechButtonPrefab, groupPanel.transform);
                    techPanelItem.transform.name = tech;
                    techPanelItem.transform.Find("Tech Button Text").GetComponent<Text>().text = tech + "";
                    techPanelItem.transform.Find("Tech Button Text").name = tech + " Button Name";
                    techPanelItem.transform.Find("Tech Button Hit").GetComponent<Text>().text = GetMoveHitChanceClass(GetMoveHitChance(tech));
                    techPanelItem.transform.Find("Tech Button Hit").name = tech + " Button Hit";
                    techPanelItem.transform.Find("Tech Button Cost").GetComponent<Text>().text = GetMoveCost(tech) + "";
                    techPanelItem.transform.Find("Tech Button Cost").name = tech + " Button Cost";
                    if (battleStats.gameObject.name == "Player 1")
                    {
                        techPanelItem.GetComponent<Button>().onClick.AddListener(() => TechButtonClicked(techPanelItem.transform.name, player1, player2));
                    }
                    else if (battleStats.gameObject.name == "Player 2")
                    {
                        techPanelItem.GetComponent<Button>().onClick.AddListener(() => TechButtonClicked(techPanelItem.transform.name, player2, player1));
                    }
                    
                }
            }
        }
    }

    public string GetMoveHitChanceClass(float hitChance)
    {
        string hitChanceClass = "";
        if (hitChance > 15)
        {
            hitChanceClass = "S";
        }
        else if (hitChance > 10 && hitChance <= 15)
        {
            hitChanceClass = "A";
        }
        else if (hitChance > 5 && hitChance <= 10)
        {
            hitChanceClass = "B";
        }
        else if (hitChance > 0 && hitChance <= 5)
        {
            hitChanceClass = "B";
        }
        else if (hitChance > -5 && hitChance <= 0)
        {
            hitChanceClass = "C";
        }
        else if (hitChance > -10 && hitChance <= -5)
        {
            hitChanceClass = "D";
        }
        else if (hitChance > -15 && hitChance <= -10)
        {
            hitChanceClass = "E";
        }
        else if (hitChance <= -15)
        {
            hitChanceClass = "F";
        }
        return hitChanceClass;
    }

    public Action<string, GameObject, GameObject> onMoveButtonClicked;
    public void TechButtonClicked(string move, GameObject attacker, GameObject defender)
    {
        if(onMoveButtonClicked != null)
        {
            onMoveButtonClicked(move, attacker, defender);
        }
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

    public void LifeDamageOccured(GameObject monster, float damage)
    {
        if(monster.name == "Player 1")
        {
            //text
            float from = monster.GetComponent<BattleStats>().lifeCurrent;
            float newValue = monster.GetComponent<BattleStats>().lifeCurrent - damage;
            StartCoroutine(TextLerper(p1LifeCurrentText, from, newValue));

            //slider
            StartCoroutine(SliderLerper(p1LifeSlider, from, newValue, monster.GetComponent<BattleStats>().lifeMax));

            //display damage
            StartCoroutine(TextLerper(p1LifeDamageText, 0, damage));
        }

        if (monster.name == "Player 2")
        {
            //text
            float from = monster.GetComponent<BattleStats>().lifeCurrent;
            float newValue = monster.GetComponent<BattleStats>().lifeCurrent - damage;
            StartCoroutine(TextLerper(p2LifeCurrentText, from, newValue));

            //slider
            StartCoroutine(SliderLerper(p2LifeSlider, from, newValue, monster.GetComponent<BattleStats>().lifeMax));

            //display damage
            StartCoroutine(TextLerper(p2LifeDamageText, 0, damage));
        }
    }

    public void StaminaDamageOccured(GameObject monster, float damage)
    {
        if (monster.name == "Player 1")
        {
            //text
            float from = monster.GetComponent<BattleStats>().staminaCurrent;
            float newValue = monster.GetComponent<BattleStats>().staminaCurrent - damage;
            StartCoroutine(TextLerper(p1StaminaCurrentText, from, newValue));

            //slider
            StartCoroutine(SliderLerper(p1StaminaSlider, from, newValue, monster.GetComponent<BattleStats>().staminaMax));

            //display damage
            StartCoroutine(TextLerper(p1StaminaDamageText, 0, damage));
        }

        if (monster.name == "Player 2")
        {
            //text
            float from = monster.GetComponent<BattleStats>().staminaCurrent;
            float newValue = monster.GetComponent<BattleStats>().staminaCurrent - damage;
            StartCoroutine(TextLerper(p2StaminaCurrentText, from, newValue));

            //slider
            StartCoroutine(SliderLerper(p2StaminaSlider, from, newValue, monster.GetComponent<BattleStats>().staminaMax));

            //display damage
            StartCoroutine(TextLerper(p2StaminaDamageText, 0, damage));
        }
    }

    IEnumerator TextLerper(Text textField, float from, float newValue)
    {
        if(newValue < 0)
        {
            newValue = 0;
        }

        while(Mathf.RoundToInt(from) != Mathf.RoundToInt(newValue))
        {
            from = Mathf.Lerp(from, newValue, Time.deltaTime * 5f);
            textField.text = Mathf.RoundToInt(from) + "";
            yield return null;
        }
    }

    IEnumerator SliderLerper(Slider slider, float from, float newValue, float maxValue)
    {
        while (Mathf.RoundToInt(from) != Mathf.RoundToInt(newValue))
        {
            from = Mathf.Lerp(from, newValue, Time.deltaTime * 5f);
            slider.value = from/maxValue;
            yield return null;
        }
    }

    public void ShowMoveNameAndHitChance(GameObject attacker, string move, float hitChance)
    {
        if(attacker.name == "Player 1")
        {
            p1MoveNameText.gameObject.SetActive(true);
            p1MoveHitChanceText.gameObject.SetActive(true);
            p1MoveNameText.text = move;
            p1MoveHitChanceText.text = Mathf.RoundToInt(hitChance) + "%";
        }

        if (attacker.name == "Player 2")
        {
            p2MoveNameText.gameObject.SetActive(true);
            p2MoveHitChanceText.gameObject.SetActive(true);
            p2MoveNameText.text = move;
            p2MoveHitChanceText.text = Mathf.RoundToInt(hitChance) + "%";
        }
    }

    public void ShowMoveDamages(GameObject defender, float lifeDamage, float staminaDamage)
    {
        if (defender.name == "Player 1")
        {
            p1LifeDamageText.gameObject.SetActive(true);
            p1StaminaDamageText.gameObject.SetActive(true);
            p1LifeDamageText.text = lifeDamage + "";
            p1StaminaDamageText.text = staminaDamage + "";
        }

        if (defender.name == "Player 2")
        {
            p2LifeDamageText.gameObject.SetActive(true);
            p2StaminaDamageText.gameObject.SetActive(true);
            p2LifeDamageText.text = lifeDamage + "";
            p2StaminaDamageText.text = staminaDamage + "";
        }
    }

    public void DisableAllMoveActivatedUi()
    {
        foreach (Transform child in moveActivatedUi.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void DisplayMovePanelsBasedOnRange(BattleManager.battleRanges range)
    {
        //range panels
        foreach (Transform child in rangePanels.transform)
        {
            child.gameObject.SetActive(false);
        }

        //techs
        foreach (Transform child in p1TechPanels.transform)
        {
            child.gameObject.SetActive(false);
        }
        foreach (Transform child in p2TechPanels.transform)
        {
            child.gameObject.SetActive(false);
        }

        if (range == BattleManager.battleRanges.CLOSE)
        {
            p1ClosePanelGroup.SetActive(true);
            p2ClosePanelGroup.SetActive(true);
            closeRangePanel.SetActive(true);
        }
        else if (range == BattleManager.battleRanges.MID)
        {
            p1MidPanelGroup.SetActive(true);
            p2MidPanelGroup.SetActive(true);
            midRangePanel.SetActive(true);
        }
        else if (range == BattleManager.battleRanges.LONG)
        {
            p1LongPanelGroup.SetActive(true);
            p2LongPanelGroup.SetActive(true);
            longRangePanel.SetActive(true);
        }
        else if (range == BattleManager.battleRanges.EXTREME)
        {
            p1ExtremePanelGroup.SetActive(true);
            p2ExtremePanelGroup.SetActive(true);
            extremeRangePanel.SetActive(true);
        }
    }

    public void BattleStateChanged(BattleManager.battleStates battlestate)
    {
        if(battlestate == BattleManager.battleStates.WAIT)
        {
            p1TechPanels.SetActive(true);
            p2TechPanels.SetActive(true);
            rangePanels.SetActive(true);
        }
        else
        {
            p1TechPanels.SetActive(false);
            p2TechPanels.SetActive(false);
            rangePanels.SetActive(false);
        }
    }
}
