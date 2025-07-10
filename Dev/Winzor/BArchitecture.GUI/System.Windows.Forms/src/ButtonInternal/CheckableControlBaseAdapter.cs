// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms.ButtonInternal
{
	/// <summary>
	///  Common class for RadioButtonBaseAdapter and CheckBoxBaseAdapter
	/// </summary>
	internal abstract class CheckableControlBaseAdapter : ButtonBaseAdapter
	{
		private const int StandardCheckSize = 13;

		internal CheckableControlBaseAdapter(ButtonBase control) : base(control) { }

		internal override LayoutOptions CommonLayout()
		{
			LayoutOptions layout = base.CommonLayout();
			layout.GrowBorderBy1PxWhenDefault = false;
			layout.BorderSize = 0;
			layout.PaddingSize = 0;
			layout.MaxFocus = false;
			layout.FocusOddEvenFixup = true;
			layout.CheckSize = StandardCheckSize;
			return layout;
		}
	}
}
