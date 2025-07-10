// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms.ButtonInternal
{
    internal class RadioButtonStandardAdapter : RadioButtonBaseAdapter
    {
	    internal RadioButtonStandardAdapter(ButtonBase control) : base(control) { }

		#region Temp
		internal override LayoutOptions CommonLayout()
		{
			LayoutOptions layout = base.CommonLayout();
			layout.HintTextUp = false;
            layout.DotNetOneButtonCompat = !Application.RenderWithVisualStyles;

			if (Control.Appearance == Appearance.Button)
            {
	            layout.CheckSize = 0;
	            layout.Client.Width -= 4;
	            layout.Client.Height -= 4;
			}
			return layout;
		}

        #endregion
    }
}
