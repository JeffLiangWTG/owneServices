/**
 * @param {HTMLElement} overlayContainer
 * */
export function forceCursorUpdate(overlayContainer) {
	const document = overlayContainer?.ownerDocument;

	if (!document) {
		return; // // DOM may have been updated causing the overlay to be removed
	}

	// Important! This code is required to address an issue whereby the cursor
	// does not change without mouse movement. Once our overlay div is in place,
	// the correct style interpretation is to set the cursor to 'wait', but this
	// does not seem to occur until either a mouse event, or, according to the
	// following Chromium issue, a layout recalculation.
	//
	// Therefore, the following code is setup to forcefully trigger a layout
	// recalculation. It does so in an idempotent, non-destructive way. You can
	// verify the layout is triggered by inspecting the "Performance" tab of the
	// dev tools: there will be a "Recalculate Style" and "Layout" events
	// triggered during the forceCursorUpdate call. If a WebView2 update fixes
	// the underlying cursor update issue, this function can be removed
	// entirely.
	//
	// Source: https://bugs.chromium.org/p/chromium/issues/detail?id=26723#c126

	// 1. Add a new element so there is no cached style information
	const d = document.createElement('div');
	document.body.appendChild(d);
	// 2. Query the new element. This triggers the layout recalaulation
	void d.offsetHeight;
	// 3. Clean up
	d.remove();
}
