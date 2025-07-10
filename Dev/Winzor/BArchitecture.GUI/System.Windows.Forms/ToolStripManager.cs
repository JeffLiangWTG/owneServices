// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Win32;

namespace System.Windows.Forms;

public static partial class ToolStripManager
{
	// WARNING: ThreadStatic initialization happens only on the first thread at class CTOR time.
	// use InitializeThread mechanism to initialize ThreadStatic members
	[ThreadStatic]
	static WeakRefCollection t_toolStripWeakArrayList;

	[ThreadStatic]
	static WeakRefCollection t_toolStripPanelWeakArrayList;

	[ThreadStatic]
	static bool t_initialized;

	static Font s_defaultFont;

	// WARNING: When subscribing to static event handlers - make sure you unhook from them
	// otherwise you can leak USER objects on process shutdown.
	// Consider: use WeakRefCollection
	[ThreadStatic]
	static Delegate[] t_staticEventHandlers;
	const int StaticEventDefaultRendererChanged = 0;
	const int StaticEventCount = 1;

	static readonly object s_internalSyncObject = new object();

	static void InitializeThread()
	{
		if (!t_initialized)
		{
			t_initialized = true;
			t_currentRendererType = s_professionalRendererType;
		}
	}

	static ToolStripManager()
	{
		SystemEvents.UserPreferenceChanging += new UserPreferenceChangingEventHandler(OnUserPreferenceChanging);
	}

	internal static Font DefaultFont
	{
		get
		{
			// Threadsafe local reference
			Font retFont = s_defaultFont;

			if (retFont is null)
			{
				lock (s_internalSyncObject)
				{
					// Double check the defaultFont after the lock.
					retFont = s_defaultFont;

					if (retFont is null)
					{
						// Default to menu font or to control font if menu font unavailable
						var sysFont = SystemFonts.MenuFont ?? Control.DefaultFont;

						if (sysFont is not null)
						{
							// Ensure font is in pixels so it displays properly in the property grid at design time.
							if (sysFont.Unit != GraphicsUnit.Point)
							{
								s_defaultFont = ControlPaint.FontInPoints(sysFont);
								retFont = s_defaultFont;
								sysFont.Dispose();
							}
							else
							{
								s_defaultFont = sysFont;
								retFont = s_defaultFont;
							}
						}

						return retFont;
					}
				}
			}

			return retFont;
		}
	}

	internal static WeakRefCollection ToolStrips
		=> t_toolStripWeakArrayList ??= new WeakRefCollection();

	/// <summary>Static events only!!!</summary>
	static void AddEventHandler(int key, Delegate value)
	{
		lock (s_internalSyncObject)
		{
			t_staticEventHandlers ??= new Delegate[StaticEventCount];
			t_staticEventHandlers[key] = Delegate.Combine(t_staticEventHandlers[key], value);
		}
	}

	/// <summary>
	///  Find a toolstrip in the weak ref ArrayList, return null if nothing was found
	/// </summary>
	public static ToolStrip FindToolStrip(string toolStripName)
	{
		ToolStrip result = null;
		for (int i = 0; i < ToolStrips.Count; i++)
		{
			if (ToolStrips[i] is not null && string.Equals(((ToolStrip)ToolStrips[i]).Name, toolStripName, StringComparison.Ordinal))
			{
				result = (ToolStrip)ToolStrips[i];
				break;
			}
		}

		return result;
	}

	/// <summary>
	///  Find a toolstrip in the weak ref ArrayList, return null if nothing was found
	/// </summary>
	internal static ToolStrip FindToolStrip(Form owningForm, string toolStripName)
	{
		ToolStrip result = null;
		for (int i = 0; i < ToolStrips.Count; i++)
		{
			if (ToolStrips[i] is not null && string.Equals(((ToolStrip)ToolStrips[i]).Name, toolStripName, StringComparison.Ordinal))
			{
				result = (ToolStrip)ToolStrips[i];
				if (result.FindForm() == owningForm)
				{
					break;
				}
			}
		}

		return result;
	}

	static Delegate GetEventHandler(int key)
	{
		lock (s_internalSyncObject)
		{
			if (t_staticEventHandlers is null)
			{
				return null;
			}

			return t_staticEventHandlers[key];
		}
	}

