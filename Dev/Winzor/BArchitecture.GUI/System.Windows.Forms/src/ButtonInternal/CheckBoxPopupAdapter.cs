// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms.ButtonInternal
{
    internal class CheckBoxPopupAdapter : CheckBoxBaseAdapter
    {
	    internal CheckBoxPopupAdapter(ButtonBase control) : base(control) { }

		#region Layout

		internal override LayoutOptions CommonLayout()
		{
			LayoutOptions layout = base.CommonLayout();
			layout.CheckSize = FlatCheckSize;
			layout.CheckPaddingSize = 1;
			return layout;
		}

		#endregion
	}
}
