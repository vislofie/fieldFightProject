using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnBrain : MonoBehaviour, IPawnBrain
{
    protected Dictionary<ActionType, ActionObj> _actionScripts = new Dictionary<ActionType, ActionObj>();

    protected bool _alive;
    public bool Alive => _alive;

    protected PawnUI _ui;
    protected PawnChars _chars;

    [SerializeField]
    protected GameObject _shieldVisualizer;

    protected Rigidbody _rigidbody;

    private Vector3 _pawnInitialPosition;
    private Quaternion _pawnInitialRotation;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _ui = GetComponent<PawnUI>();
        _chars = GetComponent<PawnChars>();

        _pawnInitialPosition = transform.localPosition;
        _pawnInitialRotation = transform.localRotation;

        _alive = true;

        foreach (ActionObj action in GetComponentsInChildren<ActionObj>())
        {
            _actionScripts.Add(action.Action, action);
        }

        _ui.UpdateHP(_chars.Hp, _chars.AdditiveHp);
    }

    virtual public void AddEffect(ActionType action)
    {
        switch (action)
        {
            case ActionType.Attack:
                _chars.AddEffect(ApplicableCharacterEffect.Attack);
                if (_chars.AdditiveHp == 0 && _shieldVisualizer.activeSelf)
                {
                    _shieldVisualizer.SetActive(false);
                }
                break;
            case ActionType.Heal:
                _chars.AddEffect(ApplicableCharacterEffect.Heal);
                break;
            case ActionType.Poison:
                _chars.AddEffect(ApplicableCharacterEffect.Poison);
                _ui.ActivatePoison();
                break;
            case ActionType.Shield:
                _chars.AddEffect(FixedCharacterEffect.Shield);
                _shieldVisualizer.SetActive(true);
                break;
            default:
                throw new System.Exception("Not considered action!");

        }
        _ui.UpdateHP(_chars.Hp, _chars.AdditiveHp);
    }
    virtual public void AddRandomAction()
    {
        throw new System.NotImplementedException();
    }


    virtual public void ApplyAllApplicableEffects()
    {
        _chars.TryApplyAllEffects();
        _ui.UpdateHP(_chars.Hp, _chars.AdditiveHp);

        if (!_chars.IsEffectActive(ActionType.Poison))
        {
            _ui.DeactivatePoison();
        }
    }

    public void ResetToStart()
    {
        GetComponent<CapsuleCollider>().radius = 1.0f;

        transform.localPosition = _pawnInitialPosition;
        transform.localRotation = _pawnInitialRotation;

        _chars.ResetValues();
        _shieldVisualizer.SetActive(false);

        _ui.UpdateHP(_chars.Hp, _chars.AdditiveHp);
        _ui.ShowAllUI();

        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;

        _alive = true;

        StopAllCoroutines();
    }

    virtual public void EndTurn()
    {
        foreach (ActionObj action in _actionScripts.Values)
        {
            Debug.Log(action.Initialized);
            if (action.Initialized)
            {
                if (action.EffectApplied)
                    StartCoroutine(FinishResetDelay(5.0f, action));
                else
                    action.ResetPosAndRot();

                MatchManager.Instance.FinishStep();
            }
        }
    }

    private IEnumerator FinishResetDelay(float resetShatteredPos, ActionObj action)
    {
        yield return new WaitForSeconds(resetShatteredPos);
        action.ResetPosAndRot();
    }
}
