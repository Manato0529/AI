using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Generate : MonoBehaviour
{
    // シングルトンパターン
    public static Generate Instance { get; private set; }

    // 生成範囲
    public GameObject erea;          

    // 生成するオブジェクトのPrefab
    public GameObject obstaclePrefab; 
    public GameObject targetAPrefab;
    public GameObject targetBPrefab;
    public GameObject targetCPrefab;

    // 生成数
    [SerializeField, MaxValue(100)] int obstacleNum;
    [SerializeField, MaxValue(20)] int targetANum;
    [SerializeField, MaxValue(10)] int targetBNum;
    [SerializeField, MaxValue(5)] int targetCNum; 

    // エージェントに渡すターゲットのリスト
    public List<Transform> targets = new List<Transform>();
    public List<int> prioritys = new List<int>();
    // 全体のリスト
    public List<GameObject> objects = new List<GameObject>(); 

    private List<Vector3> availablePositions = new List<Vector3>();

    private void Start()
    {
        GenerateProc();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void GenerateProc()
    {
        EreaCheck();

        // 生成
        GeneratePrefab(obstaclePrefab, obstacleNum,0);
        GeneratePrefab(targetAPrefab, targetANum,1);
        GeneratePrefab(targetBPrefab, targetBNum,5);
        GeneratePrefab(targetCPrefab, targetCNum,10);
    }

    public void ShuffleProc()
    {
        EreaCheck();

        // 既存のオブジェクトの座標をランダムに変更
        foreach (var obj in objects)
        {
            
            // 新しい座標を取得
            if (availablePositions.Count > 0)
            {
                Vector3 newPosition = availablePositions[0]; // ランダムに選ばれた位置
                availablePositions.RemoveAt(0); // 使った位置を削除

                // オブジェクトの座標を更新
                obj.transform.position = newPosition;
            }
        }

        // 座標が重ならないように再シャッフル
        NewPosition(availablePositions);
    }

    public void ResetTargets()
    {
        foreach (var target in targets)
        {
            target.GetComponent<Target>().isExplored = false;  // すべてのターゲットを未探索に戻す
        }
    }

    private void GeneratePrefab(GameObject _prefab, int _generateNum, int _p)
    {
        for (int i = 0; i < _generateNum && availablePositions.Count > 0; i++)
        {
            // ランダムな空き座標を取得
            Vector3 spawnPosition = availablePositions[0];
            availablePositions.RemoveAt(0);

            // オブジェクトを生成
            GameObject obj = Instantiate(_prefab, spawnPosition, Quaternion.identity);

            // 生成したオブジェクトがターゲットの場合、priorityを設定
            if (_prefab == targetAPrefab || _prefab == targetBPrefab || _prefab == targetCPrefab)
            {
                Target target = obj.GetComponent<Target>();
                if (target != null)
                {
                    target.priority = _p; // priorityに_pの値を設定
                }

                // targetsリストに追加
                targets.Add(obj.transform);
            }

            // 全体リストに追加
            objects.Add(obj);
        }
    }

    private void EreaCheck()
    {
        // 生成範囲のRendererを取得
        Renderer ereaRenderer = erea.GetComponent<Renderer>();
        if (ereaRenderer == null) { return; }

        // 生成範囲のサイズを取得
        Vector3 ereaSize = ereaRenderer.bounds.size;

        // 生成範囲の中心位置を取得
        Vector3 ereaCenter = ereaRenderer.bounds.center;

        // 生成可能な座標をリストアップ
        InitializeAvailablePositions(ereaSize, ereaCenter);
    }

    private void InitializeAvailablePositions(Vector3 ereaSize, Vector3 ereaCenter)
    {
        // 生成範囲の制限値を計算
        float minX = ereaCenter.x - ereaSize.x / 2;
        float maxX = ereaCenter.x + ereaSize.x / 2;
        float minZ = ereaCenter.z - ereaSize.z / 2;
        float maxZ = ereaCenter.z + ereaSize.z / 2;

        availablePositions.Clear();

        for (int x = Mathf.FloorToInt(minX); x <= Mathf.CeilToInt(maxX); x++)
        {
            for (int z = Mathf.FloorToInt(minZ); z <= Mathf.CeilToInt(maxZ); z++)
            {
                // 範囲内の座標のみリストに追加
                if (x >= minX && x <= maxX && z >= minZ && z <= maxZ)
                {
                    Vector3 position = new Vector3(x, ereaCenter.y + 0.5f, z);
                    availablePositions.Add(position);
                }
            }
        }

        // 座標をシャッフル
        NewPosition(availablePositions);
    }

    private void NewPosition(List<Vector3> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            Vector3 temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
