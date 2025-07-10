/*
	Register Event Listener
	Invoked from the server to register an event on the given element with the given type.
	The eventData is stored locally to be used by registered event callbacks when validating if the event should be fired.
	Throws an error if the given element does not have a Winzor Control Id.
*/
export const registerEventListener = (callback, eventType, element, eventData) => {
	if (!registeredEventHandlers[eventType]) {
		throw new Error(`There is not event handler registered for event type ${eventType}.`);
	}

	const elementId = getElementId(element);
	if (!elementId) {
		throw new Error('Element does not have a Winzor Control Id.');
	}

	addEventListener(eventType, element);
	const eventId = getNextEventId();

	window.winzor = {
		...window?.winzor,
		events: {
			...window?.winzor?.events,
			[eventType]: {
				...window?.winzor?.events?.[eventType],
				[elementId]: {
					...window?.winzor?.events?.[eventType]?.[elementId],
					registeredEvents: {
						...window?.winzor?.events?.[eventType]?.[elementId]?.registeredEvents,
						[eventId]: {
							...eventData,
							_callback: callback,
						},
					},
				},
			},
		},
	};

	return {
		unregister: () => {
			const registeredEvents = getRegisteredEvents(eventType, elementId);
			if (registeredEvents && eventId in registeredEvents) {
				registeredEvents[eventId]?._callback?.dispose();
				delete registeredEvents[eventId];
				if (Object.keys(registeredEvents).length === 0) {
					removeEventListener(eventType, element);
				}
			}
		},
	};
};

/*
	Register Global Event Listener
	Invoked from the server to register an event on document with the given type.
	The eventData is stored locally to be used by registered event callbacks when validating if the event should be fired.
*/
export const registerGlobalEventListener = (callback, eventType, eventData) => {
	return registerEventListener(callback, eventType, document, eventData);
};

const getNextEventId = () => {
	window.winzor = {
		...window?.winzor,
		events: {
			...window?.winzor?.events,
			_lastEventId: window?.winzor?.events?._lastEventId + 1 || 0,
		},
	};
	return window.winzor.events._lastEventId;
};

/*
	Add Event Listener
	Attempts to add an event listener to the supplied element with the event type if one does not already exist.
*/
const addEventListener = (eventType, element) => {
	const elementId = getElementId(element);
	if (!isEventListenerAdded(eventType, elementId)) {
		element.addEventListener(eventType, handleEventAsync);
		setIsEventListenerAdded(eventType, elementId, true);
	}
};

/*
	Remove Event Listener
	Attempts to remove an event listener from the supplied element with the event type if one exists.
*/
const removeEventListener = (eventType, element) => {
	const elementId = getElementId(element);
	if (isEventListenerAdded(eventType, elementId)) {
		element.removeEventListener(eventType, handleEventAsync);
		setIsEventListenerAdded(eventType, elementId, false);
	}
};

const isEventListenerAdded = (eventType, elementId) =>
	Boolean(window?.winzor?.events?.[eventType]?.[elementId]?._isEventListenerAttached);
const setIsEventListenerAdded = (eventType, elementId, value) => {
	window.winzor = {
		...window?.winzor,
		events: {
			...window?.winzor?.events,
			[eventType]: {
				...window?.winzor?.events?.[eventType],
				[elementId]: {
					...window?.winzor?.events?.[eventType]?.[elementId],
					_isEventListenerAttached: value,
				},
			},
		},
	};
};

/*
	Primary Event Handler
	Event handler which is called from the registered event listeners.
	Will find all registered events that match the event type and element and run the event handler callback for the event type.
	If the event is valid, will callback to the server to invoke the event.
*/
const handleEventAsync = async (e) => {
	const elementId = getElementId(e.currentTarget);
	if (!elementId) {
		throw new Error('Element does not have a Winzor Control Id.');
	}

	const registeredEvents = Object.entries(getRegisteredEvents(e.type, elementId));
	if (registeredEvents.length === 0) {
		// No event listeners were added for this event type and element. Therefore we can remove the event listener.
		removeEventListener(e.type, e.currentTarget);
		return;
	}

	const eventHandler = registeredEventHandlers[e.type];
	if (!eventHandler) {
		throw new Error(`No registered event handler could be found for event type ${e.type}.`);
	}

	await Promise.all(
		registeredEvents.map(async (eventListener) => {
			const [_, data] = eventListener;
			if (eventHandler?.shouldHandle(e, data)) {
				if (data.shouldPreventDefault) {
					e.preventDefault();
				}
				await data?._callback?.invokeMethodAsync('InvokeCallbackAsync', eventHandler.makeArg(e));
			}
		})
	);
};

const getRegisteredEvents = (eventType, elementId) =>
	window?.winzor?.events?.[eventType]?.[elementId]?.registeredEvents || {};
const getElementId = (element) => (element === document ? 'document' : element.dataset.winzorControlId);

/*
	Event Handler Callbacks
	To add support for additional events, add a callback function to validate if the event should be fired.
	The function should accept the event as well as the registered data.
	The function should return a bool with a value of true if the event should be fired.
*/
const keyEventHandler = {
	shouldHandle: (event, registeredEventData) => {
		return (
			event.key?.toLowerCase() === registeredEventData.key?.toLowerCase() &&
			event.altKey === registeredEventData.altKey
		);
	},
	makeArg: () => null,
};

const mouseEventHandler = {
	shouldHandle: () => true,
	makeArg: (e) => {
		const properties = [
			'detail',
			'screenX',
			'screenY',
			'clientX',
			'clientY',
			'offsetX',
			'offsetY',
			'pageX',
			'pageY',
			'movementX',
			'movementY',
			'button',
			'buttons',
			'ctrlKey',
			'shiftKey',
			'altKey',
			'metaKey',
			'type',
		];
		let args = {};
		for (let p of properties) {
			args[p] = e[p];
		}
		return args;
	},
};

/*
	Registered Event Handlers
	To add support for additional events, add an item to this object with a callback to validate if the event should be fired.
*/
const registeredEventHandlers = {
	keydown: keyEventHandler,
	keypress: keyEventHandler,
	click: mouseEventHandler,
	mousedown: mouseEventHandler,
	mousemove: mouseEventHandler,
	mouseup: mouseEventHandler,
};
