# 外部データ参照：enemy.csv 仕様

Enemyのパラメータを CSV から読み込む仕組みの仕様書。
CSVの書式、読み込みの流れ、文字コード（UTF-8 with BOM）の注意点をまとめる。
（Item版 `外部データ参照_Item_csv仕様.md` と同じ構成。Enemy特有の差分は各章に記載）

---

## 1. 概要

### 目的
- Enemyのパラメータを CSV で管理し、**ビルド後も CSV を差し替えるだけで調整できる**ようにする
- CSV が壊れていてもゲームが落ちないよう、**Inspector値へフォールバック**する

### 基本方針

| 項目 | 方針 |
| --- | --- |
| 読み込みタイミング | **実行時**に CSV を読む（ビルド後の差し替えが可能） |
| 置き場所 | `Assets/StreamingAssets/enemy.csv` |
| CSVに入れるもの | 数値・文字列・フラグのみ |
| CSVに入れないもの | SE / エフェクトPrefab / 判定位置(Transform) / Layer などのアセット・シーン参照 |
| ドロップアイテム | ItemDataの参照そのものではなく **id文字列(dropItemId)** で指定 |
| 壊れたとき | Inspector値を使って通常どおり動作する |

### Itemとの違い（重要）

| | Item（ItemData） | Enemy（EnemyAI / EnemyHealth） |
| --- | --- | --- |
| 実体 | **ScriptableObject** | **GameObject上のMonoBehaviour** |
| 実行時のフィールド書き換え | `.asset`に永続化される（危険） | Play終了で元に戻る（安全） |
| CSV反映の方式 | プロパティで「CSV値 or Inspector値」を分岐 | **Awakeでフィールドを直接上書き**（値がある項目だけ） |
| 構成 | 1アセット=1データ | **1体を EnemyAI と EnemyHealth の2コンポーネントで構成** |

Enemyは MonoBehaviour なので実行時にフィールドを書き換えてもアセットが汚れない。
そのため「非シリアライズの第2の箱」は不要で、`Awake` で**値がある項目だけ上書き**するだけでよい。

---

## 2. 文字コード：UTF-8 with BOM（重要）

Item版と全く同じ。**必ず「UTF-8 with BOM」で保存すること。**

### 要点
- テキストファイルに文字コードの情報は入っておらず、開く側が推測する
- 日本語版ExcelはデフォルトでShift-JISと推測するため、BOMが無いUTF-8は文字化けする
- **BOM = ファイル先頭の3バイト `EF BB BF`**。「これはUTF-8」という目印
- BOMは見えない文字（U+FEFF）。プログラム側は読み込み時に除去する（`CsvLoader`が対応済み）

### 確認・付与方法

| 目的 | 方法 |
| --- | --- |
| 確認（コマンド） | `file enemy.csv` → `(with BOM)` と出るか |
| 確認（バイト） | `head -c 16 enemy.csv | xxd` → 先頭が `efbb bf` か |
| 付与（Excel） | 「CSV UTF-8(コンマ区切り)」で保存する（自動でBOMが付く） |
| 付与（VSCode） | ステータスバーの文字コード → 「エンコード付きで保存」 → `UTF-8 with BOM` |

---

## 3. CSVの書式ルール

Item版と共通。

| ルール | 内容 |
| --- | --- |
| ヘッダー | 最初の有効行を列名として扱う |
| 列の指定方法 | **列名（ヘッダー）をキーに読む。並び順は自由で、変えてもコード修正は不要** |
| コメント行 | 行頭が `#` の行は無視される |
| 空行 | 無視される |
| 空欄 | 「未指定」として扱い、**その項目だけInspector値が使われる** |
| カンマを含む値 | `"..."` で囲む |
| bool値 | `true/false` `1/0` `yes/no` `on/off`（Excelが書く `TRUE/FALSE` もそのまま読める） |
| 列順の方針 | 文字数の多い項目は表が見づらくなるため後ろに置く |

---

## 4. enemy.csv 列定義

```text
id,enemyName,maxHP,moveSpeed,chaseDistance,chaseHeight,meleeAttackDistance,canShoot,rangedAttackDistance,rangedAttackCooldown,attackCooldown,attackRadius,attackDamage,knockbackForce,checkDistance,retreatDuration,retreatSpeed,stunLevel,weightLevel,weight0Multiplier,weight1Multiplier,weight2Multiplier,landingCheckRadius,maxLaunchTime,effectScale,dropCount,dropItemId,isBoss,deathAnimationDuration,nextSceneOnDeath
```

### キー

| 列名 | 型 | 内容 |
| --- | --- | --- |
| `id` | string | **EnemyHealthのenemyId（空欄ならオブジェクト名）と一致させる**。1体1行 |
| `enemyName` | string | 表示・識別用の名前（任意） |

