using UnityEngine;

public class AllyBrain : PawnBrain
{
    override public void AddEffect(ActionType action)
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

        if (_actionScripts.ContainsKey((ActionType)actionIndex))
        {
            _actionScripts[(ActionType)actionIndex].Initialize(true);
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
