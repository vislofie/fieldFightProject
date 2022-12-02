using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    private static MatchManager _instance;
    public static MatchManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MatchManager>();
            }
            return _instance;
        }
    }

    [SerializeField]
    private GameObject _skipBtn;
    [SerializeField]
    private GameObject _winPanel;
    [SerializeField]
    private GameObject _losePanel;


    private List<AllyBrain> _allies = new List<AllyBrain>();
    public List<AllyBrain> Allies => _allies;

    private List<EnemyBrain> _enemies = new List<EnemyBrain>();
    public List<EnemyBrain> Enemies => _enemies;

    private List<ActionObj> _actionList = new List<ActionObj>();

    private int _allyCount;
    private int _enemyCount;

    private int _step;

    private void Awake()
    {
        _allies.AddRange(FindObjectsOfType<AllyBrain>());
        _enemies.AddRange(FindObjectsOfType<EnemyBrain>());

        _actionList.AddRange(FindObjectsOfType<ActionObj>());

        _allyCount = _allies.Count;
        _enemyCount = _enemies.Count;

        _step = 0;
    }

    private void Start()
    {
        StartNewRound();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _step <= _allyCount - 1)
        {
            EndTurn();
        }
        
        else if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
    }

    public void StartNewRound()
    {
        _step = 0;

        for (int i = _allyCount - 1; i >= 0; i--)
        {
            _allies[i].ApplyAllApplicableEffects();
        }
        for (int i = _enemyCount - 1; i >= 0; i--)
        {
            _enemies[i].ApplyAllApplicableEffects();
        }

        if (_allyCount > 0 && _enemyCount > 0)
        {
            _skipBtn.SetActive(true);
            _allies[_step].AddRandomAction();
        }
    }

    public void FinishStep()
    {
        if (_allyCount == 0 || _enemyCount == 0) return;

        _step++;
        if (_step > _allyCount + _enemyCount - 1)
        {
            StartNewRound();
        }
        else
        {
            if (_step <= _allyCount - 1)
            {
                if (!_skipBtn.activeSelf)
                {
                    _skipBtn.SetActive(true);
                }
                _allies[_step].AddRandomAction();
            }
            else
            {
                if (_skipBtn.activeSelf)
                {
                    _skipBtn.SetActive(false);
                }
                if (_allyCount > 0)
                {
                    _enemies[_step - _allyCount].AddRandomAction();
                }
            }
            
        }
    }

    public void RemovePawn(AllyBrain pawn)
    {
        if (_allies.Contains(pawn))
        {
            _allies.Remove(pawn);
            _allyCount--;
            _step--;
        }

        if (_allyCount == 0)
        {
            _losePanel.SetActive(true);
            _skipBtn.SetActive(false);
        }
    }

    public void RemovePawn(EnemyBrain pawn)
    {
        if (_enemies.Contains(pawn))
        {
            _enemies.Remove(pawn);
            _enemyCount--;
        }    

        if (_enemyCount == 0)
        {
            _winPanel.SetActive(true);
            _skipBtn.SetActive(false);
        }
    }

    public void Restart()
    {
        _enemies.Clear();
        _allies.Clear();

        _allies.AddRange(FindObjectsOfType<AllyBrain>());
        _enemies.AddRange(FindObjectsOfType<EnemyBrain>());

        foreach (AllyBrain ally in _allies)
        {
            ally.ResetToStart();
        }
        foreach (EnemyBrain enemy in _enemies)
        {
            enemy.ResetToStart();
        }

        _allyCount = _allies.Count;
        _enemyCount = _enemies.Count;

        foreach (ActionObj action in _actionList)
        {
            action.ResetPosAndRot();
        }

        StartNewRound();

    }

    public void EndTurn()
    {
        if (_step <= _allyCount - 1)
        {
            _allies[_step].EndTurn();
        }
        else if (_step <= _allyCount + _enemyCount - 1)
        {
            _enemies[_step - _allyCount].EndTurn();
        }
    }
}
