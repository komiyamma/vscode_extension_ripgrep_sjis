import * as vscode from 'vscode';
let exec = require('child_process').exec;
let path = require('path');
let process = require('process');

export function activate(context: vscode.ExtensionContext) {
	var command = path.dirname(__filename) +`/rg_sjis.exe --mode-install "${process.execPath}"`;
	exec(command, function(error:any, stdout:any, stderr:any) {
		// シェル上でコマンドを実行できなかった場合のエラー処理
		if (error !== null) {
		  console.log('exec error: ' + error);
		  return;
		}
	  });
	let disposable = vscode.commands.registerCommand('rg-sjis.Install', () => {});
	context.subscriptions.push(disposable);
}

export function deactivate() {
	var command = path.dirname(__filename) + `/rg_sjis.exe --mode-uninstall "${process.execPath}"`;
	exec(command, function(error:any, stdout:any, stderr:any) {
		// シェル上でコマンドを実行できなかった場合のエラー処理
		if (error !== null) {
		  console.log('exec error: ' + error);
		  return;
		}
	});
}
