using System;
using System.Drawing;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class NoDeduplicationTipUserControl : ZUserControl
	{
		readonly float originalWidth;
		readonly int originalFontSize;

		public NoDeduplicationTipUserControl(WaterMarkType waterMarkType)
		{
			InitializeComponent();
			originalWidth = Width;
			originalFontSize = waterMarkType == WaterMarkType.NoDeduplicates ? 40 : 33;
			TipLabel.Font = new Font("Tahoma", originalFontSize, FontStyle.Bold | FontStyle.Italic);
		}

		void Layout_SizeChanged(object sender, EventArgs e)
		{
			var scaleFactor = Width / originalWidth;
			TipLabel.Font = new Font("Tahoma", originalFontSize * scaleFactor, FontStyle.Bold | FontStyle.Italic);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
