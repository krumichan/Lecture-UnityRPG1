using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    PlayerStat _stat;
    Vector3 _destination;

    Texture2D _attackIcon;
    Texture2D _handIcon;

    enum CursorType
    {
        None
        , Hand
        , Attack
    }

    CursorType _cursorType = CursorType.None;
    
    void Start()
    {
        _attackIcon = Managers.Resource.Load<Texture2D>("Textures/Cursor/Attack");
        _handIcon = Managers.Resource.Load<Texture2D>("Textures/Cursor/Hand");

        _stat = gameObject.GetComponent<PlayerStat>();

        Managers.Input.MouseAction -= OnMouseClicked;
        Managers.Input.MouseAction += OnMouseClicked;
    }

    public enum PlayerState
    {
        Die
        , Moving
        , Idle
        , Skill
    }

    PlayerState _state = PlayerState.Idle;

    void UpdateDie()
    {
        // �ƹ��͵� ����.
    }

    void UpdateMoving()
    {
        Vector3 direction = _destination - transform.position;
        if (direction.magnitude < 0.1f)
        {
            _state = PlayerState.Idle;
        }
        else
        {
            NavMeshAgent nma = gameObject.GetOrAddComponent<NavMeshAgent>();

            // �̵� �Ÿ��� �ݵ�� ����&ũ�� ����(direction)�� ũ�⺸�� �۾ƾ� �Ѵ�.
            // �׷��� ������, ������ �ٷ� �αٿ��� �������� �Դٰ��� �Ѵ�.
            float moveDistance = Mathf.Clamp(_stat.MoveSpeed * Time.deltaTime, 0, direction.magnitude);

            // nma.CalculatePath
            // ũ����� �����ϴ� ���� ����.
            nma.Move(direction.normalized * moveDistance);

            Debug.DrawRay(transform.position, direction.normalized, Color.green);
            // ���� ��� ��ġ���� ��� ����. ( Vector3.up * 0.5f )
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, direction, 1.0f, LayerMask.GetMask("Block")))
            {
                _state = PlayerState.Idle;
                return;
            }

            /*transform.position += direction.normalized * moveDistance;*/
            transform.rotation = Quaternion.Slerp(
                   transform.rotation
                   , Quaternion.LookRotation(direction)
                   , 20 * Time.deltaTime
            );
        }

        // animation
        Animator animator = GetComponent<Animator>();
        // ���� ���� ���¿� ���� ������ ����.
        animator.SetFloat("speed", _stat.MoveSpeed);
    }

     void UpdateIdle()
    {
        // animation
        Animator animator = GetComponent<Animator>();
        animator.SetFloat("speed", 0);
    }

    void Update()
    {
        UpdateMouseCursor();

        switch (_state)
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
        }
    }

    void UpdateMouseCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100.0f, _mask))
        {
            if (hit.collider.gameObject.layer == (int)Define.Layer.Monster)
            {
                if (_cursorType != CursorType.Attack)
                {
                    Cursor.SetCursor(_attackIcon, new Vector2(_attackIcon.width / 5, 0), CursorMode.Auto); // 1��° �μ� = �̹������� ����(��Ŀ)�� �Ǵ� ��.
                    _cursorType = CursorType.Attack;
                }
            }
            else
            {
                if (_cursorType != CursorType.Hand)
                {
                    Cursor.SetCursor(_handIcon, new Vector2(_handIcon.width / 3, 0), CursorMode.Auto);
                    _cursorType = CursorType.Hand;
                }
            }
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

    int _mask = (1 << (int)Define.Layer.Ground) | (1 << (int)Define.Layer.Monster);

    void OnMouseClicked(Define.MouseEvent evt)
    {
        if (_state == PlayerState.Die)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        /*Debug.DrawRay(Camera.main.transform.position, ray.direction * 100.0f, Color.red, 1.0f);*/

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100.0f, _mask))
        {
            _destination = hit.point;
            _state = PlayerState.Moving;

            if (hit.collider.gameObject.layer == (int)Define.Layer.Monster)
            {
                Debug.Log("Monster Click");
            }
            else
            {
                Debug.Log("Ground Click");
            }
        }
    }
}
