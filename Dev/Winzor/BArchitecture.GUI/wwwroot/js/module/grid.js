import { updateTextAreaHeight } from '../../WinzorFramework.lib.module.js';

const guideWidth = 3;
const minWidth = 3;
const dataGridBorderWidth = 1;
// these color should be dynamic, for now, write it as fixed.
const columnReadonlyBackColor = '#d7dadc';
const columnReadonlyColor = 'gray';
const selectedRowClassName = 'datagrid__row--selected';
const selectedAllClassName = 'datagrid__rows--selected';

export const initializeGridEvents = (grid, dotNet) => {
	if (!grid || !(grid instanceof HTMLElement)) {
		return; // DOM may have been updated causing the grid to be removed
	}

	let lastExecution = 0;
	let lastWheel = 0;
	const throttleIntervalMillisecond = 100;
	const clickScrollThrottleIntervalMillisecond = 200;
	const debounceTimeoutMillisecond = 50;
	let debounceTimeout;

	let isAwaitingScrollRender = false;
	let isFirstScroll = true;
	let wheeling = false;

	const handleScrollActions = async () => {
		updateTextAreaHeight(grid);
		isAwaitingScrollRender = true;
		lastExecution = Date.now();
		await dotNet.invokeMethodAsync('OnScrollAsync', grid.scrollTop, grid.scrollLeft);
		isAwaitingScrollRender = false;
	};

	grid.addEventListener('scroll', (e) => {
		e.stopPropagation();

		// If the scroll event is triggered by a click instead of a wheel, we should handle it specially.
		// When it is scrolling by click, we should skip scroll events between 0ms and clickScrollThrottleIntervalMillisecond ms,
		// this is to make sure that one quick click trigger scroll action only once.
		if (isFirstScroll) {
			if (Date.now() - lastWheel <= 50) {
				wheeling = true;
			}

			if (!wheeling) {
				lastExecution = Date.now();
			}
			isFirstScroll = false;
		}

		let throttleInterval = wheeling ? throttleIntervalMillisecond : clickScrollThrottleIntervalMillisecond;
		if (!isAwaitingScrollRender && Date.now() - lastExecution >= throttleInterval) {
			handleScrollActions();
		}

		debounceScroll();
	});

	function debounceScroll() {
		clearTimeout(debounceTimeout);

		if (isAwaitingScrollRender) {
			debounceTimeout = setTimeout(debounceScroll, debounceTimeoutMillisecond);
		} else {
			debounceTimeout = setTimeout(() => {
				handleScrollActions();
				isFirstScroll = true;
				wheeling = false;
			}, debounceTimeoutMillisecond);
		}
	}

	grid.addEventListener('wheel', () => {
		lastWheel = Date.now();
	});

	let isGridScrollClick = false;
	let firstClickIsTextBox = false;
	grid.addEventListener('mousedown', (e) => {
		if (e.offsetX > e.target.clientWidth || e.offsetY > e.target.clientHeight) {
			e.stopPropagation();
		}

		if (e.detail === 1) {
			firstClickIsTextBox = e.target.classList.contains('textbox');
		}

		if (e.target.classList.contains('datagrid')) {
			const mouseX = e.clientX;
			const mouseY = e.clientY;
			const rect = e.target.getBoundingClientRect();
			const yDiff = mouseY - rect.top;
			const xDiff = mouseX - rect.left;
			if (yDiff > e.target.clientHeight || xDiff > e.target.clientWidth) {
				// if focus is outside of grid(click scroll bar)
				isGridScrollClick = true;
			}
		}
	});

	grid.addEventListener('dblclick', (e) => {
		if (!firstClickIsTextBox && e.target.classList.contains('textbox')) {
			const range = document.caretPositionFromPoint(e.clientX, e.clientY);

			if (range) {
				const sel = window.getSelection();
				sel.removeAllRanges();
				e.target.focus();
				e.target.setSelectionRange(range.offset, range.offset);
			}
		}
	});

	grid.addEventListener('focusin', (e) => {
		if (e.target.className === 'datagrid' && isGridScrollClick) {
			e.stopPropagation();
			grid.blur();
		}
		isGridScrollClick = false;
	});

	grid.hasVertScrollbar = grid.scrollHeight > grid.clientHeight;
	grid.hasHorizScrollbar = grid.scrollWidth > grid.clientWidth;

	const scrollbarMonitor = () => {
		const vScroll = grid.scrollHeight > grid.clientHeight;
		const hScroll = grid.scrollWidth > grid.clientWidth;
		if (vScroll !== grid.hasVertScrollbar || hScroll !== grid.hasHorizScrollbar) {
			grid.hasVertScrollbar = vScroll;
			grid.hasHorizScrollbar = hScroll;
			dotNet.invokeMethodAsync('OnScrollbarsChangedAsync', vScroll, hScroll);
		}
	};
	grid.resizeObserver = new ResizeObserver(scrollbarMonitor).observe(grid);
	dotNet.invokeMethodAsync('OnScrollbarsChangedAsync', grid.hasVertScrollbar, grid.hasHorizScrollbar);

	grid.mutationObserver = new MutationObserver(observerInputNode);
	grid.mutationObserver.observe(grid, { childList: true, subtree: true });
};

