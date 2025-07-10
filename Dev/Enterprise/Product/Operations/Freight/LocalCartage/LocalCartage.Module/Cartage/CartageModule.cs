using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.DataTransfer;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public CartageModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected void ImportCartage_Click(object sender, EventArgs e)
		{
			new XmlDataTransferDirector(new CommonCartageBookingValueObjectDataAdapter(), true).PromptUserAndImport(BillingInterfaceName.CartageXmlImport);
		}

		public override ModuleIdentifier ID => ModuleIDs.Cartage;

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.Cartage };

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Cartage);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CartageFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ModuleCartageCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CartageFilterBusinessObject();
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, new EventHandler(ImportCartage_Click));
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			KMenuItem postMenuItem = BulkPostingHelper.GetPostMenuItem(PostTransactions) as KMenuItem;
			result.Add(postMenuItem);

			return result.ToArray();
		}

		void PostTransactions(JobInvoicingPostingOption postingOption)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				fLastUsedPostingOptionForTest = postingOption;
			}
			else
#endif
			{
				BulkPostingHelper.PostTransactions(postingOption, Grid.SelectedElements);
			}
		}

		IBulkPostingModuleHelper BulkPostingHelper
		{
			get
			{
				if (fBulkPostingHelper == null)
				{
					fBulkPostingHelper = ObjectFactory.Get<IBulkPostingModuleHelper>();
					fBulkPostingHelper.Initialize(Description, false);
				}

				return fBulkPostingHelper;
			}
		}
		IBulkPostingModuleHelper fBulkPostingHelper;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.LocalTransport;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.TransportJob;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => JobInvoicingConsumerTypes.LocalCartage.Code;

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new CartageOperationalActionSupporter();

		internal JobInvoicingPostingOption fLastUsedPostingOptionForTest;
	}
}
