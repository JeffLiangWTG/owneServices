// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms.ButtonInternal
{
	internal sealed class CheckBoxStandardAdapter : CheckBoxBaseAdapter
	{
		internal CheckBoxStandardAdapter(ButtonBase control) : base(control) { }

		internal override LayoutOptions CommonLayout()
		{
			LayoutOptions layout = base.CommonLayout();
			layout.DotNetOneButtonCompat = !Application.RenderWithVisualStyles;
			layout.CheckAndTextSpace = 1;
			layout.CheckSize = Control.Appearance == Appearance.Button ? 0 : layout.CheckSize;
			return layout;
		}
	}
}


