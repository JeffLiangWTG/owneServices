using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.GUI.Testing
{
	sealed partial class VoyageDetailsControlTestForm
	{
		new void InitializeComponent()
		{
			base.InitializeComponent();

			control = new VoyageDetailsControl();
			control.Dock = DockStyle.Fill;

			this.Controls.Add(control);
			this.ClientSize = control.Size;

			ExRatesGrid = (ZGrid)GetControl("exRatesGrid");
			SailingsGrid = (SailingsGrid)GetControl("jobSailingBoundGrid");

			foreach (ZGridColumnInfo info in ExRatesGrid.ColumnStyles.ToArray())
			{
				if (info.ColumnName == JobVoyageExRateSchema.E8_VoyageExchangeRate.Name)
				{
					ExRateColumnStyle = info as ZCalcEditColumnStyleInfo;
					break;
				}
			}

			this.CaptionRenderingEnabled = true;
		}
	}
}
