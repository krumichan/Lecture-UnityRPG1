using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Die
        , Moving
        , Idle
        , Skill
    }

    int _mask = (1 << (int)Define.Layer.Ground) | (1 << (int)Define.Layer.Monster);

    PlayerStat _stat;
    Vector3 _destination;

    [SerializeField]
    PlayerState _state = PlayerState.Idle;

    GameObject _lockTarget;

    public PlayerState State
    {
        get { return _state; }
        set
        {
            _state = value;

            Animator animator = GetComponent<Animator>();
            switch (_state)
            {
                case PlayerState.Idle:
                    animator.CrossFade("WAIT", 0.1f);
                    break;

                case PlayerState.Moving:
                    animator.CrossFade("RUN", 0.1f);
                    break;

                case PlayerState.Skill:
                    animator.CrossFade("ATTACK", 0.1f, -1, 0);
                    break;

                case PlayerState.Die:
                    break;
            }
        }
    }

    void Start()
    {
        _stat = gameObject.GetComponent<PlayerStat>();

        Managers.Input.MouseAction -= OnMouseEvent;
        Managers.Input.MouseAction += OnMouseEvent;
    }

    void UpdateDie()
    {
        // 아무것도 못함.
    }

    void UpdateMoving()
    {
        // Monster가 내 사정거리보다 가까운 경우.
        if (_lockTarget != null)
        {
            _destination = _lockTarget.transform.position;

            float distance = (_destination - transform.position).magnitude;
            if (distance <= 1)
            {
                State = PlayerState.Skill;
                return;
            }
        }

        Vector3 direction = _destination - transform.position;
        if (direction.magnitude < 0.1f)
        {
            State = PlayerState.Idle;
        }
        else
        {
            NavMeshAgent nma = gameObject.GetOrAddComponent<NavMeshAgent>();

            // 이동 거리는 반드시 방향&크기 벡터(direction)의 크기보다 작아야 한다.
            // 그렇지 않으면, 목적지 바로 부근에서 마구마구 왔다갔다 한다.
            float moveDistance = Mathf.Clamp(_stat.MoveSpeed * Time.deltaTime, 0, direction.magnitude);

            // nma.CalculatePath
            // 크기까지 포함하는 방향 벡터.
            nma.Move(direction.normalized * moveDistance);

            Debug.DrawRay(transform.position, direction.normalized, Color.green);
            // 대충 배꼽 위치에서 쏘도록 설정. ( Vector3.up * 0.5f )
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, direction, 1.0f, LayerMask.GetMask("Block")))
            {
                if (Input.GetMouseButton(0) == false)
                {
                    State = PlayerState.Idle;
                } 
                return;
            }

            /*transform.position += direction.normalized * moveDistance;*/
            transform.rotation = Quaternion.Slerp(
                   transform.rotation
                   , Quaternion.LookRotation(direction)
                   , 20 * Time.deltaTime
            );
        }
    }

     void UpdateIdle()
    {

    }

    void UpdateSkill()
    {
        if (_lockTarget != null)
        {
            Vector3 direction = _lockTarget.transform.position - transform.position;
            Quaternion quaternion = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, 20 * Time.deltaTime);
        }
    }

    void OnHitEvent()
    {
        Debug.Log("On Hit Event");

        if (_stopSkill)
        {
            State = PlayerState.Idle;
        }
        else
        {
            State = PlayerState.Skill;
        }
    }

    void Update()
    {
        switch (State)
        {
            case PlayerState.Die:
                UpdateDie();
                break;

            case PlayerState.Moving:
                UpdateMoving();
                break;

            case PlayerState.Idle:
                UpdateIdle();
                break;

            case PlayerState.Skill:
                UpdateSkill();
                break;
        }
    }

    #region not use
    /*    void OnKeyboard()
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation
                    , Quaternion.LookRotation(Vector3.forward)
                    , 0.2f
                );
                transform.position += Vector3.forward * Time.deltaTime * _speed;
            }

            if (Input.GetKey(KeyCode.S))
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation
                    , Quaternion.LookRotation(Vector3.back)
                    , 0.2f
                );
                transform.position += Vector3.back * Time.deltaTime * _speed;
            }

            if (Input.GetKey(KeyCode.A))
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation
                    , Quaternion.LookRotation(Vector3.left)
                    , 0.2f
                );
                transform.position += Vector3.left * Time.deltaTime * _speed;
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation
                    , Quaternion.LookRotation(Vector3.right)
                    , 0.2f
                );
                transform.position += Vector3.right * Time.deltaTime * _speed;
            }

            _moveToDestination = false;
        }*/
    #endregion

    bool _stopSkill = false;
    void OnMouseEvent(Define.MouseEvent evt)
    {
        switch (State)
        {
            case PlayerState.Idle:
                OnMouseEvent_IdleRun(evt);
                break;

            case PlayerState.Moving:
                OnMouseEvent_IdleRun(evt);
                break;

            case PlayerState.Skill:
                {
                    if (evt == Define.MouseEvent.PointerUp)
                    {
                        _stopSkill = true;
                    }
                }
                break;
        }
    }

    void OnMouseEvent_IdleRun(Define.MouseEvent evt)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool raycastHit = Physics.Raycast(ray, out hit, 100.0f, _mask);
        /*Debug.DrawRay(Camera.main.transform.position, ray.direction * 100.0f, Color.red, 1.0f);*/

        switch (evt)
        {
            case Define.MouseEvent.PointerDown:
                {
                    if (raycastHit)
                    {
                        _destination = hit.point;
                        State = PlayerState.Moving;
                        _stopSkill = false;

                        if (hit.collider.gameObject.layer == (int)Define.Layer.Monster)
                        {
                            _lockTarget = hit.collider.gameObject;
                        }
                        else
                        {
                            _lockTarget = null;
                        }
                    }
                }
                break;

            case Define.MouseEvent.Press:
                {
                    if (_lockTarget == null && raycastHit)
                    {
                        _destination = hit.point;
                    }
                }
                break;

            case Define.MouseEvent.PointerUp:
                _stopSkill = true;
                break;
        }
    }
}
