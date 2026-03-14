// using UnityEngine;
// using UnityEngine.AI;
// using System;
// using System.Collections.Generic;

// [Serializable]
// public class EnemyAI : MonoBehaviour
// {
//     public Transform Target;
//     public float AttackDistance;
//     public float CloseChaseDistance;
//     public float FarChaseDistance;
//     public float MoveSpeed;
//     public float SprintSpeed;
//     private NavMeshAgent m_Agent;
//     private float m_Distance;

//     // Start is called before the first frame update
//     void Start()
//     {
//         m_Agent = GetComponent<NavMeshAgent>();
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         EnemyAILogic();
//     }

//     private void EnemyAILogic()
//     {
//         m_Distance = Vector3.Distance(m_Agent.transform.position, Target.position);
//         // Debug.Log(m_Distance);
//         if (m_Distance < AttackDistance)
//         {
//             m_Agent.isStopped = true;
//             m_Agent.speed = 0;
//         }
//         else if (m_Distance < CloseChaseDistance)
//         {
//             m_Agent.isStopped= false;
//             m_Agent.destination = Target.position;
//             m_Agent.speed = MoveSpeed;
//         }
//         else if (CloseChaseDistance <= m_Distance && m_Distance <= FarChaseDistance)
//         {
//             m_Agent.isStopped = false;
//             m_Agent.destination = Target.position;
//             m_Agent.speed = SprintSpeed * 1.45f;
//         }
//         else
//         {
//             m_Agent.isStopped = true;
//             m_Agent.speed = 0;
//         }
//     }
// }
