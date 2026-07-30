using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// player.csv（key-value形式）から読み込んだPlayerのパラメータ。
/// keyで値を型付き取得する。
///
/// ・キーが無い/空欄 → null を返す（＝未指定。呼び出し側はInspector値へフォールバック。警告なし）
/// ・キーはあるが変換できない → null＋警告（データのミス）
///
/// Playerは1体なので Item/Enemy のような「行の集合」ではなく、
/// key→value の辞書1つとして扱う。
/// </summary>
public class PlayerStats
{
    // key → value（文字列のまま保持し、取得時に型変換する）
    private readonly Dictionary<string, string> values;

    private const string Source = "player.csv";

    public PlayerStats(Dictionary<string, string> values)
    {
        this.values = values;
    }

    public float? GetFloat(string key)
    {
        if (!TryGetRaw(key, out string raw))
        {
            return null;
        }
        if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }
        WarnParseFailed(key, raw, "float");
        return null;
    }

    public int? GetInt(string key)
    {
        if (!TryGetRaw(key, out string raw))
        {
            return null;
        }
        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
        {
            return result;
        }
        WarnParseFailed(key, raw, "int");
        return null;
    }

    public bool? GetBool(string key)
    {
        if (!TryGetRaw(key, out string raw))
        {
            return null;
        }

        switch (raw.ToLowerInvariant())
        {
            case "true":
            case "1":
            case "yes":
            case "on":
                return true;
            case "false":
            case "0":
            case "no":
            case "off":
                return false;
        }

        WarnParseFailed(key, raw, "bool");
        return null;
    }

    public string GetString(string key)
    {
        return TryGetRaw(key, out string raw) ? raw : null;
    }

    //==============================
    // 内部処理
    //==============================

    // keyの生値を取得する。key欠落 or 空欄なら false（＝未指定。ここでは警告を出さない）
    private bool TryGetRaw(string key, out string raw)
    {
        if (!values.TryGetValue(key, out raw) || string.IsNullOrWhiteSpace(raw))
        {
            raw = null;
            return false;
        }
        raw = raw.Trim();
        return true;
    }

    private void WarnParseFailed(string key, string raw, string typeName)
    {
        Debug.LogWarning($"[{Source}] key '{key}' の値 '{raw}' を {typeName} に変換できません。Inspector値を使用します");
    }
}
