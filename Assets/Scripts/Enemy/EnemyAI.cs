using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform Target;
    public float AttackDistance;
    public float ChaseDistance;
    public float WalkSpeed;
    private NavMeshAgent m_Agent;
    private float m_Distance;

    // Start is called before the first frame update
    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        m_Distance = Vector3.Distance(m_Agent.transform.position, Target.position);
        Debug.Log(m_Distance);
        if (m_Distance < AttackDistance)
        {
            m_Agent.isStopped = true;
            m_Agent.speed = 0;
        }
        else if(m_Distance < ChaseDistance)
        {
            m_Agent.isStopped= false;
            m_Agent.destination = Target.position;
            m_Agent.speed = WalkSpeed;
        }
        else
        {
            m_Agent.isStopped = false;
            m_Agent.destination = Target.position;
            m_Agent.speed = WalkSpeed * 1.45f;
        }
    }
}
