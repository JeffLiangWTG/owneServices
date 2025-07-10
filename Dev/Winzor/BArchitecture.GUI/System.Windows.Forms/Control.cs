using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using WinzorFramework.Telemetry;

namespace System.Windows.Forms;

public partial class Control : IHandleEvent, IHandleAfterRender, IBindableComponent, IWin32Window, IArrangedElement, ISynchronizeInvoke, IDisposable
{
	byte _requiredScaling;
	LayoutEventArgs? _cachedLayoutEventArgs;
	ExtendedStates _extendedState;
	States _state;
	const byte RequiredScalingMask = 0x0F;

	internal int LayoutSuspendCount => layoutSuspendCount;

	public bool PreventDefaultMouseDown { get; set; }

	public Control()
	{
		WinzorDispatcher = WinzorDispatcher.Current;

		WinzorControlGuid = Guid.NewGuid();

		Controls = CreateControlsInstance();
		WinzorSpecificControls = new WinzorSpecificControlsCollection(this);
		DataBindings = new ControlBindingsCollection(this);

		Properties = new PropertyStore();

		height = DefaultSize.Height;
		width = DefaultSize.Width;

		if (width != 0 && height != 0)
		{
			Interop.RECT rect = default;

			AdjustWindowRectEx(ref rect);

			clientWidth = width - (rect.right - rect.left);
			clientHeight = height - (rect.bottom - rect.top);
		}

		TabStop = true;

		_state = States.Visible | States.Enabled | States.TabStop | States.CausesValidation;
		SetStyle(ControlStyles.Selectable, true);

		if (DefaultMargin != CommonProperties.DefaultMargin)
		{
			Margin = DefaultMargin;
		}

		RenderFragment = builder =>
		{
			hasPendingQueuedRender = false;
			hasRendered = true;
			RenderFull(builder);
		};
	}

	~Control()
	{
		try
		{
			Dispose(false);
		}
		catch (Exception e)
		{
			Application.ReportDeveloperException("Exception thrown in finalizer", e);
		}
	}

	public Control(string text, int left, int top, int width, int height)
		: this()
	{
		Text = text;
		Bounds = new Rectangle(left, top, width, height);
	}

	public Control(string text)
		: this()
	{
		Text = text;
	}

	protected virtual Padding DefaultPadding => Padding.Empty;

	protected virtual ControlCollection CreateControlsInstance() => new ControlCollection(this);

	#region Blazor interfaces

	bool hasRendered;
	bool hasPendingQueuedRender;

	public bool HasRendered => hasRendered;

	public RenderFragment RenderFragment { get; }

	protected virtual EventAttribute EventAttributes => EventAttribute.None;

	protected virtual void RenderFull(RenderTreeBuilder builder)
	{
		if (UseParentDivForLayout)
		{
			builder.OpenElement(0, "div");
			builder.AddAttribute(1, "style", ControlStyleString);
			builder.AddAttribute(2, "data-winzor-control-id", WinzorControlId);
			builder.AddAttribute(3, "data-type", GetType()); // this is temporary for making development easier
			builder.AddAttribute(4, "data-name", Name ?? string.Empty); // this is temporary for making development easier
#if DEBUG
			builder.AddAttribute(5, "data-layout", DebugAttributesString); // this is temporary for making development easier
#endif
			if (!string.IsNullOrEmpty(ClassName))
			{
				builder.AddAttribute(6, "class", ClassName);
			}
			if (!string.IsNullOrEmpty(ToolTipText))
			{
				builder.AddAttribute(7, "title", ToolTipText);
			}
			if ((EventAttributes & EventAttribute.MouseDown) == EventAttribute.MouseDown)
			{
				builder.AddAttribute(8, "onmousedown", OnMouseDownAsync);
				builder.AddEventStopPropagationAttribute(9, "onmousedown", true);
			}
			if ((EventAttributes & EventAttribute.MouseUp) == EventAttribute.MouseUp)
			{
				builder.AddAttribute(10, "onmouseup", OnMouseUpAsync);
				builder.AddEventStopPropagationAttribute(11, "onmouseup", true);
			}
			if ((EventAttributes & EventAttribute.Click) == EventAttribute.Click)
			{
				builder.AddAttribute(12, "onclick", OnClickAsync);
				builder.AddEventStopPropagationAttribute(13, "onclick", true);
			}
			if ((EventAttributes & EventAttribute.ContextMenu) == EventAttribute.ContextMenu)
			{
				builder.AddAttribute(14, "oncontextmenu", OnContextMenuAsync);
				builder.AddEventStopPropagationAttribute(15, "oncontextmenu", true);
				builder.AddEventPreventDefaultAttribute(16, "oncontextmenu", true);
			}
			if (AllowItemDrag)
			{
				builder.AddAttribute(17, "draggable", "true");
				builder.AddAttribute(18, "ondragstart", OnDragStartAsync);

				// The coordinates of the dragend event are broken when using non-standard 'Text Size' values in Windows (due to a WebView2 issue with custom RasterizationScale values).
				// We need to use a custom event as a workaround for this.
				builder.AddAttribute(19, "onwinzordragend", OnWinzorDragEndAsync);
			}
			if (DragTargetHidden)
			{
				builder.AddAttribute(20, "data-drag-target-hidden", "true");
			}
			if (IsMouseEnterSubscribed())
			{
				builder.AddAttribute(21, "onmouseenter", OnMouseEnterAsync);
			}
			if (IsMouseLeaveSubscribed())
			{
				builder.AddAttribute(22, "onmouseleave", OnMouseLeaveAsync);
			}
			builder.AddAttribute(23, "tabindex", tabIndexHtmlAttribute);
			// Different from Winform. https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.control.focus?view=netframework-4.8.1#remarks 
			builder.AddAttribute(24, "onwinzorfocusin", OnFocusInAsync);
			builder.AddEventStopPropagationAttribute(25, "onwinzorfocusin", true);
			builder.AddAttribute(26, "onwinzorfocusout", OnFocusOutAsync);
			builder.AddEventStopPropagationAttribute(27, "onwinzorfocusout", true);
			// place it last for avoiding exceptions thrown by Blazor
			if (CaptureElementReference)
			{
				builder.AddElementReferenceCapture(28, reference => ElementReference = reference);
			}
			builder.OpenRegion(29);
			BuildRenderTree(builder);
			builder.CloseRegion();
			builder.CloseElement();
		}
		else
		{
			BuildRenderTree(builder);
		}
	}

	protected virtual string ClassName => string.Empty;

	protected virtual void BuildRenderTree(RenderTreeBuilder builder)
	{
		RenderControls(builder);
	}

	protected RenderFragment RenderControls() => builder =>
	{
		RenderControls(builder);
	};

	protected void RenderControls(RenderTreeBuilder builder)
	{
		foreach (var control in Controls.GetSnapshot().OrderBy(c => c.TabIndex).Concat(WinzorSpecificControls.GetSnapshot()))
		{
			if (control is { ShouldRender: true, RenderChainIsDisposedOrDisposing: false })
			{
				builder.OpenComponent<ControlProxyComponent>(0);
				builder.SetKey(control.WinzorControlGuid);
				builder.AddAttribute(1, "Control", control);
				builder.CloseComponent();
			}
		}
	}

	protected internal virtual bool ShouldRender => Visible && Width > 0 && Height > 0;

	public virtual bool RenderInPortal => false;

	public virtual bool CaptureElementReference => false;

	public virtual Color FindControlRealBackColor()
	{
		if (this.BackColor == Color.Transparent && this.Parent != null)
		{
			return this.Parent.FindControlRealBackColor();
		}

		else if (this.BackColor == Color.Transparent)
		{
			return SystemColors.Control;
		}

		return this.BackColor;
	}

	void AddMetrics(Activity? activity)
	{
		activity?.AddTag("Control.Name", Name);
		activity?.AddTag("WinzorControlId", WinzorControlId);
	}

	async Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem callback, object? arg)
	{
		using (var activity = TelemetryService.ActivitySource.StartActivity($"{FindForm()?.GetType().Name}.{GetType().Name}.Handle_{arg?.GetType().Name}", ActivityKind.Server))
		{
			AddMetrics(activity);
			var task = callback.InvokeAsync(arg);
			var shouldAwaitTask = task.Status != TaskStatus.RanToCompletion &&
								task.Status != TaskStatus.Canceled;

			if (shouldAwaitTask)
			{
				try
				{
					await task;
				}
				catch // avoiding exception filters for AOT runtime support
				{
					// Ignore exceptions from task cancellations, but don't bother issuing a state change.
					if (task.IsCanceled)
					{
						return;
					}

					throw;
				}
			}

			if (AutomaticallyReRenderOnEventCallbacks)
			{
				StateHasChanged();
			}
		}
	}

	protected bool AutomaticallyReRenderOnEventCallbacks { get; set; }

	async Task IHandleAfterRender.OnAfterRenderAsync()
	{
		if (afterRenderActions != null)
		{
			var afterRenderActionsToProcess = Interlocked.Exchange(ref afterRenderActions, null);
			if (afterRenderActionsToProcess != null)
			{
				using var activity = TelemetryService.ActivitySource?.StartActivity($"{GetType().Name}.OnAfterRenderAsync");
				StartRenderInvoke();
				try
				{
					await Task.WhenAll(afterRenderActionsToProcess.Select(action => action()));
				}
				finally
				{
					EndRenderInvoke();
				}
			}
		}
	}

	public void RegisterAfterRenderAction(Func<Task> action)
	{
		var links = Activity.Current is null ? null : new[] { new ActivityLink(Activity.Current.Context) };
		if (afterRenderActions == null)
		{
			afterRenderActions = new WrappedList<Func<Task>>();
		}
		afterRenderActions.Add(async () =>
		{
			using var activity = TelemetryService.ActivitySource?.StartActivity($"{GetType().Name}.AfterRenderAction", ActivityKind.Internal, null, links: links);
			await action();
		});
	}
	WrappedList<Func<Task>>? afterRenderActions;

	protected internal virtual Task OnAfterRenderAsync(bool firstRender)
	{
		return Task.CompletedTask;
	}

	protected internal virtual Task OnInitializedAsync()
	{
		return Task.CompletedTask;
	}

	public async Task InvokeStateHasChangedAsync()
	{
		if (RenderChainIsDisposedOrDisposing || !ProxyInitialized)
		{
			return;
		}

		var proxy = Proxy;
		if (proxy != null)
		{
			await proxy.InvokeStateHasChangedAsync();
		}
	}

	public async Task InvokeRenderDispatcherAsync(Func<Task> workItem)
	{
		if (!ProxyInitialized)
		{
			return;
		}

		var wrappedWorkItem = new Func<Task>(async () =>
		{
			if (ProxyInitialized)
			{
				StartRenderInvoke();
				try
				{
					await workItem();
				}
				finally
				{
					EndRenderInvoke();
				}
			}
		});

		await Proxy.InvokeAsync(wrappedWorkItem);
	}

	public void InvokeRenderDispatcher(Func<Task> workItem, bool needElementRendered = false)
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(InvokeRenderDispatcher)}");
		if (ProxyInitialized && WinzorDispatcher.HasCurrentContext && (!needElementRendered || IsElementReferenceCaptured))
		{
			WinzorDispatcher.CurrentContext.RegisterRenderTask(WithExceptionHandlingAsync(() => InvokeRenderDispatcherAsync(workItem)));
		}
		else
		{
			RegisterAfterRenderAction(workItem);
		}
	}

	void StartRenderInvoke()
	{
		if (!inRenderInvoke.Value)
		{
			inRenderInvoke.Value = true;
		}
	}

	void EndRenderInvoke()
	{
		inRenderInvoke.Value = false;
	}
	readonly AsyncLocal<bool> inRenderInvoke = new AsyncLocal<bool>();

	protected bool IsInRenderInvoke => inRenderInvoke.Value;

#if DEBUG
	/// <summary>
	/// For testing with bUnit we may need to capture a render thread callback and invoke back onto the WinzorDispatcher, however this should never be needed in production code
	/// </summary>
	public void EndRenderInvokeForTest()
	{
		EndRenderInvoke();
	}
