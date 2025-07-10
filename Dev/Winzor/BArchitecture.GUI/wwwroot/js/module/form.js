class Form {
	constructor() {}

	debounce(func, time) {
		const delay = time || 100;
		let timer;

		return function (event) {
			if (timer) clearTimeout(timer);
			timer = setTimeout(func, delay, event);
		};
	}

	resizeContent(dotnetform) {
		return async (event) => {
			const width = event.target.innerWidth;
			const height = event.target.innerHeight;

			await dotnetform.invokeMethodAsync('OnBrowserSizeChangedAsync', width, height);
		};
	}

	parseKeyboardEvent(event) {
		this.preventDefaultControlA(event);
		return {
			key: event.key,
			code: event.code,
			location: event.location,
			repeat: event.repeat,
			ctrlKey: event.ctrlKey,
			shiftKey: event.shiftKey,
			altKey: event.altKey,
			metaKey: event.metaKey,
			type: event.type,
			capsLockKey: event.getModifierState('CapsLock'),
		};
	}

	preventDefaultControlA(e) {
		if (e.ctrlKey && e.key.toLowerCase() === 'a') {
			e.preventDefault();

			switch (e.target?.nodeName) {
				case 'INPUT':
				case 'TEXTAREA': {
					window.getSelection().removeAllRanges();
					e.target.select();
					break;
				}
				case 'DIV': {
					const readDiv = e.target.querySelector('div > div[data-read-only].richtextbox__data');
					if (readDiv) {
						const range = document.createRange();
						range.selectNodeContents(readDiv);
						window.getSelection().removeAllRanges();
						window.getSelection().addRange(range);
					}
					break;
				}
			}
		}
	}

	async resizeWindowWhenNoMainMenuStrip(dotnetform, height, width) {
		if (height > window.innerHeight || width > window.innerWidth) {
			await dotnetform.invokeMethodAsync('OnBrowserSizeChangedAsync', window.innerWidth, window.innerHeight);
		}
	}

	initialize(dotnetform) {
		this.isZoomAttempted = false;
		this.dotnetform = dotnetform;

		window.addEventListener('resize', this.debounce(this.resizeContent(dotnetform)), true);
		document.addEventListener('visibilitychange', function () {
			if (document.hidden) {
				dotnetform.invokeMethodAsync('OnWindowStateChangeAsync', 'Minimized');
			} else {
				dotnetform.invokeMethodAsync('OnWindowStateChangeAsync', 'Normal');
			}
		});

		const browserMoveIntervalTime = 250;
		let windowScreenX = window.screenX;
		let windowScreenY = window.screenY;
		const moveEvent = new Event('browsermove');
		setInterval(function () {
			if (windowScreenX !== window.screenX || windowScreenY !== window.screenY) {
				windowScreenX = window.screenX;
				windowScreenY = window.screenY;
				window.dispatchEvent(moveEvent);
			}
		}, browserMoveIntervalTime);

		window.addEventListener('blur', () => {
			dotnetform.invokeMethodAsync('DeactivateFromClientAsync');
		});

		window.addEventListener('focus', () => {
			dotnetform.invokeMethodAsync('ActivatedFromClientAsync');
		});

		document.addEventListener('keydown', (e) =>
			dotnetform.invokeMethodAsync(
				'OnFormKeyEventAsync',
				this.parseKeyboardEvent(e),
				this.findWinzorControlId(e.target)
			)
		);
		document.addEventListener('keyup', (e) =>
			dotnetform.invokeMethodAsync(
				'OnFormKeyEventAsync',
				this.parseKeyboardEvent(e),
				this.findWinzorControlId(e.target)
			)
		);
		document.addEventListener('click', () => dotnetform.invokeMethodAsync('OnUserActivity'));

		window.addEventListener('paste', (event) => this.handlePaste(event));

		document.addEventListener('dragstart', (e) => {
			if (e.target.getAttribute && e.target.getAttribute('data-drag-target-hidden') === 'true') {
				setTimeout(function () {
					e.target.style.visibility = 'hidden';
				}, 0);
			}
		});

		document.addEventListener(
			'wheel',
			(e) => {
				if (e.ctrlKey) {
					this.isZoomAttempted = true;
					e.preventDefault();
				}
			},
			{ passive: false }
		);
	}

	dragFileUpload(event) {
		event.preventDefault();
		let files = null;
		if (event.dataTransfer !== null) {
			files = Array.from(event.dataTransfer.files).map((file) => {
				return {
					lastModified: new Date(file.lastModified).toISOString(),
					name: file.name,
					size: file.size,
					contentType: file.type,
					fileStream: DotNet.createJSStreamReference(file),
				};
			});
		}

		this.dotnetform.invokeMethodAsync(
			'OnDropFilesAsync',
			files,
			event.clientX,
			event.clientY,
			this.findWinzorControlId(event.target)
		);
	}

	findWinzorControlId(element) {
		let winzorControlId;
		while (!(winzorControlId = element.getAttribute('data-winzor-control-id')) && element.parentElement) {
			element = element.parentElement;
		}
		return winzorControlId;
	}

	saveShortcut(fileName, data) {
		const urlObj = window.URL || window.webkitURL || window;
		const blob = new Blob([data]);
		const url = urlObj.createObjectURL(blob);
		const link = document.createElementNS('http://www.w3.org/1999/xhtml', 'a');

		link.href = url;
		link.download = fileName;
		document.body.appendChild(link);
		link.click();
		link.remove();
		URL.revokeObjectURL(url);
	}

	changeMouseCursorStyle(clientX, clientY) {
		const dragHeight = 4; // refer to Winform code
		const cursorStyleMap = new Map();

		const onMouseMove = (e) => {
			if (e.buttons !== 1 || e.target.draggable === true) {
				removeMouseEvents();
				return;
			}

			let offsetY = e.clientX - clientX;
			let offsetX = e.clientY - clientY;
			if (Math.abs(offsetY) > dragHeight || Math.abs(offsetX) > dragHeight) {
				const targetWinzorId = this.findWinzorControlId(e.target);
				const cursorStyle = getComputedStyle(e.target, null)['cursor'];
				if (!cursorStyleMap.has(targetWinzorId) && targetWinzorId !== null && cursorStyle !== 'not-allowed') {
					this.dotnetform.invokeMethodAsync(
						'OnChangeCursorAsync',
						this.findWinzorControlId(e.target),
						'not-allowed'
					);
					cursorStyleMap.set(targetWinzorId, cursorStyle);
				}
			}
		};

		const onMouseUp = () => {
			cursorStyleMap.forEach((value, key) => {
				this.dotnetform.invokeMethodAsync('OnChangeCursorAsync', key, value);
			});
			removeMouseEvents();
		};

		const bindMouseEvents = () => {
			document.addEventListener('mousemove', onMouseMove);
			document.addEventListener('mouseup', onMouseUp);
		};

		const removeMouseEvents = () => {
			document.removeEventListener('mousemove', onMouseMove);
			document.removeEventListener('mouseup', onMouseUp);
		};

		bindMouseEvents();
	}

	handlePaste(event) {
		try {
			const data = this.getClipboardData(event);

			if (data === null || (data.text === '' && data.html === '' && data.files.length === 0)) {
				return;
			}

			const pastableControl = this.getPastableControl(event.target);

			if (pastableControl === null) {
				return;
			}

			event.preventDefault();

			pastableControl.dispatchEvent(
				new CustomEvent('winzorpaste', {
					bubbles: true,
					detail: data,
				})
			);
		} catch (error) {
			console.error('Failed when handling paste event: ', error);
		}
	}

	getPastableControl(eventTarget) {
		// The target is a pastable control
		if (eventTarget.getAttribute('data-allow-paste') === 'true') {
			return eventTarget;
		}

		// The target is a descendant of a pastable control
		const pastableControl = eventTarget.closest('[data-allow-paste="true"]');

		if (pastableControl) {
			return pastableControl;
		}

		// The target is an ancestor of a pastable control
		return eventTarget.querySelector('[data-allow-paste="true"]');
	}

	getClipboardData(event) {
		if (event === null || event.clipboardData === null) {
			return null;
		}

		const data = { text: '', html: '', files: [] };
		const clipboardData = event.clipboardData;

		// Read text from clipboard
		data.text = clipboardData.getData('text/plain');

		// Read HTML from clipboard
		data.html = clipboardData.getData('text/html');

		data.files = Array.from(clipboardData.files).map((file) => {
			return {
				lastModified: new Date(file.lastModified).toISOString(),
				name: file.name,
				size: file.size,
				contentType: file.type,
				fileStream: DotNet.createJSStreamReference(file),
			};
		});

		return data;
	}

	async createNotification(title, body) {
		if (Notification.permission !== 'granted') {
			await Notification.requestPermission();
		}
		new Notification(title, { body: body });
	}
}

export let form = new Form();
if (!window.form) {
	window.form = form;
}
