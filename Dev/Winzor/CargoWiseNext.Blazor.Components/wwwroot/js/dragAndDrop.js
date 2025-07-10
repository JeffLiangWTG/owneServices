/**
 * dragAndDrop module
 * ------------------
 * Enables basic drag-and-drop reordering functionality for elements with the given class.
 * Intended to work with Blazor via DotNetObjectReference for notifying the server on valid drop.
 *
 * Usage:
 *   1. Import the module in your Blazor component:
 *      [Inject] DragAndDropJsInterop? DragAndDropJsInterop { get; set; }
 *
 *   2. Call the `dragAndDrop.init` method from Blazor:
 *      await DragAndDropJsInterop.InitAsync("drop-area-class-name", "drag-over-class-name", DotNetObjectReference.Create<IDragAndDropView>(this));
 *
 * Parameters:
 *   @param {string} dropAreaClassname       - The class of elements that are valid drop targets (e.g., "draggable-item").
 *   @param {string} dragOverClassname       - A CSS class that will be added/removed during dragover for visual feedback.
 *   @param {DotNetObjectReference} dotNetHelper - A Blazor .NET reference that must expose an `OnDrop(dragIndex, dropIndex)` method (see `IDragAndDropView`).
 *
 *   3. To clean up:
 *      await module.InvokeVoidAsync("dragAndDrop.teardown");
 *
 * Requirements:
 *   - Each draggable element must have:
 *       - The attribute: [draggable="true"]
 *       - A numeric data-index attribute: data-index="0", "1", "2", etc.
 *           <div draggable="true" data-index="0">1</div>
 *
 * Behavior:
 *   - Only elements with [draggable="true"] will be draggable.
 *   - On drop over a valid target, it calls `OnDrop(dragIndex, dropIndex)` in the .NET component.
 *   - Adds/removes `dragOverClassname` to highlight drop targets visually.
 *   - Prevents drop on invalid areas and ensures ordering is only triggered when dropped over another valid target.
 */
export const dragAndDrop = (() => {
	let draggedItem = null;
	let dropAreaSelector = null;
	let dragOverClassname = null;
	let dotNetObjectRef = null;

	const listeners = [];

	const isValidTarget = (el) =>
		el && el !== draggedItem && el.matches(dropAreaSelector);

	const getDraggable = (e) =>
		e.target.closest('[draggable="true"]');

	const getDropTarget = (e) =>
		e.target.closest(dropAreaSelector);

	const addEvent = (type, handler) => {
		document.addEventListener(type, handler);
		listeners.push({ type, handler });
	};

	const removeEvents = () => {
		for (const { type, handler } of listeners) {
			document.removeEventListener(type, handler);
		}
		listeners.length = 0;
	};

	return {
		init: (_dropAreaClassname, _dragOverClassname, _dotNetObjectRef) => {
			dropAreaSelector = `.${_dropAreaClassname}`;
			dragOverClassname = _dragOverClassname;
			dotNetObjectRef = _dotNetObjectRef;

			addEvent("dragstart", (e) => {
				const target = getDraggable(e);
				if (!target) return;

				draggedItem = target;
				e.dataTransfer.effectAllowed = "move";
			});

			addEvent("dragover", (e) => {
				e.preventDefault();
				const target = getDropTarget(e);

				if (isValidTarget(target)) {
					e.dataTransfer.dropEffect = "move";
					target.classList.add(dragOverClassname);
				} else {
					e.dataTransfer.dropEffect = "none";
				}
			});

			addEvent("dragleave", (e) => {
				const target = getDropTarget(e);
				if (target) {
					target.classList.remove(dragOverClassname);
				}
			});

			addEvent("drop", (e) => {
				e.preventDefault();
				const target = getDropTarget(e);

				if (isValidTarget(target)) {
					target.classList.remove(dragOverClassname);

					if (dotNetObjectRef) {
						const dragIndex = draggedItem?.dataset?.index;
						const dropIndex = target?.dataset?.index;

						dotNetObjectRef.invokeMethodAsync("OnDrop", dragIndex, dropIndex);
					}
				}
			});
		},

		teardown: () => {
			removeEvents();
			draggedItem = null;
			dropAreaSelector = null;
			dragOverClassname = null;
			dotNetObjectRef = null;
		}
	};
})();
