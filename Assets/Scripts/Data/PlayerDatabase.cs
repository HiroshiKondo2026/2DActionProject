using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StreamingAssets/player.csv（key-value形式）を読み込むためのDatabase。
///
/// Item/Enemyと違いPlayerは1体なので「id→データ」ではなく、
/// key→value の辞書1つ（PlayerStats）を保持する。
/// PlayerController / PlayerCombat / PlayerHealth が共通してここから取得する。
/// </summary>
public static class PlayerDatabase
{
    private const string CsvFileName = "player.csv";

    // 読み込んだPlayerパラメータ。null = 未ロード（EnsureLoadedで生成）
    private static PlayerStats stats;

    // ファイルが無い等でも stats は空の状態で生成される（全項目フォールバック）
    private static bool loaded;

    /// <summary>
    /// Playerパラメータを取得する。CSVが無くても空のPlayerStatsを返す（nullは返さない）。
    /// </summary>
    public static PlayerStats Get()
    {
        EnsureLoaded();
        return stats;
    }

    /// <summary>CSVを読み込み直す（データ編集の確認用）。</summary>
    public static void Reload()
    {
        loaded = false;
        EnsureLoaded();
    }

    //==============================
    // 内部処理
    //==============================

    private static void EnsureLoaded()
    {
        if (loaded)
        {
            return;
        }
        loaded = true;

        var dict = new Dictionary<string, string>();

        // player.csv は「key,value」の2列。各行を key→value として辞書化する
        List<CsvRow> rows = CsvLoader.Load(CsvFileName);

        foreach (CsvRow row in rows)
        {
            string key = row.GetStringOrNull("key");

            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            if (dict.ContainsKey(key))
            {
                Debug.LogWarning($"[PlayerDatabase] {CsvFileName}: key '{key}' が重複しています。最初の行を使用します");
                continue;
            }

            // value は空欄(null)もありうる。その場合PlayerStats側で未指定として扱われる
            dict[key] = row.GetStringOrNull("value");
        }

        stats = new PlayerStats(dict);
        Debug.Log($"[PlayerDatabase] {CsvFileName} から {dict.Count} 件のパラメータを読み込みました");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        loaded = false;
        stats = null;
    }
}
