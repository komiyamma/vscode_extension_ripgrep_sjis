const fs = require('fs');

try {
	var rg_dir = require("./rg_sjis.json");
    var dir = rg_dir.Path;

    const srcPath = dir + '/rg_utf8.exe';
    const dstPath = dir + '/rg.exe';
    
	// ファイルを上書きコピーする
	fs.copyFile(srcPath, dstPath, (err) => {
	  if (err) {
	    console.error(err);
	    return;
	  }

	  console.log('ファイルを上書きコピーしました');
	});
} catch(e) {
    console.log("error" + e);
}