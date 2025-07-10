using System.Drawing;
using System.Globalization;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DensityCube : ZUserControl
	{
		public DensityCube(Color color)
		{
			DefaultColor = color;
			InitializeComponent();
			SetInactive();
		}

		public void SetActive(ZDecimal densityFactor)
		{
			BackColor = ActiveColor;
			DensityFactorLabel.Text = densityFactor.ToString("n2", CultureInfo.InvariantCulture);
		}

		public void SetInactive()
		{
			BackColor = DefaultColor;
			DensityFactorLabel.Text = ZString.Empty;
		}

		Color DefaultColor { get; }

		Color ActiveColor => Color.FromArgb(0, 168, 225);
	}
}
