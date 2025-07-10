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

async function writeFile(clientName, functionName, filePath, streamRef) {
	let buffer = await streamRef.arrayBuffer();
	let typedArray = new Uint8Array(buffer);
	await window[clientName][functionName](filePath, Array.from(typedArray));
}
