// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;

namespace System.Windows.Forms;

/// <summary>
///  The ControlPaint class provides a series of methods that can be used to
///  paint common Windows UI pieces. Many windows forms controls use this class to paint
///  their UI elements.
/// </summary>
public sealed class ControlPaint
{
	//use these value to signify ANY of the right, top, left, center, or bottom alignments with the ContentAlignment enum.
	static readonly ContentAlignment anyRight = ContentAlignment.TopRight | ContentAlignment.MiddleRight | ContentAlignment.BottomRight;
	static readonly ContentAlignment anyBottom = ContentAlignment.BottomLeft | ContentAlignment.BottomCenter | ContentAlignment.BottomRight;
	static readonly ContentAlignment anyCenter = ContentAlignment.TopCenter | ContentAlignment.MiddleCenter | ContentAlignment.BottomCenter;
	static readonly ContentAlignment anyMiddle = ContentAlignment.MiddleLeft | ContentAlignment.MiddleCenter | ContentAlignment.MiddleRight;

	/// <summary>
	///  Get TextFormatFlags flags for rendering text using GDI (TextRenderer).
	/// </summary>
	internal static TextFormatFlags CreateTextFormatFlags(Control ctl, ContentAlignment textAlign, bool showEllipsis, bool useMnemonic)
	{
		textAlign = ctl.RtlTranslateContent(textAlign);
		TextFormatFlags flags = ControlPaint.TextFormatFlagsForAlignmentGDI(textAlign);

		// The effect of the TextBoxControl flag is that in-word line breaking will occur if needed, this happens when AutoSize
		// is false and a one-word line still doesn't fit the binding box (width). The other effect is that partially visible
		// lines are clipped; this is how GDI+ works by default.
		flags |= TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl;

		if (showEllipsis)
		{
			flags |= TextFormatFlags.EndEllipsis;
		}

		// Adjust string format for Rtl controls
		if (ctl.RightToLeft == RightToLeft.Yes)
		{
			flags |= TextFormatFlags.RightToLeft;
		}

		//if we don't use mnemonic, set formatFlag to NoPrefix as this will show the ampersand
		if (!useMnemonic)
		{
			flags |= TextFormatFlags.NoPrefix;
		}

		return flags;
	}

	internal static TextFormatFlags TextFormatFlagsForAlignmentGDI(ContentAlignment align)
	{
		TextFormatFlags output = new TextFormatFlags();
		output |= TranslateAlignmentForGDI(align);
		output |= TranslateLineAlignmentForGDI(align);
		return output;
	}

	/// <summary>
	///  Converts the font into one where Font.Unit == Point. If the original font is in device-dependent units
	///  (and it usually is), we interpret the size relative to the screen. This is not really a general-purpose
	///  function -- when used on something not obtained from ChooseFont, it may round away some precision.
	/// </summary>
	internal static Font FontInPoints(Font font)
		=> new Font(
			font.FontFamily,
			font.SizeInPoints,
			font.Style,
			GraphicsUnit.Point,
			font.GdiCharSet,
			font.GdiVerticalFont);

	/// <summary>
	///  Creates a new color that is a object of the given color.
	/// </summary>
	public static Color Dark(Color baseColor)
	{
		return new HLSColor(baseColor).Darker(0.5f);
	}

	/// <summary>
	///  Creates a new color that is a object of the given color.
	/// </summary>
	public static Color DarkDark(Color baseColor)
	{
		return new HLSColor(baseColor).Darker(1.0f);
	}

	/// <summary>
	///  Creates a new color that is a object of the given color.
	/// </summary>
	public static Color LightLight(Color baseColor)
	{
		return new HLSColor(baseColor).Lighter(1.0f);
	}

	internal static TextFormatFlags TranslateAlignmentForGDI(ContentAlignment align)
	{
		TextFormatFlags result;
		if ((align & anyBottom) != 0)
		{
			result = TextFormatFlags.Bottom;
		}
		else if ((align & anyMiddle) != 0)
		{
			result = TextFormatFlags.VerticalCenter;
		}
		else
		{
			result = TextFormatFlags.Top;
		}

		return result;
	}

