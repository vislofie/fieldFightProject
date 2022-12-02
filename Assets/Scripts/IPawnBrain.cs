using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPawnBrain
{

    /// <summary>
    /// Adds effect cause by applying interactable object on this pawn
    /// </summary>
    /// <param name="interactable">interactable that is creating an effect</param>
    abstract public void AddEffect(ActionType interactable);

    /// <summary>
    /// Adds random action for this pawn
    /// </summary>
    abstract public void AddRandomAction();

    /// <summary>
    /// Applies all applicable effects that are on this pawn. Usually calls each round
    /// </summary>
    abstract public void ApplyAllApplicableEffects();

    abstract public void EndTurn();
}
