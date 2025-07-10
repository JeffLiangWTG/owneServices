using System.Windows.Forms;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class ImportMessageUserControl : MessageUserControl
	{
		public ImportMessageUserControl()
		{
			InitializeComponent();
			RequiresMergeLabel.AllowOverlap(MainHorizontalSplitContainer);
			SetupEntryFeesTab();
		}

		protected override EntryLineAdditionalDataUserControl GetEntryLineAdditionalData() => new ImportEntryLineAdditionalDataUserControl();

		void SetupEntryFeesTab()
		{
			ZGrid entryFeeGrid = new ZGrid();
			entryFeeGrid.Name = "EntryFeesGrid";
			entryFeeGrid.BindTo = "CustomsEntryHeaders.Charges";
			entryFeeGrid.CaptionText = Res.GetString("c0ce7e9b-1574-4630-b17d-5945c32385c4", "Entry Fees");
			entryFeeGrid.GridId = "83eae562-a685-4d31-b891-f3eb24b1fee3";

			ZPanel entryFeePanel = new ZPanel();
			entryFeePanel.Dock = DockStyle.Fill;
			EntryFeesTabPage.Controls.Add(entryFeePanel);

			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo1.Caption = Res.GetString("4c06c4ba-df35-44ce-b66c-aab741edbc6a", "Fee Code");
			zDropEditColumnStyleInfo1.ColumnName = "C1_ChargeType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);

			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo1.Caption = Res.GetString("332aefc5-1471-47f0-8e67-9f77f9755e64", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "DescriptionOfChargeType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);

			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo2.Caption = Res.GetString("4d1475c6-0679-4af8-a1e8-4714b31dfd9e", "Amount");
			zTextBoxColumnStyleInfo2.ColumnName = "C1_ChargeAmount";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);

			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo2.Caption = Res.GetString("bde947c1-ee26-4ef3-8deb-deecd8b1a581", "Method of Payment");
			zDropEditColumnStyleInfo2.ColumnName = "C1_MethodOfPayment";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo3.Caption = Res.GetString("bad6ed29-9cae-4804-a88e-526199bde736", "Action");
			zDropEditColumnStyleInfo3.ColumnName = "C1_RateOverrideReasonCode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);

			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo3.Caption = Res.GetString("6a4f12d6-e911-4808-81ce-39e4e83cdaef", "Method of Calculation");
			zTextBoxColumnStyleInfo3.ColumnName = "C1_Source";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			entryFeeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			entryFeeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			entryFeeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			entryFeeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			entryFeeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			entryFeeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);

			entryFeeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			entryFeeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			entryFeeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 109, true);

			entryFeePanel.Controls.Add(entryFeeGrid);
		}
	}
}