	internal static bool IsThreadUsingToolStrips()
		=> t_toolStripWeakArrayList is not null && t_toolStripWeakArrayList.Count > 0;

	static void OnUserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
	{
		// Using changing here so that the cache will be cleared by the time the ToolStrip
		// hooks onto the changed event.

		// SPI_SETNONCLIENTMETRICS is put up in WM_SETTINGCHANGE if the Menu font changes.
		// this corresponds to UserPreferenceCategory.Window.
		if (e.Category != UserPreferenceCategory.Window)
		{
			return;
		}

		lock (s_internalSyncObject)
		{
			s_defaultFont = null;
		}
	}

	internal static void NotifyMenuModeChange(bool invalidateText, bool activationChange)
	{
		bool toolStripPruneNeeded = false;

		// If we've toggled the ShowUnderlines value, we'll need to invalidate
		for (int i = 0; i < ToolStrips.Count; i++)
		{
			if (!(ToolStrips[i] is ToolStrip toolStrip))
			{
				toolStripPruneNeeded = true;
				continue;
			}

			if (invalidateText)
			{
				toolStrip.InvalidateTextItems();
			}

			if (activationChange)
			{
				toolStrip.KeyboardActive = false;
			}
		}

		if (toolStripPruneNeeded)
		{
			PruneToolStripList();
		}
	}

	internal static void CloseActiveDropDown()
	{
		for (var i = 0; i < ToolStrips.Count; i++)
		{
			if (ToolStrips[i] is ToolStripDropDown toolStripDropDown && toolStripDropDown.Visible)
			{
				toolStripDropDown.Close();
			}
		}
	}

	/// <summary>
	///  Removes dead entries from the toolstrip weak reference collection.
	/// </summary>
	internal static void PruneToolStripList()
	{
		if (t_toolStripWeakArrayList is null || t_toolStripWeakArrayList.Count == 0)
		{
			return;
		}

		for (int i = t_toolStripWeakArrayList.Count - 1; i >= 0; i--)
		{
			if (t_toolStripWeakArrayList[i] is null)
			{
				t_toolStripWeakArrayList.RemoveAt(i);
			}
		}
	}

	static void RemoveEventHandler(int key, Delegate value)
	{
		lock (s_internalSyncObject)
		{
			if (t_staticEventHandlers is not null)
			{
				t_staticEventHandlers[key] = Delegate.Remove(t_staticEventHandlers[key], value);
			}
		}
	}

	/// <remarks>
	///  This is thread static because we want separate instances for each thread.
	///  We don't want to guarantee thread safety and don't want to have to take
	///  locks in painting code.
	/// </remarks>
	[ThreadStatic]
	static ToolStripRenderer t_defaultRenderer;

	// types cached for perf.
	internal static Type s_systemRendererType = typeof(ToolStripSystemRenderer);
	internal static Type s_professionalRendererType = typeof(ToolStripProfessionalRenderer);
	static bool s_visualStylesEnabledIfPossible = true;

	[ThreadStatic]
	static Type t_currentRendererType;

	static Type CurrentRendererType
	{
		get
		{
			InitializeThread();
			return t_currentRendererType;
		}
		set => t_currentRendererType = value;
	}

	static Type s_defaultRendererType => s_professionalRendererType;

	/// <summary>
	///  The default renderer for the thread. When ToolStrip.RenderMode is set
	///  to manager - this is the property used.
	/// </summary>
	public static ToolStripRenderer Renderer
	{
		get
		{
			if (t_defaultRenderer is null)
			{
				t_defaultRenderer = CreateRenderer(RenderMode);
			}

			return t_defaultRenderer;
		}
		set
		{
			if (t_defaultRenderer != value)
			{
				CurrentRendererType = (value is null) ? s_defaultRendererType : value.GetType();
				t_defaultRenderer = value;

				((EventHandler)GetEventHandler(StaticEventDefaultRendererChanged))?.Invoke(null, EventArgs.Empty);
			}
		}
	}

	/// <summary>
	///  Occurs when toolstripmanager.Renderer property has changed
	///  Warning: When subscribing to static event handlers - make sure you unhook from them
	///  otherwise you can leak user objects on process shutdown.
	/// </summary>
	public static event EventHandler RendererChanged
	{
		add => AddEventHandler(StaticEventDefaultRendererChanged, value);
		remove => RemoveEventHandler(StaticEventDefaultRendererChanged, value);
	}

