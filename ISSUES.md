# rg-sjis プロジェクトの問題点分析

このドキュメントは、`rg-sjis` プロジェクトのソースコードを多角的に検証し、確認された問題点とその潜在的なリスクをまとめたものです。

---

## 1. 堅牢性・安定性の問題 (Robustness/Stability Issues)

アプリケーションの安定した動作を阻害し、ユーザーが予期せぬ不具合に遭遇する可能性のある、最も重大な問題群です。

### 1.1. ファイル操作におけるサイレントエラー（エラーの完全な無視）

**問題点:**
`Installer.cs` および `Uninstaller.cs` 内でのファイル操作（`File.Copy`）が、すべて空の `try-catch` ブロックで囲まれています。

```csharp
// Installer.cs
try { File.Copy(rgFullPath, rgUTF8FullPath, true); } catch { }
try
{
    File.Copy(myProgramFullPath, rgFullPath, true);
    // ...
}
catch { }

// Uninstaller.cs
try
{
    File.Copy(rgUTF8FullPath, rgFullPath, true);
    // ...
}
catch { }
```

**リスク:**
- **ファイルロックによる失敗**: `README.md` に記載の「既知の不具合」（VSCodeを複数起動すると機能しない）の直接的な原因と考えられます。他のVSCodeプロセスが `rg.exe` を掴んでいるため `File.Copy` が失敗し、エラーが握りつぶされるため、インストール/アンインストールが不完全な状態で終了します。
- **権限不足**: 管理者権限がない場合など、ファイル書き込みに失敗してもユーザーには何も通知されません。
- **不整合な状態**: バックアップの作成に失敗したまま `rg.exe` の置き換えだけが試みられるなど、ファイルシステムが中途半端な状態に陥り、VSCodeの検索機能が完全に破壊される危険性があります。

### 1.2. 脆弱な `rg.exe` のパス探索ロジック

**問題点:**
`rg.exe` の場所を特定するために、`where code.cmd` で得られたパスを基点とする、ハードコードされた複数の相対パスに依存しています。

```csharp
// Installer.cs / Uninstaller.cs
string[] relativePaths =
{
    @"..\resources\app\node_modules.asar.unpacked\vscode-ripgrep\bin\rg.exe",
    @"..\resources\app\node_modules.asar.unpacked\@vscode\ripgrep\bin\rg.exe",
    @"..\resources\app\node_modules\@vscode\ripgrep\bin\rg.exe"
};
```

**リスク:**
- **VSCodeのアップデートへの追従不可**: 将来的にVSCodeが内部のディレクトリ構造を変更した場合、この拡張機能は `rg.exe` を見つけられなくなり、機能しなくなります。作者が都度アップデートで対応する必要があり、メンテナンスコストが高い状態です。
- **多様なインストール形態**: User版/System版のインストール、Portableモードなど、通常と異なる方法でVSCodeがインストールされた場合にパスの探索が失敗する可能性があります。

### 1.3. 不整合な状態管理と信頼性の低い状態判定

**問題点:**
- `Installer.cs` は `rg_sjis.json` に `rg.exe` のパスを保存しますが、`Uninstaller.cs` はこのファイルを利用せずに、再度パスを探索しています。
- インストール済みかどうかの判定を、`rg.exe` のファイルサイズ (`1024000` バイトより大きいか小さいか) という「マジックナンバー」で行っています。
- アンインストール後も、バックアップファイル `rg_utf8.exe` が削除されずに残ります。

**リスク:**
- **アンインストール失敗**: インストール時とアンインストール時で `rg.exe` の探索ロジックの結果が異なった場合（例：VSCodeのアップデートをまたぐ）、アンインストールに失敗し、ラッパーファイルが残存する可能性があります。
- **判定ミス**: `ripgrep` 本体のサイズが将来的に変わった場合、ファイルサイズによる判定が誤動作する可能性があります。
- **ゴミファイルの残存**: 不要なファイルがシステムに残ることは、望ましい状態ではありません。

