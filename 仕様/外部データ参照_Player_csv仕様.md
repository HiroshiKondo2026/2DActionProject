# 外部データ参照：player.csv 仕様

Playerのパラメータを CSV から読み込む仕組みの仕様書。
CSVの書式（key-value形式）、読み込みの流れ、文字コード（UTF-8 with BOM）の注意点をまとめる。
（Item版 / Enemy版と同じ構成。Player特有の差分は各章に記載）

---

## 1. 概要

### 目的
- Playerのパラメータを CSV で管理し、**ビルド後も CSV を差し替えるだけで調整できる**ようにする
- CSV が壊れていてもゲームが落ちないよう、**Inspector値へフォールバック**する

### 基本方針

| 項目 | 方針 |
| --- | --- |
| 読み込みタイミング | **実行時**に CSV を読む |
| 置き場所 | `Assets/StreamingAssets/player.csv` |
| **形式** | **key-value形式（1行 = 1パラメータ）** ← Item/Enemyと最大の違い |
| CSVに入れるもの | 数値・bool・文字列（スカラー） |
| CSVに入れないもの | 攻撃位置などのVector2オフセット、SE/Prefab/Layer などのアセット・シーン参照 |
| 壊れたとき | Inspector値を使って通常どおり動作する |

### なぜ key-value 形式なのか（Item/Enemyとの違い）

Item / Enemy は「**複数の種類（行）**」があるので「1行=1エンティティ」の表形式だった。
一方 **Playerは1体だけ**。同じ表形式にすると40項目超を横1行に並べることになり、Excelで激烈に見づらい。

そこでPlayerは **key-value形式**（`key,value` の2列、1パラメータ=1行）を採用した。

| | Item / Enemy | Player |
| --- | --- | --- |
| エンティティ数 | 複数（多数の行） | 1体のみ |
| 形式 | 1行=1エンティティ（横に多列） | 1行=1パラメータ（縦に読む） |
| キー | `id`（行を識別） | `key`（パラメータ名） |

### 実装方式（Item/Enemyとの違い）

Playerは Item と違い ScriptableObject ではなく、複数の MonoBehaviour（Controller/Combat/Health）で構成される。
MonoBehaviourは実行時のフィールド書き換えがアセットに永続化されないため、Enemyと同じく
**Awake/Startで「値がある項目だけフィールドを上書き」** する方式でフォールバックを実現している
（ItemDataのような「非シリアライズの第2の箱」は不要）。

---

## 2. 文字コード：UTF-8 with BOM（重要）

Item/Enemy版と同じ。**必ず「UTF-8 with BOM」で保存すること。**

- テキストファイルに文字コードの情報は入っておらず、開く側が推測する
- 日本語版ExcelはデフォルトでShift-JISと推測するため、BOMが無いUTF-8は文字化けする
- **BOM = ファイル先頭の3バイト `EF BB BF`**（見えない文字 U+FEFF）。「これはUTF-8」という目印
- プログラム側は読み込み時にBOMを除去する（`CsvLoader`が対応済み）

| 目的 | 方法 |
| --- | --- |
| 付与（Excel） | 「CSV UTF-8(コンマ区切り)」で保存する |
| 付与（VSCode） | ステータスバー → 「エンコード付きで保存」 → `UTF-8 with BOM` |
| 確認（コマンド） | `file player.csv` → `(with BOM)` |

---

## 3. CSVの書式ルール

| ルール | 内容 |
| --- | --- |
| 形式 | 1行目を `key,value` のヘッダーにし、以降1行1パラメータ |
| コメント行 | 行頭が `#` の行は無視される（セクション区切りに使う） |
| 空行 | 無視される |
| **未指定** | keyを書かない or value空欄 → **その項目だけInspector値**（フォールバック） |
| bool値 | `true/false` `1/0` `yes/no` `on/off`（Excelが書く `TRUE/FALSE` もそのまま読める） |

---

## 4. player.csv 項目定義

### 移動・ジャンプ（PlayerController）

| key | 型 | 内容 |
| --- | --- | --- |
| `moveSpeed` | float | 移動速度 |
| `jumpPower` | float | ジャンプ力 |
| `maxJumpCount` | int | 最大ジャンプ回数 |
| `maxFallSpeed` | float | 落下速度上限 |
| `startFacingRight` | bool | 開始時に右向きか |
| `groundCheckRadius` | float | 接地判定の半径 |
| `attackMoveGroundCheckDistance` | float | 攻撃中移動の前方地面確認距離 |

### 被弾リアクション（PlayerController）

| key | 型 | 内容 |
| --- | --- | --- |
| `knockbackSafetyTime` | float | ノックバックStateのセーフティ解除時間 |

### 攻撃 Attack1/2/3（PlayerCombat）

| key | 型 | 内容 |
| --- | --- | --- |
| `attackDM` | int | 攻撃ダメージ（共通） |
| `attack1Radius` / `2` / `3` | float | 各コンボの攻撃範囲 |
| `attack1LaunchPower` / `2` / `3` | float | 各コンボの打ち上げ力 |
| `attack1Knockback` / `2` / `3` | float | 各コンボのノックバック |
| `attack1MoveDistance` / `2` / `3` | float | 各コンボの前進距離 |
| `attack1EndLock` / `2` / `3` | float | 各コンボ後の硬直時間 |

### ジャンプ攻撃（PlayerCombat）

