import {
	computePosition,
	autoUpdate,
	shift,
	flip,
	size,
	arrow,
	offset,
} from '/_content/WinzorFramework/js/external/floating-ui/dom.js';

export const attachPopup = (popupElement, anchorElement) => {
	if (popupElement === null || anchorElement === null) {
		return {
			unregister: () => {},
		};
	}

	const popupTypeElement = popupElement.querySelector(
		'.balloon, .combobox__dropdown, .zdropform, .dynamic-multiline-textbox'
	) || { className: '' };

	let placement;
	let middleware;

	switch (popupTypeElement.className) {
		case 'balloon': {
			const arrowEl = popupElement.querySelector('.arrow');
			const floatingOffset = Math.sqrt(2 * arrowEl.offsetWidth ** 2) / 2 + 6;

			middleware = [
				offset(floatingOffset),
				size({
					apply({ availableHeight, elements }) {
						if (availableHeight <= elements.floating.clientHeight) {
							popupTypeElement.classList.add('balloon__compact');
						}
					},
				}),
				shift(),
				flip({ fallbackStrategy: 'bestFit' }),
				arrow({ element: arrowEl }),
			];
			placement = avoidOverlapWithCalendarAndOtherPopup(popupElement, anchorElement, floatingOffset);
			break;
		}
		case 'combobox__dropdown': {
			middleware = [
				size({
					apply({ availableHeight, elements }) {
						const flipThreshold = 175;
						const requestedHeight = elements.floating.firstChild.attributes['data-desired-height'].value;
						const maximumHeight = Math.min(requestedHeight, availableHeight);
						const menuHeight = Math.max(flipThreshold, maximumHeight);

						Object.assign(elements.floating.style, {
							maxHeight: `${menuHeight}px`,
						});

						setTimeout(() => {
							const selectedItem = elements.floating.getElementsByClassName(
								'combobox__dropdown-item--selected'
							);
							if (selectedItem && selectedItem.length > 0) {
								selectedItem[0].scrollIntoView(true);
							}
						});
					},
				}),
				flip({
					fallbackStrategy: 'initialPlacement',
				}),
			];
			placement = 'bottom-start';
			break;
		}
		case 'dynamic-multiline-textbox': {
			const textboxOffset = -anchorElement.getBoundingClientRect().height;
			middleware = [offset(textboxOffset)];
			placement = 'bottom-start';
			break;
		}
		default: {
			anchorElement = anchorElement.closest('[data-winzor-control-id]');
			middleware = [
				flip(),
				size({
					apply({ availableWidth, availableHeight, elements }) {
						Object.assign(elements.floating.style, {
							maxWidth: `${availableWidth}px`,
							maxHeight: `${availableHeight}px`,
						});
					},
				}),
			];
			placement = 'bottom-start';
			break;
		}
	}

	const updatePosition = () => {
		computePosition(anchorElement, popupElement, {
			placement,
			middleware,
		}).then(({ x, y, middlewareData, placement }) => {
			let xPosition = x;
			let yPosition = y;
			if (anchorElement.classList?.contains('richtextbox')) {
				const currentSelection = getSelection();
				const selectionPosition = currentSelection.getRangeAt(currentSelection).getBoundingClientRect();
				xPosition = selectionPosition.x;
				yPosition = selectionPosition.y + selectionPosition.height;
			}
			Object.assign(popupElement.style, {
				left: `${xPosition}px`,
				top: `${yPosition}px`,
				visibility: 'visible',
			});

			if (popupTypeElement.classList?.contains('balloon')) {
				updateArrowPosition(anchorElement, popupElement, middlewareData, placement);
			}

			if (popupTypeElement.className === 'dynamic-multiline-textbox') {
				Object.assign(popupElement.parentElement.style, {
					backgroundColor: 'transparent',
				});
				popupTypeElement.querySelector('textarea').focus();
			}
		});
	};

	const updateArrowPosition = (anchorElement, popupElement, middlewareData, placement) => {
		const skewFactor = 10;
		const maxWidthForSkewY = 80;
		const arrowElement = popupElement.querySelector('.arrow');
		const isBalloonFlipped = placement.includes('top');
		const xArrow = middlewareData.arrow.x !== null ? middlewareData.arrow.x : 0;
		let centerOffsetArrow = middlewareData.arrow.centerOffset !== null ? middlewareData.arrow.centerOffset : 0;
		let bordersToDraw;
		let transformFunction;

		arrowElement.style.border = 'none';

		if (anchorElement.getBoundingClientRect().left < window.innerWidth / 2) {
			if (isBalloonFlipped) {
				bordersToDraw = ['border-right', 'border-bottom'];
				centerOffsetArrow = centerOffsetArrow - skewFactor;
			} else {
				bordersToDraw = ['border-left', 'border-top'];
				centerOffsetArrow = centerOffsetArrow + skewFactor;
			}

			if (popupElement.clientWidth < maxWidthForSkewY) {
				transformFunction = 'skew(15deg, 45deg)';
				centerOffsetArrow = 0;
			} else {
				transformFunction = 'skew(0deg, 45deg)';
			}
		} else {
			if (isBalloonFlipped) {
				bordersToDraw = ['border-left', 'border-bottom'];
				centerOffsetArrow = centerOffsetArrow + skewFactor;
			} else {
				bordersToDraw = ['border-right', 'border-top'];
				centerOffsetArrow = centerOffsetArrow - skewFactor;
			}

			if (popupElement.clientWidth < maxWidthForSkewY) {
				transformFunction = 'skew(-15deg, -45deg)';
				centerOffsetArrow = 0;
			} else {
				transformFunction = 'skew(0deg, -45deg)';
			}
		}

		Object.assign(arrowElement.style, {
			left: `${xArrow + centerOffsetArrow}px`,
			top: '',
			right: '',
			bottom: '',
			transform: transformFunction,
			[isBalloonFlipped ? 'bottom' : 'top']: `${-arrowElement.offsetWidth / 2}px`,
			[bordersToDraw[0]]: '1px solid black',
			[bordersToDraw[1]]: '1px solid black',
		});
	};

	const cleanup = autoUpdate(anchorElement, popupElement, updatePosition, {
		animationFrame: true,
	});

	return {
		unregister: () => cleanup(),
	};
};

