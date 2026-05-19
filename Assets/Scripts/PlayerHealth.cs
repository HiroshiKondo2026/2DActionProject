using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // 最大HP
    [SerializeField]
    private int maxHP = 5;

    // 現在HP
    private int currentHP;

    // 無敵状態の有無
    private bool isInvincible = false;

    // 無敵時間用変数
    [SerializeField]
    private float invincibleTime = 1.0f;

    // ゲーム開始時に呼ばれる
    private void Start()
    {
        // HP初期化
        currentHP = maxHP;

        Debug.Log("Player HPを最大HPで初期化しました: " + currentHP);
    }

    // ダメージを受ける処理
    public void TakeDamage(int damage)
    {
        // 無敵中ならダメージ無効
        if (isInvincible)
        {
            Debug.Log("無敵中だぜ");

            return;
        }
        // HPをダメージで減少させる
        currentHP -= damage;

        Debug.Log("Playerは" + damage + " のダメージを受けた");
        Debug.Log("現在HP: " + currentHP);

        StartCoroutine(InvincibleCoroutine());

        // HP0以下なら死亡
        if (currentHP <= 0)
        {
            Die();
        }
    }

    // HP0時処理
    private void Die()
    {
        Debug.Log("Playerは死んでしまった");

        // 仮で非表示
        gameObject.SetActive(false);
    }

    // 一定時間無敵状態にする
    private System.Collections.IEnumerator InvincibleCoroutine()
    {
        // 無敵ON
        isInvincible = true;

        Debug.Log("無敵開始");

        // 指定秒待機
        yield return new WaitForSeconds(invincibleTime);

        // 無敵OFF
        isInvincible = false;

        Debug.Log("無敵時間終了");
    }
}