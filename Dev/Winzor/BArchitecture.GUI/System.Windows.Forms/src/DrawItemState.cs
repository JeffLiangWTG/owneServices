// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms
{
	/// <summary>
	///  Specifies the state of an item that is being drawn.
	/// </summary>
	[Flags]
	public enum DrawItemState
	{
		/// <summary>
		///  The item is checked. Only menu controls use this value.
		/// </summary>
		Checked = 0x0008,

		/// <summary>
		///  The item is the editing portion of a <see cref='ComboBox'/> .
		/// </summary>
		ComboBoxEdit = 0x1000,

		/// <summary>
		///  The item is the default item of the control.
		/// </summary>
		Default = 0x0020,

		/// <summary>
		///  The item is disabled.
		/// </summary>
		Disabled = 0x0004,

		/// <summary>
		///  The item has focus.
		/// </summary>
		Focus = 0x0010,

		/// <summary>
		///  The item is grayed. Only menu controls use this value.
		/// </summary>
		Grayed = 0x0002,

		/// <summary>
		///  The item is being hot-tracked.
		/// </summary>
		HotLight = 0x0040,

		/// <summary>
		///  The item is inactive.
		/// </summary>
		Inactive = 0x0080,

		/// <summary>
		///  The item displays without a keyboard accelarator.
		/// </summary>
		NoAccelerator = 0x0100,

		/// <summary>
		///  The item displays without the visual cue that indicates it has the focus.
		/// </summary>
		NoFocusRect = 0x0200,

		/// <summary>
		///  The item is selected.
		/// </summary>
		Selected = 0x0001,

		/// <summary>
		///  The item is in its default visual state.
		/// </summary>
		None = 0,
	}
}
