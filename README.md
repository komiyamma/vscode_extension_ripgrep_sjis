# rg-sjis

[![rg_sjis v0.2.6](https://img.shields.io/badge/rg_sjis-v0.2.5-6479ff.svg)](https://github.com/komiyamma/vscode_ripgrep_sjis_extension/releases)
[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE)
![Windows Only](https://img.shields.io/badge/Windows-Only-6479ff.svg)
![.NET Framework 4.5.2](https://img.shields.io/badge/.NET_Framework-v4.5.2-6479ff.svg)


This is the grep to hit not only UTF8 but also Japanese SJIS(cp932).
When grep is done with Visual Studio Code, sjis cannot be detected if the files are a mixture of utf8 and sjis.   
This extension is made detectable.

(Visual Studio Code の Grep で SJIS も ヒットするように。  
 Visual Studio Code で grep すると、utf8とsjisが混じったファイル群だと、sjis が検知できない。  
 そこで検知できるようにしたもの。)

## Requirements

1 Windows system. I think it's about Win7 or later. Maybe.  
(Windows系。Win7以降くらいじゃないかな。多分。)  
2 .NET Framework 4.5.2 and above.

# Remarks
In Visual Studio Code, select "File"-> "User Settings"-> "Settings".  
It is recommended to enter "guess" in the search field and check "Check" in "Auto Guess Encoding".  
The recommended reason is that if you jump to the file with "wrong encoding" from the grep search result, Visual Studio Code will  
Because it judges that "the character string to be searched does not exist if the target file is the latest state" and deletes it from the candidates.

(Visual Studio Code にて「ファイル」→「ユーザー設定」→「設定」で、  
検索欄に「guess」と入れて「Auto Guess Encoding」に「チェック」を入れることを推奨。  
推奨理由としては、grep 検索結果から「間違えたエンコード」で該当のファイルへとジャンプした場合、Visual Studio Code は  
「対象のファイルは最新状態だと検索対象の文字列は存在しない」と判断して候補から消してしまうため。)

# Usage
Just install the extension and it will be available.
(拡張機能をインストールするだけで利用可能となります)

## Known Issues

Problems occur when launching multiple Visual Studio Codes.  
(Visual Studio Codeを複数起動した後、１つを閉じるとsjisを検索できない)

## Related repositories
「rg_sjis.exe という実行ファイル」がありますが、この実行ファイルのソースリポジトリは「[vscode_ripgrep_sjis](https://github.com/komiyamma/vscode_ripgrep_sjis) 」となります。

### 0.2.6

デバッグ目的のダイアログが表示されていたため、非表示に

### 0.2.5

Visual Studio Code にパスが通っていなくても動作するように修正

### 0.2.4

デバッグ目的のダイアログが表示されていたため、非表示に

### 0.2.3

A draft version.  
(試験版)




