function setupThrottledScrollEvent(blazor) {
	const throttledScrollEventName = 'throttledscroll';

	blazor.registerCustomEventType(throttledScrollEventName, {
		createEventArgs: event => {
			return {
				scrollLeft: event?.target?.scrollLeft,
				scrollTop: event?.target?.scrollTop,
			}
		}
	});

	document.addEventListener("scroll", (e) => {
		const element = e.target;
		if (!element) {
			return;
		}

		if (element.suppressScrollUpdate) {
			element.suppressScrollUpdate = false;
			return;
		}

		document.querySelectorAll("div.diagramareausercontrol").forEach(control => {
			if (control.scrollTop != element.scrollTop) {
				control.suppressScrollUpdate = true;
				control.scrollTop = element.scrollTop;
			}
		});

		if (!element.throttleScrollEvents) {
			element.throttleScrollEvents = true;
			setTimeout(() => {
				element.dispatchEvent(new CustomEvent(throttledScrollEventName, { bubbles: true, detail: { scrollLeft: element.scrollLeft, scrollTop: element.scrollTop, } }));
				element.throttleScrollEvents = false;
			}, 30);
		}
	}, true);
}

document.addEventListener('keydown', (e) => {
	if (e.ctrlKey && e.key.toLowerCase() === 's') {
		if (document.activeElement?.closest('.diagramareausercontrol')) {
			document.activeElement.blur();
		}
		e.preventDefault();
	}
}, true);

export function afterServerStarted(blazor) {
	setupThrottledScrollEvent(blazor);
}

window.getScrollWidthAndHeight = (element) => {
	if (!element) {
		return [0,0];
	}
	return [element.scrollWidth, element.scrollHeight];
}
