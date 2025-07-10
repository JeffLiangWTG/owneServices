using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms.Layout;
using System.Windows.Input;
using BArchitecture;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Blazor.Client.Integration.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using WinzorFramework.Telemetry;
using IntegrationFormBorderStyle = CargoWise.Blazor.Client.Integration.Messaging.FormBorderStyle;
using IntegrationWindowState = CargoWise.Blazor.Client.Integration.Messaging.FormWindowState;

namespace System.Windows.Forms;

public partial class Form : ContainerControl
{
	public Form() : this(true)
	{
		// redirect flow to another ctor
	}

	public Form(bool initDefaultFormBorderStyle)
	{
		SetState(States.Visible, false);
		SetState(States.TopLevel, true);

		if (initDefaultFormBorderStyle)
		{
			FormBorderStyle = FormBorderStyle.Sizable;
		}
		mainMenuInterop = new MenuInterop(this, MenuType.MainMenu, WinzorControlGuid);
	}

	/// <summary>
	///  Returns a string representation for this control.
	/// </summary>
	public override string ToString()
	{
		var s = base.ToString();
		return s + ", Text: " + Text;
	}

	public void ReadyToRender(CargoWiseClientServices clientServices, Guid? windowId = null)
	{
		if (CargoWiseClientServices != null)
		{
			throw new InvalidOperationException("ReadyToRender has already been called on this form");
		}

		CargoWiseClientServices = clientServices;
		WinzorWindowId = windowId;

		if (!GetState(States.Visible))
		{
			// Attempting to render a form that has not been shown yet
			// This is a known issue with the SplashScreen and PlayWright tests
			// To be addressed in WI00803733 - Remove Winzor specific logic from Form.ReadyToRender

			// In the meantime - we should run all the Form.Show logic
			// However we should not attempt to open the form as it's already open
			skipOpenForm = true;
			Show();
			skipOpenForm = false;
		}
	}

	public override async Task DetachFromRendererAsync()
	{
		await base.DetachFromRendererAsync();
		shown = false;
	}

	public IFormJSInterop? Interop => GetJSInterop<IFormJSInterop>();

	internal Uri? Uri => Proxy?.ServiceProvider?.GetRequiredService<NavigationManager>().Uri is string uri
		? new Uri(uri)
		: null;

	protected internal override async Task OnInitializedAsync()
	{
		using var activity = TelemetryService.ActivitySource?.StartActivity($"{GetType().Name}.{nameof(OnInitializedAsync)}");
		var logger = GetService<ILogger<Form>>();
		var circuitId = GetService<ICircuitIdProvider>()?.CircuitId;
		logger?.LogInformation($"Initialized form {GetType().Name} on circuit {circuitId}");
		await base.OnInitializedAsync();
		await (Interop?.InitializeAsync(this) ?? Task.CompletedTask);
		await RenderMainMenuAsync();
		GetJSInterop<IClientEventServiceJSInterop>()?.PreloadInterop();
		GetJSInterop<IClipboardJSInterop>()?.PreloadInterop();
		GetJSInterop<IFileServiceJSInterop>()?.PreloadInterop();
	}

	async Task RenderMainMenuAsync()
	{
		using var activity = TelemetryService.ActivitySource?.StartActivity($"{GetType().Name}.{nameof(RenderMainMenuAsync)}");
		if (IsClosing || IsDisposed)
		{
			throw new InvalidOperationException("RenderMainMenu cannot be invoked when form is closing or disposed.");
		}

		if (mainMenuInterop is not null && Menu?.MenuItems is { } menuItems && menuItems.Count > 0)
		{
			await mainMenuInterop.ShowAsync(menuItems.ToArray(), Point.Empty);
		}
	}

	readonly MenuInterop mainMenuInterop;

