export const showCalendar = async (element) => {
	await window.cargoWiseClient?.requestUserActivation();
	element.showPicker();
	element.setAttribute('data-calendar-open', true);
	element.addEventListener(
		'blur',
		() => {
			element.setAttribute('data-calendar-open', false);
		},
		{ once: true }
	);
};
