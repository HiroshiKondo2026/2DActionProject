# CSVロード処理フロー（Item / Enemy）

CSVデータの読み込み処理の全体フローと、不正データに対する耐性（フォールバック・エラー処理）をまとめる。
各処理には担当メソッド名を記載する。

---

## 1. 全体像

CSVロードは「**共通の読み込み基盤**」＋「**データ種別ごとのDatabase**」＋「**受け手（SO / MonoBehaviour）**」の3層で構成される。

```text
                    ┌─────────────────────────────────────────┐
                    │ 共通基盤（全データ種別で使い回す）        │
                    │  CsvLoader.Load → Parse → ParseRecords   │
                    │            └→ CsvRow（型付きアクセサ）    │
                    └─────────────────────────────────────────┘
                          ▲                         ▲
          ┌───────────────┘                         └───────────────┐
   ┌──────────────────────────┐              ┌──────────────────────────┐
   │ Item系                    │              │ Enemy系                   │
   │  ItemDatabase             │              │  EnemyDatabase            │
   │  ItemAssetDatabase        │              │  EnemyHealth.ApplyCsvStats│
   │  ItemData(プロパティ分岐) │              │  EnemyAI.ApplyCsvStats    │
   └──────────────────────────┘              └──────────────────────────┘
```

---

## 2. 共通の読み込みフロー（CsvLoader）

どのCSVも、まずこの共通処理を通る。

```text
呼び出し： CsvLoader.Load(fileName)
   │
   ├─ StreamingAssets のパスを組み立て（Application.streamingAssetsPath + fileName）
   │
   ├─ File.Exists でファイル存在チェック
   │     └─ 無ければ Debug.LogError → 空リストを返す（＝全項目フォールバック）
   │
   ├─ File.ReadAllText(UTF-8) で読み込み
   │     └─ 例外（IO/権限/その他）は try-catch で捕捉 → Debug.LogError → 空リスト
   │
   └─ CsvLoader.Parse(text, fileName)
         │
         ├─ 先頭のBOM(U+FEFF)を除去
         │
         ├─ CsvLoader.ParseRecords(text)
         │     └─ 1文字ずつ走査し、引用符を考慮して行・列へ分解
         │        （引用符内のカンマ・改行はそのまま値として扱う）
         │
         ├─ コメント行（行頭 #）と空行を除外
         │
         ├─ 最初の有効行を「ヘッダー（列名）」として採用
         │
         └─ 2行目以降を CsvRow として生成してリスト化
               （列名 → 値 の Dictionary を保持）
```

### CsvRow：1行分の型付きアクセサ

`CsvRow` は列名をキーに値を取り出す。**列が無い / 空欄 / 変換失敗でも落ちない**。

| メソッド | 挙動 |
| --- | --- |
| `GetStringOrNull(列名)` | 未指定なら null。`\n` は改行へ変換 |
| `GetIntOrNull(列名)` | 未指定・変換失敗なら null（変換失敗は警告） |
| `GetFloatOrNull(列名)` | 同上 |
| `GetBoolOrNull(列名)` | 同上（true/false/1/0/yes/no/on/off対応） |
| `GetString/GetInt/GetFloat/GetBool(列名, 既定値)` | 上記の「未指定なら既定値」版 |

内部の `CsvRow.TryGetRaw` が「列が無い→警告」「空欄→未指定扱い（警告なし）」を判定する。

---

## 3. Item系のフロー

### 3-1. 数値データ（item.csv → ItemData のプロパティ）

```text
ItemData.Cooldown など各プロパティ参照
   │
   └─ ItemData.Stats（= ItemDatabase.GetStats(Id)）を見る
         │
         ├─ ItemDatabase.GetStats(id)
         │     └─ ItemDatabase.EnsureLoaded（初回のみ）
         │           └─ CsvLoader.Load("item.csv")
         │           └─ ItemDatabase.CreateStats で各行を ItemStats 化
         │
         ├─ Stats が非null（CSVに該当id行あり）
         │     └─ 「Stats?.cooldown ?? cooldown」→ CSV値を返す
         │        （列が空欄なら Stats.cooldown が null → Inspector値）
         │
         └─ Stats が null（CSV無し / 該当id行なし）
               └─ Inspector値（SerializeField）を返す
```

- **id の決定**： `ItemData.Id`（enemy同様、Inspectorのid空欄なら**アセット名**）
- **アセット参照**（icon/SE/prefab）は常にInspector値。CSVでは扱わない
- ※ItemDataはScriptableObjectのため、実行時に`.asset`を書き換えると永続化される。
  それを避けるため**フィールドは上書きせず、プロパティで「CSV値 or Inspector値」を分岐**している

### 3-2. ドロップ等での「id → ItemDataアセット」解決

```text
ItemAssetDatabase.GetItem(id)
   │
   └─ ItemAssetDatabase.EnsureLoaded（初回のみ）
         └─ Resources.LoadAll<ItemData>("Item")
            で Resources/Item/ 配下の全ItemDataを収集し id→ItemData 辞書化
```

---

## 4. Enemy系のフロー

### 4-1. 数値データ（enemy.csv → 各フィールドへ上書き）

