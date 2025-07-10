const parsePointerEvent = (event) => {
	return {
		...parseMouseEvent(event),
		pointerId: event.pointerId,
		width: event.width,
		height: event.height,
		pressure: event.pressure,
		tiltX: event.tiltX,
		tiltY: event.tiltY,
		pointerType: event.pointerType,
		isPrimary: event.isPrimary,
	};
}

const parseMouseEvent = (event) => {
	return {
		detail: event.detail,
		screenX: event.screenX,
		screenY: event.screenY,
		clientX: event.clientX,
		clientY: event.clientY,
		offsetX: event.offsetX,
		offsetY: event.offsetY,
		pageX: event.pageX,
		pageY: event.pageY,
		movementX: event.movementX,
		movementY: event.movementY,
		button: event.button,
		buttons: event.buttons,
		ctrlKey: event.ctrlKey,
		shiftKey: event.shiftKey,
		altKey: event.altKey,
		metaKey: event.metaKey,
		type: event.type,
	};
}

export const subscribeToPointerEventsOutsideElement = (element, onPointerMoveCallback, onPointerUpCallback) => {
	if (element == null)
	{
		return;
	}

	const onPointerMove = async (event) => {
		if (!element.contains(event.target)) {
			await onPointerMoveCallback?.invokeMethodAsync("InvokeCallbackAsync", parsePointerEvent(event));
		}
	}

	const onPointerUp = async (event) => {
		document.removeEventListener("pointermove", onPointerMove);
		document.removeEventListener("pointerup", onPointerUp);
		if (!element.contains(event.target)) {
			await onPointerUpCallback?.invokeMethodAsync("InvokeCallbackAsync", parsePointerEvent(event));
		}
	}

	document.addEventListener("pointermove", onPointerMove)
	document.addEventListener("pointerup", onPointerUp)
}