const avoidOverlapWithCalendarAndOtherPopup = (popupElement, anchorElement, floatingOffset) => {
	const dateInput = anchorElement.querySelector('input[type="date"], input[type="datetime-local"]');
	const hasCalendarOpen = dateInput
		? dateInput.hasAttribute('data-calendar-open')
		: anchorElement.parentElement?.parentElement
				?.querySelector('input[type="date"], input[type="datetime-local"]')
				?.hasAttribute('data-calendar-open');

	const balloonHeight = floatingOffset + popupElement.offsetHeight;

	const viewportHeight = window.innerHeight;
	const anchorRect = anchorElement.getBoundingClientRect();

	const willOverflowTop = anchorRect.top - balloonHeight < 0;
	const willOverflowBottom = anchorRect.bottom + balloonHeight > viewportHeight;

	let placement;

	if (hasCalendarOpen || checkOtherPopupOverlapBelow(popupElement, anchorElement, balloonHeight)) {
		placement = willOverflowTop ? 'bottom' : 'top';
	} else {
		placement = willOverflowBottom ? 'top' : 'bottom';
	}
	return placement;
};

const checkOtherPopupOverlapBelow = (popupElement, anchorElement, balloonHeight) => {
	let otherPopup = null;
	document.querySelectorAll('.popup').forEach((popup) => {
		if (popup !== popupElement) {
			otherPopup = popup;
		}
	});

	if (!otherPopup) {
		return false;
	}

	const otherPopupPosition = getElementPagePosition(otherPopup);
	const anchorPosition = getElementPagePosition(anchorElement);
	return otherPopupPosition.y > anchorPosition.y && otherPopupPosition.y < anchorPosition.y + balloonHeight;
};

const getElementPagePosition = (element) => {
	let actualLeft = element.offsetLeft;
	let actualTop = element.offsetTop;
	let current = element.offsetParent;
	while (current !== null) {
		actualLeft += current.offsetLeft;
		actualTop += current.offsetTop + current.clientTop;
		current = current.offsetParent;
	}

	return { x: actualLeft, y: actualTop };
};