| key | 型 | 内容 |
| --- | --- | --- |
| `jumpAttackRadius` | float | 判定半径 |
| `jumpAttackKnockback` | float | ノックバック |
| `jumpAttackDamage` | int | ダメージ |
| `jumpAttackStopTime` | float | 空中停止時間 |
| `jumpAttackStunRate` | float | スタン係数 |

### HP・被弾（PlayerHealth）

| key | 型 | 内容 |
| --- | --- | --- |
| `maxHP` | int | 最大HP |
| `invincibleTime` | float | 被弾後の無敵時間 |
| `knockbackPower` | float | ノックバック力 |
| `knockbackDuration` | float | ノックバック速度が乗る時間 |
| `blinkInterval` | float | 無敵中の点滅間隔 |
| `fallDeathY` | float | この高さより下に落ちたら死亡 |
| `deathKnockbackPower` | float | 死亡時の吹っ飛び力 |
| `deathUpPower` | float | 死亡時の上方向の力 |

### CSVで扱わない項目（Inspectorで設定）

| 項目 | 理由 |
| --- | --- |
| `attack1〜3Offset` / `jumpAttackForwardOffset` / `jumpAttackDownOffset` (Vector2) | **Gizmoを見ながら視覚調整する空間パラメータ**のためInspectorに残す |
| `jumpSE` / 各攻撃SE / `projectileSE` / `recoverySE` (AudioClip) | アセット参照 |
| `groundCheck` / `footstepSource` など (Transform / Component) | シーン参照 |
| `groundLayer` / `enemyLayer` (LayerMask) | Layer情報 |
| デバッグ用（`alwaysShowAttackGizmo` 等） | 調整対象でないため除外 |

※ `PlayerItemAction` はCSV対象の数値項目を持たない（SE等アセットのみ）。

---

## 5. 読み込みの仕組み

### データの流れ

```text
Assets/StreamingAssets/player.csv （key-value）
        │  初回アクセス時に自動で読み込み
        ▼
CsvLoader          … 共通のファイル読み込み＋パース（Item/Enemyと同じものを流用）
        ▼
PlayerDatabase     … 各行の key列→value列 を辞書化し、PlayerStats を1つ作る
        ▲
        │  各コンポーネントが自分のkeyを引きに行く
PlayerController / PlayerCombat / PlayerHealth
        └─ Awake（Healthのみ Start）で ApplyCsvStats：値がある項目だけ上書き
```

### 各コンポーネントの反映タイミング

| コンポーネント | 呼ぶ場所 | 備考 |
| --- | --- | --- |
| PlayerController | Awake | 移動・ジャンプ・接地・knockbackSafetyTime |
| PlayerCombat | Awake | Attack1/2/3・ジャンプ攻撃のスカラー |
| PlayerHealth | **Start（`currentHP = maxHP` の前）** | maxHPをCSV値にしてからHP初期化する必要があるため |
| PlayerItemAction | （なし） | CSV対象の数値なし |

---

## 6. フォールバックの仕組み

### 「値があれば上書き」の書き方

```csharp
/* PlayerStats.GetFloat は float?（null許容）を返す */
if (p.GetFloat("moveSpeed") is float ms) moveSpeed = ms;
/*  ↑値がある → msに入りフィールド上書き / null（未指定）→ 何もしない＝Inspector値のまま */
```

`PlayerStats` の getter は「未指定なら null」を返す。
- キーが無い / 値が空欄 → null（**警告なし**。未指定は通常運用のため）
- キーはあるが変換できない → null＋警告（データのミス）

### 動作パターン

| ケース | 結果 |
| --- | --- |
| CSVに値がある | **CSV値**を使う |
| CSVファイルが無い / 読み込み失敗 | **Inspector値**（エラーログは出るが正常動作） |
| そのkeyがCSVに無い | **Inspector値** |
| keyはあるが値が空欄 | **Inspector値** |
| 値が型変換できない | 警告を出して**Inspector値** |

---

## 7. Excel運用ルール

| 操作 | 手順 |
| --- | --- |
| 開く | ダブルクリックでOK（BOMがあるので化けない） |
| **保存** | **必ず「CSV UTF-8(コンマ区切り)」を選ぶ** |
| セクション追加 | `#` で始まる行を自由に足せる（パーサが無視する） |

---

## 8. パラメータを追加するときの手順

1. 対象コンポーネントに `[SerializeField]` フィールドを追加（Inspector値＝フォールバック）
2. `player.csv` に `キー名,値` の行を追加（該当セクションのコメント下に置くと見やすい）
3. そのコンポーネントの `ApplyCsvStats()` に1行追加
   （例：`if (p.GetFloat("newParam") is float v) newParam = v;`）

---

## 9. 関連ファイル

| ファイル | 役割 |
| --- | --- |
| `Assets/StreamingAssets/player.csv` | Playerパラメータ本体（key-value） |
| `Assets/Scripts/Data/CsvLoader.cs` / `CsvRow.cs` | CSV読み込み・パースの共通処理 |
| `Assets/Scripts/Data/PlayerStats.cs` | key→value の型付きアクセサ |
| `Assets/Scripts/Data/PlayerDatabase.cs` | player.csvの読み込みと辞書管理 |
| `Assets/Scripts/Player/PlayerController.cs` / `PlayerCombat.cs` / `PlayerHealth.cs` | Awake/StartでフィールドへCSV値を上書き |
