using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AutoratingDateFilteringChargeGroupAndCustomizedUserControl : ZUserControl
	{
		public AutoratingDateFilteringChargeGroupAndCustomizedUserControl()
		{
			InitializeComponent();
			AddNewColumns();
		}

		void AddNewColumns()
		{
			var rateType = new ZDropEditColumnStyleInfo();
			rateType.CharacterCasing = CharacterCasing.Upper;
			rateType.CaptionResourceString = Res.GetData("AutoratingDateFilteringChargeGroupAndCustomizedUserControl|8E0FC1A9-EFFF-45CD-A699-01B9F2B344CE", "Rate Type");
			rateType.ColumnName = "RateType";
			ControlDpiScalingHelper.SetWidth(ref rateType, 60, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(rateType);

			var containerMode = new ZDropEditColumnStyleInfo();
			containerMode.CharacterCasing = CharacterCasing.Upper;
			containerMode.CaptionResourceString = Res.GetData("AutoratingDateFilteringChargeGroupAndCustomizedUserControl|FEC847EA-38E4-4C4B-B639-6A16DD20568A", "Container Mode");
			containerMode.ColumnName = "ContainerMode";
			ControlDpiScalingHelper.SetWidth(ref containerMode, 100, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(containerMode);

			var date = new ZDropEditColumnStyleInfo();
			date.CharacterCasing = CharacterCasing.Upper;
			date.CaptionResourceString = Res.GetData("AutoratingDateFilteringChargeGroupAndCustomizedUserControl|B2537922-1A71-4AE6-AE70-40A998A902AD", "Autorate Date");
			date.ColumnName = "DateType";
			ControlDpiScalingHelper.SetWidth(ref date, 100, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(date);

			var location = new ZCodeFindBoxColumnStyleInfo();
			location.CharacterCasing = CharacterCasing.Upper;
			location.CaptionResourceString = Res.GetData("AutoratingDateFilteringChargeGroupAndCustomizedUserControl|0A0C4FC2-086F-47C9-AEF4-4E3FDED87652", "Location");
			location.ColumnName = "Location";
			ControlDpiScalingHelper.SetWidth(ref location, 60, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(location);

			var noFallback = new ZCheckBoxColumnStyleInfo();
			noFallback.CharacterCasing = CharacterCasing.Upper;
			noFallback.CaptionResourceString = Res.GetData("AutoratingDateFilteringChargeGroupAndCustomizedUserControl|EC65462F-5B85-452B-A663-F2170B403D2E", "No Fallback");
			noFallback.ColumnName = "IsFallbackDisabled";
			ControlDpiScalingHelper.SetWidth(ref noFallback, 80, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(noFallback);
		}
	}
}
