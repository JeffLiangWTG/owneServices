export const scrollIntoView = (input) => {
	if (input instanceof HTMLElement) {
		input.scrollIntoView({ block: 'nearest' });
	}
};

export const scrollIntoViewAndFocus = (input) => {
	if (input instanceof HTMLElement) {
		input.querySelector('.treeview__nodetext')?.focus();
		input.scrollIntoView({ block: 'nearest' });
	}
};

export const copyTreeNodeContent = async (input) => {
	if (input instanceof HTMLElement) {
		const content = input.querySelector('.treeview__nodetext')?.innerText || '';
		await window.cargoWiseClient?.requestUserActivation();
		await navigator.clipboard.writeText(content);
	}
};

export const initialize = (treeview, dotNet) => {
	if (!treeview || !dotNet) {
		return;
	}

	treeview.addEventListener('keydown', (e) => handleKeyDown(e, dotNet));

	dotNet.invokeMethodAsync('UpdateNodesPerPage', calculateNodesPerPage(treeview));
	new ResizeObserver(() => {
		dotNet.invokeMethodAsync('UpdateNodesPerPage', calculateNodesPerPage(treeview));
	}).observe(treeview);
};

function handleKeyDown(e, dotNet) {
	if (
		!e.ctrlKey &&
		['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'PageUp', 'PageDown', 'Home', 'End'].indexOf(e.key) > -1
	) {
		dotNet.invokeMethodAsync('HandleNavigationKeyAsync', {
			key: e.key,
			code: e.code,
			location: e.location,
			repeat: e.repeat,
			ctrlKey: e.ctrlKey,
			shiftKey: e.shiftKey,
			altKey: e.altKey,
			metaKey: e.metaKey,
			type: e.type,
		});
		e.stopPropagation();
		e.preventDefault();
	}
}

function calculateNodesPerPage(treeview) {
	if (!treeview || !(treeview instanceof HTMLElement)) {
		return 0;
	}
	const height = treeview.clientHeight;
	const oneNodeSize = treeview.querySelector('.treeview__nodecontent')?.getBoundingClientRect();
	return oneNodeSize && oneNodeSize.height > 0 ? Math.trunc(height / oneNodeSize.height) : 0;
}
