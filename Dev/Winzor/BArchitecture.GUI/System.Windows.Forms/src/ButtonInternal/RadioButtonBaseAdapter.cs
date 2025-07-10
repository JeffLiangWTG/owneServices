// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms.ButtonInternal
{
	internal abstract class RadioButtonBaseAdapter : CheckableControlBaseAdapter
    {
		internal RadioButtonBaseAdapter(ButtonBase control) : base(control) { }

		protected new RadioButton Control
        {
            get
            {
                return ((RadioButton)base.Control);
            }
        }

        internal override LayoutOptions CommonLayout()
        {
            LayoutOptions layout = base.CommonLayout();
            layout.CheckAlign = Control.CheckAlign;
            return layout;
        }

        internal override string GetCheckBoundsStyleString(LayoutData layout)
        {
	        var bounds = layout.CheckBounds;
	        if (bounds.Width == 13)
	        {
		        return $" position: absolute; top: {bounds.Top}px; left: {bounds.Left}px; width: 11px; height: 11px; margin: 1px";
			}
			return base.GetCheckBoundsStyleString(layout);
		}
	}
}
