using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 軍種
/// </summary>
public enum ArmyType
{
    Player, // プレイヤー
    Ally, // プレイヤーの友軍
    Enemy, // 敵
    Count
}

/// <summary>
/// Unit共通のstatus
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class UnitBase : MonoBehaviour
{
    //// 外部から読み取り専用のプロパティ
    //public int MaxHp => maxHp;
    //public int Hp => hp;
    //public Transform hitPos; // 攻撃されるときの座標
    //protected GameManager game;

    public ArmyType armyType;

    public float unitHp; // ユニットの現在の体力
    public float unitMaxHp = 1000; // ユニットの最大体力
    

    protected virtual void Start()
    {
        Init();
        //playerだけAwakeでInit()実行したいかも
        //（AwakeでInit();してStartでhpとか取得したい）
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    protected virtual void Init()
    {
        // 体力を最大値に合わせる
        unitHp = unitMaxHp;

        // 自身のタグを変更する
        //tag = armyType.ToString();
        // ユニットリストに自身を追加
        //game = GameManager.Instance;
        //game.Unit.AddUnit(this);
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    public abstract void Move();

    /// <summary>
    /// 攻撃処理
    /// </summary>
    public abstract void Attack();


    public void StatusList()
    {

    }


    /// <summary>
    /// ダメージ処理
    /// </summary>
    /// <param name="damage">与えるダメージ</param>
    /// <param name="buff"></param>
    /// <returns></returns>
    public virtual void HitDamage(float damage, float buff = 1.0f)
    {
        // HPが0よりも低くならないように制限する処理
        unitHp = Mathf.Clamp(unitHp - damage * buff, 0, unitMaxHp);

        // HPが0になったら
        if (unitHp <= 0)
        {
            // 死亡処理を呼ぶ
            Death();
        }
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    protected abstract void Death();



}

