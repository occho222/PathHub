# ModernLauncher - FenrirFS Style Application

## 概要
FenrirFS風のクラシックなデザインを持つランチャーアプリケーションです。MVVMパターンとSOLID原則に従って再構築されています。

## アーキテクチャ

### フォルダ構成
```
ModernLauncher/
├── Models/               # データモデル
│   ├── Project.cs
│   ├── ProjectInfo.cs
│   ├── ItemGroup.cs
│   ├── LauncherItem.cs
│   └── SelectableGroup.cs
├── ViewModels/           # ビューモデル（MVVMパターン）
│   └── MainViewModel.cs
├── Views/                # ビュー（XAML/Window）
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   └── TextInputDialog.cs
├── Services/             # ビジネスロジック・サービス
│   ├── ProjectService.cs
│   ├── LauncherService.cs
│   └── ServiceLocator.cs
├── Interfaces/           # インターフェース定義
│   ├── IProjectService.cs
│   └── ILauncherService.cs
├── Commands/             # コマンドパターン実装
│   └── RelayCommand.cs
└── Converters/           # データバインディング用コンバーター
    └── GroupButtonVisibilityConverter.cs
```

## 設計原則

### MVVMパターン
- **Model**: データとビジネスロジック
- **View**: UI表示（XAML）
- **ViewModel**: ViewとModelの仲介、データバインディング

### SOLID原則の適用
1. **単一責任の原則 (SRP)**: 各クラスは1つの責任のみ
2. **開放閉鎖の原則 (OCP)**: 拡張に開放、修正に閉鎖
3. **リスコフの置換原則 (LSP)**: 派生クラスは基底クラスの代替可能
4. **インターフェース分離の原則 (ISP)**: 不要な依存関係を避ける
5. **依存関係逆転の原則 (DIP)**: 抽象に依存、具象に非依存

### レイヤ分離
- **Presentation Layer**: Views, ViewModels
- **Business Logic Layer**: Services, Commands
- **Data Access Layer**: Services (ProjectService)
- **Domain Layer**: Models, Interfaces

## 主要な改善点

### 1. 責任の分離
- UI関連のロジックはViewModelに
- データ操作はServiceクラスに
- ビジネスルールは適切なサービスに分離

### 2. 依存関係注入
- ServiceLocatorパターンでサービスを管理
- インターフェースベースの設計
- テスタビリティの向上

### 3. データバインディング
- ViewとViewModelの完全分離
- INotifyPropertyChangedの実装
- コマンドパターンによるアクション処理

### 4. エラーハンドリング
- 各層での適切な例外処理
- ユーザーフレンドリーなエラーメッセージ

## 使用技術
- .NET 6.0
- WPF (Windows Presentation Foundation)
- MVVM (Model-View-ViewModel) パターン
- Newtonsoft.Json (JSONシリアライゼーション)

## 今後の拡張予定
1. アイテム追加・編集ダイアログの実装
2. インポート・エクスポート機能の完全実装
3. ヘルプシステムの実装
4. 設定管理機能
5. プラグインアーキテクチャ
6. 単体テストの追加

## 開発者向け情報

### 新機能の追加方法
1. 必要に応じてインターフェースを定義
2. サービスクラスでビジネスロジックを実装
3. ViewModelにコマンドとプロパティを追加
4. ViewでUIとデータバインディングを設定

### テスト
- 各サービスは独立してテスト可能
- ViewModelのロジックは依存関係注入でモック化可能
- ビジネスロジックとUIが分離されているため単体テストが容易