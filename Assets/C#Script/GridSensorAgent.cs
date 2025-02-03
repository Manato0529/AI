using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using Unity.IO.LowLevel.Unsafe;

public class GridSensorAgent : Agent
{
    public List<Transform> targets;
    Rigidbody rBody;

    int count;
    private float previousDistance; // 前回の距離を記録
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f);
            this.transform.Rotate(new Vector3(0, 10, 0), 90);
        }
    }

    public override void OnEpisodeBegin()
    {
        if (this.transform.position.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }
        Generate.Instance.ResetTargets();
        targets.Clear();
        for(int i=0;i<Generate.Instance.targets.Count;i++)
        {
            targets.Add(Generate.Instance.targets[i]);
        }

        // カウントを初期化
        count = 0;

        // ランダムにターゲットの位置を再設定
        Generate.Instance.ShuffleProc();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // 動きや回転処理はそのまま
        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;
        int action = actions.DiscreteActions[0];

        if (action == 1) dirToGo = transform.forward;
        if (action == 2) dirToGo = transform.forward * -1.0f;
        if (action == 3) rotateDir = transform.up * -1.0f;
        if (action == 4) rotateDir = transform.up;
        transform.Rotate(rotateDir, Time.deltaTime * 200f);
        rBody.AddForce(dirToGo * 0.4f, ForceMode.VelocityChange);

        // 最も近いターゲットを取得
        Transform closestTarget = GetClosestTarget();
        if (closestTarget != null)
        {
            float currentDistance = Vector3.Distance(this.transform.localPosition, closestTarget.localPosition);

            // 距離が縮まった場合の報酬
            if (currentDistance < previousDistance)
            {
                AddReward(0.01f);
            }
            else
            {
                AddReward(-0.01f);
            }

            previousDistance = currentDistance;

            // ターゲットに到達した場合
            if (currentDistance < 1.42f)
            {
                Target target = closestTarget.GetComponent<Target>();
                if (target != null)
                {
                    float reward = 0.5f + (target.priority * 0.5f);
                    AddReward(reward);

                    count++;

                    if (count == Generate.Instance.targets.Count)
                    {
                        EndEpisode(); // 全ターゲット探索で終了
                    }
                }
            }
        }

        // 落下したらエピソードを終了
        if (this.transform.position.y < 0)
        {
            EndEpisode();
        }
    }

    private Transform GetClosestTarget()
    {
        Transform closestTarget = null;
        float maxPriority = float.MinValue; // 優先度が高いターゲットを探すため
        float minDistance = float.MaxValue;

        foreach (Transform target in targets)
        {
            Target targetScript = target.GetComponent<Target>();
            if (targetScript != null && targetScript.isExplored)
            {
                continue; // 探索済みターゲットをスキップ
            }

            float distance = Vector3.Distance(this.transform.localPosition, target.localPosition);

            // 優先度が高いターゲットを優先、同じ優先度なら距離が近いものを選ぶ
            if (targetScript.priority > maxPriority ||
                (targetScript.priority == maxPriority && distance < minDistance))
            {
                maxPriority = targetScript.priority;
                minDistance = distance;
                closestTarget = target;
            }
        }

        return closestTarget;
    }
}
