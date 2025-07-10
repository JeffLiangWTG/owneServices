using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.DataTransfer;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Module
{
	public partial class BillOfLadingModule : ZFilterGridModule, IOperationalActionSupportable, IBulkSendUniversalDataSupportable
	{
		public BillOfLadingModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override void OnSetupAndGetGrid(ZDisplayGrid grid)
		{
			base.OnSetupAndGetGrid(grid);

			if (previousGrid?.ContextMenu != null)
			{
				previousGrid.ContextMenu.Popup -= ContextMenu_Popup;
			}

			if (grid.ContextMenu != null)
			{
				grid.ContextMenu.Popup += ContextMenu_Popup;
			}

			previousGrid = grid;
		}

		ZDisplayGrid previousGrid;

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			var universalCopyMenuItem = DisplayGrid.ContextMenu.MenuItems.FindByName("UniversalCopy");
			if (universalCopyMenuItem != null)
			{
				universalCopyMenuItem.Visible = AllowUniversalCopy;
				universalCopyMenuItem.Enabled = AllowUniversalCopy;
			}
		}

		void AddImportMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, delegate
			{
				var director = new XmlDataTransferDirector(new AgencyShipmentValueObjectDataAdapter<BillOfLading>(), true);
				director.PromptUserAndImport(BillingInterfaceName.AgencyBillOfLadingXmlImport); // Interface name for billing purposes
			});

			if (GlbBranch.CurrentBranch.Country != null && GlbBranch.CurrentBranch.Country.Code == Constants.CountryCodes.Australia)
			{
				var importer = ObjectFactory.Get<Enterprise.Integration.Customs.AU.IShipnetToBillOfLadingImporter>() as DataImporter;

				if (importer != null)
				{
					AddInterfaceConnectorCSVImportMenuItem(CommonDataTransferCaptions.FromCsvMenuText, delegate
					{
						using (var form = DataImporterForm.Create(BillingInterfaceName.AgencyBillOfLadingCsvImport)) // Interface name for billing purposes
						{
							form.Importer = importer;
							ZFormModaliser.ShowDialogWithoutDispose(form);
						}
					});
				}
			}
		}

		void AddExportMenuItems()
		{
			AddInterfaceConnectorExportMenuItem((NoResString)"To XML", delegate // Menu item name
			{
				var exportor = new XmlDataTransferExporter(new AgencyShipmentValueObjectDataAdapter<BillOfLading>(), true);

				var businessObjects = (GetSelectedBusinessObjects().Length > 0)
					? GetSelectedBusinessObjects()
					: GridCollection.ToArray();

				exportor.PromptUserAndExport(businessObjects);
				Factory.Save();
			});
		}

		public override BusinessObject[] GetSelectedBusinessObjects()
		{
			return base.GetSelectedBusinessObjects()
				.Cast<BillOfLading>()
				.Where((bol) => bol.IsBillOfLadingStage)
				.ToArray();
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddImportMenuItems();
			AddExportMenuItems();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("-"));

			var postMenuItem = BulkPostingHelper.GetPostMenuItem(PostTransactions) as MenuItem;
			postMenuItem.MenuItems.Add(BulkJobProfitPrintingHelper.GetMenuItem(PrintJobProfitDocument) as MenuItem);
			result.Add(postMenuItem);

			Func<IDtbBookingParent> selectedObjectGetter = () => (IDtbBookingParent)CurrentBusinessObjectInGrid;

			var dtbBookingProvider = ObjectFactory.New<IDtbBookingMenuProvider>(selectedObjectGetter);
			result.Add((MenuItem)dtbBookingProvider.ConstructMenu());

			return result.ToArray();
		}

		protected override MenuItem[] GetUniversalDataTransferMenuItems()
		{
			var menuItem = UniversalDataMenuBuilder.BuildUniversalDataReceipientsMenu(this);
			return menuItem != null ? new[] { menuItem } : null;
		}

		public override ModuleIdentifier ID => ModuleIDs.AgencyBillOfLading;

		public override BusinessContext[] BusinessContexts => new[] { BusinessContext.AgencyDocumentation };

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ShippingManagerBillOfLading;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.AgencyBillOfLading;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AgencyBillOfLading);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new BillOfLadingFilterControl(GridCollection, (BillOfLadingFilterStrip)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new BillOfLadingCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new BillOfLadingFilterStrip();
		}

		#region Posting

		void PostTransactions(JobInvoicingPostingOption postingOption)
		{
#if DEBUG
			if (((IBulkPostingModuleInternalsForTesting)this).DontDoActualPosting)
			{
				lastUsedPostingOptionForTest = postingOption;
			}
			else
#endif
			{
				BulkPostingHelper.PostTransactions(postingOption, GetSelectedBusinessObjects());
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

		#endregion

		#region PrintJobProfitDocument

		void PrintJobProfitDocument()
		{
			BulkJobProfitPrintingHelper.PrintJobProfitDocument(Factory, GetSelectedBusinessObjects());
		}

		IBulkJobProfitPrintingModuleHelper BulkJobProfitPrintingHelper => fBulkJobProfitPrintingHelper ?? (fBulkJobProfitPrintingHelper = ObjectFactory.Get<IBulkJobProfitPrintingModuleHelper>());

		IBulkJobProfitPrintingModuleHelper fBulkJobProfitPrintingHelper;

		#endregion

		#region IOperationalActionSupportable Members

		public OperationalActionSupporter OperationalActionSupporter => operationalActionSupporter ?? (operationalActionSupporter = new BillOfLadingActionSupporter());

		OperationalActionSupporter operationalActionSupporter;

		#endregion

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode;

		#region IUniversalDataSupportable Members

		IEnumerable<IWorkflowProvider> IBulkSendUniversalDataSupportable.GetElementsToSend()
		{
			return GetSelectedBusinessObjects().Cast<IWorkflowProvider>();
		}

		string IBulkSendUniversalDataSupportable.NameOfSingleObject => Res.GetString("1b4df901-38b3-4b4e-9643-d1b6de22b2af", "Bill Of Lading");

		Type IBulkSendUniversalDataSupportable.TypeOfSingleObject => GridCollection.TypeOfElements;

		string IBulkSendUniversalDataSupportable.WorkflowDescriptorCode => WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode;

		#endregion

		public override bool CouldAllowUniversalCopy
		{
			get
			{
				return true;
			}
		}

		public override bool AllowUniversalCopy
		{
			get
			{
				if (!Globals.IsUserInteractive)
				{
					return true;
				}

				return base.GetSelectedBusinessObjects()
					.Cast<BillOfLading>()
					.All(bol => bol.IsBillOfLadingStage);
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			if (IsEmbeddedControlConstructed)
			{
				if (isDisposing && Globals.IsUserInteractive && DisplayGrid?.ContextMenu != null)
				{
					DisplayGrid.ContextMenu.Popup -= ContextMenu_Popup;
				}
			}

			base.Dispose(isDisposing);
		}
	}
}

#region TestCase
#if DEBUG

#region dodgy arse accounting crap

namespace Enterprise.Freight.Agency.Module
{
	partial class BillOfLadingModule : IBulkPostingModuleInternalsForTesting
	{
		#region IBulkPostingModuleInternalsForTesting Members

		JobInvoicingPostingOption IBulkPostingModuleInternalsForTesting.LastUsedPostingOptionForTest => lastUsedPostingOptionForTest;

		JobInvoicingPostingOption lastUsedPostingOptionForTest;

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		IMenuItem IBulkPostingModuleInternalsForTesting.PostMenuItem
		{
			get
			{
				var menuItem = GetNewActionMenuItems().FindByText("&Post");
				var iMenuItem = menuItem as IMenuItem;
				if (menuItem != null && iMenuItem == null)
				{
					throw new ApplicationException("Post menu item should have type ZMenuItem");
				}
				return iMenuItem;
			}
		}

		bool IBulkPostingModuleInternalsForTesting.DontDoActualPosting
		{
			get;
			set;
		}

		#endregion
	}
}

#endregion



#endif
#endregion
