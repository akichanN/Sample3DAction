//using UnityEngine;
//using System.Linq;
//using System.Collections;
//using System.Collections.Generic;

//[RequireComponent(typeof(MeshRenderer))]
//[RequireComponent(typeof(MeshFilter))]
//[RequireComponent(typeof(Rigidbody))]

//// 三点の頂点から三角形のメッシュを生成するクラス
//public class MeshCreate : SingletonMonoBehaviour<MeshCreate>
//{
//    [Header("基本設定値")]
//    [SerializeField] private Rigidbody rb;
//    [SerializeField] private LineRenderer lr;
//    [SerializeField] private GameObject meshVertices;
//    [SerializeField] private Mesh mesh; 
//    [SerializeField] private Material m_material = null;
//    [SerializeField] private MeshCollider m_meshCollider = null;

//    // Singleton
//    private GrapplingUI g_UI;


//    [Header("メッシュ動作処理の設定値")]
//    [SerializeField] private float moveDownDelayTime; // メッシュを降下させる間隔

//    private Stack<Vector3> playerPosStack = new Stack<Vector3>(); // 三角形固定ならstack
//    private List<GameObject> meshPositions = new List<GameObject>(); // 頂点の座標に生成するオブジェクトを管理するリスト
//    private Vector3[] temp; // メッシュの頂点を一時的に保持しておく配列
//    private GameObject m_Prefab; // 生成するプレハブの一時格納用の変数
//    private int pushCount = 0; // 頂点の格納した回数をカウントする変数
//    private int lineCount = 0; // lineRendererの頂点数
//    private float hight_PosY = 0; // y軸の最大値を格納する変数
//    private float check_PosY = 0; // 生成されたメッシュの終点値
    
//    private bool isCreate;  // メッシュが生成されたらtrues
//    private bool isMeshDieFlag; // メッシュが削除される条件がそろったらtrue

//    // routine
//    private IEnumerator meshMoveRoutine; // メッシュを一定間隔で降下させるroutine

//    [Header("効果音")]
//    [SerializeField] private AudioClip verticesCreateSE;
//    [SerializeField] private AudioClip meshCreateSE;


//    private void Start()
//    {
//        mesh = new Mesh();

//        // Singleton
//        g_UI = GrapplingUI.Instance;

//    }

//    private void FixedUpdate()
//    {
//        // 格納した頂点の数をUI反映
//        g_UI.MeshCreatePosCounter(pushCount);

//        // メッシュ生成が完了していたら実行
//        if (isCreate)
//        {
//            // routineがNULL(routineが稼働していなければ)実行
//            if (meshMoveRoutine == null)
//            {
//                rb.isKinematic = false;
//                meshMoveRoutine = MeshMoveRoutine();
//                // メッシュの降下routineを開始
//                StartCoroutine(meshMoveRoutine);
//            }

//            // メッシュが一定の座標よりも下に降りたら削除する
//            if(gameObject.transform.position.y < -check_PosY)
//            {
//                //Debug.Log("メッシュ削除座標：" + gameObject.transform.position.y);
//                //Debug.Log("この座標まで降下します" + -check_PosY);
//                // 削除
//                Destroy(mesh);

//                // 初期化
//                mesh = new Mesh();
//                isCreate = false;
//                isMeshDieFlag = true;
//                check_PosY = 0;
//                gameObject.transform.position = new Vector3(0, 0, 0);
//            }
//        }
//        else // メッシュを生成していない間
//        {
//            // 動かないようにする
//            rb.isKinematic = true;

//            // routineが入っていたら
//            if (meshMoveRoutine != null)
//            {
//                // routineを停止
//                StopCoroutine(meshMoveRoutine);
//                // 解放
//                meshMoveRoutine = null;
//            }
//        }
//    }

//    /// <summary>
//    /// メッシュ生成用の頂点を受け取るメソッド
//    /// </summary>
//    /// <param name="pos">頂点として使用する座標</param>
//    public void PushPos(Vector3 pos)
//    {
//        // メッシュ生成に使用するの頂点座標を送信
//        playerPosStack.Push(transform.TransformPoint(pos));
//        pushCount = Mathf.Clamp(pushCount + 1, 0, 3);
        