```text
Enemy生成時
   │
   ├─ EnemyHealth.Awake
   │     └─ EnemyHealth.ApplyCsvStats
   │           ├─ EnemyDatabase.GetStats(EnemyId)
   │           │     └─ EnemyDatabase.EnsureLoaded（初回のみ）
   │           │           └─ CsvLoader.Load("enemy.csv")
   │           │           └─ EnemyDatabase.CreateStats で各行を EnemyStats 化
   │           │
   │           ├─ Stats が null → 何もしない（＝全項目Inspector値のまま）
   │           │
   │           ├─ Stats が非null → 値がある項目だけフィールドへ上書き
   │           │     （例：if (s.maxHP.HasValue) maxHP = s.maxHP.Value;）
   │           │
   │           └─ dropItemId があれば ItemAssetDatabase.GetItem で解決し dropItem へ
   │                 見つからなければ警告 → InspectorのdropItemを使用
   │
   └─ EnemyAI.Awake
         └─ EnemyAI.ApplyCsvStats
               ├─ id は EnemyHealth.EnemyId を共用
               └─ EnemyDatabase.GetStats(id) → 値がある項目だけ上書き
```

- **id の決定**： `EnemyHealth.EnemyId`（Inspectorの`enemyId`空欄なら**オブジェクト名**）
- **Enemyは MonoBehaviour** のため、実行時のフィールド書き換えはPlay終了で元に戻り**アセットを汚さない**。
  よってItemのようなプロパティ分岐は不要で、**フィールドを直接上書き**している
- EnemyAI と EnemyHealth は**同じidで同じ行**を読む（1体1行）

---

## 5. 不正データ・異常系の扱い（一覧）

**すべてのケースで「ゲームは落ちず、動作を継続」する**よう設計されている。

| ケース | 検知メソッド | 出るメッセージ | 結果 |
| --- | --- | --- | --- |
| CSVファイルが無い | `CsvLoader.Load` | LogError「ファイルが見つかりません」 | 空リスト → 全項目Inspector値 |
| ファイル読み込み失敗（権限等） | `CsvLoader.Load` (try-catch) | LogError「読み込みに失敗しました」 | 空リスト → 全項目Inspector値 |
| ファイルが空 / 有効行なし | `CsvLoader.Parse` | LogWarning「中身が空 / 有効な行がありません」 | 空リスト → 全項目Inspector値 |
| idが空の行 | `XxxDatabase.EnsureLoaded` | LogWarning「id が空の行があります」 | その行を無視 |
| idが重複 | `XxxDatabase.EnsureLoaded` | LogWarning「id が重複しています」 | 最初の行を採用 |
| 列名が無い（ヘッダー綴りミス） | `CsvRow.TryGetRaw` | LogWarning「列 'X' がありません」 | その項目はInspector値 |
| 値が型変換できない | `CsvRow.GetXxxOrNull` | LogWarning「'X' を int に変換できません」 | その項目はInspector値 |
| 列が空欄 | `CsvRow.TryGetRaw` | （警告なし。意図的な空欄運用のため） | その項目はInspector値 |
| **オブジェクトが存在しないidの行** | （なし） | （なし） | **無害。誰も引かないので辞書に眠るだけ** |
| オブジェクトはあるがCSVに行が無い | `XxxDatabase.GetStats`→null | （なし。正常なフォールバック） | 全項目Inspector値 |
| dropItemId が解決できない | `EnemyHealth.ApplyCsvStats` | LogWarning「一致するItemDataが見つかりません」 | InspectorのdropItemを使用 |

### 補足：意図的に「無害」としているもの

- **オブジェクトが存在しないidの行**（例：テスト用に消したEnemyの行が残っている）
  → pull型のため参照されず、問題を起こさない。エラーにもしていない
- **id typo による静かなフォールバック**（例：`Bos-1` と打ち間違え）
  → 動作は継続するが、狙った行が効かない。マルチシーン構成では「シーンにそのidが居るか」を
    実行時に確実に判定できないため、あえて自動エラーにはしていない。
    Console の `[XxxDatabase] N 件読み込みました` と、値が反映されない挙動で気付く形

---

## 6. 読み込みタイミングと再読込

- **初回アクセス時に自動ロード**（`EnsureLoaded`）。シーン配置やコンポーネント不要
- Play開始時に `RuntimeInitializeOnLoadMethod` で内部辞書を null にリセット
  → 毎Playでファイルを読み直すため、CSV編集がそのまま反映される
- `XxxDatabase.Reload()` を呼べば任意タイミングで再読込も可能

---

## 7. 関連ファイル

| ファイル | 役割 |
| --- | --- |
| `Assets/Scripts/Data/CsvLoader.cs` | 読み込み・パースの共通基盤 |
| `Assets/Scripts/Data/CsvRow.cs` | 1行分の型付きアクセサ |
| `Assets/Scripts/Data/ItemStats.cs` / `ItemDatabase.cs` | item.csvの数値データ |
| `Assets/Scripts/Data/ItemAssetDatabase.cs` | id→ItemDataアセットの解決（Resources収集） |
| `Assets/Scripts/Data/EnemyStats.cs` / `EnemyDatabase.cs` | enemy.csvの数値データ |
| `Assets/Scripts/Item/ItemData.cs` | Item：プロパティでCSV値/Inspector値を分岐 |
| `Assets/Scripts/Enemy/EnemyHealth.cs` / `EnemyAI.cs` | Enemy：Awakeでフィールドを上書き |
