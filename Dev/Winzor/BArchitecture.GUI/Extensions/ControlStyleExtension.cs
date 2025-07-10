using System.Drawing;
using System.Windows.Forms;
using Extensions;

namespace WinzorFramework.Extensions;

public static class ControlStyleExtension
{
	public static string ControlStyle(this Control control, ControlStyleOptions styleOptions = default(ControlStyleOptions))
	{
		return $"{control.LayoutStyle()}{control.LookStyle(styleOptions)}{control.Cursor.CursorStyleVars()}";
	}

	public static string LayoutStyle(this Control control)
	{
		var result = $"position:absolute;{control.SizeStyle()}top:{control.Top}px;left:{control.Left}px;";
		if (control.ZIndex != 0)
		{
			result += $"z-index:{control.ZIndex};";
		}
		if (!string.IsNullOrWhiteSpace(control.Visibility))
		{
			result += $"visibility: {control.Visibility};";
		}
		return result;
	}

	public static string SizeStyle(this Control control)
	{
		return $"width:{control.Width}px;height:{control.Height}px;";
	}

	public static string ToolStripItemImageStyleString(this ToolStripItem control) => $"width:{control.PreferredImageSize.Width}px;height:{control.PreferredImageSize.Height}px;";

	public static string LookStyle(this Control control, ControlStyleOptions styleOptions = default(ControlStyleOptions))
	{
		var lookStyleString = control.BackgroundColorStyleString();

		if (styleOptions.BackgroundImage)
		{
			lookStyleString += control.BackgroundImageStyleString();
		}

		lookStyleString += control.FontStyleString();

		return lookStyleString;
	}

	internal static class FontType
	{
		internal const string MicrosoftSansSerif = "Microsoft Sans Serif";
	}

	public static string BackgroundColorStyleString(this Control control)
	{
		if (control is IRendererControlledElement rControl && rControl.RendererControlledBackColor != Color.Empty)
		{
			return $"background-color:{GetColorStyleValue(rControl.RendererControlledBackColor)};";
		}

		if (control is Button && (((ButtonBase)control).FlatStyle == FlatStyle.System || control.BackColor == Color.Transparent))
		{
			return string.Empty;
		}

		if (control is ToolStripItem && control.BackColor == Color.Transparent)
		{
			return $"background-color: inherit;";
		}

		var parentBackground = control.Parent?.BackgroundImageStyleString();

		if (parentBackground is not null && control.BackColor == Color.Transparent && parentBackground.Contains("background-image: inherit"))
		{
			return $"background-color: transparent;";
		}

		// SystemColors.Window is a system color to match with the OS theme.
		// We can't invoke the system call to get SystemColors.Window so simply paint it with a similar color.
		if (!control.Enabled && control.BackColor == SystemColors.Window)
		{
			return $"background-color:{GetColorStyleValue(SystemColors.Control)};";
		}

		return $"background-color:{GetColorStyleValue(control.FindControlRealBackColor())};";
	}

	/// <summary>
	/// Creates a font style string that includes color, font family, line height, font style, font weight, text decoration, and font size
	/// Prefer this method over <see cref="FontStyleString(Font, bool)"/>
	/// </summary>
	/// <param name="control"></param>
	/// <returns>string</returns>
	public static string FontStyleString(this Control control)
	{
		var font = control.Font;
		var parent = control.Parent;
		var fontStyleString = font.FontStyleString(control, parent);
		return fontStyleString;
	}

	/// <summary>
	///  Creates a font style string that includes font style, font weight, text decoration, and font size
	///  Should only be used when working directly with fonts.
	/// </summary>
	/// <param name="font"></param>
	/// <param name="control"></param>
	/// <param name="parent"></param>
	/// <returns></returns>
	public static string FontStyleString(this Font font, Control? control = null, Control? parent = null)
	{
		var fontStyleString = string.Empty;

		if (control is not null)
		{
			var parentColor = parent?.ForeColor;
			if (parentColor is null || (parentColor is not null && control.ForeColor != parentColor))
			{
				fontStyleString += $"color:{GetColorStyleValue(control.ForeColor)};";
			}

			var parentFont = parent?.Font;

			if (parentFont is null || (control.HasFontSet && font.Name != parentFont.Name))
			{
				fontStyleString += $"font-family:{font.Name};";
			}

			var parentFontHeight = parent?.FontHeight;

			if (parentFont is null || (control.HasFontSet && control.FontHeight != parentFontHeight))
			{
				fontStyleString += $"line-height:{control.FontHeight}px;";
			}

			if (parentFont is null || (control.HasFontSet && font.Size != parentFont.Size))
			{
				fontStyleString += $"font-size:{font.Size}pt;";
			}
		}

		if (font.Italic)
		{
			fontStyleString += "font-style:italic;";
		}

		if (font.Bold)
		{
			fontStyleString += "font-weight:bold;";
		}

		if (font.Underline || font.Strikeout)
		{
			var textDecoration = "text-decoration:";
			if (font.Underline)
			{
				textDecoration += " underline";
			}
			if (font.Strikeout)
			{
				textDecoration += " line-through";
			}

			fontStyleString += textDecoration + ';';
		}

		return fontStyleString;
	}