//        // 格納した頂点の場所に目印になるオブジェクトを生成する
//        m_Prefab = Instantiate(meshVertices, transform.TransformPoint(pos), Quaternion.identity);

//        // 頂点オブジェクト管理用のリストに追加する
//        meshPositions.Add(m_Prefab);

//        //AudioManager2D.Instance.AudioSE.PlayOneShot(verticesCreateSE);

//        // 目印のオブジェクトが3個以上になったら実行
//        if (meshPositions.Count > 3)
//        {
//            // 一番古いオブジェクトを削除
//            var deleteObj = meshPositions.First();
           
//            // 削除するオブジェクトが無かったら実行
//            if(deleteObj == null)
//            {
//                Debug.LogError("deleteObj : NULL");
//                Debug.Break();
//            }

//            Destroy(deleteObj);

//            // 削除した先頭要素に残りの要素を詰める
//            meshPositions = new List<GameObject>() 
//            {
//                meshPositions[1],
//                meshPositions[2],
//                meshPositions[3],
//            };
//        }

//        DrawAuxiliaryLine();
//    }


//    /// <summary>
//    /// 視覚的にメッシュの形状を認識できる用にする補助線を生成するメソッド
//    /// </summary>
//    private void DrawAuxiliaryLine()
//    {

//        // 1回目(線なし)
//        if (lineCount == 0)
//        {
//            lineCount++;
//            return;
//        }
//        // 2回目(直線)
//        else if (lineCount == 1)
//        {
//            // 頂点数2
//            lr.positionCount = lineCount + 1;
//            lineCount++;

//            // 頂点を設定して線を引く
//            lr.SetPosition(0, meshPositions[0].transform.position);
//            lr.SetPosition(1, meshPositions[1].transform.position);

//            return;
//        }

//        // 頂点数3以上（三角形生成）
//        lr.positionCount = lineCount + 1;

//        // 頂点の始点値と終点値を結合する
//        lr.loop = true; 

//        // 頂点を設定して線を引く
//        lr.SetPosition(0, meshPositions[0].transform.position);
//        lr.SetPosition(1, meshPositions[1].transform.position);
//        lr.SetPosition(2, meshPositions[2].transform.position);
        
//    }    


//    /// <summary>
//    /// スタックされた座標にメッシュを作る
//    /// </summary>
//    public void CreatTriangleMesh()
//    {
//        // 頂点の個数チェック
//        if (pushCount < 3)
//        {
//            Debug.Log("頂点不足");
//            return;
//        }

//        //AudioManager2D.Instance.AudioSE.PlayOneShot(meshCreateSE);
//        #region デバッグ１
//        //int a = 0;
//        //foreach (var i in playerPosStack)
//        //{
//        //    Debug.Log("playerPosStack" + a + "個目の座標： " + i + "　型：" + i.GetType());
//        //    a++;
//        //}
//        #endregion

//        // 座標の一時保存
//        // 通常の配列型出ないと値の変更ができないため追加
//        temp = new Vector3[] 
//        {
//            playerPosStack.Pop() + -2 * this.gameObject.transform.position,
//            playerPosStack.Pop() + -2 * this.gameObject.transform.position,
//            playerPosStack.Pop() + -2 * this.gameObject.transform.position,
//        };

//        #region　デバッグ２
//        // テスト用
//        //int c = 0;
//        //foreach (var i in temp)
//        //{
//        //    Debug.Log("temp" + c + "個目の座標： " + i + "　型：" + i.GetType());
//        //    c++;
//        //}
//        #endregion

//        // y軸が一番高い位置にある座標を取得する
//        hight_PosY = temp.Max(t => t.y);
//        check_PosY = hight_PosY;

//        // 全てのy座標を一番高い位置に固定する
//        for (int i = 0; i < temp.Length; i++)
//        {
//            // y軸に代入
//            temp[i].y = hight_PosY;
//        }

