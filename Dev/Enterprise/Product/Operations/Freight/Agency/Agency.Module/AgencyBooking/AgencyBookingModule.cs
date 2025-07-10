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
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.GUI;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module
{
	[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public partial class BookingModule : ZFilterGridModule, IOperationalActionSupportable, IBulkSendUniversalDataSupportable
	{
		public BookingModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AgencyBooking; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode; }
		}

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[1] { BusinessContext.AgencyBooking }; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ShippingManagerBookings; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AgencyBooking; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());

			result.Add(new ZMenuItem(ResString.GetMultilingualString("e0b5efed-4200-4920-a14d-181de95630ca", "Merge Into Selected Booking"), delegate
			{
				if (Grid.ListManager.Position < 0)
				{
					Globals.Message.Show(Res.GetString("d200780e-702d-4be3-bbef-ad284540d4a4", "Please select a booking to merge into"));
				}
				else
				{
					AgencyBooking booking = (AgencyBooking)Grid.ListManager.GetCurrent();
					CombineBookingsForm.Show(booking);
				}
			}));

			result.Add(new ZMenuItem(ResString.GetMultilingualString("4c74ad49-dae7-48ba-910e-1daff01824ec", "Split Booking"), delegate
			{
				if (Grid.ListManager.Position < 0)
				{
					Globals.Message.Show(Res.GetString("135b2634-576f-4e3d-9384-eef4672051fc", "Please select a booking to split"));
				}
				else
				{
					AgencyBooking booking = (AgencyBooking)Grid.ListManager.GetCurrent();
					SplitBookingsForm.Show(booking);
				}
			}));

			KMenuItem postMenuItem = BulkPostingHelper.GetPostMenuItem(new PostTransactionsDelegate(PostTransactions)) as KMenuItem;
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

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AgencyBooking);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AgencyBookingFilterControl(GridCollection, (AgencyBookingFilterStrip)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetGuiProviders(Factory);
			return new AgencyBookingCollection(Factory);
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
			return new AgencyBookingFilterStrip();
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

		#region IOperationalActionSupportable Members

		public OperationalActionSupporter OperationalActionSupporter
		{
			get { return new AgencyBookingActionSupporter(); }
		}

		#endregion

		#region IUniversalDataSupportable Members

		IEnumerable<IWorkflowProvider> IBulkSendUniversalDataSupportable.GetElementsToSend()
		{
			return GetSelectedBusinessObjects().Cast<IWorkflowProvider>();
		}

		string IBulkSendUniversalDataSupportable.NameOfSingleObject
		{
			get { return Res.GetString("3c54970a-ef48-4ef2-af2b-b4f8e0f20bc0", "Booking"); }
		}

		Type IBulkSendUniversalDataSupportable.TypeOfSingleObject
		{
			get { return GridCollection.TypeOfElements; }
		}

		string IBulkSendUniversalDataSupportable.WorkflowDescriptorCode
		{
			get { return WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode; }
		}

		#endregion
	}
}

#region Test
#if DEBUG

#region dodgy arse accounting crap

namespace Enterprise.Freight.Agency.Module
{
	partial class BookingModule : IBulkPostingModuleInternalsForTesting
	{
		#region IBulkPostingModuleInternalsForTesting Members

		JobInvoicingPostingOption IBulkPostingModuleInternalsForTesting.LastUsedPostingOptionForTest
		{
			get { return lastUsedPostingOptionForTest; }
		}

		JobInvoicingPostingOption lastUsedPostingOptionForTest;

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		IMenuItem IBulkPostingModuleInternalsForTesting.PostMenuItem
		{
			get
			{
				MenuItem menuItem = GetNewActionMenuItems().FindByText("&Post");
				IMenuItem iMenuItem = menuItem as IMenuItem;
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
