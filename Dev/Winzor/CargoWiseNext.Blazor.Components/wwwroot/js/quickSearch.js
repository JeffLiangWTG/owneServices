let focusShortcutHandler = undefined;
let selectedResultIndex = -1;
let resultItems = [];

export const registerFocusShortcut = (dotNet) => {
	focusShortcutHandler = (event) => {
		if (event.key === 'f' && event.ctrlKey) {
			event.preventDefault();
			dotNet.invokeMethodAsync('FocusAsync');
		}
	}
	document.addEventListener('keydown', focusShortcutHandler);
}

export const unregisterFocusShortcut = () => {
	if (focusShortcutHandler) {
		document.removeEventListener('keydown', focusShortcutHandler);
		focusShortcutHandler = undefined;
	}
}

export const updateResults = (popoverId) => {
	const list = document.querySelector(`#${popoverId} .cwn-quick-search__result-items`);
	if (!list) {
		return;
	}
	resultItems = Array.from(list.querySelectorAll('.cwn-quick-search__result-item'));
	selectedResultIndex = -1;
}

export const registerPopoverNavigation = () => {
	document.addEventListener('keydown', (event) => {
		if (resultItems.length === 0) {
			return;
		}
		if (event.key === 'ArrowDown') {
			event.preventDefault();
			selectedResultIndex = (selectedResultIndex + 1) % resultItems.length;
			resultItems[selectedResultIndex].focus();
		} else if (event.key === 'ArrowUp') {
			event.preventDefault();
			selectedResultIndex = (selectedResultIndex - 1 + resultItems.length) % resultItems.length;
			resultItems[selectedResultIndex].focus();
		} else if (event.key === 'Enter') {
			if (document.activeElement && document.activeElement.classList.contains('cwn-quick-search__result-item')) {
				event.preventDefault();
				document.activeElement.click();
			}
		}
	});
}
