import * as vscode from 'vscode';
var exec = require('child_process').exec;
const path = require('path');

export function activate(context: vscode.ExtensionContext) {
	var command = path.dirname(__filename) + "/rg_sjis.exe";
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
	var command = path.dirname(__filename) + "/rg_sjis.exe --mode-uninstall";
	exec(command, function(error:any, stdout:any, stderr:any) {
		// シェル上でコマンドを実行できなかった場合のエラー処理
		if (error !== null) {
		  console.log('exec error: ' + error);
		  return;
		}
	});
}
