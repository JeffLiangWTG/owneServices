class Clipboard {
	async setDataObject(_, data) {
		for (const key in data) {
			const value = data[key];
			if (typeof value.stream !== 'function') {
				data[key] = new Blob([value], { type: key });
			}
		}

		if (Object.keys(data).length === 0) {
			await window.cargoWiseClient?.requestUserActivation();
			await navigator.clipboard.writeText('');
			return;
		}

		const cbi = new ClipboardItem(data);

		try {
			// The default navigator.clipboard.write Web API is replaced with the ClientApp implementation
			await navigator.clipboard.write([cbi]);
		} catch (err) {
			if (err instanceof DOMException && err.name === 'NotAllowedError') {
				if (Notification.permission !== 'granted') {
					await Notification.requestPermission();
				}
				new Notification('Content not copied', {
					body: 'Please attempt the copy process again. It typically completes quickly, but occasionally it may take up to 10 seconds, so please keep the window open until it finishes if the problem persists.',
				});
			} else {
				console.error('Copy failed: ', err);
			}
		}
	}

	threshold = 8192; // 8KB limit

	async getDataObject() {
		try {
			await window.cargoWiseClient?.requestUserActivation();
			const clipboardItems = await navigator.clipboard.read();

			const data = {};

			for (const item of clipboardItems) {
				for (const type of item.types) {
					if (type === 'text/plain' || type === 'text/html') {
						let blob = await item.getType(type);
						if (!(blob instanceof Blob)) {
							blob = new Blob([blob], { type: type });
						}
						const text = await blob.text();

						if (text.length < this.threshold) {
							data[type] = { contentString: text, contentStream: null };
						} else {
							data[type] = { contentString: null, contentStream: DotNet.createJSStreamReference(blob) };
						}
					}
				}
			}

			return data;
		} catch (error) {
			console.error('Get Data Object failed: ', error);
			return null;
		}
	}

	async copy(element) {
		element ??= document.activeElement;
		if (this.allowCopy(element)) {
			if (this.isRichTextBox(element)) {
				const rtbAnchor = this.richTextBoxAnchor(element);
				const copyEvent = new Event('copy', { bubbles: true });
				return rtbAnchor.dispatchEvent(copyEvent);
			}

			const selected = element.value.substring(element.selectionStart, element.selectionEnd);
			await window.cargoWiseClient?.requestUserActivation();
			await navigator.clipboard.writeText(selected);
		}
	}

	async cut(element) {
		element ??= document.activeElement;
		if (this.allowCut(element)) {
			if (this.isRichTextBox(element)) {
				const rtbAnchor = this.richTextBoxAnchor(element);
				const cutEvent = new Event('cut', { bubbles: true });
				return rtbAnchor.dispatchEvent(cutEvent);
			}

			const selected = element.value.substring(element.selectionStart, element.selectionEnd);

			if (selected.length > 0) {
				await window.cargoWiseClient?.requestUserActivation();
				await navigator.clipboard.writeText(selected);
				element.setRangeText('', element.selectionStart, element.selectionEnd, 'end');
				element.dispatchEvent(new Event('input', { bubbles: true }));
				element.addEventListener('focusout', this.focusOut);
				element.isPasteOrCut = true;
			}
		}
	}

	async paste(element) {
		element ??= document.activeElement;
		if (!this.allowPaste(element)) {
			return;
		}

		if (this.isRichTextBox(element)) {
			const rtbEditor = element.children[element.children.length - 1];
			await window.cargoWiseClient?.requestUserActivation();
			try {
				const clipboardItems = await navigator.clipboard.read();
				for (const item of clipboardItems) {
					if (item.types.includes('text/html')) {
						item.getType('text/html').then((clipboardMarkup) => {
							clipboardMarkup.text().then((clipText) => rtbEditor.insertContent(clipText));
						});
					} else if (item.types.includes('text/plain')) {
						item.getType('text/plain').then((clipboardMarkup) => {
							clipboardMarkup.text().then((clipText) => rtbEditor.insertText(clipText));
						});
					}
					break;
				}
			} catch (err) {
				console.error('Failed to read clipboard contents: ', err);
			}
			return;
		}

		await window.cargoWiseClient?.requestUserActivation();
		try {
			const text = await navigator.clipboard.readText();
			if (text.length === 0) {
				return;
			}

			const maxLength = element.maxLength;
			const oldSelectionStart = element.selectionStart;
			let newValue =
				element.value.substring(0, element.selectionStart) +
				text +
				element.value.substring(element.selectionEnd);

			if (maxLength > -1 && newValue.length > maxLength) {
				newValue = newValue.substring(0, maxLength);
			}

			element.value = newValue;
			element.dispatchEvent(new Event('input', { bubbles: true }));
			element.addEventListener('focusout', this.focusOut);
			element.isPasteOrCut = true;
			element.selectionStart = oldSelectionStart + text.length;
			element.selectionEnd = oldSelectionStart + text.length;
		} catch (err) {
			console.error('Failed to read clipboard contents: ', err);
		}
	}

	async pastePlainText(element) {
		element ??= document.activeElement;
		if (!this.allowPaste(element)) {
			return;
		}

		const rtbEditor = element.children[element.children.length - 1];
		await window.cargoWiseClient?.requestUserActivation();
		const clipboardItems = await navigator.clipboard.read();
		try {
			for (const item of clipboardItems) {
				if (item.types.includes('text/plain')) {
					item.getType('text/plain').then((clipboardMarkup) => {
						clipboardMarkup.text().then((clipText) => {
							const lineBreak = clipText.includes('\r\n') ? '\r\n' : '\n';

							if (item.types.includes('text/html')) {
								clipText = clipText.replaceAll(lineBreak + lineBreak, lineBreak);
							}
							rtbEditor.insertText(clipText);
						});
					});
				} else if (item.types.includes('text/html')) {
					item.getType('text/html').then((clipboardMarkup) => {
						clipboardMarkup.text().then((clipText) => {
							const newDocument = document.implementation.createHTMLDocument();
							const newContent = newDocument.createElement('DIV');
							newContent.innerHTML = clipText;
							newDocument.body.appendChild(newContent);

							rtbEditor.insertText(newContent.innerText);
						});
					});
				}
				break;
			}
		} catch (err) {
			console.error('Failed to read clipboard contents: ', err);
		}
	}

	getSupportedActions(element) {
		element ??= document.activeElement;
		return {
			allowCut: this.allowCut(element),
			allowCopy: this.allowCopy(element),
			allowPaste: this.allowPaste(element),
		};
	}

	focusOut(e) {
		const element = e.currentTarget;
		if (element) {
			element.dispatchEvent(new Event('change'));
			element.removeEventListener('focusout', this.focusOut);
		}
	}

	allowCut(element) {
		return this.isSupportedElement(element) && !this.isReadOnly(element) && !this.isSelectionEmpty(element);
	}

	allowCopy(element) {
		return this.isSupportedElement(element) && !this.isSelectionEmpty(element);
	}

	allowPaste(element) {
		return this.isSupportedElement(element) && !this.isReadOnly(element);
	}

	isSelectionEmpty(element) {
		if (this.isRichTextBox(element)) {
			const selection = document.getSelection();
			return selection.isCollapsed || !this.isSelectionInElement(selection, element);
		}

		return element.selectionStart === element.selectionEnd;
	}

	isSupportedElement(element) {
		return (
			(element instanceof HTMLInputElement && element.type === 'text') ||
			element instanceof HTMLTextAreaElement ||
			this.isRichTextBox(element)
		);
	}

	isRichTextBox(element) {
		const containsEditorAnchor =
			element.classList?.contains('richtextbox') &&
			element.children[element.children.length - 1].classList?.contains('richtextbox__editoranchor');
		const isReadOnly =
			element.classList?.contains('richtextbox--readonly') &&
			element.children[element.children.length - 1].classList?.contains('richtextbox__data');

		return (
			element instanceof HTMLDivElement &&
			(element.classList?.contains('richtextbox__editoranchor') || containsEditorAnchor || isReadOnly)
		);
	}

	richTextBoxAnchor(element) {
		const isRichTextBox = this.isRichTextBox(element);
		const containsEditorAnchor =
			element.children[element.children.length - 1].classList?.contains('richtextbox__editoranchor');
		const containsData = element.children[element.children.length - 1].classList?.contains('richtextbox__data');

		if (isRichTextBox && (containsEditorAnchor || containsData)) {
			return element.children[element.children.length - 1];
		} else if (
			element.classList?.contains('richtextbox__editoranchor') ||
			element.classList?.contains('richtextbox__data')
		) {
			return element;
		}
		return null;
	}

	isReadOnly(element) {
		return element.readOnly || element.disabled;
	}

	isSelectionInElement(selection, element) {
		if (selection.rangeCount === 0) {
			return false;
		}

		for (let i = 0; i < selection.rangeCount; ++i) {
			if (!element.contains(selection.getRangeAt(i).commonAncestorContainer)) {
				return false;
			}
		}
		return true;
	}
}
export let clipboard = new Clipboard();
