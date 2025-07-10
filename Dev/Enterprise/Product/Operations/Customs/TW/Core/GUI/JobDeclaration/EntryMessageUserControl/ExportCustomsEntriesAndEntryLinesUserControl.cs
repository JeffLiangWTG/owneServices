using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.GUI
{
	public partial class ExportCustomsEntriesAndEntryLinesUserControl : CustomsEntriesAndEntryLinesUserControl
	{
		public ExportCustomsEntriesAndEntryLinesUserControl()
		{
			InitializeComponent();
			InitializeGridLayoutCore();
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			using (EntryLineGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var procedureColumnInfo = EntryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Procedure);
				procedureColumnInfo.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("860c2320-aa73-4db3-be31-cd6b0b672cdf", "Mode of Statistics");
				procedureColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);

				var customsValueColumnInfo = EntryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_CustomsValue);
				customsValueColumnInfo.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("5D299D4D-02A8-4F55-8B02-2C8E6F78DBC1", "FOB Value");
				customsValueColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			}
		}
	}
}