### EnemyAI（行動・攻撃）

| 列名 | 型 | 内容 |
| --- | --- | --- |
| `moveSpeed` | float | 移動速度 |
| `chaseDistance` | float | 追尾開始距離 |
| `chaseHeight` | float | 上下方向の索敵許容距離 |
| `meleeAttackDistance` | float | 近接攻撃距離 |
| `canShoot` | bool | 飛び道具を使うか |
| `rangedAttackDistance` | float | 飛び道具攻撃距離 |
| `rangedAttackCooldown` | float | 飛び道具攻撃間隔（秒） |
| `attackCooldown` | float | 近接攻撃クールダウン（秒） |
| `attackRadius` | float | 攻撃範囲（半径） |
| `attackDamage` | int | 攻撃力 |
| `knockbackForce` | float | 攻撃時にPlayerへ与えるノックバック力 |
| `checkDistance` | float | 壁・足元Rayの長さ |
| `retreatDuration` | float | 近接攻撃後の後退時間（秒） |
| `retreatSpeed` | float | 後退速度 |
| `stunLevel` | int | スタン耐性レベル（0:弱い 1:普通 2:強い） |

### EnemyHealth（HP・被弾・ドロップ）

| 列名 | 型 | 内容 |
| --- | --- | --- |
| `maxHP` | int | 最大HP |
| `weightLevel` | int | 重さ（0:軽い 1:普通 2:重い。打ち上げやすさに影響） |
| `weight0Multiplier` | float | 重さ0のときの打ち上げ倍率 |
| `weight1Multiplier` | float | 重さ1のときの打ち上げ倍率 |
| `weight2Multiplier` | float | 重さ2のときの打ち上げ倍率 |
| `landingCheckRadius` | float | 着地判定の半径 |
| `maxLaunchTime` | float | 打ち上げ状態の最大継続時間（セーフティ） |
| `effectScale` | float | 被弾エフェクトの表示倍率 |
| `dropCount` | int | ドロップ個数 |
| `dropItemId` | string | ドロップするItemDataのID（後述） |
| `isBoss` | bool | ボスか（死亡演出が変わる） |
| `deathAnimationDuration` | float | ボスDie演出の再生時間（秒） |
| `nextSceneOnDeath` | string | ボス撃破後に移動するScene名（長文寄りのため末尾） |

### CSVで扱わない項目（Inspectorで設定）

| 項目 | 理由 |
| --- | --- |
| `meleeAttackSE` / `rangedAttackSE` / `damageSE` / `dieSE` (AudioClip) | アセット参照 |
| `projectilePrefab` / 各種EffectPrefab / `itemPickupPrefab` (GameObject) | アセット参照 |
| `attackPoint` / `firePoint` / `wallCheck` / `groundCheck` / `retreatGroundCheck` / `landingCheck` (Transform) | **プレハブ内の相対位置＝シーン参照**のためCSV不可 |
| `playerLayer` / `groundLayer` (LayerMask) | Layer情報 |
| `effectPositionOffset` (Vector2) | 現状Inspector管理（必要ならx,y2列でCSV化可能） |

---

## 5. dropItemId：ドロップアイテムのID参照

アセット参照（ItemData）はCSVに直接書けないが、**id文字列で間接指定**できる。

```text
enemy.csv:  dropItemId = "Potion"
   │
   ├─ ItemAssetDatabase.GetItem("Potion")
   │     └─ Resources/Item/ 配下の全ItemDataから Id が "Potion" のものを探す
   │
   ├─ 見つかった → EnemyHealth.dropItem に代入
   └─ 見つからない → 警告を出して InspectorのdropItem を使用（フォールバック）
```

- 参照先の索引は `ItemAssetDatabase` が **Resources/Item/ 配下を自動収集**して作る（手動登録不要）
- `dropItemId` 空欄なら Inspector の `dropItem` がそのまま使われる

---

## 6. 読み込みの仕組み

### データの流れ

```text
Assets/StreamingAssets/enemy.csv
        │  初回アクセス時に自動で読み込み
        ▼
CsvLoader          … ファイル読み込み＋パース（BOM除去・引用符・コメント対応）
        ▼
EnemyDatabase      … id → EnemyStats の辞書を構築（static。シーン配置不要）
        ▲
        │  EnemyAI / EnemyHealth が自分のIDで引きに行く（pull型）
EnemyAI / EnemyHealth
        └─ Awakeで ApplyCsvStats：値がある項目だけ自分のフィールドへ上書き
```

### 設計上のポイント

**① 1体を2コンポーネントが同じidで読む**
EnemyHealth が `enemyId`（＝`EnemyId`）を一元管理し、EnemyAI はそれを共用する。
`enemy.csv` は1体1行で、両コンポーネントが同じ行から各自の担当列を読む。

