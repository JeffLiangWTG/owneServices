using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Module
{
	public partial class JobAirSailingFilterControl
	{
		void InitializeComponent()
		{
			var zTextBoxAircraftTypeColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();

			zTextBoxAircraftTypeColumnStyleInfo.CaptionResourceString = Res.GetData("JobSeaSailingFilterControl|abdedc50-c08c-4557-9145-e72535512c3d", "Aircraft Type");
			zTextBoxAircraftTypeColumnStyleInfo.ColumnName = "JX_JV_AircraftType";
			zTextBoxAircraftTypeColumnStyleInfo.IsVisible = false;
			zTextBoxAircraftTypeColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(70);

			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("JobAirSailingFilterControl|E740AA2E-617E-42CC-B9BE-9BF9257E3950", "Flight Status");
			zTextBoxColumnStyleInfo1.ColumnName = "OnlineScheduleStatusDescription";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(140);

			this.grid.ColumnStyles.Add(zTextBoxAircraftTypeColumnStyleInfo);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
		}

	}
}
