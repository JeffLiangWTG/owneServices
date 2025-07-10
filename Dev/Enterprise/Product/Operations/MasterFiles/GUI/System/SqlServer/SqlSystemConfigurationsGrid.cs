using System.Drawing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SqlSystemConfigurationsGrid : ZUserControl
	{
		public SqlSystemConfigurationsGrid()
		{
			InitializeComponent();
		}

		void OnGridColorDeciding(object sender, ColourDecidingEventArgs e)
		{
			if (savedDefaultColor == Color.Empty)
			{
				savedDefaultColor = e.Colour;
			}

			e.Colour = savedDefaultColor;
			if (e.ObjectAtRow is SqlSystemConfiguration configuration)
			{
				if (configuration.ReadOnly)
				{
					e.Colour =
						configuration.HasProposedChange
							? Color.Orange
							: SystemDataRegistry.Instance.ColorTheme.GridReadOnlyColor;
				}
				else
				{
					if (configuration.HasProposedChange)
					{
						e.Colour = Color.Yellow;
					}
					else if (configuration.HasRecommendedValue)
					{
						e.Colour = Color.LightSkyBlue;
					}
				}
			}
		}

		Color savedDefaultColor = Color.Empty;
	}
}
