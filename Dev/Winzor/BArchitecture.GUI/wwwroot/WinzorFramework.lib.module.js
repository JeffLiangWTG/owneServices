export function afterServerStarted() {
	Blazor.registerCustomEventType('textboxselectionchange', {
		createEventArgs: (event) => {
			return event.detail;
		},
	});

	Blazor.registerCustomEventType('splittermoved', {
		createEventArgs: (event) => {
			return event.detail;
		},
	});

	Blazor.registerCustomEventType('richtextboxcontextmenu', {
		createEventArgs: (event) => event.detail,
	});

	Blazor.registerCustomEventType('winzorfocusin', {
		browserEventName: 'focusin',
		createEventArgs: (event) => {
			const initiatedFromServer = Boolean(event?.target?.dataset?.serverInitiatedFocus);
			return {
				initiatedFromServer: initiatedFromServer,
			};
		},
	});

	Blazor.registerCustomEventType('winzorfocusout', {
		browserEventName: 'focusout',
		createEventArgs: (event) => {
			if (event.target && event.target.isPasteOrCut) {
				event.target.dispatchEvent(new Event('change'));
				event.target.isPasteOrCut = false;
			}
			const initiatedFromServer = Boolean(event?.relatedTarget?.dataset?.serverInitiatedFocus);
			const winzorTarget =
				event.target instanceof Element ? event.target.closest('[data-winzor-control-id]') : null;
			const relatedWinzorTarget =
				event.relatedTarget instanceof Element ? event.relatedTarget.closest('[data-winzor-control-id]') : null;
			return {
				initiatedFromServer: initiatedFromServer,
				targetWinzorControlId: winzorTarget?.getAttribute('data-winzor-control-id'),
				relatedTargetWinzorControlId: relatedWinzorTarget?.getAttribute('data-winzor-control-id'),
			};
		},
	});

	Blazor.registerCustomEventType('linkclicked', {
		createEventArgs: (event) => {
			return event.detail;
		},
	});

	Blazor.registerCustomEventType('modified', {
		createEventArgs: () => {
			return {};
		},
	});

	Blazor.registerCustomEventType('contentchanged', {
		createEventArgs: (event) => {
			return event.detail;
		},
	});

	Blazor.registerCustomEventType('winzordrop', {
		browserEventName: 'drop',
		createEventArgs: (event) => {
			return {
				clientX: event.clientX,
				clientY: event.clientY,
				controlID: event.dataTransfer.getData('text/winzor-control-id'),
			};
		},
	});

	Blazor.registerCustomEventType('winzorpaste', {
		createEventArgs: (event) => {
			return event.detail;
		},
	});

	Blazor.registerCustomEventType('winzordragend', {
		createEventArgs: (event) => {
			return {
				clientX: event.detail.clientX,
				clientY: event.detail.clientY,
			};
		},
	});

	setupThrottledWheelEvent(Blazor);

	function setupThrottledWheelEvent(blazor) {
		const eventName = 'throttledwheel';

		blazor.registerCustomEventType(eventName, {
			createEventArgs: ({ detail: { deltaY, ctrlKey } }) => ({ deltaY, ctrlKey }),
		});

		document.addEventListener(
			'wheel',
			(event) => {
				const element = event.target;
				if (!element || !event.ctrlKey) {
					return;
				}
				if (!element.throttleWheelEvents) {
					element.throttleWheelEvents = true;
					setTimeout(() => {
						element.dispatchEvent(new CustomEvent(eventName, { bubbles: true, detail: event }));
						element.throttleWheelEvents = false;
					}, 30);
				}
			},
			true
		);
	}

	// Prevents unnecessary focusout events from being sent to the backend when the page loses focus
	document.addEventListener(
		'focusout',
		(e) => {
			if (!document.hasFocus()) {
				e.stopPropagation();
			}
		},
		true
	);

	// Add Event Listener to bind data for the winzordrop event
	document.addEventListener('dragstart', (e) => {
		let winzorDragElement = e.target instanceof Element ? e.target.closest('[data-drag-winzor-id]') : null;

		if (winzorDragElement) {
			let winzorControlID = winzorDragElement.getAttribute('data-drag-winzor-id');
			e.dataTransfer.setData('text/winzor-control-id', winzorControlID);
		}
	});

	// Add Event Listener for textboxes
	let isSelectionChangedByInput = false;
	function dispatchTextBoxSelectionChangeEvent() {
		if (
			(document.activeElement instanceof HTMLInputElement &&
				document.activeElement.getAttribute('type') === 'text') ||
			document.activeElement instanceof HTMLTextAreaElement
		) {
			let selectionStart = document.activeElement.selectionStart;
			let selectionEnd = document.activeElement.selectionEnd;
			document.activeElement.dispatchEvent(
				new CustomEvent('textboxselectionchange', {
					bubbles: true,
					detail: {
						selectionStart: selectionStart,
						selectionEnd: selectionEnd,
					},
				})
			);
			setTimeout(() => (isSelectionChangedByInput = false));
		}
	}

	document.addEventListener('selectionchange', function () {
		if (!isSelectionChangedByInput) {
			dispatchTextBoxSelectionChangeEvent();
		}
	});

	document.addEventListener('input', function () {
		isSelectionChangedByInput = true;
		dispatchTextBoxSelectionChangeEvent();
	});

	document.addEventListener(
		'input',
		(e) => {
			const input = e.target;
			if (
				!input ||
				!(input instanceof HTMLInputElement) ||
				input.getAttribute('type') !== 'text' ||
				!input.hasAttribute('data-winzor-autocomplete') ||
				e.inputType.includes('delete')
			) {
				return;
			}
			const suggestions = document.getElementById(input.getAttribute('data-winzor-control-id'))?.options;
			if (!suggestions) {
				return;
			}
			const autoComplete = Array.from(suggestions)
				.map((s) => s.value)
				.find((s) => s.toUpperCase().startsWith(input.value.toUpperCase()));
			if (!autoComplete) {
				return;
			}
			const selectionStart = e.target.selectionStart;
			e.target.value = autoComplete;
			e.target.setSelectionRange(selectionStart, autoComplete.length);
		},
		true
	);

	Element.prototype.isDataGridEditControl = function () {
		// we don't want to check too many levels up as tryFocusFromServer could be called on any element and called a lot
		let checkDepth = 3;
		let p = this.parentElement;
		while (p && checkDepth-- > 0) {
			if (p.matches('.datagrid__control--edit')) {
				return true;
			}
			p = p.parentElement;
		}
		return false;
	};

	// Focus from server method
	window.tryFocusFromServer = (element) => {
		if (!element) {
			document.activeElement.blur();
			return false;
		}

		if (element === document.activeElement) {
			return true;
		}

		element.dataset.serverInitiatedFocus = true;
		if (element.isDataGridEditControl()) {
			element.focus({ preventScroll: true });
		} else {
			element.focus();
		}

		delete element.dataset.serverInitiatedFocus;

		if (element !== document.activeElement) {
			document.activeElement.blur();
			return false;
		}

		return true;
	};

	window.scrollElementTo = (element, x, y, animated) => {
		if (element?.scrollTo) {
			const behavior = animated ? 'smooth' : 'instant';
			element.scrollTo(x, y, behavior);
		}
	};

	// ScrollIntoViewIfNeeded from server method
	window.scrollIntoViewIfNeeded = (element) => {
		if (element && typeof element.scrollIntoViewIfNeeded === 'function') {
			element.scrollIntoViewIfNeeded();
		} else if (element && typeof element.scrollIntoView === 'function') {
			element.scrollIntoView();
		}
	};

	window.elementEventHandlerTokens = {
		none: 0,
		textBoxDisableHomeEndKeyWhenSelectAll: 1 << 0,
		textBoxStartTyping: 1 << 1,
		gridUpdateTextAreaHeightWhenFocusin: 1 << 2,
	};
	const maxHandlerToken = 1 << 2;

	const keyDownEventHandlers = {
		[window.elementEventHandlerTokens.textBoxDisableHomeEndKeyWhenSelectAll]: disableHomeEndKeyWhenSelectAll,
	};
	const keyPressEventHandlers = {
		[window.elementEventHandlerTokens.textBoxStartTyping]: startTyping,
	};

	const focusinEventHandlers = {
		[window.elementEventHandlerTokens.gridUpdateTextAreaHeightWhenFocusin]: updateTextAreaHeightWhenFocusin,
	};

	document.addEventListener(
		'keydown',
		(e) => {
			handleEvent(e, keyDownEventHandlers);
		},
		true
	);

	document.addEventListener(
		'keydown',
		(e) => {
			handleArrowKey(e);
		},
		false
	);

	document.addEventListener(
		'keypress',
		(e) => {
			handleEvent(e, keyPressEventHandlers);
		},
		true
	);
	document.addEventListener(
		'focusin',
		(e) => {
			handleEvent(e, focusinEventHandlers);
		},
		true
	);

	function handleEvent(e, eventHandlers) {
		let handlers = e.target.dataset.clientSideEventHandlers;
		if (handlers !== null) {
			handlers = parseInt(handlers);
			for (let i = 1; i <= maxHandlerToken; i <<= 1) {
				if ((handlers & i) !== 0 && eventHandlers[i]) {
					eventHandlers[i](e);
				}
			}
		}
	}

	function startTyping(e) {
		const textBox = e.target;
		textBox.setAttribute('data-is-typing', 'true');
		clearTimeout(textBox.typingTimer);
		textBox.typingTimer = setTimeout(() => textBox.setAttribute('data-is-typing', 'false'), 1000);
	}

	/**
	 * Disable Home|End KeyDown for DataGrid Navigation that happens in back-end.
	 */
	function disableHomeEndKeyWhenSelectAll(event) {
		const start = event.target.selectionStart || 0;
		const end = event.target.selectionEnd || 0;
		const isSelectAll = end - start === event.target.value.length;

		switch (event.key) {
			case 'Home':
			case 'End':
				if (isSelectAll) {
					event.preventDefault();
				}
				break;
		}
	}

	function handleArrowKey(e) {
		const grid = e.target.closest('.datagrid');
		if (!grid || !(grid instanceof HTMLElement)) return;

		const arrowKeys = new Set(['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight']);
		const isTextbox = e.target.classList.contains('textbox');

		if (!arrowKeys.has(e.code)) return;

		if (isTextbox) {
			const textbox = e.target;
			const { selectionStart, selectionEnd, value } = textbox;

			if (selectionEnd > selectionStart) {
				if (
					(e.code === 'ArrowRight' && selectionEnd === value.length) ||
					(e.code === 'ArrowLeft' && selectionStart === 0 && selectionEnd === value.length) ||
					e.code === 'ArrowUp' ||
					e.code === 'ArrowDown'
				) {
					e.preventDefault();
					return;
				}
			}

			if (selectionEnd === selectionStart) {
				if (
					(selectionEnd === value.length &&
						(e.code === 'ArrowRight' || e.code === 'ArrowUp' || e.code === 'ArrowDown')) ||
					(selectionEnd === 0 && e.code === 'ArrowLeft') ||
					e.code === 'ArrowUp' ||
					e.code === 'ArrowDown'
				) {
					e.preventDefault();
					return;
				}
			}

			e.stopPropagation();
		} else {
			e.preventDefault();
		}
	}

	function updateTextAreaHeightWhenFocusin(e) {
		const grid = e.target.closest('.datagrid');
		if (!grid) return;

		const expandArrow = grid.querySelector('.textbox--dynamic-expand-arrow');
		if (e.target.nodeName === 'TEXTAREA' && !expandArrow) {
			e.stopPropagation();
			updateTextAreaHeight(grid);
		}
	}

	window.fireWinzorDragEnd = (event) => {
		let dragEndEvent = new CustomEvent('winzordragend', {
			bubbles: true,
			detail: {
				clientX: winzorDragEndX,
				clientY: winzorDragEndY,
			},
		});

		event.target.dispatchEvent(dragEndEvent);
	};

	window.trackWinzorDragEndPosition = (event) => {
		winzorDragEndX = event.x;
		winzorDragEndY = event.y;
	};
}