	protected internal override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender)
		{
			await (CargoWiseClientServices?.WindowService?.RegisterCloseListenerAsync(CloseHandlerAsync) ?? Task.CompletedTask);
			if (CargoWiseClientServices?.ClientEventService is not null)
			{
				altKeyEvent = await CargoWiseClientServices.ClientEventService.RegisterGlobalKeyEventListenerAsync(HandleAltKeyPressAsync, ClientKeyEvent.KeyDown, new ClientKeyEventData("Alt", true));
			}
			if (!resizedFromClient && MainMenuStrip is null)
			{
				await (Interop?.ResizeWindowWhenNoMainMenuStripAsync(this, this.Height, this.Width) ?? Task.CompletedTask);
			}
		}
	}

	public CargoWiseClientServices? CargoWiseClientServices
	{
		get => cargoWiseClientServices;
		private set => cargoWiseClientServices = value;
	}
	CargoWiseClientServices? cargoWiseClientServices;

	public override bool UseParentDivForLayout => false;

	internal async Task CloseHandlerAsync()
	{
		using var activity = TelemetryService.ActivitySource?.StartActivity($"{GetType().Name}.{nameof(CloseHandlerAsync)}");
		await InvokeRenderDispatcherAsync(async () => await (CargoWiseClientServices?.WindowService.CloseRequestReceivedAsync() ?? Task.CompletedTask));
		await InvokeWinzorDispatcherAsync(() => Close(), executeIfDisposed: true);
	}

	void SetFormSizeOnClient(Size preferUnscaledSize)
	{
		//When initializing the form, we should send the form size in the RequestShowWindowAsync call (to minimize redundant calls).
		if (!ClientWindowOpenSent)
		{
			return;
		}

		Size minimumSize = MinimumSize, maximumSize = MaximumSize;

		if (AutoSize)
		{
			var preferSize = PreferredSize;
			minimumSize = LayoutUtils.UnionSizes(minimumSize, preferSize);
			if (AutoSizeMode == AutoSizeMode.GrowAndShrink)
			{
				maximumSize = preferSize;
			}
		}

		InvokeRenderDispatcher(async () =>
		{
			await (CargoWiseClientServices?.WindowService?.RequestUpdateWindowStyleAsync(new WindowStyleOptions { PreferredSizeUnscaled = preferUnscaledSize, PreferredMinimumSizeUnscaled = minimumSize, PreferredMaximumSizeUnscaled = maximumSize }) ?? Task.CompletedTask);
		});
	}

	protected override Size DefaultSize => new Size(300, 300);

	internal CommonDialog? CommonDialog { get; set; }

	Size minimumSize;
	public override Size MinimumSize
	{
		get
		{
			return minimumSize.IsEmpty ? DefaultMinimumSize : minimumSize;
		}
		set
		{
			if (value.Width < 0 || value.Height < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(MinimumSize));
			}

			if (UpdateProperty(ref minimumSize, value))
			{
				// Keep form size within new limits
				var size = Size;
				if (size.Width < value.Width || size.Height < value.Height)
				{
					Size = new Size(Math.Max(size.Width, value.Width), Math.Max(size.Height, value.Height));
				}

				OnMinimumSizeChanged(EventArgs.Empty);
			}
		}
	}

	Size maximumSize;
	public override Size MaximumSize
	{
		get
		{
			return maximumSize.IsEmpty ? DefaultMaximumSize : maximumSize;
		}
		set
		{
			if (value.Width < 0 || value.Height < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(MaximumSize));
			}

			if (UpdateProperty(ref maximumSize, value))
			{
				// Keep form size within new limits
				var size = Size;
				if (!value.IsEmpty && (size.Width > value.Width || size.Height > value.Height))
				{
					Size = new Size(Math.Min(size.Width, value.Width), Math.Min(size.Height, value.Height));
				}

				OnMaximumSizeChanged(EventArgs.Empty);
			}
		}
	}

	/// <summary>
	///  Makes the control display by setting the Visible property to true
	/// </summary>
	public void Show(IWin32Window? owner)
	{
		if (owner == this)
		{
			throw new InvalidOperationException(string.Format(SR.OwnsSelfOrOwner, nameof(Show)));
		}
		else if (Visible)
		{
			throw new InvalidOperationException(string.Format(SR.ShowDialogOnVisible, nameof(Show)));
		}
		else if (!Enabled)
		{
			throw new InvalidOperationException(string.Format(SR.ShowDialogOnDisabled, nameof(Show)));
		}
		else if (!TopLevel)
		{
			throw new InvalidOperationException(string.Format(SR.ShowDialogOnNonTopLevel, nameof(Show)));
		}

		if (owner is Form ownerForm)
		{
			if (!ownerForm.TopMost)
			{
				// It's not the top-most window
				owner = ownerForm.TopLevelControlInternal;
			}

			// Catch the case of a window trying to own its owner
			if (ownerForm.Owner == this)
			{
				throw new ArgumentException(string.Format(SR.OwnsSelfOrOwner, nameof(Show)), nameof(owner));
			}

			// Set the new owner.
			if (ownerForm != Owner)
			{
				Owner = ownerForm;
			}
		}
		Visible = true;
	}

	public bool IsClosing { get; private set; }

	/// <summary>
	///  Displays this form as a modal dialog box with no owner window.
	/// </summary>
	public DialogResult ShowDialog()
		=> ShowDialog(null);

	/// <summary>
	///  Shows this form as a modal dialog with the specified owner.
	/// </summary>
	public DialogResult ShowDialog(IWin32Window? owner)
	{
		if (owner == this)
		{
			throw new ArgumentException(string.Format(SR.OwnsSelfOrOwner, nameof(ShowDialog)), nameof(owner));
		}
		else if (Visible)
		{
			throw new InvalidOperationException(string.Format(SR.ShowDialogOnVisible, nameof(ShowDialog)));
		}
		else if (!Enabled)
		{
			throw new InvalidOperationException(string.Format(SR.ShowDialogOnDisabled, nameof(ShowDialog)));
		}
		else if (!TopLevel)
		{
			throw new InvalidOperationException(string.Format(SR.ShowDialogOnNonTopLevel, nameof(ShowDialog)));
		}
		else if (Modal)
		{
			throw new InvalidOperationException(string.Format(SR.ShowDialogOnModal, nameof(ShowDialog)));
		}

		using var activity = TelemetryService.ActivitySource?.StartActivity($"{GetType().Name}.{nameof(ShowDialog)}");

		CalledOnLoad = false;
		CalledMakeVisible = false;

		// for modal dialogs make sure we reset close reason.
		CloseReason = CloseReason.None;

		var oldOwner = Owner;

		if (owner is Form ownerForm)
		{
			if (!ownerForm.TopMost)
			{
				// It's not the top-most window
				owner = ownerForm.TopLevelControlInternal;
			}

			// Catch the case of a window trying to own its owner
			if (ownerForm.Owner == this)
			{
				throw new ArgumentException(string.Format(SR.OwnsSelfOrOwner, nameof(ShowDialog)), nameof(owner));
			}

			// Set the new owner.
			if (ownerForm != Owner)
			{
				Owner = ownerForm;
			}
		}

		try
		{
			SetState(States.Modal, true);
			
			// In WinForms when a modal is opened on a separate thread the parent remains active as it is on a separate message loop.
			// We imitate this in Winzor by opening the window on the client as non-modal despite maintaining the modal behavior on the server.
			ModalWindow = WinzorDispatcher.Current.IsSameAs(WinzorDispatcher.Current.CurrentContext?.Form?.WinzorDispatcher);

			// It's possible that while in the process of creating the control,
			// (i.e. inside the CreateControl() call) the dialog can be closed.
			// e.g. A user might call Close() inside the OnLoad() event.
			// Calling Close() will set the DialogResult to some value, so that
			// we'll know to terminate the RunDialog loop immediately.
			// Thus we must initialize the DialogResult *before* the call
			// to CreateControl().
			DialogResult = DialogResult.None;

			// If "this" is an MDI parent then the window gets activated,
			// causing GetActiveWindow to return "this.handle"... to prevent setting
			// the owner of this to this, we must create the control AFTER calling
			// GetActiveWindow.
			CreateControl();

			try
			{
				// If the DialogResult was already set, then there's
				// no need to actually display the dialog.
				if (DialogResult == DialogResult.None)
				{
					dialogCts = new CancellationTokenSource();

					Show();
					WinzorDispatcher.Current.RunMessageLoop(dialogCts, contextForm: this);
				}
			}
			finally
			{
				SetVisibleCore(false);
				if (IsHandleCreated)
				{
					DestroyHandle();
				}
				SetState(States.Modal, false);
			}
		}
		finally
		{
			Owner = oldOwner;
		}
		return DialogResult;
	}

	internal CloseReason CloseReason { get; set; } = CloseReason.None;

	bool skipOpenForm;

	/// <summary>
	/// The Winzor specific implementation of User32 ShowWindow when the form is being shown
	/// </summary>
	void ShowCore()
	{
		using var activity = TelemetryService.ActivitySource?.StartActivity($"{GetType().Name}.{nameof(ShowCore)}");

		if (!skipOpenForm)
		{
			if (CargoWiseClientServices is not null)
			{
				// The form is currently hidden
				InvokeRenderDispatcher(async () => await (CargoWiseClientServices?.WindowService.RequestShowWindowAsync(GenerateShowWindowOptions(), GenerateWindowStyleOptions()) ?? Task.CompletedTask));
			}
			else
			{
				WinzorDispatcher.Current.CurrentContext?.OpenForm(this);
			}
		}
		SetActiveForm(this, this);
	}

	/// <summary>
	/// The Winzor specific implementation of User32 ShowWindow when the form is being hidden
	/// </summary>
	void HideCore()
	{
		InvokeRenderDispatcher(async () => await (CargoWiseClientServices?.WindowService.RequestHideWindowAsync() ?? Task.CompletedTask));
		SetActiveForm(null, this);
	}

	/// <summary>
	///  Raises the FormClosed event for this form when Application.Exit is called.
	/// </summary>
	internal void RaiseFormClosedOnAppExit()
	{
		if (!Modal)
		{
			// Fire FormClosed event on all the forms that this form owns and are not in the Application.OpenForms collection
			// This is to be consistent with what WmClose does.
			var fce = new FormClosedEventArgs(CloseReason.FormOwnerClosing);
			foreach (var ownedForm in ownedForms.GetSnapshot())
			{
				if (ownedForm is not null && !Application.OpenForms.Contains(ownedForm))
				{
					ownedForm.OnFormClosed(fce);
					ownedForm.CloseClientWindow();
					ownedForm.dialogCts?.Cancel();
					ownedForm.CommonDialog?.EndModalMessageLoop();
				}
			}
		}

		OnFormClosed(new FormClosedEventArgs(CloseReason.ApplicationExitCall));

		CloseClientWindow();
		dialogCts?.Cancel();
		CommonDialog?.EndModalMessageLoop();
	}

	protected override void SetVisibleCore(bool value)
	{
		// If DialogResult.OK and the value == GetVisibleCore() then this code has been called either through
		// ShowDialog() or explicit Hide() by the user. So don't go through this function again.
		// This will avoid flashing during closing the dialog;
		if (GetVisibleCore() == value && DialogResult == DialogResult.OK)
		{
			return;
		}

		// (!value || calledMakeVisible) is to make sure that we fall
		// through and execute the code below at least once.
		if (GetVisibleCore() == value && (!value || CalledMakeVisible))
		{
			base.SetVisibleCore(value);
			return;
		}

		if (value)
		{
			CalledMakeVisible = true;
			if (CalledCreateControl)
			{
				if (CalledOnLoad)
				{
					// Make sure the form is in the Application.OpenForms collection
					if (!Application.OpenForms.Contains(this))
					{
						Application.OpenForms.Add(this);
					}
				}
				else
				{
					CalledOnLoad = true;
					OnLoad(EventArgs.Empty);
					if (DialogResult != DialogResult.None)
					{
						// Don't show the dialog if the dialog result was set in the OnLoad event.
						value = false;
					}
				}
			}
		}

		base.SetVisibleCore(value);

		OnFrameWindowActivate();

		ReadyToRender();

		if (IsHandleCreated || value)
		{
			if (value)
			{
				ShowCore();
			}
			else
			{
				HideCore();
			}
		}
	}

	bool shown;
	protected internal bool resizedFromClient;

	[JSInvokable]
	public virtual async Task OnBrowserSizeChangedAsync(int jsBrowserWidth, int jsBrowserHeight)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			resizedFromClient = true;
			ClientSize = new Size(jsBrowserWidth, jsBrowserHeight);
			resizedFromClient = false;
			NotifyRenderRequired();
		});
	}

	[JSInvokable]
	public async Task OnWindowStateChangeAsync(string windowState)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			this.windowState = windowState switch
			{
				"Maximized" => FormWindowState.Maximized,
				"Minimized" => FormWindowState.Minimized,
				_ => FormWindowState.Normal
			};
		});
	}

	protected override void OnClientSizeChanged(EventArgs e)
	{
		if (!resizedFromClient && IsHandleCreated)
		{
			SetFormSizeOnClient(Bounds.Size);
		}
		base.OnClientSizeChanged(e);
	}

	/// <summary>
	///  Stores information about the last button or combination pressed by the user.
	/// </summary>
	protected internal Keys LastFormKeyData { get; set; }

	[JSInvokable]
	public async Task OnFormKeyEventAsync(WinzorKeyboardEventArgs args, string targetWinzorControlId)
	{
		EventExtensions.SetKeyDownStatus(args);
		EventExtensions.GetModifierState(args);
		OnUserActivity();

		await InvokeWinzorDispatcherAsync(() =>
		{
			try
			{
				//WinForms sends key events to the foreground form.
				//This is being replicated here by using ActiveForm, to prevent race conditions.
				var currentTarget = FindDescendantByWinzorControlId(targetWinzorControlId) ?? LastFocusedControl ?? this;
				if (ActiveForm != null && currentTarget.FindForm() != ActiveForm)
				{
					if (!ActiveForm.HasRendered)
					{
						return;
					}
					currentTarget = ActiveForm;
				}

				LastFormKeyData = args.GetKey();
				CurrentKeyEvent = Enum.Parse<KeyEventType>(args.Type, ignoreCase: true);
				currentTarget.ProcessKeyEvent(CurrentKeyEvent, args);
			}
			catch (Exception e)
			{
				throw new Exception("The detail for the Keyboard Event: " + "{" + $"Code:{args.Code}, Key:{args.Key}, CtrlKey:{args.CtrlKey}, ShiftKey:{args.ShiftKey}, AltKey:{args.AltKey}, MetaKey:{args.MetaKey}" + "}", e);
			}
			finally
			{
				CurrentKeyEvent = KeyEventType.None;
			}
		});
	}

	[JSInvokable]
	public async Task OnChangeCursorAsync(string targetWinzorControlId, string cursorStyle)
	{
		var target = FindDescendantByWinzorControlId(targetWinzorControlId) ?? this;
		await target.InvokeWinzorDispatcherAsync(() =>
		{
			var newCursor = cursorStyle switch
			{
				_ when cursorStyle == Cursors.No.CursorData => Cursors.No,
				_ when cursorStyle == Cursors.IBeam.CursorData => Cursors.IBeam,
				_ when cursorStyle == Cursors.Default.CursorData => Cursors.Default,
				_ => Cursors.Default
			};

			target.Cursor = newCursor;
		});
	}

	CancellationTokenSource? dialogCts;

	public bool Modal => GetState(States.Modal);

	/// <summary>
	/// Determines if the ClientApp Window should open as a modal.
	/// Generally this is set to false when showing the form normally and true when the form is shown as a dialog.
	/// However in some cases, such as when a form is shown as a dialog on a separate thread, this will be set to false.
	/// </summary>
	protected internal bool ModalWindow { get; set; }

	public Form? Owner
	{
		get => owner;
		set
		{
			if (owner != value)
			{
				if (owner != null)
				{
					owner.RemoveOwnedForm(this);
				}

				if (value != null)
				{
					value.AddOwnedForm(this);
				}

				owner = value;
			}
		}
	}

	Form? owner;

	public Form[] OwnedForms => ownedForms?.ToArray() ?? Array.Empty<Form>();

	readonly WrappedList<Form> ownedForms = new WrappedList<Form>();

	public void AddOwnedForm(Form ownedForm)
	{
		ownedForms.Add(ownedForm);
	}

	public void RemoveOwnedForm(Form ownedForm)
	{
		ownedForms.Remove(ownedForm);
	}

	static readonly int PropDefaultButton = PropertyStore.CreateKey();

	static readonly int PropAcceptButton = PropertyStore.CreateKey();

	static readonly int PropCancelButton = PropertyStore.CreateKey();

	public IButtonControl? AcceptButton
	{
		get
		{
			return (IButtonControl?)base.Properties.GetObject(PropAcceptButton);
		}
		set
		{
			if (AcceptButton != value)
			{
				Properties.SetObject(PropAcceptButton, value);
				UpdateDefaultButton();
			}
		}
	}

	public IButtonControl? CancelButton
	{
		get
		{
			return (IButtonControl?)base.Properties.GetObject(PropCancelButton);
		}
		set
		{
			Properties.SetObject(PropCancelButton, value);
			if (value != null && value.DialogResult == DialogResult.None)
			{
				value.DialogResult = DialogResult.Cancel;
			}
		}
	}

	/// <summary>Updates which button is the default button.</summary>
	/// 
	protected override void UpdateDefaultButton()
	{
		ContainerControl? containerControl = this;
		while (containerControl?.ActiveControl is ContainerControl)
		{
			containerControl = containerControl.ActiveControl as ContainerControl;
			if (containerControl is Form)
			{
				containerControl = this;
				break;
			}
		}
		if (containerControl?.ActiveControl is IButtonControl)
		{
			SetDefaultButton((IButtonControl)containerControl.ActiveControl);
		}
		else
		{
			SetDefaultButton(AcceptButton);
		}
	}

	void SetDefaultButton(IButtonControl? button)
	{
		var defaultButton = (IButtonControl?)Properties.GetObject(PropDefaultButton);

		if (defaultButton != button)
		{
			if (defaultButton is not null)
			{
				defaultButton.NotifyDefault(false);
			}

			Properties.SetObject(PropDefaultButton, button);

			if (button is not null)
			{
				button.NotifyDefault(true);
			}
		}
	}

	public bool HelpButton { get; set; }

	FormBorderStyle formBorderStyle;
	public FormBorderStyle FormBorderStyle
	{
		get => formBorderStyle;
		set
		{
			if (UpdateProperty(ref formBorderStyle, value))
			{
				UpdateFormStyles(() => new WindowStyleOptions { FormBorderStyle = FormBorderStyleForClientMessage() });
				OnResize(EventArgs.Empty);
			}
		}
	}

	bool minimizeBox = true;
	public bool MinimizeBox
	{
		get => minimizeBox;
		set
		{
			if (UpdateProperty(ref minimizeBox, value))
			{
				UpdateFormStyles(() => new WindowStyleOptions { MinimizeBox = minimizeBox });
			}
		}
	}

	bool maximizeBox = true;
	public bool MaximizeBox
	{
		get => maximizeBox;
		set
		{
			if (UpdateProperty(ref maximizeBox, value))
			{
				UpdateFormStyles(() => new WindowStyleOptions { MaximizeBox = maximizeBox });
			}
		}
	}

	bool controlBox = true;
	public bool ControlBox
	{
		get => controlBox;
		set
		{
			if (UpdateProperty(ref controlBox, value))
			{
				UpdateFormStyles(() => new WindowStyleOptions { ControlBox = controlBox });
			}
		}
	}

	/// <summary>
	///  Gets or sets a value indicating whether to display the form as a top-level
	///  window.
	/// </summary>
	public bool TopLevel
		=> GetTopLevel();

	bool topMost;
	public bool TopMost
	{
		get => topMost;
		set
		{
			if (UpdateProperty(ref topMost, value))
			{
				UpdateFormStyles(() => new WindowStyleOptions { TopMost = topMost });
			}
		}
	}

	FormWindowState windowState;
	public FormWindowState WindowState
	{
		get => windowState;
		set
		{
			var oldState = windowState;

			if (UpdateProperty(ref windowState, value))
			{
				InvokeRenderDispatcher(async () => await (CargoWiseClientServices?.WindowService?.UpdateWindowStateAsync((IntegrationWindowState)WindowState) ?? Task.CompletedTask));

				if (oldState == FormWindowState.Normal && windowState != FormWindowState.Normal)
				{
					restoreBounds.Size = Size;
					restoreBounds.Location = Location;
				}
			}
		}
	}

	public FormStartPosition StartPosition { get; set; }

	public Rectangle RestoreBounds
	{
		get
		{
			if (restoreBounds.Width == -1
				&& restoreBounds.Height == -1
				&& restoreBounds.X == -1
				&& restoreBounds.Y == -1)
			{
				// Form scaling depends on this property being
				// set correctly.  In some cases (where the size has not yet been set or
				// has only been set to the default, restoreBounds will remain uninitialized until the
				// handle has been created.  In this case, return the current Bounds.
				return Bounds;
			}
			return restoreBounds;
		}
	}

	Rectangle restoreBounds = new Rectangle(-1, -1, -1, -1);

	public Rectangle DesktopBounds => Bounds;

	public bool ShowInTaskbar { get; set; }

	public Icon? Icon { get; set; }

	public bool ShowIcon { get; set; }

	public SizeGripStyle SizeGripStyle { get; set; }

	bool CalledCreateControl { get; set; }

	bool CalledMakeVisible { get; set; }

	bool CalledOnLoad { get; set; }

	bool autoSize;

	public override bool AutoSize
	{
		get => autoSize;
		set
		{
			if (value != autoSize)
			{
				autoSize = value;
				if (!value)
				{
					Size = CommonProperties.GetSpecifiedBounds(this).Size;
					// OnLayout only call SetFormSizeOnClient when AutoSize is True (this can reduce a lot of unnecessary calls to SetFormSizeOnClient),
					// so when AutoSize is setted to False, SetFormSizeOnClient will not called by OnLayout to refresh the MinimumSize and MaximumSize for Client App,
					// so AutoSize.Set need to call SetFormSizeOnClient when value is False
					SetFormSizeOnClient(Size.Empty);
				}

				LayoutTransaction.DoLayout(this, this, PropertyNames.AutoSize);
				OnAutoSizeChanged(EventArgs.Empty);
			}
		}
	}

	public AutoSizeMode AutoSizeMode
	{
		get => GetAutoSizeMode();
		set
		{
			if (GetAutoSizeMode() != value)
			{
				SetAutoSizeMode(value);
				UpdateFormStyles(GenerateWindowStyleOptions);
				var toLayout = DesignMode || Parent is null ? this : Parent;

				if (toLayout is not null)
				{
					// DefaultLayout does not keep anchor information until it needs to.  When
					// AutoSize became a common property, we could no longer blindly call into
					// DefaultLayout, so now we do a special InitLayout just for DefaultLayout.
					if (toLayout.LayoutEngine == DefaultLayout.Instance)
					{
						toLayout.LayoutEngine.InitLayout(this, BoundsSpecified.Size);
					}

					LayoutTransaction.DoLayout(toLayout, this, PropertyNames.AutoSize);
				}
			}
		}
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		if (AutoSize)
		{
			SetFormSizeOnClient(Size.Empty);
		}
		base.OnLayout(levent);
	}

	protected virtual bool ShowWithoutActivation { get; }

	public DialogResult DialogResult { get; set; }

	public MainMenu? Menu
	{
		get => menu;
		set
		{
			if (menu != value)
			{
				if (menu != null)
				{
					menu.form = null;
				}

				menu = value;
				if (menu != null)
				{
					if (menu.form is not null)
					{
						menu.form.Menu = null;
					}
					menu.form = this;
				}
			}
		}
	}
	MainMenu? menu;

	internal void RenderMainMenu()
	{
		if (ProxyInitialized)
		{
			InvokeRenderDispatcher(async () => await RenderMainMenuAsync());
		}
	}

	public MenuStrip? MainMenuStrip { get; set; }

	public bool KeyPreview { get; set; }

	protected override bool ProcessKeyPreview(ref Message m)
	{
		if (KeyPreview && ProcessKeyEventArgs(ref m))
		{
			return true;
		}

		return base.ProcessKeyPreview(ref m);
	}

	public void Close()
	{
		if (IsHandleCreated)
		{
			CloseReason = CloseReason.UserClosing;
			WMClose();
		}
		else
		{
			var fc = new FormClosedEventArgs(CloseReason.None);
			OnClosed(fc);
			OnFormClosed(fc);
			// When a form is closed, all resources created within the object are closed and the form is disposed.
			Dispose();
		}
	}

	// In WinForms, the WMClose event is setup to handle the WM_CLOSE, WM_QUERYENDSESSION, and WM_ENDSESSION messages.
	// WM_QUERYENDSESSION and WM_ENDSESSION are not relevant in Winzor, and all associated logic has been removed.
	protected virtual void WMClose()
	{
		var e = new FormClosingEventArgs(CloseReason, false);

		// Pass 1 - Closing
		if (Modal)
		{
			if (DialogResult == DialogResult.None)
			{
				DialogResult = DialogResult.Cancel;
			}
			CalledClosing = false;

			// if this comes back false, someone canceled the close.  we want
			// to call this here so that we can get the cancel event properly,
			// and if this is a WM_QUERYENDSESSION, appriopriately set the result
			// based on this call.
			//
			// NOTE: We should also check !Validate(true) below too in the modal case,
			// but we cannot, because we didn't do this in Everett, and doing so
			// now would introduce a breaking change. User can always validate in the
			// FormClosing event if they really need to.

			e.Cancel = !CheckCloseDialog(true);
		}
		else
		{
			e.Cancel = !Validate(true);

			//Always fire OnClosing irrespectively of the validation result
			//Pass the validation result into the EventArgs...

			// Call OnClosing/OnFormClosing on all the forms that current form owns.
			foreach (var ownedForm in ownedForms.GetSnapshot())
			{
				var cfe = new FormClosingEventArgs(CloseReason.FormOwnerClosing, e.Cancel);

				//Call OnFormClosing on the child forms.
				ownedForm.OnFormClosing(cfe);
				if (cfe.Cancel)
				{
					// Set the cancel flag for the Owner form
					e.Cancel = true;
					break;
				}
			}

			OnClosing(e);
			OnFormClosing(e);
		}

		if (Modal)
		{
			return;
		}

		// Pass 2 - Closing
		// Fire closed event on all children and ourselves
		FormClosedEventArgs fc;
		if (!e.Cancel)
		{
			IsClosing = true;
			// Call OnClosed/OnFormClosed on all the forms that current form owns.
			foreach (var ownedForm in ownedForms.GetSnapshot())
			{
				fc = new FormClosedEventArgs(CloseReason.FormOwnerClosing);
				if (ownedForm is not null)
				{
					//Call OnClosed and OnFormClosed on the child forms.
					ownedForm.OnClosed(fc);
					ownedForm.OnFormClosed(fc);
				}
			}

			fc = new FormClosedEventArgs(CloseReason);
			OnClosed(fc);
			OnFormClosed(fc);

			Dispose();
		}
	}

	internal bool CheckCloseDialog(bool closingOnly)
	{
		if (DialogResult == DialogResult.None && Visible)
		{
			return false;
		}
		try
		{
			var e = new FormClosingEventArgs(CloseReason, false);

			if (!CalledClosing)
			{
				OnClosing(e);
				OnFormClosing(e);
				if (e.Cancel)
				{
					DialogResult = DialogResult.None;
				}
				else
				{
					// we have called closing here, and it wasn't cancelled, so we're expecting a close
					// call again soon.
					CalledClosing = true;
				}
			}

			if (!closingOnly && DialogResult != DialogResult.None)
			{
				CloseClientWindow();
				IsClosing = true;
				var fc = new FormClosedEventArgs(CloseReason);
				OnClosed(fc);
				OnFormClosed(fc);
				dialogCts?.Cancel();

				// reset called closing.
				CalledClosing = false;
			}
		}
		catch (Exception e)
		{
			DialogResult = DialogResult.None;
			Application.OnThreadException(e);
		}
		return DialogResult != DialogResult.None || !Visible;
	}

	bool CalledClosing { get; set; }

	internal void CloseClientWindow()
	{
		var clientServices = Interlocked.Exchange(ref cargoWiseClientServices, null);
		if (clientServices is not null)
		{
			InvokeRenderDispatcher(clientServices.WindowService.RequestCloseAsync);
		}
	}

	internal void UpdateFormStyles(Func<WindowStyleOptions> createWindowStyleOptions)
	{
		UpdateStyles();

		//When initializing the form, we should send the form style in the RequestShowWindowAsync call (to minimize redundant calls).
		if (!ClientWindowOpenSent)
		{
			return;
		}

		var windowStyleOptions = createWindowStyleOptions();
		InvokeRenderDispatcher(async () => await (CargoWiseClientServices?.WindowService?.RequestUpdateWindowStyleAsync(windowStyleOptions) ?? Task.CompletedTask));
	}

	protected override void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			CalledOnLoad = false;
			CalledMakeVisible = false;
			CalledCreateControl = false;

			CloseClientWindow();
			dialogCts?.Cancel();
			CommonDialog?.EndModalMessageLoop();

				Owner = null;
			foreach (var ownedForm in OwnedForms)
			{
				ownedForm.Dispose();
			}
		}

		base.Dispose(disposing);
		SetActiveForm(null, this);
		MouseButtons = MouseButtons.None;
	}

	protected override void OnHandleDestroyed(EventArgs e)
	{
		base.OnHandleDestroyed(e);
		Application.OpenForms.Remove(this);
	}

	public void Activate()
	{
		if (Visible && IsHandleCreated && CargoWiseClientServices != null)
		{
			InvokeRenderDispatcher(() => CargoWiseClientServices.WindowService.FormActivateAsync());
			SetActiveForm(this, this);
		}
	}

	protected virtual void OnActivated(EventArgs e)
	{
		Activated?.Invoke(this, e);
	}

	protected virtual void OnClosed(EventArgs e)
	{
		Closed?.Invoke(this, e);
	}

	protected virtual void OnClosing(CancelEventArgs e)
	{
		Closing?.Invoke(this, e);
	}

	protected virtual void OnDeactivate(EventArgs e)
	{
		Deactivate?.Invoke(this, e);
	}

	protected virtual void OnFormClosing(FormClosingEventArgs e)
	{
		using var activity = TelemetryService.ActivitySource?.StartActivity($"{GetType().Name}.{nameof(OnFormClosing)}");
		FormClosing?.Invoke(this, e);
	}

	protected virtual void OnFormClosed(FormClosedEventArgs e)
	{
		Application.OpenForms.Remove(this);
		FormClosed?.Invoke(this, e);
	}

	/// <summary>
	///  Raises the CreateControl event.
	/// </summary>
	protected override void OnCreateControl()
	{
		CalledCreateControl = true;
		base.OnCreateControl();

		if (CalledMakeVisible && !CalledOnLoad)
		{
			CalledOnLoad = true;
			OnLoad(EventArgs.Empty);
		}
	}

	/// <summary>
	///  The Load event is fired before the form becomes visible for the first time.
	/// </summary>
	protected virtual void OnLoad(EventArgs e)
	{
		Application.OpenForms.Add(this);

		Load?.Invoke(this, e);

		if (IsHandleCreated)
		{
			BeginInvoke(new MethodInvoker(CallShownEvent));
		}
	}

	void CallShownEvent()
	{
		OnShown(EventArgs.Empty);
	}

	protected virtual void OnShown(EventArgs e)
	{
		Shown?.Invoke(this, e);
	}

	protected virtual void OnMinimumSizeChanged(EventArgs e)
	{
		if (!resizedFromClient && IsHandleCreated)
		{
			SetFormSizeOnClient(Size.Empty);
		}

		MinimumSizeChanged?.Invoke(this, e);
	}

	protected virtual void OnMaximumSizeChanged(EventArgs e)
	{
		if (!resizedFromClient && IsHandleCreated)
		{
			SetFormSizeOnClient(Size.Empty);
		}

		MaximumSizeChanged?.Invoke(this, e);
	}

	bool RenderSizeGrip()
	{
		switch (FormBorderStyle)
		{
			case FormBorderStyle.Sizable:
			case FormBorderStyle.SizableToolWindow:
				switch (SizeGripStyle)
				{
					case SizeGripStyle.Show:
						return true;
					case SizeGripStyle.Auto:
						return Modal;
					case SizeGripStyle.Hide:
					default:
						return false;
				}
			case FormBorderStyle.None:
			case FormBorderStyle.FixedSingle:
			case FormBorderStyle.FixedDialog:
			case FormBorderStyle.Fixed3D:
			case FormBorderStyle.FixedToolWindow:
			default:
				return false;
		}
	}

	[JSInvokable]
	public async Task OnDropFilesAsync(BrowserFile[] files, int clientX, int clientY, string targetWinzorControlId)
	{
		if (files.Length == 0)
		{
			return;
		}

		var target = FindDescendantByWinzorControlId(targetWinzorControlId) ?? this;
		while (!target.AllowDrop && target.Parent != null)
		{
			target = target.Parent;
		}

		if (!target.AllowDrop)
		{
			return;
		}

		try
		{
			// Temporarily set at 100M. Because the eDocsMaximumFileSize's maximum value is 100M.
			await DoDropFilesAsync(target, files, clientX, clientY, 100 * 1024 * 1024);
		}
		catch (TimeoutException ex)
		{
			Application.ReportDeveloperException("Uploading dropped files to server time out.", ex);
		}
	}

	protected virtual async Task DoDropFilesAsync(Control target, BrowserFile[] files, int clientX, int clientY, long maximumFileSize)
	{
		var dataObject = new DataObject();
		var filePaths = await (CargoWiseClientServices?.FileService?.UploadFilesToServerAsync(files, maximumFileSize, CancellationToken.None) ?? Task.FromResult(Array.Empty<string>()));
		dataObject.SetData(DataFormats.FileDrop, filePaths);
		var args = new DragEventArgs(dataObject, 0, clientX, clientY, DragDropEffects.All, DragDropEffects.None);
		await InvokeWinzorDispatcherAsync(() => target.DoDragOverDrop(args));
	}

	public event EventHandler? Activated;

	public event CancelEventHandler? Closing;

	public event EventHandler? Closed;

	public event FormClosingEventHandler? FormClosing;

	public event FormClosedEventHandler? FormClosed;

	public event EventHandler? Deactivate;

	public event EventHandler? Load;

	public event EventHandler? Shown;

	public event EventHandler? ResizeBegin;

	public event EventHandler? ResizeEnd;

	public event EventHandler? MinimumSizeChanged;

	public event EventHandler? MaximumSizeChanged;

	public static Form? ActiveForm => activeForm;

	static Form? activeForm;

	internal static void SetActiveForm(Form? value, Form? current)
	{
		if (value is not null && current is not null && !current.IsDisposed)
		{
			activeForm = value;
			Keyboard.KeyDownStatus.Clear();
		}
		else if (value is null && activeForm == current)
		{
			activeForm = null;
			Keyboard.KeyDownStatus.Clear();
		}
	}

	async Task HandleAltKeyPressAsync()
	{
		if (!showMnemonicKeys)
		{
			await (altKeyEvent?.DisposeAsync() ?? ValueTask.CompletedTask);
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			showMnemonicKeys = true;
			NotifyRenderRequired();
		});
	}

	RegisteredClientEvent? altKeyEvent;
	protected internal bool showMnemonicKeys;

	/// <summary>
	///  Processes a command key. Overrides Control.processCmdKey() to provide
	///  additional handling of main menu command keys and Mdi accelerators.
	/// </summary>
	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (base.ProcessCmdKey(ref msg, keyData))
		{
			return true;
		}

		if (Menu != null && Menu.ProcessCmdKey(ref msg, keyData))
		{
			return true;
		}

		return false;
	}

	/// <summary>
	///  Processes a dialog key. Overrides Control.processDialogKey(). This
	///  method implements handling of the RETURN, and ESCAPE keys in dialogs.
	///  The method performs no processing on keys that include the ALT or
	///  CONTROL modifiers.
	/// </summary>
	protected override bool ProcessDialogKey(Keys keyData)
	{
		if ((keyData & (Keys.Alt | Keys.Control)) == Keys.None)
		{
			Keys keyCode = keyData & Keys.KeyCode;
			IButtonControl? button;

			switch (keyCode)
			{
				case Keys.Return:
					button = (IButtonControl?)Properties.GetObject(PropDefaultButton);
					if (button is not null)
					{
						//PerformClick now checks for validationcancelled...
						if (button is Control)
						{
							button.PerformClick();
						}

						return true;
					}

					break;
				case Keys.Escape:
					button = (IButtonControl?)Properties.GetObject(PropCancelButton);
					if (button is not null)
					{
						// In order to keep the behavior in sync with native
						// and MFC dialogs, we want to not give the cancel button
						// the focus on Escape. If we do, we end up with giving it
						// the focus when we reshow the dialog.
						//
						//if (button is Control) {
						//    ((Control)button).Focus();
						//}
						button.PerformClick();
						return true;
					}

					break;
			}
		}

		return base.ProcessDialogKey(keyData);
	}

	protected internal override bool ProcessMnemonic(char charCode)
	{
		if (base.ProcessMnemonic(charCode))
		{
			return true;
		}

		// In WinForms, the mnemonic command is dispatched directly to the Menu control using a WM_COMMAND message.
		// In Winzor, we need to forward it to the Menu control directly.
		if (ModifierKeys.HasFlag(Keys.Alt) && Menu != null && Menu.ProcessMnemonic(charCode))
		{
			return true;
		}

		return false;
	}

	/// <summary>
	///  Selects this form, and optionally selects the next/previous control.
	/// </summary>
	protected override void Select(bool directed, bool forward)
	{
		if (directed)
		{
			SelectNextControl(null, forward, true, true, false);
		}

		var form = ParentForm;
		if (form is not null)
		{
			form.ActiveControl = this;
		}
	}

	/// <summary>
	///  Sets the clientSize of the form. This will adjust the bounds of the form
	///  to make the clientSize the requested size.
	/// </summary>
	protected override void SetClientSizeCore(int x, int y)
	{
		bool hadHScroll = HScroll, hadVScroll = VScroll;
		base.SetClientSizeCore(x, y);

		if (IsHandleCreated)
		{
			// Adjust for the scrollbars, if they were introduced by
			// the call to base.SetClientSizeCore
			if (VScroll != hadVScroll)
			{
				if (VScroll)
				{
					x += SystemInformation.VerticalScrollBarWidth;
				}
			}

			if (HScroll != hadHScroll)
			{
				if (HScroll)
				{
					y += SystemInformation.HorizontalScrollBarHeight;
				}
			}

			if (x != ClientSize.Width || y != ClientSize.Height)
			{
				base.SetClientSizeCore(x, y);
			}
		}
	}

	[JSInvokable]
	public void OnUserActivity() => WinzorDispatcher?.OnUserActivity();

	[JSInvokable]
	public async Task DeactivateFromClientAsync()
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			WmKillFocus(true);
			OnDeactivate(EventArgs.Empty);
		});
	}

	[JSInvokable]
	public async Task ActivatedFromClientAsync()
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			OnActivated(EventArgs.Empty);
			SetActiveForm(this, this);
		});
	}

	internal async Task<IAsyncDisposable?> SetNotRespondingAsync()
	{
		if (NotRespondingOverlay != null)
		{
			return await NotRespondingOverlay.SetVisibleAsync().ConfigureAwait(false);
		}
		else
		{
			return null;
		}
	}

	NotRespondingOverlayComponent? NotRespondingOverlay { get; set; }

	public Control? LastFocusedControl
	{
		get => lastFocusedControl;
		internal set
		{
			lastFocusedControl?.WmKillFocus();
			lastFocusedControl = value;
		}
	}
	Control? lastFocusedControl;

	[DefaultValue(true)]
	public new bool TabStop
	{
		get => base.TabStop;
		set => base.TabStop = value;
	}

	/// <summary>
	/// Used for FormMoveByMouseDragExtensions in Winzor.
	/// WinForms does not have an equivalent value.
	/// </summary>
	public bool DisableBorderlessWindow
	{
		get => disableBorderlessWindow;
		set
		{
			if (UpdateProperty(ref disableBorderlessWindow, value))
			{
				UpdateFormStyles(() => new WindowStyleOptions { FormBorderStyle = FormBorderStyleForClientMessage() });
			}
		}
	}
	bool disableBorderlessWindow;

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		base.SetBoundsCore(x, y, width, height, specified);

		// Update RestoreBounds
		if ((specified & BoundsSpecified.X) != 0)
		{
			restoreBounds.X = x;
		}

		if ((specified & BoundsSpecified.Y) != 0)
		{
			restoreBounds.Y = y;
		}

		if ((specified & BoundsSpecified.Width) != 0 || restoreBounds.Width == -1)
		{
			restoreBounds.Width = width;
		}

		if ((specified & BoundsSpecified.Height) != 0 || restoreBounds.Height == -1)
		{
			restoreBounds.Height = height;
		}
	}

	protected override internal string ScrollStyleString
	{
		get
		{
			if (ClientSize.Width < MinimumSize.Width || ClientSize.Height < MinimumSize.Height)
			{
				return "overflow:auto;";
			}

			return base.ScrollStyleString;
		}
	}

	public static void SaveShortcutFile(string fileName, string content)
	{
		var form = WinzorDispatcher.Current.CurrentContext?.Form;
		form?.InvokeRenderDispatcher(async () =>
		{
			await (form?.Interop?.SaveShortcutFileAsync(fileName, content) ?? Task.CompletedTask);
		});
	}

	public ShowWindowOptions GenerateShowWindowOptions() => new ShowWindowOptions
	{
		Modal = ModalWindow,
		ParentWindowId = Owner?.WinzorWindowId,
		StartPosition = (CargoWise.Blazor.Client.Integration.Messaging.FormStartPosition)StartPosition,
	};

	public WindowStyleOptions GenerateWindowStyleOptions() => new WindowStyleOptions
	{
		PreferredSizeUnscaled = Size,
		PreferredMinimumSizeUnscaled = MinimumSize,
		PreferredMaximumSizeUnscaled = MaximumSize,
		MaximizeBox = MaximizeBox,
		MinimizeBox = MinimizeBox,
		ControlBox = ControlBox,
		FormBorderStyle = FormBorderStyleForClientMessage(),
		TopMost = TopMost,
		Enabled = WindowEnabled,
	};

	/// <summary>
	/// Enables or disables the window on the client machine.
	/// Differs from the Enabled property which impacts the form's controls.
	/// </summary>
	public bool WindowEnabled
	{
		get => windowEnabled;
		set
		{
			if (UpdateProperty(ref windowEnabled, value))
			{
				//When initializing the form, we should send the form style in the RequestShowWindowAsync call (to minimize redundant calls).
				if (!ClientWindowOpenSent)
				{
					return;
				}

				InvokeRenderDispatcher(async () => await (CargoWiseClientServices?.WindowService?.RequestUpdateWindowStyleAsync(new WindowStyleOptions { Enabled = windowEnabled }) ?? Task.CompletedTask));
			}
		}
	}
	bool windowEnabled = true;

	/// <summary>
	/// Border style sent to the client app, which may differ from the form's FormBorderStyle value.
	/// </summary>
	IntegrationFormBorderStyle FormBorderStyleForClientMessage()
	{
		var formBorderStyle = FormBorderStyle;

		if (AutoSizeMode == AutoSizeMode.GrowAndShrink)
		{
			if (formBorderStyle == FormBorderStyle.Sizable)
			{
				formBorderStyle = FormBorderStyle.FixedSingle;
			}
			else if (formBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				formBorderStyle = FormBorderStyle.FixedToolWindow;
			}
		}
		else if (DisableBorderlessWindow && formBorderStyle == FormBorderStyle.None)
		{
			formBorderStyle = FormBorderStyle.FixedSingle;
		}

		return (IntegrationFormBorderStyle)formBorderStyle;
	}

	/// <summary>
	/// Winzor-specific value.
	/// Form size/style change messages should not be sent to the client app if the initial message (used to open a window and define its initial size/style/behaviour) has not been sent.
	/// </summary>
	public bool ClientWindowOpenSent { get; set; }

	/// <summary>
	/// Winzor specific window id (HostForm id of Client App)
	/// </summary>
	public Guid? WinzorWindowId { get; private set; }
}
