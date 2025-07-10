using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USSIMDataPopulator.Test
{
	class USSIMDataPopulatorFixture
	{
		#region URL and HTML

		const string url = @"https://www.fisheries.noaa.gov/resource/form/three-alpha-codes-seafood-import-monitoring-program";
		const string html = @"<!DOCTYPE html>
<html lang=""en"" dir=""ltr"" prefix=""content: http://purl.org/rss/1.0/modules/content/  dc: http://purl.org/dc/terms/  foaf: http://xmlns.com/foaf/0.1/  og: http://ogp.me/ns#  rdfs: http://www.w3.org/2000/01/rdf-schema#  schema: http://schema.org/  sioc: http://rdfs.org/sioc/ns#  sioct: http://rdfs.org/sioc/types#  skos: http://www.w3.org/2004/02/skos/core#  xsd: http://www.w3.org/2001/XMLSchema# "">
  <head>
    <meta charset=""utf-8"" /><script type=""text/javascript"">(window.NREUM||(NREUM={})).init={ajax:{deny_list:[""bam.nr-data.net""]}};(window.NREUM||(NREUM={})).loader_config={licenseKey:""249ea9cb86"",applicationID:""659846010""};/*! For license information please see nr-loader-rum-1220.min.js.LICENSE.txt */
!function(t,e){""object""==typeof exports&&""object""==typeof module?module.exports=e():""function""==typeof define&&define.amd?define([],e):""object""==typeof exports?exports.NRBA=e():t.NRBA=e()}(self,(function(){return function(){var t,e,n={9034:function(t,e,n){""use strict"";var r=n(4168);e.Z=(0,r.ky)(16)},5973:function(t,e,n){""use strict"";n.d(e,{I:function(){return r}});var r=0,o=navigator.userAgent.match(/Firefox[\/\s](\d+\.\d+)/);o&&(r=+o[1])},4280:function(t,e,n){""use strict"";n.d(e,{H:function(){return o}});var r=document.createElement(""div"");r.innerHTML=""\x3c!--[if lte IE 6]><div></div><![endif]--\x3e\x3c!--[if lte IE 7]><div></div><![endif]--\x3e\x3c!--[if lte IE 8]><div></div><![endif]--\x3e\x3c!--[if lte IE 9]><div></div><![endif]--\x3e"";var o,i=r.getElementsByTagName(""div"").length;o=4===i?6:3===i?7:2===i?8:1===i?9:0},5955:function(t,e,n){""use strict"";n.d(e,{I:function(){return r}});var r=function(t,e){var n=this;return t&&""object""==typeof t?e&&""object""==typeof e?(Object.assign(this,e),void Object.entries(t).forEach((function(t){var e=t[0],r=t[1];n[e]=r}))):console.error(""setting a Configurable requires a model to set its initial properties""):console.error(""setting a Configurable requires an object as input"")}},441:function(t,e,n){""use strict"";n.d(e,{C:function(){return c},L:function(){return u}});var r=n(1424),o=n(5955),i={beacon:r.ce.beacon,errorBeacon:r.ce.errorBeacon,licenseKey:void 0,applicationID:void 0,sa:void 0,queueTime:void 0,applicationTime:void 0,ttGuid:void 0,user:void 0,account:void 0,product:void 0,extra:void 0,jsAttributes:{},userAttributes:void 0,atts:void 0,transactionName:void 0,tNamePlain:void 0},a={};function c(t){if(!t)throw new Error(""All info objects require an agent identifier!"");if(!a[t])throw new Error(""Info for ""+t+"" was never set"");return a[t]}function u(t,e){if(!t)throw new Error(""All info objects require an agent identifier!"");a[t]=new o.I(e,i),(0,r.Qy)(t,a[t],""info"")}},1476:function(t,e,n){""use strict"";n.d(e,{Dg:function(){return c},Mt:function(){return u}});var r=n(1424),o=n(5955),i={privacy:{cookies_enabled:!0},ajax:{deny_list:void 0,enabled:!0},distributed_tracing:{enabled:void 0,exclude_newrelic_header:void 0,cors_use_newrelic_header:void 0,cors_use_tracecontext_headers:void 0,allowed_origins:void 0},ssl:void 0,obfuscate:void 0,jserrors:{enabled:!0},metrics:{enabled:!0},page_action:{enabled:!0},page_view_event:{enabled:!0},page_view_timing:{enabled:!0},session_trace:{enabled:!0},spa:{enabled:!0}},a={};function c(t,e){if(!t)throw new Error(""All configuration objects require an agent identifier!"");a[t]=new o.I(e,i),(0,r.Qy)(t,a[t],""config"")}function u(t,e){if(!t)throw new Error(""All configuration objects require an agent identifier!"");var n=function(t){if(!t)throw new Error(""All configuration objects require an agent identifier!"");if(!a[t])throw new Error(""Configuration for ""+t+"" was never set"");return a[t]}(t);if(n){for(var r=e.split("".""),o=0;o<r.length-1;o++)if(""object""!=typeof(n=n[r[o]]))return;n=n[r[r.length-1]]}return n}},2085:function(t,e,n){""use strict"";n.d(e,{Y:function(){return r}});var r=(0,n(1424).mF)().o},1220:function(t,e,n){""use strict"";n.d(e,{O:function(){return w},s:function(){return x}});var r={};n.r(r),n.d(r,{agent:function(){return a},match:function(){return f},version:function(){return c}});var o=n(4280),i=n(6959),a=null,c=null;if(navigator.userAgent){var u=navigator.userAgent,s=u.match(/Version\/(\S+)\s+Safari/);s&&-1===u.indexOf(""Chrome"")&&-1===u.indexOf(""Chromium"")&&(a=""Safari"",c=s[1])}function f(t,e){if(!a)return!1;if(t!==a)return!1;if(!e)return!0;if(!c)return!1;for(var n=c.split("".""),r=e.split("".""),o=0;o<r.length;o++)if(r[o]!==n[o])return!1;return!0}var d=n(5955),l=n(1424),v=n(4168),p=window.sessionStorage,h=""NRBA_SESSION_ID"";var g=n(1476),y=window.XMLHttpRequest,m=y&&y.prototype,b={};function w(t){if(!t)throw new Error(""All runtime objects require an agent identifier!"");if(!b[t])throw new Error(""Runtime for ""+t+"" was never set"");return b[t]}function x(t,e){if(!t)throw new Error(""All runtime objects require an agent identifier!"");var n,a;b[t]=new d.I(e,(n=t,{customTransaction:void 0,disabled:!1,features:{},maxBytes:6===o.H?2e3:3e4,offset:(0,i.yf)(),onerror:void 0,origin:""""+window.location,ptid:void 0,releaseIds:{},sessionId:!0===(0,g.Mt)(n,""privacy.cookies_enabled"")?(null===(a=p.getItem(h))&&(a=(0,v.ky)(16),p.setItem(h,a)),a):""0"",xhrWrappable:y&&m&&m.addEventListener&&!/CriOS/.test(navigator.userAgent),userAgent:r})),(0,l.Qy)(t,b[t],""runtime"")}},158:function(t,e,n){""use strict"";n.d(e,{q:function(){return r}});var r=[""1220"",""PROD""].filter((function(t){return t})).join(""."")},3707:function(t,e,n){""use strict"";n.d(e,{w:function(){return o}});var r={agentIdentifier:""""},o=function(t){var e=this;if(""object""!=typeof t)return console.error(""shared context requires an object as input"");this.sharedContext={},Object.assign(this.sharedContext,r),Object.entries(t).forEach((function(t){var n=t[0],o=t[1];Object.keys(r).includes(n)&&(e.sharedContext[n]=o)}))}},1776:function(t,e,n){""use strict"";n.d(e,{ee:function(){return r}});var r,o=n(1424),i=n(4217),a=n(357),c=""nr@context"",u=(0,o.fP)();function s(){}function f(){return new s}function d(){(r.backlog.api||r.backlog.feature)&&(r.aborted=!0,r.backlog={})}u.ee?r=u.ee:(r=function t(e,n){var o={},u={},l={},v={on:g,addEventListener:g,removeEventListener:y,emit:h,get:b,listeners:m,context:p,buffer:w,abort:d,aborted:!1,isBuffering:x,debugId:n,backlog:e&&e.backlog?e.backlog:{}};return v;function p(t){return t&&t instanceof s?t:t?(0,i.X)(t,c,f):f()}function h(t,n,o,i,a){if(!1!==a&&(a=!0),!r.aborted||i){e&&a&&e.emit(t,n,o);for(var c=p(o),s=m(t),f=s.length,d=0;d<f;d++)s[d].apply(c,n);var l=O()[u[t]];return l&&l.push([v,t,n,c]),c}}function g(t,e){o[t]=m(t).concat(e)}function y(t,e){var n=o[t];if(n)for(var r=0;r<n.length;r++)n[r]===e&&n.splice(r,1)}function m(t){return o[t]||[]}function b(e){return l[e]=l[e]||t(v,e)}function w(t,e){var n=O();v.aborted||(0,a.D)(t,(function(t,r){e=e||""feature"",u[r]=e,e in n||(n[e]=[])}))}function x(t){return!!O()[u[t]]}function O(){return v.backlog}}(void 0,""globalEE""),u.ee=r)},7361:function(t,e,n){""use strict"";n.d(e,{E:function(){return r},p:function(){return o}});var r=n(1776).ee.get(""handle"");function o(t,e,n,o,i){i?(i.buffer([t],o),i.emit(t,e,n)):(r.buffer([t],o),r.emit(t,e,n))}},3350:function(t,e,n){""use strict"";n.d(e,{X:function(){return i}});var r=n(7361);i.on=a;var o=i.handlers={};function i(t,e,n,i){a(i||r.E,o,t,e,n)}function a(t,e,n,o,i){i||(i=""feature""),t||(t=r.E);var a=e[i]=e[i]||{};(a[n]=a[n]||[]).push([t,o])}},4408:function(t,e,n){""use strict"";n.d(e,{m:function(){return i}});var r=!1;try{var o=Object.defineProperty({},""passive"",{get:function(){r=!0}});window.addEventListener(""testPassive"",null,o),window.removeEventListener(""testPassive"",null,o)}catch(t){}function i(t){return r?{passive:!0,capture:!!t}:!!t}},4168:function(t,e,n){""use strict"";function r(){var t=null,e=0,n=window.crypto||window.msCrypto;function r(){return t?15&t[e++]:16*Math.random()|0}n&&n.getRandomValues&&(t=n.getRandomValues(new Uint8Array(31)));for(var o,i=""xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx"",a="""",c=0;c<i.length;c++)a+=""x""===(o=i[c])?r().toString(16):""y""===o?(o=3&r()|8).toString(16):o;return a}function o(t){var e=null,n=0,r=window.crypto||window.msCrypto;r&&r.getRandomValues&&Uint8Array&&(e=r.getRandomValues(new Uint8Array(31)));for(var o=[],i=0;i<t;i++)o.push(a().toString(16));return o.join("""");function a(){return e?15&e[n++]:16*Math.random()|0}}n.d(e,{Rl:function(){return r},ky:function(){return o}})},6959:function(t,e,n){""use strict"";n.d(e,{nb:function(){return u},os:function(){return s},yf:function(){return c},zO:function(){return a}});var r=n(2364),o=(new Date).getTime(),i=o;function a(){return r.G&&performance.now?Math.round(performance.now()):(o=Math.max((new Date).getTime(),o))-i}function c(){return o}function u(t){i=t}function s(){return i}},2364:function(t,e,n){""use strict"";n.d(e,{G:function(){return r}});var r=void 0!==window.performance&&window.performance.timing&&void 0!==window.performance.timing.navigationStart},1793:function(t,e,n){""use strict"";function r(t){var e,n=0;for(e=0;e<t.length;e++)n+=(e+1)*t.charCodeAt(e);return Math.abs(n)}n.d(e,{v:function(){return s},s:function(){return u}});var o=n(6972),i=n(5973),a=n(6959),c=n(2364),u=!0;function s(t){var e=function(){if(i.I&&i.I<9)return;if(c.G)return u=!1,window.performance.timing.navigationStart}()||function(){for(var t=document.cookie.split("" ""),e=0;e<t.length;e++)if(0===t[e].indexOf(""NREUM="")){for(var n,o,i,a,c=t[e].substring(""NREUM="".length).split(""&""),u=0;u<c.length;u++)0===c[u].indexOf(""s="")?i=c[u].substring(2):0===c[u].indexOf(""p="")?"";""===(o=c[u].substring(2)).charAt(o.length-1)&&(o=o.substr(0,o.length-1)):0===c[u].indexOf(""r="")&&"";""===(n=c[u].substring(2)).charAt(n.length-1)&&(n=n.substr(0,n.length-1));if(n){var s=r(document.referrer);(a=s==n)||(a=r(document.location.href)==n&&s==o)}if(a&&i){if((new Date).getTime()-i>6e4)return;return i}}}();e&&((0,o.B)(t,""starttime"",e),(0,a.nb)(e))}},6972:function(t,e,n){""use strict"";n.d(e,{B:function(){return i},L:function(){return a}});var r=n(6959),o={};function i(t,e,n){void 0===n&&(n=(0,r.zO)()+(0,r.os)()),o[t]=o[t]||{},o[t][e]=n}function a(t,e,n,r){var i,a,c=t.sharedContext.agentIdentifier,u=null==(i=o[c])?void 0:i[n],s=null==(a=o[c])?void 0:a[r];void 0!==u&&void 0!==s&&t.store(""measures"",e,{value:s-u})}},7299:function(t,e,n){""use strict"";n.d(e,{T:function(){return a}});var r=window,o=r;function i(){return o}var a={isFileProtocol:function(){var t=i(),e=!(!t.location||!t.location.protocol||""file:""!==t.location.protocol);e&&(a.supportabilityMetricSent=!0);return e},supportabilityMetricSent:!1}},847:function(t,e,n){""use strict"";n.d(e,{K:function(){return a}});var r=n(1220),o=n(1476),i=[""ajax"",""jserrors"",""metrics"",""page_action"",""page_view_event"",""page_view_timing"",""session_trace"",""spa""];function a(t){var e={};return i.forEach((function(n){e[n]=function(t,e){return!0!==(0,r.O)(e).disabled&&!1!==(0,o.Mt)(e,t+"".enabled"")}(n,t)})),e}},5023:function(t,e,n){""use strict"";n.d(e,{W:function(){return o}});var r=n(1776),o=function(t,e,n){void 0===n&&(n=[]),this.agentIdentifier=t,this.aggregator=e,this.ee=r.ee.get(t),this.externalFeatures=n}},4217:function(t,e,n){""use strict"";n.d(e,{X:function(){return o}});var r=Object.prototype.hasOwnProperty;function o(t,e,n){if(r.call(t,e))return t[e];var o=n();if(Object.defineProperty&&Object.keys)try{return Object.defineProperty(t,e,{value:o,writable:!0,enumerable:!1}),o}catch(t){}return t[e]=o,o}},357:function(t,e,n){""use strict"";n.d(e,{D:function(){return o}});var r=Object.prototype.hasOwnProperty;function o(t,e){var n=[],o="""",i=0;for(o in t)r.call(t,o)&&(n[i]=e(o,t[o]),i+=1);return n}},603:function(t,e,n){""use strict"";n.d(e,{$c:function(){return s},Ng:function(){return f},RR:function(){return u}});var r=n(1476),o=n(3707),i=n(7299);function a(t,e){return a=Object.setPrototypeOf?Object.setPrototypeOf.bind():function(t,e){return t.__proto__=e,t},a(t,e)}var c={regex:/^file:\/\/(.*)/,replacement:""file://OBFUSCATED""},u=function(t){var e,n;function r(e){return t.call(this,e)||this}n=t,(e=r).prototype=Object.create(n.prototype),e.prototype.constructor=e,a(e,n);var o=r.prototype;return o.shouldObfuscate=function(){return s(this.sharedContext.agentIdentifier).length>0},o.obfuscateString=function(t){if(!t||""string""!=typeof t)return t;for(var e=s(this.sharedContext.agentIdentifier),n=t,r=0;r<e.length;r++){var o=e[r].regex,i=e[r].replacement||""*"";n=n.replace(o,i)}return n},r}(o.w);function s(t){var e=[],n=(0,r.Mt)(t,""obfuscate"")||[];return e=e.concat(n),i.T.isFileProtocol()&&e.push(c),e}function f(t){for(var e=!1,n=!1,r=0;r<t.length;r++){""regex""in t[r]?""string""!=typeof t[r].regex&&t[r].regex.constructor!==RegExp&&(console&&console.warn&&console.warn('An obfuscation replacement rule contains a ""regex"" value with an invalid type (must be a string or RegExp)'),n=!0):(console&&console.warn&&console.warn('An obfuscation replacement rule was detected missing a ""regex"" value.'),n=!0);var o=t[r].replacement;o&&""string""!=typeof o&&(console&&console.warn&&console.warn('An obfuscation replacement rule contains a ""replacement"" value with an invalid type (must be a string)'),e=!0)}return!e&&!n}},1424:function(t,e,n){""use strict"";n.d(e,{EZ:function(){return u},Qy:function(){return c},ce:function(){return o},fP:function(){return i},gG:function(){return s},mF:function(){return a}});var r=n(6959),o={beacon:""bam.nr-data.net"",errorBeacon:""bam.nr-data.net""};function i(){return window.NREUM||(window.NREUM={}),void 0===window.newrelic&&(window.newrelic=window.NREUM),window.NREUM}function a(){var t=i();if(!t.o){var e=window,n=e.XMLHttpRequest;t.o={ST:setTimeout,SI:e.setImmediate,CT:clearTimeout,XHR:n,REQ:e.Request,EV:e.Event,PR:e.Promise,MO:e.MutationObserver,FETCH:e.fetch}}return t}function c(t,e,n){var o,a,c=i(),u=c.initializedAgents||{},s=u[t]||{};return Object.keys(s).length||(s.initializedAt={ms:(0,r.zO)(),date:new Date}),c.initializedAgents=Object.assign({},u,((a={})[t]=Object.assign({},s,((o={})[n]=e,o)),a)),c}function u(t,e){i()[t]=e}function s(){var t,e;return t=i(),e=t.info||{},t.info=Object.assign({beacon:o.beacon,errorBeacon:o.errorBeacon},e),function(){var t=i(),e=t.init||{};t.init=Object.assign({},e)}(),a(),function(){var t=i(),e=t.loader_config||{};t.loader_config=Object.assign({},e)}(),i()}},8539:function(t){t.exports=function(t,e,n){e||(e=0),void 0===n&&(n=t?t.length:0);for(var r=-1,o=n-e||0,i=Array(o<0?0:o);++r<o;)i[r]=t[e+r];return i}}},r={};function o(t){var e=r[t];if(void 0!==e)return e.exports;var i=r[t]={exports:{}};return n[t](i,i.exports,o),i.exports}o.m=n,o.n=function(t){var e=t&&t.__esModule?function(){return t.default}:function(){return t};return o.d(e,{a:e}),e},o.d=function(t,e){for(var n in e)o.o(e,n)&&!o.o(t,n)&&Object.defineProperty(t,n,{enumerable:!0,get:e[n]})},o.f={},o.e=function(t){return Promise.all(Object.keys(o.f).reduce((function(e,n){return o.f[n](t,e),e}),[]))},o.u=function(t){return t+"".""+o.h().slice(0,8)+""-1220.js""},o.h=function(){return""2d6a2503b7f18a5b77dd""},o.o=function(t,e){return Object.prototype.hasOwnProperty.call(t,e)},t={},e=""NRBA:"",o.l=function(n,r,i,a){if(t[n])t[n].push(r);else{var c,u;if(void 0!==i)for(var s=document.getElementsByTagName(""script""),f=0;f<s.length;f++){var d=s[f];if(d.getAttribute(""src"")==n||d.getAttribute(""data-webpack"")==e+i){c=d;break}}c||(u=!0,(c=document.createElement(""script"")).charset=""utf-8"",c.timeout=120,o.nc&&c.setAttribute(""nonce"",o.nc),c.setAttribute(""data-webpack"",e+i),c.src=n),t[n]=[r];var l=function(e,r){c.onerror=c.onload=null,clearTimeout(v);var o=t[n];if(delete t[n],c.parentNode&&c.parentNode.removeChild(c),o&&o.forEach((function(t){return t(r)})),e)return e(r)},v=setTimeout(l.bind(null,void 0,{type:""timeout"",target:c}),12e4);c.onerror=l.bind(null,c.onerror),c.onload=l.bind(null,c.onload),u&&document.head.appendChild(c)}},o.r=function(t){""undefined""!=typeof Symbol&&Symbol.toStringTag&&Object.defineProperty(t,Symbol.toStringTag,{value:""Module""}),Object.defineProperty(t,""__esModule"",{value:!0})},o.p=""https://js-agent.newrelic.com/"",function(){var t={299:0,82:0};o.f.j=function(e,n){var r=o.o(t,e)?t[e]:void 0;if(0!==r)if(r)n.push(r[2]);else{var i=new Promise((function(n,o){r=t[e]=[n,o]}));n.push(r[2]=i);var a=o.p+o.u(e),c=new Error;o.l(a,(function(n){if(o.o(t,e)&&(0!==(r=t[e])&&(t[e]=void 0),r)){var i=n&&(""load""===n.type?""missing"":n.type),a=n&&n.target&&n.target.src;c.message=""Loading chunk ""+e+"" failed.\n(""+i+"": ""+a+"")"",c.name=""ChunkLoadError"",c.type=i,c.request=a,r[1](c)}}),""chunk-""+e,e)}};var e=function(e,n){var r,i,a=n[0],c=n[1],u=n[2],s=0;if(a.some((function(e){return 0!==t[e]}))){for(r in c)o.o(c,r)&&(o.m[r]=c[r]);if(u)u(o)}for(e&&e(n);s<a.length;s++)i=a[s],o.o(t,i)&&t[i]&&t[i][0](),t[i]=0},n=self.webpackChunkNRBA=self.webpackChunkNRBA||[];n.forEach(e.bind(null,0)),n.push=e.bind(null,n.push.bind(n))}();var i={};return function(){""use strict"";o.r(i);var t=o(9034),e=o(4408),n=window,r=n.document;function a(t){""complete""===r.readyState&&t()}function c(t){a(t),r.addEventListener?n.addEventListener(""load"",t,(0,e.m)(!1)):n.attachEvent(""onload"",t)}function u(t){a(t),r.addEventListener?r.addEventListener(""DOMContentLoaded"",t,(0,e.m)(!1)):r.attachEvent(""onreadystatechange"",a)}var s=o(1776);function f(){f=function(){return t};var t={},e=Object.prototype,n=e.hasOwnProperty,r=""function""==typeof Symbol?Symbol:{},o=r.iterator||""@@iterator"",i=r.asyncIterator||""@@asyncIterator"",a=r.toStringTag||""@@toStringTag"";function c(t,e,n){return Object.defineProperty(t,e,{value:n,enumerable:!0,configurable:!0,writable:!0}),t[e]}try{c({},"""")}catch(t){c=function(t,e,n){return t[e]=n}}function u(t,e,n,r){var o=e&&e.prototype instanceof l?e:l,i=Object.create(o.prototype),a=new j(r||[]);return i._invoke=function(t,e,n){var r=""suspendedStart"";return function(o,i){if(""executing""===r)throw new Error(""Generator is already running"");if(""completed""===r){if(""throw""===o)throw i;return P()}for(n.method=o,n.arg=i;;){var a=n.delegate;if(a){var c=x(a,n);if(c){if(c===d)continue;return c}}if(""next""===n.method)n.sent=n._sent=n.arg;else if(""throw""===n.method){if(""suspendedStart""===r)throw r=""completed"",n.arg;n.dispatchException(n.arg)}else""return""===n.method&&n.abrupt(""return"",n.arg);r=""executing"";var u=s(t,e,n);if(""normal""===u.type){if(r=n.done?""completed"":""suspendedYield"",u.arg===d)continue;return{value:u.arg,done:n.done}}""throw""===u.type&&(r=""completed"",n.method=""throw"",n.arg=u.arg)}}}(t,n,a),i}function s(t,e,n){try{return{type:""normal"",arg:t.call(e,n)}}catch(t){return{type:""throw"",arg:t}}}t.wrap=u;var d={};function l(){}function v(){}function p(){}var h={};c(h,o,(function(){return this}));var g=Object.getPrototypeOf,y=g&&g(g(_([])));y&&y!==e&&n.call(y,o)&&(h=y);var m=p.prototype=l.prototype=Object.create(h);function b(t){[""next"",""throw"",""return""].forEach((function(e){c(t,e,(function(t){return this._invoke(e,t)}))}))}function w(t,e){function r(o,i,a,c){var u=s(t[o],t,i);if(""throw""!==u.type){var f=u.arg,d=f.value;return d&&""object""==typeof d&&n.call(d,""__await"")?e.resolve(d.__await).then((function(t){r(""next"",t,a,c)}),(function(t){r(""throw"",t,a,c)})):e.resolve(d).then((function(t){f.value=t,a(f)}),(function(t){return r(""throw"",t,a,c)}))}c(u.arg)}var o;this._invoke=function(t,n){function i(){return new e((function(e,o){r(t,n,e,o)}))}return o=o?o.then(i,i):i()}}function x(t,e){var n=t.iterator[e.method];if(void 0===n){if(e.delegate=null,""throw""===e.method){if(t.iterator.return&&(e.method=""return"",e.arg=void 0,x(t,e),""throw""===e.method))return d;e.method=""throw"",e.arg=new TypeError(""The iterator does not provide a 'throw' method"")}return d}var r=s(n,t.iterator,e.arg);if(""throw""===r.type)return e.method=""throw"",e.arg=r.arg,e.delegate=null,d;var o=r.arg;return o?o.done?(e[t.resultName]=o.value,e.next=t.nextLoc,""return""!==e.method&&(e.method=""next"",e.arg=void 0),e.delegate=null,d):o:(e.method=""throw"",e.arg=new TypeError(""iterator result is not an object""),e.delegate=null,d)}function O(t){var e={tryLoc:t[0]};1 in t&&(e.catchLoc=t[1]),2 in t&&(e.finallyLoc=t[2],e.afterLoc=t[3]),this.tryEntries.push(e)}function E(t){var e=t.completion||{};e.type=""normal"",delete e.arg,t.completion=e}function j(t){this.tryEntries=[{tryLoc:""root""}],t.forEach(O,this),this.reset(!0)}function _(t){if(t){var e=t[o];if(e)return e.call(t);if(""function""==typeof t.next)return t;if(!isNaN(t.length)){var r=-1,i=function e(){for(;++r<t.length;)if(n.call(t,r))return e.value=t[r],e.done=!1,e;return e.value=void 0,e.done=!0,e};return i.next=i}}return{next:P}}function P(){return{value:void 0,done:!0}}return v.prototype=p,c(m,""constructor"",p),c(p,""constructor"",v),v.displayName=c(p,a,""GeneratorFunction""),t.isGeneratorFunction=function(t){var e=""function""==typeof t&&t.constructor;return!!e&&(e===v||""GeneratorFunction""===(e.displayName||e.name))},t.mark=function(t){return Object.setPrototypeOf?Object.setPrototypeOf(t,p):(t.__proto__=p,c(t,a,""GeneratorFunction"")),t.prototype=Object.create(m),t},t.awrap=function(t){return{__await:t}},b(w.prototype),c(w.prototype,i,(function(){return this})),t.AsyncIterator=w,t.async=function(e,n,r,o,i){void 0===i&&(i=Promise);var a=new w(u(e,n,r,o),i);return t.isGeneratorFunction(n)?a:a.next().then((function(t){return t.done?t.value:a.next()}))},b(m),c(m,a,""Generator""),c(m,o,(function(){return this})),c(m,""toString"",(function(){return""[object Generator]""})),t.keys=function(t){var e=[];for(var n in t)e.push(n);return e.reverse(),function n(){for(;e.length;){var r=e.pop();if(r in t)return n.value=r,n.done=!1,n}return n.done=!0,n}},t.values=_,j.prototype={constructor:j,reset:function(t){if(this.prev=0,this.next=0,this.sent=this._sent=void 0,this.done=!1,this.delegate=null,this.method=""next"",this.arg=void 0,this.tryEntries.forEach(E),!t)for(var e in this)""t""===e.charAt(0)&&n.call(this,e)&&!isNaN(+e.slice(1))&&(this[e]=void 0)},stop:function(){this.done=!0;var t=this.tryEntries[0].completion;if(""throw""===t.type)throw t.arg;return this.rval},dispatchException:function(t){if(this.done)throw t;var e=this;function r(n,r){return a.type=""throw"",a.arg=t,e.next=n,r&&(e.method=""next"",e.arg=void 0),!!r}for(var o=this.tryEntries.length-1;o>=0;--o){var i=this.tryEntries[o],a=i.completion;if(""root""===i.tryLoc)return r(""end"");if(i.tryLoc<=this.prev){var c=n.call(i,""catchLoc""),u=n.call(i,""finallyLoc"");if(c&&u){if(this.prev<i.catchLoc)return r(i.catchLoc,!0);if(this.prev<i.finallyLoc)return r(i.finallyLoc)}else if(c){if(this.prev<i.catchLoc)return r(i.catchLoc,!0)}else{if(!u)throw new Error(""try statement without catch or finally"");if(this.prev<i.finallyLoc)return r(i.finallyLoc)}}}},abrupt:function(t,e){for(var r=this.tryEntries.length-1;r>=0;--r){var o=this.tryEntries[r];if(o.tryLoc<=this.prev&&n.call(o,""finallyLoc"")&&this.prev<o.finallyLoc){var i=o;break}}i&&(""break""===t||""continue""===t)&&i.tryLoc<=e&&e<=i.finallyLoc&&(i=null);var a=i?i.completion:{};return a.type=t,a.arg=e,i?(this.method=""next"",this.next=i.finallyLoc,d):this.complete(a)},complete:function(t,e){if(""throw""===t.type)throw t.arg;return""break""===t.type||""continue""===t.type?this.next=t.arg:""return""===t.type?(this.rval=this.arg=t.arg,this.method=""return"",this.next=""end""):""normal""===t.type&&e&&(this.next=e),d},finish:function(t){for(var e=this.tryEntries.length-1;e>=0;--e){var n=this.tryEntries[e];if(n.finallyLoc===t)return this.complete(n.completion,n.afterLoc),E(n),d}},catch:function(t){for(var e=this.tryEntries.length-1;e>=0;--e){var n=this.tryEntries[e];if(n.tryLoc===t){var r=n.completion;if(""throw""===r.type){var o=r.arg;E(n)}return o}}throw new Error(""illegal catch attempt"")},delegateYield:function(t,e,n){return this.delegate={iterator:_(t),resultName:e,nextLoc:n},""next""===this.method&&(this.arg=void 0),d}},t}function d(t,e,n,r,o,i,a){try{var c=t[i](a),u=c.value}catch(t){return void n(t)}c.done?e(u):Promise.resolve(u).then(r,o)}var l=0;function v(t){var e;(e=f().mark((function e(){var n,r;return f().wrap((function(e){for(;;)switch(e.prev=e.next){case 0:if(!l++){e.next=2;break}return e.abrupt(""return"");case 2:return e.prev=2,e.next=5,o.e(552).then(o.bind(o,5552));case 5:return n=e.sent,r=n.aggregator,e.next=9,r(t);case 9:e.next=15;break;case 11:e.prev=11,e.t0=e.catch(2),console.error(""Failed to successfully load all aggregators. Aborting...\n"",e.t0),s.ee.abort();case 15:case""end"":return e.stop()}}),e,null,[[2,11]])})),function(){var t=this,n=arguments;return new Promise((function(r,o){var i=e.apply(t,n);function a(t){d(i,r,o,a,c,""next"",t)}function c(t){d(i,r,o,a,c,""throw"",t)}a(void 0)}))})()}var p=o(8539),h=o.n(p),g=o(1424),y=o(1220),m=o(441),b=o(7361),w=o(357),x=o(6959);var O=o(1476),E=o(5955),j={accountID:void 0,trustKey:void 0,agentID:void 0,licenseKey:void 0,applicationID:void 0,xpid:void 0},_={};var P=!1;var A=o(6972),k=o(1793),L=o(5023);function S(t,e){return S=Object.setPrototypeOf?Object.setPrototypeOf.bind():function(t,e){return t.__proto__=e,t},S(t,e)}var T,C,I,R=function(t){var e,n;function r(e){var n;return n=t.call(this,e)||this,(0,k.v)(e),(0,A.B)(e,""firstbyte"",(0,x.yf)()),c((function(){return n.measureWindowLoaded()})),u((function(){return n.measureDomContentLoaded()})),n}n=t,(e=r).prototype=Object.create(n.prototype),e.prototype.constructor=e,S(e,n);var o=r.prototype;return o.measureWindowLoaded=function(){var t=(0,x.zO)();(0,A.B)(this.agentIdentifier,""onload"",t+(0,x.os)()),(0,b.p)(""timing"",[""load"",t],void 0,void 0,this.ee)},o.measureDomContentLoaded=function(){(0,A.B)(this.agentIdentifier,""domContent"",(0,x.zO)()+(0,x.os)())},r}(L.W);void 0!==document.hidden?(T=""hidden"",C=""visibilitychange"",I=""visibilityState""):void 0!==document.msHidden?(T=""msHidden"",C=""msvisibilitychange""):void 0!==document.webkitHidden&&(T=""webkitHidden"",C=""webkitvisibilitychange"",I=""webkitVisibilityState"");var M=o(2085);function N(t,e){return N=Object.setPrototypeOf?Object.setPrototypeOf.bind():function(t,e){return t.__proto__=e,t},N(t,e)}var D=function(t){var n,r;function o(n){var r,o;if((r=t.call(this,n)||this).pageHiddenTime=""hidden""===document.visibilityState?-1:1/0,r.performanceObserver,r.lcpPerformanceObserver,r.clsPerformanceObserver,r.fiRecorded=!1,!r.isEnabled())return function(t){if(void 0===t)throw new ReferenceError(""this hasn't been initialised - super() hasn't been called"");return t}(r);if(""PerformanceObserver""in window&&""function""==typeof window.PerformanceObserver){r.performanceObserver=new PerformanceObserver((function(){var t;return(t=r).perfObserver.apply(t,arguments)}));try{r.performanceObserver.observe({entryTypes:[""paint""]})}catch(t){}r.lcpPerformanceObserver=new PerformanceObserver((function(){var t;return(t=r).lcpObserver.apply(t,arguments)}));try{r.lcpPerformanceObserver.observe({entryTypes:[""largest-contentful-paint""]})}catch(t){}r.clsPerformanceObserver=new PerformanceObserver((function(){var t;return(t=r).clsObserver.apply(t,arguments)}));try{r.clsPerformanceObserver.observe({type:""layout-shift"",buffered:!0})}catch(t){}}if(""addEventListener""in document){r.fiRecorded=!1;[""click"",""keydown"",""mousedown"",""pointerdown"",""touchstart""].forEach((function(t){document.addEventListener(t,(function(){var t;return(t=r).captureInteraction.apply(t,arguments)}),(0,e.m)(!1))}))}return o=function(){var t;return(t=r).captureVisibilityChange.apply(t,arguments)},""addEventListener""in document&&C&&document.addEventListener(C,(function(){I&&document[I]?o(document[I]):document[T]?o(""hidden""):o(""visible"")}),(0,e.m)(!1)),r}r=t,(n=o).prototype=Object.create(r.prototype),n.prototype.constructor=n,N(n,r);var i=o.prototype;return i.isEnabled=function(){return!1!==(0,O.Mt)(this.agentIdentifier,""page_view_timing.enabled"")},i.perfObserver=function(t,e){var n=this;t.getEntries().forEach((function(t){""first-paint""===t.name?(0,b.p)(""timing"",[""fp"",Math.floor(t.startTime)],void 0,void 0,n.ee):""first-contentful-paint""===t.name&&(0,b.p)(""timing"",[""fcp"",Math.floor(t.startTime)],void 0,void 0,n.ee)}))},i.lcpObserver=function(t,e){var n=t.getEntries();if(n.length>0){var r=n[n.length-1];if(this.pageHiddenTime<r.startTime)return;var o=[r],i=this.addConnectionAttributes({});i&&o.push(i),(0,b.p)(""lcp"",o,void 0,void 0,this.ee)}},i.clsObserver=function(t){var e=this;t.getEntries().forEach((function(t){t.hadRecentInput||(0,b.p)(""cls"",[t],void 0,void 0,e.ee)}))},i.addConnectionAttributes=function(t){var e=navigator.connection||navigator.mozConnection||navigator.webkitConnection;if(e)return e.type&&(t[""net-type""]=e.type),e.effectiveType&&(t[""net-etype""]=e.effectiveType),e.rtt&&(t[""net-rtt""]=e.rtt),e.downlink&&(t[""net-dlink""]=e.downlink),t},i.captureInteraction=function(t){if(t instanceof M.Y.EV&&!this.fiRecorded){var e=Math.round(t.timeStamp),n={type:t.type};this.addConnectionAttributes(n),e<=(0,x.zO)()?n.fid=(0,x.zO)()-e:e>(0,x.os)()&&e<=Date.now()?(e-=(0,x.os)(),n.fid=(0,x.zO)()-e):e=(0,x.zO)(),this.fiRecorded=!0,(0,b.p)(""timing"",[""fi"",e,n],void 0,void 0,this.ee)}},i.captureVisibilityChange=function(t){""hidden""===t&&(this.pageHiddenTime=(0,x.zO)(),(0,b.p)(""pageHide"",[this.pageHiddenTime],void 0,void 0,this.ee))},o}(L.W),q=o(3350),B=""React"",z=""Angular"",G=""AngularJS"",H=""Backbone"",F=""Ember"",V=""Vue"",Z=""Meteor"",U=""Zepto"",X=""Jquery"";function Q(){var t=[];try{(function(){try{if(window.React||window.ReactDOM||window.ReactRedux)return!0;if(document.querySelector(""[data-reactroot], [data-reactid]""))return!0;for(var t=document.querySelectorAll(""body > div""),e=0;e<t.length;e++)if(Object.keys(t[e]).indexOf(""_reactRootContainer"")>=0)return!0;return!1}catch(t){return!1}})()&&t.push(B),function(){try{return!!window.angular||(!!document.querySelector("".ng-binding, [ng-app], [data-ng-app], [ng-controller], [data-ng-controller], [ng-repeat], [data-ng-repeat]"")||!!document.querySelector('script[src*=""angular.js""], script[src*=""angular.min.js""]'))}catch(t){return!1}}()&&t.push(G),function(){try{return!!(window.hasOwnProperty(""ng"")&&window.ng.hasOwnProperty(""coreTokens"")&&window.ng.coreTokens.hasOwnProperty(""NgZone""))||!!document.querySelectorAll(""[ng-version]"").length}catch(t){return!1}}()&&t.push(z),window.Backbone&&t.push(H),window.Ember&&t.push(F),window.Vue&&t.push(V),window.Meteor&&t.push(Z),window.Zepto&&t.push(U),window.jQuery&&t.push(X)}catch(t){}return t}var W=o(7299),K=o(603),Y=o(158);function J(t,e){return J=Object.setPrototypeOf?Object.setPrototypeOf.bind():function(t,e){return t.__proto__=e,t},J(t,e)}var $=function(t){var e,n;function r(e){var n;return(n=t.call(this,e)||this).singleChecks(),(0,q.X)(""record-supportability"",(function(){var t;return(t=n).recordSupportability.apply(t,arguments)}),void 0,n.ee),(0,q.X)(""record-custom"",(function(){var t;return(t=n).recordCustom.apply(t,arguments)}),void 0,n.ee),n}n=t,(e=r).prototype=Object.create(n.prototype),e.prototype.constructor=e,J(e,n);var o=r.prototype;return o.recordSupportability=function(t,e){var n=[""sm"",t,{name:t},e];return(0,b.p)(""storeMetric"",n,null,void 0,this.ee),n},o.recordCustom=function(t,e){var n=[""cm"",t,{name:t},e];return(0,b.p)(""storeEventMetrics"",n,null,void 0,this.ee),n},o.singleChecks=function(){var t=this;this.recordSupportability(""Generic/Version/""+Y.q+""/Detected""),u((function(){Q().forEach((function(e){t.recordSupportability(""Framework/""+e+""/Detected"")}))})),W.T.isFileProtocol()&&(this.recordSupportability(""Generic/FileProtocol/Detected""),W.T.supportabilityMetricSent=!0);var e=(0,K.$c)(this.agentIdentifier);e.length>0&&this.recordSupportability(""Generic/Obfuscate/Detected""),e.length>0&&!(0,K.Ng)(e)&&this.recordSupportability(""Generic/Obfuscate/Invalid"")},r}(L.W),tt=o(847);new Promise((function(e,n){if(P)e(P);else{var r=(0,g.gG)();try{(0,m.L)(t.Z,r.info),(0,O.Dg)(t.Z,r.init),function(t,e){if(!t)throw new Error(""All loader-config objects require an agent identifier!"");_[t]=new E.I(e,j),(0,g.Qy)(t,_[t],""loader_config"")}(t.Z,r.loader_config),(0,y.s)(t.Z,{}),function(t){var e=(0,g.fP)(),n=s.ee.get(t),r=n.get(""tracer""),o=""api-"",i=""api-ixn-"";function a(){}(0,w.D)([""setErrorHandler"",""finished"",""addToTrace"",""inlineHit"",""addRelease""],(function(t,n){e[n]=u(o,n,!0,""api"")})),e.addPageAction=u(o,""addPageAction"",!0),e.setCurrentRouteName=u(o,""routeName"",!0),e.setPageViewName=function(e,n){if(""string""==typeof e)return""/""!==e.charAt(0)&&(e=""/""+e),(0,y.O)(t).customTransaction=(n||""http://custom.transaction"")+e,u(o,""setPageViewName"",!0,""api"")()},e.setCustomAttribute=function(e,n){var r,i=(0,m.C)(t);return(0,m.L)(t,Object.assign({},i,{jsAttributes:Object.assign({},i.jsAttributes,(r={},r[e]=n,r))})),u(o,""setCustomAttribute"",!0,""api"")()},e.interaction=function(){return(new a).get()};var c=a.prototype={createTracer:function(t,e){var o={},i=this,a=""function""==typeof e;return(0,b.p)(""api-ixn-tracer"",[(0,x.zO)(),t,o],i,void 0,n),function(){if(r.emit((a?"""":""no-"")+""fn-start"",[(0,x.zO)(),i,a],o),a)try{return e.apply(this,arguments)}catch(t){throw r.emit(""fn-err"",[arguments,this,""string""==typeof t?new Error(t):t],o),t}finally{r.emit(""fn-end"",[(0,x.zO)()],o)}}}};function u(t,e,r,o){return function(){return(0,b.p)(""record-supportability"",[""API/""+e+""/called""],void 0,void 0,n),(0,b.p)(t+e,[(0,x.zO)()].concat(h()(arguments)),r?null:this,o,n),r?void 0:this}}(0,w.D)(""actionText,setName,setAttribute,save,ignore,onEnd,getContext,end,get"".split("",""),(function(t,e){c[e]=u(i,e)})),e.noticeError=function(t,e){""string""==typeof t&&(t=new Error(t)),(0,b.p)(""record-supportability"",[""API/noticeError/called""],void 0,void 0,n),(0,b.p)(""err"",[t,(0,x.zO)(),!1,e],void 0,void 0,n)}}(t.Z),e(P=!0)}catch(t){n(t)}}})).then((function(){var e,n,r,o=(0,tt.K)(t.Z);o.page_view_event&&new R(t.Z),o.page_view_timing&&new D(t.Z),o.metrics&&new $(t.Z),e=""lite"",n?setTimeout((function(){return v(e)}),r||1e3):c((function(){return v(e)}))}))}(),i}()}));</script>
<style>/* @see https://github.com/aFarkas/lazysizes#broken-image-symbol */.js img.dam-image:not([src]) { visibility: hidden; }/* @see https://github.com/aFarkas/lazysizes#automatically-setting-the-sizes-attribute */.js img.lazyloaded[data-sizes=auto] { display: block; width: 100%; }/* Transition effect. */.js .dam-image, .js .lazyloading { opacity: 0; }.js .lazyloaded { opacity: 1; -webkit-transition: opacity 2000ms; transition: opacity 2000ms; }</style>
<link rel=""canonical"" href=""https://www.fisheries.noaa.gov/resource/form/three-alpha-codes-seafood-import-monitoring-program"" />
<meta http-equiv=""content-language"" content=""en"" />
<meta name=""news_keywords"" content=""International Trade, Foreign Trade , Seafood Commerce and Trade"" />
<meta name=""description"" content=""The Seafood Import Monitoring Program, or SIMP, establishes the reporting and recordkeeping requirements needed to prevent illegal, unreported, and unregulated fishing and/or misrepresented seafood from entering U.S. commerce. The three alpha codes list includes the species that require the full set of SIMP records."" />
<meta name=""keywords"" content=""International Trade, Foreign Trade , Seafood Commerce and Trade"" />
<meta name=""dcterms.title"" content=""Three Alpha Codes for Seafood Import Monitoring Program | NOAA Fisheries"" />
<meta name=""dcterms.creator"" content=""NOAA Fisheries"" />
<meta name=""dcterms.subject"" content=""International Affairs, Seafood Commerce &amp; Trade, Sustainable Seafood"" />
<meta name=""dcterms.description"" content=""The Seafood Import Monitoring Program, or SIMP, establishes the reporting and recordkeeping requirements needed to prevent illegal, unreported, and unregulated fishing and/or misrepresented seafood from entering U.S. commerce. The three alpha codes list includes the species that require the full set of SIMP records."" />
<meta name=""dcterms.contributor"" content=""Office of International Affairs, Trade, and Commerce"" />
<meta name=""dcterms.date"" content=""Tue, 01/26/2021 - 20:52"" />
<meta name=""dcterms.language"" content=""en"" />
<meta name=""dcterms.coverage"" content=""International"" />
<meta property=""og:site_name"" content=""NOAA"" />
<meta property=""og:type"" content=""Resource"" />
<meta property=""og:url"" content=""https://www.fisheries.noaa.gov/resource/form/three-alpha-codes-seafood-import-monitoring-program"" />
<meta property=""og:title"" content=""Three Alpha Codes for Seafood Import Monitoring Program"" />
<meta property=""og:description"" content=""The Seafood Import Monitoring Program, or SIMP, establishes the reporting and recordkeeping requirements needed to prevent illegal, unreported, and unregulated fishing and/or misrepresented seafood from entering U.S. commerce. The three alpha codes list includes the species that require the full set of SIMP records."" />
<meta property=""og:image"" content=""https://www.fisheries.noaa.gov/themes/custom/noaa_components/images/open-graph.jpg"" />
<meta name=""twitter:card"" content=""summary_large_image"" />
<meta name=""twitter:site"" content=""@NOAAFisheries"" />
<meta name=""twitter:description"" content=""The three alpha codes list is available for download in PDF and Excel file formats.  Three alpha codes (PDF, 35 pages)"" />
<meta name=""twitter:title"" content=""Three Alpha Codes for Seafood Import Monitoring Program"" />
<meta name=""twitter:creator"" content=""@NOAAFisheries"" />
<meta name=""twitter:url"" content=""https://www.fisheries.noaa.gov/resource/form/three-alpha-codes-seafood-import-monitoring-program"" />
<meta name=""Generator"" content=""Drupal 9 (https://www.drupal.org)"" />
<meta name=""MobileOptimized"" content=""width"" />
<meta name=""HandheldFriendly"" content=""true"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
<link rel=""icon"" href=""/themes/custom/noaa_components/favicon.ico"" type=""image/vnd.microsoft.icon"" />
<script src=""/sites/default/files/google_tag/fisheries/google_tag.script.js?rmln2u"" defer></script>
<script>window.a2a_config=window.a2a_config||{};a2a_config.callbacks=[];a2a_config.overlays=[];a2a_config.templates={};</script>

    <title>Three Alpha Codes for Seafood Import Monitoring Program | NOAA Fisheries</title>
    <link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/align.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/fieldgroup.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/container-inline.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/clearfix.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/details.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/hidden.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/item-list.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/js.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/nowrap.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/position-container.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/progress.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/reset-appearance.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/resize.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/sticky-header.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/system-status-counter.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/system-status-report-counters.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/system-status-report-general-info.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/tablesort.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/core/modules/system/css/components/tree-child.module.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/modules/contrib/addtoany/css/addtoany.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/modules/contrib/extlink/extlink.css?rmln2u"" />
<link rel=""stylesheet"" media=""all"" href=""/themes/custom/noaa_components/dest/style.css?rmln2u"" />

    
    <script src=""https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js""></script>
      </head>
  <body>
        <noscript aria-hidden=""true""><iframe src=""https://www.googletagmanager.com/ns.html?id=GTM-M67WBF"" height=""0"" width=""0"" style=""display:none;visibility:hidden""></iframe></noscript>    <a href=""#main-content"" class=""sr-only sr-only-focusable"">      Skip to main content    </a>
      <div class=""dialog-off-canvas-main-canvas"" data-off-canvas-main-canvas>
    
<header role=""banner"">
    <div class=""region region-header"">
    
<div id=""block-outdatedbrowser"">
  
  
    <div class=""alert alert--alert "">
    <span class=""alert__icon fa fa-exclamation-circle""></span>
    <div class=""alert__content"">
              <span class=""alert__title"">Unsupported Browser Detected</span>
            <div class=""alert__message"">
        <p>Internet Explorer lacks support for the features of this website. For the best experience, please use a modern browser such as Chrome, Firefox, or Edge.</p>
      </div>
    </div>
  </div>

</div>
<nav aria-label=""block-noaamegamenu-menu"" id=""block-noaamegamenu"">
      
  
 
  

        




<header class=""site-header"">
  <div class=""site-header__top"">
    <div class=""site-header__top-bar--wrapper"">
      <div class=""container"">
        <div class=""site-header__top-bar"">
          <div class=""site-header__top-bar-links--left"">
                          <a class=""link--darkergray"" href=""https://www.noaa.gov"">NATIONAL OCEANIC AND ATMOSPHERIC ADMINISTRATION</a> | 
                          <a class=""link--darkergray"" href=""https://www.commerce.gov/"">U.S. DEPARTMENT OF COMMERCE</a>
                      </div>
          <div class=""site-header__top-bar-social"">
            <nav class=""site-header__social"">
                              <a class=""site-header__social-icon fab fa-facebook-f"" href=""https://www.facebook.com/NOAAFisheries""><span class=""sr-only"">Facebook</span></a>
                              <a class=""site-header__social-icon fab fa-instagram"" href=""https://www.instagram.com/noaafisheries""><span class=""sr-only"">Instagram</span></a>
                              <a class=""site-header__social-icon fab fa-twitter"" href=""https://twitter.com/NOAAFisheries""><span class=""sr-only"">Twitter</span></a>
                              <a class=""site-header__social-icon fab fa-youtube"" href=""https://www.youtube.com/user/usnoaafisheriesgov""><span class=""sr-only"">YouTube</span></a>
                              <a class=""site-header__social-icon fa fa-envelope"" href=""https://public.govdelivery.com/accounts/USNOAAFISHERIES/subscriber/new""><span class=""sr-only"">Mail</span></a> | 
                          </nav>
          </div>
          <div class=""site-header__top-bar-links--right"">
                          <div>
                <a class=""link--darkergray"" href=""/site-index"">SITE INDEX</a>   
              </div>
                          <div>
                <a class=""link--darkergray"" href=""/contact-us"">CONTACT US</a>
              </div>
                      </div>
        </div>
      </div>
    </div>
    <div class=""container"">
      <div class=""row"">
        <div class=""col-md-12"">
          
<div id=""block-promobanner"">
  
  
  
</div>
          
<div id=""block-seafoodpromo"">
  
  
  
  <div class=""promo-banner"">
    <a class=""promo-banner__icon fa fa-times"" href=""#""><span class=""sr-only"">Close Promo Banner</span></a>
    
        <div class=""promo-banner__image hidden-xs"">
                      
                  </div>

        <div class=""promo-banner__image hidden-sm hidden-md hidden-lg"">
                      
                  </div>

      </div>
</div>
         </div>
      </div>

      <div class=""row"">
        <div class=""site-header__branding col-lg-6 col-md-5 col-sm-8 col-xs-8"">
                      <div class=""site-header__label"">
              <a href=""/"" title=""Home"" class=""site-header__label-image-link"">
                <img class=""site-header__logo-text"" src=""/themes/custom/noaa_components/images/NOAA_FISHERIES_logoH.png"" alt=""NOAA Fisheries emblem"">
              </a>
              <br>
            </div>
                  </div>

        <div class=""col-lg-6 col-md-7 col-sm-6 hidden-sm hidden-xs"">
          <form id=""input-search-form_header-search"" class=""input-search"" action=""/search"" method=""GET"">
  
  <label for=""header-search"" class=""sr-only"">Search NOAA Fisheries</label>
  <input type=""search"" id=""header-search"" name=""oq"" placeholder=""Search NOAA Fisheries"">

      <button type=""submit"" class=""fa fa-search input-search__button""><span class=""sr-only"">Search</span></button>
  </form>
        </div>

        <nav class=""col-sm-4 col-xs-4 hidden-lg hidden-md site-header__menu"">
          <a class=""site-header__menu-link"" href=""#"" role=""button"" aria-haspopup=""true"" aria-expanded=""false""><span class=""site-header__menu-icon fa fa-bars""></span>Menu</a>
        </nav>
      </div>
    </div>
  </div>

  <div class=""site-header__main hidden-sm hidden-xs"">
    <div class=""container"">
          
      <nav class=""main-menu"">
          <ul class=""main-menu__items""> 
                           
                                                <li class=""main-menu__item main-menu__item--expanded"">
              <a class=""link link--text main-menu__link"" role=""button"" aria-haspopup=""true"" href="""" >Find A Species</a>
                             
                  
    <ul class=""main-menu__items main-menu__container--mega main-menu--hidden"" id=""container-1"">
      <li class=""hidden"">
        <ul class=""main-menu--mega main-menu--1"">
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/find-species"">Find a Species</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/dolphins-porpoises"">Dolphins &amp; Porpoises</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/fish-sharks"">Fish &amp; Sharks</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/highly-migratory-species"">Highly Migratory Species</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/invertebrates"">Invertebrates</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/sea-turtles"">Sea Turtles</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/seals-sea-lions"">Seals &amp; Sea Lions</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/whales"">Whales</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                     <span class=""main-menu__title"">Protected Species</span>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/species-directory/threatened-endangered"">All Threatened &amp; Endangered Species</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/species-directory/marine-mammals"">Marine Mammals</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                     <span class=""main-menu__title"">Species By Region</span>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/species-directory?oq=&amp;field_species_categories_vocab=All&amp;field_region_vocab=1000001106&amp;items_per_page=25"">Alaska</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/species-directory?oq=&amp;field_species_categories_vocab=All&amp;field_region_vocab=1000001111&amp;items_per_page=25"">New England/Mid-Atlantic</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/species-directory?oq=&amp;field_species_categories_vocab=All&amp;field_region_vocab=1000001116&amp;items_per_page=25"">Pacific Islands</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/species-directory?oq=&amp;field_species_categories_vocab=All&amp;field_region_vocab=1000001121&amp;items_per_page=25"">Southeast</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/species-directory?oq=&amp;field_species_categories_vocab=All&amp;field_region_vocab=1000001126&amp;items_per_page=25"">West Coast</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                     <span class=""main-menu__title"">Helpful Resources</span>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/marine-life-viewing-guidelines#guidelines-&amp;-distances"">Marine Life Viewing Guidelines</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/marine-life-distress"">Marine Life in Distress</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/report"">Report a Stranded or Injured Marine Animal</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/endangered-species-conservation/species-in-the-spotlight"">Species in the Spotlight</a>
              </li>
                                    
      </li>
  
            
            </ul>
        </li>
      </ul>
  
                         
                                                <li class=""main-menu__item main-menu__item--expanded"">
              <a class=""link link--text main-menu__link"" role=""button"" aria-haspopup=""true"" href="""" >Fishing &amp; Seafood</a>
                             
                  
    <ul class=""main-menu__items main-menu__container--mega main-menu--hidden"" id=""container-2"">
      <li class=""hidden"">
        <ul class=""main-menu--mega main-menu--1"">
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/topic/sustainable-fisheries"">Sustainable Fisheries</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/bycatch"">Bycatch</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/national/laws-and-policies/catch-shares"">Catch Shares</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/fishery-observers"">Fishery Observers</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/international-affairs/iuu-fishing"">Illegal, Unregulated, Unreported Fishing</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/laws-policies/magnuson-stevens-act"">Magnuson-Stevens Act</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/national/science-data/research-surveys"">Research Surveys</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/population-assessments"">Population Assessments</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/topic/resources-fishing"">Resources for Fishing</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/resources-fishing/commercial-fishing"">Commercial Fishing</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/resources-fishing/recreational-fishing"">Recreational Fishing</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/resources-fishing/subsistence-fishing"">Subsistence Fishing</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/rules-and-announcements/notices-and-rules"">Fishery Management Info</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/permits-and-forms"">Permits &amp; Forms</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/rules-and-regulations"">Rules &amp; Regulations by Region</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/topic/sustainable-seafood"">Sustainable Seafood</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/aquaculture"">Aquaculture</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/seafood-commerce-and-trade"">Commerce &amp; Trade</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/seafood-commerce-and-trade/seafood-inspection"">Seafood Inspection</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/seafood-commerce-and-trade/trade"">Trade</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                     <span class=""main-menu__title"">Related Topics</span>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/atlantic-highly-migratory-species"">Atlantic Highly Migratory Species </a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/offshore-wind-energy"">Offshore Wind Energy</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/sustainable-fisheries/national-cooperative-research-program"">Cooperative Research</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/enforcement"">Enforcement</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/funding-opportunities#financial-services"">Financial Services</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/international-affairs"">International Affairs</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/science-and-data"">Science &amp; Data</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/socioeconomics"">Socioeconomics</a>
              </li>
                                    
      </li>
  
            
            </ul>
        </li>
      </ul>
  
                         
                                                <li class=""main-menu__item main-menu__item--expanded"">
              <a class=""link link--text main-menu__link"" role=""button"" aria-haspopup=""true"" href="""" >Protecting Marine Life</a>
                             
                  
    <ul class=""main-menu__items main-menu__container--mega main-menu--hidden"" id=""container-3"">
      <li class=""hidden"">
        <ul class=""main-menu--mega main-menu--1"">
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/topic/endangered-species-conservation"">Endangered Species Conservation</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/consultations/endangered-species-act-consultations"">Consultations</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/national/endangered-species-conservation/critical-habitat"">Critical Habitat</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/laws-policies/endangered-species-act"">Endangered Species Act</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/national/science-data/research-surveys"">Research Surveys</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/population-assessments/endangered-species"">Population Assessments</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/national/endangered-species-conservation/recovery-species-under-endangered-species-act"">Species Recovery</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/endangered-species-conservation/species-in-the-spotlight"">Species in the Spotlight</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/topic/marine-mammal-protection"">Marine Mammal Protection</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/national/marine-life-distress/marine-mammal-health-and-stranding-response-program"">Health &amp; Stranding Response</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/laws-policies/marine-mammal-protection-act"">Marine Mammal Protection Act</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/national/science-data/research-surveys"">Research Surveys</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/population-assessments/marine-mammals"">Population Assessments</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/national/marine-mammal-protection/marine-mammal-take-reduction-plans-and-teams"">Take Reduction Plans</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/topic/marine-life-distress"">Marine Life in Distress</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/report"">Report a Stranded or Injured Marine Animal</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/bycatch"">Bycatch</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/national/science-data/ocean-noise"">Ocean Acoustics/Noise</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/national/marine-mammal-protection/marine-mammal-unusual-mortality-events"">Unusual Mortality Events</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/insight/understanding-vessel-strikes"">Vessel Strikes</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                     <span class=""main-menu__title"">Related Topics</span>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/marine-life-viewing-guidelines"">Marine Life Viewing Guidelines</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/enforcement"">Enforcement</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/funding-opportunities"">Funding Opportunities</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/international-affairs/international-cooperation-on-key-issues"">International Cooperation</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/permits-and-forms#protected-resources"">Permits &amp; Authorizations</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/rules-and-regulations#protected-resources"">Regulations &amp; Actions</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/offshore-wind-energy"">Offshore Wind Energy</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/science-and-data"">Science &amp; Data</a>
              </li>
                                    
      </li>
  
            
            </ul>
        </li>
      </ul>
  
                         
                                                <li class=""main-menu__item main-menu__item--expanded"">
              <a class=""link link--text main-menu__link"" role=""button"" aria-haspopup=""true"" href="""" >Environment</a>
                             
                  
    <ul class=""main-menu__items main-menu__container--mega main-menu--hidden"" id=""container-4"">
      <li class=""hidden"">
        <ul class=""main-menu--mega main-menu--1"">
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/topic/ecosystems"">Ecosystems</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/ecosystems/u.s.-regional-ecosystems"">U.S. Regional Ecosystems </a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/ecosystems/ecosystem-based-fishery-management"">Management</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/ecosystems/science"">Science</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/topic/habitat-conservation"">Habitat Conservation</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/habitat-conservation/how-we-restore"">Habitat Restoration</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/habitat-conservation/how-we-protect"">Habitat Protection</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/habitat-conservation/types-of-habitat"">Types of Habitat</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/habitat-conservation/habitat-conservation-in-the-regions"">Habitat by Region</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/habitat-conservation/science"">Science</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/consultations/habitat-consultations"">Consultations</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/topic/climate-change"">Climate Change</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/climate-change/understanding-the-impacts"">Understanding the Impacts</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/climate-change/responding-to-change"">Responding to Change</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""https://www.fisheries.noaa.gov/topic/climate-change/climate,-ecosystems,-and-fisheries"">Climate, Ecosystems &amp; Fisheries Initiative</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/climate-change/regional-activities"">Regional Activities</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/offshore-wind-energy"">Offshore Wind Energy</a>
              </li>
                                    
      </li>
  
            
            </ul>
        </li>
      </ul>
  
                         
                                                <li class=""main-menu__item main-menu__item--expanded"">
              <a class=""link link--text main-menu__link"" role=""button"" aria-haspopup=""true"" href="""" >Regions</a>
                             
                  
    <ul class=""main-menu__items main-menu__container--mega main-menu--hidden"" id=""container-5"">
      <li class=""hidden"">
        <ul class=""main-menu--mega main-menu--1"">
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/regions"">Our Regions</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/region/alaska"">Alaska</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/region/new-england-mid-atlantic"">New England/ Mid-Atlantic</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/region/pacific-islands"">Pacific Islands</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/region/southeast"">Southeast</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/region/west-coast"">West Coast</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                     <span class=""main-menu__title"">Contact Us</span>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/contact-directory/regional-offices"">Regional Offices</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/contact-directory/science-centers"">Science Centers</a>
              </li>
                                    
      </li>
  
            
            </ul>
        </li>
      </ul>
  
                         
                                                <li class=""main-menu__item main-menu__item--expanded"">
              <a class=""link link--text main-menu__link"" role=""button"" aria-haspopup=""true"" href="""" >Resources &amp; Services</a>
                             
                  
    <ul class=""main-menu__items main-menu__container--mega main-menu--hidden"" id=""container-6"">
      <li class=""hidden"">
        <ul class=""main-menu--mega main-menu--1"">
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                     <span class=""main-menu__title"">Rules &amp; Regulations</span>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/rules-and-regulations#fisheries"">Fisheries Rules &amp; Regs</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/rules-and-announcements/notices-and-rules"">Fisheries Management Info</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/rules-and-regulations#protected-resources"">Protected Resources Regs &amp; Actions</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                     <span class=""main-menu__title"">Permits</span>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/permits-and-forms"">Fishing &amp; Seafood</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/permits-and-forms#protected-resources"">Protected Resources</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/fishing-and-seafood-permits?fishing_permits%5B1000008636%5D=1000008636"">International &amp; Trade</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/funding-opportunities"">Funding &amp; Financial Services</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/funding-opportunities"">Funding Opportunities</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/funding-opportunities#financial-services"">Financial Services</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/grant/john-h-prescott-marine-mammal-rescue-assistance-grant-program"">Prescott Grants</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/grant/saltonstall-kennedy-grant-program"">Saltonstall-Kennedy Grants</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/grant/coastal-and-marine-habitat-restoration-grants"">Habitat Restoration Grants</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/topic/consultations"">Consultations</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/consultations/habitat-consultations"">Habitat</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/consultations/endangered-species-act-consultations"">Endangered Species</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/consultations/tribal-consultations"">Tribal</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/science-and-data"">Science &amp; Data</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/resources/research"">Research</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/resources/surveys"">Surveys</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/resources/data"">Data</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/resources/maps"">Maps &amp; GIS</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/resources/all-publications"">Publications</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/resources/peer-reviewed-research"">Published Research</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/resources/key-reports"">Key Reports</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/resources/documents"">Documents</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/resources/publication-databases"">Publication Databases</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/resources/outreach-materials"">Outreach Materials</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/topic/laws-policies"">Laws &amp; Policies</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/laws-policies/magnuson-stevens-act"">Magnuson-Stevens Act</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/laws-policies/endangered-species-act"">Endangered Species Act</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/laws-policies/marine-mammal-protection-act"">Marine Mammal Protection Act</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/laws-policies/policies"">Policies</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/outreach-and-education"">Outreach &amp; Education</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/resources/for-educators"">For Educators</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/resources/for-students"">For Students</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/resources/educational-materials"">Educational Materials</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/resources/outreach-materials"">Outreach Materials</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/resources/outreach-events"">Events</a>
              </li>
                                    
      </li>
  
            
            </ul>
        </li>
      </ul>
  
                         
                                                <li class=""main-menu__item main-menu__item--expanded"">
              <a class=""link link--text main-menu__link"" role=""button"" aria-haspopup=""true"" href="""" >About Us</a>
                             
                  
    <ul class=""main-menu__items main-menu__container--mega main-menu--hidden"" id=""container-7"">
      <li class=""hidden"">
        <ul class=""main-menu--mega main-menu--1"">
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                     <span class=""main-menu__title"">NOAA Fisheries</span>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/about-us"">Our Mission</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/about-us#who-we-are"">Who We Are</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/about-us#where-we-work"">Where We Work</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/about-us#our-history"">Our History</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                     <span class=""main-menu__title"">News &amp; Media</span>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/news-and-announcements/news"">News &amp; Announcements</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/news-and-announcements/bulletins"">Bulletins</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/news-and-announcements/multimedia"">Multimedia</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/news-and-announcements/science-blog"">Science Blogs</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/events"">Events</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""https://videos.fisheries.noaa.gov/"">Video Gallery</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/photo-gallery"">Photo Gallery</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/topic/careers-more"">Careers &amp; More</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/careers-more/internships-and-more"">Internships</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/careers-more/volunteering-and-citizen-science"">Volunteering and Citizen Science</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/careers-more/diversity-and-inclusion"">Diversity &amp; Inclusion</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/contact-us"">Contact Us</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/contact-directory/national-program-offices"">National Program Offices</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/contact-directory/regional-offices"">Regional Offices</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/contact-directory/science-centers"">Science Centers</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                      <a class=""link--cta""  href=""/topic/partners"">Our Partners</a>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/partners"">Regional Fishery Management Councils</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/partners/marine-fisheries-advisory-committee"">Marine Fishery Advisory Committee</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/partners/federal-agencies"">Federal Partners</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/partners/state-agencies"">State Partners</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/partners/tribal-governments"">Tribal Governments</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/topic/partners/non-government-organizations"">Non-Government Organizations</a>
              </li>
                                    
      </li>
  
                         
                   
              </ul>
                  <li class=""main-menu__column--mega"" >
     <ul class=""main-menu--mega main-menu--2"">
                     <span class=""main-menu__title"">COVID-19 Information</span>
                <br>
        <div class=""bar"" ></div>
                       
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/national/about-us/noaa-fisheries-coronavirus-covid-19-update"">Fisheries COVID-19 Updates</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/feature-story/us-fishing-and-seafood-industries-saw-broad-declines-last-summer-due-covid-19"">Economic Impacts</a>
              </li>
                                                 
                                                    <li class=""main-menu__item"">
                <a class=""link link--text main-menu__link"" href=""/feature-story/commerce-secretary-announces-allocation-300-million-cares-act-funding"">CARES Act</a>
              </li>
                                    
      </li>
  
            
            </ul>
        </li>
      </ul>
  
            
      </ul>    </nav>
  
    </div>
  </div>

  <div class=""site-header__bottom hidden-lg hidden-md"">
      
  
  <nav class=""mobile-menu"">
    <ul class=""mobile-menu__level"" data-level=""1"" aria-expanded=""false"">
      <form id=""input-search-form_header-search-mobile"" class=""input-search"" action=""https://www.fisheries.noaa.gov/search"" method=""GET"">
  
  <label for=""header-search-mobile"" class=""sr-only"">Search NOAA Fisheries</label>
  <input type=""search"" id=""header-search-mobile"" name=""oq"" placeholder=""Search NOAA Fisheries"">

      <button type=""submit"" class=""fa fa-search input-search__button""><span class=""sr-only"">Search</span></button>
  </form>

                
  <ul class=""mobile-menu__items"">
                  <li class=""mobile-menu__item mobile-menu__item--first mobile-menu__item--expanded"">
          <a class=""mobile-menu__link link link--text"" href=""#"" >Find A Species</a>
      
      
              <ul class=""mobile-menu__level"" data-level=""2"" aria-expanded=""false"">
          <div class=""mobile-menu__back"">
            <a class=""link link--breadcrumb"" href=""#"" role=""button"">Back</a>
          </div>
          <div class=""mobile-menu__explore"">
            <span class=""link link-cta"">Find A Species</span>
          </div>
            
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/find-species"">Find a Species</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/dolphins-porpoises"" title=""Dolphins &amp; Porpoises"">Dolphins &amp; Porpoises</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/fish-sharks"" title=""Fish &amp; Sharks"">Fish &amp; Sharks</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/highly-migratory-species"" title=""Highly Migratory Species"">Highly Migratory Species</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/invertebrates"" title=""Invertebrates"">Invertebrates</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/sea-turtles"" title=""Sea Turtles"">Sea Turtles</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/seals-sea-lions"" title=""Seals &amp; Sea Lions"">Seals &amp; Sea Lions</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/whales"" title=""Whales"">Whales</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                  <span class=""mobile-menu__title"">Protected Species</span>
                   
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/species-directory/threatened-endangered"" title=""All Threatened &amp; Endangered Species"">All Threatened &amp; Endangered Species</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/species-directory/marine-mammals"" title=""Marine Mammals"">Marine Mammals</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                  <span class=""mobile-menu__title"">Species By Region</span>
                   
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/species-directory?oq=&amp;field_species_categories_vocab=All&amp;field_region_vocab=1000001106&amp;items_per_page=25"" title=""Alaska"">Alaska</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/species-directory?oq=&amp;field_species_categories_vocab=All&amp;field_region_vocab=1000001111&amp;items_per_page=25"" title=""New England/Mid-Atlantic"">New England/Mid-Atlantic</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/species-directory?oq=&amp;field_species_categories_vocab=All&amp;field_region_vocab=1000001116&amp;items_per_page=25"" title=""Pacific Islands"">Pacific Islands</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/species-directory?oq=&amp;field_species_categories_vocab=All&amp;field_region_vocab=1000001121&amp;items_per_page=25"" title=""Southeast"">Southeast</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/species-directory?oq=&amp;field_species_categories_vocab=All&amp;field_region_vocab=1000001126&amp;items_per_page=25"" title=""West Coast"">West Coast</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                  <span class=""mobile-menu__title"">Helpful Resources</span>
                   
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/marine-life-viewing-guidelines#guidelines-&amp;-distances"" title=""Marine Life Viewing Guidelines"">Marine Life Viewing Guidelines</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/marine-life-distress"" title=""Marine Life in Distress"">Marine Life in Distress</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/report"" title=""Report a Stranded or Injured Marine Animal"">Report a Stranded or Injured Marine Animal</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/endangered-species-conservation/species-in-the-spotlight"" title=""Species in the Spotlight"">Species in the Spotlight</a>
                      </li>
  
      </ul>
        </li>
  
        </ul>
      
            </li>
                  <li class=""mobile-menu__item mobile-menu__item--first mobile-menu__item--expanded"">
          <a class=""mobile-menu__link link link--text"" href=""#"" >Fishing &amp; Seafood</a>
      
      
              <ul class=""mobile-menu__level"" data-level=""2"" aria-expanded=""false"">
          <div class=""mobile-menu__back"">
            <a class=""link link--breadcrumb"" href=""#"" role=""button"">Back</a>
          </div>
          <div class=""mobile-menu__explore"">
            <span class=""link link-cta"">Fishing &amp; Seafood</span>
          </div>
            
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/topic/sustainable-fisheries"">Sustainable Fisheries</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/bycatch"" title=""Bycatch"">Bycatch</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/national/laws-and-policies/catch-shares"" title=""Catch Shares"">Catch Shares</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/fishery-observers"" title=""Fishery Observers"">Fishery Observers</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/international-affairs/iuu-fishing"" title=""Illegal, Unregulated, Unreported Fishing"">Illegal, Unregulated, Unreported Fishing</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/laws-policies/magnuson-stevens-act"" title=""Magnuson-Stevens Act"">Magnuson-Stevens Act</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/national/science-data/research-surveys"" title=""Research Surveys"">Research Surveys</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/population-assessments"" title=""Population Assessments"">Population Assessments</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/topic/resources-fishing"">Resources for Fishing</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/resources-fishing/commercial-fishing"" title=""Commercial Fishing"">Commercial Fishing</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/resources-fishing/recreational-fishing"" title=""Recreational Fishing"">Recreational Fishing</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/resources-fishing/subsistence-fishing"" title=""Subsistence Fishing"">Subsistence Fishing</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/rules-and-announcements/notices-and-rules"" title=""Fishery Management Info"">Fishery Management Info</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/permits-and-forms"" title=""Permits &amp; Forms"">Permits &amp; Forms</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/rules-and-regulations"" title=""Rules &amp; Regulations by Region"">Rules &amp; Regulations by Region</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/topic/sustainable-seafood"">Sustainable Seafood</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/aquaculture"" title=""Aquaculture"">Aquaculture</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/seafood-commerce-and-trade"" title=""Commerce &amp; Trade"">Commerce &amp; Trade</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/seafood-commerce-and-trade/seafood-inspection"" title=""Seafood Inspection"">Seafood Inspection</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/seafood-commerce-and-trade/trade"" title=""Trade"">Trade</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                  <span class=""mobile-menu__title"">Related Topics</span>
                   
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/atlantic-highly-migratory-species"" title=""Atlantic Highly Migratory Species "">Atlantic Highly Migratory Species </a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/offshore-wind-energy"" title=""Offshore Wind Energy"">Offshore Wind Energy</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/sustainable-fisheries/national-cooperative-research-program"" title=""Cooperative Research"">Cooperative Research</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/enforcement"" title=""Enforcement"">Enforcement</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/funding-opportunities#financial-services"" title=""Financial Services"">Financial Services</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/international-affairs"" title=""International Affairs"">International Affairs</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/science-and-data"" title=""Science &amp; Data"">Science &amp; Data</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/socioeconomics"" title=""Socioeconomics"">Socioeconomics</a>
                      </li>
  
      </ul>
        </li>
  
        </ul>
      
            </li>
                  <li class=""mobile-menu__item mobile-menu__item--first mobile-menu__item--expanded"">
          <a class=""mobile-menu__link link link--text"" href=""#"" >Protecting Marine Life</a>
      
      
              <ul class=""mobile-menu__level"" data-level=""2"" aria-expanded=""false"">
          <div class=""mobile-menu__back"">
            <a class=""link link--breadcrumb"" href=""#"" role=""button"">Back</a>
          </div>
          <div class=""mobile-menu__explore"">
            <span class=""link link-cta"">Protecting Marine Life</span>
          </div>
            
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/topic/endangered-species-conservation"">Endangered Species Conservation</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/consultations/endangered-species-act-consultations"" title=""Consultations"">Consultations</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/national/endangered-species-conservation/critical-habitat"" title=""Critical Habitat"">Critical Habitat</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/laws-policies/endangered-species-act"" title=""Endangered Species Act"">Endangered Species Act</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/national/science-data/research-surveys"" title=""Research Surveys"">Research Surveys</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/population-assessments/endangered-species"" title=""Population Assessments"">Population Assessments</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/national/endangered-species-conservation/recovery-species-under-endangered-species-act"" title=""Species Recovery"">Species Recovery</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/endangered-species-conservation/species-in-the-spotlight"" title=""Species in the Spotlight"">Species in the Spotlight</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/topic/marine-mammal-protection"">Marine Mammal Protection</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/national/marine-life-distress/marine-mammal-health-and-stranding-response-program"" title=""Health &amp; Stranding Response"">Health &amp; Stranding Response</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/laws-policies/marine-mammal-protection-act"" title=""Marine Mammal Protection Act"">Marine Mammal Protection Act</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/national/science-data/research-surveys"" title=""Research Surveys"">Research Surveys</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/population-assessments/marine-mammals"" title=""Population Assessments"">Population Assessments</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/national/marine-mammal-protection/marine-mammal-take-reduction-plans-and-teams"" title=""Take Reduction Plans"">Take Reduction Plans</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/topic/marine-life-distress"">Marine Life in Distress</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/report"" title=""Report a Stranded or Injured Marine Animal"">Report a Stranded or Injured Marine Animal</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/bycatch"" title=""Bycatch"">Bycatch</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/national/science-data/ocean-noise"" title=""Ocean Acoustics/Noise"">Ocean Acoustics/Noise</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/national/marine-mammal-protection/marine-mammal-unusual-mortality-events"" title=""Unusual Mortality Events"">Unusual Mortality Events</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/insight/understanding-vessel-strikes"" title=""Vessel Strikes"">Vessel Strikes</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                  <span class=""mobile-menu__title"">Related Topics</span>
                   
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/marine-life-viewing-guidelines"" title=""Marine Life Viewing Guidelines"">Marine Life Viewing Guidelines</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/enforcement"" title=""Enforcement"">Enforcement</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/funding-opportunities"" title=""Funding Opportunities"">Funding Opportunities</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/international-affairs/international-cooperation-on-key-issues"" title=""International Cooperation"">International Cooperation</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/permits-and-forms#protected-resources"" title=""Permits &amp; Authorizations"">Permits &amp; Authorizations</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/rules-and-regulations#protected-resources"" title=""Regulations &amp; Actions"">Regulations &amp; Actions</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/offshore-wind-energy"" title=""Offshore Wind Energy"">Offshore Wind Energy</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/science-and-data"" title=""Science &amp; Data"">Science &amp; Data</a>
                      </li>
  
      </ul>
        </li>
  
        </ul>
      
            </li>
                  <li class=""mobile-menu__item mobile-menu__item--first mobile-menu__item--expanded"">
          <a class=""mobile-menu__link link link--text"" href=""#"" >Environment</a>
      
      
              <ul class=""mobile-menu__level"" data-level=""2"" aria-expanded=""false"">
          <div class=""mobile-menu__back"">
            <a class=""link link--breadcrumb"" href=""#"" role=""button"">Back</a>
          </div>
          <div class=""mobile-menu__explore"">
            <span class=""link link-cta"">Environment</span>
          </div>
            
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/topic/ecosystems"">Ecosystems</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/ecosystems/u.s.-regional-ecosystems"" title=""U.S. Regional Ecosystems "">U.S. Regional Ecosystems </a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/ecosystems/ecosystem-based-fishery-management"" title=""Management"">Management</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/ecosystems/science"" title=""Science"">Science</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/topic/habitat-conservation"">Habitat Conservation</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/habitat-conservation/how-we-restore"" title=""Habitat Restoration"">Habitat Restoration</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/habitat-conservation/how-we-protect"" title=""Habitat Protection"">Habitat Protection</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/habitat-conservation/types-of-habitat"" title=""Types of Habitat"">Types of Habitat</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/habitat-conservation/habitat-conservation-in-the-regions"" title=""Habitat by Region"">Habitat by Region</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/habitat-conservation/science"" title=""Science"">Science</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/consultations/habitat-consultations"" title=""Consultations"">Consultations</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/topic/climate-change"">Climate Change</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/climate-change/understanding-the-impacts"" title=""Understanding the Impacts"">Understanding the Impacts</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/climate-change/responding-to-change"" title=""Responding to Change"">Responding to Change</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""https://www.fisheries.noaa.gov/topic/climate-change/climate,-ecosystems,-and-fisheries"" title=""Climate, Ecosystems &amp; Fisheries Initiative"">Climate, Ecosystems &amp; Fisheries Initiative</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/climate-change/regional-activities"" title=""Regional Activities"">Regional Activities</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/offshore-wind-energy"" title=""Offshore Wind Energy"">Offshore Wind Energy</a>
                      </li>
  
      </ul>
        </li>
  
        </ul>
      
            </li>
                  <li class=""mobile-menu__item mobile-menu__item--first mobile-menu__item--expanded"">
          <a class=""mobile-menu__link link link--text"" href=""#"" >Regions</a>
      
      
              <ul class=""mobile-menu__level"" data-level=""2"" aria-expanded=""false"">
          <div class=""mobile-menu__back"">
            <a class=""link link--breadcrumb"" href=""#"" role=""button"">Back</a>
          </div>
          <div class=""mobile-menu__explore"">
            <span class=""link link-cta"">Regions</span>
          </div>
            
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/regions"">Our Regions</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/region/alaska"" title=""Alaska"">Alaska</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/region/new-england-mid-atlantic"" title=""New England/ Mid-Atlantic"">New England/ Mid-Atlantic</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/region/pacific-islands"" title=""Pacific Islands"">Pacific Islands</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/region/southeast"" title=""Southeast"">Southeast</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/region/west-coast"" title=""West Coast"">West Coast</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                  <span class=""mobile-menu__title"">Contact Us</span>
                   
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/contact-directory/regional-offices"" title=""Regional Offices"">Regional Offices</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/contact-directory/science-centers"" title=""Science Centers"">Science Centers</a>
                      </li>
  
      </ul>
        </li>
  
        </ul>
      
            </li>
                  <li class=""mobile-menu__item mobile-menu__item--first mobile-menu__item--expanded"">
          <a class=""mobile-menu__link link link--text"" href=""#"" >Resources &amp; Services</a>
      
      
              <ul class=""mobile-menu__level"" data-level=""2"" aria-expanded=""false"">
          <div class=""mobile-menu__back"">
            <a class=""link link--breadcrumb"" href=""#"" role=""button"">Back</a>
          </div>
          <div class=""mobile-menu__explore"">
            <span class=""link link-cta"">Resources &amp; Services</span>
          </div>
            
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                  <span class=""mobile-menu__title"">Rules &amp; Regulations</span>
                   
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/rules-and-regulations#fisheries"" title=""Fisheries Rules &amp; Regs"">Fisheries Rules &amp; Regs</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/rules-and-announcements/notices-and-rules"" title=""Fisheries Management Info"">Fisheries Management Info</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/rules-and-regulations#protected-resources"" title=""Protected Resources Regs &amp; Actions"">Protected Resources Regs &amp; Actions</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                  <span class=""mobile-menu__title"">Permits</span>
                   
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/permits-and-forms"" title=""Fishing &amp; Seafood"">Fishing &amp; Seafood</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/permits-and-forms#protected-resources"" title=""Protected Resources"">Protected Resources</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/fishing-and-seafood-permits?fishing_permits%5B1000008636%5D=1000008636"" title=""International &amp; Trade"">International &amp; Trade</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/funding-opportunities"">Funding &amp; Financial Services</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/funding-opportunities"" title=""Funding Opportunities"">Funding Opportunities</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/funding-opportunities#financial-services"" title=""Financial Services"">Financial Services</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/grant/john-h-prescott-marine-mammal-rescue-assistance-grant-program"" title=""Prescott Grants"">Prescott Grants</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/grant/saltonstall-kennedy-grant-program"" title=""Saltonstall-Kennedy Grants"">Saltonstall-Kennedy Grants</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/grant/coastal-and-marine-habitat-restoration-grants"" title=""Habitat Restoration Grants"">Habitat Restoration Grants</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/topic/consultations"">Consultations</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/consultations/habitat-consultations"" title=""Habitat"">Habitat</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/consultations/endangered-species-act-consultations"" title=""Endangered Species"">Endangered Species</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/consultations/tribal-consultations"" title=""Tribal"">Tribal</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/science-and-data"">Science &amp; Data</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/resources/research"" title=""Research"">Research</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/resources/surveys"" title=""Surveys"">Surveys</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/resources/data"" title=""Data"">Data</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/resources/maps"" title=""Maps &amp; GIS"">Maps &amp; GIS</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/resources/all-publications"">Publications</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/resources/peer-reviewed-research"" title=""Published Research"">Published Research</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/resources/key-reports"" title=""Key Reports"">Key Reports</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/resources/documents"" title=""Documents"">Documents</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/resources/publication-databases"" title=""Publication Databases"">Publication Databases</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/resources/outreach-materials"" title=""Outreach Materials"">Outreach Materials</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/topic/laws-policies"">Laws &amp; Policies</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/laws-policies/magnuson-stevens-act"" title=""Magnuson-Stevens Act"">Magnuson-Stevens Act</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/laws-policies/endangered-species-act"" title=""Endangered Species Act"">Endangered Species Act</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/laws-policies/marine-mammal-protection-act"" title=""Marine Mammal Protection Act"">Marine Mammal Protection Act</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/laws-policies/policies"" title=""Policies"">Policies</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/outreach-and-education"">Outreach &amp; Education</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/resources/for-educators"" title=""For Educators"">For Educators</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/resources/for-students"" title=""For Students"">For Students</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/resources/educational-materials"" title=""Educational Materials"">Educational Materials</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/resources/outreach-materials"" title=""Outreach Materials"">Outreach Materials</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/resources/outreach-events"" title=""Events"">Events</a>
                      </li>
  
      </ul>
        </li>
  
        </ul>
      
            </li>
                  <li class=""mobile-menu__item mobile-menu__item--first mobile-menu__item--expanded"">
          <a class=""mobile-menu__link link link--text"" href=""#"" >About Us</a>
      
      
              <ul class=""mobile-menu__level"" data-level=""2"" aria-expanded=""false"">
          <div class=""mobile-menu__back"">
            <a class=""link link--breadcrumb"" href=""#"" role=""button"">Back</a>
          </div>
          <div class=""mobile-menu__explore"">
            <span class=""link link-cta"">About Us</span>
          </div>
            
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                  <span class=""mobile-menu__title"">NOAA Fisheries</span>
                   
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/about-us"" title=""Our Mission"">Our Mission</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/about-us#who-we-are"" title=""Who We Are"">Who We Are</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/about-us#where-we-work"" title=""Where We Work"">Where We Work</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/about-us#our-history"" title=""Our History"">Our History</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                  <span class=""mobile-menu__title"">News &amp; Media</span>
                   
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/news-and-announcements/news"" title=""News &amp; Announcements"">News &amp; Announcements</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/news-and-announcements/bulletins"" title=""Bulletins"">Bulletins</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/news-and-announcements/multimedia"" title=""Multimedia"">Multimedia</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/news-and-announcements/science-blog"" title=""Science Blogs"">Science Blogs</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/events"" title=""Events"">Events</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""https://videos.fisheries.noaa.gov/"" title=""Video Gallery"">Video Gallery</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/photo-gallery"" title=""Photo Gallery"">Photo Gallery</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/topic/careers-more"">Careers &amp; More</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/careers-more/internships-and-more"" title=""Internships"">Internships</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/careers-more/volunteering-and-citizen-science"" title=""Volunteering and Citizen Science"">Volunteering and Citizen Science</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/careers-more/diversity-and-inclusion"" title=""Diversity &amp; Inclusion"">Diversity &amp; Inclusion</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/contact-us"">Contact Us</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/contact-directory/national-program-offices"" title=""National Program Offices"">National Program Offices</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/contact-directory/regional-offices"" title=""Regional Offices"">Regional Offices</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/contact-directory/science-centers"" title=""Science Centers"">Science Centers</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                    <a class=""link link--cta"" href=""/topic/partners"">Our Partners</a>
                  
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/partners"" title=""Regional Fishery Management Councils"">Regional Fishery Management Councils</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/partners/marine-fisheries-advisory-committee"" title=""Marine Fishery Advisory Committee"">Marine Fishery Advisory Committee</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/partners/federal-agencies"" title=""Federal Partners"">Federal Partners</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/partners/state-agencies"" title=""State Partners"">State Partners</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/partners/tribal-governments"" title=""Tribal Governments"">Tribal Governments</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/topic/partners/non-government-organizations"" title=""Non-Government Organizations"">Non-Government Organizations</a>
                      </li>
  
      </ul>
        </li>
            <ul class=""mobile-menu__level list--arrow"" data-level=""3"" aria-expanded=""false"">
                  <span class=""mobile-menu__title"">COVID-19 Information</span>
                   
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/national/about-us/noaa-fisheries-coronavirus-covid-19-update"" title=""Fisheries COVID-19 Updates"">Fisheries COVID-19 Updates</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/feature-story/us-fishing-and-seafood-industries-saw-broad-declines-last-summer-due-covid-19"" title=""Economic Impacts"">Economic Impacts</a>
                      </li>
                              <li class=""mobile-menu__item mobile-menu__item--second"">
            <a class=""mobile-menu__link link link--text"" href=""/feature-story/commerce-secretary-announces-allocation-300-million-cares-act-funding"" title=""CARES Act"">CARES Act</a>
                      </li>
  
      </ul>
        </li>
  
        </ul>
      
            </li>
      </ul>

          </ul>
  </nav>

  </div>
</header>

  </nav>

  </div>

</header>

  <main class=""main-container container js-quickedit-main-content"" id=""main-content"" tabindex=""-1"">
    <div class=""row"">

            
                  <section class=""col-sm-12"">

                
                
                            <div class=""region region-content"">
    <div data-drupal-messages-fallback class=""hidden""></div>  
      
  

                        

  
  

  




<div class=""resource"">
  <div class=""content-header hr"">
      <nav class=""content-header__breadcrumb""><a href=""/resources""                                                                             class=""link link--breadcrumb"">Resources</a></nav>
  
  <h1 class=""content-header__title"">
<span>Three Alpha Codes for Seafood Import Monitoring Program</span>
</h1>

      <p class=""caption content-header__date"">April 02, 2019</p>
  
      <p class=""content-header__content text--large"">The Seafood Import Monitoring Program, or SIMP, establishes the reporting and recordkeeping requirements needed to prevent illegal, unreported, and unregulated fishing and/or misrepresented seafood from entering U.S. commerce. The three alpha codes list includes the species that require the full set of SIMP records. <br />
</p>
  
  
  <div class=""content-header__footer"">
                                                      <span class=""content-header__label"">Form</span>
                                 
        
        
    <span class=""content-header__divider"">|</span>
          <span class=""content-header__region"">
        
  <div class=""regions "">
                
      <div class=""regions__vocab"">
            
                                                                                          
      
      <span class=""international"">
                  <a class=""link link--region link--region-international"" href=""/topic/international-affairs"">International</a>
              </span>
            
      </div>
      </div>
      </span>
    
    
    
    
      </div>
</div>
  <div class=""row"">
    <main class=""resource__main col-md-8 col-sm-12 modal__target"">
            
            <div class=""wyswyg-edit field field--name-body field--type-text-with-summary field--label-hidden field--item""><p>The three alpha codes list is available for download in PDF and Excel file formats. </p>

<p><a href=""https://media.fisheries.noaa.gov/dam-migration/simp_species_list_including_shrimp_and_abalone_revmay2018.pdf"">Three alpha codes </a>(PDF, 35 pages)</p>

<p><a href=""https://media.fisheries.noaa.gov/2020-04/simp_species_list_including_shrimp_and_abalone_5-2018.xls"">Three alpha codes</a> (Excel Download)</p>

<p> </p></div>
      
      <div class=""last-updated"">
      <div class=""last-updated__message"">
      <p class=""text--small caption last-updated"">Last updated by 
  
  
                  
                            
              
  
  <a class=""office-vocab__item"" href=""/about/office-international-affairs-trade-and-commerce"">Office of International Affairs, Trade, and Commerce</a>

  on 08/13/2019</p>
    </div>
  </div>

                          <a class=""button button--secondary button--tag"" href=""/tags/international-trade"">International Trade</a>
                    <a class=""button button--secondary button--tag"" href=""/tags/foreign-trade"">Foreign Trade </a>
                    <a class=""button button--secondary button--tag"" href=""/tags/seafood-commerce-and-trade"">Seafood Commerce and Trade</a>
                  </main>
    <aside class=""resource__aside col-md-4 col-sm-12"">
        <div class=""info-links"">
    <h2 class=""info-links__title"">More Information</h2>
    <ul class=""list--arrow info-links__links"">
              <li class=""info-links__links-item"">
          <a class=""link link--alt"" href=""https://www.fisheries.noaa.gov/resource/%7Bpath_utils%7D/harmonized-tariff-codes-seafood-import-monitoring-program"">Harmonized Tariff Codes</a>
        </li>
          </ul>
  </div>

    </aside>
  </div>

  </div>



  </div>

              </section>

                </div>
  </main>

      <footer>
        <div class=""region region-footer"">
    <nav aria-label=""block-noaa-components-footer-menu"" id=""block-noaa-components-footer"">
      
  
 
  

        

<a href=""#"" id=""scroll-to-top""><img src=""/themes/custom/noaa_components/images/Scroll-To-Top_Icon@2x.png"" alt=""Scroll to Top Icon""></a>

<div class=""footer"">
  <div class=""footer__top"">
    <div class=""container"">
      <div class=""footer__top-signup"">
        <div class=""footer__top-image"">
          <img src=""/themes/custom/noaa_components/images/Icon_Mail.png"" alt=""Sign Up Mail Button"">
        </div>
        <div class=""footer__top-text"">
          <div class=""footer__top-text--heading"">Sign up for news and announcements</div>
          <div class=""footer_top-text--subtext"">Stay informed of all the latest regional news around NOAA Fisheries</div>
        </div>
        <div class=""footer__top-button"">
            
  <a class=""button button--secondary button--nomargin-bottom"" href=""https://public.govdelivery.com/accounts/USNOAAFISHERIES/subscriber/new"">Sign Up Now!</a>
        </div>
      </div>
    </div>
  </div>

  <div class=""footer__content"">
    <div class=""container"">
      <div class=""row"">
        <div class=""col-md-12"">
          <div class=""footer__main"">
            <div class=""footer__main-content"">
                      <nav class=""footer-menu"">
      <ul class=""footer-menu__items row"">
                              <li class=""footer-menu__item col-lg-3 col-sm-6 col-xs-12"">
                              <a class=""footer-menu__title-link"" href=""/about-us"" title=""NOAA Fisheries"">NOAA Fisheries</a>

                <ul class=""footer-menu__items list--arrow"">
                                      <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""/about-us"" title=""About Us"">About Us</a>
        </li>
                              <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""/topic/laws-policies"" title=""Laws &amp; Policies"">Laws &amp; Policies</a>
        </li>
                              <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""https://www.fishwatch.gov"" title=""FishWatch"">FishWatch</a>
        </li>
                              <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""https://www.noaa.gov"" title=""NOAA"">NOAA</a>
        </li>
                              <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""https://www.commerce.gov"" title=""Department of Commerce"">Department of Commerce</a>
        </li>
                              <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""/site-index"" title=""Site Index"">Site Index</a>
        </li>
            
                </ul>
                          </li>
                                        <li class=""footer-menu__item col-lg-3 col-sm-6 col-xs-12"">
                              <a class=""footer-menu__title-link"" href=""/topic/resources-fishing"" title=""For Fishermen"">For Fishermen</a>

                <ul class=""footer-menu__items list--arrow"">
                                      <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""/rules-and-regulations"" title=""Rules &amp; Regulations"">Rules &amp; Regulations</a>
        </li>
                              <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""/permits-and-forms"" title=""Permits &amp; Forms"">Permits &amp; Forms</a>
        </li>
                              <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""/topic/commercial-fishing"" title=""Commercial Fishing"">Commercial Fishing</a>
        </li>
                              <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""/insight/recreational-fishing"" title=""Recreational Fishing"">Recreational Fishing</a>
        </li>
                              <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""/topic/fishery-observers"" title=""Fishery Observers"">Fishery Observers</a>
        </li>
            
                </ul>
                          </li>
                                        <li class=""footer-menu__item col-lg-3 col-sm-6 col-xs-12"">
                                                <a class=""footer-menu__title-link"" href=""/resources/research"" title=""For Researchers"">For Researchers</a>

                  <ul class=""footer-menu__items list--arrow"">
                                        <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""/resources/peer-reviewed-research"" title=""Published Research"">Published Research</a>
        </li>
                              <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""/science-and-data"" title=""Science &amp; Data"">Science &amp; Data</a>
        </li>
            
                  </ul>
                                          </li>
                                        <li class=""footer-menu__item col-lg-3 col-sm-6 col-xs-12"">
                              <a class=""footer-menu__title-link"" href=""/contact-us"" title=""Contact Us"">Contact Us</a>

                <ul class=""footer-menu__items list--arrow"">
                                      <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""/contact-us"" title=""Contact Us"">Contact Us</a>
        </li>
                              <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""/contact-directory/media-directory"" title=""Media Inquiries"">Media Inquiries</a>
        </li>
                              <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""/national/enforcement/report-violation"" title=""Report a Violation"">Report a Violation</a>
        </li>
                              <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""/report"" title=""Report a Stranded or Injured Marine Animal"">Report a Stranded or Injured Marine Animal</a>
        </li>
                              <li class=""footer-menu__item"">
          <a class=""footer-menu__link"" href=""https://nsd.rdc.noaa.gov/"" title=""NOAA Staff Directory"">NOAA Staff Directory</a>
        </li>
            
                </ul>
                          </li>
                        </ul>
    </nav>
  
            </div>
            <div class=""footer__social"">
              <div class=""footer__social-title"">Follow Us</div>
              <div class=""footer__social-links"">
                                  <div class=""footer__social-links-item"">
                    <a href=""https://twitter.com/@NOAAFisheries"" class=""footer__social-links-link""><span class=""fab fa-twitter""></span><span class=""sr-only"">Twitter</span></a>
                  </div>
                                  <div class=""footer__social-links-item"">
                    <a href=""https://www.facebook.com/NOAAFisheries"" class=""footer__social-links-link""><span class=""fab fa-facebook-f""></span><span class=""sr-only"">Facebook</span></a>
                  </div>
                                  <div class=""footer__social-links-item"">
                    <a href=""https://www.instagram.com/noaafisheries"" class=""footer__social-links-link""><span class=""fab fa-instagram""></span><span class=""sr-only"">Instagram</span></a>
                  </div>
                                  <div class=""footer__social-links-item"">
                    <a href=""https://www.youtube.com/user/usnoaafisheriesgov"" class=""footer__social-links-link""><span class=""fab fa-youtube""></span><span class=""sr-only"">Youtube</span></a>
                  </div>
                              </div>
              <div class=""footer__social-subtitle"">
                <a href=""/site-index"" class =""footer__social-subtitle-link"">Can&#039;t Find What You Need?</a><br>
                <a href=""/video/new-our-website-watch"" class =""footer__social-subtitle-link"">Tour Our Site</a>
              </div>
                <a class=""button button--primary footer__social-button"" href=""https://survey.foresee.com/f/lJQ9iJ20fw"">
        How are we doing? Send us your feedback
  </a>
            </div>
          </div>

          <div class=""footer__bottom"">
            <div class=""footer__logo"">
              <a href=""/""><img src=""/themes/custom/noaa_components/images/noaa_logo_white.png"" class=""footer__logo-image"" alt=""NOAA Logo"" /></a>
              <div class=""footer__logo-text"">Science. Service. Stewardship.</div>
            </div>
            <div class=""footer__links"">
                              <div class=""footer__links-item""><a href=""http://www.noaa.gov/accessibility"" title=""Accessibility"" class=""footer__links-item-link"">Accessibility</a></div>  |  
                              <div class=""footer__links-item""><a href=""/about/office-equal-employment-opportunity"" title=""EEO"" class=""footer__links-item-link"">EEO</a></div>  |  
                              <div class=""footer__links-item""><a href=""https://www.noaa.gov/foia"" title=""FOIA"" class=""footer__links-item-link"">FOIA</a></div>  |  
                              <div class=""footer__links-item""><a href=""https://www.noaa.gov/information-quality"" title=""Information Quality"" class=""footer__links-item-link"">Information Quality</a></div>  |  
                              <div class=""footer__links-item""><a href=""/website-policies-and-disclaimers"" title=""Policies &amp; Disclaimer"" class=""footer__links-item-link"">Policies &amp; Disclaimer</a></div>  |  
                              <div class=""footer__links-item""><a href=""/privacy-policy"" title=""Privacy Policy"" class=""footer__links-item-link"">Privacy Policy</a></div>  |  
                              <div class=""footer__links-item""><a href=""https://www.usa.gov"" title=""USA.gov"" class=""footer__links-item-link"">USA.gov</a></div>
                          </div>
          </div>
      </div>
    </div>
  </div>
</div>

  </nav>

  </div>

    </footer>
  
  </div>

    
    <script type=""application/json"" data-drupal-selector=""drupal-settings-json"">{""path"":{""baseUrl"":""\/"",""scriptPath"":null,""pathPrefix"":"""",""currentPath"":""node\/67571"",""currentPathIsAdmin"":false,""isFront"":false,""currentLanguage"":""en""},""pluralDelimiter"":""\u0003"",""suppressDeprecationErrors"":true,""lazy"":{""lazysizes"":{""lazyClass"":""dam-image"",""loadedClass"":""lazyloaded"",""loadingClass"":""lazyloading"",""preloadClass"":""lazypreload"",""errorClass"":""lazyerror"",""autosizesClass"":""lazyautosizes"",""srcAttr"":""data-src"",""srcsetAttr"":""data-srcset"",""sizesAttr"":""data-sizes"",""minSize"":40,""customMedia"":[],""init"":true,""expFactor"":1.5,""hFac"":0.80000000000000004,""loadMode"":2,""loadHidden"":true,""ricTimeout"":0,""throttleDelay"":125,""plugins"":[]},""placeholderSrc"":"""",""preferNative"":false,""minified"":true,""libraryPath"":""\/themes\/custom\/noaa_components\/scripts\/lazyload""},""data"":{""extlink"":{""extTarget"":false,""extTargetNoOverride"":false,""extNofollow"":false,""extNoreferrer"":false,""extFollowNoOverride"":false,""extClass"":""ext"",""extLabel"":""(link is external)"",""extImgClass"":false,""extSubdomains"":false,""extExclude"":""(\\.gov)"",""extInclude"":"""",""extCssExclude"":"".site-header__top-bar-links--left, .site-header__top-bar-links--right"",""extCssExplicit"":"""",""extAlert"":false,""extAlertText"":""This link will take you to an external web site. We are not responsible for their content."",""mailtoClass"":""0"",""mailtoLabel"":""(link sends email)"",""extUseFontAwesome"":false,""extIconPlacement"":""append"",""extFaLinkClasses"":""fa fa-external-link"",""extFaMailtoClasses"":""fa fa-envelope-o"",""whitelistedDomains"":[""fisheriesmedia.s3.amazonaws.com""]}},""bootstrap"":{""forms_has_error_value_toggle"":1,""modal_animation"":1,""modal_backdrop"":""true"",""modal_focus_input"":1,""modal_keyboard"":1,""modal_select_text"":1,""modal_show"":1,""modal_size"":"""",""popover_enabled"":1,""popover_animation"":1,""popover_auto_close"":1,""popover_container"":""body"",""popover_content"":"""",""popover_delay"":""0"",""popover_html"":0,""popover_placement"":""right"",""popover_selector"":"""",""popover_title"":"""",""popover_trigger"":""click"",""tooltip_enabled"":1,""tooltip_animation"":1,""tooltip_container"":""body"",""tooltip_delay"":""0"",""tooltip_html"":0,""tooltip_placement"":""auto left"",""tooltip_selector"":"""",""tooltip_trigger"":""hover""},""user"":{""uid"":0,""permissionsHash"":""a6d646a5a755204ab163ebc48cee55972cb880a2fecddbd6a28d200a63fb78a7""}}</script>
<script src=""/core/assets/vendor/jquery/jquery.min.js?v=3.6.0""></script>
<script src=""/core/assets/vendor/underscore/underscore-min.js?v=1.13.3""></script>
<script src=""/core/misc/polyfills/element.matches.js?v=9.4.7""></script>
<script src=""/core/misc/polyfills/object.assign.js?v=9.4.7""></script>
<script src=""/core/assets/vendor/once/once.min.js?v=1.0.1""></script>
<script src=""/core/assets/vendor/jquery-once/jquery.once.min.js?v=2.2.3""></script>
<script src=""/core/misc/drupalSettingsLoader.js?v=9.4.7""></script>
<script src=""/core/misc/drupal.js?v=9.4.7""></script>
<script src=""/core/misc/drupal.init.js?v=9.4.7""></script>
<script src=""https://static.addtoany.com/menu/page.js"" async></script>
<script src=""/modules/contrib/addtoany/js/addtoany.js?v=9.4.7""></script>
<script src=""/modules/contrib/lazy/js/lazy.js?v=9.4.7""></script>
<script src=""/modules/custom/noaa_extras/js/openwebpagefromquery.js?rmln2u""></script>
<script src=""/themes/contrib/bootstrap/js/drupal.bootstrap.js?rmln2u""></script>
<script src=""/themes/contrib/bootstrap/js/attributes.js?rmln2u""></script>
<script src=""/themes/contrib/bootstrap/js/theme.js?rmln2u""></script>
<script src=""/core/misc/debounce.js?v=9.4.7""></script>
<script src=""/core/misc/jquery.once.bc.js?v=9.4.7""></script>
<script src=""/themes/custom/noaa_components/dest/script.js?rmln2u""></script>
<script src=""/themes/custom/noaa_components/scripts/misc.js?rmln2u""></script>
<script src=""/themes/custom/noaa_components/node_modules/bootstrap-sass/assets/javascripts/bootstrap/affix.js?rmln2u""></script>
<script src=""/themes/custom/noaa_components/node_modules/bootstrap-sass/assets/javascripts/bootstrap/collapse.js?rmln2u""></script>
<script src=""/themes/custom/noaa_components/node_modules/bootstrap-sass/assets/javascripts/bootstrap/modal.js?rmln2u""></script>
<script src=""/themes/custom/noaa_components/node_modules/bootstrap-sass/assets/javascripts/bootstrap/scrollspy.js?rmln2u""></script>
<script src=""/themes/custom/noaa_components/node_modules/bootstrap-sass/assets/javascripts/bootstrap/transition.js?rmln2u""></script>
<script src=""/themes/custom/noaa_components/scripts/outdated-browser/outdated-browser.js?rmln2u""></script>
<script src=""/themes/custom/noaa_components/scripts/siteimprove/siteimprove.js?rmln2u""></script>
<script src=""/modules/contrib/extlink/extlink.js?v=9.4.7""></script>
<script src=""/themes/contrib/bootstrap/js/popover.js?rmln2u""></script>
<script src=""/themes/contrib/bootstrap/js/tooltip.js?rmln2u""></script>

  <script type=""text/javascript"">window.NREUM||(NREUM={});NREUM.info={""beacon"":""bam.nr-data.net"",""licenseKey"":""249ea9cb86"",""applicationID"":""659846010"",""transactionName"":""ZwFWZUBQV0tZVxEIDl5LdVJGWFZWF3AXFBFRCGhfXVVcZHtbCxUTXwhYVEBtd1dcUTMIBEcnW19GQ1ZUVFEXTF9GDVFG"",""queueTime"":0,""applicationTime"":401,""atts"":""S0ZVEwhKREU="",""errorBeacon"":""bam.nr-data.net"",""agent"":""""}</script></body>
</html>
";
		#endregion

		[Test]
		public void TestParsePDF()
		{
			var client = new Mock<IHttpClientHelper>();
			client.Setup(x => x.GetWebPageAsync(url)).Returns(Task.FromResult(html));
			var fileDownloaderWrapper = new FileDownloaderWrapper();
			var parser = new USSIMDataPopulator(fileDownloaderWrapper, "TEST", _xlsFilePath, _zipFilePath, "any", _pdfHtmlPath, client.Object);
			var pdfLink = parser.GetSpecifiedLink(url, "Three alpha codes", ".pdf");
			Assert.IsFalse(string.IsNullOrEmpty(pdfLink));
			Assert.IsTrue(pdfLink.Contains(".pdf"));
		}

		[Test]
		public void TestParse()
		{
			var client = new Mock<IHttpClientHelper>();
			client.Setup(x => x.GetWebPageAsync(url)).Returns(Task.FromResult(html));
			var fileDownloaderMock = new Mock<IFileDownloaderWrapper>();
			fileDownloaderMock.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
			var parser = new USSIMDataPopulator(fileDownloaderMock.Object, _pdfFilePath, _xlsFilePath, _zipFilePath, "any", url, client.Object);
			parser.Parse(_xmlDumpPath);
			var xmlResult = new XmlDocument();
			xmlResult.Load(_xmlDumpPath);

			Assert.NotNull(xmlResult);
			var cusCodeListFromXML = xmlResult.GetElementsByTagName(nameof(RefCusCodeList));
			Assert.Greater(cusCodeListFromXML.Count, 0);
			var firstRecordFromPDF = cusCodeListFromXML[0];
			Assert.AreEqual("YCL", firstRecordFromPDF[nameof(RefCusCodeList.ZZD_Code)].InnerText);
			Assert.AreEqual("Cycleptus elongatus/Blue sucker", firstRecordFromPDF[nameof(RefCusCodeList.ZZD_Description)].InnerText);
		}

		[Test]
		public void TestParseThrowExWhenDownloadPDFFail()
		{
			var client = new Mock<IHttpClientHelper>();
			client.Setup(x => x.GetWebPageAsync(url)).Returns(Task.FromResult(html));
			var fileDownloaderMock = new Mock<IFileDownloaderWrapper>();
			fileDownloaderMock.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
			
			var parser = new USSIMDataPopulator(fileDownloaderMock.Object, _pdfFilePath, _xlsFilePath, _zipFilePath, "any", url, client.Object);
			Assert.Throws<InvalidOperationException>(() => parser.Parse(_xmlDumpPath));
		}

		[Test]
		public void TestGetDate()
		{
			var dateString = "ASFIS_sp_Mar_2017.txt";
			var date = USSIMDataPopulator.GetDate(dateString);
			Assert.AreEqual(date, new DateTime(2017, 3, 1));

			dateString = "ASFIS_sp_2017.txt";
			date = USSIMDataPopulator.GetDate(dateString);
			Assert.AreEqual(date, new DateTime(2017, 1, 1));
		}

		[Test]
		public void TestGetPublishedDateAndSaveXLSFile()
		{
			var client = new Mock<IHttpClientHelper>();
			client.Setup(x => x.GetWebPageAsync(url)).Returns(Task.FromResult(html));
			var fileDownloaderMock = new Mock<IFileDownloaderWrapper>();
			fileDownloaderMock.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
			
			var paser = new USSIMDataPopulator(fileDownloaderMock.Object, "any", "any", _zipFilePath, "any", "any", client.Object);
			var date = paser.GetPublishedDateAndSaveExtractFile("any", _zipFilePath, _xlsFilePath);
			Assert.AreEqual(date, new DateTime(2017, 3, 1));
		}

		[SetUp]
		public void SetUp()
		{
			_binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_pdfFilePath = Path.Combine(_binPath, "TestFiles", "Download1b888426-c871-77bc-496f-e457f38be151\\PDFSnippet.PDF");
			_xlsFilePath = Path.Combine(_binPath, "TestFiles", "Download290949ab-ebde-2fb5-4678-20c2ca670eaa\\XlsxSnippet.xlsx");
			_xmlDumpPath = Path.Combine(_binPath, "8ab48902-cca1-42af-4253-f523e424e2cd_dump", "test.xml");
			_zipFilePath = Path.Combine(_binPath, "TestFiles", "Downloadbcd5d4a5-4890-f7ac-4e8c-eacd3d163ddd\\TestFiles.zip");
			_pdfHtmlPath = Path.Combine(_binPath, "TestFiles", "DownloadD7177A76-9C34-4892-B1D5-67A3F852C80C\\ThreeAlphaCodes.html");
		}
		string _binPath;
		string _xmlDumpPath;
		string _pdfFilePath;
		string _xlsFilePath;
		string _zipFilePath;
		string _pdfHtmlPath;
	}
}
