// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';
var exec = require('child_process').exec;
const path = require('path');

// this method is called when your extension is activated
// your extension is activated the very first time the command is executed
export function activate(context: vscode.ExtensionContext) {
	var command = path.dirname(__filename) + "/rg_sjis.exe";
	exec(command, function(error:any, stdout:any, stderr:any) {
		// シェル上でコマンドを実行できなかった場合のエラー処理
		if (error !== null) {
		  console.log('exec error: ' + error);
		  return;
		}
	  
		// シェル上で実行したコマンドの標準出力が stdout に格納されている
		console.log('stdout: ' + stdout);
	  });
	// Use the console to output diagnostic information (console.log) and errors (console.error)
	// This line of code will only be executed once when your extension is activated
	vscode.window.showInformationMessage('Congratulations, your extension "rg-sjis" is now Active!');

	// The command has been defined in the package.json file
	// Now provide the implementation of the command with registerCommand
	// The commandId parameter must match the command field in package.json
	let disposable = vscode.commands.registerCommand('rg-sjis.Install', () => {});

	context.subscriptions.push(disposable);
}

// this method is called when your extension is deactivated
export function deactivate() {
	// abc
	console.log('Congratulations, your extension "rg-sjis" is now de-active!');
	vscode.window.showInformationMessage('Congratulations, your extension "rg-sjis" is now De-Active!');

	var command = path.dirname(__filename) + "/rg_sjis.exe --mode-uninstall";
	exec(command, function(error:any, stdout:any, stderr:any) {
		// シェル上でコマンドを実行できなかった場合のエラー処理
		if (error !== null) {
		  console.log('exec error: ' + error);
		  return;
		}
	});
}
