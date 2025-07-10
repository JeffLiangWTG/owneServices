using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class VerticalLabel : ZLabel
	{
#if !WINZOR
		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;

			using (var stringFormat = new StringFormat
			{
				Alignment = StringAlignment.Far,
				Trimming = StringTrimming.None,
				FormatFlags = StringFormatFlags.DirectionVertical,
			})
			using (var textBrush = new SolidBrush(this.ForeColor))
			using (var storedState = g.Transform)
			{
				g.RotateTransform(180f);
				g.TranslateTransform(-ClientRectangle.Width, -ClientRectangle.Height);

				TextRendererHelper.DrawText(g, this.Text, this.Font, ClientRectangle, textBrush, stringFormat, renderingEngine: TextRendererType.GDIPlus);
				g.Transform = storedState;
			}
		}
#else
		protected override string TextStyleString => $"position: absolute; text-align: {base.CssTextAlign}; top: 0; left: 0; height: 100%; transform: rotate(180deg);writing-mode: vertical-rl";
#endif
	}
}
