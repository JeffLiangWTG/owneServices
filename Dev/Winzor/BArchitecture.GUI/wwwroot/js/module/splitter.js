export const moveSplitter = (splitter, isHorizontal, minSizeBefore, minSizeAfter) => {
	let splitterInc = 1;
	let hasGuide = false;
	let dragging = false;
	let mouseStartPos = {
		x: 0,
		y: 0,
	};

	let splitterStartPos = {
		top: 0,
		left: 0,
	};

	let parentBounds = null;
	let splitterGuideBounds = null;
	let guide = null;
	let guideRect = 0;

	if (!(splitter instanceof HTMLElement)) {
		return;
	}

	const onMouseDown = (e) => {
		e.stopImmediatePropagation();
		splitter.classList.add('splitter--drag');
		if (!parentBounds) {
			parentBounds = splitter.parentElement.getBoundingClientRect();
			splitterGuideBounds = {
				x: {
					min: parentBounds.left + minSizeBefore,
					max: parentBounds.right - minSizeAfter,
				},
				y: {
					min: parentBounds.top + minSizeBefore,
					max: parentBounds.bottom - minSizeAfter,
				},
			};
		}

		mouseStartPos.x = e.clientX;
		mouseStartPos.y = e.clientY;
		splitterStartPos = {
			top: splitter.offsetTop,
			left: splitter.offsetLeft,
		};
		if (!hasGuide) {
			guide = createGuide(splitter);
			guideRect = guide.getBoundingClientRect();
			hasGuide = true;
		}
		dragging = true;
		splitter.addEventListener('keydown', onKeyDown);
		splitter.addEventListener('keyup', onKeyUp);
	};

	const onMouseMove = (e) => {
		if (!dragging) return;
		const delta = {
			x: clamp(e.clientX, splitterGuideBounds.x.min, splitterGuideBounds.x.max) - mouseStartPos.x,
			y: clamp(e.clientY, splitterGuideBounds.y.min, splitterGuideBounds.y.max) - mouseStartPos.y,
		};

		if (isHorizontal) {
			const newLeft = clamp(
				splitterStartPos.left + delta.x,
				minSizeBefore,
				parentBounds.width - minSizeAfter - guideRect.width
			);
			guide.style.left = newLeft + 'px';
		} else {
			const newTop = clamp(
				splitterStartPos.top + delta.y,
				minSizeBefore,
				parentBounds.height - minSizeAfter - guideRect.height
			);
			guide.style.top = newTop + 'px';
		}
	};

	const onMouseUp = (e) => {
		e.stopPropagation();

		if (!dragging) return;
		dragging = false;
		removeGuide(guide);
		splitter.classList.remove('splitter--drag');
		hasGuide = false;

		const delta = {
			x: e.clientX - mouseStartPos.x,
			y: e.clientY - mouseStartPos.y,
		};

		splitter.dispatchEvent(
			new CustomEvent('splittermoved', {
				bubbles: true,
				detail: { xOffset: delta.x, yOffset: delta.y },
			})
		);
	};

	const onKeyDown = (e) => {
		if (!['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown'].includes(e.code)) {
			return;
		}

		if (!hasGuide) {
			guide = createGuide(splitter);
			guideRect = guide.getBoundingClientRect();
			hasGuide = true;
		}

		if (['ArrowDown', 'ArrowRight'].includes(e.code)) {
			splitterInc = 1;
		} else {
			splitterInc = -1;
		}

		if (isHorizontal) {
			const newLeft = clamp(
				guide.offsetLeft + splitterInc,
				minSizeBefore,
				parentBounds.width - minSizeAfter - guideRect.width
			);
			guide.style.left = newLeft + 'px';
		} else {
			const newTop = clamp(
				guide.offsetTop + splitterInc,
				minSizeBefore,
				parentBounds.height - minSizeAfter - guideRect.height
			);
			guide.style.top = newTop + 'px';
		}
	};

	const onKeyUp = (e) => {
		if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].indexOf(e.code) === -1) {
			return;
		}

		const delta = {
			x: guide.offsetLeft - splitter.offsetLeft,
			y: guide.offsetTop - splitter.offsetTop,
		};

		removeGuide(guide);
		hasGuide = false;
		splitter.dispatchEvent(
			new CustomEvent('splittermoved', {
				bubbles: true,
				detail: { xOffset: delta.x, yOffset: delta.y },
			})
		);
	};

	const onBlur = () => {
		splitter.removeEventListener('keydown', onKeyDown);
		splitter.removeEventListener('keyup', onKeyUp);
		splitter.removeEventListener('blur', onKeyUp);
	};

	document.addEventListener('mousemove', onMouseMove);
	splitter.addEventListener('mousedown', onMouseDown);
	document.addEventListener('mouseup', onMouseUp);
	splitter.addEventListener('blur', onBlur);
};

const createGuide = (splitter) => {
	const guide = splitter.parentElement.appendChild(splitter.cloneNode(true));
	guide.style.zIndex = 9999;
	guide.classList.add('splitter__guide');
	// Block iframe pointer events to allow guide to be dragged over iframes
	const frames = document.querySelectorAll('iframe');
	for (let i = 0; i < frames.length; i++) {
		frames[i].classList.add('allowsplitterdragover');
	}
	// Block other splitter pointer events to allow guide to be dragged over iframes
	const splitters = document.querySelectorAll('.splitter');
	for (let i = 0; i < splitters.length; i++) {
		splitters[i].classList.add('allowsplitterdragover');
	}

	return guide;
};

const removeGuide = (guide) => {
	guide.remove();
	// Reallow iframe pointer events
	const frames = document.querySelectorAll('iframe');
	for (let i = 0; i < frames.length; i++) {
		frames[i].classList.remove('allowsplitterdragover');
	}
	// Reallow splitter pointer events
	const splitters = document.querySelectorAll('.splitter');
	for (let i = 0; i < splitters.length; i++) {
		splitters[i].classList.remove('allowsplitterdragover');
	}
};

const clamp = (value, minValue, maxValue) => (value <= minValue ? minValue : value >= maxValue ? maxValue : value);
