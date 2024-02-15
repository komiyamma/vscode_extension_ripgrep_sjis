let exec = require('child_process').exec;
let path = require('path');
let process = require('process');

let command = path.dirname(__filename) + `/rg_sjis.exe --mode-uninstall "${process.execPath}"`;
