using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseController : MonoBehaviour
{
    [SerializeField]
    protected Vector3 _destination;

    [SerializeField]
    protected Define.State _state = Define.State.Idle;

    [SerializeField]
    protected GameObject _lockTarget;

    public virtual Define.State State
    {
        get { return _state; }
        set
        {
            _state = value;

            Animator animator = GetComponent<Animator>();
            switch (_state)
            {
                case Define.State.Idle:
                    animator.CrossFade("WAIT", 0.1f);
                    break;

                case Define.State.Moving:
                    animator.CrossFade("RUN", 0.1f);
                    break;

                case Define.State.Skill:
                    animator.CrossFade("ATTACK", 0.1f, -1, 0);
                    break;

                case Define.State.Die:
                    break;
            }
        }
    }


    private void Start()
    {
        Init();
    }


    void Update()
    {
        switch (State)
        {
            case Define.State.Die:
                UpdateDie();
                break;

            case Define.State.Moving:
                UpdateMoving();
                break;

            case Define.State.Idle:
                UpdateIdle();
                break;

            case Define.State.Skill:
                UpdateSkill();
                break;
        }
    }

    protected abstract void Init();
    protected virtual void UpdateDie() { }
    protected virtual void UpdateMoving() { }
    protected virtual void UpdateIdle() { }
    protected virtual void UpdateSkill() { }
}
