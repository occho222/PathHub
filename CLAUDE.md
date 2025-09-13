# リリース手順

## 実装後

### 実装後ビルド

実装完了後は必ず `dotnet build` を実行し、コードが正常にコンパイルされることを確認する

## 自動リリースワークフロー

ユーザーがリリースを要求した場合、以下の手順を自動的に実行する:

1. **バージョン更新** (Verはデータ管理にもあります。)

   * `${PROJECT_NAME}.csproj` の Version, AssemblyVersion, FileVersion を更新
   * `Models/AppSettings.cs` の DataVersion を更新

2. **リリースノート更新**:

   * `RELEASE_NOTES.md` の先頭に新バージョンの変更内容を追加
   * 新機能（✨）、改善（🔧）、修正（🐛）、削除（🗑️）に分類して記載
   * Git履歴を参考にして主要な変更点を網羅

3. **最終リリースビルド**: バージョン更新後に `dotnet build -c Release`

4. **リリースファイルのパッケージ化**:

   ```bash
   mkdir -p "release/${PROJECT_NAME}XXX"  # XXX = ドットを除いたバージョン番号
   cp -r "bin/Release/net6.0-windows/"* "release/${PROJECT_NAME}XXX/"
   cp "RELEASE_NOTES.md" "release/${PROJECT_NAME}XXX/"
   ```

   * パッケージ化したファイルサイズを確認する
   * `du -sh "release/${PROJECT_NAME}XXX/"` でサイズ確認
   * リリースノートを配布パッケージに同梱する

5. **ZIP圧縮**:

   ```bash
   cd "release/${PROJECT_NAME}XXX"
   "C:\Program Files\7-Zip\7z.exe" a -tzip "../${PROJECT_NAME}XXX.zip" *
   cd ../..
   ```

   * 配布用のZIPファイルを生成する（フォルダ直下のファイルがZIPの直下に展開される）
   * パッケージディレクトリと同じ場所にZIPファイルが作成される

6. **Gitにコミット・プッシュ**

   * 全ての変更をステージング: `git add .`
   * バージョン情報を含むコミット
   * リモートリポジトリにプッシュ: `git push origin main`
