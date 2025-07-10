using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "IDisposable only used for integration testing convenience. Has no application here.")]
	public class LoadListConsolModule : ZFilterGridModule, IBulkPostingModuleInternalsForTesting
	{
		public override ModuleIdentifier ID => ModuleIDs.LoadListConsol;

		string NameOfSingleObject => (NoResString)"Load List";
		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.CFSLoadList };

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.LoadListConsol);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new LoadListConsolFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetGuiProviders(Factory);
			return new CFSLoadListConsolCollection(Factory);
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			var factory = base.GetNewFactory();
			SetGuiProviders(factory);
			return factory;
		}

		void SetGuiProviders(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				ConsolDocumentSupporterGuiQueryProvider.Register(factory);
				ShipmentDocumentSupporterGuiQueryProvider.Register(factory);
				ServicesSelectionGuiProvider.Register(factory);
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new LoadListConsolFilterBusinessObject();
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, new EventHandler(OnImportFromXml_Click));
			AddInterfaceConnectorExportMenuItem(CommonDataTransferCaptions.ToXmlMenuText, new EventHandler(OnExportToXml_Click));
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewStandardMenuItems());
			result.Remove(CopyMenuItem);
			return result.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());

			KMenuItem postMenuItem = BulkPostingHelper.GetPostMenuItem(new PostTransactionsDelegate(PostTransactions)) as KMenuItem;
			postMenuItem.MenuItems.Add(BulkJobProfitPrintingHelper.GetMenuItem(PrintJobProfitDocument) as MenuItem);
			result.Add(postMenuItem);

			return result.ToArray();
		}

		#region Posting

		void PostTransactions(JobInvoicingPostingOption postingOption)
		{
#if DEBUG
			if (((IBulkPostingModuleInternalsForTesting)this).DontDoActualPosting)
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
					fBulkPostingHelper.Initialize(NameOfSingleObject, false);
				}

				return fBulkPostingHelper;
			}
		}
		IBulkPostingModuleHelper fBulkPostingHelper;

		#region IBulkPostingModuleInternalsForTesting Members
#if DEBUG

		JobInvoicingPostingOption IBulkPostingModuleInternalsForTesting.LastUsedPostingOptionForTest => fLastUsedPostingOptionForTest;

		JobInvoicingPostingOption fLastUsedPostingOptionForTest;

		IMenuItem IBulkPostingModuleInternalsForTesting.PostMenuItem
		{
			get
			{
				MenuItem menuItem = GetNewActionMenuItems().FindByText("&Post");
				IMenuItem iMenuItem = menuItem as IMenuItem;
				if (menuItem != null && iMenuItem == null)
				{
					throw new InvalidOperationException("Post menu item should have type ZMenuItem");
				}
				return iMenuItem;
			}
		}

		bool IBulkPostingModuleInternalsForTesting.DontDoActualPosting
		{
			get;
			set;
		}
#endif
		#endregion

		#endregion

		#region PrintJobProfitDocument

		void PrintJobProfitDocument()
		{
			BulkJobProfitPrintingHelper.PrintJobProfitDocument(Factory, Grid.SelectedElements);
		}

		IBulkJobProfitPrintingModuleHelper BulkJobProfitPrintingHelper => fBulkJobProfitPrintingHelper ?? (fBulkJobProfitPrintingHelper = ObjectFactory.Get<IBulkJobProfitPrintingModuleHelper>());

		IBulkJobProfitPrintingModuleHelper fBulkJobProfitPrintingHelper;

		#endregion

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CFSLoadList;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.CFSManager;

		protected override SortInfo DefaultSortOrder => new SortInfo(CFSLoadListConsol.Schema.JK_UniqueConsignRef, ListSortDirection.Descending);

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => JobInvoicingConsumerTypes.CFSLoadList.Code;

		void OnImportFromXml_Click(object sender, EventArgs e)
		{
			new XmlDataTransferDirector(new LoadListValueObjectDataAdapter(), true).PromptUserAndImport(BillingInterfaceName.LoadListConsolXmlImport);
		}

		void OnExportToXml_Click(object sender, EventArgs e)
		{
			new XmlDataTransferExporter(new LoadListValueObjectDataAdapter(), true).PromptUserAndExport(this.GridCollection.ToArray());
		}
	}
}