	internal static TextFormatFlags TranslateLineAlignmentForGDI(ContentAlignment align)
	{
		TextFormatFlags result;
		if ((align & anyRight) != 0)
		{
			result = TextFormatFlags.Right;
		}
		else if ((align & anyCenter) != 0)
		{
			result = TextFormatFlags.HorizontalCenter;
		}
		else
		{
			result = TextFormatFlags.Left;
		}

		return result;
	}

	/// <summary>
	///  Logic copied from Windows sources to copy the lightening and darkening of colors.
	/// </summary>
	struct HLSColor
	{
		const int ShadowAdj = -333;
		const int HilightAdj = 500;

		const int Range = 240;
		const int HLSMax = Range;
		const int RGBMax = 255;
		const int Undefined = HLSMax * 2 / 3;

		readonly int hue;
		readonly int saturation;
		readonly int luminosity;

		readonly bool isSystemColors_Control;

		public HLSColor(Color color)
		{
			isSystemColors_Control = color.ToKnownColor() == SystemColors.Control.ToKnownColor();
			int r = color.R;
			int g = color.G;
			int b = color.B;
			int max, min;        /* max and min RGB values */
			int sum, dif;
			int rdelta, gdelta, bdelta;  /* intermediate value: % of spread from max */

			/* calculate lightness */
			max = Math.Max(Math.Max(r, g), b);
			min = Math.Min(Math.Min(r, g), b);
			sum = max + min;

			luminosity = (sum * HLSMax + RGBMax) / (2 * RGBMax);

			dif = max - min;
			if (dif == 0)
			{       /* r=g=b --> achromatic case */
				saturation = 0;                         /* saturation */
				hue = Undefined;                 /* hue */
			}
			else
			{                           /* chromatic case */
				/* saturation */
				if (luminosity <= HLSMax / 2)
				{
					saturation = (dif * HLSMax + sum / 2) / sum;
				}
				else
				{
					saturation = (dif * HLSMax + (2 * RGBMax - sum) / 2)
										/ (2 * RGBMax - sum);
				}
				/* hue */
				rdelta = ((max - r) * (HLSMax / 6) + dif / 2) / dif;
				gdelta = ((max - g) * (HLSMax / 6) + dif / 2) / dif;
				bdelta = ((max - b) * (HLSMax / 6) + dif / 2) / dif;

				if (r == max)
				{
					hue = bdelta - gdelta;
				}
				else if (g == max)
				{
					hue = HLSMax / 3 + rdelta - bdelta;
				}
				else /* B == cMax */
				{
					hue = 2 * HLSMax / 3 + gdelta - rdelta;
				}

				if (hue < 0)
				{
					hue += HLSMax;
				}

				if (hue > HLSMax)
				{
					hue -= HLSMax;
				}
			}
		}

		public int Luminosity
		{
			get
			{
				return luminosity;
			}
		}

		/// <summary>
		/// </summary>
		public Color Darker(float percDarker)
		{
			if (isSystemColors_Control)
			{
				// With the usual color scheme, ControlDark/DarkDark is not exactly
				// what we would otherwise calculate
				if (percDarker == 0.0f)
				{
					return SystemColors.ControlDark;
				}
				else if (percDarker == 1.0f)
				{
					return SystemColors.ControlDarkDark;
				}
				else
				{
					var dark = SystemColors.ControlDark;
					var darkDark = SystemColors.ControlDarkDark;

					var dr = dark.R - darkDark.R;
					var dg = dark.G - darkDark.G;
					var db = dark.B - darkDark.B;

					return Color.FromArgb((byte)(dark.R - (byte)(dr * percDarker)),
										  (byte)(dark.G - (byte)(dg * percDarker)),
										  (byte)(dark.B - (byte)(db * percDarker)));
				}
			}
			else
			{
				var oneLum = 0;
				var zeroLum = NewLuma(ShadowAdj, true);

				return ColorFromHLS(hue, zeroLum - (int)((zeroLum - oneLum) * percDarker), saturation);
			}
		}

		public override bool Equals(object? o)
		{
			if (!(o is HLSColor))
			{
				return false;
			}

			var c = (HLSColor)o;
			return hue == c.hue &&
				   saturation == c.saturation &&
				   luminosity == c.luminosity &&
				   isSystemColors_Control == c.isSystemColors_Control;
		}

