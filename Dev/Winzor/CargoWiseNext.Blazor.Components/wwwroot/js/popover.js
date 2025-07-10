export const show = (id) => {
	const popover = document.getElementById(id);
	popover.classList.add("cwn-popover--open");
}

export const hide = (id) => {
	const popover = document.getElementById(id);
	popover.classList.remove("cwn-popover--open");
}

export const toggle = (id) => {
	const popover = document.getElementById(id);
	if (!popover) {
		return;
	}
	popover.classList.toggle("cwn-popover--open");
}

export const init = (id) => {
	const popover = document.getElementById(id);
	const observer = new MutationObserver((mutations) => {
		mutations.forEach(mutation => {
			if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
				if (popover.classList.contains("cwn-popover--open")) {
					const overlays = document.getElementsByClassName("cwn-overlay");
					if (overlays.length == 0) {
						const overlay = document.createElement("div");
						overlay.className = "cwn-overlay";
						document.body.appendChild(overlay);
						overlay.addEventListener('click', () => {
							document.body.removeChild(overlay);
							hide(id);
						});
					}
				} else {
					const overlays = document.getElementsByClassName("cwn-overlay");
					while (overlays.length > 0) {
						overlays[0].remove();
					}
				}
			}
		});
	});

	const config = {
		attributes: true,
		attributeOldValue: true,
		attributeFilter: ['class']
	};

	observer.observe(popover, config);
}
