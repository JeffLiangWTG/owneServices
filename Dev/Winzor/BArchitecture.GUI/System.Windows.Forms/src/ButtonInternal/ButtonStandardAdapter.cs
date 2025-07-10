// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;

namespace System.Windows.Forms.ButtonInternal
{
	internal class ButtonStandardAdapter : ButtonBaseAdapter
    {
		internal ButtonStandardAdapter(ButtonBase control) : base(control)
		{
		}

		private const int BorderWidth = 2;

		private const int BlankWidth = 1;

		private const int TotalWidth = BorderWidth + BlankWidth;

		internal override LayoutOptions CommonLayout()
        {
			LayoutOptions layout = base.CommonLayout();
			layout.TextOffset = false;
	        layout.DotNetOneButtonCompat = false;
			return layout;
        }

        internal override string GetTextBoundsStyleString(LayoutData layout)
        {
	        var textBounds = layout.TextBounds;
	        var lineHeight = string.Empty;
	        if (Control.Font.FontFamily.Name == "Tahoma" && textBounds.Height < 15)
	        {
		        lineHeight = $"line-height: {textBounds.Height}px;";
		        textBounds.Height += 1;
	        }
			return $"position: absolute; top: {textBounds.Top - TotalWidth - 1}px; left: {textBounds.Left - TotalWidth - 3}px; width: {textBounds.Width}px; height: {textBounds.Height}px;{lineHeight}";
		}

		internal override string GetImageBoundsStyleString(LayoutData layout)
        {
			var imageBounds = layout.ImageBounds;
			return $"position: absolute; top: {imageBounds.Top - TotalWidth - 1}px; left: {imageBounds.Left - TotalWidth - 1}px; width: {imageBounds.Width}px; height: {imageBounds.Height}px;";
		}

		internal override string GetOuterDivBoundsStyleString()
		{
			var buttonBounds = Rectangle.Inflate(Control.Bounds, -BlankWidth, -BlankWidth);
			return GetCommonPositionStyleString(buttonBounds);
		}

		internal override string GetInnerDivControlStyleString()
		{
			var buttonBounds = Rectangle.Inflate(Control.Bounds, -BlankWidth, -BlankWidth);
			return $"position: absolute; top: {BorderWidth}px; left: {BorderWidth}px; width: {buttonBounds.Width - TotalWidth * 2}px; height: {buttonBounds.Height - TotalWidth * 2}px;";
		}
	}
}
