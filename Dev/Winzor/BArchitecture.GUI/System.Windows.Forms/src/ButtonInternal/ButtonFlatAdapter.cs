// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms.ButtonInternal
{
	internal class ButtonFlatAdapter : ButtonBaseAdapter
    {
	    internal ButtonFlatAdapter(ButtonBase control) : base(control)
		{
		}

        internal override LayoutOptions CommonLayout()
        {
			LayoutOptions layout = base.CommonLayout();
			layout.BorderSize = base.Control.FlatAppearance.BorderSize;
			layout.PaddingSize = 2;
			layout.FocusOddEvenFixup = false;
			layout.TextOffset = false;
			return layout;
        }

	}
}
