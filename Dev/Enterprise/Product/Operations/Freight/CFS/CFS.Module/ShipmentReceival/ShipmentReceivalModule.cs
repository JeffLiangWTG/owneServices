using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
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
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Module
{
	/// <summary>
	/// Module for ShipmentReceival.
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "IDisposable only used for integration testing convenience. Has no application here.")]
	public class ShipmentReceivalModule : ZFilterGridModule, IBulkPostingModuleInternalsForTesting
	{
		public ShipmentReceivalModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ShipmentReceival; }
		}

		string NameOfSingleObject
		{
			get { return (NoResString)"Shipment"; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[] { BusinessContext.CFSShipmentReceival }; }
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

		JobInvoicingPostingOption IBulkPostingModuleInternalsForTesting.LastUsedPostingOptionForTest
		{
			get { return fLastUsedPostingOptionForTest; }
		}

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

		IBulkJobProfitPrintingModuleHelper BulkJobProfitPrintingHelper
		{
			get
			{
				if (fBulkJobProfitPrintingHelper == null)
				{
					fBulkJobProfitPrintingHelper = ObjectFactory.Get<IBulkJobProfitPrintingModuleHelper>();
				}

				return fBulkJobProfitPrintingHelper;
			}
		}
		IBulkJobProfitPrintingModuleHelper fBulkJobProfitPrintingHelper;

		#endregion

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.CFSManager; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CFSShipment; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ShipmentReceival);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ShipmentReceivalFilterControl(GridCollection, (ShipmentReceivalFilterStrip)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetGuiProviders(Factory);
			return new CFSShipmentList(Factory);
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
				ShipmentDocumentSupporterGuiQueryProvider.Register(factory);
				ServicesSelectionGuiProvider.Register(factory);
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ShipmentReceivalFilterStrip();
		}

		protected override SortInfo DefaultSortOrder
		{
			get { return new SortInfo(JobShipmentSchema.Constants.JS_UniqueConsignRef, ListSortDirection.Descending); }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return JobInvoicingConsumerTypes.CFSShipment.Code; }
		}
	}
}
