// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;

namespace System.Windows.Forms.ButtonInternal
{
    internal class CheckBoxFlatAdapter : CheckBoxBaseAdapter
    {
        internal CheckBoxFlatAdapter(ButtonBase control) : base(control) { }

        #region Layout

        internal override LayoutOptions CommonLayout()
        {
	        LayoutOptions layout = base.CommonLayout();
	        layout.CheckSize = FlatCheckSize;
	        return layout;
        }

		#endregion
	}
}
