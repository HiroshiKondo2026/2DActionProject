using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // 移動速度設定
    [SerializeField]
    private float moveSpeed = 2f;

    // 右向きかどうか
    private bool isFacingRight = true;

    // Rigidbody2D
    private Rigidbody2D rb;

    // 初期化
    private void Awake()
    {
        // Rigidbody2D取得
        rb = GetComponent<Rigidbody2D>();
    }

    // 物理更新
    private void FixedUpdate()
    {
        // 移動方向を取得　右なら正、左なら負
        float moveDirection = isFacingRight ? 1f : -1f;

        // 左右移動 　今の移動速度を正負(左右)方向へmoveSpeedの速度で進行させる。(linearVelocity.yは今の重力のままにするという宣言)
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
    }

    // OnCollisionEnter2D:
    // 衝突時処理
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Groundと衝突したら反転
        if (collision.gameObject.CompareTag("Wall"))
        {
            Flip();
        }
    }

    // Flip:
    // 向き変更
    private void Flip()
    {
        // 向き反転
        isFacingRight = !isFacingRight;

        // Scale取得
        Vector3 scale = transform.localScale;

        // X反転
        scale.x *= -1;

        // Scale反映
        transform.localScale = scale;
    }
}