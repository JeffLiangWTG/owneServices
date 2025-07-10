export const updateRangeInputStep = function (element, arrowKeyIncrement, pageUpDownIncrement) {
	const originalStep = element.step;
	element.addEventListener('keydown', function (event) {
		if (event.key === 'ArrowRight' || event.key === 'ArrowLeft') {
			// Adjust increment for arrow keys
			element.step = arrowKeyIncrement;
		} else if (event.key === 'PageUp' || event.key === 'PageDown') {
			// Adjust increment for Page Up/Down keys
			element.step = pageUpDownIncrement;
		}
	});
	element.addEventListener('mousedown', function () {
		element.step = originalStep;
	});
	element.addEventListener('click', function () {
		element.step = originalStep;
	});
};

export const updateTrackBarColorOnDrag = function (element) {
	let s = document.createElement('style');
	let isMouseDown = false;
	document.head.appendChild(s);

	element.addEventListener('mousedown', () => {
		isMouseDown = true;
	});

	element.addEventListener('mouseup', () => {
		isMouseDown = false;
		s.textContent = `.trackbar__input::-webkit-slider-thumb:hover:active{background-image: ${window
			.getComputedStyle(element)
			.getPropertyValue('--thumb-hover-background-image')}}`;
	});

	element.addEventListener('input', () => {
		if (isMouseDown) {
			s.textContent = `.trackbar__input::-webkit-slider-thumb:hover:active{background-image: ${window
				.getComputedStyle(element)
				.getPropertyValue('--thumb-drag-background-image')}}`;
		}
	});
};
