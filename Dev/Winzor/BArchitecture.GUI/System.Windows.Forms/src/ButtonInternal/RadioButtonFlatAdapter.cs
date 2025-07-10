// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms.ButtonInternal
{
	internal class RadioButtonFlatAdapter : RadioButtonBaseAdapter
    {
	    private const int FlatCheckSize = 10;

		internal RadioButtonFlatAdapter(ButtonBase control) : base(control) { }

		internal override LayoutOptions CommonLayout()
		{
			LayoutOptions layout = base.CommonLayout();
			layout.CheckSize = Control.Appearance == Appearance.Button ? 0 : FlatCheckSize;
			return layout;
		}
	}
}