export const reorderColumn = (args, column, dragMaskBackColor) => {
	let dragMask;
	const columnsHeader = column.parentElement;
	const dataGrid = findParentElement(columnsHeader, 'datagrid');
	const offsetX = args.offsetX + column.offsetLeft;

	const createDragMask = (column) => {
		const maskWidth = column.offsetWidth;
		const maskHeight = column.offsetHeight + 2;
		const maskleft = offsetX;

		dragMask = column.cloneNode(true);
		dragMask.setAttribute(
			'style',
			`position:absolute;pointer-events:none;width:${maskWidth}px;height:${maskHeight}px;left:${maskleft}px;padding-top:2px;`
		);
		dragMask.setAttribute('class', 'dragMask');
		dragMask.style.backgroundColor = dragMaskBackColor;
		dragMask.style.color = columnReadonlyColor;
		columnsHeader.appendChild(dragMask);

		column.style.border = '1px solid black';
		column.style.color = columnReadonlyColor;
		column.style.backgroundColor = columnReadonlyBackColor;

		dataGrid.style.overflow = 'hidden';

		columnsHeader.classList.add('datagrid__column--dragged');
		columnsHeader.style.setProperty('--column-header-color', columnReadonlyColor);
		columnsHeader.style.setProperty('--column-header-backcolor', columnReadonlyBackColor);

		return dragMask;
	};

	const onDragStart = (e) => {
		if (!dragMask) {
			const columnSpan = e.target;
			const column = columnSpan.parentElement;
			dragMask = createDragMask(column, columnSpan);
		}

		e.dataTransfer.effectAllowed = 'move';

		// hide default drag effect
		const ctr = document.createElement('div');
		ctr.style.display = 'none';
		e.dataTransfer.setDragImage(ctr, 0, 0);
	};

	const onDragOver = (e) => {
		e.preventDefault();
		e.stopPropagation();

		if (!dragMask) {
			return;
		}

		const currentX = e.clientX + dataGrid.scrollLeft - dataGrid.getBoundingClientRect().left;

		dragMask.style.left = `${currentX}px`;
	};

	const onDragEnd = (e) => {
		e.stopPropagation();

		const columnSpan = e.target;
		const column = columnSpan.parentElement;

		column.style.backgroundColor = '';
		column.style.color = '';
		column.style.border = '';

		dataGrid.style.overflow = '';

		if (dragMask) {
			dragMask.remove();
			dragMask = null;
		}

		removeMouseEventsAndClasses();
	};

	const onDragEnter = (e) => {
		e.preventDefault();
		e.stopPropagation();
	};

	const removeMouseEventsAndClasses = () => {
		columnsHeader.classList.remove('datagrid__column--dragged');
		columnsHeader.removeEventListener('dragstart', onDragStart);
		columnsHeader.removeEventListener('dragend', onDragEnd);
		columnsHeader.removeEventListener('dragenter', onDragEnter);
		dataGrid.removeEventListener('dragover', onDragOver);
	};

	columnsHeader.addEventListener('dragstart', onDragStart);
	columnsHeader.addEventListener('dragend', onDragEnd);
	columnsHeader.addEventListener('dragenter', onDragEnter);
	dataGrid.addEventListener('dragover', onDragOver);
};

