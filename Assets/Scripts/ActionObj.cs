using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType { Attack = 0, Heal = 1, Poison = 2, Shield = 3}
public class ActionObj : MonoBehaviour
{
    [SerializeField]
    private LayerMask _objectsToInteractMask;
    [SerializeField]
    private GameObject _shatteredObject;

    [SerializeField]
    private ActionType _action;
    public ActionType Action => _action;

    private Rigidbody _rigidbody;
    private FixedJoint _joint;
    private MeshRenderer _meshRenderer;

    private Rigidbody _jointBase;
    private Collider _collider;

    private bool _initialized = false;
    public bool Initialized => _initialized;

    private bool _effectApplied = false;
    public bool EffectApplied => _effectApplied;

    private bool _fromPlayerTeam;

    private IEnumerator _moveCoroutine;

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    private List<Quaternion> _shattersInitialRotation = new List<Quaternion>();
    private List<Transform> _shatteredParts = new List<Transform>();

    private Light _light;

    Ray ray;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _joint = GetComponent<FixedJoint>();

        _light = GetComponentInChildren<Light>(true);

        _collider = GetComponent<Collider>();

        _meshRenderer = GetComponent<MeshRenderer>();

        _initialPosition = transform.localPosition;
        _initialRotation = transform.localRotation;

        if (_action != ActionType.Shield)
        {
            foreach (Transform shatteredPart in _shatteredObject.transform.GetComponentsInChildren<Transform>())
            {
                _shattersInitialRotation.Add(shatteredPart.localRotation);
                _shatteredParts.Add(shatteredPart);
            }
        }
        
    }

    public void Initialize(bool playerTeam)
    {
        _initialized = true;
        _fromPlayerTeam = playerTeam;

        transform.localPosition = _initialPosition;
        transform.localRotation = _initialRotation;

        if (_action != ActionType.Shield)
        {
            _shatteredObject.SetActive(false);
            
            for (int i = 0; i < _shatteredParts.Count; i++)
            {
                _shatteredParts[i].localPosition = Vector3.zero;
                _shatteredParts[i].localRotation = _shattersInitialRotation[i];
            }
        }
        

        _jointBase = _joint.connectedBody;
        _joint.massScale = 1.0f;

        _meshRenderer.enabled = true;

        _rigidbody.isKinematic = false;
        _collider.enabled = true;

        _light.gameObject.SetActive(true);
    }

    public void ResetPosAndRot()
    {
        transform.localPosition = _initialPosition;
        transform.localRotation = _initialRotation;

        if (_action != ActionType.Shield)
        {
            _shatteredObject.SetActive(false);

            for (int i = 0; i < _shatteredParts.Count; i++)
            {
                _shatteredParts[i].localPosition = Vector3.zero;
                _shatteredParts[i].localRotation = _shattersInitialRotation[i];
            }
        }

        _joint.massScale = 0;

        _meshRenderer.enabled = false;
        _light.gameObject.SetActive(false);

        _rigidbody.isKinematic = true;
        _collider.enabled = false;

        _initialized = false;
    }

    public void GraduallyMoveJointBaseToPosition(Vector3 pos)
    {
        if (_initialized)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.transform.position.y - 5;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            _rigidbody.drag = 2.0f;

            _jointBase.constraints = RigidbodyConstraints.FreezePositionY;
            _jointBase.mass = 1;

            pos.y = mousePos.y;

            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = MoveCoroutine(_jointBase.gameObject.transform, pos);
            StartCoroutine(_moveCoroutine);
        }
    }

    private IEnumerator MoveCoroutine(Transform transformToMove, Vector3 pos)
    {
        float lerpDuration = 4.0f;
        float timeCounter = 0.0f;

        float t = timeCounter / lerpDuration;
        

        while (transformToMove.position != pos)
        {
            t = timeCounter / lerpDuration;
            t *= t;

            transformToMove.position = Vector3.Lerp(transformToMove.position, pos, t);
            yield return new WaitForSeconds(Time.deltaTime);
            timeCounter += Time.deltaTime;

            if (lerpDuration < timeCounter)
                transformToMove.position = pos;
        }

        ReleaseJointBase();

    }

    private void ReleaseJointBase()
    {
        _rigidbody.drag = 0.05f;

        _jointBase.mass = 0.1f;
        _jointBase.constraints = RigidbodyConstraints.None;
    }

    

    private void OnMouseDrag()
    {
        if (_initialized && _fromPlayerTeam)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.transform.position.y - 5;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            _rigidbody.drag = 2.0f;

            _jointBase.constraints = RigidbodyConstraints.FreezePositionY;
            _jointBase.mass = 1;
            _jointBase.position = Vector3.Lerp(_joint.connectedBody.position, mousePos, 50 * Time.deltaTime);
        }
    }

    private void OnMouseUp()
    {
        if (_initialized && _fromPlayerTeam)
        {
            ReleaseJointBase();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_initialized)
        {
            if (_objectsToInteractMask == (_objectsToInteractMask | (1 << collision.gameObject.layer)))
            {
                bool effectAdded = false;

                AllyBrain allyBrain;
                EnemyBrain enemyBrain;
                switch(_action)
                {
                    case ActionType.Heal:
                    case ActionType.Shield:
                        if (_fromPlayerTeam && collision.gameObject.TryGetComponent<AllyBrain>(out allyBrain) && allyBrain.Alive)
                        {
                            allyBrain.AddEffect(_action);
                            effectAdded = true;
                        }
                        break;
                    case ActionType.Poison:
                    case ActionType.Attack:
                        if (_fromPlayerTeam && collision.gameObject.TryGetComponent<EnemyBrain>(out enemyBrain) && enemyBrain.Alive)
                        {
                            enemyBrain.AddEffect(_action);
                            effectAdded = true;
                        }
                        else if (!_fromPlayerTeam && collision.gameObject.TryGetComponent<AllyBrain>(out allyBrain) && allyBrain.Alive)
                        {
                            allyBrain.AddEffect(_action);
                            effectAdded = true;
                        }
                        break;
                    default:
                        throw new System.Exception("This type of action was not considered!");
                }

                if (effectAdded)
                {
                    if (_action != ActionType.Shield)
                    {
                        _shatteredObject.SetActive(true);
                        foreach (Rigidbody shatteredPart in _shatteredObject.transform.GetComponentsInChildren<Rigidbody>())
                        {
                            shatteredPart.AddExplosionForce(200, collision.GetContact(0).point, 10.0f);
                        }
                    }

                    _joint.massScale = 0;

                    _meshRenderer.enabled = false;
                    _light.gameObject.SetActive(false);

                    _rigidbody.isKinematic = true;
                    _collider.enabled = false;

                    _effectApplied = true;

                    if (!_fromPlayerTeam)
                    {
                        MatchManager.Instance.EndTurn();
                    }
                }
            }
            

            
        }
    }

    


}