	/// <summary>
	///  Returns the default toolstrip RenderMode for the thread
	/// </summary>
	public static ToolStripManagerRenderMode RenderMode
	{
		get
		{
			Type currentType = CurrentRendererType;

			if (t_defaultRenderer is not null && !t_defaultRenderer.IsAutoGenerated)
			{
				return ToolStripManagerRenderMode.Custom;
			}

			// check the type of the currently set renderer.
			// types are cached as this may be called frequently.
			if (currentType == s_professionalRendererType)
			{
				return ToolStripManagerRenderMode.Professional;
			}

			if (currentType == s_systemRendererType)
			{
				return ToolStripManagerRenderMode.System;
			}

			return ToolStripManagerRenderMode.Custom;
		}
		set
		{
			switch (value)
			{
				case ToolStripManagerRenderMode.System:
				case ToolStripManagerRenderMode.Professional:
					Renderer = CreateRenderer(value);
					break;
				case ToolStripManagerRenderMode.Custom:
					throw new NotSupportedException();
			}
		}
	}

	/// <summary>
	///  An additional layering of control. This lets you pick whether your toolbars
	///  should use visual style information (theming) to render itself.
	///  potentially you could want a themed app but an unthemed toolstrip.
	/// </summary>
	public static bool VisualStylesEnabled
	{
		get => s_visualStylesEnabledIfPossible && Application.RenderWithVisualStyles;
		set
		{
			bool oldVis = VisualStylesEnabled;
			s_visualStylesEnabledIfPossible = value;

			if (oldVis != VisualStylesEnabled)
			{
				((EventHandler)GetEventHandler(StaticEventDefaultRendererChanged))?.Invoke(null, EventArgs.Empty);
			}
		}
	}

	internal static ToolStripRenderer CreateRenderer(ToolStripManagerRenderMode renderMode)
	{
		switch (renderMode)
		{
			case ToolStripManagerRenderMode.System:
				return new ToolStripSystemRenderer(isDefault: true);
			case ToolStripManagerRenderMode.Professional:
				return new ToolStripProfessionalRenderer(isDefault: true);
			case ToolStripManagerRenderMode.Custom:
			default:
				return new ToolStripSystemRenderer(isDefault: true);
		}
	}

	internal static ToolStripRenderer CreateRenderer(ToolStripRenderMode renderMode)
	{
		switch (renderMode)
		{
			case ToolStripRenderMode.System:
				return new ToolStripSystemRenderer(isDefault: true);
			case ToolStripRenderMode.Professional:
				return new ToolStripProfessionalRenderer(isDefault: true);
			case ToolStripRenderMode.Custom:
			default:
				return new ToolStripSystemRenderer(isDefault: true);
		}
	}

	internal static WeakRefCollection ToolStripPanels
		=> t_toolStripPanelWeakArrayList ??= new WeakRefCollection();

	internal static bool ProcessCmdKey(ref Message m, Keys keyData)
	{
		if (IsValidShortcut(keyData))
		{
			// if we're at the toplevel, check the toolstrips for matching shortcuts.
			// Win32 menus are handled in Form.ProcessCmdKey, but we cant guarantee that 
			// toolstrips will be hosted in a form.  ToolStrips have a hash of shortcuts
			// per container, so this should hopefully be a quick search.
			return ToolStripManager.ProcessShortcut(ref m, keyData);
		}
		return false;
	}

	internal static bool ProcessShortcut(ref Message m, Keys shortcut)
	{
		if (!IsThreadUsingToolStrips())
		{
			return false;
		}

		if (m.WParam != IntPtr.Zero && IsValidShortcut(shortcut))
		{
			var retVal = false;
			var needsPrune = false;

			for (var i = 0; i < ToolStrips.Count; i++)
			{
				var toolStrip = ToolStrips[i] as ToolStrip;
				if (toolStrip == null)
				{
					// consider prune tree...
					needsPrune = true;
					continue;
				}
				else if (FindToolStripMenuItem(toolStrip, shortcut, out var item))
				{
					// make sure that were processing shortcuts for the correct window.
					var rootWindowsMatch = false;
					var topMostToolStrip = toolStrip.GetToplevelOwnerToolStrip();
					if (topMostToolStrip != null && Form.ActiveForm != null)
					{
						rootWindowsMatch = topMostToolStrip.FindForm() == Form.ActiveForm;
					}

					if (rootWindowsMatch && item.ProcessCmdKeyInternal(ref m, shortcut))
					{
						retVal = true;
						break;
					}
				}
			}
			if (needsPrune)
			{
				PruneToolStripList();
			}
			return retVal;
		}
		return false;
	}