export const updateTextAreaHeight = (grid) => {
	const header = grid.querySelector('thead');
	const row = grid.querySelector('.datagrid__row--edit');
	const textarea = grid.querySelector('.datagrid__control--edit textarea.textbox--expand');
	const cell = grid.querySelector('.datagrid__cell--edit');
	const textareaMaxHeight = 200;

	if (cell && textarea) {
		let remainingHeight = grid.clientHeight - cell.offsetTop;
		if (textarea.offsetHeight > remainingHeight) {
			textarea.style.height = `${remainingHeight}px`;
		}
	}

	if (row && textarea) {
		const gridRect = grid.getBoundingClientRect();
		const rowRect = row.getBoundingClientRect();
		const headerRect = header.getBoundingClientRect();
		const textareaRect = textarea.getBoundingClientRect();

		let height;
		const visibleAtTop = textareaRect.top + rowRect.height > gridRect.top + headerRect.height;
		if (!visibleAtTop) {
			height = rowRect.height;
		} else {
			const scrollBarHeight = grid.offsetHeight - grid.clientHeight - 2;
			height = Math.round(gridRect.bottom - textareaRect.top - scrollBarHeight);
			height = Math.min(height, textareaMaxHeight);
			height = Math.max(height, rowRect.height);
		}
		textarea.style.height = `${height}px`;
	}
};

let winzorDragEndX = 0;
let winzorDragEndY = 0;
