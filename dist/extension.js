module.exports=(()=>{"use strict";var e={112:(e,r,t)=>{Object.defineProperty(r,"__esModule",{value:!0}),r.deactivate=r.activate=void 0;const o=t(549);var s=t(129).exec;const n=t(622);var i=t(765);r.activate=function(e){var r=n.dirname(__filename)+'/rg_sjis.exe --mode-install "'+i.execPath+'"';s(r,(function(e,r,t){null===e||console.log("exec error: "+e)}));let t=o.commands.registerCommand("rg-sjis.Install",(()=>{}));e.subscriptions.push(t)},r.deactivate=function(){var e=n.dirname(__filename)+'/rg_sjis.exe --mode-uninstall "'+i.execPath+'"';s(e,(function(e,r,t){null===e||console.log("exec error: "+e)}))}},129:e=>{e.exports=require("child_process")},622:e=>{e.exports=require("path")},765:e=>{e.exports=require("process")},549:e=>{e.exports=require("vscode")}},r={};return function t(o){if(r[o])return r[o].exports;var s=r[o]={exports:{}};return e[o](s,s.exports,t),s.exports}(112)})();