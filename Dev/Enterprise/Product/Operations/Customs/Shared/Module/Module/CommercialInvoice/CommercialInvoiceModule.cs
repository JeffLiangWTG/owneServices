using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	/// <summary>
	/// Module for CommercialInvoice.
	/// </summary>
	public class CommercialInvoiceModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CommercialInvoice;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.CommercialInvoice);
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, ImportXmlInvoices);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CommercialInvoiceFilterControl(GridCollection, (CommercialInvoiceFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CommercialInvoiceCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CommercialInvoiceFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsCommercialInvoice;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CommericalInvoiceWorkflowDescriptorCode;

		void ImportXmlInvoices(object sender, EventArgs args)
		{
			var importer = new InvoiceDataTransferImporter(StandAloneInvoiceValueObjectDataAdapter.New(), true);
			importer.PromptUserAndImport(BillingInterfaceName.CommercialInvoiceXmlImport);
		}

		public void PerformSearchForTesting()
		{
			base.PerformSearch();
		}

		public int ResultCountForTesting()
		{
			return GridCollection.Count;
		}

		public ZString FirstInvoiceNumberForTesting
		{
			get
			{
				if (GridCollection.Count > 0)
				{
					return ((BaseJobComInvoiceHeader)GridCollection[0]).JZ_InvoiceNumber;
				}

				return "";
			}
		}
	}
}
