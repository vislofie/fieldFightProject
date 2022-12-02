using System.Collections.Generic;
using UnityEngine;

public class EnemyBrain : PawnBrain
{
    public override void AddEffect(ActionType action)
    {
        base.AddEffect(action);

        if (_chars.Hp == 0)
        {
            GetComponent<CapsuleCollider>().radius = 0.5f;

            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;

            _rigidbody.AddForce(new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)).normalized * 25.0f);

            MatchManager.Instance.RemovePawn(this);

            _alive = false;

            _ui.HideAllUI();
        }
    }

    override public void AddRandomAction()
    {
        int actionIndex = Random.Range(0, _actionScripts.Count);
        ActionObj desiredAction;        

        if (_actionScripts.ContainsKey((ActionType)actionIndex))
        {
            desiredAction = _actionScripts[(ActionType)actionIndex];
            desiredAction.Initialize(false);

            List<AllyBrain> listOfPlayerAllies = MatchManager.Instance.Allies;

            desiredAction.GraduallyMoveJointBaseToPosition(listOfPlayerAllies[Random.Range(0, listOfPlayerAllies.Count)].transform.position);
        }
    }

    override public void ApplyAllApplicableEffects()
    {
        base.ApplyAllApplicableEffects();

        if (_chars.Hp == 0)
        {
            GetComponent<CapsuleCollider>().radius = 0.5f;

            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;

            _rigidbody.AddForce(new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)).normalized * 25.0f);

            MatchManager.Instance.RemovePawn(this);

            _alive = false;

            _ui.HideAllUI();
        }
    }
}
