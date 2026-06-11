using UnityEngine;

/// <summary>
/// プレイヤーの攻撃処理
/// ・InputReaderから入力を受ける
/// ・Projectileを発射する
/// </summary>
public class PlayerProjectileShooter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("入力管理クラス")]
    [SerializeField] private PlayerInputReader inputReader;

    [Tooltip("Projectileプレハブ")]
    [SerializeField] private Projectile projectilePrefab;

    [Tooltip("発射位置")]
    [SerializeField] private Transform firePoint;

    [Header("Attack Settings")]
    [Tooltip("ダメージ")]
    [SerializeField] private int damage = 1;

    [Tooltip("ノックバック")]
    [SerializeField] private float knockbackPower = 3f;

    [Tooltip("打ち上げ力")]
    [SerializeField] private float launchPower = 1f;

    private void Update()
    {
        if (inputReader == null)
            return;

        if (inputReader.AttackPressed)
        {
            Fire();
        }
    }

    /// <summary>
    /// Projectile発射
    /// </summary>
    private void Fire()
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        Vector2 direction =
            transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        Projectile projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        projectile.Initialize(
            direction,
            damage,
            knockbackPower,
            launchPower,
            gameObject
        );
    }
}