	internal static bool FindToolStripMenuItem(ToolStrip toolStrip, Keys shortcut, out ToolStripMenuItem foundMenuItem)
	{
		foundMenuItem = null;
		for (var i = 0; i < toolStrip.Items.Count; i++)
		{
			var item = toolStrip.Items[i];
			if (item is ToolStripMenuItem)
			{
				var newItem = item as ToolStripMenuItem;
				if (newItem.ShortcutKeys == shortcut)
				{
					foundMenuItem = newItem;
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	///  Determines if the key combination is valid for a shortcut.
	///  Must have a modifier key + a regular key.
	/// </summary>
	public static bool IsValidShortcut(Keys shortcut)
	{
		// Should have a key and one or more modifiers.
		Keys keyCode = shortcut & Keys.KeyCode;
		Keys modifiers = shortcut & Keys.Modifiers;

		if (shortcut == Keys.None)
		{
			return false;
		}
		else if ((keyCode == Keys.Delete) || (keyCode == Keys.Insert))
		{
			return true;
		}
		else if (((int)keyCode >= (int)Keys.F1) && ((int)keyCode <= (int)Keys.F24))
		{
			// Function keys by themselves are valid
			return true;
		}
		else if ((keyCode != Keys.None) && (modifiers != Keys.None))
		{
			switch (keyCode)
			{
				case Keys.Menu:
				case Keys.ControlKey:
				case Keys.ShiftKey:
					// Shift, control and alt aren't valid on their own.
					return false;
				default:
					if (modifiers == Keys.Shift)
					{
						// Shift + somekey isn't a valid modifier either
						return false;
					}

					return true;
			}
		}

		// Has to have a valid keycode and valid modifier.
		return false;
	}

	internal static bool IsMenuKey(Keys keyData)
	{
		Keys keyCode = keyData & Keys.KeyCode;
		return (Keys.Menu == keyCode || Keys.F10 == keyCode);
	}

	internal static MenuStrip GetMainMenuStrip(Control control)
	{
		if (control is null)
		{
			Debug.Fail("why are we passing null to GetMainMenuStrip?");
			return null;
		}

		// Look for a particular main menu strip to be set.
		Form mainForm = control.FindForm();
		if (mainForm is not null && mainForm.MainMenuStrip is not null)
		{
			return mainForm.MainMenuStrip;
		}

		// If not found go through the entire collection.
		return GetFirstMenuStripRecursive(control.Controls);
	}

	static MenuStrip GetFirstMenuStripRecursive(Control.ControlCollection controlsToLookIn)
	{
		try
		{
			// Perform breadth first search - as it's likely people will want controls belonging
			// to the same parent close to each other.
			for (int i = 0; i < controlsToLookIn.Count; i++)
			{
				if (controlsToLookIn[i] is null)
				{
					continue;
				}

				if (controlsToLookIn[i] is MenuStrip)
				{
					return controlsToLookIn[i] as MenuStrip;
				}
			}

			// Recursive search for controls in child collections.
			for (int i = 0; i < controlsToLookIn.Count; i++)
			{
				if (controlsToLookIn[i] is null)
				{
					continue;
				}

				if ((controlsToLookIn[i].Controls is not null) && controlsToLookIn[i].Controls.Count > 0)
				{
					// If it has a valid child collection, append those results to our collection
					MenuStrip menuStrip = GetFirstMenuStripRecursive(controlsToLookIn[i].Controls);
					if (menuStrip is not null)
					{
						return menuStrip;
					}
				}
			}
		}
		catch (Exception e) when (!ClientUtils.IsCriticalException(e))
		{
		}

		return null;
	}
}
