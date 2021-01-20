# rg-sjis

[![rg_sjis v0.2.7](https://img.shields.io/badge/rg_sjis-v0.2.7-6479ff.svg)](https://github.com/komiyamma/vscode_ripgrep_sjis_extension/releases)
[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE)
![Windows 7,8,10](https://img.shields.io/badge/Windows-7,8,10-6479ff.svg)
![.NET Framework 4.5.2](https://img.shields.io/badge/.NET_Framework-v4.5.2-6479ff.svg)

Visual Studio Code の Grep で SJIS も ヒットするように。  
Visual Studio Code で grep すると、utf8とsjisが混じったファイル群だと、sjis が検知できない。  
そこで検知できるようにしたもの。

## Requirements
1. MS-Windows  
1. .NET Framework 4.5.2 以上

## Remarks

Visual Studio Code にて「ファイル」→「ユーザー設定」→「設定」で、  
検索欄に「guess」と入れて「Auto Guess Encoding」に「チェック」を入れることを推奨。  
推奨理由としては、grep 検索結果から「間違えたエンコード」で該当のファイルへとジャンプした場合、Visual Studio Code は  
「対象のファイルは最新状態だと検索対象の文字列は存在しない」と判断して候補から消してしまうため。

## Usage
拡張機能をインストールするだけで利用可能となります。

## Known Issues
Visual Studio Codeを複数起動した後、そのうち１つを閉じるとsjisを検索できなくなります。

## Market Place
[rg-sjis](https://marketplace.visualstudio.com/items?itemName=komiyamma.rg-sjis)

## Related repositories
「rg_sjis.exe という実行ファイル」がありますが、この実行ファイルのソースリポジトリは「[vscode_ripgrep_sjis](https://github.com/komiyamma/vscode_ripgrep_sjis)」となります。

## Change Log

### 0.2.7

パッケージ情報にキーワード追記

### 0.2.6

デバッグ目的のダイアログが表示されていたため、非表示に

### 0.2.5

Visual Studio Code にパスが通っていなくても動作するように修正

### 0.2.4

デバッグ目的のダイアログが表示されていたため、非表示に

### 0.2.3

試験的な初版
