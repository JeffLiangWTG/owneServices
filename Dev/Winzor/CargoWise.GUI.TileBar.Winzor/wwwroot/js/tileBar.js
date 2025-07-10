export const getBoundingBoxForFavoriteItem = (index) => {
	let droppedOverElement = document.querySelector(
		`.tilebarcontrol > .tilebarcontrol__tabcontent:last-child > .tilebarcontrol__sectionfavorite > ul > .tilebarcontrol__sectionitem:nth-child(${
			2 * index
		})`
	);
	let position = droppedOverElement.getBoundingClientRect();
	return [parseFloat(position.x), parseFloat(position.y), parseFloat(position.height), parseFloat(position.width)];
};

export const setDividerVisibility = (index) => {
	let allDividers = document.querySelectorAll(
		'.tilebarcontrol > .tilebarcontrol__tabcontent:last-child > .tilebarcontrol__sectionfavorite > ul > .tilebarcontrol__sectionreorderingdivider'
	);
	let allHiddenDividers = document.querySelectorAll(
		'.tilebarcontrol > .tilebarcontrol__tabcontent:last-child > .tilebarcontrol__sectionfavorite > ul > .tilebarcontrol__sectionreorderingdivider--hidden'
	);
	if (allHiddenDividers.length < allDividers.length - 1) {
		resetDividerVisibility();
		return;
	}

	let currentDivider = document.querySelector(
		`.tilebarcontrol > .tilebarcontrol__tabcontent:last-child > .tilebarcontrol__sectionfavorite > ul > .tilebarcontrol__sectionreorderingdivider:nth-child(${
			2 * index + 1
		})`
	);
	if (
		currentDivider.classList.contains('tilebarcontrol__sectionreorderingdivider--hidden') &&
		allHiddenDividers.length == allDividers.length - 1
	) {
		resetDividerVisibility();
	}
	currentDivider.classList.remove('tilebarcontrol__sectionreorderingdivider--hidden');
};

export const resetDividerVisibility = () => {
	let allDividers = document.querySelectorAll(
		'.tilebarcontrol > .tilebarcontrol__tabcontent:last-child > .tilebarcontrol__sectionfavorite > ul > .tilebarcontrol__sectionreorderingdivider'
	);
	allDividers.forEach((divider) => {
		divider.classList.add('tilebarcontrol__sectionreorderingdivider--hidden');
	});
};
