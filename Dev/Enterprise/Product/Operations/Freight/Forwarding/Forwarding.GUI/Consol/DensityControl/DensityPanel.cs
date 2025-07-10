using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DensityPanel : ZPanel
	{
		public DensityPanel(DensityVisualisationControl visualisationControl)
		{
			ParentControl = visualisationControl;
			InitializeComponent();
		}

		public void ConfigureWidth(int scaledPanelWidth, int scaledCubeWidth)
		{
			Width = ControlDpiScalingHelper.MarkAsScaled(scaledPanelWidth);

			var xLocation = 0;
			foreach (var cube in DensityCubeDictionary.Values)
			{
				cube.Width = ControlDpiScalingHelper.MarkAsScaled(scaledCubeWidth);
				cube.DensityFactorLabel.Width = ControlDpiScalingHelper.MarkAsScaled(scaledCubeWidth);
				cube.Left = ControlDpiScalingHelper.MarkAsScaled(xLocation);
				xLocation += scaledCubeWidth + 1;
			}
		}

		public void SetActiveCube()
		{
			ActiveCube?.SetInactive();

			if (ParentControl != null)
			{
				var densityFactor = ParentControl.DensityFactor;
				ActiveCube = GetAppropriateCube(densityFactor);
				ActiveCube.SetActive(densityFactor);
			}
		}

		public void ResetActiveCube()
		{
			ActiveCube?.SetInactive();
		}

		DensityCube GetAppropriateCube(ZDecimal densityFactor)
		{
			var key = DensityCubeDictionary.Keys
				.OrderBy(x => -x)
				.FirstOrDefault(d => densityFactor >= d);

			return DensityCubeDictionary[key];
		}

		DensityVisualisationControl ParentControl { get; }

		DensityCube ActiveCube { get; set; }

		Dictionary<ZDecimal, DensityCube> DensityCubeDictionary => densityCubeDictionary
			?? (densityCubeDictionary = new Dictionary<ZDecimal, DensityCube>
			{
				[Density.DensityValuesList[0]] = DensityCube_1_1,
				[Density.DensityValuesList[1]] = DensityCube_1_2,
				[Density.DensityValuesList[2]] = DensityCube_1_3,
				[Density.DensityValuesList[3]] = DensityCube_1_4,
				[Density.DensityValuesList[4]] = DensityCube_1_5,
				[Density.DensityValuesList[5]] = DensityCube_1_6,
				[Density.DensityValuesList[6]] = DensityCube_1_7,
				[Density.DensityValuesList[7]] = DensityCube_1_8,
				[Density.DensityValuesList[8]] = DensityCube_1_9,
				[Density.DensityValuesList[9]] = DensityCube_1_10,
				[Density.DensityValuesList[10]] = DensityCube_1_11,
				[Density.DensityValuesList[11]] = DensityCube_1_12,
			});

		Dictionary<ZDecimal, DensityCube> densityCubeDictionary;
	}
}
