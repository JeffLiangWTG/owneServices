export const setSelection = (input, start, end) => {
	if (input && input.setSelectionRange && input.getAttribute('data-is-typing') !== 'true') {
		input.setSelectionRange(start, end);
	}
};

export const setSelectedText = (input, text, start, end) => {
	if (input && input.setRangeText) {
		input.setRangeText(text, start, end, 'end');
	}
};

export const setTextContent = (input, text) => {
	if (input) {
		input.value = text;
	}
};

// To reduce client calls and improve timing reliability, both text and selection are set within a single call.
// The use of two consecutive requestAnimationFrame callbacks ensures the DOM is fully rendered before applying the selection,
// preventing issues where the selection might not be correctly set. Additionally, setting data-is-typing=false ensures that
// the selection can be safely adjusted even if the user is currently typing.
export const setTextAndSelection = (input, text, start, end) => {
	if (!input) {
		return;
	}
	setTextContent(input, text);
	input.setAttribute('data-is-typing', 'false');
	requestAnimationFrame(() => {
		requestAnimationFrame(() => {
			setSelection(input, start, end);
		});
	});
};

export const scrollToCaret = (input) => {
	if (!input) {
		return;
	}

	switch (input.nodeName) {
		case 'INPUT': {
			if (
				input.selectionStart === input.selectionEnd &&
				(input.selectionStart === 0 || input.selectionStart === input.value.length)
			) {
				input.scrollLeft = (input.selectionStart / input.value.length) * input.scrollWidth;
				return;
			}

			const style = getComputedStyle(input);
			const contentWidth =
				parseFloat(style.width) -
				parseFloat(style.paddingLeft) -
				parseFloat(style.paddingRight) -
				parseFloat(style.borderLeftWidth) -
				parseFloat(style.borderRightWidth);
			const context = document.createElement('canvas').getContext('2d');
			// different fonts have different character width
			context.font = style.font;

			const start = context.measureText(input.value.slice(0, input.selectionStart)).width;
			// scroll closer to the middle rather than exactly on edge
			const buffer = contentWidth / 4;

			if (start < input.scrollLeft) {
				input.scrollLeft = start - buffer;
			} else if (start > input.scrollLeft + contentWidth) {
				input.scrollLeft = start + contentWidth - buffer;
			}
			break;
		}
		case 'TEXTAREA': {
			if (
				input.selectionStart === input.selectionEnd &&
				(input.selectionStart === 0 || input.selectionStart === input.value.length)
			) {
				input.scrollTop = (input.selectionStart / input.value.length) * input.scrollHeight;
				return;
			}
			// scroll to anywhere in the middle of textarea is not supported yet
			break;
		}
	}
};

export const selectAll = (input) => {
	if (input) {
		input.setSelectionRange(0, input.value.length);
	}
};

export const focus = (input) => {
	if (input) {
		input.focus();
	}
};

export const getSelection = (input) => {
	if (input) {
		const start = input.selectionStart || 0;
		const end = input.selectionEnd || 0;
		return [start, end];
	}
};

export const raiseWarning = (input, capsKeyPressed) => {
	if (input) {
		const tooltip = input.nextElementSibling.cloneNode(true);

		tooltipDetails(tooltip);

		document.querySelector('.form').appendChild(tooltip);

		const rect = input.getBoundingClientRect();

		tooltip.style.top = `${rect.y + rect.height + 8}px`;
		tooltip.style.left = `${rect.x}px`;

		if (
			tooltip.getBoundingClientRect().right > window.innerWidth ||
			tooltip.getBoundingClientRect().bottom > window.innerHeight
		) {
			resizeTooltip(tooltip, rect);
		}

		tooltip.style.visibility = 'visible';

		setTimeout(function () {
			tooltip.remove();
		}, 3000);
	}

	function tooltipDetails(tooltip) {
		const notification = tooltip.querySelector('.balloon-tooltip__notification');
		const caption = tooltip.querySelector('.balloon-tooltip__caption');
		const description = tooltip.querySelector('.balloon-tooltip');
		const tooltipDesc = document.createElement('p');
		tooltipDesc.classList.add('balloon-tooltip__description');
		if (capsKeyPressed) {
			notification.classList.remove('notification--error');
			notification.classList.add('notification--warning');

			caption.textContent = 'Caps Lock is On';
			tooltipDesc.textContent =
				'Having Caps Lock on may cause you to enter your password incorrectly.\n\nYou should press Caps Lock to turn it off before entering your password.';
		} else {
			notification.classList.remove('notification--warning');
			notification.classList.add('notification--error');

			caption.textContent = 'Not Allowed';
			tooltipDesc.textContent = 'You cannot copy text from a password field.';
		}
		description.appendChild(tooltipDesc);
	}

	function resizeTooltip(tooltip) {
		const description = tooltip.querySelector('.balloon-tooltip');
		const tooltipDesc = tooltip.querySelector('.balloon-tooltip__description');
		description.classList.add('balloon-tooltip--compact');
		if (capsKeyPressed) {
			tooltipDesc.textContent = 'Having Caps Lock on may cause you to enter your password incorrectly.';
		} else {
			tooltipDesc.textContent = 'You cannot copy text from a password field.';
		}
	}
};

export const validateTextOnKeyPress = (input, matchExpression, replacementChars) => {
	if (!input || !(input instanceof HTMLElement)) {
		return; // DOM may have been updated causing the element to be removed
	}
	input.addEventListener('keypress', (event) => {
		if (replacementChars && Object.keys(replacementChars).length > 0 && event.key in replacementChars) {
			event.preventDefault();

			const selectionStart = input.selectionStart;
			const text = input.value;

			const before = text.substring(0, selectionStart);
			const after = text.substring(input.selectionEnd, text.length);
			const charInserted = replacementChars[event.key];

			input.value = before + charInserted + after;
			input.selectionStart = input.selectionEnd = selectionStart + 1;

			const inputEvent = new Event('input', {
				bubbles: true,
				cancelable: true,
			});
			input.dispatchEvent(inputEvent);
		}

		if (matchExpression !== '') {
			const isMatch = event.key.match(matchExpression);
			const selectionLength = input.selectionEnd - input.selectionStart;
			if (isMatch && selectionLength === 0) {
				event.preventDefault();
				event.stopPropagation();
			} else if (isMatch && selectionLength > 0) {
				event.preventDefault();
				const text = input.value;
				const before = text.substring(0, input.selectionStart);
				const after = text.substring(input.selectionEnd, text.length);
				input.value = before + after;
				input.selectionStart = input.selectionEnd = before.length;

				const inputEvent = new Event('input', {
					bubbles: true,
					cancelable: true,
				});
				input.dispatchEvent(inputEvent);
			}
		}
	});
};