		public static bool operator ==(HLSColor a, HLSColor b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(HLSColor a, HLSColor b)
		{
			return !a.Equals(b);
		}

		public override int GetHashCode() => HashCode.Combine(hue, saturation, luminosity);

		/// <summary>
		/// </summary>
		public Color Lighter(float percLighter)
		{
			if (isSystemColors_Control)
			{
				// With the usual color scheme, ControlLight/LightLight is not exactly
				// what we would otherwise calculate
				if (percLighter == 0.0f)
				{
					return SystemColors.ControlLight;
				}
				else if (percLighter == 1.0f)
				{
					return SystemColors.ControlLightLight;
				}
				else
				{
					var light = SystemColors.ControlLight;
					var lightLight = SystemColors.ControlLightLight;

					var dr = light.R - lightLight.R;
					var dg = light.G - lightLight.G;
					var db = light.B - lightLight.B;

					return Color.FromArgb((byte)(light.R - (byte)(dr * percLighter)),
										  (byte)(light.G - (byte)(dg * percLighter)),
										  (byte)(light.B - (byte)(db * percLighter)));
				}
			}
			else
			{
				var zeroLum = luminosity;
				var oneLum = NewLuma(HilightAdj, true);

				return ColorFromHLS(hue, zeroLum + (int)((oneLum - zeroLum) * percLighter), saturation);
			}
		}

		/// <summary>
		/// </summary>
		int NewLuma(int n, bool scale)
		{
			return NewLuma(luminosity, n, scale);
		}

		/// <summary>
		/// </summary>
		int NewLuma(int luminosity, int n, bool scale)
		{
			if (n == 0)
			{
				return luminosity;
			}

			if (scale)
			{
				if (n > 0)
				{
					return (int)((luminosity * (1000 - n) + (Range + 1L) * n) / 1000);
				}
				else
				{
					return luminosity * (n + 1000) / 1000;
				}
			}

			var newLum = luminosity;
			newLum += (int)((long)n * Range / 1000);

			if (newLum < 0)
			{
				newLum = 0;
			}

			if (newLum > HLSMax)
			{
				newLum = HLSMax;
			}

			return newLum;
		}

		/// <summary>
		/// </summary>
		Color ColorFromHLS(int hue, int luminosity, int saturation)
		{
			byte r, g, b;                      /* RGB component values */
			int magic1, magic2;       /* calculated magic numbers (really!) */

			if (saturation == 0)
			{                /* achromatic case */
				r = g = b = (byte)(luminosity * RGBMax / HLSMax);
				if (hue != Undefined)
				{
					/* ERROR */
				}
			}
			else
			{                         /* chromatic case */
				/* set up magic numbers */
				if (luminosity <= HLSMax / 2)
				{
					magic2 = (luminosity * (HLSMax + saturation) + HLSMax / 2) / HLSMax;
				}
				else
				{
					magic2 = luminosity + saturation - (luminosity * saturation + HLSMax / 2) / HLSMax;
				}

				magic1 = 2 * luminosity - magic2;

				/* get RGB, change units from HLSMax to RGBMax */
				r = (byte)((HueToRGB(magic1, magic2, hue + HLSMax / 3) * RGBMax + HLSMax / 2) / HLSMax);
				g = (byte)((HueToRGB(magic1, magic2, hue) * RGBMax + HLSMax / 2) / HLSMax);
				b = (byte)((HueToRGB(magic1, magic2, hue - HLSMax / 3) * RGBMax + HLSMax / 2) / HLSMax);
			}
			return Color.FromArgb(r, g, b);
		}

		/// <summary>
		/// </summary>
		int HueToRGB(int n1, int n2, int hue)
		{
			/* range check: note values passed add/subtract thirds of range */

			/* The following is redundant for WORD (unsigned int) */
			if (hue < 0)
			{
				hue += HLSMax;
			}

			if (hue > HLSMax)
			{
				hue -= HLSMax;
			}

			/* return r,g, or b value from this tridrant */
			if (hue < HLSMax / 6)
			{
				return n1 + ((n2 - n1) * hue + HLSMax / 12) / (HLSMax / 6);
			}

			if (hue < HLSMax / 2)
			{
				return n2;
			}

			if (hue < HLSMax * 2 / 3)
			{
				return n1 + ((n2 - n1) * (HLSMax * 2 / 3 - hue) + HLSMax / 12) / (HLSMax / 6);
			}
			else
			{
				return n1;
			}
		}
	}
}
