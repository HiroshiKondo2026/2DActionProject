using System;

/// <summary>
/// セーブ1件分のデータ（JsonUtilityでJSON化できる素の構造）。
/// 保存するのは「進行状況」のみ。player.csv等の調整値は保存しない（起動ごとにCSVから読む）。
/// </summary>
[Serializable]
public class SaveData
{
    // 将来フォーマットを変えたときの互換判定用
    public int version = 1;

    // 保存した時刻（UTCのTicks）。スロットの新旧比較・表示に使う
    public long savedAtTicks;

    // --- 進行状況 ---
    public int currentHP;
    public int maxHP;

    // ロード時に頭から開始するシーン
    public string sceneName;

    // 装備中アイテムのID（未装備なら空）
    public string equippedId;

    // 所持アイテム（ItemDataの参照は保存できないのでID＋個数で持つ）
    public SavedItem[] items = new SavedItem[0];

    // このシーンで撃破済みのEnemyのID（ロード時に復活させない＝farming防止）
    public string[] defeatedEnemyIds = new string[0];
}

/// <summary>
/// 所持アイテム1件分（ID＋個数）。ロード時にItemAssetDatabaseでID→ItemDataへ解決する。
/// </summary>
[Serializable]
public class SavedItem
{
    public string id;
    public int count;
}