---

## 2. 保守性・コード品質の問題 (Maintainability/Code Quality Issues)

コードの可読性や変更の容易さを損なっている問題群です。

### 2.1. コードの重複

**問題点:**
`Installer.cs` と `Uninstaller.cs` に、`rg.exe` のパスを探索するためのコードがほぼ完全に重複して存在しています。

**リスク:**
- **修正漏れの発生**: 将来パスの探索ロジックを修正する際に、片方を修正し忘れるといったヒューマンエラーが発生しやすくなります。
- **可読性の低下**: 同様のコードが複数箇所に点在することで、コードベースの理解が困難になります。

### 2.2. 不適切なエラー処理（検索処理）

**問題点:**
`RipGrepMultiEncode.cs` では、`ripgrep` の標準エラー出力を、JSONをパースすることを前提とした標準出力のハンドラにリダイレクトしています。

```csharp
// RipGrepMultiEncode.cs
private void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs ev)
{
    Proc_OutputDataReceived(sender, ev); // JSONパーサーに送られる
}
```

**リスク:**
- `ripgrep` が「ファイルが見つかりません」のようなJSON形式ではないエラーを標準エラーに出力した場合、`JsonConvert.DeserializeObject` で例外が発生し、その例外もまた握りつぶされます。これにより、本来ユーザーに通知されるべきエラーが失われます。

### 2.3. 単一ファイルへの責務の集中

**問題点:**
`rg_sjis.exe` という単一の実行可能ファイルが、インストーラー、アンインストーラー、そして検索処理のラッパーという3つの全く異なる責務を担っています。

**リスク:**
- **コードの複雑化**: `Main` メソッドが巨大な `if-else` による振り分けを行っており、可読性が低く、修正が困難になっています。
- **影響範囲の拡大**: いずれか一つの機能の修正が、他の機能に意図しない影響（デグレード）を及ぼすリスクが高まります。

---

## 3. 将来の互換性の問題 (Future Compatibility Issues)

現在は動作していても、将来の外部環境の変化によって機能しなくなる可能性が高い問題群です。

### 3.1. VSCodeの内部実装への強い依存

**問題点:**
- `rg.exe` のパス探索（前述）
- `RipGrepWrapper.cs` での検索モード判定ロジックが、引数に `.code-search` という文字列が含まれるかどうかに依存しています。

```csharp
// RipGrepWrapper.cs
private static bool IsCallFromVSCodeSearch(string[] args)
{
    // ...
    if (s.Contains(@".code-search"))
    // ...
}
```

**リスク:**
- これらはVSCodeの文書化されていない内部実装です。将来のVSCodeのアップデートでこれらの仕様が予告なく変更された場合、Shift-JISの検索機能が完全に動作しなくなります。

---

## 4. パフォーマンスの問題 (Performance Issues)

アプリケーションの速度やリソース消費に関する問題です。

### 4.1. 非効率なインスタンス生成

**問題点:**
`RipGrepWrapper.cs` の `Main` メソッド内で、検索を実行する際に `RipGrepMultiEncode` クラスのインスタンスを2回生成しています。

```csharp
// RipGrepWrapper.cs
RipGrepMultiEncode rgcl1 = new RipGrepMultiEncode(args, search_mode);
rgcl1.Grep(Encoding.UTF8);

RipGrepMultiEncode rgcl2 = new RipGrepMultiEncode(args, search_mode);
rgcl2.Grep(Encoding.GetEncoding(932));
```

**リスク:**
- 処理のたびにオブジェクト生成と破棄のコストがかかり、わずかながらパフォーマンスの低下に繋がります。`RipGrepMultiEncode` クラスの設計を見る限り、単一のインスタンスを再利用して異なるエンコーディングで `Grep` メソッドを呼び出すことが可能と考えられます。
