using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DensityVisualisationControl : ZUserControl
	{
		public DensityVisualisationControl()
		{
			InitializeComponent();
		}

		public void ConfigureWidth(int width)
		{
			Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(width);
			var cubeWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX((width - intermediateSpace) / numberOfCubes);

			var cubeMargin = cubeWidth / 7;
			MidLabel.Left = ControlDpiScalingHelper.MarkAsScaled((cubeWidth + 1) * 5 + cubeMargin);

			DensityPanel.ConfigureWidth(Width, cubeWidth);
		}

		public ZDecimal DensityFactor => densityFactor ?? (densityFactor = ParentDensity.DensityFactor).Value;
		ZDecimal? densityFactor;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (ParentDensity != null)
			{
				ParentDensity.DensityFactorInfo.ValueChanged += DensityFactorChanged;
				densityFactor = ParentDensity.DensityFactor;
				DensityPanel.SetActiveCube();
			}
			else
			{
				DensityPanel.ResetActiveCube();
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (ParentDensity != null)
			{
				ParentDensity.DensityFactorInfo.ValueChanged -= DensityFactorChanged;
			}
		}

		void DensityFactorChanged(object sender, EventArgs e)
		{
			if (ParentDensity != null)
			{
				densityFactor = ParentDensity.DensityFactor;
				DensityPanel.SetActiveCube();
			}
		}

		Density ParentDensity => (Density)CurrentDataItem;

		const int numberOfCubes = 12;

		const int intermediateSpace = 11;
	}
}
