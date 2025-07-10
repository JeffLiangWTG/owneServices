using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using WinzorFramework;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class RunSheetTile : ZGroupBoxTile
	{
		public override string ExtraStyleString => base.ExtraStyleString + $"background-color: {GetRGBFromColor(TileColorGradient)}; border: 1px solid {GetRGBFromColor(BorderColor)}; border-radius: 10px; background-image: linear-gradient(180deg, {GetRGBFromColor(TileColor)}, {GetRGBFromColor(TileColorGradient)});";

		protected override EventAttribute EventAttributes => EventAttribute.MouseDown;

		protected override void OnPaint(PaintEventArgs e)
		{
			string innerBorderColor = GetRGBFromColor(PanelBorderColor);
			BodyPanel.ExtraStyleString += $"background-color: {GetRGBFromColor(TileColorGradient)}; border-top: 1px solid {innerBorderColor}; border-left: 1px solid {innerBorderColor}; border-right: 1px solid {innerBorderColor};";
			StatusBarPanel.ExtraStyleString += $"border-bottom: 1px solid {innerBorderColor}; border-left: 1px solid {innerBorderColor}; border-right: 1px solid {innerBorderColor};";
			OpenRunSheetButton.ExtraStyleString += $"border-radius: 4px;";
			titleLabel.Text = TitleBarText.ToString();
			OpenRunSheetButton.AllowOverlap(titleLabel);
		}

		public String GetRGBFromColor(Color color)
		{
			return $"rgb({color.R}, {color.G}, {color.B})";
		}
	}
}
