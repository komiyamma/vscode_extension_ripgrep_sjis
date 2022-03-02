# rg-sjis

[![rg_sjis v0.3.2](https://img.shields.io/badge/rg_sjis-v0.3.2-6479ff.svg)](https://github.com/komiyamma/vscode_ripgrep_sjis_extension/releases)
[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE)
![Windows 7|8|10|11](https://img.shields.io/badge/Windows-7_|_8_|_8.1_|_10_|_11-6479ff.svg?logo=windows&logoColor=white)
![.NET Framework 4.5.2](https://img.shields.io/badge/.NET_Framework-v4.5.2-6479ff.svg)

この拡張機能は、Visual Studio Codeで「ファイル検索(=Grep)」をした際に、  
ファイルの「文字エンコーディング」が規定の「UTF8」のファイルのみならず、  
「Shift-JIS (CP932)」であっても検索ヒットするように拡張します。

## 動作環境
1. MS-Windows  
1. .NET Framework 4.5.2 以上

## 使い方
拡張機能をインストールするだけで利用可能となります。

## 備考

Visual Studio Code のメニュー「ファイル」→「ユーザー設定」→「設定」で、  
検索欄に「guess」と入れて「Auto Guess Encoding」に「チェック」を入れることを推奨します。  
推奨理由としては、Grep 検索結果から「間違えたエンコード」で該当のファイルへとジャンプした際、  
Visual Studio Code は「対象のファイルは最新状態では検索対象の文字列は存在しない」と判断して候補から消してしまうためです。

## 既知の不具合
Visual Studio Codeを複数起動した後、そのうち１つを閉じると一時的にShift-JISが検索ヒットしなくなります。  
全てのVisual Studio Codeを閉じた後、再度Visual Studio Codeを起動すると機能するようになります。

## マーケットプレイス
[rg-sjis](https://marketplace.visualstudio.com/items?itemName=komiyamma.rg-sjis) で公開されています。

## Change Log

### 0.3.2

アイコンの変更。

### 0.2.9

まれにVSCode付属の検索機能が削除されてしまい、検索自体ができなくなるバグを修正。

### 0.2.8

説明文の更新。ソースの整理。

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
