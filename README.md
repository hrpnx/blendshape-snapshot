# BlendShape Snapshot

SkinnedMeshRenderer の BlendShape ウェイトを名前付きスナップショットとして保存・復元する Unity エディタ拡張。

## 機能

- 同一 GameObject の SkinnedMeshRenderer の全 BlendShape を名前付きで保存（履歴は新しい順 = 履歴1 が最新）
- 履歴行の **↑** で復元（名前ベース。現在のメッシュに無い名前はスキップ）、**✕** で削除
- 名前未入力で保存すると `yyyy-MM-dd HH:mm:ss` を自動採番
- `IEditorOnly`（VRChat ビルド時に除去）、復元・削除・保存は Undo 対応

## インストール

`Packages/manifest.json` か VPM で `dev.hrpnx.blendshape-snapshot` を追加。エディタ専用。

## ライセンス

MIT
