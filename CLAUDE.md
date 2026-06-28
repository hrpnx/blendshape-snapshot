# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 概要

`dev.hrpnx.blendshape-snapshot` — SkinnedMeshRenderer の BlendShape ウェイトを名前付きスナップショットとして保存・復元する Unity エディタ拡張パッケージ。VRChat アバター改変ワークフロー向け。エディタ専用で、ランタイムには何も残さない。

UPM / VPM パッケージ。開発・テストは、本パッケージを `Packages/` に取り込んだ Unity プロジェクト上で行う。

## 制約

- **C# 9**（Unity 2022.3 LTS の上限）。C# 10 以降の機能（プライマリコンストラクタ、コレクション式 `[]`、`record struct` 等）は使用禁止。
- Unity `2022.3`。`package.json` の `unity` フィールドと一致させること。

## アーキテクチャ

3 つの asmdef に分割。依存は Editor → Runtime の一方向のみ。

命名規約: プロダクト名トークンは **`Blendshape`（小文字 s）**で統一する（型名・名前空間・asmdef・ファイル名すべて）。理由は、Unity の `NicifyVariableName` が `BlendShapeSnapshot` を「Blend Shape Snapshot」と 2 スペースに分割するのを避け、Inspector 見出しを「Blendshape Snapshot」にするため。Unity 機能としての BlendShape（API・メッシュのシェイプ）を指す箇所は `BlendShape` のまま。

- **Runtime/** (`Hrpnx.BlendshapeSnapshot.Runtime`)
  - `BlendshapeSnapshot.cs` — `MonoBehaviour` コンポーネント（クラス名 `BlendshapeSnapshot`）。`IEditorOnly`（VRChat ビルド時に除去）。`Snapshot` / `ShapeValue` のネスト型でデータを保持。`Snapshots` リストは **index 0 が最新**。
  - `BlendshapeSnapshotIO.cs` — `static`。SMR と Snapshot 間の値の採取(`Capture`)・適用(`Apply`)を行う**純粋ロジック**。Undo・ダーティ化は一切行わない（呼び出し側 Editor の責務）。`Apply` は適用できた件数を返す。
  - `SnapshotName.cs` — 名前決定ロジック。未入力なら `DateTime` を `yyyy-MM-dd HH:mm:ss` で採番。`now` を引数で受け取り**テスト可能**にしてある。
- **Editor/** (`Hrpnx.BlendshapeSnapshot.Editor`, `includePlatforms: [Editor]`)
  - `BlendshapeSnapshotEditor.cs` — `CustomEditor`。GUI と Undo/`SetDirty` を担う。保存・復元・削除はすべて `Undo.RecordObject` 対応。
- **Tests/Editor/** (`Hrpnx.BlendshapeSnapshot.Tests`) — NUnit EditMode テスト。

### 設計上の要点

- **BlendShape はインデックスではなく名前で記録**する。メッシュ差し替えに強くし、`Apply` 時に現在のメッシュに無い名前はスキップ（欠落件数を `Debug.LogWarning`）。
- **純粋ロジック（IO / Name）と Unity 副作用（Editor の Undo/Dirty）を分離**する設計を維持すること。新しいロジックは Runtime 側の static に置き、Editor からは呼ぶだけにする。テストもこの分離に依存している。
- **VRChat SDK への暗黙依存**: Runtime は `VRC.SDKBase`（`IEditorOnly`）を使うが、`package.json` の `vpmDependencies` は空、asmdef の `references` も空で auto-referenced 経由で解決している。コンパイルにはプロジェクトに VRChat SDK が存在することが前提。

## ビルド・テスト

Unity Editor 上でのみコンパイル・実行する。CLI ビルドは無い。

- **コンパイル / テスト**: Unity Editor の Test Runner（EditMode）で実行する。`Hrpnx.BlendshapeSnapshot.Tests` アセンブリを対象にする。
- 純粋ロジックのみテスト対象（`BlendshapeSnapshotIO`, `SnapshotName`）。Editor GUI クラスはテストしない。
- MCP for Unity が利用可能な開発環境では、それ経由でコンパイル・テスト実行・コンソール取得を行う。

## コミット

`<type>: <description>`（type: feat, fix, refactor, docs, test, chore, perf, ci）。`package.json` の `version` はリリース時に更新する。
