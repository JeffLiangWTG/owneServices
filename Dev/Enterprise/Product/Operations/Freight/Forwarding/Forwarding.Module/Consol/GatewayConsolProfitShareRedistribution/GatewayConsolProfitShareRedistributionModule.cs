using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI.Consol.ProfitShareRedistribution.ExportToExcel;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public class GatewayConsolProfitShareRedistributionModule : TemplateRecordSupportedFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.GatewayConsolProfitShareRedistribution;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GatewayConsolProfitShareRedistribution;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override bool AllowAdvancedDataAutomationWizard => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) =>
			ZControllerFactory.Create(ControllerIDs.GatewayConsolProfitShareRedistribution);

		protected override IBusinessObjectCollection GetNewGridCollection() =>
			new ForwardingProfitShareRedistributionCollection(Factory);

		protected override IFilterControl GetNewFilterControl() =>
			new GatewayConsolProfitShareRedistributionFilterControl(GridCollection, FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() =>
			new GatewayConsolProfitShareRedistributionFilterBusinessObject();

		public override bool AllowEdit => false;
		public override bool AllowDelete => false;
		public override bool AllowNew => true;
		public override bool AllowView => true;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override void AddExtraImportExportMenuItems()
		{
			if (!exportToExcelItemAdded)
			{
				ExportMenuItems.Add(ResString.GetMultilingualString("191E2A3E-00A6-4DCB-85FA-7E2E4D30CB0C", "Export Selected Profit Share Redistribution To Excel"), new EventHandler(ExportProfitShareRedistributionToExcel_Click));
				exportToExcelItemAdded = true;
			}

			base.AddExtraImportExportMenuItems();
		}

		bool exportToExcelItemAdded { get; set; }

		void ExportProfitShareRedistributionToExcel_Click(object sender, EventArgs args)
		{
			if (DisplayGrid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowInformation(Res.GetString("31081BA0-8536-47E3-AC25-D095ED508492", "Please select a record to export."));
				return;
			}

			if (DisplayGrid.SelectedElements.Single() is ForwardingProfitShareRedistribution profitShareRedistribution)
			{
				var notifications = new ExcelExporterGuiNotifications(EmbeddedControl.FindForm());
				var exporter = new ProfitShareRedistributionExcelExporter(profitShareRedistribution, notifications);
				exporter.ExportToExcelAndOpen();
			}
		}
	}
}
