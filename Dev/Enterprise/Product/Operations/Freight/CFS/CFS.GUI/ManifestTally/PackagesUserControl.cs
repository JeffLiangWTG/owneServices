using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Core.Forms;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class PackagesUserControl : ZUserControl
	{
		ZTextBox ImportLoadListNoTextBox;
		ZTextBox LoadingPortTextBox;
		ZTextBox JC_ContainerNumTextBox;
		ZTextBox DischargePortTextBox;
		ZDateEdit JC_LCLStorageCommencesDateEdit;
		ZDateEdit JC_LCLAvailableDateEdit;
		ZCodeFindBox VesselCodeFindBox;
		ZTextBox JC_UnpackGangTextBox;
		ZTextBox JX_VoyageTextBox;
		ZDateEdit JC_LCLUnpackDateEdit;
		CargoWise.Windows.UI.KPanel ShipmentPanel;
		ManifestTallyShipmentsGrid ShipmentsGrid;
		CargoWise.Windows.UI.KPanel PacklineTotalsPanel;
		ZCalcEdit LineCountTotalCalcEdit;
		CargoWise.Windows.UI.KPanel BottomPanel;
		CargoWise.Windows.UI.KSplitter PacklinesPackLocatiosnSplitter;
		CargoWise.Windows.UI.KPanel PackLinesPanel;
		CargoWise.Windows.UI.KPanel OutturnTotalsPanel;
		ZGrid PackLinesGrid;
		CargoWise.Windows.UI.KSplitter splitter1;
		ZPanel zPanel1;
		ZTemplateTabControl PackLineNotesTabControl;
		ZTabPage WarehouseTabPage;
		ZGrid JobPackLocGrid;
		ZTabPage OutturnNotesTabPage;
		ZTextBox JL_OutturnCommentTextBox;
		ZTextBox UnpackShedTextBox;
		ZTabPage MarksAndNumbersTabPage;
		ZTextBox MarksAndNumbersTextBox;
		MasterFiles.GUI.ZOrganisationFindBox zOrganisationFindBox1;
		ZDateEdit ReceiptDateEdit;
		ZPanel zPanel2;
		ZTextBox SealNumTextBox;
		ZCheckBox SealIntactCheckBox;
		ZCalcDropEdit ShipmentsTotalVolumeCalcDropEdit;
		ZCalcDropEdit ShipmentsTotalWeightCalcDropEdit;
		ZCalcDropEdit ShipmmentsTotalPacksCalcDropEdit;
		ZCalcDropEdit PacksTotalVolumeCalcDropEdit;
		ZCalcDropEdit PacksTotalWeightCalcDropEdit;
		ZCalcDropEdit PacksTotalCountCalcDropEdit;
		ZTextBox PCNNumTextBox;
		ZTextBox CCNNumTextBox;
		ZButton NilOutturnButton;
		ZTemplateTabControl ShipmentDetailsTabControl;
		ZTabPage PackLineTabPage;

		public PackagesUserControl()
		{
			InitializeComponent();

			if (GlbCompany.CurrentCompany.Country.Code != Core.Constants.CountryCodes.Canada)
			{
				this.CCNNumTextBox.Visible = false;
				this.PCNNumTextBox.Visible = false;

				ZGridColumnInfo column = this.ShipmentsGrid.GetColumnStyle("CanadaHouseCCN");
				this.ShipmentsGrid.ColumnStyles.Remove(column);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			ObjectFactory.Get<Enterprise.Integration.Customs.CA.IShipmentGridColumnsProvider>().AddColumns(this.ShipmentsGrid);
			ShipmentDetailsTabControl.PlugIns.AddCurrentDependentPlugIn(ControllerIDs.Customs.CA.RNSMFTallyShipmentsPlugIn, this.ShipmentsGrid);
		}

		#region Nil Outturn

		void NilOutturnButton_Click(object sender, EventArgs e)
		{
			TallyContainer container = CurrentDataItem as TallyContainer;
			if (container != null && (Globals.Message.Show(Res.GetString("15d60e01-b720-4ee6-8d01-fd0acd6dfd4d", "Do you wish to perform a Nil Outturn?"), Res.GetString("31d2b15a-f841-423b-a595-04c00e784515", "Nil Outturn"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes))
			{
				container.NilOutturn();
			}
		}

		#endregion
	}
}

