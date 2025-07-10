using System.Drawing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class ProviderOutcomeRowControl : ZUserControl
	{
		public ProviderOutcomeRowControl(RatesServiceProviderOutcomeRow ratesServiceProviderOutcomeRow)
		{
			InitializeComponent();
			lblProviderName.Text = ratesServiceProviderOutcomeRow.ProviderName;
			lblRatesSeconds.Text = ratesServiceProviderOutcomeRow.RatesAndSpeed.ToString();
			var color = ratesServiceProviderOutcomeRow.ColorBrush.Color;
			labelRectangle.BackColor = Color.FromArgb(color.A, color.R, color.G, color.B);
		}
	}
}
