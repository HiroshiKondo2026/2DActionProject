using System;
using System.IO;
using UnityEngine;

/// <summary>
/// セーブスロット。オート用と通常用の2つ。
/// </summary>
public enum SaveSlot
{
    Auto,
    Manual
}

/// <summary>
/// セーブデータのディスク読み書きを担当する。
/// 保存先は Application.persistentDataPath（プラットフォームごとのユーザーデータ領域）。
/// どんな失敗でもゲームを落とさず、ログを出して null / false を返す。
/// </summary>
public static class SaveSystem
{
    // スロットごとのファイル名
    private const string AutoFileName = "save_auto.json";
    private const string ManualFileName = "save_manual.json";

    // スロット → フルパス
    private static string FilePath(SaveSlot slot)
    {
        string fileName = slot == SaveSlot.Auto ? AutoFileName : ManualFileName;
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    /// <summary>
    /// 指定スロットへ保存する。savedAtTicksはここで現在時刻を入れる。
    /// </summary>
    public static void Save(SaveSlot slot, SaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning($"[SaveSystem] data が null のため保存しません slot:{slot}");
            return;
        }

        try
        {
            // 保存時刻を刻む（スロットの新旧比較・表示用）
            data.savedAtTicks = DateTime.UtcNow.Ticks;

            // 第2引数true = 人が読める整形付きJSON
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(FilePath(slot), json);

            Debug.Log($"[SaveSystem] セーブしました slot:{slot} path:{FilePath(slot)}");
        }
        catch (Exception e)
        {
            // 書き込み失敗（容量/権限等）でも落とさない
            Debug.LogError($"[SaveSystem] セーブに失敗しました slot:{slot}\n{e.GetType().Name}: {e.Message}");
        }
    }

    /// <summary>
    /// 指定スロットを読み込む。ファイルが無い/壊れている場合は null を返す。
    /// </summary>
    public static SaveData Load(SaveSlot slot)
    {
        string path = FilePath(slot);

        // ファイルが無い＝セーブがない（正常系。エラーにしない）
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                Debug.LogWarning($"[SaveSystem] セーブデータを解析できませんでした slot:{slot}");
            }

            return data;
        }
        catch (Exception e)
        {
            // 壊れたファイル等でも落とさず null（呼び出し側はNewGame相当に倒す）
            Debug.LogError($"[SaveSystem] ロードに失敗しました slot:{slot}\n{e.GetType().Name}: {e.Message}");
            return null;
        }
    }

    /// <summary>指定スロットのセーブが存在するか（UIの活性/非活性判定に使う）。</summary>
    public static bool Exists(SaveSlot slot)
    {
        return File.Exists(FilePath(slot));
    }

    /// <summary>指定スロットのセーブを削除する。</summary>
    public static void Delete(SaveSlot slot)
    {
        string path = FilePath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveSystem] セーブを削除しました slot:{slot}");
        }
    }
}
