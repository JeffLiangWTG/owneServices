// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms.ButtonInternal
{
	/// <summary>
	///        PLEASE READ
	///        -----------
	///  This class is used for more than just Button:
	///  it's used for things that derive from ButtonBase,
	///  parts of ToolStripItem, and parts of the DataGridView.
	/// </summary>
	internal abstract partial class ButtonBaseAdapter
	{
		readonly ButtonBase control;
		internal ButtonBaseAdapter(ButtonBase control)
		{
			this.control = control;
		}

		protected ButtonBase Control
		{
			get { return control; }
		}
		internal class ColorOptions
		{
			Color backColor;
			Color foreColor;
			bool enabled;
			bool highContrast;

			internal ColorOptions(Color foreColor, Color backColor, bool enabled)
			{
				this.backColor = backColor;
				this.foreColor = foreColor;
				this.enabled = enabled;
				highContrast = SystemInformation.HighContrast;
			}

			internal static int Adjust255(float percentage, int value)
			{
				var v = (int)(percentage * value);
				if (v > 255)
				{
					return 255;
				}
				return v;
			}

			internal ColorData Calculate()
			{
				var colors = new ColorData(this)
				{
					buttonFace = backColor
				};

				if (backColor == SystemColors.Control)
				{
					colors.buttonShadow = SystemColors.ControlDark;
					colors.buttonShadowDark = SystemColors.ControlDarkDark;
					colors.highlight = SystemColors.ControlLightLight;
				}
				else
				{
					if (!highContrast)
					{
						colors.buttonShadow = ControlPaint.Dark(backColor);
						colors.buttonShadowDark = ControlPaint.DarkDark(backColor);
						colors.highlight = ControlPaint.LightLight(backColor);
					}
					else
					{
						colors.buttonShadow = ControlPaint.Dark(backColor);
						colors.buttonShadowDark = ControlPaint.LightLight(backColor);
						colors.highlight = ControlPaint.LightLight(backColor);
					}
				}
				colors.windowDisabled = highContrast ? SystemColors.GrayText : colors.buttonShadow;

				const float lowlight = .1f;
				var adjust = 1 - lowlight;

				if (colors.buttonFace.GetBrightness() < .5)
				{
					adjust = 1 + lowlight * 2;
				}
				colors.lowButtonFace = Color.FromArgb(Adjust255(adjust, colors.buttonFace.R),
													Adjust255(adjust, colors.buttonFace.G),
													Adjust255(adjust, colors.buttonFace.B));

				adjust = 1 - lowlight;
				if (colors.highlight.GetBrightness() < .5)
				{
					adjust = 1 + lowlight * 2;
				}
				colors.lowHighlight = Color.FromArgb(Adjust255(adjust, colors.highlight.R),
												   Adjust255(adjust, colors.highlight.G),
												   Adjust255(adjust, colors.highlight.B));

				if (highContrast && backColor != SystemColors.Control)
				{
					colors.highlight = colors.lowHighlight;
				}

				colors.windowFrame = foreColor;

				if (colors.buttonFace.GetBrightness() < .5)
				{
					colors.constrastButtonShadow = colors.lowHighlight;
				}
				else
				{
					colors.constrastButtonShadow = colors.buttonShadow;
				}

				if (!enabled)
				{
					colors.windowText = colors.windowDisabled;
					if (highContrast)
					{
						colors.windowFrame = colors.windowDisabled;
						colors.buttonShadow = colors.windowDisabled;
					}
				}
				else
				{
					colors.windowText = colors.windowFrame;
				}

				return colors;
			}
		}

		internal virtual LayoutOptions CommonLayout()
		{
			LayoutOptions layout = new LayoutOptions
			{
				Client = LayoutUtils.DeflateRect(Control.ClientRectangle, Control.Padding),
				Padding = Control.Padding,
				GrowBorderBy1PxWhenDefault = true,
				IsDefault = Control.IsDefault,
				BorderSize = 2,
				PaddingSize = 0,
				MaxFocus = true,
				FocusOddEvenFixup = false,
				Font = Control.Font,
				Text = Control.Text,
				ImageSize = (Control.Image == null) ? Size.Empty : Control.Image.Size,
				CheckSize = 0,
				CheckPaddingSize = 0,
				CheckAlign = ContentAlignment.TopLeft,
				ImageAlign = Control.ImageAlign,
				TextAlign = Control.TextAlign,
				HintTextUp = false,
				LayoutRTL = RightToLeft.Yes == Control.RightToLeft,
				TextImageRelation = Control.TextImageRelation,
				UseCompatibleTextRendering = Control.UseCompatibleTextRendering
			};
			return layout;
		}

		internal virtual Size GetPreferredSizeCore(Size proposedSize)
		{
			LayoutOptions options = CommonLayout();
			return options.GetPreferredSizeCore(proposedSize);
		}

		public string GetCommonPositionStyleString(Rectangle bounds)
		{
			return $" position: absolute; top: {bounds.Top}px; left: {bounds.Left}px; width: {bounds.Width}px; height: {bounds.Height}px;";
		}

		internal virtual string GetTextBoundsStyleString(LayoutData layout)
		{
			return GetCommonPositionStyleString(layout.TextBounds);
		}

		internal virtual string GetImageBoundsStyleString(LayoutData layout)
		{
			return GetCommonPositionStyleString(layout.ImageBounds);
		}

		internal virtual string GetCheckBoundsStyleString(LayoutData layout)
		{
			return GetCommonPositionStyleString(layout.CheckBounds);
		}

		internal virtual string GetInnerDivControlStyleString()
		{
			return GetCommonPositionStyleString(Control.ClientRectangle) + control.InnerDivStyleString;
		}

		internal virtual string GetOuterDivBoundsStyleString()
		{
			return string.Empty;
		}

		internal class ColorData
		{
			internal Color buttonFace;
			internal Color buttonShadow;
			internal Color buttonShadowDark;
			internal Color constrastButtonShadow;
			internal Color windowText;
			internal Color windowDisabled;
			internal Color highlight;
			internal Color lowHighlight;
			internal Color lowButtonFace;
			internal Color windowFrame;

			internal ColorOptions options;

			internal ColorData(ColorOptions options)
			{
				this.options = options;
			}
		}
	}
}