export const resizeColumn = (dotnet, args, column) => {
	let guide;
	const offsetStart = column.offsetWidth - getPageX(args);
	const table = findParentElement(column, 'table');
	const dataGrid = findParentElement(table, 'datagrid');
	const dataGridWidth = dataGrid.offsetWidth;
	table.style.cursor = 'ew-resize';

	const onBlur = () => {
		removeGuide();
	};

	const onMouseMove = (e) => {
		if (e.buttons !== 1) {
			removeMouseEvents();
			return;
		}

		if (guide !== null) {
			const newWidth = getWidth(e, offsetStart);
			const leftOffset = getLeftOffset(column, dataGrid, newWidth, dataGridWidth);
			guide.style.left = `${leftOffset}px`;
		}
	};

	const onMouseUp = (e) => {
		e.stopPropagation();
		removeMouseEvents();
		dataGrid.removeEventListener('blur', onBlur);
		table.style.cursor = '';
		removeGuide();

		let newWidth = getWidth(e, offsetStart);
		const leftOffset = getLeftOffset(column, dataGrid, newWidth, dataGridWidth);
		const columnIndex = Array.from(column.parentElement.children).findIndex((e) => e === column) - 1;
		if (leftOffset + guideWidth === dataGridWidth) {
			newWidth = getWidthByOffset(column, dataGrid, leftOffset);
		}
		dotnet.invokeMethodAsync('SetColumnSize', columnIndex, newWidth);
	};

	const removeMouseEvents = () => {
		document.removeEventListener('mousemove', onMouseMove);
		document.removeEventListener('mouseup', onMouseUp);
	};

	const removeGuide = () => {
		guide?.remove();
		guide = null;
	};

	dataGrid.addEventListener('blur', onBlur);
	document.addEventListener('mousemove', onMouseMove);
	document.addEventListener('mouseup', onMouseUp);
	guide = createGuide(column, dataGrid);
	table.appendChild(guide);
};

const findParentElement = (currentElement, tagOrClass) => {
	while (currentElement.parentNode) {
		currentElement = currentElement.parentNode;
		if (
			currentElement.tagName.toLowerCase() === tagOrClass.toLowerCase() ||
			currentElement.classList.contains(tagOrClass)
		) {
			return currentElement;
		}
	}
	return null;
};

const getPageX = (e) => {
	return Math.floor(e.pageX);
};

const getWidth = (e, startOffset) => {
	return Math.max(startOffset + getPageX(e), minWidth);
};

const getWidthByOffset = (column, dataGrid, leftOffset) => {
	return leftOffset - column.offsetLeft + guideWidth + dataGrid.scrollLeft - dataGridBorderWidth - 1;
};

const getLeftOffset = (column, dataGrid, columnWidth, dataGridWidth) => {
	const leftOffset = column.offsetLeft + columnWidth - guideWidth;
	if (leftOffset < 0) {
		return 0;
	}
	return leftOffset > dataGridWidth + dataGrid.scrollLeft ? dataGridWidth - guideWidth : leftOffset;
};

const createGuide = (column, dataGrid) => {
	const topOffset = column.offsetHeight + 1;
	const leftOffset = getLeftOffset(column, dataGrid, column.clientWidth, dataGrid.offsetWidth);
	const height = dataGrid.clientHeight - topOffset + 1;
	let guide = document.createElement('div');
	guide.setAttribute(
		'style',
		`position:absolute;width:3px;height:${height}px;left:${leftOffset}px;top:${topOffset}px;z-index:10;`
	);
	guide.classList.add('splitter__guide');
	return guide;
};

