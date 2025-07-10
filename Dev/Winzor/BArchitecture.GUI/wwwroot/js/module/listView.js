const guideWidth = 3;
const minWidth = 3;

export const resizeColumn = (dotnet, args, column) => {
	let guide;
	const offsetStart = column.offsetWidth - getPageX(args);
	const table = column.parentElement.parentElement.parentElement;
	const dataGrid = table.parentElement;
	const dataGridWidth = dataGrid.offsetWidth;
	table.style.cursor = 'ew-resize';

	const onMouseMove = (e) => {
		if (e.buttons !== 1) {
			removeMouseEvents();
			return;
		}

		let newWidth = getWidth(e, offsetStart);
		let leftOffset = getLeftOffset(column, dataGrid, newWidth, dataGridWidth);
		guide.style.left = `${leftOffset}px`;
	};

	const onMouseUp = (e) => {
		e.stopPropagation();
		removeMouseEvents();
		table.style.cursor = '';
		guide?.remove();
		guide = null;
		let newWidth = getWidth(e, offsetStart);
		let leftOffset = getLeftOffset(column, dataGrid, newWidth, dataGridWidth);
		let columnIndex = Array.from(column.parentElement.children).findIndex((e) => e === column);
		if (leftOffset + guideWidth === dataGridWidth) {
			newWidth = getWidthByOffset(column, dataGrid, leftOffset);
		}
		dotnet.invokeMethodAsync('SetColumnSize', columnIndex, newWidth);
	};

	const removeMouseEvents = () => {
		document.removeEventListener('mousemove', onMouseMove);
		document.removeEventListener('mouseup', onMouseUp);
	};

	document.addEventListener('mousemove', onMouseMove);
	document.addEventListener('mouseup', onMouseUp);
	guide = createGuide(column, dataGrid);
	table.appendChild(guide);
};

const getPageX = (e) => {
	return Math.floor(e.pageX);
};

const getWidth = (e, startOffset) => {
	return Math.max(startOffset + getPageX(e), minWidth);
};

const getLeftOffset = (column, dataGrid, columnWidth, dataGridWidth) => {
	let leftOffset = column.offsetLeft + columnWidth - guideWidth - dataGrid.scrollLeft;
	if (leftOffset < 0) {
		return 0;
	}
	return leftOffset > dataGridWidth ? dataGridWidth - guideWidth : leftOffset;
};

const getWidthByOffset = (column, dataGrid, leftOffset) => {
	return leftOffset - column.offsetLeft + guideWidth + dataGrid.scrollLeft;
};

const createGuide = (column, dataGrid) => {
	let topOffset = column.offsetHeight + 1;
	let leftOffset = getLeftOffset(column, dataGrid, column.clientWidth, dataGrid.offsetWidth);
	let height = dataGrid.clientHeight - topOffset + 1;
	let guide = document.createElement('div');
	guide.setAttribute(
		'style',
		`position:absolute;width:3px;height:${height}px;left:${leftOffset}px;top:${topOffset}px;z-index:10;`
	);
	guide.classList.add('splitter__guide');
	return guide;
};
