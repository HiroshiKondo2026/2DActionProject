using UnityEngine;

/// <summary>
/// STEP1の動作確認用デバッグツール。
/// 適当なGameObjectに付け、Inspectorでコンポーネントを右クリック → 各メニューで動作を確認する。
/// （実際のセーブ収集はSTEP2で実装するため、ここではダミーデータで読み書きだけ検証する）
/// ※確認が済んだら削除してよい。
/// </summary>
public class SaveSystemDebug : MonoBehaviour
{
    [ContextMenu("① 保存先パスをログ表示")]
    private void LogPath()
    {
        Debug.Log($"[SaveDebug] persistentDataPath: {Application.persistentDataPath}");
    }

    [ContextMenu("② テスト用データを書き込み(Manualスロット)")]
    private void WriteTest()
    {
        var data = new SaveData
        {
            currentHP = 42,
            maxHP = 100,
            sceneName = "Stage1_2",
            equippedId = "Knife",
            items = new[]
            {
                new SavedItem { id = "Knife", count = 3 },
                new SavedItem { id = "Potion", count = 5 },
            },
            defeatedEnemyIds = new[] { "test-guid-1", "test-guid-2" },
        };

        SaveSystem.Save(SaveSlot.Manual, data);
    }

    [ContextMenu("③ 読み込んでログ表示(Manualスロット)")]
    private void ReadTest()
    {
        SaveData data = SaveSystem.Load(SaveSlot.Manual);
        if (data == null)
        {
            Debug.Log("[SaveDebug] Manualスロットのセーブデータはありません");
            return;
        }

        Debug.Log($"[SaveDebug] hp:{data.currentHP}/{data.maxHP} scene:{data.sceneName} " +
                  $"equipped:{data.equippedId} items:{data.items.Length} defeated:{data.defeatedEnemyIds.Length}");
    }

    [ContextMenu("④ Manualスロットを削除")]
    private void DeleteTest()
    {
        SaveSystem.Delete(SaveSlot.Manual);
    }
}
