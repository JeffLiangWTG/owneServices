function checkClientExists(clientName) {
	return window[clientName] != "undefined";
}

function checkFunctionExists(clientName, functionName) {
	return typeof window[clientName]?.[functionName] === "function";
}

function setSubMenuItem(submenuItemJson) {
	chrome.webview.hostObjects.sync.menuManager.SubMenuItemJson = submenuItemJson;
}

function postClientMessage(message) {
	if (typeof window?.chrome?.webview?.postMessage === "function") {
		window.chrome.webview.postMessage(message);
		return 'MessageSent';
	} else {
		return 'ClientAppUnavailable';
	}
}

async function fetchFileData(clientName, functionName, arg) {
	let data = await window[clientName][functionName](arg);
	return new TextEncoder().encode(data);
}
