using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using WinzorFramework.Telemetry;

namespace System.Windows.Forms;

public class ContainerControl : ScrollableControl, IContainerControl
{
	Control? activeControl;

	Control? focusedControl;

	Control? unvalidatedControl;

	AutoValidate autoValidate = AutoValidate.Inherit;

	BitVector32 state;

	static readonly int s_stateScalingNeededOnLayout = BitVector32.CreateMask();
	static readonly int s_stateValidating = BitVector32.CreateMask(s_stateScalingNeededOnLayout);

	public ContainerControl() : base()
	{
		SetExtendedState(ExtendedStates.UserPreferredSizeCache, true);
	}

	public Form? ParentForm
	{
		get
		{
			if (Parent is not null)
			{
				return Parent.FindForm();
			}

			if (this is Form)
			{
				return null;
			}

			return FindForm();
		}
	}

	public Control? ActiveControl
	{
		get => activeControl;
		set => SetActiveControl(value);
	}

	public AutoScaleMode AutoScaleMode { get; set; }

	protected SizeF AutoScaleFactor { get; } = new SizeF(1f, 1f);

	public SizeF AutoScaleDimensions { get; set; }

	public virtual AutoValidate AutoValidate
	{
		get
		{
			if (autoValidate != AutoValidate.Inherit)
			{
				return autoValidate;
			}

			return GetAutoValidateForControl(this);
		}
		set
		{
			if (value < AutoValidate.Inherit || value > AutoValidate.EnableAllowFocusChange)
			{
				throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(AutoValidate));
			}

			if (autoValidate == value)
			{
				return;
			}

			autoValidate = value;
			OnAutoValidateChanged(EventArgs.Empty);
		}
	}

	public event EventHandler? AutoValidateChanged;

	protected virtual void OnAutoValidateChanged(EventArgs e) => AutoValidateChanged?.Invoke(this, e);

	protected virtual bool ProcessTabKey(bool forward) => false;

	protected override bool ProcessDialogKey(Keys keyData)
	{
		LastKeyData = keyData;
		if ((keyData & (Keys.Alt | Keys.Control)) == Keys.None)
		{
			var keyCode = keyData & Keys.KeyCode;
			switch (keyCode)
			{
				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
				case Keys.Down:
					if (ProcessArrowKey(keyCode == Keys.Right || keyCode == Keys.Down))
					{
						return true;
					}
					break;
			}
		}

		return base.ProcessDialogKey(keyData);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (base.ProcessCmdKey(ref msg, keyData))
		{
			return true;
		}
		if (Parent == null)
		{
			// unfortunately, we have to stick this here for the case where we're hosted without
			// a form in the chain.  This would be something like a context menu strip with shortcuts
			// hosted within Office, VS or IE.
			//
			// this is an optimized search O(number of ToolStrips in thread)
			// that happens only if the key routing makes it to the top.
			return ToolStripManager.ProcessCmdKey(ref msg, keyData);
		}
		return false;
	}

	bool ProcessArrowKey(bool forward)
	{
		Control control = this;
		if (activeControl != null)
		{
			control = activeControl.Parent!;
		}

		return control.SelectNextControl(activeControl, forward, tabStopOnly: false, nested: false, wrap: true);
	}

	/// <summary>
	///  Processes a dialog character. Overrides Control.processDialogChar(). This method calls
	///  the ProcessMnemonic() method to check if the character is a mnemonic for one of the
	///  controls on the form. If processMnemonic() does not consume the character, then
	///  base.ProcessDialogChar() is called.
	/// </summary>
	protected override bool ProcessDialogChar(char charCode)
	{
		// If we're the top-level form or control, we need to do the mnemonic handling
		if (GetContainerControl() is ContainerControl && charCode != ' ' && ProcessMnemonic(charCode))
		{
			return true;
		}

		return base.ProcessDialogChar(charCode);
	}

	protected internal override bool ProcessMnemonic(char charCode)
	{
		if (!CanProcessMnemonic())
		{
			return false;
		}

		if (Controls.Count == 0)
		{
			return false;
		}

		// Start with the active control.
		var start = ActiveControl;

		// Set the processing mnemonic flag so child controls don't check for it when checking if they
		// can process the mnemonic.
		processingMnemonic = true;

		var processed = false;

		try
		{
			// Safety flag to avoid infinite loop when testing controls in a container.
			var wrapped = false;

			var ctl = start;

			do
			{
				// Loop through the controls starting at the control next to the current Active control in the Tab order
				// till we find someone willing to process this mnemonic.
				// We don't start the search on the Active control to allow controls in the same container with the same
				// mnemonic (bad UI design but supported) to be processed sequentially
				ctl = GetNextControl(ctl, true);

				if (ctl is not null)
				{
					// Processing the mnemonic can change the value of CanProcessMnemonic.
					if (ctl.ProcessMnemonic(charCode))
					{
						processed = true;
						break;
					}
				}
				else
				{
					if (wrapped)
					{
						// This avoids infinite loops
						break;
					}

					wrapped = true;
				}
			}
			while (ctl != start);
		}
		finally
		{
			processingMnemonic = false;
		}

		return processed;
	}

	/// <summary>
	///  Specifies whether this control can process the mnemonic or not.
	/// </summary>
	internal override bool CanProcessMnemonic() => processingMnemonic || base.CanProcessMnemonic();

	bool processingMnemonic;

	public override BindingContext? BindingContext
	{
		get
		{
			var bm = base.BindingContext;
			if (bm == null)
			{
				bm = new BindingContext();
				BindingContext = bm;
			}

			return bm;
		}
		set => base.BindingContext = value;
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();
		OnBindingContextChanged(EventArgs.Empty);
	}

	protected void OnFrameWindowActivate()
	{
		if (ActiveControl == null)
		{
			SelectNextControl(null, true, true, true, false);
		}

		InnerMostActiveContainerControl.FocusActiveControlInternal();
	}

	protected ContainerControl InnerMostActiveContainerControl
	{
		get
		{
			ContainerControl result = this;
			while (result.ActiveControl is ContainerControl control)
			{
				result = control;
			}

			return result;
		}
	}

	ContainerControl InnerMostFocusedContainerControl
	{
		get
		{
			ContainerControl result = this;
			while (result.focusedControl is ContainerControl control)
			{
				result = control;
			}

			return result;
		}
	}

	/// <summary>
	///  Activates the specified control.
	/// </summary>
	bool IContainerControl.ActivateControl(Control control)
	{
		return ActivateControl(control, originator: true);
	}

	internal bool ActivateControl(Control control)
	{
		return ActivateControl(control, originator: true);
	}

	internal bool ActivateControl(Control? control, bool originator)
	{
		bool ret = true;
		bool updateContainerActiveControl = false;
		ContainerControl? cc = null;
		Control? parent = Parent;

		if (parent is not null)
		{
			cc = (parent.GetContainerControl()) as ContainerControl;
			if (cc is not null)
			{
				updateContainerActiveControl = (cc.ActiveControl != this);
			}
		}

		if (control != activeControl || updateContainerActiveControl)
		{
			if (updateContainerActiveControl)
			{
				if (cc is not null && !cc.ActivateControl(this, false))
				{
					return false;
				}
			}

			ret = AssignActiveControl((control == this) ? null : control);
		}

		if (originator)
		{
			ScrollActiveControlIntoView();
		}

		return ret;
	}

	/// <summary>
	///  Cleans up form state after a control has been removed.
	/// </summary>
	internal virtual void AfterControlRemoved(Control control, Control oldParent)
	{
		ContainerControl? cc;
		Debug.Assert(control is not null);
		if (control == activeControl || control.Contains(activeControl))
		{
			bool selected = SelectNextControl(control, true, true, true, true);
			if (selected && activeControl != control)
			{
				FocusActiveControlInternal();
			}
			else
			{
				SetActiveControl(null);
			}
		}
		else if (activeControl is null && Parent is not null)
		{
			// The last control of an active container was removed. Focus needs to be given to the next
			// control in the Form.
			cc = Parent.GetContainerControl() as ContainerControl;
			if (cc is not null && cc.ActiveControl == this)
			{
				Form? f = FindForm();
				if (f is not null)
				{
					f.SelectNextControl(this, true, true, true, true);
				}
			}
		}

		// Two controls in UserControls that don't take focus via UI can have bad behavior if ...
		// When a control is removed from a container, not only do we need to clear the unvalidatedControl of that
		// container potentially, but the unvalidatedControl of all its container parents, up the chain, needs to
		// now point to the old parent of the disappearing control.
		cc = this;
		while (cc is not null)
		{
			Control? parent = cc.Parent;
			if (parent is null)
			{
				break;
			}
			else
			{
				cc = parent.GetContainerControl() as ContainerControl;
			}

			if (cc is not null &&
				cc.unvalidatedControl is not null &&
				(cc.unvalidatedControl == control || control.Contains(cc.unvalidatedControl)))
			{
				cc.unvalidatedControl = oldParent;
			}
		}

		if (control == unvalidatedControl || control.Contains(unvalidatedControl))
		{
			unvalidatedControl = null;
		}
	}

	bool AssignActiveControl(Control? value)
	{
		if (activeControl != value)
		{
			try
			{
				if (value is not null)
				{
					value.BecomingActiveControl = true;
				}

				activeControl = value;
				UpdateFocusedControl();
			}
			finally
			{
				if (value is not null)
				{
					value.BecomingActiveControl = false;
				}
			}

			if (activeControl == value)
			{
				var form = FindForm();
				if (form is not null)
				{
					form.UpdateDefaultButton();
				}
			}
		}
		else
		{
			focusedControl = activeControl;
		}

		return activeControl == value;
	}

	/// <summary>
	///  Updates the focusedControl variable by walking towards the activeControl variable, firing
	///  enter and leave events and validation as necessary.
	/// </summary>
	internal void UpdateFocusedControl()
	{
		// Capture the current focusedControl as the unvalidatedControl if we don't have one/are not validating.
		EnsureUnvalidatedControl(focusedControl);
		Control? pathControl = focusedControl;
		var oldFocusedControl = focusedControl;

		while (activeControl != pathControl)
		{
			var oldPathControl = pathControl;

			if (pathControl is null || pathControl.IsDescendant(activeControl))
			{
				// Heading down. Find next control on path.
				Control? nextControlDown = activeControl;
				while (true)
				{
					Control? parent = nextControlDown!.Parent;
					if (parent == this || parent == pathControl)
					{
						break;
					}

					nextControlDown = nextControlDown.Parent;
				}

				Control? priorFocusedControl = focusedControl = pathControl;
				EnterValidation(nextControlDown);
				// If validation changed position, then jump back to the loop.
				if (focusedControl != priorFocusedControl)
				{
					pathControl = focusedControl;
					continue;
				}

				pathControl = nextControlDown;
				try
				{
					pathControl.NotifyEnter();
				}
				catch (Exception e)
				{
					Application.OnThreadException(e);
				}
			}
			else
			{
				// Heading up.
				ContainerControl innerMostFCC = InnerMostFocusedContainerControl;
				Control? stopControl = null;

				if (innerMostFCC.focusedControl is not null)
				{
					pathControl = innerMostFCC.focusedControl;
					stopControl = innerMostFCC;

					if (innerMostFCC != this)
					{
						innerMostFCC.focusedControl = null;
						innerMostFCC.activeControl = null;
					}
				}
				else
				{
					pathControl = innerMostFCC;
					// innerMostFCC.ParentInternal can be null when the ActiveControl is deleted.
					if (innerMostFCC.Parent is not null)
					{
						ContainerControl? cc = (innerMostFCC.Parent.GetContainerControl()) as ContainerControl;
						stopControl = cc;
						if (cc is not null && cc != this)
						{
							cc.focusedControl = null;
							cc.activeControl = null;
						}
					}
				}

				do
				{
					Control leaveControl = pathControl;

					if (pathControl is not null)
					{
						pathControl = pathControl.Parent;
					}

					if (pathControl == this)
					{
						pathControl = null;
					}

					if (leaveControl is not null)
					{
						try
						{
							leaveControl.NotifyLeave();
						}
						catch (Exception e)
						{
							Application.OnThreadException(e);
						}
					}
				}
				while (pathControl is not null &&
					   pathControl != stopControl &&
					   !pathControl.IsDescendant(activeControl));
			}

			if (oldFocusedControl != focusedControl && oldPathControl == pathControl)
			{
				break;
			}
		}

		if (activeControl is null)
		{
			focusedControl?.WmKillFocus();
		}
		focusedControl = activeControl;
		if (activeControl is not null)
		{
			EnterValidation(activeControl);
		}
	}

	public virtual bool ValidateChildren() => false;

	public bool Validate() => Validate(checkAutoValidate: false);

	public bool Validate(bool checkAutoValidate) => ValidateInternal(checkAutoValidate, out _);

	internal bool ValidateInternal(bool checkAutoValidate, out bool validatedControlAllowsFocusChange)
	{
		validatedControlAllowsFocusChange = false;

		if (AutoValidate == AutoValidate.EnablePreventFocusChange ||
			(activeControl is not null && activeControl.CausesValidation))
		{
			if (unvalidatedControl is null)
			{
				if (focusedControl is ContainerControl control && focusedControl.CausesValidation)
				{
					var c = control;
					if (!c.ValidateInternal(checkAutoValidate, out validatedControlAllowsFocusChange))
					{
						return false;
					}
				}
				else
				{
					unvalidatedControl = focusedControl;
				}
			}

			// Should we force focus to stay on same control if there is a validation error?
			bool preventFocusChangeOnError = true;

			Control? controlToValidate = unvalidatedControl ?? focusedControl;

			if (controlToValidate is not null)
			{
				// Get the effective AutoValidate mode for unvalidated control (based on its container control)
				AutoValidate autoValidateMode = GetAutoValidateForControl(controlToValidate);

				// Auto-validate has been turned off in container of unvalidated control - stop now
				if (checkAutoValidate && autoValidateMode == AutoValidate.Disable)
				{
					return true;
				}

				preventFocusChangeOnError = (autoValidateMode == AutoValidate.EnablePreventFocusChange);
				validatedControlAllowsFocusChange = (autoValidateMode == AutoValidate.EnableAllowFocusChange);
			}

			return ValidateThroughAncestor(null, preventFocusChangeOnError);
		}

		return true;
	}

	protected override void Select(bool directed, bool forward)
	{
		bool correctParentActiveControl = true;
		if (Parent is not null)
		{
			IContainerControl? c = Parent.GetContainerControl();
			if (c is not null)
			{
				c.ActiveControl = this;
				correctParentActiveControl = (c.ActiveControl == this);
			}
		}

		if (directed && correctParentActiveControl)
		{
			SelectNextControl(null, forward, tabStopOnly: true, nested: true, wrap: false);
		}
	}

	internal void SetActiveControl(Control? value)
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(SetActiveControl)}");
		if (activeControl == value && (value is null || value.Focused))
		{
			return;
		}

		if (value is not null && !Contains(value))
		{
			throw new ArgumentException(SR.CannotActivateControl, nameof(value));
		}

		var result = false;
		var containerControl = this;
		var previousInnerMostActiveControl = InnerMostActiveContainerControl?.ActiveControl;

		if (value is null)
		{
			var previousActive = previousInnerMostActiveControl;
			while (previousActive != null)
			{
				if (previousActive.StopLeaveOnNoActiveControl)
				{
					value = previousActive;
					break;
				}
				previousActive = previousActive.Parent;
			}
		}

		if (value is not null)
		{
			containerControl = value.Parent?.GetContainerControl() as ContainerControl;
		}

		if (containerControl is not null)
		{
			result = containerControl.ActivateControl(value, false);
		}
		else
		{
			AssignActiveControl(value);
		}

		if (containerControl is not null && result)
		{
			var ancestor = this;
			while (ancestor.Parent?.GetContainerControl() is ContainerControl parentContainer)
			{
				ancestor = parentContainer;
			}

			if (ancestor.ContainsFocus
				&& (value is null || value is not UserControl userControl || !userControl.HasFocusableChild()))
			{
				containerControl.FocusActiveControlInternal();
			}
		}

		if (Parent is null && previousInnerMostActiveControl is not null && InnerMostActiveContainerControl?.ActiveControl is null)
		{
			var validatingControl = previousInnerMostActiveControl;
			do
			{
				validatingControl.PerformControlValidation(false);
				validatingControl = validatingControl.Parent;
			}
			while (validatingControl != null && validatingControl != this);
		}
	}

	void EnsureUnvalidatedControl(Control? candidate)
	{
		// Don't change the unvalidated control while in the middle of validation (re-entrancy)
		if (state[s_stateValidating])
		{
			return;
		}

		// Don't change the existing unvalidated control
		if (unvalidatedControl is not null)
		{
			return;
		}

		// No new choice of unvalidated control was specified - leave unvalidated control blank
		if (candidate is null)
		{
			return;
		}

		// Specified control has auto-validation disabled - leave unvalidated control blank
		if (!candidate.ShouldAutoValidate)
		{
			return;
		}

		// Go ahead and make specified control the current unvalidated control for this container
		unvalidatedControl = candidate;

		// In the case of nested container controls, try to pick the deepest possible unvalidated
		// control. For a container with no unvalidated control, use the active control instead.
		// Stop as soon as we encounter any control that has auto-validation turned off.
		while (unvalidatedControl is ContainerControl container)
		{
			if (container.unvalidatedControl is not null && container.unvalidatedControl.ShouldAutoValidate)
			{
				unvalidatedControl = container.unvalidatedControl;
			}
			else if (container.activeControl is not null && container.activeControl.ShouldAutoValidate)
			{
				unvalidatedControl = container.activeControl;
			}
			else
			{
				break;
			}
		}
	}

	/// <summary>
	///  Validates the last unvalidated control and its ancestors (up through the ancestor in common
	///  with enterControl) if enterControl causes validation.
	/// </summary>
	void EnterValidation(Control enterControl)
	{
		// No unvalidated control to validate - stop now
		if (unvalidatedControl is null)
		{
			return;
		}

		// Entered control does not trigger validation - stop now
		if (!enterControl.CausesValidation)
		{
			return;
		}

		// Get the effective AutoValidate mode for this control (based on its container control)
		AutoValidate autoValidateMode = Control.GetAutoValidateForControl(unvalidatedControl);

		// Auto-validate has been turned off in container of unvalidated control - stop now
		if (autoValidateMode == AutoValidate.Disable)
		{
			return;
		}

		// Find common ancestor of entered control and unvalidated control
		Control? commonAncestor = enterControl;
		while (commonAncestor is not null && !commonAncestor.IsDescendant(unvalidatedControl))
		{
			commonAncestor = commonAncestor.Parent;
		}

		// Should we force focus to stay on same control if there is a validation error?
		bool preventFocusChangeOnError = (autoValidateMode == AutoValidate.EnablePreventFocusChange);

		// Validate control and its ancestors, up to (but not including) the common ancestor
		ValidateThroughAncestor(commonAncestor, preventFocusChangeOnError);
	}

	bool ValidateThroughAncestor(Control? ancestorControl, bool preventFocusChangeOnError)
	{
		if (ancestorControl is null)
		{
			ancestorControl = this;
		}

		if (state[s_stateValidating])
		{
			return false;
		}

		if (unvalidatedControl is null)
		{
			unvalidatedControl = focusedControl;
		}

		// Return true for a Container Control with no controls to validate.
		if (unvalidatedControl is null)
		{
			return true;
		}

		if (!ancestorControl.IsDescendant(unvalidatedControl))
		{
			return false;
		}

		state[s_stateValidating] = true;
		bool cancel = false;

		Control? currentActiveControl = activeControl;
		Control? currentValidatingControl = unvalidatedControl;
		if (currentActiveControl is not null)
		{
			currentActiveControl.ValidationCancelled = false;
			if (currentActiveControl is ContainerControl currentActiveContainerControl)
			{
				currentActiveContainerControl.ResetValidationFlag();
			}
		}

		try
		{
			while (currentValidatingControl is not null && currentValidatingControl != ancestorControl)
			{
				try
				{
					cancel = currentValidatingControl.PerformControlValidation(false);
				}
				catch
				{
					cancel = true;
					throw;
				}

				if (cancel)
				{
					break;
				}

				currentValidatingControl = currentValidatingControl.Parent;
			}

			if (cancel && preventFocusChangeOnError)
			{
				if (unvalidatedControl is null && currentValidatingControl is not null &&
					ancestorControl.IsDescendant(currentValidatingControl))
				{
					unvalidatedControl = currentValidatingControl;
				}

				// This bit 'marks' the control that was going to get the focus, so that it will ignore any pending
				// mouse or key events. Otherwise it would still perform its default 'click' action or whatever.
				if (currentActiveControl == activeControl)
				{
					if (currentActiveControl is not null)
					{
						CancelEventArgs ev = new CancelEventArgs
						{
							Cancel = true
						};
						currentActiveControl.NotifyValidationResult(currentValidatingControl, ev);
						if (currentActiveControl is ContainerControl currentActiveContainerControl)
						{
							if (currentActiveContainerControl.focusedControl is not null)
							{
								currentActiveContainerControl.focusedControl.ValidationCancelled = true;
							}

							currentActiveContainerControl.ResetActiveAndFocusedControlsRecursive();
						}
					}
				}

				// This bit forces the focus to move back to the invalid control
				SetActiveControl(unvalidatedControl);
			}
		}
		finally
		{
			unvalidatedControl = null;
			state[s_stateValidating] = false;
		}

		return !cancel;
	}

	void ResetValidationFlag()
	{
		// Performance: This is more efficient than using Foreach. Foreach forces the creation of
		// an array subset enum each time we enumerate
		ControlCollection children = Controls;
		int count = children.Count;
		for (int i = 0; i < count; i++)
		{
			children[i].ValidationCancelled = false;
		}
	}

	internal void ResetActiveAndFocusedControlsRecursive()
	{
		if (activeControl is ContainerControl activeContainerControl)
		{
			activeContainerControl.ResetActiveAndFocusedControlsRecursive();
		}

		activeControl = null;
		focusedControl = null;
	}

	protected override void WmSetFocus()
	{
		if (ActiveControl is not null)
		{
			// Do not raise GotFocus event since the focus is given to the visible ActiveControl
			if (!ActiveControl.Visible)
			{
				InvokeGotFocus(this, EventArgs.Empty);
			}

			FocusActiveControlInternal();
		}
		else
		{
			if (Parent is not null)
			{
				IContainerControl? c = Parent.GetContainerControl();
				if (c is not null)
				{
					bool succeeded;

					if (c is ContainerControl knowncontainer)
					{
						succeeded = knowncontainer.ActivateControl(this);
					}
					else
					{
						succeeded = c.ActivateControl(this);
					}

					if (!succeeded)
					{
						return;
					}
				}
			}

			base.WmSetFocus();
		}
	}

	/// <summary>
	///  Assigns focus to the activeControl. If there is no activeControl then focus is given to
	///  the form. package scope for Form
	/// </summary>
	internal void FocusActiveControlInternal()
	{
		if (activeControl is not null && activeControl.Visible)
		{
			// Avoid focus loops, especially with ComboBoxes.
			if (!activeControl.Focused)
			{
				activeControl.SetFocus();
			}
		}
		else
		{
			// Determine and focus closest visible parent
			var cc = this;
			while (cc != null && !cc.Visible)
			{
				Control? parent = cc.Parent;
				if (parent is not null)
				{
					cc = parent.GetContainerControl() as ContainerControl;
				}
				else
				{
					break;
				}
			}

			if (cc is not null && cc.Visible)
			{
				cc.SetFocus();
			}
		}
	}

	void ScrollActiveControlIntoView()
	{
	}

	bool HasFocusableChild()
	{
		Control? ctl = null;
		do
		{
			ctl = GetNextControl(ctl, true);
			if (ctl is not null && ctl.CanSelect && ctl.TabStop)
			{
				break;
			}
		}
		while (ctl is not null);

		return ctl is not null;
	}

	protected virtual void UpdateDefaultButton()
	{
	}

	/// <summary>
	///  Disposes of the resources (other than memory) used by the <see cref="ContainerControl"/>.
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			activeControl = null;
		}

		base.Dispose(disposing);

		focusedControl = null;
		unvalidatedControl = null;
	}

	internal override Size GetPreferredSizeCore(Size proposedSize)
	{
		var size = SizeFromClientSize(Size.Empty) + Padding.Size;
		return LayoutEngine.GetPreferredSize(this, proposedSize - size) + size;
	}
}
