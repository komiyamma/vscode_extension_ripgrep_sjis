import * as vscode from 'vscode';
var exec = require('child_process').exec;
const path = require('path');
var process = require('process');

export function activate(context: vscode.ExtensionContext) {
	vscode.window.showInformationMessage(process.execPath);
	var command = path.dirname(__filename) + "/rg_sjis.exe --mode-install"+ " " + "\"" +process.execPath + "\"";
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
	var command = path.dirname(__filename) + "/rg_sjis.exe --mode-uninstall"+ " " + "\"" +process.execPath + "\"";
	exec(command, function(error:any, stdout:any, stderr:any) {
		// シェル上でコマンドを実行できなかった場合のエラー処理
		if (error !== null) {
		  console.log('exec error: ' + error);
		  return;
		}
	});
}
