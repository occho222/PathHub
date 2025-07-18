# ?? GitHub Copilot 開発ガイドライン

<div align="center">

## ??? PathHub プロジェクト開発規約

*美しいコード、効率的な開発を目指して* ?

</div>

---

## ?? コミットルール

### ?? 基本原則
- **?? 明確性**: 変更内容が一目で分かる適切な日本語のコメントでコミットしてください
- **? タイミング**: コミットは「コミットの命令」があったときのみ実行してください
- **??? 安全性**: エラーを避けるため特殊文字の使用は控えてください

### ?? 重要な注意事項
- **?? 禁止文字**: コロン（`:`）、セミコロン（`;`）、括弧などはPowerShellでエラーの原因となります
- **? 推奨文字**: 日本語、英数字、ハイフン（`-`）、アンダースコア（`_`）を使用してください

---

## ?? コマンド実行ルール

### ?? PowerShell実行時の注意
- **?? 区切り文字**: 複数コマンドの実行は「`&&`」ではなく「`;`」で区切ってください
- **??? エラー処理**: コマンドが失敗した場合の適切な処理を心がけてください

### ?? 推奨パターン# ? 良い例
git add -A; git commit -m "機能追加"; git push origin main

# ? 避けるべき例  
git add -A && git commit -m "機能追加: 新しい機能" && git push
---

## ?? 開発スタイル

### ?? コード品質
- **?? 一貫性**: 統一されたコーディングスタイルを維持
- **?? 可読性**: 他の開発者が理解しやすいコードを心がける
- **?? 保守性**: 将来の変更に対応しやすい設計を採用

### ? 効率性
- **?? パフォーマンス**: 最適化されたコードを目指す
- **? シンプルさ**: 複雑さを避け、シンプルで elegant な解決策を選択
- **?? 再利用性**: コンポーネントの再利用を促進

---

<div align="center">

## ?? チーム全体で素晴らしいソフトウェアを作りましょう！

### ?? 対象者
**全ての開発者・AIアシスタントが遵守すべきルール**

*Happy Coding! ??*

---

?? **PathHub Team** | ??? **Powered by .NET 6 & WPF** | ?? **Modern Development**

</div>