**② IDは未設定でも動く**
```csharp
public string EnemyId => string.IsNullOrEmpty(enemyId) ? name : enemyId;
```
Inspectorの `enemyId` が空なら**オブジェクト名**がIDとして使われる。
シーン上の名前（例：`Boss-1` / `Boss-2`）がCSVのidと一致していればそのまま動く。

**③ Variantも特別扱い不要**
Prefab Variant（例：Boss-1のVariantであるBoss-2）も中身は同じコンポーネント。
`enemy.csv` に該当idの行を1つ足すだけでCSV対応になる（フォーマットの違いは無い）。
※ `enemyId` を空欄にして名前で照合する運用なら、Variantも名前で自然に別行へ分かれる。
　ベースに`enemyId`を明示設定するとVariantが継承して同じidになるため、その場合はVariant側で上書きすること。

**④ 呼び出し側のコードは変更不要**
`moveSpeed` `maxHP` などのフィールドはそのまま使われ、Awakeで中身が上書きされるだけ。

---

## 7. フォールバックの仕組み

### 方式（Itemとの違い）

Enemyは MonoBehaviour で、実行時のフィールド書き換えがアセットに永続化されない。
そのため **Awakeで「CSVに値がある項目だけ上書き」** するだけでフォールバックが成立する。

```csharp
/* EnemyHealth.ApplyCsvStats（抜粋） */
EnemyStats s = EnemyDatabase.GetStats(EnemyId);
if (s == null) return;                          /* CSV無し → 全項目Inspector値のまま */
if (s.maxHP.HasValue) maxHP = s.maxHP.Value;    /* 値がある項目だけ上書き */
```

`EnemyStats` の各項目は null許容で、null = 「CSVで未指定」を意味する。

### 動作パターン

| ケース | 結果 |
| --- | --- |
| CSVが正常に読めた | **CSV値**を使う |
| CSVファイルが無い / 読み込み失敗 | **Inspector値**（エラーログは出るが正常動作） |
| CSVはあるが、その `id` の行が無い | **Inspector値** |
| 行はあるが、その列が空欄 | **その項目だけInspector値** |
| 値が型変換できない | 警告を出して**その項目だけInspector値** |
| **オブジェクトが存在しないidの行** | **無害**（誰も引かないので辞書に眠るだけ） |

---

## 8. Excel運用ルール

Item版と同じ。

| 操作 | 手順 |
| --- | --- |
| 開く | ダブルクリックでOK（BOMがあるので化けない） |
| **保存** | **必ず「CSV UTF-8(コンマ区切り)」を選ぶ** |

> ⚠️ 「CSV(コンマ区切り)」（UTF-8が付かない方）で保存すると Shift-JIS になりファイルが壊れる。
> Excelで保存するとコメント行に末尾カンマが付いたり bool が `TRUE/FALSE` になるが、パーサ側で吸収するため問題ない。

---

## 9. 現在の登録データ（参考）

| id | 用途 | 備考 |
| --- | --- | --- |
| `Enemy` | 基本Enemy | 一部フィールドは旧プレハブのため空欄（Inspector値へフォールバック） |
| `TankEnemy` | 低速Enemy | 同上 |
| `FastEnemy` | 高速Enemy | ドロップ=Knife |
| `Boss-1` | ボス（Stage1_3） | isBoss、撃破後 S1Clear |
| `Boss-2` | ボス（Stage2_3、Boss-1のVariant） | 強化ステータス、撃破後 ClearScene |

※テスト用の仮Enemyが混在している場合は、上表と実際のプレハブ/シーンを突き合わせて整理すること。

---

## 10. 新しいEnemyを追加するときの手順

1. Enemyプレハブに `EnemyHealth.enemyId` を設定（空欄ならオブジェクト名がidになる）
2. `enemy.csv` に同じidの行を追加（**UTF-8 with BOM** を維持）
3. 数値・bool・文字列の列を埋める（アセット・シーン参照はInspectorで設定）
4. ドロップさせるなら `dropItemId` にItemDataのIDを書き、対象ItemDataを Resources/Item/ に置く

---

## 11. 関連ファイル

| ファイル | 役割 |
| --- | --- |
| `Assets/StreamingAssets/enemy.csv` | Enemyデータ本体 |
| `Assets/Scripts/Data/CsvLoader.cs` / `CsvRow.cs` | CSV読み込み・パースの共通処理 |
| `Assets/Scripts/Data/EnemyStats.cs` | CSV1体分のデータ（null許容） |
| `Assets/Scripts/Data/EnemyDatabase.cs` | enemy.csvの読み込みと辞書管理 |
| `Assets/Scripts/Data/ItemAssetDatabase.cs` | dropItemId → ItemDataアセットの解決 |
| `Assets/Scripts/Enemy/EnemyHealth.cs` / `EnemyAI.cs` | Awakeでフィールドへ上書き |