export const setRowDragData = (grid, files) => {
	const firstColumns = grid?.querySelectorAll('tbody tr td:first-child');
	const visibleRows = grid?.querySelectorAll('tr.datagrid__row');

	visibleRows.forEach((row, index) => {
		const col = firstColumns[index];
		col.draggable = 'true';
		const rowNumber = row.getAttribute('data-row-number');
		col.ondragstart = (e) => {
			const file = files[rowNumber].split('|');
			const url = `application/octet-stream:${file[0]}:${location.protocol}//${location.host}/edoc/download/${file[0]}?docPK=${file[1]}&bizPK=${file[2]}`;
			e.dataTransfer.setData('DownloadURL', url);
			// hide default drag effect
			const ctr = document.createElement('div');
			ctr.style.display = 'none';
			e.dataTransfer.setDragImage(ctr, 0, 0);
		};
	});
};

export const multiSelectionRows = (dotnet, args, lastSelectedRow, dataGrid) => {
	const offsetY = args.pageY + dataGrid.scrollTop;
	const dragHeight = 4; // refer to Winform code
	let sendFlag = false;
	let selectEndRow = lastSelectedRow;

	const onMouseMove = (e) => {
		if (e.buttons !== 1) {
			removeMouseEvents();
			return;
		}

		let currentY = e.pageY + dataGrid.scrollTop - offsetY;
		if (Math.abs(currentY) > dragHeight) {
			sendFlag = true;
			let min = Math.min(e.pageY, e.pageY - currentY);
			let max = Math.max(e.pageY, e.pageY - currentY);
			let trs = dataGrid.querySelectorAll('table tbody tr');
			for (let i = 0; i < trs.length; i++) {
				let item = trs[i];
				let currentRow = getGridRowNumber(item);
				if (currentRow === lastSelectedRow || checkSelected(min, max, item)) {
					item.classList.add(selectedRowClassName);

					if (currentRow <= lastSelectedRow) {
						selectEndRow = Math.min(selectEndRow, currentRow);
					} else {
						selectEndRow = Math.max(selectEndRow, currentRow);
					}
				} else {
					item.classList.remove(selectedRowClassName);
				}
			}
		}
	};

	const onMouseUp = () => {
		if (sendFlag) {
			dotnet.invokeMethodAsync(
				'SelectRows',
				Math.min(selectEndRow, lastSelectedRow),
				Math.max(selectEndRow, lastSelectedRow)
			);
		}
		removeMouseEvents();
	};

	const bindMouseEvents = () => {
		// bind to document so that onMouseUp would be invoked even if mouse is outside of dataGrid
		document.addEventListener('mousemove', onMouseMove);
		document.addEventListener('mouseup', onMouseUp);
	};

	const removeMouseEvents = () => {
		document.removeEventListener('mousemove', onMouseMove);
		document.removeEventListener('mouseup', onMouseUp);
	};

	const checkSelected = (min, max, tr) => {
		const trRect = tr.getBoundingClientRect();
		if ((trRect.top > min && trRect.top < max) || (trRect.bottom > min && trRect.bottom < max)) {
			return true;
		}
		return false;
	};

	bindMouseEvents();
};

const getGridRowNumber = (rowElement) => {
	const currentRowNumber = rowElement.getAttribute('data-row-number');
	const result = parseInt(currentRowNumber);
	return isNaN(result) ? -1 : result;
};

export const setScrollTop = (dataGrid, scrollTop) => {
	if (dataGrid) {
		dataGrid.scrollTop = scrollTop;
	}
};

export const setScrollLeft = (dataGrid, scrollLeft) => {
	if (dataGrid) {
		dataGrid.scrollLeft = scrollLeft;
	}
};

export const selectAll = (dataGrid) => {
	if (dataGrid) {
		let tbody = dataGrid.querySelector('table tbody');
		const clear = () => {
			tbody.classList.remove(selectedAllClassName);
		};
		dataGrid.removeEventListener('mousedown', clear);
		tbody.classList.add(selectedAllClassName);
		dataGrid.addEventListener('mousedown', clear);
	}
};

const observerInputNode = (records) => {
	for (const record of records) {
		for (const addedNode of record.addedNodes) {
			if (addedNode.nodeName === 'INPUT' && addedNode.type === 'text') {
				addedNode.select();
				return;
			}
		}
	}
};