#endif

	protected void StateHasChanged() => Proxy?.StateHasChanged();

	protected bool UpdateProperty<T>(ref T field, T value)
	{
		bool changed;
		if (field is Color fieldColor && value is Color valueColor)
		{
			changed = fieldColor.ToArgb() != valueColor.ToArgb();
		}
		else
		{
			changed = !field?.Equals(value) ?? value != null;
		}
		field = value;
		if (changed)
		{
			NotifyRenderRequired();
		}

		return changed;
	}

	/// <summary>
	/// Adds a render task to the queue so that when InvokeWinzorDispatcher is called the render task will be executed
	/// and call the Blazor method StateHasChanged in the correct context
	/// Use this method instead of StateHasChanged when invoking actions on the WinzorDispatcher thread
	/// </summary>
	protected internal void NotifyRenderRequired(bool invalidated = false)
	{
		if (WinzorDispatcher.HasCurrentContext && !RenderChainIsDisposedOrDisposing)
		{
			if (layoutSuspendCount > 0 && !invalidated)
			{
				return;
			}

			WinzorDispatcher.CurrentContext.NotifyRenderRequired(this);
		}
	}

	[MemberNotNullWhen(true, nameof(Proxy))]
	protected internal bool ProxyInitialized => Proxy != null;

	bool AncestorIsDisposedOrDisposing()
	{
		var control = this;
		while (control != null)
		{
			if (control.IsDisposed || control.Disposing)
			{
				return true;
			}

			control = control.Parent;
		}

		return false;
	}

	public ControlProxyComponent? Proxy { get; internal set; }

	Guid winzorControlGuid;

	public Guid WinzorControlGuid
	{
		get => winzorControlGuid;
		private set
		{
			winzorControlGuid = value;
			WinzorControlId = WinzorControlGuid.ToString("N");
		}
	}

	public string WinzorControlId { get; private set; } = string.Empty;

	/// <summary>
	/// Is this control part of the <see cref="WinzorSpecificControls"/>
	/// collection?
	/// </summary>
	/// <remarks>If <c>true</c>, this control does not trigger a re-layout
	/// of its parent when its bounds are changed</remarks>
	public bool IsWinzorSpecific { get; set; }

	protected void OnIllegalOperations(string details)
	{
		throw new InvalidOperationException($"An illegal operation is attempted to be executed: {details}");
	}

	protected Control? FindDescendantByWinzorControlId(string winzorControlId)
	{
		if (string.IsNullOrEmpty(winzorControlId))
		{
			return null;
		}
		var match = AllControls.SingleOrDefault(c => c.WinzorControlId == winzorControlId);
		if (match == null)
		{
			foreach (var child in AllControls)
			{
				match = child.FindDescendantByWinzorControlId(winzorControlId);
				if (match != null)
				{
					break;
				}
			}
		}
		return match;
	}

	public Control? FindDescendantByElementReference(ElementReference elementReference)
	{
		var match = AllControls.SingleOrDefault(c => c.ElementReference.Id == elementReference.Id);
		if (match == null)
		{
			foreach (var child in AllControls)
			{
				match = child.FindDescendantByElementReference(elementReference);
				if (match != null)
				{
					break;
				}
			}
		}
		return match;
	}

	public virtual ElementReference ElementReference
	{
		get => elementReference;
		protected internal set
		{
			elementReference = value;
			IsElementReferenceCaptured = !value.Equals(default(ElementReference));
		}
	}

	ElementReference elementReference;

	protected internal bool IsElementReferenceCaptured { get; private set; }

	public ElementReference AnchorElement
	{
		get => anchorElement ?? ElementReference;
		set => anchorElement = value;
	}

	ElementReference? anchorElement;

	#endregion

	#region WinzorDispatcher

	public WinzorDispatcher WinzorDispatcher { get; }

	public async Task InvokeWinzorDispatcherAsync(Action action, bool executeIfDisposed = false)
	{
		var activitySource = Activity.Current?.Source ?? TelemetryService.ActivitySource;
		using var activity = activitySource?.StartActivity($"{GetType().Name}.{nameof(InvokeWinzorDispatcherAsync)}");
		await WithExceptionHandlingAsync(async () =>
		{
			if (inRenderInvoke.Value)
			{
				throw new InvalidOperationException("Invalid call to InvokeWinzorDispatcher from InvokeRenderDispatcher or RegisterAfterRenderAction. Only js invocations should be run on the render thread. Server side code should always run in a single render cycle.");
			}

			var context = new ControlUnitOfWorkContext(this);
			LongActionHandler? longActionHandler = null;
			if (!WinzorDispatcher.IsCurrent)
			{
				longActionHandler = new LongActionHandler(this);
			}
			try
			{
				var invokeAsyncActivity = activitySource?.CreateActivity("WinzorDispatcher.InvokeAsync", ActivityKind.Internal);
				action = activitySource.WithTracing(action, $"{GetType().Name}.WinzorDispatcherTask");
				action = WithOnBeforeRender(WithExceptionHandling(action));
				action = activitySource.WithTracing(action, $"{GetType().Name}.TaskWithOnBeforeRender", invokeAsyncActivity);
				action = WithCurrentControlContext(action, context);
				if (!executeIfDisposed)
				{
					if (IsDisposed)
					{
						return;
					}
					action = WithDisposedCheck(action);
				}

				using (invokeAsyncActivity?.Start())
				{
					await WinzorDispatcher.InvokeAsync(action);
				}
			}
			finally
			{
				if (longActionHandler != null)
				{
					await longActionHandler.DisposeAsync();
				}
			}
			await context.WaitForAllRenderTasksAsync();
			await context.CallStateHasChangedOnRequiredControlsAsync();
		});
	}

	Action WithCurrentControlContext(Action action, IWinzorDispatcherContext context)
	{
		return () =>
		{
			using (WinzorDispatcher.Current.WithContext(context))
			{
				action();
			}
		};
	}

	Action WithExceptionHandling(Action action)
	{
		return () =>
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				Application.OnThreadException(ex);
			}
		};
	}

	Action WithDisposedCheck(Action action)
	{
		return () =>
		{
			if (!IsDisposed)
			{
				action();
			}
		};
	}

	Action WithOnBeforeRender(Action action)
	{
		return () =>
		{
			action();
			((ControlUnitOfWorkContext)WinzorDispatcher.CurrentContext).CallOnBeforeRenderOnRequiredControls();
		};
	}

	internal async Task WithExceptionHandlingAsync(Func<Task> action)
	{
		try
		{
			await ExceptionHandlerExtension.HandleJSExceptionAsync(action);
		}
		catch (Exception ex)
		{
			var form = FindForm();
			if (form == null || (form.IsClosing || form.IsDisposed))
			{
				return;
			}
			if (!ProxyInitialized || IsDisposed)
			{
				return;
			}

			if (ex is OperationCanceledException)
			{
				WinzorDispatcher.Queue(() => Application.ReportDeveloperException(null, ex));
			}
			else
			{
				WinzorDispatcher.Queue(() =>
				{
					using (WinzorDispatcher.WithContext(new ServerInitiatedCallbackContext()))
					{
						Application.OnThreadException(ex);
					}
				});
			}
		}
	}

	#endregion

	public void ReadyToRender()
	{
		using var activity = TelemetryService.ActivitySource?.StartActivity($"{GetType().Name}.{nameof(ReadyToRender)}");

		if (Visible)
		{
			OnBeforeRender();
		}
	}

	internal protected virtual void OnBeforeRender()
	{
		OnPaint(new PaintEventArgs(this));

		foreach (var control in AllControls)
		{
			if (control.Visible)
			{
				InvokeWithExceptionHandling(() => control.OnBeforeRender());
			}
		}

		tabIndexHtmlAttribute = SelectableByTabKey ? "0" : "-1";
	}

	void InvokeWithExceptionHandling(Action action)
	{
		var actionWithExceptionHandling = WithExceptionHandling(action);
		actionWithExceptionHandling();
	}

	string tabIndexHtmlAttribute = string.Empty;

	public virtual async Task DetachFromRendererAsync()
	{
		await Task.CompletedTask;
	}

	public void DetachFromGrid()
	{
		Proxy = null;
		afterRenderActions?.Clear();
		ElementReference = default(ElementReference);
		WmKillFocus();

		WinzorControlGuid = Guid.NewGuid();

		foreach (var control in AllControls)
		{
			control.DetachFromGrid();
		}
	}

	/// <summary>
	///  Indicates whether or not this control has an accessible object associated with it.
	/// </summary>
	internal bool IsAccessibilityObjectCreated;

	protected internal virtual string ControlStyleString => this.ControlStyle() + Proxy?.Style + ExtraStyleString;

	internal string LayoutStyleString => this.LayoutStyle();

	internal string LookStyleString => this.LookStyle();

	public virtual bool UseParentDivForLayout => true;

	public int ZIndex
	{
		get => zIndex;
		set => UpdateProperty(ref zIndex, value);
	}
	int zIndex;

	public int ZIndexMax => Controls.Any() ? Math.Max(this.ZIndex, Controls.Max(c => c.ZIndexMax)) : ZIndex;

	public int ZIndexMin => Controls.Any() ? Math.Min(this.ZIndex, Controls.Min(c => c.ZIndexMin)) : ZIndex;

	public virtual string? ToolTipText { get; set; }

	internal Color RawBackColor => backColor;

	public virtual Color BackColor
	{
		get
		{
			if (!backColor.IsEmpty)
			{
				return backColor;
			}

			if (parent != null)
			{
				return parent.BackgroundImage != null ? Color.Transparent : parent.BackColor;
			}

			return DefaultBackColor;
		}
		set
		{
			if (UpdateProperty(ref backColor, value))
			{
				OnBackColorChanged(new EventArgs());
			}
		}
	}

	Color backColor = Color.Empty;

	public virtual void ResetBackColor() => BackColor = Color.Empty;

	public virtual Image? BackgroundImage
	{
		get => backgroundImage;
		set => UpdateProperty(ref backgroundImage, value);
	}
	Image? backgroundImage;

	public virtual ImageLayout BackgroundImageLayout { get; set; } = ImageLayout.None;

	public Point Location
	{
		get
		{
			return new Point(Left, Top);
		}
		set
		{
			SetBounds(value.X, value.Y, width, height, BoundsSpecified.Location);
		}
	}

	protected virtual Size DefaultSize { get; } = Size.Empty;

	protected virtual Size DefaultMinimumSize => CommonProperties.DefaultMinimumSize;

	protected virtual Size DefaultMaximumSize => CommonProperties.DefaultMaximumSize;

	public Size Size
	{
		get
		{
			return new Size(Width, Height);
		}
		set
		{
			SetBounds(Left, Top, value.Width, value.Height, BoundsSpecified.Size);
		}
	}

	int clientWidth;
	int clientHeight;

	public Size ClientSize
	{
		get => new Size(clientWidth, clientHeight);
		set => SetClientSizeCore(value.Width, value.Height);
	}

	protected virtual void SetClientSizeCore(int x, int y)
	{
		Size = SizeFromClientSize(new Size(x, y));
		clientWidth = x;
		clientHeight = y;
	}

	public Size AutoScaleBaseSize { get; set; }

	public Rectangle ClientRectangle => new Rectangle(Point.Empty, ClientSize);

	public virtual Rectangle DisplayRectangle => new Rectangle(Point.Empty, ClientSize);

	public Rectangle Bounds
	{
		get => new Rectangle(Location, Size);
		set
		{
			SetBounds(value.X, value.Y, value.Width, value.Height, BoundsSpecified.All);
		}
	}

	/// <summary>
	///  Sets the bounds of the control.
	/// </summary>
	public void SetBounds(int x, int y, int width, int height, BoundsSpecified specified)
	{
		if ((specified & BoundsSpecified.X) == BoundsSpecified.None)
		{
			x = Left;
		}

		if ((specified & BoundsSpecified.Y) == BoundsSpecified.None)
		{
			y = Top;
		}

		if ((specified & BoundsSpecified.Width) == BoundsSpecified.None)
		{
			width = Width;
		}

		if ((specified & BoundsSpecified.Height) == BoundsSpecified.None)
		{
			height = Height;
		}

		if (Left != x || Top != y || Width != width ||
			Height != height)
		{
			SetBoundsCore(x, y, width, height, specified);

			LayoutTransaction.DoLayoutIf(!IsWinzorSpecific, Parent, this, PropertyNames.Bounds);
		}
		else
		{
			// Still need to init scaling.
			InitScaling(specified);
		}
	}

	public void SetBounds(int x, int y, int width, int height)
	{
		if (Location.X != x || Location.Y != y || Width != width || Height != height)
		{
			SetBoundsCore(x, y, width, height, BoundsSpecified.All);

			LayoutTransaction.DoLayoutIf(!IsWinzorSpecific, Parent, this, PropertyNames.Bounds);
		}
		else
		{
			// Still need to init scaling.
			InitScaling(BoundsSpecified.All);
		}
	}

	/// <summary>
	///  This method initializes the scaling bits for this control based on
	///  the bounds.
	/// </summary>
	void InitScaling(BoundsSpecified specified)
	{
		_requiredScaling |= (byte)((int)specified & RequiredScalingMask);
	}

	protected virtual void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		if (Parent is not null)
		{
			Parent.SuspendLayout();
		}

		try
		{
			if (Location.X != x || Location.Y != y || Width != width || Height != height)
			{
				CommonProperties.UpdateSpecifiedBounds(this, x, y, width, height, specified);

				// Provide control with an opportunity to apply self imposed constraints on its size.
				Rectangle adjustedBounds = ApplyBoundsConstraints(x, y, width, height);
				width = adjustedBounds.Width;
				height = adjustedBounds.Height;
				x = adjustedBounds.X;
				y = adjustedBounds.Y;

				if (!IsHandleCreated)
				{
					// Handle is not created, just record our new position and we're done.
					UpdateBounds(x, y, width, height);
				}
				else
				{
					// Give a chance for derived controls to do what they want, just before we resize.
					OnBoundsUpdate(x, y, width, height);

					// SetWindowPos prevents using negative width or height
					width = Math.Max(width, 0);
					height = Math.Max(height, 0);

					UpdateBounds(x, y, width, height);
				}
			}
		}
		finally
		{
			// Initialize the scaling engine.
			InitScaling(specified);

			if (Parent is not null)
			{
				// Some layout engines (DefaultLayout) base their PreferredSize on
				// the bounds of their children.  If we change change the child bounds, we
				// need to clear their PreferredSize cache.  The semantics of SetBoundsCore
				// is that it does not cause a layout, so we just clear.
				CommonProperties.xClearPreferredSizeCache(Parent);

				// Cause the current control to initialize its layout (e.g., Anchored controls
				// memorize their distance from their parent's edges).  It is your parent's
				// LayoutEngine which manages your layout, so we call into the parent's
				// LayoutEngine.
				Parent.LayoutEngine.InitLayout(this, specified);
				Parent.ResumeLayout(performLayout: true, this);
			}
		}
	}

	// Give a chance for derived controls to do what they want, just before we resize.
	internal virtual void OnBoundsUpdate(int x, int y, int width, int height)
	{
	}

	internal virtual void AdjustWindowRectEx(ref Interop.RECT rect)
	{
	}

	protected void UpdateBounds(int x, int y, int width, int height)
	{
		// reverse-engineer the AdjustWindowRectEx call to figure out
		// the appropriate clientWidth and clientHeight
		Interop.RECT rect = default;
		rect.left = rect.right = rect.top = rect.bottom = 0;
		AdjustWindowRectEx(ref rect);

		int clientWidth = width - (rect.right - rect.left);
		int clientHeight = height - (rect.bottom - rect.top);
		UpdateBounds(x, y, width, height, clientWidth, clientHeight);
	}

	void UpdateBounds(int x, int y, int width, int height, int clientWidth, int clientHeight)
	{
		bool newLocation = LocationWasChanged(x, y);
		bool newSize = SizeWasChanged(width, height, clientWidth, clientHeight);

		left = x;
		top = y;
		this.width = width;
		this.height = height;
		this.clientWidth = clientWidth;
		this.clientHeight = clientHeight;

		if (newLocation
			// In WinForms, the form location will likely have changed when creating the handle due to AdjustWindowRectEx
			// This is not implemented in Winzor, so force the LocationChanged event to be fired in this scenario
			|| (GetState(States.CreatingHandle) && this is Form))
		{
			OnLocationChanged(EventArgs.Empty);
		}

		if (newSize
			// In WinForms, the form size will likely have changed when creating the handle due to AdjustWindowRectEx
			// This is not implemented in Winzor, so force the SizeChanged event to be fired in this scenario
			|| (GetState(States.CreatingHandle) && this is Form))
		{
			OnSizeChanged(EventArgs.Empty);
			OnClientSizeChanged(EventArgs.Empty);

			// Clear PreferredSize cache for this control
			CommonProperties.xClearPreferredSizeCache(this);
			LayoutTransaction.DoLayoutIf(!IsWinzorSpecific, Parent, this, PropertyNames.Bounds);
		}

		if (newLocation || newSize)
		{
			NotifyRenderRequired();
		}
	}

	public Rectangle ClientAreaBounds
	{
		get
		{
			Interop.RECT adjustmentRect = new Interop.RECT { left = 0, top = 0, right = 0, bottom = 0 };
			AdjustWindowRectEx(ref adjustmentRect);

			var bounds = Bounds;
			var adjustedBounds = new Interop.RECT(bounds.X - adjustmentRect.left, bounds.Y - adjustmentRect.Y, bounds.Right - adjustmentRect.right, bounds.Bottom - adjustmentRect.bottom);

			return new Rectangle(adjustedBounds.X, adjustedBounds.Y, adjustedBounds.Width, adjustedBounds.Height);
		}
	}

	bool LocationWasChanged(int x, int y)
	{
		return Location.X != x || Location.Y != y;
	}

	bool SizeWasChanged(int width, int height, int clientWidth, int clientHeight)
	{
		return Width != width || Height != height ||
				ClientSize.Width != clientWidth || ClientSize.Height != clientHeight;
	}

	protected virtual void OnClientSizeChanged(EventArgs empty)
	{
		this.ClientSizeChanged?.Invoke(this, empty);
	}

	// GetPreferredSize and SetBoundsCore call this method to allow controls to self impose
	// constraints on their size.
	internal Size ApplySizeConstraints(int width, int height)
	{
		return ApplyBoundsConstraints(0, 0, width, height).Size;
	}

	// GetPreferredSize and SetBoundsCore call this method to allow controls to self impose
	// constraints on their size.
	internal Size ApplySizeConstraints(Size proposedSize)
	{
		return ApplyBoundsConstraints(0, 0, proposedSize.Width, proposedSize.Height).Size;
	}

	internal virtual Rectangle ApplyBoundsConstraints(int suggestedX, int suggestedY, int proposedWidth, int proposedHeight)
	{
		// COMPAT: in Everett we would allow you to set negative values in pre-handle mode
		// in Whidbey, if you've set Min/Max size we will constrain you to 0,0.  Everett apps didnt
		// have min/max size on control, which is why this works.
		if (MaximumSize != Size.Empty || MinimumSize != Size.Empty)
		{
			Size maximumSize = LayoutUtils.ConvertZeroToUnbounded(MaximumSize);
			Rectangle newBounds = new Rectangle(suggestedX, suggestedY, 0, 0)
			{
				// Clip the size to maximum and inflate it to minimum as necessary.
				Size = LayoutUtils.IntersectSizes(new Size(proposedWidth, proposedHeight), maximumSize)
			};
			newBounds.Size = LayoutUtils.UnionSizes(newBounds.Size, MinimumSize);

			return newBounds;
		}

		return new Rectangle(suggestedX, suggestedY, proposedWidth, proposedHeight);
	}

	public AccessibleObject AccessibilityObject => accessibilityObject ??= CreateAccessibilityInstance();
	AccessibleObject? accessibilityObject;

	protected virtual AccessibleObject CreateAccessibilityInstance() => new ControlAccessibleObject(this);

	protected internal void UpdateBounds()
	{
		UpdateBounds(Bounds.X, Bounds.Y, Math.Max(width, 0), Math.Max(height, 0));
	}

	public void Scale(SizeF factor)
	{
	}

	protected virtual Size SizeFromClientSize(Size clientSize)
	{
		Interop.RECT rect = new Interop.RECT(0, 0, clientSize.Width, clientSize.Height);
		AdjustWindowRectEx(ref rect);
		return rect.Size;
	}

	public virtual Size MinimumSize
	{
		get => CommonProperties.GetMinimumSize(this, DefaultMinimumSize);
		set
		{
			if (value != MinimumSize)
			{
				CommonProperties.SetMinimumSize(this, value);
				NotifyRenderRequired();
			}
		}
	}

	public virtual Size MaximumSize
	{
		get => CommonProperties.GetMaximumSize(this, DefaultMaximumSize);
		set
		{
			if (value == Size.Empty)
			{
				CommonProperties.ClearMaximumSize(this);
				NotifyRenderRequired();
			}
			else if (value != MaximumSize)
			{
				CommonProperties.SetMaximumSize(this, value);
				NotifyRenderRequired();
			}
		}
	}

	public Size PreferredSize => GetPreferredSize(Size.Empty);

	public virtual Size GetPreferredSize(Size proposedSize)
	{
		Size prefSize;

		if (Disposing || IsDisposed)
		{
			// if someone's asking when we're disposing just return what we last had.
			prefSize = CommonProperties.xGetPreferredSizeCache(this);
		}
		else
		{
			// Switch Size.Empty to maximum possible values
			proposedSize = LayoutUtils.ConvertZeroToUnbounded(proposedSize);

			// Force proposedSize to be within the elements constraints.  (This applies
			// minimumSize, maximumSize, etc.)
			proposedSize = ApplySizeConstraints(proposedSize);
			if (GetExtendedState(ExtendedStates.UserPreferredSizeCache))
			{
				Size cachedSize = CommonProperties.xGetPreferredSizeCache(this);

				// If the "default" preferred size is being requested, and we have a cached value for it, return it.
				if (!cachedSize.IsEmpty && (proposedSize == LayoutUtils.s_maxSize))
				{
					return cachedSize;
				}
			}

			prefSize = GetPreferredSizeCore(proposedSize);

			// There is no guarantee that GetPreferredSizeCore() return something within
			// proposedSize, so we apply the element's constraints again.
			prefSize = ApplySizeConstraints(prefSize);

			// If the "default" preferred size was requested, cache the computed value.
			if (GetExtendedState(ExtendedStates.UserPreferredSizeCache) && proposedSize == LayoutUtils.s_maxSize)
			{
				CommonProperties.xSetPreferredSizeCache(this, prefSize);
			}
		}

		return prefSize;
	}

	// Overriding this method allows us to get the caching and clamping the proposedSize/output to
	// MinimumSize / MaximumSize from GetPreferredSize for free.
	internal virtual Size GetPreferredSizeCore(Size proposedSize)
	{
		var size = CommonProperties.GetSpecifiedBounds(this).Size;
		if (AutoSize)
		{
			size.Width = TextRenderer.MeasureText(Text, Font).Width + AutoSizeExtraWidth;
		}
		return size;
	}

	protected virtual int AutoSizeExtraWidth => 0;

	protected void SetAutoSizeMode(AutoSizeMode mode) => CommonProperties.SetAutoSizeMode(this, mode);

	protected AutoSizeMode GetAutoSizeMode() => CommonProperties.GetAutoSizeMode(this);

	public int Height
	{
		get
		{
			return height;
		}
		set
		{
			SetBounds(Left, Top, Width, value, BoundsSpecified.Height);
		}
	}

	int height;

	public int Width
	{
		get
		{
			return width;
		}
		set
		{
			SetBounds(Left, Top, value, Height, BoundsSpecified.Width);
		}
	}

	int width;

	public int Top
	{
		get
		{
			return top;
		}
		set
		{
			SetBounds(Left, value, Width, Height, BoundsSpecified.Y);
		}
	}

	int top;

	public int Left
	{
		get
		{
			return left;
		}
		set
		{
			SetBounds(value, Top, Width, Height, BoundsSpecified.X);
		}
	}

	int left;

	public int Right => Left + Width;

	public int Bottom => Top + Height;

	protected virtual Padding DefaultMargin => CommonProperties.DefaultMargin;

	void ResetMargin()
	{
		Margin = DefaultMargin;
	}

	public Padding Margin
	{
		get => CommonProperties.GetMargin(this);
		set
		{
			// This should be done here rather than in the property store as
			// some IArrangedElements actually support negative padding.
			value = LayoutUtils.ClampNegativePaddingToZero(value);

			// SetMargin causes a layout as a side effect.
			if (value != Margin)
			{
				CommonProperties.SetMargin(this, value);
				OnMarginChanged(EventArgs.Empty);
				NotifyRenderRequired();
			}
		}
	}

	public Padding Padding
	{
		get => CommonProperties.GetPadding(this, DefaultPadding);
		set
		{
			if (value != Padding)
			{
				CommonProperties.SetPadding(this, value);
				// Ideally we are being laid out by a LayoutEngine that cares about our preferred size.
				// We set our LAYOUTISDIRTY bit and ask our parent to refresh us.
				SetState(States.LayoutIsDirty, true);
				using (new LayoutTransaction(Parent, this, PropertyNames.Padding))
				{
					OnPaddingChanged(EventArgs.Empty);
				}

				if (GetState(States.LayoutIsDirty))
				{
					// The above did not cause our layout to be refreshed.  We explicitly refresh our
					// layout to ensure that any children are repositioned to account for the change
					// in padding.
					LayoutTransaction.DoLayout(this, this, PropertyNames.Padding);
				}
			}
		}
	}

	protected virtual void OnPaddingChanged(EventArgs e)
	{
		PaddingChanged?.Invoke(this, e);
	}

	protected virtual void OnMarginChanged(EventArgs e)
	{
		MarginChanged?.Invoke(this, e);
	}

	public virtual AnchorStyles Anchor
	{
		get => DefaultLayout.GetAnchor(this);
		set => DefaultLayout.SetAnchor(Parent, this, value);
	}

	public virtual DockStyle Dock
	{
		get => DefaultLayout.GetDock(this);
		set
		{
			if (value != Dock)
			{
				SuspendLayout();
				try
				{
					DefaultLayout.SetDock(this, value);
					OnDockChanged(EventArgs.Empty);
				}
				finally
				{
					ResumeLayout();
				}
			}
		}
	}

	public event EventHandler? DockChanged;

	protected virtual void OnDockChanged(EventArgs e)
	{
		DockChanged?.Invoke(this, e);
	}

	protected virtual void InitLayout()
	{
		LayoutEngine.InitLayout(this, BoundsSpecified.All);
	}

	public virtual bool AutoSize
	{
		get => CommonProperties.GetAutoSize(this);
		set
		{
			if (value == AutoSize)
			{
				return;
			}

			CommonProperties.SetAutoSize(this, value);

			if (Parent is not null)
			{
				// DefaultLayout does not keep anchor information until it needs to.  When
				// AutoSize became a common property, we could no longer blindly call into
				// DefaultLayout, so now we do a special InitLayout just for DefaultLayout.
				if (value && Parent.LayoutEngine == DefaultLayout.Instance)
				{
					Parent.LayoutEngine.InitLayout(this, BoundsSpecified.Size);
				}

				LayoutTransaction.DoLayout(Parent, this, PropertyNames.AutoSize);
			}
		}
	}

	protected bool ResizeRedraw { get; set; }

	public virtual LayoutEngine LayoutEngine => DefaultLayout.Instance;

	public virtual RightToLeft RightToLeft { get; set; }

	public string? AccessibleDescription { get; set; }

	public string? AccessibleName { get; set; }

	/// <summary>
	/// User supplied control style
	/// </summary>
	ControlStyles controlStyle;

	protected void SetStyle(ControlStyles flag, bool value)
	{
		// WARNING: if we ever add argument checking to "flag", we will need
		// to move private styles like Layered to State.
		controlStyle = value ? controlStyle | flag : controlStyle & ~flag;
	}

	protected void UpdateStyles()
	{
		OnStyleChanged(EventArgs.Empty);
	}

	/// <summary>
	///  Computes the location of the screen point p in client coords.
	///
	///  (but in Winzor is currently doing it relative to current window
	///   rather than screen - until we need multi window drag or something
	///   else that needs *screen*)
	///
	///  (assumes that Form is the top level and ignores offset)
	///

	/// </summary>
	public Point PointToClient(Point p)
	{
		var result = p;
		var item = this;
		while (item != null && item is not Form)
		{
			result.X -= item.Location.X;
			result.Y -= item.Location.Y;
			item = item.parent;
		}
		return result;
	}

	/// <summary>
	///  Computes the location of the client point p in screen coords.
	///
	///  (but in Winzor is currently doing it in current window coords
	///   rather than screen - until we need multi window drag or something
	///   else that needs screen)
	///
	///  (assumes that Form is the top level and ignores offset)
	///
	/// </summary>
	public Point PointToScreen(Point p)
	{
		var result = p;
		var item = this;
		while (item != null && item is not Form)
		{
			result.X += item.Location.X;
			result.Y += item.Location.Y;
			item = item.parent;
		}
		return result;
	}

	/// <summary>
	///  Computes the location of a Rectangle r in screen coords.
	///
	///  (but in Winzor is currently doing it in current window coords
	///   rather than screen - until we need multi window drag or something
	///   else that needs screen)
	///
	/// </summary>
	public Rectangle RectangleToScreen(Rectangle r)
	{
		var screenPoint = PointToScreen(new Point(r.X, r.Y));
		var result = new Rectangle(screenPoint, new Size(r.Width, r.Height));
		return result;
	}

	/// <summary>
	///  Deriving classes can override this to configure a default cursor for their control.
	/// </summary>
	protected virtual Cursor DefaultCursor => Cursors.Default;

	public virtual Cursor Cursor
	{
		get
		{
			if (useWaitCursor)
			{
				return Cursors.WaitCursor;
			}

			if (cursor != null)
			{
				return cursor;
			}

			if (DefaultCursor != Cursors.Default)
			{
				return DefaultCursor;
			}

			if (Parent is not null)
			{
				return Parent.Cursor;
			}

			return DefaultCursor;
		}
		set
		{
			UpdateProperty(ref cursor, value);
		}
	}
	Cursor? cursor;

	bool useWaitCursor;
	public virtual bool UseWaitCursor
	{
		get => useWaitCursor;
		set
		{
			if (useWaitCursor != value)
			{
				useWaitCursor = value;
				NotifyRenderRequired();
				var controlCollection = AllControls?.ToList();
				if (controlCollection != null)
				{
					for (int i = 0; i < controlCollection.Count; i++)
					{
						controlCollection[i].UseWaitCursor = value;
					}
				}
			}
		}
	}

	[AllowNull]
	public string Name
	{
		get => name ?? string.Empty;
		set => name = value;
	}
	string? name;

	public int TabIndex
	{
		get => tabIndex == -1 ? 0 : tabIndex;
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidLowBoundArgumentEx, nameof(TabIndex), value, 0));
			}

			UpdateProperty(ref tabIndex, value);
		}
	}

	int tabIndex = -1;

	/// <summary>
	///  Indicates whether the control is visible.
	/// </summary>
	public virtual bool Visible
	{
		get => GetVisibleCore();
		set => SetVisibleCore(value);
	}

	public bool ControlVisible => GetState(States.Visible);

	/// <summary>
	/// Updates the local visible bool inside of Control.cs to provide the same behaviour as SetState(visible, ...) from WinForms
	/// </summary>
	/// <param name="value"></param>
	protected void SetVisibleState(bool value)
		=> SetState(States.Visible, value);

	/// <summary>
	///  Hides the control by setting the visible property to false;
	/// </summary>
	public void Hide()
	{
		Visible = false;
	}

	internal virtual bool GetVisibleCore()
	{
		// We are only visible if our parent is visible
		if (!GetState(States.Visible))
		{
			return false;
		}
		else if (Parent == null)
		{
			return true;
		}
		else
		{
			return Parent.GetVisibleCore();
		}
	}

	protected virtual void SetVisibleCore(bool value)
	{
		if (GetVisibleCore() != value)
		{
			if (!value)
			{
				SelectNextIfFocused();
			}

			var fireChange = false;

			if (GetTopLevel())
			{
				// The processing of WmShowWindow will set the visibility
				// bit and call CreateControl()

				if (IsHandleCreated || value)
				{
					// In WinForms, this logic is contained in the getter of the Handle property
					// We have replicated this behavior to ensure the handle is always created
					if (!IsHandleCreated)
					{
						CreateHandle();
					}

					// In WinForms they would call WmShowWindow here to show/hide the form
					// We will call an equivalent function to achieve the same thing in a Winzor environment

					// The processing of ShowWindow will set the visibility bit and call CreateControl()
					ShowWindow(value);
				}
			}
			else if (IsHandleCreated || value && parent != null && parent.Created)
			{
				// We want to mark the control as visible so that CreateControl
				// knows that we are going to be displayed... however in case
				// an exception is thrown, we need to back the change out.

				SetState(States.Visible, value);
				fireChange = true;
				try
				{
					if (value)
					{
						CreateControl();
					}
				}
				catch
				{
					SetState(States.Visible, !value);
					throw;
				}
			}

			if (GetVisibleCore() != value)
			{
				SetState(States.Visible, value);
				fireChange = true;
			}

			if (fireChange)
			{
				// We do not do this in the OnPropertyChanged event for visible
				// Lots of things could cause us to become visible, including a
				// parent window.  We do not want to indiscriminately layout
				// due to this, but we do want to layout if the user changed
				// our visibility.
				using (new LayoutTransaction(parent, this, PropertyNames.Visible))
				{
					OnVisibleChanged(EventArgs.Empty);
				}
			}
		}
		else
		{
			// value of Visible property not changed, but raw bit may have
			SetState(States.Visible, value);
		}
	}

	/// <summary>
	/// Winzor equivalent of the ShowWindow method in User32 and is Called when updating the visibility of a window.
	/// This method should be overridden in top level controls (eg. Form) to dictate their show and hide behavior.
	/// When overriding, the base implementation inside Control should be called before any additional logic to
	/// recreate the WinForms behaviour of handling the WM_SHOWWINDOW message before the Window is made visible 
	/// </summary>
	/// <param name="isBecomingVisible">Indicates whether the control is becoming visible or hidden. Winzor equivalent of the SW_SHOW/SW_HIDE params in User32.</param>
	void ShowWindow(bool isBecomingVisible)
	{
		// The WmShowWindow message is usually invoked via WndProc which implements exception handling which we must emulate
		InvokeWithExceptionHandling(() => WmShowWindow(isBecomingVisible));
	}

	/// <summary>
	/// Winzor equivalent of the WndProc hander WinForms for the WM_SHOWWINDOW message
	/// </summary>
	/// <param name="isBecomingVisible">Indicates whether the control is becoming visible or hidden. Winzor equivalent of the WParam in the WndProc message.</param>
	void WmShowWindow(bool isBecomingVisible)
	{
		// We get this message for each control, even if their parent is not visible.
		if (!GetState(States.Recreate))
		{
			var oldVisibleProperty = Visible;

			if (isBecomingVisible)
			{
				var oldVisibleBit = GetState(States.Visible);
				SetState(States.Visible, true);
				var executedOk = false;
				try
				{
					CreateControl();
					executedOk = true;
				}

				finally
				{
					if (!executedOk)
					{
						// We do it this way instead of a try/catch because catching and rethrowing
						// an exception loses call stack information
						SetState(States.Visible, oldVisibleBit);
					}
				}
			}
			else
			{
				// If Windows tells us it's visible, that's pretty unambiguous.
				// But if it tells us it's not visible, there's more than one explanation --
				// maybe the container control became invisible.  So we look at the parent
				// and take a guess at the reason.

				// We do not want to update state if we are on the parking window.
				var parentVisible = GetTopLevel();
				if (Parent != null)
				{
					parentVisible = Parent.Visible;
				}

				if (parentVisible)
				{
					SetState(States.Visible, false);
				}
			}

			if (!GetState(States.ParentRecreating) && (oldVisibleProperty != isBecomingVisible))
			{
				OnVisibleChanged(EventArgs.Empty);
			}
		}
	}

	protected bool GetTopLevel()
		=> GetState(States.TopLevel);

	void SelectNextIfFocused()
	{
		if (ContainsFocus && Parent != null)
		{
			IContainerControl? c = Parent.GetContainerControl();

			if (c != null)
			{
				((Control)c).SelectNextControl(this, true, true, true, true);
			}
		}
	}

	public bool IsAccessible { get; set; }

	public AccessibleRole AccessibleRole { get; set; }

	public virtual ContextMenu? ContextMenu { get; set; }

	public virtual ContextMenuStrip? ContextMenuStrip { get; set; }

	public IntPtr Handle
	{
		get
		{
			if (!IsHandleCreated)
			{
				CreateHandle();
			}
			return IntPtr.Zero;
		}
	}

	public bool HandlesClick => this.Click != null;

	public event EventHandler? AutoSizeChanged;

	public event EventHandler? BackColorChanged;

	public event EventHandler? BindingContextChanged;

	public event ControlEventHandler? ControlAdded;

	public event ControlEventHandler? ControlRemoved;

	public event EventHandler? Click;

	public event EventHandler? ClientSizeChanged;

	public event EventHandler? CursorChanged;

	public event EventHandler? Disposed;

	public event EventHandler? DoubleClick;

	public event EventHandler? EnabledChanged;

	public event EventHandler? Enter;

	public event EventHandler? FontChanged;

	public event EventHandler? ForeColorChanged;

	public event EventHandler? GotFocus;

	public event EventHandler? HandleCreated;

	public event EventHandler? HandleDestroyed;

	public event InvalidateEventHandler? Invalidated;

	public event KeyEventHandler? KeyDown;

	public event KeyPressEventHandler? KeyPress;

	public event KeyEventHandler? KeyUp;

	public event LayoutEventHandler? Layout;

	public event EventHandler? Leave;

	public event EventHandler? LocationChanged;

	public event EventHandler? LostFocus;

	public event MouseEventHandler? MouseClick;

	public event MouseEventHandler? MouseDoubleClick;

	public event MouseEventHandler? MouseDown;

	public event EventHandler? MouseEnter;

	public event EventHandler? MouseHover;

	public event EventHandler? MouseLeave;

	public event MouseEventHandler? MouseMove;

	public event MouseEventHandler? MouseUp;

	public event MouseEventHandler? MouseOver;

	public event MouseEventHandler? MouseWheel;

	public event EventHandler? PaddingChanged;

	public event EventHandler? MarginChanged;

	public event PaintEventHandler? Paint;

	public event EventHandler? ParentChanged;

	public event QueryContinueDragEventHandler? QueryContinueDrag;

	public event EventHandler? Resize;

	public event EventHandler? SizeChanged;

	public event EventHandler? StyleChanged;

	public event EventHandler? TabIndexChanged;

	public event EventHandler? TextChanged;

	public event CancelEventHandler? Validating;

	public event EventHandler? Validated;

	public event EventHandler? VisibleChanged;

	public event EventHandler? CausesValidationChanged;

	readonly Lazy<EventHandlerList> lazyEvents = new Lazy<EventHandlerList>(() => new EventHandlerList());
	protected EventHandlerList Events => lazyEvents.Value;

	protected virtual void OnControlAdded(ControlEventArgs e)
	{
		ControlAdded?.Invoke(this, e);
	}
	protected virtual void OnControlRemoved(ControlEventArgs e)
	{
		ControlRemoved?.Invoke(this, e);
	}

	protected virtual void OnBindingContextChanged(EventArgs e)
	{
		if (DataBindings is not null)
		{
			UpdateBindings();
		}

		BindingContextChanged?.Invoke(this, e);

		foreach (var control in AllControls)
		{
			control.OnBindingContextChanged(e);
		}
	}

	void UpdateBindings()
	{
		for (int i = 0; i < DataBindings.Count; i++)
		{
			BindingContext.UpdateBinding(BindingContext, DataBindings[i]);
		}
	}

	protected virtual void OnForeColorChanged(EventArgs e)
	{
		ForeColorChanged?.Invoke(this, e);

		foreach (var ctrl in AllControls)
		{
			ctrl.OnParentForeColorChanged(e);
		}
	}

	protected virtual void OnBackColorChanged(EventArgs e)
	{
		BackColorChanged?.Invoke(this, e);

		foreach (var ctrl in AllControls)
		{
			ctrl.OnParentBackColorChanged(e);
		}
	}

	protected virtual void OnParentForeColorChanged(EventArgs e)
	{
		if (foreColor.IsEmpty)
		{
			OnForeColorChanged(e);
		}
	}

	protected virtual void OnParentBackColorChanged(EventArgs e)
	{
		if (backColor.IsEmpty)
		{
			OnBackColorChanged(e);
		}
	}

	protected virtual void OnClick(EventArgs e)
	{
		Click?.Invoke(this, e);
	}

	protected internal void CopyClickEventHandler(MenuItem itemSrc)
	{
		Click = itemSrc.Click;
	}

	/// <summary>
	///  Called when the control is first created.
	/// </summary>
	protected virtual void OnCreateControl()
	{
	}

	protected virtual void OnDoubleClick(EventArgs e)
	{
		DoubleClick?.Invoke(this, e);
	}

	protected virtual void OnEnabledChanged(EventArgs e)
	{
		EnabledChanged?.Invoke(this, e);
	}

	protected virtual void OnEnter(EventArgs e)
	{
		Enter?.Invoke(this, e);
	}

	protected virtual void OnFontChanged(EventArgs e)
	{
		FontChanged?.Invoke(this, e);
	}

	protected virtual void OnHandleCreated(EventArgs e)
	{
		HandleCreated?.Invoke(this, e);
	}

	protected virtual void OnHandleDestroyed(EventArgs e)
	{
		HandleDestroyed?.Invoke(this, e);
	}

	protected virtual void OnKeyDown(KeyEventArgs e)
	{
		KeyDown?.Invoke(this, e);
	}

	protected virtual void OnKeyUp(KeyEventArgs e)
	{
		KeyUp?.Invoke(this, e);
	}

	protected virtual void OnKeyPress(KeyPressEventArgs e)
	{
		KeyPress?.Invoke(this, e);
	}

	protected virtual void OnLayout(LayoutEventArgs e)
	{
		Layout?.Invoke(this, e);
		bool parentRequiresLayout = LayoutEngine.Layout(this, e);
		if (parentRequiresLayout && Parent is not null)
		{
			// LayoutEngine.Layout can return true to request that our parent resize us because
			// we did not have enough room for our contents. We can not just call PerformLayout
			// because this container is currently suspended. PerformLayout will check this state
			// flag and PerformLayout on our parent.
			Parent.SetState(States.LayoutIsDirty, true);
		}
	}

	protected virtual void OnLeave(EventArgs e)
	{
		Leave?.Invoke(this, e);
	}

	protected virtual void OnLocationChanged(EventArgs e)
	{
		LocationChanged?.Invoke(this, e);
	}

	protected virtual void OnPaint(PaintEventArgs e)
	{
		Paint?.Invoke(this, e);
	}

	protected virtual void OnPaintBackground(PaintEventArgs pevent)
	{
	}

	protected virtual void OnParentVisibleChanged(EventArgs e)
	{
		if (GetState(States.Visible))
		{
			OnVisibleChanged(e);
		}
	}

	internal virtual void OnParentBecameInvisible()
	{
		if (GetState(States.Visible))
		{
			foreach (var control in AllControls)
			{
				control.OnParentBecameInvisible();
			}
		}
	}

	protected virtual void OnMouseClick(MouseEventArgs e)
	{
		MouseButtons = e.Button;
		MouseClick?.Invoke(this, e);
		MouseButtons = MouseButtons.None;
	}

	protected virtual void OnMouseDoubleClick(MouseEventArgs e)
	{
		MouseDoubleClick?.Invoke(this, e);
	}

	protected virtual void OnMouseDown(MouseEventArgs e)
	{
		MouseButtons = e.Button;
		MouseDown?.Invoke(this, e);
	}

	protected virtual void OnMouseUp(MouseEventArgs e)
	{
		MouseUp?.Invoke(this, e);
		MouseButtons = MouseButtons.None;
	}

	protected virtual void OnMouseHover(EventArgs e)
	{
		MouseHover?.Invoke(this, e);
	}

	protected virtual void OnMouseEnter(EventArgs e)
	{
		MouseEnter?.Invoke(this, e);
	}

	protected async Task OnMouseEnterAsync()
	{
		if (!Enabled)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() => OnMouseEnter(EventArgs.Empty));
	}

	protected virtual void OnMouseLeave(EventArgs e)
	{
		MouseLeave?.Invoke(this, e);
	}

	protected async Task OnMouseLeaveAsync()
	{
		if (!Enabled)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() => OnMouseLeave(EventArgs.Empty));
	}

	protected virtual void OnMouseMove(MouseEventArgs e)
	{
		MouseMove?.Invoke(this, e);
	}

	protected virtual void OnMouseOver(MouseEventArgs e)
	{
		MouseOver?.Invoke(this, e);
	}

	protected async Task OnMouseOverAsync(WebMouseEventArgs e)
	{
		if (!Enabled)
		{
			return;
		}

		SetMouseData(e);
		await InvokeWinzorDispatcherAsync(() =>
		{
			OnMouseHover(EventArgs.Empty);
			OnMouseOver(new MouseEventArgs(e.GetMouseButtons(), (int)e.Detail, (int)e.ClientX, (int)e.ClientY, 0));
		});
	}

	protected virtual void OnParentChanged(EventArgs e)
	{
		ParentChanged?.Invoke(this, e);
	}

	protected virtual void OnResize(EventArgs e)
	{
		LayoutTransaction.DoLayout(this, this, PropertyNames.Bounds);
		Resize?.Invoke(this, e);
	}

	protected virtual void OnSizeChanged(EventArgs e)
	{
		OnResize(EventArgs.Empty);
		SizeChanged?.Invoke(this, e);
	}

	protected virtual void OnStyleChanged(EventArgs e)
	{
		StyleChanged?.Invoke(this, e);
	}

	protected virtual void OnTextChanged(EventArgs e)
	{
		TextChanged?.Invoke(this, e);
		if (AutoSize)
		{
			PerformLayout();
		}
	}

	protected virtual void OnValidated(EventArgs e)
	{
		Validated?.Invoke(this, e);
	}

	protected virtual void OnValidating(CancelEventArgs e)
	{
		Validating?.Invoke(this, e);
	}

	/// <summary>
	///  Raises the <see cref='Visible'/> event.
	///  Inheriting classes should override this method to handle this event.
	///  Call base.OnVisibleChanged to send this event to any registered event listeners.
	/// </summary>
	protected virtual void OnVisibleChanged(EventArgs e)
	{
		var visible = Visible;
		if (parent != null && visible && !Created)
		{
			var isDisposing = GetAnyDisposingInHierarchy();
			if (!isDisposing)
			{
				// Usually the control is created by now, but in a few corner cases
				// exercised by the PropertyGrid dropdowns, it isn't
				CreateControl();
			}
		}

		VisibleChanged?.Invoke(this, e);

		foreach (var control in AllControls)
		{
			if (control.Visible)
			{
				control.OnParentVisibleChanged(e);
			}
			if (!visible)
			{
				control.OnParentBecameInvisible();
			}
		}
	}

	protected void InvokeOnClick(Control toInvoke, EventArgs e)
	{
		toInvoke?.OnClick(e);
	}

	protected void InvokeOnMouseDown(Control toInvoke, MouseEventArgs e)
	{
		toInvoke?.OnMouseDown(e);
	}

	protected void InvokeOnMouseUp(Control toInvoke, MouseEventArgs e)
	{
		toInvoke?.OnMouseUp(e);
	}

	public bool CausesValidation
	{
		get => GetState(States.CausesValidation);
		set
		{
			if (value != CausesValidation)
			{
				SetState(States.CausesValidation, value);
				OnCausesValidationChanged(EventArgs.Empty);
			}
		}
	}

	protected virtual void OnCausesValidationChanged(EventArgs e)
	{
		CausesValidationChanged?.Invoke(this, e);
	}

	protected virtual void CreateHandle()
	{
		ObjectDisposedException.ThrowIf(IsDisposed, this);

		if (GetState(States.CreatingHandle))
		{
			return;
		}

		Rectangle originalBounds;

		try
		{
			SetState(States.CreatingHandle, true);

			originalBounds = Bounds;

			IsHandleCreated = true;

			// The WmCreate message is usually invoked via WndProc which implements exception handling which we must emulate
			InvokeWithExceptionHandling(WmCreate);
		}
		finally
		{
			SetState(States.CreatingHandle, false);
		}

		// For certain controls (e.g., ComboBox) CreateWindowEx
		// may cause the control to resize.  WM_SETWINDOWPOSCHANGED takes care of
		// the control being resized, but our layout container may need a refresh as well.
		if (Bounds != originalBounds)
		{
			LayoutTransaction.DoLayout(Parent, this, PropertyNames.Bounds);
		}
	}

	void WmCreate()
	{
		UpdateBounds();
		OnHandleCreated(EventArgs.Empty);
	}

	protected virtual void DestroyHandle()
		// The WMDestroy message is usually invoked via WndProc which implements exception handling which we must emulate
		=> InvokeWithExceptionHandling(WmDestroy);

	void WmDestroy()
	{
		IsHandleCreated = false;

		foreach (var control in Controls)
		{
			control.DestroyHandle();
		}

		OnHandleDestroyed(EventArgs.Empty);

		if (!Disposing)
		{
			// If we are not recreating the handle, set our created state
			// back to false so we can be rebuilt if we need to be.
			if (!GetState(States.Recreate))
			{
				SetState(States.Created, false);
			}
		}
		else
		{
			SetState(States.Visible, false);

			// In NativeWindow.ReleaseHandle, the Handle will be reset to null which will reset the created state to false
			SetState(States.Created, false);
		}
	}

	public void RecreateHandle()
	{
		var created = GetState(States.Created);

		DestroyHandle();
		CreateHandle();

		if (created)
		{
			CreateControl();
		}

		if (Focused)
		{
			FocusInternal();
		}
	}

	protected virtual bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (ContextMenu != null && ContextMenu.ProcessCmdKey(ref msg, keyData, this))
		{
			return true;
		}

		return Parent?.ProcessCmdKey(ref msg, keyData) ?? false;
	}

	protected virtual bool ProcessKeyMessage(ref Message m)
	{
		if (Parent?.ProcessKeyPreview(ref m) ?? false)
		{
			this.supressKeyPress = HasParentSupressKeyPress();
			return true;
		}

		return ProcessKeyEventArgs(ref m);
	}

	bool HasParentSupressKeyPress()
	{
		var current = this;
		while (current != null)
		{
			if (current.supressKeyPress)
			{
				return true;
			}
			current = current.Parent;
		}
		return false;
	}

	public void ProcessKeyCharMessage(Keys key, string rawKey, bool altKey)
	{
		var message = GetKeyMessage(KeyEventType.WMCHAR, rawKey, key, altKey);
		ProcessKeyMessage(ref message);
	}

	protected virtual bool ProcessKeyEventArgs(ref Message m)
	{
		if (m.Msg == WM_CHAR || m.Msg == WM_SYSCHAR)
		{
			var kpe = new KeyPressEventArgs((char)(int)m.WParam);
			OnKeyPress(kpe);
			var newWParam = kpe.KeyChar;

			m.WParam = newWParam;
			return kpe.Handled;
		}
		else
		{
			var ke = new KeyEventArgs((Keys)(int)m.WParam | ModifierKeys);
			if (m.Msg == WM_KEYDOWN || m.Msg == WM_SYSKEYDOWN)
			{
				OnKeyDown(ke);
			}
			else
			{
				OnKeyUp(ke);
			}

			supressKeyPress = ke.SuppressKeyPress;

			return ke.Handled;
		}
	}

	internal const int WM_KEYDOWN = 0x0100;
	internal const int WM_KEYUP = 0x0101;
	internal const int WM_CHAR = 0x0102;
	internal const int WM_SYSCHAR = 0x0106;
	internal const int WM_SYSKEYDOWN = 0x0104;
	internal const int WM_SYSKEYUP = 0x0105;

	public virtual bool Enabled
	{
		get
		{
			if (!enabled)
			{
				return false;
			}
			return Parent?.Enabled ?? true;
		}
		set
		{
			if (UpdateProperty(ref enabled, value))
			{
				OnEnabledChanged(EventArgs.Empty);
			}
		}
	}

	bool enabled = true;

	protected virtual bool SelectableByTabKey => TabStop;

	public bool TabStop { get; set; }

	[AllowNull]
	public virtual string Text
	{
		get => text;
		set
		{
			if (UpdateProperty(ref text, value ?? string.Empty))
			{
				OnTextChanged(EventArgs.Empty);
			}
		}
	}

	public virtual string ExtraStyleString
	{
		get => extraStyleString;
		set
		{
			UpdateProperty(ref extraStyleString, value ?? string.Empty);
		}
	}

	public virtual Dictionary<string, object> DataAttributes
	{
		get
		{
			if (UseParentDivForLayout)
			{
				return [];
			}
			return new Dictionary<string, object>
			{
				{ "data-winzor-control-id", WinzorControlId },
				{ "data-type", GetType() },
				{ "data-name", Name ?? string.Empty },
#if DEBUG
				{ "data-layout", DebugAttributesString }
#endif
			};
		}
	}

	internal virtual string DebugAttributesString
	{
		get
		{
			return $"margin: {Margin.Top} {Margin.Right} {Margin.Bottom} {Margin.Left}; padding: {Padding.Top} {Padding.Right} {Padding.Bottom} {Padding.Left}";
		}
	}

	protected void UpdateTextDirectly(string value)
	{
		text = value;
		OnTextChanged(EventArgs.Empty);
	}

	protected void UpdateTextSilently(string value)
	{
		text = value;
	}

	string text = string.Empty;

	string extraStyleString = string.Empty;

	public virtual void ResetText() => Text = string.Empty;

	public object? Tag { get; set; }

	public bool AllowItemDrag { get; set; }

	public bool DragTargetHidden { get; set; }

	public string? Visibility { get; set; }

	protected virtual bool AllowTextChangeFromClient => Enabled;

	protected async Task OnClickAsync(WebMouseEventArgs e)
	{
		if (!Enabled)
		{
			return;
		}

		await OnClickCoreAsync(e);
	}

	protected virtual async Task OnClickCoreAsync(WebMouseEventArgs e)
	{
		SetMouseData(e);
		await InvokeWinzorDispatcherAsync(() =>
		{
			if (Form.ActiveForm != null && this.FindForm() != Form.ActiveForm)
			{
				return;
			}
			OnClick(EventArgs.Empty);
			OnMouseClick(new MouseEventArgs(e.GetMouseButtons(), (int)e.Detail, (int)e.ClientX, (int)e.ClientY, 0));
		});
	}

	protected async Task OnMouseUpAsync(WebMouseEventArgs e)
	{
		if (!Enabled)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			OnMouseUpCore(e);
		});
	}

	protected void OnMouseUpCore(WebMouseEventArgs e)
	{
		Cursor.Position = new Point((int)e.ClientX, (int)e.ClientY);
		MouseButtons = MouseButtons.None;
		SetMouseData(e);
		OnMouseUp(new MouseEventArgs(e.GetMouseButtons(), 1, (int)e.ClientX, (int)e.ClientY, 0));
		OnQueryContinueDrag(new QueryContinueDragEventArgs(0, false, DragAction.Continue));
	}

	bool isContextMenuMouseDown;

	protected async Task OnMouseDownAsync(WebMouseEventArgs e)
	{
		if (!Enabled)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			OnMouseDownCore(e);
		});
	}

	protected void OnMouseDownCore(WebMouseEventArgs e)
	{
		Cursor.Position = new Point((int)e.ClientX, (int)e.ClientY);
		MouseButtons = e.GetMouseButtons();
		var buttons = e.GetMouseButtons();
		if (buttons != MouseButtons.None)
		{
			SetMouseData(e);
		}

		OnMouseDown(new MouseEventArgs(buttons, (int)e.Detail, (int)e.OffsetX, (int)e.OffsetY, 0));

		if (e.Detail == 2)
		{
			OnDoubleClick(EventArgs.Empty);
			var args = new MouseEventArgs(MouseButtons.Left, 2, MousePosition.X, MousePosition.Y, 0);
			OnMouseDoubleClick(args);
		}
	}

	protected Task OnContextMenuAsync(WebMouseEventArgs e)
	{
		return InvokeWinzorDispatcherAsync(() =>
		{
			OnContextMenuCore(e);
		});
	}

	protected void OnContextMenuCore(WebMouseEventArgs e)
	{
		var buttons = e.GetMouseButtons();
		if (buttons != MouseButtons.None)
		{
			SetMouseData(e);
		}
		isContextMenuMouseDown = e.Type == "mousedown";
		var triggeredByKeyboard = buttons == MouseButtons.None;

		if (!isContextMenuMouseDown && (ContextMenuStrip != null || ContextMenu != null))
		{
			CallParentContextMenu(Parent, triggeredByKeyboard);
		}
		else
		{
			OpenContextMenu(triggeredByKeyboard);
		}

		isContextMenuMouseDown = false;
	}

	void CallParentContextMenu(Control? parent, bool triggeredByKeyboard = false)
	{
		if (parent is null)
		{
			OpenContextMenu(triggeredByKeyboard);
			return;
		}

		if (!parent.isContextMenuMouseDown)
		{
			CallParentContextMenu(parent.Parent, triggeredByKeyboard);
			return;
		}

		parent.OpenContextMenu(triggeredByKeyboard);
	}

	internal void OpenContextMenu(bool triggeredByKeyboard = false)
	{
		if (ContextMenuStrip is not null)
		{
			var point = triggeredByKeyboard ? new Point(Width / 2, Height / 2) : PointToClient(MousePosition);
			ContextMenuStrip.Show(this, point);
		}
		else if (ContextMenu is not null)
		{
			var point = triggeredByKeyboard ? PointToScreen(new Point(Width / 2, Height / 2)) : MousePosition;
			ContextMenu.Show(this, point);
		}
		else if (Parent is not null)
		{
			Parent.OpenContextMenu(triggeredByKeyboard);
		}
	}

	protected async Task OnDragStartAsync(WebDragEventArgs arg)
	{
		if (!Enabled)
		{
			return;
		}

		Cursor.Position = new Point((int)arg.ClientX, (int)arg.ClientY);
		await InvokeWinzorDispatcherAsync(() =>
		{
			OnDragStart(
					new DragEventArgs(
						new DataObject(this),
						0,
						(int)arg.ClientX,
						(int)arg.ClientY,
						DragDropEffects.Move,
						DragDropEffects.Move));
		});
	}

	protected async Task OnDragDropAsync(WinzorDragEventArgs arg)
	{
		if (!Enabled)
		{
			return;
		}

		var ctrl = arg.ControlID != null ? FindForm()?.FindDescendantByWinzorControlId(arg.ControlID) : null;

		if (ctrl != null)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				OnDragDrop(
						new DragEventArgs(
							new DataObject(ctrl),
							0,
							arg.ClientX,
							arg.ClientY,
							DragDropEffects.Move,
							DragDropEffects.Move));
			});
		}
	}

	protected async Task OnDragOverAsync(WebDragEventArgs arg)
	{
		if (!Enabled)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			OnDragOver(
					new DragEventArgs(
						new DataObject(this),
						0,
						(int)arg.ClientX,
						(int)arg.ClientY,
						DragDropEffects.Move,
						DragDropEffects.Move));
		});
	}

	protected async Task OnWinzorDragEndAsync(WinzorDragEndEventArgs arg)
	{
		if (!Enabled)
		{
			return;
		}

		Cursor.Position = new Point(arg.ClientX, arg.ClientY);
		await InvokeWinzorDispatcherAsync(() =>
		{
			OnDragEnd(
					new DragEventArgs(
						new DataObject(this),
						0,
						arg.ClientX,
						arg.ClientY,
						DragDropEffects.Move,
						DragDropEffects.Move));
		});
	}

	protected async Task OnDragLeaveAsync(WebDragEventArgs arg)
	{
		if (!Enabled)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			OnDragLeave(
					new DragEventArgs(
						new DataObject(this),
						0,
						(int)arg.ClientX,
						(int)arg.ClientY,
						DragDropEffects.Move,
						DragDropEffects.Move));
		});
	}

	protected void SetMouseData(WebMouseEventArgs args)
	{
		MousePosition = new Point((int)args.ClientX, (int)args.ClientY);
		ModifierKeys = args.GetModifierKeys();
	}

	protected async Task OnFocusInAsync(WinzorFocusInEventArgs args)
	{
		if (!Enabled || args.InitiatedFromServer)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			if (!Focused)
			{
				FocusInternal(focusFromClient: true);
			}
		});
	}

	public virtual ElementReference? GetFocusElement() => CaptureElementReference ? ElementReference : null;

	protected virtual bool FocusInternal(bool focusFromClient = false)
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(FocusInternal)}");
		if (CanFocus)
		{
			SetFocus(focusFromClient);
		}

		if (Focused && Parent is not null)
		{
			IContainerControl? control = Parent.GetContainerControl();

			if (control is not null)
			{
				if (control is ContainerControl containerControl)
				{
					containerControl.SetActiveControl(this);
				}
				else
				{
					control.ActiveControl = this;
				}
			}

			var form = FindForm();
			Form.SetActiveForm(form, form);
		}

		return Focused;
	}

	protected async Task OnFocusOutAsync(WinzorFocusOutEventArgs args)
	{
		if (!Enabled || args.InitiatedFromServer)
		{
			return;
		}

		if (args.TargetWinzorControlId == args.RelatedTargetWinzorControlId)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			FocusOutInternal(args.TargetWinzorControlId, args.RelatedTargetWinzorControlId);
		});
	}

	protected virtual void FocusOutInternal(string? targetWinzorControlId, string? relatedTargetWinzorControlId)
	{
		var controlForm = FindForm();
		var target = targetWinzorControlId != null ? controlForm?.FindDescendantByWinzorControlId(targetWinzorControlId) : null;
		if (target?.Focused ?? false)
		{
			var relatedTarget = relatedTargetWinzorControlId != null ? controlForm?.FindDescendantByWinzorControlId(relatedTargetWinzorControlId) : null;
			target.OnLostFocus(EventArgs.Empty, relatedTarget);
			if (relatedTarget is null)
			{
				controlForm?.SetActiveControl(null);
			}
		}
	}

	protected virtual void OnGotFocus(EventArgs e)
	{
		GotFocus?.Invoke(this, EventArgs.Empty);
	}

	internal void SetFocus(bool focusFromClient = false)
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(SetFocus)}");
		// This method will sumilate the behaviour of the PInvoke.SetFocus method to:
		// - Set the Focused variable (as in WinForms this is done by using a PInvoke with a reference to control handle)
		// - Set the focus to the current object in JavaScript
		// - Set the active control and invoke the GotFocus event (both of which are normally invoked by WM_SETFOCUS through WndProc - see WmSetFocus event in Control.cs)
		// - Invoke WM_SETFOCUS and WM_KILLFOCUS messages normally provided via WndProc

		if (Focused)
		{
			return;
		}

		Focused = true;

		if (!focusFromClient)
		{
			InvokeRenderDispatcher(async () =>
			{
				if (!ProxyInitialized || JSRuntime is null || !Focused)
				{
					return;
				}

				await FocusOnClientAsync(JSRuntime);
			}, true);
		}

		var form = FindForm();
		if (form is not null)
		{
			form.LastFocusedControl = this;
		}

		WmSetFocus();
	}

	internal virtual async Task FocusOnClientAsync(IJSRuntime jsRuntime) => await ElementReference.TryFocusOnClientAsync(jsRuntime);

	protected virtual void WmSetFocus()
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(WmSetFocus)}");
		// Our Winzor implementation of the WM_SETFOCUS WndProc
		// Override this function if your control normally listens for the WM_SETFOCUS message
		IContainerControl? c = GetContainerControl();
		if (c is not null)
		{
			bool activateSucceed;

			if (c is ContainerControl knowncontainer)
			{
				activateSucceed = knowncontainer.ActivateControl(this);
			}
			else
			{
				// Taking focus and activating a control in response to a user gesture (WM_SETFOCUS) is OK.
				activateSucceed = c.ActivateControl(this);
			}

			if (!activateSucceed)
			{
				return;
			}
		}

		InvokeGotFocus(this, EventArgs.Empty);
	}

	internal virtual void WmKillFocus(bool force = false)
	{
		// Our Winzor implementation of the WM_KILLFOCUS WndProc
		// Override this function if your control normally listens for the WM_KILLFOCUS message
		if (Focused || force)
		{
			Focused = false;
			InvokeLostFocus(this, EventArgs.Empty);
			var form = FindForm();
			if (form is not null)
			{
				form.LastFocusedControl = null;
			}
		}
	}

	protected void InvokeGotFocus(Control toInvoke, EventArgs e)
	{
		if (toInvoke is not null)
		{
			toInvoke.OnGotFocus(e);
		}
	}

	void OnLostFocus(EventArgs e, Control? newFocusedControl)
	{
		if (newFocusedControl == null || !Contains(newFocusedControl))
		{
			WmKillFocus();
		}
	}

	protected virtual void OnLostFocus(EventArgs e)
	{
		LostFocus?.Invoke(this, EventArgs.Empty);
	}

	protected void InvokeLostFocus(Control toInvoke, EventArgs e)
	{
		if (toInvoke is not null)
		{
			toInvoke.OnLostFocus(e);
		}
	}

	public bool Created => (_state & States.Created) != 0;

	/// <summary>
	///  Forces the creation of the control. This includes the creation of the handle,
	///  and any child controls.
	/// </summary>
	public void CreateControl()
	{
		var controlIsAlreadyCreated = Created;
		CreateControl(false);

		if (bindingContext == null && Parent != null && !controlIsAlreadyCreated)
		{
			// We do not want to call our parent's BindingContext property here.
			// We have no idea if us or any of our children are using data binding,
			// and invoking the property would just create the binding manager, which
			// we don't need.  We just blindly notify that the binding manager has
			// changed, and if anyone cares, they will do the comparison at that time.
			OnBindingContextChanged(EventArgs.Empty);
		}
	}

	/// <summary>
	///  Forces the creation of the control. This includes the creation of the handle,
	///  and any child controls.
	/// <param name='ignoreVisible'>
	///  Determines whether we should create the handle after checking the Visible
	///  property of the control or not.
	/// </param>
	/// </summary>
	internal void CreateControl(bool ignoreVisible)
	{
		// PERF: Only "create" the control if it is visible.
		// This has the effect of delayed handle creation of hidden controls.
		if ((!GetState(States.Created) && Visible) || ignoreVisible)
		{
			using var activity = TelemetryService.ActivitySource?.StartActivity($"{GetType().Name}.{nameof(CreateControl)}");
			SetState(States.Created, true);
			var createdOK = false;
			try
			{
				if (!IsHandleCreated)
				{
					CreateHandle();
				}

				foreach (var control in AllControls)
				{
					control.CreateControl(ignoreVisible);
				}

				createdOK = true;
			}
			finally
			{
				if (!createdOK)
				{
					SetState(States.Created, false);
				}
			}

			OnCreateControl();
		}
	}

	public virtual Color ForeColor
	{
		get
		{
			if (!foreColor.IsEmpty)
			{
				return foreColor;
			}

			if (parent != null)
			{
				return parent.ForeColor;
			}

			return DefaultForeColor;
		}
		set
		{
			UpdateProperty(ref foreColor, value);
			OnForeColorChanged(EventArgs.Empty);
		}
	}
	Color foreColor = Color.Empty;

	public virtual Font Font
	{
		get => font ?? Parent?.Font ?? DefaultFont;
		set
		{
			if (UpdateProperty(ref font, value))
			{
				FontHeight = font?.GetFontHeight() ?? -1;
				using (new LayoutTransaction(Parent, this, PropertyNames.Font))
				{
					OnFontChanged(EventArgs.Empty);
				}
			}
		}
	}
	Font? font;

	internal bool HasFontSet => font is not null;

	public int FontHeight { get; private set; } = DefaultFontHeight;

	internal void RemoveControl(Control item)
	{
		if (item.IsWinzorSpecific)
		{
			WinzorSpecificControls.Remove(item);
		}
		else
		{
			Controls.Remove(item);
		}
	}

	public Control? Parent
	{
		get => parent;
		set
		{
			if (parent == value)
			{
				return;
			}
			if (value is not null)
			{
				value.Controls.Add(this);
			}
			else if (parent is not null)
			{
				parent.RemoveControl(this);
			}
		}
	}
	Control? parent;

	internal virtual void AssignParent(Control? value)
	{
		var oldBackColor = BackColor;
		var oldEnabled = Enabled;
		var oldVisible = Visible;

		parent = value;
		OnParentChanged(EventArgs.Empty);

		if (oldEnabled != Enabled)
		{
			OnEnabledChanged(EventArgs.Empty);
		}

		var newVisible = Visible;
		if (oldVisible != newVisible && !(!oldVisible && newVisible && parent == null))
		{
			OnVisibleChanged(EventArgs.Empty);
		}

		if (Parent != null && Created)
		{
			OnBindingContextChanged(EventArgs.Empty);
		}

		if (Parent is not null)
		{
			Parent.LayoutEngine.InitLayout(this, BoundsSpecified.All);
		}

		if (oldBackColor != BackColor)
		{
			OnBackColorChanged(EventArgs.Empty);
		}
	}

	public Control? TopLevelControl => TopLevelControlInternal;

	internal Control? TopLevelControlInternal
	{
		get
		{
			Control? control = this;
			while (control is not null && control.Parent is not null)
			{
				control = control.Parent;
			}

			return control;
		}
	}

	protected virtual Control? RenderParentNode => Parent;

	public Form? FindForm() => FindForm(throwIfNotOnForm: false);

	public Form? FindForm([DoesNotReturnIf(true)] bool throwIfNotOnForm)
	{
		var control = this;
		while (!(control is Form) && control != null)
		{
			control = control.Parent;
			if (control == null && throwIfNotOnForm)
			{
				throw new InvalidOperationException("Control is not on a Form");
			}
		}

		return (Form?)control;
	}

	internal virtual bool IsContainerControl => false;

	public IContainerControl? GetContainerControl()
	{
		var c = this;

		if (c is not null && IsContainerControl)
		{
			c = c.Parent;
		}
		while (c is not null && !IsFocusManagingContainerControl(c))
		{
			c = c.Parent;
		}
		return (IContainerControl?)c;
	}

	static bool IsFocusManagingContainerControl(Control ctl)
	{
		return ((ctl.controlStyle & ControlStyles.ContainerControl) == ControlStyles.ContainerControl && ctl is IContainerControl);
	}

	public virtual bool AllowDrop { get; set; }

	// A WinForms property that returns a list of all child controls of a WinForms Control.
	public ControlCollection Controls { get; }

	// A custom collection used to store controls that exist only in Winzor and are not part of the standard WinForms Controls collection.
	// These custom controls are typically used to render elements that WinForms doesn't natively support, such as custom painted elements.
	// The purpose of WinzorSpecificControls is to "hide" these custom controls from any application logic that enumerates the Controls collection,
	// ensuring that they are not counted in scenarios like unit tests that check the number of controls (which would otherwise include these custom controls).
	// This allows the custom controls to be rendered correctly in Winzor, while keeping the original behavior of the Controls collection intact.
	public WinzorSpecificControlsCollection WinzorSpecificControls { get; }

	IEnumerable<Control> AllControls => Controls.Concat(WinzorSpecificControls);

	// A WinForms property that contains the WinForms child controls
	public IList<Control> Children => Controls;

	// A WinForms property that indicates whether a control has any child controls.
	// In Winzor, if a control contains a painted element
	// (which is added as a custom control in WinzorSpecificControls),
	// HasChildren will still return false, maintaining compatibility with WinForms behavior.
	public bool HasChildren => Controls.Count > 0;

	public bool Contains(Control? ctl)
	{
		while (ctl is not null)
		{
			ctl = ctl.Parent;
			if (ctl is null)
			{
				return false;
			}

			if (ctl == this)
			{
				return true;
			}
		}

		return false;
	}

	public Control? GetNextControl(Control? ctl, bool forward)
	{
		if (!Contains(ctl))
		{
			ctl = this;
		}

		if (forward)
		{
			var ctlControls = ctl!.Controls;

			if (ctlControls is not null && ctlControls.Count > 0 && (ctl == this || !IsFocusManagingContainerControl(ctl)))
			{
				var found = ctl.GetFirstChildControlInTabOrder(/*forward=*/true);
				if (found is not null)
				{
					return found;
				}
			}

			while (ctl != this)
			{
				var targetIndex = ctl!.tabIndex;
				var hitCtl = false;
				Control? found = null;
				var p = ctl.Parent;

				// Cycle through the controls in z-order looking for the one with the next highest
				// tab index.  Because there can be dups, we have to start with the existing tab index and
				// remember to exclude the current control.
				var parentControlCount = 0;

				var parentControls = p?.Controls;

				if (parentControls is not null)
				{
					parentControlCount = parentControls.Count;
				}

				for (var c = 0; c < parentControlCount; c++)
				{
					// The logic for this is a bit lengthy, so I have broken it into separate
					// clauses:

					// We are not interested in ourself.
					if (parentControls![c] != ctl)
					{
						// We are interested in controls with >= tab indexes to ctl.  We must include those
						// controls with equal indexes to account for duplicate indexes.
						if (parentControls[c].tabIndex >= targetIndex)
						{
							// Check to see if this control replaces the "best match" we've already
							// found.
							if (found is null || found.tabIndex > parentControls[c].tabIndex)
							{
								// Finally, check to make sure that if this tab index is the same as ctl,
								// that we've already encountered ctl in the z-order.  If it isn't the same,
								// than we're more than happy with it.
								if (parentControls[c].tabIndex != targetIndex || hitCtl)
								{
									found = parentControls[c];
								}
							}
						}
					}
					else
					{
						// We track when we have encountered "ctl".  We never want to select ctl again, but
						// we want to know when we've seen it in case we find another control with the same tab index.
						hitCtl = true;
					}
				}

				if (found is not null)
				{
					return found;
				}

				ctl = ctl.Parent;
			}
		}
		else
		{
			if (ctl != this)
			{
				var targetIndex = ctl!.tabIndex;
				var hitCtl = false;
				Control? found = null;
				var parent = ctl.Parent ?? throw new InvalidOperationException(string.Format(SR.ParentPropertyNotSetInGetNextControl, nameof(Control.Parent), ctl));

				var siblings = parent.Controls ?? throw new InvalidOperationException(string.Format(SR.ControlsPropertyNotSetInGetNextControl, nameof(Controls), parent));

				int siblingCount = siblings.Count;

				if (siblingCount == 0)
				{
					throw new InvalidOperationException(
						string.Format(SR.ControlsCollectionShouldNotBeEmptyInGetNextControl,
							nameof(Controls), parent));
				}

				// Cycle through the controls in reverse z-order looking for the next lowest tab index.  We must
				// start with the same tab index as ctl, because there can be dups.
				for (int c = siblingCount - 1; c >= 0; c--)
				{
					var sibling = siblings[c];
					// The logic for this is a bit lengthy, so I have broken it into separate
					// clauses:

					// We are not interested in ourself.
					if (sibling != ctl)
					{
						// We are interested in controls with <= tab indexes to ctl.  We must include those
						// controls with equal indexes to account for duplicate indexes.
						if (sibling.tabIndex <= targetIndex)
						{
							// Check to see if this control replaces the "best match" we've already
							// found.
							if (found is null || found.tabIndex < sibling.tabIndex)
							{
								// Finally, check to make sure that if this tab index is the same as ctl,
								// that we've already encountered ctl in the z-order.  If it isn't the same,
								// than we're more than happy with it.
								if (sibling.tabIndex != targetIndex || hitCtl)
								{
									found = sibling;
								}
							}
						}
					}
					else
					{
						// We track when we have encountered "ctl".  We never want to select ctl again, but
						// we want to know when we've seen it in case we find another control with the same tab index.
						hitCtl = true;
					}
				}

				// If we were unable to find a control we should return the control's parent.  However, if that parent is us, return
				// NULL.
				if (found is not null)
				{
					ctl = found;
				}
				else
				{
					if (parent == this)
					{
						return null;
					}
					else
					{
						// If we don't found any siblings, and the control is a ToolStripItem that hosts a control itself,
						// then we shouldn't return its parent, because it would be the same ToolStrip we're currently at.
						// Instead, we should return the control that is previous to the current ToolStrip
						if (ctl.ToolStripControlHost is not null)
						{
							return GetNextControl(ctl.Parent, forward: false);
						}

						return parent;
					}
				}
			}

			// We found a control.  Walk into this control to find the proper child control within it to select.
			var children = ctl.Controls;

			while (children is not null && children.Count > 0 && (ctl == this || !IsFocusManagingContainerControl(ctl)))
			{
				var found = ctl.GetFirstChildControlInTabOrder(forward: false);
				if (found is not null)
				{
					ctl = found;
					children = ctl.Controls;
				}
				else
				{
					break;
				}
			}
		}

		return ctl == this ? null : ctl;
	}

	internal virtual Control? GetFirstChildControlInTabOrder(bool forward)
	{
		var ctlControls = Controls;

		Control? found = null;
		if (ctlControls is not null)
		{
			if (forward)
			{
				for (int c = 0; c < ctlControls.Count; c++)
				{
					if (found is null || found.tabIndex > ctlControls[c].tabIndex)
					{
						found = ctlControls[c];
					}
				}
			}
			else
			{
				// Cycle through the controls in reverse z-order looking for the one with the highest
				// tab index.
				for (int c = ctlControls.Count - 1; c >= 0; c--)
				{
					if (found is null || found.tabIndex < ctlControls[c].tabIndex)
					{
						found = ctlControls[c];
					}
				}
			}
		}

		return found;
	}

	public ControlBindingsCollection DataBindings { get; }

	public virtual BindingContext? BindingContext
	{
		get
		{
			return bindingContext ?? Parent?.BindingContext;
		}
		set
		{
			if (bindingContext != value)
			{
				bindingContext = value;
				OnBindingContextChanged(EventArgs.Empty);
			}
		}
	}

	BindingContext? bindingContext;

	public bool Focus() => FocusInternal();

	public bool Capture { get; set; }

	public virtual bool Focused { get; private set; }

	public bool CanFocus => Visible && Enabled && !SuspendRedraw && FindForm() is not null;

	protected virtual bool ShowFocusCues { get; }

	public bool CanSelect
	{
		get
		{
			if ((controlStyle & ControlStyles.Selectable) != ControlStyles.Selectable)
			{
				return false;
			}

			for (var ctl = this; ctl != null; ctl = ctl.Parent)
			{
				if (!ctl.Enabled || !ctl.Visible)
				{
					return false;
				}
			}

			return true;
		}
	}

	public bool ContainsFocus
	{
		get
		{
			if (!IsHandleCreated)
			{
				return false;
			}

			if (Focused || IsDescendant(FindForm()?.LastFocusedControl))
			{
				return true;
			}

			return false;
		}
	}

	public bool IsHandleCreated { get; private set; }

	/// <summary>
	///  Makes the control display by setting the Visible property to true
	/// </summary>
	public void Show()
	{
		Visible = true;
	}

	public void BringToFront()
	{
		if (parent != null)
		{
			parent.Controls.SetChildIndex(this, 0);

			var childControls = parent.Controls.Where(c => c != this);
			if (childControls.Any())
			{
				var maxZIndex = childControls.Max(c => c.ZIndexMax);
				ZIndex = Math.Max(maxZIndex + 1, zIndex);
			}
		}
	}

	public void SendToBack()
	{
		if (parent != null)
		{
			parent.Controls.SetChildIndex(this, -1);

			var minZIndex = parent.Controls.Min(c => c.ZIndexMin);
			if (minZIndex == 0)
			{
				ZIndex = 0;
			}
			else
			{
				ZIndex = minZIndex - 1;
			}
		}
	}

	public void Invalidate() => Invalidate(false);

	public void Invalidate(bool invalidateChildren)
	{
		if (invalidateChildren && this.Children != null && this.Children.Count > 0)
		{
			foreach (var child in this.Children)
			{
				child?.Invalidate(invalidateChildren);
			}
		}
		NotifyRenderRequired(true);
	}

	public void Invalidate(Rectangle rc) => Invalidate(false);

	public void Invalidate(Rectangle rc, bool invalidateChildren) => Invalidate(invalidateChildren);

	public void Invalidate(Region region) => Invalidate(false);

	public void Update()
	{
		if (FindForm() is null)
		{
			// Control is not currently on a form, so it should not be re-rendered
			return;
		}

		OnBeforeRender();
		InvokeRenderDispatcher(InvokeStateHasChangedAsync);
	}

	public virtual void Refresh()
	{
		Invalidate(true);
		Update();
	}

	public void SuspendLayout()
	{
		layoutSuspendCount++;
	}

	bool suspendRedraw;

	public bool SuspendRedraw
	{
		get => suspendRedraw || Parent is { SuspendRedraw: true };
		set => suspendRedraw = value;
	}

	void ResumeLayout(bool performLayout, Control? childControlToPerformLayout)
	{
		bool performedLayout = false;
		if (layoutSuspendCount > 0)
		{
			layoutSuspendCount--;
			if (layoutSuspendCount == 0 && performLayout)
			{
				PerformLayout();
				// the direct child needs to be layout after the parent, otherwise it may end up
				// with negative width or size
				childControlToPerformLayout?.PerformLayout();
				NotifyRenderRequired();
				performedLayout = true;
			}
		}

		if (!performedLayout)
		{
			SetExtendedState(ExtendedStates.ClearLayoutArgs, true);
		}

		if (!performLayout)
		{
			CommonProperties.xClearPreferredSizeCache(this);
			var controlsCollection = AllControls?.ToList();

			// PERFNOTE:
			// This is more efficient than using Foreach.Foreach
			// forces the creation of an array subset enum each time we
			// enumerate
			if (controlsCollection is not null)
			{
				for (int i = 0; i < controlsCollection.Count; i++)
				{
					LayoutEngine.InitLayout(controlsCollection[i], BoundsSpecified.All);
					CommonProperties.xClearPreferredSizeCache(controlsCollection[i]);
				}
			}
		}
	}

	void OnLayoutResuming(bool performLayout)
	{
	}

	public void ResumeLayout() => ResumeLayout(true);

	public void ResumeLayout(bool performLayout)
	{
		ResumeLayout(performLayout, null);
	}

	byte layoutSuspendCount;

	internal bool IsLayoutSuspended => layoutSuspendCount > 0;

	public void PerformLayout()
	{
		if (_cachedLayoutEventArgs != null)
		{
			PerformLayout(_cachedLayoutEventArgs);
			_cachedLayoutEventArgs = null;
		}
		else
		{
			PerformLayout(null, null);
		}
		NotifyRenderRequired();
	}

	public void PerformLayout(Control? affectedControl, string? affectedProperty)
	{
		PerformLayout(new LayoutEventArgs(affectedControl, affectedProperty));
	}

	internal void PerformLayout(LayoutEventArgs args)
	{
		if (GetAnyDisposingInHierarchy())
		{
			return;
		}

		if (LayoutSuspendCount > 0)
		{
			SetState(States.LayoutDeferred, true);
			if (_cachedLayoutEventArgs is null || GetExtendedState(ExtendedStates.ClearLayoutArgs))
			{
				_cachedLayoutEventArgs = args;
				if (GetExtendedState(ExtendedStates.ClearLayoutArgs))
				{
					SetExtendedState(ExtendedStates.ClearLayoutArgs, false);
				}
			}

			LayoutEngine.ProcessSuspendedLayoutEventArgs(this, args);

			return;
		}

		layoutSuspendCount = 1;

		try
		{
			OnLayout(args);
		}
		finally
		{
			// Rather than resume layout (which will could allow a deferred layout to layout the
			// the container we just finished laying out) we set layoutSuspendCount back to zero
			// and clear the deferred and dirty flags.
			SetState(States.LayoutDeferred | States.LayoutIsDirty, false);
			layoutSuspendCount = 0;

			// LayoutEngine.Layout can return true to request that our parent resize us because
			// we did not have enough room for our contents.  Now that we are unsuspended,
			// see if this happened and layout parent if necessary.  (See also OnLayout)
			if (Parent is not null && Parent.GetState(States.LayoutIsDirty))
			{
				LayoutTransaction.DoLayout(Parent, this, PropertyNames.PreferredSize);
			}
		}
	}

	private protected bool GetExtendedState(ExtendedStates flag) => (_extendedState & flag) != 0;

	private protected void SetExtendedState(ExtendedStates flag, bool value)
	{
		_extendedState = value ? _extendedState | flag : _extendedState & ~flag;
	}

	internal bool GetAnyDisposingInHierarchy()
	{
		Control? up = this;

		bool isDisposing = false;
		while (up is not null)
		{
			if (up.Disposing)
			{
				isDisposing = true;
				break;
			}

			up = up.Parent;
		}

		return isDisposing;
	}

	public virtual void Select()
	{
		Select(false, false);
	}

	public bool SelectNextControl(Control? ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
	{
		var nextSelectableControl = GetNextSelectableControl(ctl, forward, tabStopOnly, nested, wrap);
		if (nextSelectableControl is not null)
		{
			nextSelectableControl.Select(true, forward);
			return true;
		}
		else
		{
			return false;
		}
	}

	protected Control? GetNextSelectableControl(Control? ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
	{
		if (!Contains(ctl) ||
			(!nested && ctl?.Parent != this))
		{
			ctl = null;
		}

		var alreadyWrapped = false;
		var start = ctl;
		do
		{
			ctl = GetNextControl(ctl, forward);
			if (ctl is null)
			{
				if (!wrap)
				{
					break;
				}

				if (alreadyWrapped)
				{
					return null; //prevent infinite wrapping.
				}

				alreadyWrapped = true;
			}
			else
			{
				if (ctl.CanSelect
					&& (!tabStopOnly || ctl.TabStop)
					&& (nested || ctl.Parent == this))
				{
					if (ctl.Parent is ToolStrip)
					{
						continue;
					}

					return ctl;
				}
			}
		}
		while (ctl != start);
		return null;
	}

	protected virtual void Select(bool directed, bool forward)
	{
		var c = GetContainerControl();
		if (c != null)
		{
			c.ActiveControl = this;
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (Disposing || IsDisposed)
			{
				return;
			}
			Disposing = true;

			try
			{
				ResetBindings();

				if (IsHandleCreated)
				{
					DestroyHandle();
				}

				if (Parent != null)
				{
					Parent.RemoveControl(this);
				}

				foreach (var control in AllControls)
				{
					control.Dispose();
				}

				Disposed?.Invoke(this, EventArgs.Empty);
			}
			finally
			{
				IsDisposed = true;
			}
		}
	}

	public void Dispose()
	{
		if (InvokeRequired)
		{
			_ = InvokeWinzorDispatcherAsync(Dispose);
			return;
		}

		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public bool Disposing { get; private set; }

	public bool IsDisposed { get; private set; }

	protected virtual bool RenderChainIsDisposedOrDisposing => IsDisposed || Disposing || (RenderParentNode?.AncestorIsDisposedOrDisposing() ?? false);

	public bool InvokeRequired => WinzorDispatcher.ManagedThreadId != Environment.CurrentManagedThreadId;

	public IAsyncResult BeginInvoke(Delegate method) => InvokeWinzorDispatcherAsync(() =>
	{
		if (!IsDisposed)
		{
			method.DynamicInvoke();
		}
	});

	public IAsyncResult BeginInvoke(Delegate method, params object?[]? args) => InvokeWinzorDispatcherAsync(() =>
	{
		if (!IsDisposed)
		{
			method.DynamicInvoke(args);
		}
	});

	[SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "Implementing WinForms API that requies a wait")]
	public object? EndInvoke(IAsyncResult asyncResult)
	{
		if (asyncResult is Task task)
		{
			if (WinzorDispatcher.IsCurrent)
			{
				using var cts = new CancellationTokenSource();
				task = task.ContinueWith(_ => cts.Cancel(), TaskScheduler.Default);
				WinzorDispatcher.Current.RunMessageLoop(cts);
			}
			task.GetAwaiter().GetResult();
		}
		return null;
	}

	[SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "Implementing WinForms API that requies a wait")]
	public object? Invoke(Delegate method)
	{
		if (InvokeRequired)
		{
			object? result = null;
			var task = InvokeWinzorDispatcherAsync(() => result = method.DynamicInvoke());
			if (WinzorDispatcher.IsCurrent)
			{
				using var cts = new CancellationTokenSource();
				task = task.ContinueWith(_ => cts.Cancel(), TaskScheduler.Default);
				WinzorDispatcher.Current.RunMessageLoop(cts);
			}
			task.GetAwaiter().GetResult();
			return result;
		}
		else
		{
			return method.DynamicInvoke();
		}
	}

	[SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "Implementing WinForms API that requies a wait")]
	public object? Invoke(Delegate method, params object?[]? args)
	{
		if (InvokeRequired)
		{
			object? result = null;
			var task = InvokeWinzorDispatcherAsync(() => result = method.DynamicInvoke(args));
			if (WinzorDispatcher.IsCurrent)
			{
				using var cts = new CancellationTokenSource();
				task = task.ContinueWith(_ => cts.Cancel(), TaskScheduler.Default);
				WinzorDispatcher.Current.RunMessageLoop(cts);
			}
			task.GetAwaiter().GetResult();
			return result;
		}
		else
		{
			return method.DynamicInvoke(args);
		}
	}

	public virtual ISite? Site
	{
		get => site;
		set => site = value is null ? null : new SiteWithContainerExtenders(value);
	}

	ISite? site;

	protected virtual object? GetService(Type service) => site?.GetService(service);

	public BGraphics CreateGraphics() => CreateGraphicsInternal();

	internal BGraphics CreateGraphicsInternal()
	{
		return new BGraphics();
	}

	public Control? GetChildAtPoint(Point location) => null;

	private protected void SetState(States flag, bool value)
	{
		_state = value ? _state | flag : _state & ~flag;
	}

	private protected bool GetState(States flag) => (_state & flag) != 0;

	protected virtual bool ProcessKeyPreview(ref Message m) => Parent?.ProcessKeyPreview(ref m) ?? false;

	protected virtual bool ProcessDialogKey(Keys keyData) => Parent?.ProcessDialogKey(keyData) ?? false;

	internal void DoDragOverDrop(DragEventArgs dragEvent)
	{
		OnDragOver(dragEvent);
		OnDragDrop(dragEvent);
	}

	protected virtual void OnDragStart(DragEventArgs dragEvent)
	{
		DragStart?.Invoke(this, dragEvent);
	}

	protected virtual void OnDragDrop(DragEventArgs dragEvent)
	{
		DragDrop?.Invoke(this, dragEvent);
	}

	protected virtual void OnDragOver(DragEventArgs dragEvent)
	{
		DragOver?.Invoke(this, dragEvent);
	}

	protected virtual void OnDragEnter(DragEventArgs dragEvent)
	{
		DragEnter?.Invoke(this, dragEvent);
	}

	protected virtual void OnDragLeave(EventArgs dragEvent)
	{
		DragLeave?.Invoke(this, dragEvent);
	}

	protected virtual void OnDragEnd(DragEventArgs dragEvent)
	{
		DragEnd?.Invoke(this, dragEvent);
	}

	protected virtual void OnQueryContinueDrag(QueryContinueDragEventArgs queryContinueDragEventArgs)
	{
		QueryContinueDrag?.Invoke(this, queryContinueDragEventArgs);
	}

	public virtual DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects)
	{
		if ((allowedEffects & DragDropEffects.Move) != 0)
		{
			InvokeRenderDispatcher(async () => await (GetJSInterop<IFormJSInterop>()?.ChangeMouseCursorStyleAsync(MousePosition.X, MousePosition.Y) ?? Task.CompletedTask));
		}
		return DragDropEffects.None;
	}

	public event DragEventHandler? DragStart;

	public event DragEventHandler? DragDrop;

	public event DragEventHandler? DragEnter;

	public event DragEventHandler? DragOver;

	public event EventHandler? DragLeave;

	public event DragEventHandler? DragEnd;

	protected virtual bool DoubleBuffered { get; set; }

	protected bool DesignMode => false;

	public ImeMode ImeMode { get; set; }

	public static bool CheckForIllegalCrossThreadCalls { get; set; } = true;

	void IArrangedElement.SetBounds(Rectangle bounds, BoundsSpecified specified)
	{
		var site = Site;
		var changeService = site?.GetService<IComponentChangeService>();
		PropertyDescriptor? sizeProperty = null;
		PropertyDescriptor? locationProperty = null;
		var sizeChanged = false;
		var locationChanged = false;

		SetBoundsCore(bounds.X, bounds.Y, bounds.Width, bounds.Height, specified);

		if (changeService is not null)
		{
			try
			{
				if (sizeChanged)
				{
					changeService.OnComponentChanged(this, sizeProperty);
				}

				if (locationChanged)
				{
					changeService.OnComponentChanged(this, locationProperty);
				}
			}
			catch (InvalidOperationException)
			{
				// The component change events can throw InvalidOperationException if a change is
				// currently not allowed (typically because the doc data in VS is locked).
				// When this happens, we just eat the exception and proceed with the change.
			}
		}
	}

	void IArrangedElement.PerformLayout(IArrangedElement affectedElement, string affectedProperty)
	{
		PerformLayout(new LayoutEventArgs(affectedElement, affectedProperty));
	}

	public static Font DefaultFont { get; } = new Font("Tahoma", 8);

	public const int DefaultFontHeight = 13;

	public static Color DefaultForeColor => Color.Black;

	public static Color DefaultBackColor => SystemColors.Control;

	public static MouseButtons MouseButtons { get; set; } = MouseButtons.None;

	public static Point MousePosition { get; protected set; }

	public static Keys ModifierKeys { get; private set; }

	public bool ParticipatesInLayout => GetState(States.Visible) && !RenderInPortal;

	PropertyStore IArrangedElement.Properties
	{
		get { return Properties; }
	}

	internal PropertyStore Properties { get; }

	IArrangedElement? IArrangedElement.Container => Parent;

	internal virtual string WindowText
	{
		get => windowText ?? text ?? string.Empty;
		set
		{
			windowText = value;
		}
	}
	string? windowText;

	public void ResetBindings()
	{
		DataBindings?.Clear();
	}

	protected internal IJSRuntime? JSRuntime => FindForm()?.CargoWiseClientServices?.JSRuntime;

	public T? GetJSInterop<T>() where T : IJSInterop => GetRequiredService<T>();

	public T? GetService<T>() where T : notnull
	{
		if (Proxy?.ServiceProvider is null)
		{
			return default(T);
		}
		return Proxy.ServiceProvider.GetService<T>();
	}

	public T? GetRequiredService<T>() where T : notnull
	{
		if (Proxy?.ServiceProvider is null)
		{
			return default(T);
		}
		return Proxy.ServiceProvider.GetRequiredService<T>();
	}

	internal virtual bool ShouldSerializeBackColor()
	{
		return !backColor.IsEmpty;
	}

	internal virtual bool CanProcessMnemonic()
	{
		if (!Enabled || !Visible || FindForm()?.CurrentKeyEvent != KeyEventType.KeyDown)
		{
			return false;
		}

		return Parent?.CanProcessMnemonic() ?? true;
	}

	public static bool IsMnemonic(char charCode, string? text)
	{
		if (charCode == '&')
		{
			return false;
		}

		if (text is not null)
		{
			var pos = -1; // start with -1 to handle double &'s
			var c2 = char.ToUpper(charCode, CultureInfo.CurrentCulture);
			for (; ; )
			{
				if (pos + 1 >= text.Length)
				{
					break;
				}

				pos = text.IndexOf('&', pos + 1) + 1;
				if (pos <= 0 || pos >= text.Length)
				{
					break;
				}

				var c1 = char.ToUpper(text[pos], CultureInfo.CurrentCulture);
				if (c1 == c2 || char.ToLower(c1, CultureInfo.CurrentCulture) == char.ToLower(c2, CultureInfo.CurrentCulture))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected internal virtual bool ProcessMnemonic(char charCode) => false;

	protected virtual void OnAutoSizeChanged(EventArgs e)
	{
		AutoSizeChanged?.Invoke(this, e);
	}

	// Used by form to notify the control that it has been "entered"
	internal void NotifyEnter()
	{
		OnEnter(EventArgs.Empty);
	}

	// Used by form to notify the control that it has been "left"
	internal void NotifyLeave()
	{
		OnLeave(EventArgs.Empty);
	}

	internal bool IsDescendant(Control? descendant)
	{
		var control = descendant;
		while (control is not null)
		{
			if (control == this)
			{
				return true;
			}

			control = control.Parent;
		}

		return false;
	}

	protected virtual void OnParentBindingContextChanged(EventArgs e)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	///  Stores information about the last button or combination pressed by the user.
	/// </summary>
	protected static Keys LastKeyData { get; set; }

	bool NotifyValidating()
	{
		CancelEventArgs ev = new CancelEventArgs();
		OnValidating(ev);
		return ev.Cancel;
	}

	/// <summary>
	///  Performs data validation (not paint validation!) on a single control.
	///
	///  Returns whether validation failed:
	///  False = Validation succeeded, control is valid, accept its new value
	///  True = Validation was cancelled, control is invalid, reject its new value
	///
	///  NOTE: This is the lowest possible level of validation. It does not account
	///  for the context in which the validation is occurring, eg. change of focus
	///  between controls in a container. Stuff like that is handled by the caller.
	/// </summary>
	internal bool PerformControlValidation(bool bulkValidation)
	{
		// Skip validation for controls that don't support it
		if (!CausesValidation)
		{
			return false;
		}

		// Raise the 'Validating' event. Stop now if handler cancels (ie. control is invalid).
		// NOTE: Handler may throw an exception here, but we must not attempt to catch it.
		if (NotifyValidating())
		{
			return true;
		}

		// Raise the 'Validated' event. Handlers may throw exceptions here too - but
		// convert these to ThreadException events, unless the control is being validated
		// as part of a bulk validation operation.
		if (bulkValidation)
		{
			OnValidated(EventArgs.Empty);
		}
		else
		{
			try
			{
				OnValidated(EventArgs.Empty);
			}
			catch (Exception e)
			{
				Application.OnThreadException(e);
			}
		}

		return false;
	}

	internal virtual void NotifyValidationResult(object? sender, CancelEventArgs ev)
	{
		ValidationCancelled = ev.Cancel;
	}

	internal bool ValidationCancelled
	{
		set => SetState(States.ValidationCancelled, value);
		get
		{
			if (GetState(States.ValidationCancelled))
			{
				return true;
			}
			else
			{
				var parent = Parent;
				if (parent is not null)
				{
					return parent.ValidationCancelled;
				}

				return false;
			}
		}
	}

	/// <summary>
	///  Find ContainerControl that is the container of this control.
	/// </summary>
	internal ContainerControl? ParentContainerControl
	{
		get
		{
			for (Control? c = Parent; c is not null; c = c.Parent)
			{
				if (c is ContainerControl)
				{
					return c as ContainerControl;
				}
			}

			return null;
		}
	}

	/// <summary>
	///  Determine effective auto-validation setting for a given control, based on the AutoValidate property
	///  of its containing control. Defaults to 'EnablePreventFocusChange' if there is no containing control
	///  (eg. because this control is a top-level container).
	/// </summary>
	internal static AutoValidate GetAutoValidateForControl(Control control)
	{
		ContainerControl? parent = control.ParentContainerControl;
		return (parent is not null) ? parent.AutoValidate : AutoValidate.EnablePreventFocusChange;
	}

	internal bool ShouldAutoValidate => GetAutoValidateForControl(this) != AutoValidate.Disable;

	internal ToolStripControlHost? ToolStripControlHost;

	internal bool BecomingActiveControl;

	internal protected virtual bool StopLeaveOnNoActiveControl => false;

#if DEBUG

	public void InvokeMouseEvent(string type, MouseEventArgs args)
	{
		switch (type)
		{
			case "MouseDown":
				OnMouseDown(args);
				break;
			case "MouseUp":
				OnMouseUp(args);
				break;
		}
	}

	public void InvokeProcessKeyEvent(string type, string rawKey, Keys key, bool translateMessage = true)
	{
		var eventType = Enum.Parse<KeyEventType>(type, ignoreCase: true);
		var altKey = (key & Keys.Alt) == Keys.Alt;
		var ctrlKey = (key & Keys.Control) == Keys.Control;
		var shiftKey = (key & Keys.Shift) == Keys.Shift;
		ProcessKeyEventCore(eventType, rawKey, key, altKey, ctrlKey, shiftKey, translateMessage);
	}

#endif

	internal void ProcessKeyEventCore(KeyEventType eventType, string rawKey, Keys key, bool altKey, bool ctrlKey, bool shiftKey, bool translateMessage = true)
	{
		supressKeyPress = false;
		var message = GetKeyMessage(eventType, rawKey, key, altKey);

		// Adapted from PreProcessControlMessageInternal
		var modifiers = EventExtensions.GetModifierKeys(altKey, ctrlKey, shiftKey);
		var keyData = key | modifiers;

		ModifierKeys = modifiers;

		if (!PreProcessMessage(ref message, keyData))
		{
			// In WinForms at this point they dispatch the KeyEvent to be handled by Windows
			// This next part is adapted from WmKeyChar which gets called by WndProc when these
			// events are fired by Windows.
			ProcessKeyMessage(ref message);

			if (translateMessage)
			{
				if (eventType == KeyEventType.KeyDown && !supressKeyPress && (key.ProducesVisibleChar(ctrlKey) || AllowNonCharKeyPress(key)))
				{
					ProcessKeyEventCore(KeyEventType.WMCHAR, rawKey, key, altKey, ctrlKey, shiftKey, false);
				}
			}
		}

		if (translateMessage)
		{
			// When pressing a combination in controlKey + `a-Z`(key) range,
			// We need to call the ProcessKeyMessage method an additional time to trigger it.
			if (ctrlKey && key != Keys.ControlKey && key >= Keys.A && key <= Keys.Z)
			{
				var additionalKeyMessage = new Message() { Msg = WM_CHAR, WParam = key - Keys.A + 1 };
				ProcessKeyMessage(ref additionalKeyMessage);
			}
		}
	}

	internal virtual void ProcessKeyEvent(KeyEventType eventType, WinzorKeyboardEventArgs e)
	{
		ProcessKeyEventCore(eventType, e.Key, e.GetKey(), e.AltKey, e.CtrlKey, e.ShiftKey);
	}

	protected bool supressKeyPress;

	private protected virtual bool AllowNonCharKeyPress(Keys key) => key == Keys.Back || key == Keys.Space || key == Keys.Enter || key == Keys.Escape;

	Message GetKeyMessage(KeyEventType eventType, string rawKey, Keys key, bool altKey)
	{
		var isSystemMessage = altKey || key == Keys.F10;
		if (eventType == KeyEventType.WMCHAR)
		{
			var wParam = key switch
			{
				_ when key == Keys.Enter => (char)Keys.Enter,
				_ when key == Keys.Back => (char)Keys.Back,
				_ when key == Keys.Escape => (char)Keys.Escape,
				_ => rawKey.FirstOrDefault(),
			};
			return new Message() { Msg = isSystemMessage ? WM_SYSCHAR : WM_CHAR, WParam = wParam, HWnd = Handle };
		}
		else
		{
			if (eventType == KeyEventType.KeyDown)
			{
				return new Message() { Msg = isSystemMessage ? WM_SYSKEYDOWN : WM_KEYDOWN, WParam = (IntPtr)key, HWnd = Handle };
			}
			else
			{
				return new Message() { Msg = isSystemMessage ? WM_SYSKEYUP : WM_KEYUP, WParam = (IntPtr)key, HWnd = Handle };
			}
		}
	}

	public virtual bool PreProcessMessage(ref Message msg, Keys keyData)
	{
		bool result = false;

		if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
		{
			if (ProcessCmdKey(ref msg, keyData))
			{
				result = true;
			}
			else if (IsInputKey(keyData))
			{
				result = false;
			}
			else
			{
				result = ProcessDialogKey(keyData);
			}
		}
		else if (msg.Msg == WM_CHAR || msg.Msg == WM_SYSCHAR)
		{
			if (msg.Msg == WM_CHAR && IsInputChar((char)msg.WParam))
			{
				result = false;
			}
			else
			{
				result = ProcessDialogChar((char)msg.WParam);
			}
		}

		return result;
	}

	protected virtual bool WantArrowKeys => false;

	protected virtual bool WantAllKeys => false;

	protected virtual bool WantTab => false;

	protected virtual bool WantChars => false;

	protected virtual bool IsInputKey(Keys keyData)
	{
		if ((keyData & Keys.Alt) == Keys.Alt)
		{
			return false;
		}

		switch (keyData & Keys.KeyCode)
		{
			case Keys.Tab:
				return WantAllKeys || WantTab;
			case Keys.Left:
			case Keys.Right:
			case Keys.Up:
			case Keys.Down:
				return IsHandleCreated && WantArrowKeys;
		}

		return WantAllKeys;
	}

	protected virtual bool IsInputChar(char charCode)
	{
		if (charCode == (char)(int)Keys.Tab)
		{
			return WantTab;
		}

		return WantChars || WantAllKeys;
	}

	protected virtual bool ProcessDialogChar(char charCode) => Parent?.ProcessDialogChar(charCode) ?? false;

	internal bool IsMouseEnterSubscribed() => (MouseEnter?.GetInvocationList().Length > 0);

	internal bool IsMouseLeaveSubscribed() => (MouseLeave?.GetInvocationList().Length > 0);

	internal enum KeyEventType
	{
		None,
		KeyDown,
		KeyUp,
		WMCHAR,
	}

	internal KeyEventType CurrentKeyEvent
	{
		get
		{
			return currentKeyEvent;
		}
		set
		{
			currentKeyEvent = value;
		}
	}
	KeyEventType currentKeyEvent = KeyEventType.None;

	//
	// Summary:
	//     Converts the specified System.Drawing.ContentAlignment to the appropriate System.Drawing.ContentAlignment
	//     to support right-to-left text.
	//
	// Parameters:
	//   align:
	//     One of the System.Drawing.ContentAlignment values.
	//
	// Returns:
	//     One of the System.Drawing.ContentAlignment values.
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	protected internal ContentAlignment RtlTranslateContent(ContentAlignment align)
	{
		if (RightToLeft.Yes == RightToLeft)
		{
			if ((align & WindowsFormsUtils.AnyTopAlign) != 0)
			{
				switch (align)
				{
					case ContentAlignment.TopLeft:
						return ContentAlignment.TopRight;
					case ContentAlignment.TopRight:
						return ContentAlignment.TopLeft;
				}
			}

			if ((align & WindowsFormsUtils.AnyMiddleAlign) != 0)
			{
				switch (align)
				{
					case ContentAlignment.MiddleLeft:
						return ContentAlignment.MiddleRight;
					case ContentAlignment.MiddleRight:
						return ContentAlignment.MiddleLeft;
				}
			}

			if ((align & WindowsFormsUtils.AnyBottomAlign) != 0)
			{
				switch (align)
				{
					case ContentAlignment.BottomLeft:
						return ContentAlignment.BottomRight;
					case ContentAlignment.BottomRight:
						return ContentAlignment.BottomLeft;
				}
			}
		}

		return align;
	}

	/// <summary>
	///  Performs the work of scaling the entire control and any child controls.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected virtual void ScaleCore(float dx, float dy) { }

	/// <summary>
	///  Scales an individual control's location, size, padding and margin.
	///  If the control is top level, this will not scale the control's location.
	///  This does not scale children or the size of auto sized controls.  You can
	///  omit scaling in any direction by changing BoundsSpecified.
	///
	///  After the control is scaled the RequiredScaling property is set to
	///  BoundsSpecified.None.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	protected virtual void ScaleControl(SizeF factor, BoundsSpecified specified) { }

	/// <summary>
	/// Winzor-only. Does the control have a visible/active context menu?
	/// </summary>
	protected internal bool HasActiveContextMenu;

	#region WinzorPaste Event Handler
	protected virtual bool AllowWinzorPaste { get { return false; } }

	protected virtual async Task OnWinzorPasteAsync(WinzorPasteEventArgs args)
	{
		if (!AllowWinzorPaste)
		{
			return;
		}

		await WithExceptionHandlingAsync(async () =>
		{
			await OnWinzorPasteCoreAsync(args);
		});
	}

	protected virtual async Task OnWinzorPasteCoreAsync(WinzorPasteEventArgs args)
	{
		await Task.CompletedTask;
	}

	#endregion
}
