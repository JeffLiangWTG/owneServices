// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;

namespace System.Windows.Forms.ButtonInternal
{
    internal class RadioButtonPopupAdapter : RadioButtonFlatAdapter
    {
	    private const int PopupCheckSize = 10;

		internal RadioButtonPopupAdapter(ButtonBase control) : base(control) { }

        internal override LayoutOptions CommonLayout()
        {
	        LayoutOptions layout = base.CommonLayout();
	        layout.CheckSize = Control.Appearance == Appearance.Button ? 0 : PopupCheckSize;
			return layout;
        }
	}
}
