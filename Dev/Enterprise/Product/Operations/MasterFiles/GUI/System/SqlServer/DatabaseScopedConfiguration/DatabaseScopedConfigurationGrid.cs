using System.Drawing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	sealed partial class DatabaseScopedConfigurationGrid : ZUserControl
	{
		public DatabaseScopedConfigurationGrid()
		{
			InitializeComponent();
		}

		public void RefreshBinding(DatabaseScopedConfigurationCollection configurations)
		{
			BindingSource.DataSource = configurations;
		}

		void OnGridColorDeciding(object sender, ColourDecidingEventArgs e)
		{
			if (!defaultColor.HasValue)
			{
				defaultColor = e.Colour;
			}

			if (!(e.ObjectAtRow is DatabaseScopedConfiguration configuration))
			{
				return;
			}

			if (configuration.IsDbReadOnly)
			{
				return;
			}

			if (configuration.CanBeSavedPotentially)
			{
				e.Colour = Color.Yellow;
				return;
			}

			if (configuration.HasRecommendedValue)
			{
				e.Colour = Color.LightSkyBlue;
				return;
			}

			e.Colour = defaultColor.Value;
		}

		Color? defaultColor;
	}
}