	public static string GetColorStyleValue(this Color color, double? opacity = null, bool skipCssVariable = false)
	{
		if (color == Color.Transparent)
		{
			return "inherit";
		}

		if (opacity != null)
		{
			opacity = Math.Max(0.0, Math.Min(1.0, opacity.Value));

			color = Color.FromArgb((int)(opacity.Value * 255), color.R, color.G, color.B);
		}

		if ((opacity == 1 || opacity is null) && color.IsSystemColor && !skipCssVariable)
		{
			return $"var(--color-{color.Name.ToLowerHyphen()})";
		}

		return $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}{color.A.ToString("X2")}";
	}

	public static string TabIndexHtmlAttributeValue(this Control control) => control.TabIndexHtmlAttributeValue(c => c.TabStop);

	public static string TabIndexHtmlAttributeValue<T>(this T control, Func<T, bool> isTabStoppingFunc, bool? childTabStop = null) where T : Control
	{
		//In WinForms, TabStop does not affect Forms and SplitterPanels, so these are skipped.
		if (control is SplitterPanel || control is Form)
		{
			//Find the next valid control
			Control? nextValidControl = null;
			if (control.Parent != null)
			{
				var currentParent = control.Parent;
				while (currentParent != null)
				{
					if (currentParent.Parent is not SplitterPanel &&
						currentParent.Parent is not Form)
					{
						nextValidControl = currentParent.Parent;
						break;
					}

					currentParent = currentParent.Parent;
				}
			}

			if (nextValidControl != null)
			{
				return nextValidControl.TabIndexHtmlAttributeValue(c => c is not ContainerControl || c.TabStop, childTabStop);
			}

			if (childTabStop != null)
			{
				return (bool)childTabStop ? "0" : "-1";
			}

			return "-1";
		}

		//We check the parent's TabStop value, as TabStop = false will affect children controls.
		var tabStop = isTabStoppingFunc(control);
		if (tabStop && control.Parent != null)
		{
			return control.Parent.TabIndexHtmlAttributeValue(c => c is not ContainerControl || c.TabStop, tabStop);
		}

		return tabStop ? "0" : "-1";
	}

	public static string BorderStyleClass(this BorderStyle borderStyle)
	{
		return borderStyle.ToString().ToLower();
	}

	public static string TextAlign(this TextBox textbox)
	{
		return textbox.TextAlign == HorizontalAlignment.Left ? string.Empty : $"--textbox-align:{textbox.TextAlign};";
	}

	public static string TextAlign(this DataGridColumnStyle dataGridTextBoxColumn)
	{
		return $"text-align: {dataGridTextBoxColumn.Alignment.ToString().ToLower()};";
	}

	public static string TextAlign(this NumericUpDown numericUpDown)
	{
		return numericUpDown.TextAlign == HorizontalAlignment.Left
			? string.Empty
			: $"text-align:{numericUpDown.TextAlign.ToString().ToLower()};";
	}

	public static string BackgroundImageStyleString(this Control control)
	{
		if (control.BackgroundImage is not null)
		{
			return $"background-image: url('{control.BackgroundImage.ToBase64DataUrl()}'); background-repeat: no-repeat;";
		}
		else if (control.Parent?.BackgroundImage is not null && control.BackColor == Color.Transparent)
		{
			return $"background-image: inherit; background-repeat: no-repeat; background-position: {-control.Left}px {-control.Top}px;";
		}
		else
		{
			return string.Empty;
		}
	}

	public static string BackgroundImageLayoutClass(this Control control)
	{
		if (control.BackgroundImage is not null)
		{
			return control.BackgroundImageLayout switch
			{
				ImageLayout.Tile => "backgroundimage--tile",
				ImageLayout.Center => "backgroundimage--center",
				ImageLayout.Stretch => "backgroundimage--stretch",
				ImageLayout.Zoom => "backgroundimage--zoom",
				_ => string.Empty,
			};
		}
		return string.Empty;
	}

	public static string LinkBehaviorClass(this LinkLabel linkLabel)
	{
		var link = linkLabel.LinkBehavior;
		if (link == LinkBehavior.SystemDefault)
		{
			link = LinkUtilities.GetIELinkBehavior();
		}
		return link switch
		{
			LinkBehavior.HoverUnderline => "linklabel--hoverunderline",
			LinkBehavior.AlwaysUnderline => "linklabel--alwaysunderline",
			LinkBehavior.NeverUnderline => "linklabel--neverunderline",
			_ => string.Empty
		};
	}

	public static string FlatButtonAppearanceStyleString(this ButtonBase button, Color hoverCandidate)
	{
		var hoverColor = (button.FlatAppearance.MouseOverBackColor.IsEmpty ? hoverCandidate : button.FlatAppearance.MouseOverBackColor).GetColorStyleValue();
		var activeColor = (button.FlatAppearance.MouseDownBackColor.IsEmpty ? button.BackColor : button.FlatAppearance.MouseDownBackColor).GetColorStyleValue();
		return $"border-width: {button.FlatAppearance.BorderSize}px;border-color: {button.FlatAppearance.BorderColor.GetColorStyleValue()};--flat-button-hover-color: {hoverColor};--flat-button-active-color: {activeColor};";
	}

	public static string MnemonicStyleVars(bool showMnemonic) => showMnemonic ? "--mnemonic-text-decoration: underline;" : "--mnemonic-text-decoration: none;";
	public static string CursorStyleVars(this Cursor cursor)
	{
		var style = string.Empty;
		var styleFormat = "--current-cursor:{0};";
		if (cursor != null && !string.IsNullOrEmpty(cursor.CursorData) && cursor != Cursors.Default)
		{
			style = cursor.CursorType switch
			{
				CursorType.System => string.Format(styleFormat, cursor.CursorData),
				CursorType.SystemCursor => string.Format(styleFormat, $"url(/_content/WinzorFramework/images/cursors/{cursor.CursorData}), default"),
				_ => string.Empty
			};
		}

		return style;
	}
}
