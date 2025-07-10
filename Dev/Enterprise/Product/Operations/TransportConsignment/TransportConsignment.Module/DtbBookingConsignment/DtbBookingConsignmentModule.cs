using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportCommon.Module;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbBookingConsignmentModule : DtbTransportModule, IOperationalActionSupportable
#if DEBUG
, IBulkPostingModuleInternalsForTesting
#endif
	{
		public DtbBookingConsignmentModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			result.Add(new ZMenuItem("-"));
			var postMenuItem = BulkPostingHelper.GetPostMenuItem(new PostTransactionsDelegate(PostTransactions)) as KMenuItem;
			postMenuItem.MenuItems.Add(new ZMenuItem("-"));
			postMenuItem.MenuItems.Add(BulkJobProfitPrintingHelper.GetMenuItem(PrintJobProfitDocument) as MenuItem);
			result.Add(postMenuItem);

			return result.ToArray();
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.DtbBookingConsignmentWorkflowDescriptorCode; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.DtbBookingConsignment);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DtbBookingConsignmentFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new DtbBookingConsignmentFilterControl((DtbBookingConsignmentCollection)GridCollection, (DtbBookingConsignmentFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new DtbBookingConsignmentCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DtbBookingConsignment; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.LandTransport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DtbBookingConsignment; }
		}

		#region Post

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
				var workFactory = new BusinessObjectFactory { NameForDebugging = "DtbConignmentBulkPostingModuleHelper" };
				var pks = new List<ZGuid>(Grid.SelectedElements.Select(element => element.PK));
				var jobs = workFactory.Load<DtbBookingConsignment>(new ZQuery(DtbBookingSchema.PK, pks));
				BulkPostingHelper.PostTransactions(postingOption, jobs);
				GCWrapper.ReclaimMemory(ref workFactory);
			}
		}

		IBulkPostingModuleHelper BulkPostingHelper
		{
			get
			{
				if (bulkPostingHelper == null)
				{
					bulkPostingHelper = ObjectFactory.Get<IBulkPostingModuleHelper>();
					bulkPostingHelper.Initialize(Description, false);
				}

				return bulkPostingHelper;
			}
		}
		IBulkPostingModuleHelper bulkPostingHelper;

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
				var menuItem = GetNewActionMenuItems().FindByText("&Post");
				var iMenuItem = menuItem as IMenuItem;
				if (menuItem != null && iMenuItem == null)
				{
					throw new InvalidOperationException("Post menu item should have type ZMenuItem");
				}

				return iMenuItem;
			}
		}

		bool IBulkPostingModuleInternalsForTesting.DontDoActualPosting { get; set; }
#endif

		#endregion

		#endregion

		#region Printing

		void PrintJobProfitDocument()
		{
			BulkJobProfitPrintingHelper.PrintJobProfitDocument(Factory, Grid.SelectedElements);
		}

		IBulkJobProfitPrintingModuleHelper BulkJobProfitPrintingHelper
		{
			get { return bulkJobProfitPrintingHelper ?? (bulkJobProfitPrintingHelper = ObjectFactory.Get<IBulkJobProfitPrintingModuleHelper>()); }
		}
		IBulkJobProfitPrintingModuleHelper bulkJobProfitPrintingHelper;

		#endregion

		#region IOperationalActionSupportable Members

		public OperationalActionSupporter OperationalActionSupporter
		{
			get { return new DtbBookingConsignmentOperationalActionSupporter(); }
		}

		#endregion
	}
}
