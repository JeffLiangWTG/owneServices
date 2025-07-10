// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms.ButtonInternal
{
	internal abstract class CheckBoxBaseAdapter : CheckableControlBaseAdapter
	{
		protected const int FlatCheckSize = 11;

		internal CheckBoxBaseAdapter(ButtonBase control) : base(control) { }

		protected new CheckBox Control
		{
			get
			{
				return ((CheckBox)base.Control);
			}
		}

		internal override LayoutOptions CommonLayout()
		{
			LayoutOptions layout = base.CommonLayout();
			layout.CheckAlign = Control.CheckAlign;
			layout.TextOffset = false;
			layout.LayoutRTL = RightToLeft.Yes == Control.RightToLeft;

			return layout;
		}
	}
}