//        #region　デバッグ３
//        //// テスト用
//        //int c = 0;
//        //foreach (var i in temp)
//        //{
//        //    Debug.Log("temp" + c + "個目の座標： " + i + "　型：" + i.GetType());
//        //    c++;
//        //}
//        #endregion

//        // メッシュを生成する頂点の格納
//        mesh.vertices = temp;

//        // スタックの中身を消す
//        ClearStack();

//        mesh.triangles = new int[]
//        {
//            0, 1, 2,
//            //0, 2, 1,
//            //0, 3, 1
//        };

//        // 頂点を計算し三角形の反映させる
//        mesh.RecalculateNormals();

//        var filter = GetComponent<MeshFilter>();
//        filter.sharedMesh = mesh;
        
//        var renderer = GetComponent<MeshRenderer>();
//        renderer.material = m_material;

//        m_meshCollider.sharedMesh = mesh;
//        //Debug.Log("メッシュを作成しました");

//        isCreate = true;
//        hight_PosY = 0;
//    }

//    /// <summary>
//    /// メッシュ生成後に実行するメソッド
//    /// </summary>
//    public void ClearStack()
//    {
//        var loopCounter = 0;

//        // Listに保存したオブジェクトを全て削除する
//        foreach(var obj in meshPositions)
//        {
//            Destroy(obj);
//            loopCounter++;

//            // 4回目なら抜ける
//            if (loopCounter > 3)
//            {
//                break;   
//            }
//        }

//        // 初期化
//        playerPosStack.Clear();
//        meshPositions.Clear();
//        pushCount = 0;
//        lineCount = 0;
//        lr.positionCount = 0;
//    }

//    /// <summary>
//    /// メッシュを一定の間隔で降下させるroutine
//    /// </summary>
//    /// <returns></returns>
//    private IEnumerator MeshMoveRoutine()
//    {
//        // 自動回復開始までの待機時間
//        yield return new WaitForSeconds(moveDownDelayTime);

//        do
//        {
//            // 自動回復の間隔
//            yield return new WaitForSeconds(moveDownDelayTime);

//            // 降下
//            rb.velocity += Vector3.down;

//        }
//        while (!isMeshDieFlag); // 削除条件が通らない間ループ

//        // routineの解放
//        meshMoveRoutine = null;
//    }


//    /// <summary>
//    /// 当たり判定検知（Trigger）
//    /// </summary>
//    /// <param name="other"></param>
//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.gameObject.CompareTag("Enemy"))
//        {
//            var otherUnitBase = other.gameObject.GetComponent<UnitBase>();

//            // UnitBaseがNullなら実行
//            if (otherUnitBase == null)
//            {
//                // 親オブジェクトにあるか取得
//                var otherParent = other.transform.root.gameObject.GetComponent<UnitBase>();

//                // 親オブジェクトにもなっかたら実行
//                if(otherParent == null)
//                {
//                    Debug.LogError("UnitBaseの取得に失敗しました\nUnitBaseがアタッチされているか確認をしてください。");
//                }
//                // UnitBaseがあれば実行
//                else
//                {
//                    // ダメージを与える
//                    otherParent.HitDamage(1000);
//                }

//            }
//            // UnitBaseがあれば実行
//            else
//            {
//                // ダメージを与える
//                otherUnitBase.HitDamage(1000);
//            }
//        }
//    }

//   /// <summary>
//   /// デバッグ用 
//   /// </summary>
//   /// <param name="mesh"></param>
//    //private void DebugStack(Mesh mesh)
//    //{
//    //    // デバッグ用
//    //    // リスト内の要素を表示する
//    //    int c = 0;
//    //    foreach (var i in mesh.vertices)
//    //    {
//    //        //Debug.Log("最大値に合わせた座標" + c + "個目の座標： " + i + "　型：" + i.GetType());
//    //        Debug.Log($"現在保持しているStack：{playerPosStack}");
//    //        c++;
//    //    }

//    //}

//}
