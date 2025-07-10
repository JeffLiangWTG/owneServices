export const scrollToId = (selectedElement) => {
	if (!selectedElement) {
		return;
	}

	const rect = selectedElement.getBoundingClientRect();
	const dropdownRect = selectedElement.parentElement.getBoundingClientRect();
	const isVisible = rect.top >= dropdownRect.top && rect.bottom <= dropdownRect.bottom;

	if (!isVisible) {
		selectedElement.scrollIntoView();
	}
}

export const initialize = (selectedCode, dropdownElementReference, inputElementReference, dotnet) => {
	const dropdownContainerElementReference = dropdownElementReference.querySelector('.cwn-dropdownlist-container');
	let lastInputTextLength = 0;
	let items;
	let keys;

	const debounce = (func, time) => {
		let timer;

		return function (...args) {
			if (timer) {
				clearTimeout(timer);
			}

			timer = setTimeout(() => {
				func.apply(this, args);
			}, time);
		};
	}

	const debounceInvoke = debounce((bestMatchParam) => {
		dotnet.invokeMethodAsync('SelectItemAsync', bestMatchParam);
	}, 200);

	const handleFocus = () => {
		inputElementReference.select();
		lastInputTextLength = 0;
	}

	const handleInput = (e) => {
		const inputText = inputElementReference.value.toLowerCase();
		let bestMatch = null;
		let bestMatchLength = 0;
		let selectedIndex = -1;
		let currentMatchIndex = 0;

		keys.forEach(key => {
			const keyText = key.toLowerCase();
			let matchLength = 0;

			for (let i = 0; i < inputText.length && i < keyText.length; i++) {
				if (inputText[i] === keyText[i]) matchLength++;
				else break;
			}

			if (matchLength > bestMatchLength) {
				bestMatchLength = matchLength;
				bestMatch = key;
				selectedIndex = currentMatchIndex;
			}

			currentMatchIndex++;
		});

		if (lastInputTextLength < inputText.length && bestMatch) {
			inputElementReference.value = bestMatch;
			inputElementReference.setSelectionRange(bestMatchLength, inputElementReference.value.length);

			let target = dropdownContainerElementReference.querySelectorAll('.cwn-dropdownlist-content')[selectedIndex];
			target.scrollIntoView();

			debounceInvoke(bestMatch);
		}

		lastInputTextLength = inputText.length;
	};

	const handleKeydown = (e) => {
		if (!Array.isArray(keys) || keys.length === 0) {
			return;
		}

		let lastSelectedIndex = keys.indexOf(inputElementReference.value);

		if (lastSelectedIndex === -1) {
			lastSelectedIndex = 0;
		}

		if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
			const increment = e.key === 'ArrowDown' ? 1 : -1;
			let selectedIndex = lastSelectedIndex + increment;

			selectedIndex = Math.max(0, Math.min(selectedIndex, keys.length - 1));

			if (selectedIndex !== lastSelectedIndex && keys[selectedIndex]) {
				lastSelectedIndex = selectedIndex;
				debounceInvoke(keys[selectedIndex]);
			}
		}
	}

	const handlecompositionstart = (e) => {
		e.preventDefault();
	}

	const observer = new MutationObserver((mutationsList, observer) => {
		let isChildListChanged = false;
		for (const mutation of mutationsList) {
			if (mutation.type === 'childList') {
				isChildListChanged = true;
			}
		}

		if (isChildListChanged) {
			updateItems();
		}
	});

	const updateItems = () => {
		items = Array.from(dropdownContainerElementReference.querySelectorAll('button'));
		keys = items?.map(e => e.querySelector('.cwn-dropdownlist-content-key').textContent) || [];
	}

	dropdownElementReference.addEventListener('keydown', handleKeydown);

	if (inputElementReference) {
		inputElementReference.value = selectedCode;

		inputElementReference.addEventListener('focus', handleFocus);
		inputElementReference.addEventListener('input', handleInput);
		inputElementReference.addEventListener("compositionstart", handlecompositionstart);

		updateItems();
		observer.observe(dropdownContainerElementReference, { childList: true, subtree: true });
	}
};
