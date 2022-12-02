using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ApplicableCharacterEffect { Attack = 0, Heal = 1, Poison = 2 };
public enum FixedCharacterEffect { Shield = 0 };
public class PawnChars : MonoBehaviour
{
    [SerializeField]
    private int _maxHp = 30;

    private int _hp;
    private int _additiveHp;
    public int Hp
    {
        get { return _hp; }
        set
        {
            if (_additiveHp != 0)
            {
                _additiveHp -= _hp - value;
                if (_additiveHp < 0)
                {
                    _hp = Mathf.Clamp(_hp, 0, _hp + _additiveHp);
                    _additiveHp = 0;
                }
            }
            else
            {
                _hp = Mathf.Clamp(value, 0, value);
            }
        }
    }
    public int AdditiveHp => _additiveHp;

    private class ApplicableEffect
    {
        private ApplicableCharacterEffect _associatedEffect;
        public ApplicableCharacterEffect AssociatedEffect => _associatedEffect;

        private int _hpChangeEachStep;
        public int HpChangeEachStep => _hpChangeEachStep;

        private int _stepsLeft;
        public int StepsLeft => _stepsLeft;

        /// <summary>
        /// Creates a new applicable effect that affects health
        /// </summary>
        /// <param name="hpChange">how much health is going to change each step</param>
        /// <param name="steps">how much steps(including the initial one) is this effect going to be active</param>
        public ApplicableEffect(int hpChange, int steps)
        {
            _hpChangeEachStep = hpChange;
            _stepsLeft = steps;
        }

        /// <summary>
        /// Tries to apply effect to given hp and returns changed hp if succeeded to apply, and unchanged if effect could not be applied
        /// </summary>
        /// <param name="hp">given hp</param>
        /// <param name="maxHp">hp limit</param>
        /// <returns></returns>
        public int TryApplyEffect(int hp, int maxHp)
        {
            if (_stepsLeft > 0 && hp > 0)
            {
                _stepsLeft--;
                return Mathf.Clamp(hp + _hpChangeEachStep, hp + _hpChangeEachStep, maxHp);
            }
            return hp;
        }


    }

    private class FixedEffect
    {
        private FixedCharacterEffect _associatedEffect;
        public FixedCharacterEffect AssociatedEffect => _associatedEffect;

        private int _hpValue;
        public int HpValue => _hpValue;

        private int _stepsLeft;
        public int StepsLeft => _stepsLeft;

        /// <summary>
        /// Creates a new fixed effect that is going to affect health
        /// </summary>
        /// <param name="hpValue">amount of fixed hp to add</param>
        /// <param name="steps">how much steps this fixed effect is going to be active(including current step)</param>
        public FixedEffect(int hpValue, int steps)
        {
            _hpValue = hpValue;
            _stepsLeft = steps;
        }

        /// <summary>
        /// Changes the number of steps and returns it
        /// </summary>
        /// <returns></returns>
        public int ApplyEffect()
        {
            if (_stepsLeft > 0)
            {
                _stepsLeft--;
            }

            return _stepsLeft;

        }


    }

    private List<ApplicableEffect> _applicableEffects = new List<ApplicableEffect>();
    private List<FixedEffect> _fixedEffects = new List<FixedEffect>();


    private void Awake()
    {
        _hp = _maxHp;
    }

    /// <summary>
    /// Removes given applicable effect from this character
    /// </summary>
    /// <param name="effect">given applicable effect</param>
    private void RemoveEffect(ApplicableEffect effect)
    {
        if (_applicableEffects.Contains(effect))
        {
            _applicableEffects.Remove(effect);
        }
    }

    /// <summary>
    /// Removes given fixed effect from this character
    /// </summary>
    /// <param name="effect">given fixed effect</param>
    private void RemoveEffect(FixedEffect effect)
    {
        if (_fixedEffects.Contains(effect))
        {
            _fixedEffects.Remove(effect);
        }
    }

    /// <summary>
    /// Adds applicable effects to this character
    /// </summary>
    /// <param name="effect">type of applicable effect to add</param>
    public void AddEffect(ApplicableCharacterEffect effect)
    {
        switch (effect)
        {
            case ApplicableCharacterEffect.Attack:
                Hp -= 3;
                break;
            case ApplicableCharacterEffect.Heal:
                for (int i = _applicableEffects.Count - 1; i >= 0; i--)
                {
                    if (_applicableEffects[i].AssociatedEffect == ApplicableCharacterEffect.Poison)
                    {
                        RemoveEffect(_applicableEffects[i]);
                    }
                }

                _applicableEffects.Add(new ApplicableEffect(1, 1));
                _hp = _applicableEffects[_applicableEffects.Count - 1].TryApplyEffect(_hp, _maxHp);
                break;
            case ApplicableCharacterEffect.Poison:
                _applicableEffects.Add(new ApplicableEffect(-1, 2));
                _hp = _applicableEffects[_applicableEffects.Count - 1].TryApplyEffect(_hp, _maxHp);
                break;
            default:
                throw new System.Exception("Wrong effect given");
        }
    }

    /// <summary>
    /// Adds fixed effect to this character
    /// </summary>
    /// <param name="effect">type of fixed effect to add</param>
    public void AddEffect(FixedCharacterEffect effect)
    {
        switch (effect)
        {
            case FixedCharacterEffect.Shield:
                _fixedEffects.Add(new FixedEffect(5, 3));
                _additiveHp += 5;
                break;
            default:
                throw new System.Exception("Wrong effect given");
        }
    }

    /// <summary>
    /// Applies all of effects and reduces step count for each of them. Returns true if character survives after applying effects and false otherwise.
    /// </summary>
    public bool TryApplyAllEffects()
    {
        for (int i = _fixedEffects.Count - 1; i >= 0; i--)
        {
            if (_fixedEffects[i].ApplyEffect() == 0)
            {
                RemoveEffect(_fixedEffects[i]);
            }
        }

        int tempHp;
        for (int i = _applicableEffects.Count - 1; i >= 0; i--)
        {
            tempHp = _hp;
            _hp = _applicableEffects[i].TryApplyEffect(_hp, _maxHp);
            if (tempHp == _hp && _hp != 0)
            {
                RemoveEffect(_applicableEffects[i]);
            }
            else if (_hp <= 0)
            {
                if (_hp + _additiveHp <= 0)
                {
                    return false; // dis guy dead
                }
                else
                {
                    continue;
                }
            }
        }

        return true;

    }

    public bool IsEffectActive(ActionType action)
    {
        switch (action)
        {
            case ActionType.Shield:
                foreach (FixedEffect effect in _fixedEffects)
                    if (effect.AssociatedEffect == FixedCharacterEffect.Shield)
                        return true;
                break;
            case ActionType.Poison:
                foreach (ApplicableEffect effect in _applicableEffects)
                    if (effect.AssociatedEffect == ApplicableCharacterEffect.Poison)
                        return true;
                break;
            default:
                throw new System.Exception("Wrong action given");
        }
        return false;
    }

    public void ResetValues()
    {
        _hp = _maxHp;
        _additiveHp = 0;
        _applicableEffects.Clear();
        _fixedEffects.Clear();
        TryApplyAllEffects();
    }

    
}
