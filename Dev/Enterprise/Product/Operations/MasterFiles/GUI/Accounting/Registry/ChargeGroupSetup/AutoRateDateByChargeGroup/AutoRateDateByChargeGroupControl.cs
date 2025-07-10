using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class AutoRateDateByChargeGroupControl : ChargeGroupSettingControl
	{
		public AutoRateDateByChargeGroupControl()
		{
			AddNewColumns();
		}

		void AddNewColumns()
		{
			var rateType = new ZDropEditColumnStyleInfo();
			rateType.CharacterCasing = CharacterCasing.Upper;
			rateType.CaptionResourceString = Res.GetData("AutoRateDateByChargeGroupControl|B6E6B253-8F51-47C6-AC88-4BF586FB5C52", "Rate Type");
			rateType.ColumnName = "RateType";
			ControlDpiScalingHelper.SetWidth(ref rateType, 80, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(rateType);

			var containerMode = new ZDropEditColumnStyleInfo();
			containerMode.CharacterCasing = CharacterCasing.Upper;
			containerMode.CaptionResourceString = Res.GetData("AutoRateDateByChargeGroupControl|B73BDE66-3710-4027-89CA-306B948F667B", "Container Mode");
			containerMode.ColumnName = "ContainerMode";
			ControlDpiScalingHelper.SetWidth(ref containerMode, 60, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(containerMode);

			var date = new ZDropEditColumnStyleInfo();
			date.CharacterCasing = CharacterCasing.Upper;
			date.CaptionResourceString = Res.GetData("AutoRateDateByChargeGroupControl|7c1f8e95-7138-4b31-89b7-cac85b39fda0", "Autorate Date");
			date.ColumnName = "DateType";
			ControlDpiScalingHelper.SetWidth(ref date, 80, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(date);

			var location = new ZCodeFindBoxColumnStyleInfo();
			location.CharacterCasing = CharacterCasing.Upper;
			location.CaptionResourceString = Res.GetData("AutoRateDateByChargeGroupControl|B6D82957-D917-4192-B9EA-FF220FA5D00B", "Location");
			location.ColumnName = "Location";
			ControlDpiScalingHelper.SetWidth(ref location, 60, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(location);

			var noFallback = new ZCheckBoxColumnStyleInfo();
			noFallback.CharacterCasing = CharacterCasing.Upper;
			noFallback.CaptionResourceString = Res.GetData("AutoRateDateByChargeGroupControl|C5705464-E74A-455A-B20C-49A663069D0B", "No Fallback");
			noFallback.ColumnName = "IsFallbackDisabled";
			ControlDpiScalingHelper.SetWidth(ref noFallback, 80, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(noFallback);
		}
	}
}
