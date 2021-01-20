# rg-sjis

[![rg_sjis v0.2.3](https://img.shields.io/badge/rg_sjis-v0.2.3-6479ff.svg)](https://github.com/komiyamma/vscode_ripgrep_sjis_extension/releases)
[![MIT License](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE)

This is the grep to hit not only UTF8 but also Japanese SJIS(cp932).
When grep is done with Visual Studio Code, sjis cannot be detected if the files are a mixture of utf8 and sjis.   
This extension is made detectable.

Visual Studio Code の Grep で SJIS も ヒットするように。  
Visual Studio Code で grep すると、utf8とsjisが混じったファイル群だと、sjis が検知できない。  
そこで検知できるようにしたもの。

## Requirements

- Windows system. I think it's about Win7 or later. Maybe.  
(Windows系。Win7以降くらいじゃないかな。多分。)
- .NET Framework 4.5.2 and above.

## Known Issues

Problems occur when launching multiple Visual Studio Codes.  
(Visual Studio Codeを複数起動した後、１つを閉じるとsjisを検索できない)

### 0.2.3

A draft version.  
(試験版)




