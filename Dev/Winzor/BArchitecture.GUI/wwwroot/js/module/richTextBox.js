import { openFileDialog } from '/_content/WinzorFramework/js/module/fileService.js';
import { attachPopup } from '/_content/WinzorFramework/js/module/popup.js';

let isToolBarVisible = true;
let plainText = false;

const smallDataSizeCount = 1024 * 8;
const utf8Encoder = new TextEncoder();

const defaultFontName = 'Microsoft Sans Serif';
const defaultFontSize = '10';

const smbAddress =
	/(\\\\([a-zA-Z0-9\-_\?\,\'\+&%\$#\=~\:\*@\^]|\/\\)[a-zA-Z0-9\-_\?\.\,\'\+&%\$#\=~\:\*@\^\\]*((([a-zA-Z0-9-]+ ([a-zA-Z0-9-]+ )*)[a-zA-Z0-9-]+|(?=([a-zA-Z0-9\-_\?\,\'\+&%\$#\=~\:\*@\^]|\/))))*)/g;

const standardIndentationAmount = 40; // Pixels

class RichTextBoxClientBase {
	constructor(root, dotNetObjectReference, winzorControlId, font) {
		this.winzorControlId = winzorControlId;
		this.dotNetObjectReference = dotNetObjectReference;
		this.font = font;

		this.isReadOnly = false;
		this.acceptsTab = false;
		this.zoomScale = 1;

		this.dataObserver = new MutationObserver(this.handleMutation.bind(this));

		this.root = root;
		this.editor = null;
		this.preventFocusoutDefault = false;
		this.syncedSelection = {
			start: 0,
			end: 0,
		};
		this.repairPromise = Promise.resolve();

		this.fontList = ['Microsoft Sans Serif=microsoft sans serif,sans-serif'];
		this.shortcuts = new Map();
		this.hasPendingChanges = false;
		this.typingTimeout = null;
		this.isTyping = false;

		this.editorReadyPromise = new Promise((resolve) => {
			this.resolveEditorReady = resolve;
		});
	}

	static instances = new Map(); // maps: winzorControlId => RichTextBoxClient

	static async getOrCreateInstance(ClientType, root, content, dotNetObjectReference, winzorControlId, font) {
		let result = this.instances?.get(winzorControlId);
		if (result) {
			// for some reason it's possible for the references to change but if we update them here ensurePrepared will run corrections.
			result.root = root;
			result.dotNetObjectReference = dotNetObjectReference;
		} else {
			result = new ClientType(root, dotNetObjectReference, winzorControlId, font);
			this.instances?.set(winzorControlId, result);
		}

		await result.withErrorHandling(() => result.repair(content));
		return result;
	}

	static tryGetInstance(winzorControlId) {
		return this.instances?.get(winzorControlId);
	}

	static tryDeleteInstance(winzorControlId) {
		return this.instances?.delete(winzorControlId);
	}

	// this method deals with the possibility that e.g. the element tinymce bound to as it's anchor is gone after a tab switch.
	async repair(content) {
		//if for any reason we've tried to call another interop before finishing the previous preparation, this promise chain prevents redundant preparations due to e.g. two preparations waiting on script load in parallel.
		return new Promise((resolve, reject) => {
			this.repairPromise = this.repairPromise
				.then(this.repairInternal.bind(this, content))
				.then(resolve)
				.catch(reject);
		});
	}

	async repairInternal(content) {
		let dataElement = this.root.querySelector('.richtextbox__data');

		if (this.dataElement !== dataElement) {
			this.dataElement = dataElement;

			// observers seem to die during tab switch without necessarily breaking elements, so we may as well reset the observations.
			this.dataObserver.disconnect();

			this.isReadOnly = attributeTruthiness(dataElement.dataset.readOnly);
			this.acceptsTab = attributeTruthiness(dataElement.dataset.acceptsTab);
			this.syncedSelection = {
				start: parseInt(dataElement.dataset.selectionStart, 10),
				end: parseInt(dataElement.dataset.selectionEnd, 10),
			};

			this.dataObserver.observe(this.dataElement, {
				subtree: true,
				childList: true,
				attributes: true,
				characterData: true,
			});
		}
		await this.readyEditor(content);
	}

	handleMouseScroll(e) {
		if (!e.ctrlKey) {
			return;
		}

		e.preventDefault();

		const zoomStep = 0.1;
		const minZoom = 0.1;
		const maxZoom = 4;

		if (e.deltaY < 0) {
			this.zoomScale = Math.min(this.zoomScale + zoomStep, maxZoom);
		} else if (e.deltaY > 0) {
			this.zoomScale = Math.max(this.zoomScale - zoomStep, minZoom);
		}

		this.editorAnchor.style.zoom = this.zoomScale;
	}

	addShortcut(keyCombinations, action) {
		this.shortcuts.set(keyCombinations, action);
	}

	handleShortcuts(event) {
		const normalizedKeyCodes = normalizeKeyboardEventKeys(event);
		let action = this.shortcuts.get(normalizedKeyCodes);
		if (action) {
			action();
		}
	}

	async redispatchKeyEvent(event) {
		event.stopPropagation();
		this.root.dispatchEvent(new KeyboardEvent(event.type, event));
	}

	async redispatchMouseEvent(event, preventDefault) {
		if (preventDefault) {
			event.preventDefault();
		}

		this.handleLinkClicked(event);
		this.root.dispatchEvent(new MouseEvent(event.type, event));
	}

	async handleKeyDown(event) {
		if (event.key === 'Tab' && this.acceptsTab && !event.shiftKey) {
			this.insertTab();
			event.preventDefault();
			event.stopImmediatePropagation();
		} else if (event.key === 'Backspace' && !this.getEditorContent()) {
			event.preventDefault();
			event.stopImmediatePropagation();
		}
		await this.redispatchKeyEvent(event, this.root);
	}

	async handleMutation(changes) {
		this.handleMutationInternal(changes);
	}

	getTextNodes(element) {
		const textNodes = [];
		const nodeIterator = document.createNodeIterator(element, NodeFilter.SHOW_TEXT, {
			acceptNode: function (node) {
				if (node.nodeValue.trim() && !node.parentElement.closest('a')) {
					return NodeFilter.FILTER_ACCEPT;
				}
				return NodeFilter.FILTER_REJECT;
			},
		});

		let currentNode;
		while ((currentNode = nodeIterator.nextNode())) {
			textNodes.push(currentNode);
		}
		return textNodes;
	}

	convertSmbLinkToFileLink(text) {
		const matches = text.match(smbAddress);

		if (matches?.length > 0) {
			const [url] = matches;
			return url.replace('\\\\', 'file://').replaceAll('\\', '/');
		}

		return text;
	}

	isLink(text) {
		const matches = this.getLinkMatches(text);

		if (matches.length > 0) {
			const [url] = matches[0];
			return url === text;
		}

		return false;
	}

	isSmbLink(text) {
		const matches = text.match(smbAddress);

		if (matches?.length > 0) {
			const [url] = matches;
			return url === text;
		}

		return false;
	}

	getLinkMatches(text) {
		// Protocols: (((onenote:)?https?:\/\/)|(mailto|outlook):)
		// Domains: [-a-zA-Z0-9@:%._\+~#=]*
		// SubDirectory: [-a-zA-Z0-9()@:%_\+.~#?&//=(&amp;/*)]*
		const urlRegexWithDomain =
			/((((onenote:)?https?:\/\/)|(mailto|outlook):)([-a-zA-Z0-9@:%._+~#=]*)(\/)?([-a-zA-Z0-9(){}@:%_+,!.~#?&$/=/*(&amp;)'|]*))/g;
		const urlRegexWithoutDomain =
			/(((ftp:\/\/)|(callto|file:[A-Z]|telnet|edient|tel):)([-a-zA-Z0-9()@:%_.~#?&/=/*(&amp;)]*))/g;
		const combinedRegex = new RegExp(`${urlRegexWithDomain.source}|${urlRegexWithoutDomain.source}`, 'g');
		return [...text.matchAll(combinedRegex)];
	}

	linkifyTextNode(node) {
		if (isNullOrUndefined(node)) {
			return;
		}

		const originalText = node.nodeValue;
		const parent = node.parentNode;
		if (parent === null) {
			return;
		}

		const matches = this.getLinkMatches(originalText);

		if (matches.length > 0) {
			const fragments = [];
			let lastIndex = 0;

			matches.forEach((match) => {
				const [url] = match;

				// Push text before the match
				fragments.push(document.createTextNode(originalText.slice(lastIndex, match.index)));

				// Create and push the link
				const a = document.createElement('a');
				a.href = url.trim();
				a.textContent = url.trim();
				a.target = '_blank';
				a.rel = 'noopener';
				fragments.push(a);

				lastIndex = match.index + url.length;
			});

			// Push remaining text after the last match
			fragments.push(document.createTextNode(originalText.slice(lastIndex)));

			fragments.forEach((fragment) => {
				parent.insertBefore(fragment, node);
			});

			parent.removeChild(node);
		}
	}

	linkifyTextNodes(contentElement) {
		const textNodes = this.getTextNodes(contentElement);

		textNodes.forEach((node) => this.linkifyTextNode(node));
	}

	pastePlainText(text) {
		const newDocument = document.implementation.createHTMLDocument();
		const newContent = newDocument.createElement('P');
		newContent.innerText = text;
		const documentSnapshot = '' + newContent.innerHTML;

		if (newContent.innerHTML.startsWith('<br>')) {
			newContent.innerHTML = newContent.innerHTML.slice('<br>'.length);
		}

		if (newContent.innerHTML.endsWith('<br>')) {
			newContent.innerHTML = newContent.innerHTML.slice(0, newContent.innerHTML.length - '<br>'.length);
		}

		this.cleanMarkup(newContent.childNodes, newDocument);

		this.linkifyTextNodes(newContent);

		const cleanedText = newContent.innerHTML.replaceAll('<br>', '</p><p>');

		if (documentSnapshot !== newContent.innerHTML || cleanedText !== newContent.innerHTML) {
			this.editor.insertContent(`<p>${cleanedText}</p>`);
		} else {
			// When we're adding just text, especially to a list item, adding a <p> tag can cause issues.
			this.editor.insertContent(cleanedText);
		}

		const selection = this.getSelection();
		setTimeout(() => {
			this.linkifyTextNodes(this.editor);
			this.setSelection(selection);
		});
	}

	async handleMutationInternal(changes) {
		for (let change of changes) {
			if (change.target === this.dataElement && change.type === 'attributes') {
				switch (change.attributeName) {
					case 'data-read-only':
						this.isReadOnly = attributeTruthiness(this.dataElement.dataset.readOnly);
						await this.applyReadOnlySetting();
						break;
					case 'data-accepts-tab':
						this.acceptsTab = attributeTruthiness(this.dataElement.dataset.acceptsTab);
						break;
				}
			}
		}
	}

	applyDataFromServer(content) {
		// The HTML content may have spaces between tags... lets remove any extra (and meaningless) whitespace
		const tempElement = document.createElement('div');
		tempElement.innerHTML = content;
		let parsedContent = Array.from(tempElement.childNodes)
			.map((node) => node.outerHTML)
			.join('');

		if (!this.isReadOnly) {
			this.editor.innerHTML = parsedContent;
			this.convertToSegoeUiEmojiForEmoji(this.editor);
			this.hasPendingChanges = false;
			document.removeEventListener('mousedown', this.aboutToLoseFocus, true);
		} else {
			const textbox = this.dataElement;
			textbox.innerHTML = parsedContent;
			textbox.querySelectorAll('a').forEach((el) => (el.target = '_blank'));
			this.convertToSegoeUiEmojiForEmoji(textbox);
		}
	}

	convertToSegoeUiEmojiForEmoji(root) {
		const walker = document.createTreeWalker(
			root,
			NodeFilter.SHOW_ELEMENT,
			{
				acceptNode: (node) => {
					const matchesFont = node.style.fontFamily.startsWith('"Segoe UI Symbol"');
					// CW's richtextbox and WTG.RtfConverter automatically breaks emoji and text into different fragments,
					// so we can be optimistic that a textContent will either contain only emoji or none at all.
					const containsOnlyEmoji = /\p{Extended_Pictographic}/u.test(node.textContent);
					return matchesFont && containsOnlyEmoji ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_SKIP;
				},
			},
			false
		);

		while (walker.nextNode()) {
			walker.currentNode.style.fontFamily = '"Segoe UI Emoji", "Segoe UI Symbol", "Segoe UI", sans-serif';
		}
	}

	onLinkClicked(event) {
		this.root.dispatchEvent(
			new CustomEvent('linkclicked', {
				bubbles: true,
				detail: { linkText: event?.target?.attributes?.href?.value },
			})
		);
	}

	toJSStreamRefIntelligence(str) {
		const useRef = str && str.length > smallDataSizeCount;
		if (useRef) {
			// the buffer size is larger than str length
			const encodedData = utf8Encoder.encode(str);
			const jsStreamRef = DotNet.createJSStreamReference(encodedData);
			return {
				contentString: null,
				contentStream: jsStreamRef,
				contentLength: encodedData.length,
			};
		}
		return {
			contentString: str,
			contentStream: null,
			contentLength: 0,
		};
	}

	async applyReadOnlySetting() {
		if (this.isReadOnly) {
			this.destroyEditor();
		} else {
			await this.readyEditor();
		}
	}

	getDOMSelection() {
		return document.getSelection();
	}

	getSelection() {
		let domSelection = this.getDOMSelection();
		if (domSelection.type === 'None') {
			return null;
		} else {
			let selectionFinder = this.createSelectionFinder();
			return selectionFinder.caretRangeFor(domSelection.getRangeAt(0));
		}
	}

	setSelection(selection) {
		if (isNullOrUndefined(selection)) {
			selection = { start: 0, end: 0 };
		}

		// note: don't manually fire events in this method, this is used by tests so it should be left up to the 3rd-party editor if possible.
		let newRange = this.createSelectionFinder().DOMRangeFor(selection);
		let domSelection = this.getDOMSelection();
		domSelection.setBaseAndExtent(
			newRange.startContainer,
			newRange.startOffset,
			newRange.endContainer,
			newRange.endOffset
		);
	}

	setContent(content) {
		// note: don't manually fire events in this method, this is used by tests so it should be left up to the 3rd-party editor if possible.
		if (this.isReadOnly) {
			throw new Error('Client cannot set content in read-only mode');
		} else {
			this.editorAnchor.innerHTML = content;
		}
	}

	createSelectionFinder() {
		if (this.isReadOnly) {
			return new SelectionFinder(this.dataElement);
		} else {
			return new SelectionFinder(this.getEditorBody());
		}
	}

	async withErrorHandling(action) {
		try {
			await action();
		} catch (error) {
			console.error(error);
		}
	}

	cleanMarkup(nodes, activeDocument) {
		if (typeof activeDocument === 'undefined') {
			activeDocument = document;
		}

		const inlineElements = [
			'A',
			'ABBR',
			'B',
			'BDI',
			'BDO',
			'BR',
			'CITE',
			'CODE',
			'DATA',
			'DFN',
			'EM',
			'I',
			'KBD',
			'MARK',
			'Q',
			'RP',
			'RT',
			'RUBY',
			'S',
			'SAMP',
			'SMALL',
			'SPAN',
			'STRONG',
			'SUB',
			'SUP',
			'TIME',
			'U',
			'VAR',
			'WBR',
		];
		const safeAttributes = ['style', 'href', 'data-squiggle'];
		const safeElements = [
			'#text',
			'P',
			'UL',
			'OL',
			'LI',
			'SPAN',
			'STRONG',
			'B',
			'EM',
			'I',
			'U',
			'STRIKE',
			'S',
			'A',
			'BLOCKQUOTE',
			'BR',
			'TABLE',
			'THEAD',
			'TBODY',
			'TFOOT',
			'TR',
			'TH',
			'TD',
		];
		const safeStyles = [
			'backgroundColor',
			'background',
			'border',
			'borderBottom',
			'borderBottomColor',
			'borderBottomStyle',
			'borderBottomWidth',
			'borderColor',
			'borderLeft',
			'borderLeftColor',
			'borderLeftStyle',
			'borderLeftWidth',
			'borderRight',
			'borderRightColor',
			'borderRightStyle',
			'borderRightWidth',
			'borderTop',
			'borderTopColor',
			'borderTopStyle',
			'borderTopWidth',
			'borderStyle',
			'borderWidth',
			'color',
			'font',
			'fontFamily',
			'fontSize',
			'fontStyle',
			'fontWeight',
			'margin',
			'marginLeft',
			'padding',
			'paddingBottom',
			'paddingLeft',
			'paddingTop',
			'textAlign',
			'textDecoration',
			'textDecorationColor',
			'textDecorationLine',
			'textDecorationStyle',
			'width',
		];
		const rootElements = ['P', 'UL', 'OL', 'BLOCKQUOTE', 'TABLE'];
		const stripElements = ['IMG', 'SCRIPT', 'STYLE', 'LINK', 'META'];
		const isAnchorRelOrTargetAttribute = (node, attribute) =>
			node.nodeName === 'A' && ['rel', 'target'].includes(attribute.name);
		const isRootElement = (node) =>
			node.parentNode?.classList?.contains('richtextbox__editoranchor') && node.parentNode === this.editor;
		const isRootOfSandbox = (node) =>
			node.parentNode.className === 'richtextbox__sandbox' && node.parentNode.parentNode.nodeName === 'BODY';
		const isSandboxed = activeDocument.URL === 'about:blank'; // user is pasting or using this.editor.insertContent()
		const copyStyles = (oldNode, newNode) => {
			const currentStyles = getComputedStyle(oldNode);
			const parentStyles = getComputedStyle(oldNode.parentNode);
			newNode.style.fontFamily = oldNode.style.fontFamily || currentStyles.fontFamily;
			newNode.style.fontSize = oldNode.style.fontSize || currentStyles.fontSize;
			newNode.style.fontStyle = oldNode.style.fontStyle || currentStyles.fontStyle;
			newNode.style.fontWeight = oldNode.style.fontWeight || currentStyles.fontWeight;

			if (oldNode.nodeName !== 'A') {
				newNode.style.color = oldNode.style.color || currentStyles.color;
				newNode.style.textDecoration = oldNode.style.textDecoration || currentStyles.textDecoration;
				newNode.style.textDecorationLine = oldNode.style.textDecorationLine || currentStyles.textDecorationLine;
			}

			Object.entries(newNode.style).forEach((style) => {
				let [key, value] = style;

				if (value === parentStyles[key] && value !== '') {
					newNode.style[key] = '';
				}
			});
		};

		const nodesToRemove = [];
		nodes.forEach((node) => {
			if (
				stripElements.includes(node.nodeName) ||
				node.nodeName === '#comment' ||
				node.nodeName.startsWith('O:')
			) {
				nodesToRemove.push(node);
				return;
			}

			if (node.nodeName === 'SPAN' && node.textContent !== '' && node.textContent.trim() === '') {
				const msoTabCountMatch = node.getAttribute('style').match(/mso-tab-count:\s*(\d+)/);
				if (msoTabCountMatch) {
					const msoTabCount = msoTabCountMatch[1];
					node.textContent = '\t'.repeat(msoTabCount);
					node.removeAttribute('style');
				}
			}

			if (
				isRootOfSandbox(node) &&
				node.nodeName === '#text' &&
				node.textContent.trim() === '' &&
				nodes.length > 1
			) {
				nodesToRemove.push(node);
				return;
			} else if (isRootElement(node) && node.nodeName === '#text') {
				if (node.textContent.trim() === '' && nodes.length > 1) {
					nodesToRemove.push(node);
					return;
				}

				const replacementNode = activeDocument.createElement('p');
				replacementNode.textContent = node.textContent;
				node.replaceWith(replacementNode);
				return;
			} else if (isRootElement(node) && node.nodeName === 'LI') {
				const previousSibling = node.previousSibling;

				if (previousSibling && ['UL', 'OL'].includes(previousSibling.nodeName)) {
					previousSibling.appendChild(node);
				} else {
					const newList = activeDocument.createElement('ul');
					newList.innerHTML += node.outerHTML;
					node.replaceWith(newList);
					return;
				}
			} else if (isRootElement(node) && !rootElements.includes(node.nodeName)) {
				const replacementNode = activeDocument.createElement('p');
				replacementNode.innerHTML = node.innerHTML;
				copyStyles(node, replacementNode);

				switch (node.nodeName) {
					case 'A':
						replacementNode.innerHTML = node.outerHTML;
						break;

					case 'SPAN':
						if (node.nextElementSibling?.nodeName === 'BR') {
							node.nextElementSibling.remove();
						}
						break;

					case 'PRE':
					case 'CODE':
						if (node.innerHTML.indexOf('\n') >= 0) {
							replacementNode.style = node.style;
							replacementNode.innerHTML = node.innerHTML.trim().split('\n').join('<br>');
						}
						break;
				}

				node.replaceWith(replacementNode);
			} else if (!safeElements.includes(node.nodeName)) {
				const currentDisplay = inlineElements.includes(node.nodeName)
					? 'inline'
					: getComputedStyle(node).display;
				let replacementNode;

				if (currentDisplay === 'none') {
					nodesToRemove.push(node);
					return;
				}

				switch (currentDisplay) {
					case 'inline':
					case 'inline-flex':
						replacementNode = activeDocument.createElement('span');
						break;

					case 'block':
					case 'flex':
					default:
						replacementNode = activeDocument.createElement('p');
						break;
				}

				replacementNode.innerHTML = node.innerHTML;
				switch (node.nodeName) {
					case 'PRE':
					case 'CODE':
						if (node.innerHTML.indexOf('\n') >= 0) {
							replacementNode.style = node.style;
							replacementNode.innerHTML = node.innerHTML.trim().split('\n').join('<br>');
						}
						break;
				}

				copyStyles(node, replacementNode);
				node.replaceWith(replacementNode);

				if (node.nodeName !== replacementNode.nodeName) {
					// This should be redundant, but isn't always.
					node = replacementNode;
				}
			} else if (
				!isRootElement(node) &&
				!inlineElements.includes(node.nodeName) &&
				rootElements.includes(node.nodeName) &&
				node.parentNode.nodeName === 'P'
			) {
				// Fix for nested lists
				const nodeIndex = Array.from(node.parentNode.childNodes).indexOf(node);
				const currentSelection = this.getSelection();

				if (nodeIndex === 0 && isRootElement(node.parentNode)) {
					const currentParent = node.parentNode;
					node.parentNode.before(node);

					if (currentParent.innerText === '' && currentParent.innerHTML === '') {
						currentParent.remove();

						currentSelection.start--;
					}
				}
			}

			// <span style="font-style: italic;">Lorem </span><span style="font-style: italic;">ipsum</span> => <span style="font-style: italic;">Lorem ipsum</span>
			if (!isRootElement(node) && node.nodeName === 'SPAN') {
				const nextNode = node.nextSibling;
				if (
					nextNode &&
					nextNode.nodeName === 'SPAN' &&
					node.getAttribute('style') === nextNode.getAttribute('style')
				) {
					node.innerHTML += nextNode.innerHTML;
					nextNode.remove();
				}
			}

			if (typeof node.attributes !== 'undefined') {
				const nodeAttributes = Object.entries(node.attributes);
				const nodeStyles = node.attributes.getNamedItem('style')
					? Object.entries(node.style).filter((item) => isNaN(item[0]) && item[1] !== '')
					: [];

				/**
				 * If we have nodeAttributes.style == "font-weight: bold; font-style: italic;,",
				 * then node.style returns an object of 621 objects that looks like:
				 * {
				 *     0: "font-weight",
				 *     1: "font-style",
				 *     accentColor: "",
				 *     additiveSymbols: "",
				 *     ...
				 *     fontWeight: "bold",
				 *     fontStyle: "italic",
				 * }
				 * Filtering down to be non-numeric results with non-blank results is the best way to access the specified styles.
				 */

				nodeAttributes.forEach((attribute) => {
					// If the attribute is not safe or its not an anchor rel or target attribute, remove it.
					if (
						!safeAttributes.includes(attribute[1].name.toLowerCase()) &&
						!isAnchorRelOrTargetAttribute(node, attribute[1])
					) {
						node.attributes.removeNamedItem(attribute[1].name);
					}
				});

				nodeStyles.forEach((style) => {
					if (node.style[style[0]].indexOf('var(') > -1 || !safeStyles.includes(style[0])) {
						node.style[style[0]] = '';
					}

					switch (style[0]) {
						case 'textAlign':
							if (!['left', 'center', 'right'].includes(style[1])) {
								node.style[style[0]] = '';
							}
							break;

						case 'margin':
						case 'marginLeft':
						case 'padding':
						case 'paddingBottom':
						case 'paddingLeft':
						case 'paddingTop':
						case 'width':
							if (style[1].match(/^[a-z]/gi)) {
								node.style[style[0]] = '';
							}
							break;
					}

					switch (node.style[style[0]]) {
						case 'auto':
						case 'inherit':
						case 'initial':
							node.style[style[0]] = '';
							break;
					}
				});

				if (node.attributes.style?.nodeValue === '') {
					// If the node has no styles, remove the style attribute.
					node.removeAttribute('style');
				}

				if (node.attributes.length === 0 && node.nodeName === 'SPAN' && node.parentNode !== null) {
					// If the node has no attributes, remove it.
					if (node.textContent.trim() === '') {
						node.replaceWith(...node.childNodes);
					} else {
						node.outerHTML = node.innerHTML;
					}
				}
			}

			switch (node.nodeName) {
				case 'A':
					// prevent observer from firing when we assign the same value to target or rel attributes.
					if (node.target !== '_blank') {
						node.target = '_blank';
					}
					if (node.rel !== 'noopener') {
						node.rel = 'noopener';
					}
					if (activeDocument.URL === 'about:blank' && node.href.startsWith('edient:')) {
						node.style.color = '#0000FF';
						node.style.fontSize = '12pt';
					}
					break;

				case '#text':
					if (node.textContent === '\n' && isToolBarVisible) {
						const br = activeDocument.createElement('br');
						node.replaceWith(br);

						// Set cursor position after the <br> tag
						const range = activeDocument.createRange();
						range.setStartAfter(br);
						range.setEndAfter(br);

						const selection = window.getSelection();
						selection.removeAllRanges();
						selection.addRange(range);
					} else if (node.textContent.indexOf('\n') >= 0) {
						if (!plainText) {
							node.textContent = node.textContent.replaceAll('\n', ' ');
						} else {
							// split the text node into multiple text nodes, separated by a <br>
							const text = node.textContent.split('\n');
							const parent = node.parentNode;
							const nextSibling = node.nextSibling;
							const selection = this.getSelection();

							text.forEach((line, index) => {
								const newTextNode = activeDocument.createTextNode(line);
								parent.insertBefore(newTextNode, nextSibling);
								if (index < text.length - 1) {
									parent.insertBefore(activeDocument.createElement('br'), nextSibling);
								}
							});

							node.remove();
							this.setSelection(selection);
						}
					}

					if (node.parentNode && ['UL', 'OL'].includes(node.parentNode.nodeName) && node.textContent.trim()) {
						const previousSibling = node.previousSibling;
						if (previousSibling && previousSibling.nodeName === 'LI') {
							previousSibling.innerHTML += `<br>${node.textContent}`;
							nodesToRemove.push(node);
						} else if (!previousSibling) {
							const newLi = activeDocument.createElement('li');
							newLi.innerHTML = node.textContent;
							node.replaceWith(newLi);
						}
					}
					if (node.textContent.trim() !== '' && node.parentNode?.nodeName !== 'A') {
						this.linkifyTextNode(node);
					}
					break;

				case 'P':
					if (node.parentNode.nodeName === 'P' && node.innerText === '' && !isSandboxed) {
						node.replaceWith(activeDocument.createElement('br'));
						return;
					} else if (node.parentNode.nodeName !== 'P' && node.innerText === '' && node.innerHTML === '') {
						node.append(activeDocument.createElement('br'));
					}
					break;
			}

			if (node.childNodes?.length > 0) {
				this.cleanMarkup(node.childNodes, activeDocument);
			}
		});
		nodesToRemove.forEach((node) => node.remove());
	}

	updateLinkDestination(nodes) {
		nodes.forEach((node) => {
			if (node.nodeName === 'A') {
				// Update the destination if it is different from the display link
				const displayText = node.innerText;
				const destination = node.getAttribute('href');

				if (
					displayText !== destination &&
					displayText !== decodeURIComponent(destination) &&
					this.isLink(displayText)
				) {
					node.href = displayText;
				} else if (
					displayText !== destination &&
					displayText !== decodeURIComponent(destination) &&
					this.isSmbLink(displayText)
				) {
					node.href = this.convertSmbLinkToFileLink(displayText);
				}
			} else if (node.childNodes?.length > 0) {
				this.updateLinkDestination(node.childNodes);
			}
		});
	}

	/* Editor-specific logic */
	async createEditor() {
		throw new Error('Call to unimplemented base method');
	}

	destroyEditor() {
		throw new Error('Call to unimplemented base method');
	}

	async readyEditor() {
		throw new Error('Call to unimplemented base method');
	}

	intentToPasteAsPlainText() {
		return this.shiftKeyActive && this.ctrlKeyActive;
	}

	mapFont(font) {
		const fontMap = new Map([
			['Arial', 'sans-serif'],
			['Calibri', 'sans-serif'],
			['Helvetica', 'sans-serif'],
			['Verdana', 'sans-serif'],
			['Consolas', 'monospace'],
			['Courier', 'monospace'],
			['Lucida Console', 'monospace'],
			['Times', 'serif'],
			['Georgia', 'serif'],
			['Cambria', 'serif'],
			['Sans', 'sans-serif'],
			['Mono', 'monospace'],
		]);

		const matchedFont = [...fontMap.keys()].find((key) => font.includes(key));
		return matchedFont ? font + ', ' + fontMap.get(matchedFont) : font + ', serif';
	}

	async pastePostProcess() {
		throw new Error('Call to unimplemented base method');
	}

	getEditorContent() {
		throw new Error('Call to unimplemented base method');
	}

	getDOMSelectionFromEditor() {
		throw new Error('Call to unimplemented base method');
	}

	insertTab() {
		throw new Error('Call to unimplemented base method');
	}

	getEditorBody() {
		throw new Error('Call to unimplemented base method');
	}

	isCurrentSelectionTextboxOfRtb() {
		throw new Error('Call to unimplemented base method');
	}

	setSelectionContent() {
		throw new Error('Call to unimplemented base method');
	}

	traverseDOMTree(nodeToTraverse, acceptNode, callback) {
		// start with fore-most leaf node
		while (nodeToTraverse.hasChildNodes()) {
			nodeToTraverse = nodeToTraverse.firstChild;
		}

		while (!acceptNode(nodeToTraverse)) {
			nodeToTraverse = nodeToTraverse.nextSibling || nodeToTraverse.parentNode;

			if (nodeToTraverse === this.editor || nodeToTraverse === null) {
				return;
			}
		}

		nodeToTraverse = callback(nodeToTraverse);

		while (nodeToTraverse.nextSibling === null) {
			nodeToTraverse = nodeToTraverse.parentNode;

			if (nodeToTraverse === this.editor || nodeToTraverse === null) {
				return;
			}
		}

		nodeToTraverse = nodeToTraverse.nextSibling;
		this.traverseDOMTree(nodeToTraverse, acceptNode, callback);
	}

	isInRange(node, startContainer, endContainer) {
		const isPreceding = Boolean(startContainer.compareDocumentPosition(node) & Node.DOCUMENT_POSITION_PRECEDING);
		const isFollowing = Boolean(endContainer.compareDocumentPosition(node) & Node.DOCUMENT_POSITION_FOLLOWING);
		return !isPreceding && !isFollowing;
	}

	mergeList(listNode) {
		if (!['OL', 'UL'].includes(listNode?.nodeName) || !listNode.hasChildNodes()) {
			return listNode;
		}

		let { nextSibling, previousSibling } = listNode;
		if (nextSibling?.nodeName === listNode.nodeName) {
			while (nextSibling.hasChildNodes()) {
				const child = nextSibling.firstChild;
				listNode.append(child);
				this.mergeList(child);
			}

			nextSibling.remove();
		}

		if (previousSibling?.nodeName === 'LI') {
			previousSibling.append(listNode);
			previousSibling = listNode.previousSibling;
		}

		if (previousSibling?.nodeName === listNode.nodeName) {
			while (listNode.hasChildNodes()) {
				const child = listNode.firstChild;
				previousSibling.append(child);
				this.mergeList(child);
			}

			listNode.remove();
			return previousSibling;
		}

		return listNode;
	}

	convertToListInRange(startContainer, endContainer, listType) {
		// If no new list is being created, we're removing the list
		let hasUpdate = false;

		const splitList = (listItemNode) => {
			const currentList = listItemNode.parentNode;
			const hasPreviousSibling = listItemNode.previousSibling !== null;
			const hasNextSibling = listItemNode.nextSibling !== null;

			const postList = document.createElement(currentList.nodeName);
			while (listItemNode.nextSibling !== null) {
				postList.append(listItemNode.nextSibling);
			}

			const newList = document.createElement(listType);
			newList.append(listItemNode);

			const lists = [];
			if (hasPreviousSibling) {
				lists.push(currentList);
			}

			lists.push(newList);

			if (hasNextSibling) {
				lists.push(postList);
			}

			currentList.replaceWith(...lists);
		};

		const convertToList = (node) => {
			let currentNode = node;
			let nodeToReturn = node;
			let cleanUpOnly = false;

			while (currentNode !== this.editor && currentNode !== null) {
				currentNode = this.mergeList(currentNode);

				// If we're only cleaning up, we don't need to convert the current node
				if (cleanUpOnly) {
					currentNode = currentNode.parentNode;
					continue;
				}

				switch (currentNode.nodeName) {
					case 'BLOCKQUOTE':
						const newP = document.createElement('P');
						newP.style.paddingLeft = '40px';
						newP.append(...currentNode.childNodes);
						currentNode.replaceWith(newP);
						currentNode = newP;
						break;
					case 'P':
						hasUpdate = true;

						let list = document.createElement(listType);
						const listItem = document.createElement('LI');
						list.append(listItem);

						// get indent level
						const paddingLeft = parseInt(currentNode.style.paddingLeft, 10);
						const indentLevel = paddingLeft / 40;

						for (let i = 0; i < indentLevel; i++) {
							const newList = document.createElement(listType);
							newList.append(list);
							list = newList;
						}

						listItem.append(...currentNode.childNodes);

						// node will be replaced below, should return the replacement
						if (nodeToReturn === currentNode) {
							nodeToReturn = listItem;
						}

						currentNode.replaceWith(list);
						currentNode = listItem;
						cleanUpOnly = true;
						currentNode = currentNode.parentNode;
						break;
					case 'OL':
					case 'UL':
						if (['OL', 'UL'].includes(currentNode.parentNode.nodeName)) {
							if (currentNode.parentNode.nodeName !== listType) {
								splitList(currentNode);
								// Repeat the process for the current node in the new list
								break;
							}

							currentNode = currentNode.parentNode;
							break;
						}

						cleanUpOnly = true;
						currentNode = currentNode.parentNode;
						break;
					case 'LI':
						// structural clean-up: nested LI are at the same level
						if (currentNode.parentNode.nodeName === 'LI') {
							const currentParentNode = currentNode.parentNode;
							let childrenToflatten = [];
							let tempNode = currentNode;

							while (tempNode !== null) {
								childrenToflatten.push(tempNode);
								tempNode = tempNode.nextSibling;
							}

							currentParentNode.replaceWith(currentParentNode, ...childrenToflatten);
							break;
						}

						if (currentNode.parentNode.nodeName !== listType) {
							hasUpdate = true;

							splitList(currentNode);
						}

						currentNode = currentNode.parentNode;
						break;
					default:
						currentNode = currentNode.parentNode;
				}
			}

			return nodeToReturn;
		};

		this.traverseDOMTree(
			startContainer,
			(node) => this.isInRange(node, startContainer, endContainer),
			convertToList
		);

		return hasUpdate;
	}

	removeListInRange(startContainer, endContainer) {
		const splitList = (listItemNode, shouldIndent) => {
			const listNode = listItemNode.parentNode;
			const hasPreviousSibling = listItemNode.previousSibling !== null;
			const hasNextSibling = listItemNode.nextSibling !== null;

			const postList = document.createElement(listNode.nodeName);
			while (listItemNode.nextSibling !== null) {
				postList.append(listItemNode.nextSibling);
			}

			const newLists = [];
			if (hasPreviousSibling) {
				newLists.push(listNode);
			}

			const newNode = document.createElement('P');

			if (shouldIndent) {
				newNode.style.paddingLeft = (parseInt(listItemNode.style.paddingLeft, 10) || 0) + 40 + 'px';
			}

			newNode.append(...listItemNode.childNodes);
			listItemNode.remove();
			newLists.push(newNode);

			if (hasNextSibling) {
				newLists.push(postList);
			}

			listNode.replaceWith(...newLists);
			return newNode;
		};

		const removeList = (node) => {
			let currentNode = node;

			while (currentNode !== this.editor && currentNode !== null) {
				switch (currentNode.nodeName) {
					case 'LI':
						currentNode = splitList(currentNode);
						break;
					case 'P':
						// Move out of LI
						if (currentNode.parentNode?.nodeName === 'LI') {
							const currentParentNode = currentNode.parentNode;
							let childrenToflatten = [];
							let tempNode = currentNode;

							while (tempNode !== null) {
								childrenToflatten.push(tempNode);
								tempNode = tempNode.nextSibling;
							}

							currentParentNode.replaceWith(currentParentNode, ...childrenToflatten);
						}

						if (['OL', 'UL'].includes(currentNode.parentNode?.nodeName)) {
							currentNode = splitList(currentNode, true);
							break;
						}

						currentNode = currentNode.parentNode;
						break;
					default:
						currentNode = currentNode.parentNode;
				}
			}

			return node;
		};

		this.traverseDOMTree(startContainer, (node) => this.isInRange(node, startContainer, endContainer), removeList);
	}

	toggleList(listType) {
		const selection = this.getDOMSelectionFromEditor();
		const range = selection.getRangeAt(0);

		// Restrict the range to be within the editor
		if (range.startContainer.contains(this.editor)) {
			if (!this.editor.hasChildNodes()) {
				const list = document.createElement(listType);
				list.append(document.createElement('LI'));
				this.editor.append(list);
				return;
			}

			range.setStart(this.editor.firstChild, 0);
		}

		if (range.endContainer.contains(this.editor)) {
			let endNode = this.editor;
			while (endNode.hasChildNodes()) {
				endNode = endNode.lastChild;
			}

			if (endNode.nodeType === Node.TEXT_NODE) {
				range.setEnd(endNode, endNode.length);
			} else {
				range.setEnd(endNode, 0);
			}
		}

		let { startContainer, endContainer, startOffset, endOffset } = range;

		while (startContainer.nodeType === Node.ELEMENT_NODE && startContainer.hasChildNodes()) {
			if (endContainer === startContainer) {
				endContainer = endContainer.childNodes[endOffset];
				endOffset = 0;
			}

			startContainer = startContainer.childNodes[startOffset];
			startOffset = 0;
		}

		if (this.convertToListInRange(startContainer, endContainer, listType)) {
			return;
		}

		// remove list
		this.removeListInRange(startContainer, endContainer);
	}

	async copy(e) {
		e.preventDefault();
		const selection = document.getSelection();

		if (selection.isCollapsed) {
			// No selection to copy
			return;
		}

		const cleanedText = selection.toString().replaceAll('\n\n', '\n');
		const richText = selection.getRangeAt(0).cloneContents();

		const currentRange = selection.getRangeAt(0);
		const simpleRecreation = currentRange.cloneContents();
		const isOnlyText = simpleRecreation.children.length === 0 && simpleRecreation.childNodes.length > 0;
		const parentIsLink = currentRange.commonAncestorContainer.parentElement.nodeName === 'A';
		const formattingParent = currentRange.commonAncestorContainer.parentElement.nodeName !== 'P';
		const listParent = ['UL', 'OL'].includes(currentRange.commonAncestorContainer.nodeName);

		const newDocument = document.implementation.createHTMLDocument();
		newDocument.body.append(richText);

		if (isOnlyText && parentIsLink) {
			newDocument.body.innerHTML = `<a href="${currentRange.commonAncestorContainer.parentElement.getAttribute('href')}" target="_blank" rel="noopener">${newDocument.body.innerHTML}</a>`;
		} else if (isOnlyText && formattingParent) {
			const parent = currentRange.commonAncestorContainer.parentElement;
			const parentStyling = parent.getAttribute('style');

			if (parent.nodeName === 'LI' && parentStyling) {
				newDocument.body.innerHTML = `<span style="${parentStyling}">${newDocument.body.innerHTML}</span>`;
			} else if (parent.nodeName !== 'LI') {
				newDocument.body.innerHTML = `<${parent.nodeName}${parentStyling ? ` style="${parentStyling}"` : ''}>${newDocument.body.innerHTML}</${parent.nodeName}>`;
			}
		} else if (listParent) {
			const parent = currentRange.commonAncestorContainer;
			const parentStyling = parent.getAttribute('style');
			newDocument.body.innerHTML = `<${parent.nodeName}${parentStyling ? ` style="${parentStyling}"` : ''}>${newDocument.body.innerHTML}</${parent.nodeName}>`;
		}

		if (!plainText) {
			const htmlBlob = new Blob([newDocument.body.innerHTML], { type: 'text/html' });
			const textBlob = new Blob([cleanedText], { type: 'text/plain' });
			const data = [
				new ClipboardItem({
					['text/html']: htmlBlob,
					['text/plain']: textBlob,
				}),
			];
			await window.cargoWiseClient?.requestUserActivation();
			await navigator.clipboard.write(data);
		} else {
			await window.cargoWiseClient?.requestUserActivation();
			await navigator.clipboard.writeText(cleanedText);
		}
	}

	formatPainterActive = false;
	formatPainterStyles = {};
}

class WtgEditorBackedClient extends RichTextBoxClientBase {
	static async getOrCreateInstance(root, content, dotNetObjectReference, winzorControlId, font) {
		return RichTextBoxClientBase.getOrCreateInstance(
			WtgEditorBackedClient,
			root,
			content,
			dotNetObjectReference,
			winzorControlId,
			font
		);
	}

	async withBoundaryCheck(action) {
		if (!this.isCurrentSelectionTextboxOfRtb()) {
			return false;
		}

		try {
			await action();
			setTimeout(() => this.selectionChange());
		} catch (error) {
			console.error(error);
		}
	}

	async withRestorativeBoundaryCheck(action) {
		if (
			!this.isCurrentSelectionTextboxOfRtb() &&
			typeof this.currentSelection !== 'undefined' &&
			this.currentSelection !== null
		) {
			this.setSelection(this.currentSelection);
		}

		this.editor?.focus();

		try {
			await action();
		} catch (error) {
			console.error(error);
		}
	}

	async createEditor(content) {
		this.editor = this.editorAnchor;
		this.editorAnchor.setAttribute('contenteditable', true);
		this.editorAnchor.setAttribute('spellcheck', false);
		this.editorAnchor.setAttribute('role', 'textbox');
		this.editorAnchor.setAttribute('aria-multiline', true);
		if (content) {
			this.applyDataFromServer(content);
		}
		this.editorAnchor.addEventListener('input', (e) => this.withErrorHandling(() => this.handleInput(e)));
		this.editorAnchor.addEventListener('blur', (e) => this.withErrorHandling(() => this.handleBlur(e)));
		this.editorAnchor.addEventListener('focusout', (e) => this.withErrorHandling(() => this.handleFocusOut(e)));
		/*The mousedown event may be handled by other controls to dispatch events to the server.
		These events may require the current RichTextBox content, and so we should proactively sync the editor content if we suspect
		that we may be about to lose focus as a result of the user clicking elsewhere in the document. This ensures that the contentchanged
		event will be dispatched before any mousedown event handlers are run, ensuring the correct order when processed on the server.
		NOTE: This needs to be run in the capturing phase of the mousedown event to ensure we are able to handle the event before Blazor*/
		this.aboutToLoseFocus = (e) => this.withErrorHandling(() => this.handleAboutToLoseFocus(e));
		this.editorAnchor.addEventListener('mousedown', (e) => {
			const RIGHT_MOUSE_BUTTON = 2;
			if (e.buttons === RIGHT_MOUSE_BUTTON) {
				e.preventDefault();
			}
			this.withErrorHandling(() => this.redispatchMouseEvent(e, false));
			return !IsLinkClick(e);
		});
		this.editorAnchor.addEventListener('click', (event) => {
			if (event.layerX < 3) {
				setTimeout(() => {
					const selection = window.getSelection();
					switch (event.detail) {
						case 1:
							selection.modify('move', 'backward', 'lineboundary');
							selection.modify('extend', 'forward', 'line');
							break;
						case 2:
							if (event.target.nodeName === 'P' || event.target.nodeName === 'SPAN') {
								selection.selectAllChildren(selection.baseNode.parentElement);
							} else {
								selection.modify('move', 'backward', 'paragraphboundary');
								selection.modify('extend', 'forward', 'paragraph');
							}
							break;
						case 3:
							selection.selectAllChildren(this.editor);
							break;
					}
				});
			}
		});

		this.editorAnchor.addEventListener('keydown', (e) => {
			this.withErrorHandling(() => this.handleKeyDown(e));
			return WtgEditorBackedClient.hitEnterOnLink(e);
		});
		this.editorAnchor.addEventListener('keydown', (e) => {
			this.withErrorHandling(() => {
				if (this.editorAnchor.isContentEditable) {
					this.handleShortcuts(e);
				}
			});

			const isValidInputChar = e.key.length === 1 && !e.ctrlKey && !e.altKey && !e.metaKey;
			if (isValidInputChar || e.isComposing) {
				this.isTyping = true;
				clearTimeout(this.typingTimeout);
				this.typingTimeout = setTimeout(() => (this.isTyping = false), 500);
			}
		});
		this.editorAnchor.addEventListener('keyup', (e) => this.withErrorHandling(() => this.redispatchKeyEvent(e)));

		this.editorAnchor.mutationHandler = new MutationObserver(() =>
			this.cleanMarkup(this.editorAnchor.childNodes, document)
		);

		this.editorAnchor.mutationHandler.observe(this.root.querySelector('.richtextbox__editoranchor'), {
			subtree: true,
			attributeOldValue: true,
			childList: true,
		});

		this.editorAnchor.linkMutationHandler = new MutationObserver(() =>
			this.updateLinkDestination(this.editorAnchor.childNodes)
		);

		this.editorAnchor.linkMutationHandler.observe(this.root.querySelector('.richtextbox__editoranchor'), {
			subtree: true,
			childList: true,
			characterData: true,
		});

		this.shiftKeyActive = false;
		this.ctrlKeyActive = false;
		this.editorAnchor.addEventListener('keydown', (e) => {
			this.shiftKeyActive = e.shiftKey;
			this.ctrlKeyActive = e.ctrlKey;
		});
		this.editorAnchor.addEventListener('keyup', (e) => {
			this.shiftKeyActive = e.shiftKey;
			this.ctrlKeyActive = e.ctrlKey;
		});

		this.editorAnchor.addEventListener('copy', this.copy);
		this.editorAnchor.addEventListener('cut', async (e) => {
			await this.copy(e);
			document.execCommand('delete');
		});

		this.editorAnchor.addEventListener('paste', (e) => {
			e.preventDefault();

			if (e.clipboardData?.files?.length > 0) {
				return this.pastePostProcess(e);
			}

			this.withErrorHandling(async () => {
				await window.cargoWiseClient?.requestUserActivation();
				let clipboardItems = await navigator.clipboard.read();
				try {
					if (typeof clipboardItems === 'undefined' || !clipboardItems) {
						return;
					} else if (clipboardItems?.items?.length === 0) {
						return;
					}

					for (const item of clipboardItems) {
						if (this.intentToPasteAsPlainText()) {
							// Paste as plain text
							if (item.types.includes('text/plain')) {
								item.getType('text/plain').then((clipboardMarkup) => {
									clipboardMarkup.text().then((clipText) => {
										const lineBreak = clipText.includes('\r\n') ? '\r\n' : '\n';

										if (item.types.includes('text/html')) {
											clipText = clipText.replaceAll(lineBreak + lineBreak, lineBreak);
										}
										this.editorAnchor.insertText(clipText);
									});
								});
							} else if (item.types.includes('text/html')) {
								item.getType('text/html').then((clipboardMarkup) => {
									clipboardMarkup.text().then((clipText) => {
										const newDocument = document.implementation.createHTMLDocument();
										const newContent = newDocument.createElement('DIV');
										newContent.innerHTML = clipText;
										newDocument.body.appendChild(newContent);

										this.editorAnchor.insertText(newContent.innerText);
									});
								});
							}
						} else {
							if (item.types.includes('text/html')) {
								item.getType('text/html').then((clipboardMarkup) => {
									clipboardMarkup
										.text()
										.then((clipText) => this.editorAnchor.insertContent(clipText.trim()));
								});
							} else if (item.types.includes('text/plain')) {
								item.getType('text/plain').then((clipboardMarkup) => {
									clipboardMarkup.text().then((clipText) => this.editorAnchor.insertText(clipText));
								});
							}
						}
						break;
					}
				} catch (err) {
					console.log('Failed to read clipboard contents: ', err);
				}
			});
		});

		this.editorAnchor.addEventListener('drop', (e) => {
			e.preventDefault();

			//Test-only hook: notify host/test that drop was allowed to bubble
			if (typeof window.dropBubbledToHost === 'function') {
				window.dropBubbledToHost();
			}

			if (e.dataTransfer?.files?.length > 0) {
				return this.dropPostProcess(e);
			}
		});

		this.editorAnchor.addEventListener('contextmenu', (e) => this.handleContextMenuEvent(e));

		this.editorAnchor.addEventListener('wheel', (e) => this.handleMouseScroll(e), { passive: false });

		this.editor.insertContent = (html) => {
			const newDocument = document.implementation.createHTMLDocument();
			const newContent = newDocument.createElement('div');
			newContent.className = 'richtextbox__sandbox';
			newContent.innerHTML = html;
			newDocument.body.appendChild(newContent);

			this.cleanMarkup(newDocument.body.children[0].childNodes, newDocument);

			this.withRestorativeBoundaryCheck(() => {
				this.appendLinkDestinations(newDocument.body);
				document.execCommand('insertHTML', false, newDocument.body.children[0].innerHTML);
			});

			const selection = this.getSelection();
			setTimeout(() => {
				this.setSelection(selection);
				this.scrollAfterPaste();
			});
		};

		this.editor.insertText = (text) => this.pastePlainText(text);
		this.editor.getSelection = () => this.getSelection();
		this.editor.setSelection = (selection) => this.setSelection(selection);
		this.editor.setForeColor = (color) => this.setForeColor(color);
		this.editor.getDOMRangeFor = (selection) => this.createSelectionFinder().DOMRangeFor(selection);

		this.cleanMarkup(this.editorAnchor.childNodes, document);
	}

	/**
	 *	Appends the link destinations after the plain text when it runs in plaintext-only mode, unlike linkifyTextNodes() which convert plain URLs found in text nodes into clickable links
	 *	Example:
	 *	Before: <a href="http://example.com">Example</a>
	 *	After: Example <http://example.com>
	 */
	appendLinkDestinations(root) {
		if (!plainText) {
			return;
		}
		const walker = document.createTreeWalker(
			root,
			NodeFilter.SHOW_ELEMENT,
			{
				acceptNode: (node) => {
					return node.nodeName === 'A' ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_SKIP;
				},
			},
			false
		);

		let node;
		while ((node = walker.nextNode())) {
			const displayText = node.innerText;
			const destination = node.getAttribute('href');

			if (displayText !== destination) {
				const replacementText = `${displayText} <${destination}>`;
				const replacementNode = document.createTextNode(replacementText);
				node.replaceWith(replacementNode);
			}
		}
	}

	getSelectedBlocks() {
		const selection = window.getSelection();

		if (!selection.rangeCount) {
			return [];
		}

		function isIndentableElement(element) {
			const selectedTags = ['P', 'UL', 'OL', 'LI'];
			return element && selectedTags.includes(element.tagName);
		}

		const range = selection.getRangeAt(0);

		const commonAncestor = range.commonAncestorContainer;

		let blocks = [];

		const walker = document.createTreeWalker(
			commonAncestor,
			NodeFilter.SHOW_ELEMENT,
			{
				acceptNode: function (node) {
					if (!isIndentableElement(node)) {
						return NodeFilter.FILTER_SKIP;
					}

					const nodeRange = document.createRange();
					nodeRange.selectNodeContents(node);
					return range.compareBoundaryPoints(Range.END_TO_START, nodeRange) < 0 &&
						range.compareBoundaryPoints(Range.START_TO_END, nodeRange) > 0
						? NodeFilter.FILTER_ACCEPT
						: NodeFilter.FILTER_REJECT;
				},
			},
			false
		);

		let currNode = walker.nextNode();
		while (currNode) {
			blocks.push(currNode);
			currNode = walker.nextNode();
		}

		let node = range.startContainer;

		while (node && node !== document) {
			if (isIndentableElement(node) && !blocks.includes(node)) {
				blocks.unshift(node);
				break;
			}
			node = node.parentNode;
		}

		return blocks;
	}

	indentEmptyElement() {
		// place a empty p tag there
		const selection = window.getSelection();
		const range = selection.getRangeAt(0);
		const p = document.createElement('p');
		p.style.paddingLeft = `${standardIndentationAmount}px`;
		range.insertNode(p);
	}

	indent() {
		const blocks = this.getSelectedBlocks();

		let selectionList = [];

		if (!blocks || blocks.length === 0) {
			this.indentEmptyElement();
			return;
		}

		// case for mixed scenarios like: p p p ul li li li /ul p p
		// it will handle the 3 p then call indentList on ul li li li /ul then handle the remaining 2p...
		for (const block of blocks) {
			if (block.tagName === 'P') {
				if (selectionList.length > 0) {
					this.indentList(selectionList);
					selectionList = [];
				}

				const paddingLeft = getComputedStyle(block).paddingLeft;
				const currentIndent = parseFloat(paddingLeft) || 0;
				block.style.paddingLeft = `${currentIndent + standardIndentationAmount}px`;
			} else if (['UL', 'OL', 'LI'].includes(block.tagName)) {
				selectionList.push(block);
			}
		}

		// if the selection ends with list handle it here
		if (selectionList.length > 0) {
			this.indentList(selectionList);
		}
	}

	// finds when the selection cuts off for nested list cases
	// e.g if
	// <ul>
	//   <li>1</li>
	//   <li>2</li>
	//   <ul>
	//     <li>3</li>
	//     <li>4</li>
	//   </ul>
	//   <li>5</li>
	// </ul>
	// in the case of handling 3 and 4 it will know to stop at 4
	// and not go to 5
	findOuterParentIndex(selection, index) {
		const parents = [selection[index]];
		let i = index + 1;

		while (i < selection.length) {
			if (['OL', 'UL'].includes(selection[i].tagName)) {
				parents.push(selection[i]);
			} else if (!parents.includes(selection[i].parentElement)) {
				return i;
			}
			i++;
		}

		return i;
	}

	// if empty lists are left at the end of the indent/outdent remove
	cleanupLists(newList) {
		let prev = newList.previousElementSibling;
		while (prev) {
			if (
				['OL', 'UL'].includes(prev.tagName) &&
				!Array.from(prev.children).some((child) => child.tagName === 'LI')
			) {
				const element = prev.previousElementSibling;
				prev.remove();
				prev = element;
				continue;
			}
			prev = prev.previousElementSibling;
		}

		let next = newList.nextElementSibling;

		while (next) {
			if (
				['OL', 'UL'].includes(next.tagName) &&
				!Array.from(next.children).some((child) => child.tagName === 'LI')
			) {
				const element = next.nextElementSibling;
				next.remove();
				next = element;
				continue;
			}
			next = next.nextElementSibling;
		}
	}

	indentList(selection, inputList = null) {
		const listBlocks = selection.filter((node) => node.tagName === 'LI');

		const first = listBlocks[0];
		const last = listBlocks[listBlocks.length - 1];

		const parent = first.parentElement;
		const parentLast = last.parentElement;

		let newList;

		const prev = first.previousElementSibling;
		const next = last.nextElementSibling;

		// variable used to hold the first child as we insert the contents one by one before the list
		let firstNodeAnchor;

		let appendNextList = false;
		let appendBefore = false;

		if (prev && ['OL', 'UL'].includes(prev.tagName) && next && ['OL', 'UL'].includes(next.tagName)) {
			// case where the selection is between two lists
			newList = prev;
			appendNextList = true;
		} else if (prev && ['OL', 'UL'].includes(prev.tagName)) {
			// if the previous sibling itself is a list then indenting simply adds to this list
			newList = prev;
		} else if (next && ['OL', 'UL'].includes(next.tagName)) {
			// similar to above for next
			newList = next;
			appendBefore = true;
			firstNodeAnchor = newList.firstChild;
		} else {
			// neither is a list so we have to create a new list and place the elements inside
			newList = document.createElement(first.parentElement.tagName);
			parent.insertBefore(newList, first);
		}

		if (parent === parentLast) {
			// simple case where first li and last li share the same parent -> simply append content to new UL or prev/next
			let currListItem = first;

			while (currListItem) {
				const nextListItem = currListItem.nextElementSibling;
				if (appendBefore) {
					// case for next child being a list so we have to add to it in order
					newList.insertBefore(currListItem, firstNodeAnchor);
				} else {
					// case for prev child being a list we simply append
					newList.appendChild(currListItem);
				}
				if (currListItem === last) {
					break;
				}
				currListItem = nextListItem;
			}
		} else {
			// complex cases where first li and last li are not in the same parent
			for (let i = 0; i < selection.length; i++) {
				const currElement = selection[i];
				if (currElement === newList.parentElement) {
					continue;
				}

				if (['OL', 'UL'].includes(currElement.tagName)) {
					// if nested or sublists then recursively call this method
					let lastIndex = this.findOuterParentIndex(selection, i);
					const remaining = selection.slice(i + 1, lastIndex);
					this.indentList(remaining, newList);
					i = lastIndex - 1;
				} else if (appendBefore) {
					// case for next child being a list so we have to add to it in order
					newList.insertBefore(currElement, firstNodeAnchor);
				} else {
					// case for prev child being a list we simply append
					newList.appendChild(currElement);
				}
			}
		}

		if (appendNextList) {
			// appends the new list to the current list merging the two
			// case for selection is between two lists
			while (next.firstChild) {
				newList.appendChild(next.firstChild);
			}
			next.remove();
		}

		// for nested loops cases
		// we append the newly created/indented list to the parent
		if (inputList && !newList.contains(inputList) && newList.children.length > 0) {
			inputList.appendChild(newList);
		}

		// in case of empty lists we remove them
		this.cleanupLists(parent);
	}

	outdent() {
		const blocks = this.getSelectedBlocks();

		let selectionList = [];

		if (!blocks || blocks.length === 0) {
			return;
		}
		for (const block of blocks) {
			switch (block.tagName) {
				case 'P':
					if (selectionList.length > 0) {
						this.outdentList(selectionList);
						selectionList = [];
					}

					const paddingLeft = getComputedStyle(block).paddingLeft;
					const currentIndent = parseFloat(paddingLeft) || 0;

					block.style.paddingLeft =
						currentIndent - standardIndentationAmount > 0
							? `${currentIndent - standardIndentationAmount}px`
							: '';
					break;
				case 'LI':
				case 'UL':
				case 'OL':
					selectionList.push(block);
					break;
			}
		}

		if (selectionList.length > 0) {
			this.outdentList(selectionList);
		}
	}

	getPreviousSiblings(elem) {
		const prev = [];
		let sibling = elem.previousSibling;
		while (sibling) {
			prev.unshift(sibling);
			sibling = sibling.previousSibling;
		}
		return prev;
	}

	getNextSiblings(elem) {
		const next = [];
		let sibling = elem.nextSibling;
		while (sibling) {
			next.push(sibling);
			sibling = sibling.nextSibling;
		}
		return next;
	}

	outdentList(selection) {
		const listBlocks = selection.filter((node) => node.tagName === 'LI');

		let previousList = this.getPreviousSiblings(listBlocks[0]);
		let nextList = this.getNextSiblings(listBlocks[listBlocks.length - 1]);

		let parent = listBlocks[0].parentElement;
		let grandParent = parent.parentElement;

		// next paragraph
		// get the remaining paragraph PAST the end of the selection and attach
		if (nextList.length > 0) {
			let list2 = document.createElement(nextList[0].parentElement.tagName);
			nextList.forEach((li) => list2.appendChild(li));
			grandParent.insertBefore(list2, parent.nextSibling);
		}

		// get the current selection and insert the content
		if (!['OL', 'UL'].includes(grandParent.tagName)) {
			// case where we reach level 0 -> convert to p tag
			[...selection].reverse().forEach((e) => {
				if (e.tagName === 'LI') {
					const p = document.createElement('p');
					p.append(...Array.from(e.childNodes));
					grandParent.insertBefore(p, parent.nextSibling);
					e.remove();
				} else if (e !== parent) {
					grandParent.insertBefore(e, parent.nextSibling);
					e.remove();
				}
			});
		} else {
			// else create list item
			[...selection].reverse().forEach((e) => {
				if (e.tagName === 'LI') {
					const li = document.createElement('li');
					li.append(...Array.from(e.childNodes));
					grandParent.insertBefore(li, parent.nextSibling);
					e.remove();
				} else if (e !== parent) {
					grandParent.insertBefore(e, parent.nextSibling);
					e.remove();
				}
			});
		}

		if (previousList.length > 0) {
			// get the elements before the selection and attach
			let list1 = document.createElement(previousList[0].parentElement.tagName);
			previousList.forEach((li) => list1.appendChild(li));
			grandParent.insertBefore(list1, parent.nextSibling);
		}

		if (parent.children.length === 0) {
			parent.remove();
		}

		// in case of empty lists we remove them
		this.cleanupLists(parent);
	}

	handleContextMenuEvent(e) {
		const MouseEventArgs = [
			'altKey',
			'button',
			'buttons',
			'clientX',
			'clientY',
			'ctrlKey',
			'detail',
			'metaKey',
			'movementX',
			'movementY',
			'offsetX',
			'offsetY',
			'pageX',
			'pageY',
			'screenX',
			'screenY',
			'shiftKey',
			'type',
		];
		const currentSelection = this.getSelection();
		const selectionDetail = {
			selectionStart: currentSelection?.start || 0,
			selectionEnd: currentSelection?.end || 0,
		};

		let dotnetEvent = {};
		MouseEventArgs.forEach((item) => {
			if (typeof e[item] !== 'undefined') {
				dotnetEvent[item] = e[item];
			}
		});
		dotnetEvent.SelectionStart = selectionDetail.selectionStart;
		dotnetEvent.SelectionLength = selectionDetail.selectionEnd - selectionDetail.selectionStart;

		this.root.dispatchEvent(
			new CustomEvent('richtextboxcontextmenu', {
				bubbles: true,
				detail: dotnetEvent,
			})
		);

		this.withErrorHandling(() => this.redispatchMouseEvent(e, true));
		return !IsLinkClick(e);
	}

	handleLinkClicked(event) {
		if (IsLinkClick(event)) {
			this.onLinkClicked(event);
		}
	}

	registerF5Inserter() {
		this.addShortcut('F5', async () => {
			const timeStampWithUserText = await this.withReadOnly(() =>
				this.dotNetObjectReference.invokeMethodAsync('GetTimeStampWithUserTextAsync')
			);
			document.execCommand('insertText', false, timeStampWithUserText);
		});
	}

	destroyEditor() {
		if (this.editor) {
			this.editor.remove();
			this.editor = null;
			this.editorAnchor.remove();
			this.editorAnchor = null;
		}
	}

	clearUndoManager() {
		//TODO: need to redo this, unstable, more function review needed.
	}

	unitConversion(fontSize, desiredUnit) {
		const PT_TO_PX_CONVERSION_FACTOR = 0.75;
		const suffix = fontSize.substring(fontSize.length - 2);

		if (desiredUnit === 'pt' && suffix === 'px') {
			const numbers = parseFloat(fontSize);
			const size = (numbers * PT_TO_PX_CONVERSION_FACTOR).toFixed(1);
			return parseFloat(size);
		}

		return fontSize;
	}

	rgbToHex(rgbString) {
		if (rgbString.substring(0, 3) !== 'rgb') {
			return '#000000';
		}

		const splitRgbToHex = (r, g, b) => {
			return intRgbToHex(parseInt(r), parseInt(g), parseInt(b));
		};
		const intRgbToHex = (r, g, b) => {
			return '#' + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1);
		};
		const processed = rgbString.replace('rgb(', '').replace(')', '').split(', ');

		return splitRgbToHex(processed[0], processed[1], processed[2]);
	}

	setFont(fontStyle) {
		if (isNonNull(fontStyle)) {
			this.forceModifyAttribute(() => this.changeFont(fontStyle));
		}
	}

	changeFont(fontStyle, isFormatPainterButton = false) {
		const originalStyle = new FormatPainterStyles({
			bold: document.queryCommandValue('bold') === 'true',
			italic: document.queryCommandValue('italic') === 'true',
			underline: document.queryCommandValue('underline') === 'true',
			strikethrough: document.queryCommandValue('strikethrough') === 'true',
			color: document.queryCommandValue('foreColor'),
			fontName: document.queryCommandValue('fontName'),
			fontSize: getComputedStyle(document.getSelection().anchorNode.parentNode).fontSize,
		});

		if (originalStyle.bold !== fontStyle.bold) {
			document.execCommand('bold', false, fontStyle.bold);
		}
		if (originalStyle.italic !== fontStyle.italic) {
			document.execCommand('italic', false, fontStyle.italic);
		}
		if (originalStyle.underline !== fontStyle.underline) {
			document.execCommand('underline', false, fontStyle.underline);
		}
		if (originalStyle.strikethrough !== fontStyle.strikethrough) {
			document.execCommand('strikethrough', false, fontStyle.strikethrough);
		}
		if (originalStyle.color !== fontStyle.color) {
			document.execCommand('foreColor', false, fontStyle.color);
		}
		if (originalStyle.fontName.toLowerCase() !== fontStyle.fontName.toLowerCase()) {
			document.execCommand('fontName', false, fontStyle.fontName);
		}
		if (originalStyle.fontSize !== fontStyle.fontSize) {
			if (isFormatPainterButton) {
				document.execCommand('fontSize', false, 3);
				setTimeout(() => {
					const currentSelection = document.getSelection();
					currentSelection.focusNode.parentElement.style.fontSize = fontStyle.fontSize;
				});
			} else {
				document.getSelection().focusNode.parentElement.style.fontSize = fontStyle.fontSize;
			}
		}
	}

	setForeColor(color) {
		if (isNonNull(color) && document.queryCommandValue('foreColor') !== color) {
			this.forceModifyAttribute(() => {
				document.execCommand('foreColor', false, color);
			});
		}
	}

	getWrappingSpansWithTextDecorationLineValue(textDecorationLineValue) {
		const spans = this.editorAnchor.querySelectorAll('span');
		const wrappingSpansWithRequiredStyle = Array.from(spans).filter((span) => {
			const hasLineThroughStyle = span.style.textDecorationLine.includes(textDecorationLineValue);
			const hasMultipleChildren = span.childNodes.length > 1;
			return hasLineThroughStyle && hasMultipleChildren;
		});
		return wrappingSpansWithRequiredStyle;
	}

	removeTextDecorationLineAndEmptySpanParent(wrappingSpan) {
		if (!wrappingSpan || !(wrappingSpan instanceof HTMLElement) || wrappingSpan.tagName !== 'SPAN') {
			return;
		}

		wrappingSpan.style.textDecoration = null;
		wrappingSpan.style.textDecorationLine = null;

		if (wrappingSpan.style.cssText === '') {
			wrappingSpan.removeAttribute('style');
		}

		if (!wrappingSpan.hasAttributes()) {
			while (wrappingSpan.firstChild) {
				wrappingSpan.parentNode.insertBefore(wrappingSpan.firstChild, wrappingSpan);
			}
			wrappingSpan.remove();
		}
	}

	applyTextDecorationLineOnChildNodes(wrappingSpan, textDecorationLineValue) {
		if (!wrappingSpan || !(wrappingSpan instanceof HTMLElement) || wrappingSpan.tagName !== 'SPAN') {
			return;
		}

		Array.from(wrappingSpan.childNodes).forEach((child) => {
			if (child.tagName === 'SPAN') {
				if (child.childNodes.length > 1) {
					this.applyTextDecorationLineOnChildNodes(
						child,
						textDecorationLineValue ||
							wrappingSpan.style.textDecorationLine ||
							wrappingSpan.style.textDecoration
					);
				} else {
					child.style.textDecorationLine =
						textDecorationLineValue ||
						wrappingSpan.style.textDecorationLine ||
						wrappingSpan.style.textDecoration;
				}
			} else if (child.nodeType === Node.TEXT_NODE) {
				const span = document.createElement('span');
				span.textContent = child.textContent;
				span.style.textDecorationLine =
					textDecorationLineValue ||
					wrappingSpan.style.textDecorationLine ||
					wrappingSpan.style.textDecoration;
				child.parentNode.replaceChild(span, child);
			} else if (child.tagName === 'A') {
				child.style.textDecorationLine =
					textDecorationLineValue ||
					wrappingSpan.style.textDecorationLine ||
					wrappingSpan.style.textDecoration;
			}
		});
	}

	copyScrollStyle(editorAnchor, editorData) {
		if (!editorAnchor || !editorData) {
			return;
		}
		editorAnchor.style.overflowWrap = editorData.style.overflowWrap;
		editorAnchor.style.overflowX = editorData.style.overflowX;
		editorAnchor.style.overflowY = editorData.style.overflowY;
	}

	async readyEditor(content) {
		const editorData = this.root.querySelector('.richtextbox__data');
		if (this.isReadOnly) {
			this.applyDataFromServer(content);
			this.root.querySelectorAll('.richtextbox__data a').forEach((el) => (el.target = '_blank'));
			editorData.addEventListener('contextmenu', (e) => this.handleContextMenuEvent(e));
			editorData.addEventListener('copy', async (e) => await this.copy(e));
			editorData.addEventListener('click', (e) => {
				e.preventDefault();
				this.handleLinkClicked(e);
			});
			if (editorData.scrollHeight > editorData.clientHeight) {
				const client = await RichTextBoxClient.tryGetInstance(this.winzorControlId);
				await this.onContentsResized(client, editorData.scrollHeight);
			}
			const scrollHeightMonitor = () => {
				if (editorData.scrollHeight > editorData.clientHeight) {
					const client = RichTextBoxClient.tryGetInstance(this.winzorControlId);
					this.onContentsResized(client, editorData.scrollHeight);
				}
			};
			new ResizeObserver(scrollHeightMonitor).observe(editorData);
			return;
		}

		this.editorAnchor = this.root.querySelector('.richtextbox__editoranchor');
		if (!this.editorAnchor) {
			this.editorAnchor = document.createElement('div');
			this.editorAnchor.className = 'richtextbox__editoranchor';
			this.editorAnchor.id = `richtextbox__editoranchor__${this.winzorControlId}`;
			this.editorAnchor.contentEditable = true;
			this.copyScrollStyle(this.editorAnchor, editorData);
			this.root.appendChild(this.editorAnchor);
		}
		if (this.editor && this.editor !== this.editorAnchor) {
			this.editor.remove();
			this.editor = null;
		}
		if (!this.editor) {
			await this.createEditor(content);
		}

		if (isToolBarVisible && !this.editorToolbar) {
			this.createToolbar();
		}

		document.execCommand('styleWithCSS', true, true);

		this.formatPainterActive = false;

		this.resolveEditorReady();
	}

	async onContentsResized(client, scrollHeight) {
		await client.dotNetObjectReference.invokeMethodAsync('OnContentsResized', scrollHeight);
	}

	createToolbar() {
		this.editorToolbar = this.root.querySelector('.richtextbox__toolbar');
		this.editorToolbarMenus = {
			fontName: this.editorToolbar.querySelector('[aria-label="Fonts"]'),
			fontNameButton: this.editorToolbar.querySelector('[aria-label="Open fonts menu"]'),
			fontSize: this.editorToolbar.querySelector('[aria-label="Font sizes"]'),
			fontSizeButton: this.editorToolbar.querySelector('[aria-label="Open font sizes menu"]'),
		};

		this.editorToolbarButtons = {
			bold: this.editorToolbar.querySelector('[aria-label="Bold"]'),
			italic: this.editorToolbar.querySelector('[aria-label="Italic"]'),
			underline: this.editorToolbar.querySelector('[aria-label="Underline"]'),
			strikethrough: this.editorToolbar.querySelector('[aria-label="Strikethrough"]'),
			unorderedList: this.editorToolbar.querySelector('[aria-label="Bullet list"]'),
			orderedList: this.editorToolbar.querySelector('[aria-label="Numbered list"]'),
			increaseIndent: this.editorToolbar.querySelector('[aria-label="Increase indent"]'),
			decreaseIndent: this.editorToolbar.querySelector('[aria-label="Decrease indent"]'),
			insertFile: this.editorToolbar.querySelector('[aria-label="Attach a file"]'),
			insertImage: this.editorToolbar.querySelector('[aria-label="Insert Image"]'),
			color: this.editorToolbar.querySelector('[aria-label="Text color"] input'),
			formatPainter: this.editorToolbar.querySelector('[aria-label="Format Painter - Copy Selected Formatting"]'),
		};
		const fontPopupHeight = 482;
		this.editorToolbarMenus.fontNameButton.addEventListener('click', () => {
			this.showPopup(
				`${this.winzorControlId}_fontNamePopup`,
				this.editorToolbarMenus.fontName,
				fontPopupHeight,
				'fontName'
			);
		});

		this.editorToolbarMenus.fontNameButton.addEventListener('blur', (event) => {
			this.blurComboBox(event, `${this.winzorControlId}_fontNamePopup`);
		});

		this.editorToolbarMenus.fontSizeButton.addEventListener('click', () => {
			this.showPopup(
				`${this.winzorControlId}_fontSizesPopup`,
				this.editorToolbarMenus.fontSize,
				fontPopupHeight,
				'fontSize'
			);
		});

		this.editorToolbarMenus.fontSizeButton.addEventListener('blur', (event) => {
			this.blurComboBox(event, `${this.winzorControlId}_fontSizesPopup`);
		});

		this.editorToolbarMenus.fontName.addEventListener('change', (event) =>
			this.withRestorativeBoundaryCheck(() => {
				const currentFont = event.target.value;
				if (!this.checkFont(currentFont)) {
					return;
				}
				const newFont = this.mapFont(currentFont);
				document.execCommand('fontName', false, newFont);
			})
		);

		this.editorToolbarMenus.fontSize.addEventListener('change', (event) =>
			this.withRestorativeBoundaryCheck(() => {
				this.fontSizeChange(event.target.value);
			})
		);

		this.editorToolbarButtons.bold.addEventListener('click', () =>
			this.withRestorativeBoundaryCheck(() => document.execCommand('bold'))
		);
		this.editorToolbarButtons.italic.addEventListener('click', () =>
			this.withRestorativeBoundaryCheck(() => document.execCommand('italic'))
		);
		this.editorToolbarButtons.underline.addEventListener('click', () =>
			this.withRestorativeBoundaryCheck(() => {
				const currentSelection = this.getSelection();
				document.execCommand('underline');
				const wrappingSpansWithUnderline = this.getWrappingSpansWithTextDecorationLineValue('underline');

				if (wrappingSpansWithUnderline.length === 0) {
					const wrappingSpansWithStrikethrough =
						this.getWrappingSpansWithTextDecorationLineValue('line-through');
					wrappingSpansWithStrikethrough.forEach((wrappingSpan) => {
						this.applyTextDecorationLineOnChildNodes(wrappingSpan);
						this.removeTextDecorationLineAndEmptySpanParent(wrappingSpan);
					});
				} else {
					wrappingSpansWithUnderline.forEach((wrappingSpan) => {
						this.applyTextDecorationLineOnChildNodes(wrappingSpan);
						this.removeTextDecorationLineAndEmptySpanParent(wrappingSpan);
					});
				}
				this.setSelection({ start: currentSelection.start, end: currentSelection.end });
			})
		);

		this.editorToolbarButtons.strikethrough.addEventListener('click', () =>
			this.withRestorativeBoundaryCheck(() => {
				const currentSelection = this.getSelection();
				document.execCommand('strikethrough');
				const wrappingSpansWithStrikethrough = this.getWrappingSpansWithTextDecorationLineValue('line-through');

				if (wrappingSpansWithStrikethrough.length === 0) {
					const wrappingSpansWithUnderline = this.getWrappingSpansWithTextDecorationLineValue('underline');
					wrappingSpansWithUnderline.forEach((wrappingSpan) => {
						this.applyTextDecorationLineOnChildNodes(wrappingSpan);
						this.removeTextDecorationLineAndEmptySpanParent(wrappingSpan);
					});
				} else {
					wrappingSpansWithStrikethrough.forEach((wrappingSpan) => {
						this.applyTextDecorationLineOnChildNodes(wrappingSpan);
						this.removeTextDecorationLineAndEmptySpanParent(wrappingSpan);
					});
				}
				this.setSelection({ start: currentSelection.start, end: currentSelection.end });

				const selectedContentCommonAncestor = document.getSelection().getRangeAt(0).commonAncestorContainer;
				if (selectedContentCommonAncestor.nodeType === Node.ELEMENT_NODE) {
					const selectedLinks = selectedContentCommonAncestor.querySelectorAll('a');
					selectedLinks.forEach((link) => {
						if (
							window.getSelection().containsNode(link, true) &&
							link.style.textDecorationLine.includes('line-through') &&
							!link.style.textDecorationLine.includes('underline')
						) {
							link.style.textDecorationLine = 'underline ' + link.style.textDecorationLine;
						}
					});
				}
			})
		);

		this.editorToolbarButtons.unorderedList.addEventListener('click', () => {
			this.withRestorativeBoundaryCheck(() => {
				const rtbSelection = this.getSelection();
				this.toggleList('UL');
				setTimeout(() => this.setSelection(rtbSelection));
			});
		});
		this.editorToolbarButtons.orderedList.addEventListener('click', () => {
			this.withRestorativeBoundaryCheck(() => {
				const rtbSelection = this.getSelection();
				this.toggleList('OL');
				setTimeout(() => this.setSelection(rtbSelection));
			});
		});
		this.editorToolbarButtons.insertFile.addEventListener('click', () => this.wtgEditorInsertFile(false));
		this.editorToolbarButtons.insertImage.addEventListener('click', () => this.wtgEditorInsertFile(true));
		this.editorToolbarButtons.increaseIndent.addEventListener('click', () =>
			this.withRestorativeBoundaryCheck(() => this.indent())
		);
		this.editorToolbarButtons.decreaseIndent.addEventListener('click', () =>
			this.withRestorativeBoundaryCheck(() => this.outdent())
		);
		this.editorToolbarButtons.color.addEventListener('change', (e) =>
			this.withRestorativeBoundaryCheck(() => document.execCommand('foreColor', false, e.target.value))
		);
		this.editorToolbarButtons.formatPainter.addEventListener('click', () =>
			this.withRestorativeBoundaryCheck(() => this.formatPainterButton())
		);

		setTimeout(() => {
			this.editorToolbarMenus.fontSize.value = this.getFontSizeOfTheFirstTextNode();
			// if the font of the first node in the rtb is not default font, event selectionchange will be triggered after this createToolbar() to set the correct font
			// so we don't have to handle specifically as the font size
			this.editorToolbarMenus.fontName.value = defaultFontName;
		}, 50);

		document.addEventListener('selectionchange', () => this.withBoundaryCheck(() => this.selectionChange()));
	}

	blurComboBox(event, targetElement) {
		if (event?.relatedTarget?.classList?.contains('combobox__dropdown-item')) {
			return;
		}
		this.removePopup(targetElement);
	}

	showPopup(popupDivId, element, height, type) {
		if (this.removePopup(popupDivId)) {
			return;
		}

		const popupDiv = document.createElement('div');
		const parentElement = element.parentElement;
		const datalist = parentElement.querySelector('datalist');
		popupDiv.id = popupDivId;
		popupDiv.className = 'popup';

		const dropdownDiv = document.createElement('div');
		dropdownDiv.className = 'combobox__dropdown';
		dropdownDiv.style.setProperty('--dropdown-width', `${parentElement.offsetWidth}px`);
		dropdownDiv.style.setProperty('font-size', '10pt');
		dropdownDiv.setAttribute('data-desired-height', height);
		dropdownDiv.addEventListener('onmousedown', (e) => e.preventDefault());

		for (const option of datalist.options) {
			const optionButton = document.createElement('button');
			optionButton.innerText = option.value;
			optionButton.classList.add('combobox__dropdown-item');
			optionButton.style.setProperty('min-height', '16px');
			if (element.value === option.value) {
				optionButton.classList.add('combobox__dropdown-item--selected');
			}
			optionButton.addEventListener('click', () => {
				element.value = option.value;
				popupDiv?.remove();
				switch (type) {
					case 'fontName':
						const newFont = this.mapFont(option.value);
						this.withRestorativeBoundaryCheck(() => document.execCommand('fontName', false, newFont));
						break;
					case 'fontSize':
						this.withRestorativeBoundaryCheck(() => this.fontSizeChange(option.value));
						break;
				}
			});
			dropdownDiv.appendChild(optionButton);
		}
		popupDiv.appendChild(dropdownDiv);
		document.body.appendChild(popupDiv);
		attachPopup(popupDiv, parentElement);
	}

	checkFont(font) {
		const datalist = this.editorToolbarMenus.fontName.parentElement.querySelector('datalist');
		return Array.from(datalist.options).some((option) => option.value === font);
	}

	removePopup(popupDivId) {
		const popupDiv = document.getElementById(popupDivId);
		if (!isNullOrUndefined(popupDiv)) {
			popupDiv.remove();
		}
		return !!popupDiv;
	}

	fontSizeChange(fontSize) {
		document.execCommand('fontSize', false, 7);
		const childSpans = this.editorAnchor.querySelectorAll('*');

		childSpans.forEach((child) => {
			if (child.style.fontSize === 'xxx-large') {
				const regex = /^(?=.*\d).+?(pt|px)$/i;
				if (!regex.test(fontSize)) {
					fontSize = `${fontSize}pt`;
				}

				child.style.fontSize = fontSize;
				if (
					child.parentNode.style.textDecorationLine.match('line-through|underline') ||
					child.parentNode.style.textDecoration.match('line-through|underline')
				) {
					this.applyTextDecorationLineOnChildNodes(child.parentNode);
					this.removeTextDecorationLineAndEmptySpanParent(child.parentNode);
				}
			}
		});
	}

	getFontSizes(selection) {
		const range = selection.getRangeAt(0);

		if (range.collapsed || range.commonAncestorContainer.nodeType === Node.TEXT_NODE) {
			const fontSize = this.getFontSize(range.commonAncestorContainer);
			return fontSize !== null ? [fontSize] : [];
		}

		const walker = document.createTreeWalker(
			range.commonAncestorContainer,
			NodeFilter.SHOW_ALL,
			{
				acceptNode: function (node) {
					return range.intersectsNode(node) ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_SKIP;
				},
			},
			false
		);

		const fontSizes = new Set();
		let node;
		while ((node = walker.nextNode())) {
			if (selection.containsNode(node) && (node.nodeType === Node.TEXT_NODE || node.childNodes.length === 0)) {
				const fontSize = this.getFontSize(node);

				if (fontSize !== null) {
					fontSizes.add(fontSize);
				}
			}
		}

		return Array.from(fontSizes);
	}

	getFontSize(node) {
		if (!node) {
			return null;
		}

		const targetNode = node.nodeType === Node.TEXT_NODE ? node.parentNode : node;

		if (targetNode) {
			const style = window.getComputedStyle(targetNode);
			return style?.fontSize;
		}

		return null;
	}

	getFontSizeOfTheFirstTextNode() {
		if (!this.isCurrentSelectionTextboxOfRtb()) {
			return defaultFontSize;
		}

		const currentFontSizes = this.getFontSizes(document.getSelection());
		const currentFontSize =
			currentFontSizes?.length !== 0 ? this.unitConversion(currentFontSizes[0], 'pt') : defaultFontSize;
		return currentFontSize;
	}

	selectionChange() {
		if (!this.isCurrentSelectionTextboxOfRtb() || !isToolBarVisible || !this.editorToolbarButtons) {
			return;
		}

		this.currentSelection = this.getSelection();
		const currentFontSizes = this.getFontSizes(document.getSelection());
		const currentFontSize = currentFontSizes.length === 1 ? this.unitConversion(currentFontSizes[0], 'pt') : null;

		let anchorParent = document.getSelection().anchorNode.parentNode;
		let currentStyle = new FormatPainterStyles({
			bold: document.queryCommandValue('bold') === 'true',
			italic: document.queryCommandValue('italic') === 'true',
			underline: document.queryCommandValue('underline') === 'true',
			strikethrough: document.queryCommandValue('strikethrough') === 'true',
			color: this.rgbToHex(document.queryCommandValue('foreColor')),
			fontName: getComputedStyle(anchorParent).fontFamily.replaceAll('"', ''),
			fontSize: currentFontSize,
			listItem: anchorParent.nodeName === 'LI',
			unorderedList: anchorParent.parentNode.nodeName === 'UL',
			orderedList: anchorParent.parentNode.nodeName === 'OL',
		});

		if (currentStyle.bold) {
			this.editorToolbarButtons.bold.classList.add('richtextbox__toolbar-item--active');
		} else {
			this.editorToolbarButtons.bold.classList.remove('richtextbox__toolbar-item--active');
		}

		if (currentStyle.italic) {
			this.editorToolbarButtons.italic.classList.add('richtextbox__toolbar-item--active');
		} else {
			this.editorToolbarButtons.italic.classList.remove('richtextbox__toolbar-item--active');
		}

		if (currentStyle.underline) {
			this.editorToolbarButtons.underline.classList.add('richtextbox__toolbar-item--active');
		} else {
			this.editorToolbarButtons.underline.classList.remove('richtextbox__toolbar-item--active');
		}

		if (currentStyle.strikethrough) {
			this.editorToolbarButtons.strikethrough.classList.add('richtextbox__toolbar-item--active');
		} else {
			this.editorToolbarButtons.strikethrough.classList.remove('richtextbox__toolbar-item--active');
		}

		if (currentStyle.unorderedList) {
			this.editorToolbarButtons.unorderedList.classList.add('richtextbox__toolbar-item--active');
		} else {
			this.editorToolbarButtons.unorderedList.classList.remove('richtextbox__toolbar-item--active');
		}

		if (currentStyle.orderedList) {
			this.editorToolbarButtons.orderedList.classList.add('richtextbox__toolbar-item--active');
		} else {
			this.editorToolbarButtons.orderedList.classList.remove('richtextbox__toolbar-item--active');
		}

		this.editorToolbarButtons.color.value = currentStyle.color;

		if (anchorParent.classList.contains('richtextbox')) {
			currentStyle.fontName = defaultFontName;
			currentStyle.fontSize = defaultFontSize;
		}

		const fontNameCleaned = currentStyle.fontName.replaceAll(', ', ',').split(',');
		const fontFamily = Array.from(
			this.editorToolbarMenus.fontName.parentElement.querySelector('datalist').options
		).filter((item) => item.text.toLowerCase() === fontNameCleaned[0].toLowerCase());

		if (fontFamily.length > 0) {
			this.editorToolbarMenus.fontName.value = fontFamily[0].value;
		}

		this.editorToolbarMenus.fontSize.value = currentStyle.fontSize;
	}

	async handleInput() {
		if (!this.hasPendingChanges) {
			this.hasPendingChanges = true;
			this.root.dispatchEvent(new CustomEvent('modified', { bubbles: true }));
			document.addEventListener('mousedown', this.aboutToLoseFocus, true);
		}
	}

	handleFocusOut(event) {
		if (this.preventFocusoutDefault) {
			// We are ignoring the focus out event in JS, so we must also stop the server from handling the event
			event.stopPropagation();
		}
	}

	async handleBlur(event) {
		if (this.preventFocusoutDefault) {
			event.preventDefault();
			return;
		}

		this.syncContent();
	}

	async handleAboutToLoseFocus(event) {
		if (this.editor === event.target) {
			event.stopPropagation();
			return;
		}

		this.syncContent();
	}

	async syncContent() {
		// remove initial paragraph if there is no content
		if (this.editor.childNodes.length === 1 && this.editor.childNodes[0].innerHTML === '<br>') {
			this.editor.childNodes[0].remove();
		}
		this.cleanMarkup(this.editorAnchor.childNodes, document);

		const content = this.toJSStreamRefIntelligence(this.getEditorContent());
		this.hasPendingChanges = false; // This must be set before we dispatch the update to the server so that any modifications while sending the data will be registered
		this.root.dispatchEvent(new CustomEvent('contentchanged', { bubbles: true, detail: { content: content } }));
		document.removeEventListener('mousedown', this.aboutToLoseFocus, true);
	}

	async handleKeyDown(event) {
		// contenteditible when empty, it needs a starting p tag to correctly generate paragraphs instead of textnodes or divs
		if (!this.editor.firstChild) {
			const initialNode = document.createElement('p');
			initialNode.innerHTML = '<br>';
			this.editor.appendChild(initialNode);
		}

		if (event.ctrlKey) {
			if (!plainText && event.key.toLowerCase() === 't') {
				event.preventDefault();
				event.stopImmediatePropagation();
				document.execCommand('strikeThrough', false, '');
				return false;
			} else if (plainText) {
				switch (event.key.toLowerCase()) {
					case 'b':
					case 'i':
					case 'u':
					case 't':
						event.preventDefault();
						event.stopImmediatePropagation();
				}
			}
		}

		setTimeout(() => this.selectionChange());
		return super.handleKeyDown(event);
	}

	async uploadEDocs(items) {
		let images = [];
		const currentSelection = this.getSelection();

		if (items && items.length > 0) {
			for (let item of items) {
				if (item.kind === 'file' && item.type.includes('image')) {
					let file = item.getAsFile();
					let image = {
						lastModified: file.lastModifiedDate.toISOString(),
						name: file.name,
						size: file.size,
						contentType: file.type,
						fileStream: DotNet.createJSStreamReference(file),
					};
					images.push(image);
				}
			}
			if (images.length > 0) {
				await this.dotNetObjectReference.invokeMethodAsync(
					'OnInsertImagesAsync',
					images,
					currentSelection?.start || 0,
					currentSelection?.end || 0
				);
			}
		}
	}

	async pastePostProcess(args) {
		await this.uploadEDocs(args.clipboardData.items);
	}

	async dropPostProcess(args) {
		await this.uploadEDocs(args.dataTransfer.items);
	}

	getEditorContent() {
		if (this.isReadOnly) {
			return null;
		}

		const content = this.editor?.innerHTML;

		// we're possibly trying to get the content whilst in an unloaded state, this can happen as a result of tab switching etc.
		if (isNullOrUndefined(content)) {
			console.warn('Could not get editor content while trying to emit an update');
			return null;
		}
		return content;
	}

	getEditorText() {
		return this.editor.innerText;
	}

	getDOMSelectionFromEditor() {
		return document.getSelection();
	}

	insertTab() {
		this.withRestorativeBoundaryCheck(() => {
			document.execCommand('insertHTML', false, '\t');
		});
	}

	getEditorBody() {
		return this.editor;
	}

	wtgEditorInsertFile(restrictToImages) {
		const fileType = restrictToImages ? [{ 'Image Files': ['.gif', '.bmp', '.jpg', '.tif', '.png'] }] : null;
		const currentSelection = this.getSelection();

		openFileDialog(false, fileType).then((file) => {
			this.dotNetObjectReference.invokeMethodAsync(
				'OnInsertFileAsync',
				file,
				currentSelection?.start || 0,
				currentSelection?.end || 0
			);
		});
	}

	async withReadOnly(action) {
		try {
			this.preventFocusoutDefault = true;
			this.editorAnchor.setAttribute('contenteditable', false);
			return await action();
		} finally {
			this.editorAnchor.setAttribute('contenteditable', true);
			this.preventFocusoutDefault = false;
		}
	}

	forceModifyAttribute(action) {
		let originalContentEditable = null;
		let element = null;
		try {
			if (this.editor !== null) {
				originalContentEditable = true;
				element = this.editor;
			} else {
				originalContentEditable = this.root.getAttribute('contenteditable');
				element = this.root;
			}
			element.setAttribute('contenteditable', true);
			action();
		} finally {
			element.setAttribute('contenteditable', originalContentEditable);
		}
	}

	formatPainterButton() {
		if (this.formatPainterActive) {
			this.formatPainterActive = false;
			this.editorToolbarButtons.formatPainter?.classList?.remove('richtextbox__toolbar-item--active');

			document.removeEventListener('mouseup', this.formatPainterEvent);
		} else {
			this.formatPainterActive = true;
			this.editorToolbarButtons.formatPainter?.classList?.add('richtextbox__toolbar-item--active');

			this.formatPainterEvent = (e) => this.withRestorativeBoundaryCheck(() => this.formatPainterAction(e));
			document.addEventListener('mouseup', this.formatPainterEvent);

			this.formatPainterStyles = new FormatPainterStyles({
				bold: document.queryCommandValue('bold') === 'true',
				italic: document.queryCommandValue('italic') === 'true',
				underline: document.queryCommandValue('underline') === 'true',
				strikethrough: document.queryCommandValue('strikethrough') === 'true',
				color: document.queryCommandValue('foreColor'),
				fontName: document.queryCommandValue('fontName'),
				fontSize: getComputedStyle(document.getSelection().anchorNode.parentNode).fontSize,
			});
		}
	}

	formatPainterAction(event) {
		if (!this.root.querySelector('.richtextbox__editoranchor').contains(event.target)) {
			return false;
		}
		if (!this.isCurrentSelectionTextboxOfRtb()) {
			return false;
		}

		this.formatPainterActive = false;
		this.editorToolbarButtons.formatPainter?.classList?.remove('richtextbox__toolbar-item--active');
		document.removeEventListener('mouseup', this.formatPainterEvent);

		this.changeFont(this.formatPainterStyles, true);
	}

	setSelectionContent(html) {
		this.editor?.insertContent(html);
	}

	static hitEnterOnLink(event) {
		if (event.code !== 'Enter') {
			return false;
		}
		let parent = window.getSelection().focusNode?.parentElement;
		if (isNullOrUndefined(parent) || parent.nodeName !== 'A' || !parent.href) {
			return false;
		}

		let caretOnStartOfLink = window.getSelection().focusOffset === 0;
		let caretOnEndOfLink = parent.text && parent.text.length === window.getSelection().focusOffset;
		let shouldOpenLink = !caretOnStartOfLink && !caretOnEndOfLink;

		if (shouldOpenLink) {
			event.preventDefault();
			window.open(parent.href);
		}

		return shouldOpenLink;
	}

	isCurrentSelectionTextboxOfRtb() {
		let currentSelection = document.getSelection();
		if (currentSelection.rangeCount > 0) {
			let range = currentSelection.getRangeAt(0);
			let containerElement = range.commonAncestorContainer;

			// Ensure that the containerElement is actually an element, not just a text node. A text node does not have closest().
			if (containerElement.nodeType === Node.TEXT_NODE) {
				containerElement = containerElement.parentElement;
			}

			// Check if the containerElement is a descendant of richtextbox__editoranchor.
			// Note that it can be the rtb itself if we select multiple lines
			const editorAnchor = this.root.querySelector('.richtextbox__editoranchor');
			return editorAnchor ? editorAnchor.contains(containerElement) : false;
		}
		return false;
	}

	queryCommandValue(command) {
		const domSelection = this.getDOMSelection();
		let target = null;

		if (domSelection.focusNode.nodeName === '#text') {
			target = domSelection.focusNode.parentNode;
		} else {
			target = domSelection.focusNode;
		}

		const styleMap = target.computedStyleMap();

		switch (command) {
			case 'FontSize':
			case 'font-size':
			case 'fontsize':
				return styleMap.get('font-size').toString();
			case 'FontName':
			case 'font-name':
			case 'fontname':
				return styleMap.get('font-family').toString();
		}

		return null;
	}

	scrollAfterPaste() {
		const selection = this.getSelection();
		if (selection === null) return;

		this.scrollToCaret(selection.start, selection.end, this.editorAnchor.textContent.length);
	}

	scrollToCaret(start, end, textLength) {
		const rtbElement = this.isReadOnly
			? (this.dataElement ?? this.root.querySelector('.richtextbox__data'))
			: (this.editor ?? this.root.querySelector('.richtextbox__editoranchor'));

		if (isNullOrUndefined(rtbElement)) {
			return;
		}

		if (start === end && start === 0) {
			rtbElement.scrollTop = 0;
		} else if (start === end && start === textLength) {
			rtbElement.scrollTop = rtbElement.scrollHeight;
		} else {
			this.setSelection({ start, end });
			let anchorNode = this.getDOMSelection().anchorNode;
			if (isNullOrUndefined(anchorNode)) {
				return;
			}

			if (anchorNode.nodeType === Node.TEXT_NODE) {
				anchorNode = anchorNode.parentNode;
			}
			anchorNode.scrollIntoView({ block: 'nearest' });
		}
	}
}

class FormatPainterStyles {
	constructor(props) {
		let {
			bold,
			italic,
			underline,
			strikethrough,
			color,
			fontName,
			fontSize,
			listItem,
			orderedList,
			unorderedList,
		} = props;
		this.bold = bold;
		this.italic = italic;
		this.underline = underline;
		this.strikethrough = strikethrough;
		this.color = color;
		this.fontName = fontName;
		this.fontSize = fontSize;
		this.listItem = listItem;
		this.orderedList = orderedList;
		this.unorderedList = unorderedList;
	}
}

const attributeTruthiness = (value) => {
	return typeof value !== 'undefined' && (value === 'true' || value === '');
};

const isNullOrUndefined = (a) => a === null || a === undefined;
const isNonNull = (a) => !isNullOrUndefined(a);

let RichTextBoxClient = WtgEditorBackedClient;

export const initialize = async (element, content, dotNetObjectReference, initializeParameters) => {
	if (!element) {
		console.warn('Warning: supplied element is null or undefined:', element);
	} else if (!dotNetObjectReference) {
		console.warn('Warning: supplied dotNetObjectReference is null or undefined:', dotNetObjectReference);
	} else {
		isToolBarVisible = initializeParameters.enableToolBar ?? true;
		const enableStyleShortcuts = initializeParameters.enableStyleShortcuts ?? false;
		plainText = !isToolBarVisible && !enableStyleShortcuts;
		await RichTextBoxClient.getOrCreateInstance(
			element,
			content,
			dotNetObjectReference,
			initializeParameters.winzorControlId,
			initializeParameters.font
		);
		_WTG.RichTextBoxClient = RichTextBoxClient;
	}
};

export const clearUndoManager = async (winzorControlId) => {
	let client = await RichTextBoxClient.tryGetInstance(winzorControlId);
	client?.clearUndoManager();
};

export const registerF5Inserter = async (winzorControlId) => {
	let client = await RichTextBoxClient.tryGetInstance(winzorControlId);
	client?.registerF5Inserter();
};

export const insertContent = async (winzorControlId, content) => {
	let client = await RichTextBoxClient.tryGetInstance(winzorControlId);
	client?.editor?.insertContent(content);
};

export const setSelection = async (element, start, end) => {
	if (isNullOrUndefined(element) || isNullOrUndefined(start) || isNullOrUndefined(end)) {
		return;
	}
	const winzorControlId = element.getAttribute('data-winzor-control-id');
	let client = await RichTextBoxClient.tryGetInstance(winzorControlId);

	client?.setSelection({ start: start, end: end });
};

export const setSelectionContent = async (winzorControlId, start, end, html) => {
	if (
		isNullOrUndefined(winzorControlId) ||
		isNullOrUndefined(start) ||
		isNullOrUndefined(end) ||
		isNullOrUndefined(html)
	) {
		return;
	}
	let client = await RichTextBoxClient.tryGetInstance(winzorControlId);
	if (!client.isTyping) {
		client?.setSelection({ start: start, end: end });
	}
	client?.setSelectionContent(html);
};

export const setSelectionColor = async (winzorControlId, start, end, color) => {
	if (
		isNullOrUndefined(winzorControlId) ||
		isNullOrUndefined(start) ||
		isNullOrUndefined(end) ||
		isNullOrUndefined(color)
	) {
		return;
	}
	let client = await RichTextBoxClient.tryGetInstance(winzorControlId);
	let selection = client?.getSelection();
	client?.setSelection({ start: start, end: end });
	client?.setForeColor(color);
	client?.setSelection(selection);
};

export const setSelectionFont = async (winzorControlId, start, end, font) => {
	if (
		isNullOrUndefined(winzorControlId) ||
		isNullOrUndefined(start) ||
		isNullOrUndefined(end) ||
		isNullOrUndefined(font)
	) {
		return;
	}
	let client = await RichTextBoxClient.tryGetInstance(winzorControlId);
	let selection = client?.getSelection();
	client?.setSelection({ start: start, end: end });
	let newFont = new FormatPainterStyles({
		bold: font.bold,
		italic: font.italic,
		underline: font.underline,
		strikethrough: font.strikeout,
		fontName: font.name,
		fontSize: font.size + 'pt',
	});
	client?.setFont(newFont);
	client?.setSelection(selection);
};

export const deleteRichTextBoxClient = async (winzorControlId) => {
	if (isNullOrUndefined(winzorControlId)) {
		return;
	}
	await RichTextBoxClient.tryDeleteInstance(winzorControlId);
};

export const getEditorContent = async (winzorControlId) => {
	if (isNullOrUndefined(winzorControlId)) {
		return;
	}
	const client = await RichTextBoxClient.tryGetInstance(winzorControlId);

	return client?.toJSStreamRefIntelligence(client.getEditorContent());
};

export const setEditorContent = async (winzorControlId, content) => {
	if (isNullOrUndefined(winzorControlId)) {
		return;
	}

	const client = await RichTextBoxClient.tryGetInstance(winzorControlId);

	client?.applyDataFromServer(content);
};

export const focusEditor = async (winzorControlId) => {
	if (isNullOrUndefined(winzorControlId)) {
		return;
	}
	const client = await RichTextBoxClient.tryGetInstance(winzorControlId);

	if (client?.getEditorBody() === null) {
		await client.editorReadyPromise;
	}
	client?.getEditorBody()?.focus();
};

export const scrollToCaret = async (element, start, end, textLength) => {
	if (
		isNullOrUndefined(element) ||
		isNullOrUndefined(start) ||
		isNullOrUndefined(end) ||
		isNullOrUndefined(textLength)
	) {
		return;
	}
	const winzorControlId = element.getAttribute('data-winzor-control-id');
	const client = await RichTextBoxClient.tryGetInstance(winzorControlId);

	client?.scrollToCaret(start, end, textLength);
};

export const getCurrentSelectionForeColor = async (winzorControlId) => {
	if (isNullOrUndefined(winzorControlId)) {
		return;
	}
	const client = await RichTextBoxClient.tryGetInstance(winzorControlId);

	return client?.editorToolbarButtons.color.value;
};

export const setCurrentSelectionForeColor = async (winzorControlId, color) => {
	if (isNullOrUndefined(winzorControlId) || isNullOrUndefined(color)) {
		return;
	}
	const client = await RichTextBoxClient.tryGetInstance(winzorControlId);

	client?.setForeColor(color);
};

class SelectionFinder {
	constructor(root) {
		this.root = root;
		this.caret = -1;
		this.current = { node: this.root, offset: 0 };
		this.stack = [];
	}

	advance() {
		if (this.current.offset === this.current.node.childNodes?.length) {
			if (SelectionFinder.isValidText(this.current.node)) {
				this.caret += this.current.node.length;
			} else if (SelectionFinder.isVisibleBr(this.current.node)) {
				++this.caret;
			}
			this.current = this.stack.pop();
		} else {
			let nextNode = this.current.node.childNodes[this.current.offset];
			if (SelectionFinder.isContentBlock(nextNode)) {
				++this.caret;
			} else if (this.caret < 0 && SelectionFinder.isValidText(nextNode)) {
				this.caret = 0;
			}
			++this.current.offset;
			this.stack.push(this.current);
			this.current = { node: nextNode, offset: 0 };
		}
	}

	advanceToDOMBoundary(node, offset) {
		while (this.current) {
			if (this.current.node === node) {
				switch (this.current.node.nodeType) {
					case Node.ELEMENT_NODE:
						if (this.current.offset === offset) {
							return;
						}
						break;
					case Node.TEXT_NODE:
						return;
				}
			}
			this.advance();
		}
	}

	advanceToCaret(position) {
		while (this.current) {
			switch (this.current.node.nodeType) {
				case Node.ELEMENT_NODE:
					if (this.caret >= position) {
						return;
					}
					break;
				case Node.TEXT_NODE:
					if (this.caret + this.current.node.length >= position) {
						return;
					}
					break;
			}
			this.advance();
		}
	}

	DOMBoundaryFor(caret) {
		this.advanceToCaret(caret);
		if (!this.current) {
			return { node: this.root, offset: this.root.childNodes.length };
		} else if (this.current.node.nodeType === Node.TEXT_NODE) {
			return { node: this.current.node, offset: caret - this.caret };
		} else {
			return this.current;
		}
	}

	caretFor(node, offset) {
		this.advanceToDOMBoundary(node, offset);
		if (this.current && this.current.node.nodeType === Node.TEXT_NODE) {
			return this.caret + offset;
		} else {
			return Math.max(this.caret, 0);
		}
	}

	DOMRangeFor({ start, end }) {
		let node, offset;
		let result = document.createRange();
		({ node, offset } = this.DOMBoundaryFor(start));
		result.setStart(node, offset);
		({ node, offset } = this.DOMBoundaryFor(end));
		result.setEnd(node, offset);
		return result;
	}

	caretRangeFor(domRange) {
		return {
			start: this.caretFor(domRange.startContainer, domRange.startOffset),
			end: this.caretFor(domRange.endContainer, domRange.endOffset),
		};
	}

	static isContentBlock(node) {
		switch (node.tagName) {
			case 'LI':
			case 'P':
				return true;
			default:
				return false;
		}
	}

	static isVisibleBr(node) {
		if (node.tagName === 'BR') {
			do {
				let lastSibling = node.parentNode.lastChild;
				if (
					node !== lastSibling &&
					(node !== lastSibling.previousSibling ||
						lastSibling.nodeType !== Node.ELEMENT_NODE ||
						!['OL', 'UL'].some((tagName) => lastSibling.tagName === tagName))
				) {
					return true;
				} else {
					node = node.parentNode;
				}
			} while (!SelectionFinder.isContentBlock(node));
		}
		return false;
	}

	static isValidText(node) {
		return node.nodeType === Node.TEXT_NODE && node.parentElement.closest('[data-mce-bogus]') === null;
	}
}

function IsLinkClick(e) {
	return e.srcElement.tagName === 'A' && !!e.srcElement.attributes.href && e.button === 0;
}

function normalizeKeyboardEventKeys(e) {
	let keyCodes = '';
	if (e.ctrlKey) {
		keyCodes += 'Control+';
	}
	if (e.altKey) {
		keyCodes += 'Alt+';
	}
	if (e.shiftKey) {
		keyCodes += 'Shift+';
	}
	return keyCodes + e.key;
}

let _WTG;
if (window.WTG) {
	_WTG = window.WTG;
} else {
	_WTG = {};
	window.WTG = _WTG;
}
_WTG.RichTextBoxClient = RichTextBoxClient;
_WTG.SelectionFinder = SelectionFinder;
