using UnityEngine;


// パララックス背景を実装するためのスクリプト
public class ParallaxBackground : MonoBehaviour
{
    // パララックス効果の強さを調整するための変数
    [SerializeField]
    private float parallaxEffect = 0.3f;
    // 背景の動く割合（1に近いほどカメラと同じ動きになる）

    // カメラのTransformを保存する変数
    private Transform cameraTransform;
    // 前フレームのカメラ位置を保存する変数
    private Vector3 lastCameraPosition;

    private void Start()
    {
        // メインカメラを取得
        cameraTransform = Camera.main.transform;

        // 初期カメラ位置を保存
        lastCameraPosition = cameraTransform.position;
    }

    // LateUpdateはUpdateの後に呼ばれるため、カメラの動きが確定した後に背景を動かすことができる
    private void LateUpdate()
    {
        // カメラがどれだけ動いたかを計算
        Vector3 deltaMove = cameraTransform.position - lastCameraPosition;

        // 背景は少しだけ追従させる（奥行き表現）
        transform.position += new Vector3(deltaMove.x * parallaxEffect, 0, 0);

        // 次フレーム用にカメラ位置更新
        lastCameraPosition = cameraTransform.position;
    }
}