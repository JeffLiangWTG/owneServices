export const openUrl = (url, newWindow) => {
	if (newWindow) {
		try {
			window.open(url);
		} catch {
			window.open('about:blank');
		}
	} else {
		window.location = url;
	}
};
