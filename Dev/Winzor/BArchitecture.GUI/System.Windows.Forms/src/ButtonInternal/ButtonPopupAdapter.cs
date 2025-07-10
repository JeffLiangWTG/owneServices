// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms.ButtonInternal
{
	internal class ButtonPopupAdapter : ButtonBaseAdapter
    {
	    readonly int paintedBorder;

		internal ButtonPopupAdapter(ButtonBase control) : base(control)
		{
			paintedBorder = 1;
		}

        internal override LayoutOptions CommonLayout()
        {
	        LayoutOptions layout = base.CommonLayout();
	        layout.BorderSize = paintedBorder;
	        layout.PaddingSize = 2 - paintedBorder;//3 - paintedBorder - (Control.IsDefault ? 1 : 0);
	        layout.HintTextUp = false;
	        layout.TextOffset = false;
			return layout;
        }
	}
}
