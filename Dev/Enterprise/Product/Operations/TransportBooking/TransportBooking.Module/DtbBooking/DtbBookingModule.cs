using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Module;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingModule : DtbTransportModule, IOperationalActionSupportable, IImportCollectionInfoProvider
#if DEBUG
, IBulkPostingModuleInternalsForTesting
#endif
	{
		public DtbBookingModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DtbBooking; }
		}

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[] { BusinessContext.DtbBooking }; }
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetupFactory(Factory);
			return new DtbBookingCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new DtbBookingFilterControl((IDtbBookingCollection)GridCollection, (DtbBookingFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DtbBookingFilterBusinessObject();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.DtbBooking);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.TransportBookings; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			var postMenuItem = BulkPostingHelper.GetPostMenuItem(new PostTransactionsDelegate(PostTransactions), JobInvoicingPostingOption.Costs) as MenuItem;
			result.Add(postMenuItem);

			return result.ToArray();
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			MenuItem[] result = base.GetNewStandardMenuItems();

			if (NewMenuItem != null)
			{
				newBookingMenuItem = new ZMenuItem(NewbookingMenuItemText, HandleNewBooking);
				quotedBookingMenuItem = new ZMenuItem(QuotedbookingMenuItemText, HandleQuotedBooking);
				pickupMasterBookingMenuItem = new ZMenuItem(PickupMasterBookingMenuItemText, HandlePickupMasterBooking);
				deliveryMasterBookingMenuItem = new ZMenuItem(DeliveryMasterBookingMenuItemText, HandleDeliveryMasterBooking);

				newBookingMenuItem.Text += " " + MenuItemDefaultText;
				newBookingMenuItem.DefaultItem = true;

				NewMenuItem.MenuItems.Add(newBookingMenuItem);
				NewMenuItem.MenuItems.Add(quotedBookingMenuItem);

				if (TransportRegistry.Instance.MasterBookingsEnabled.Value)
				{
					NewMenuItem.MenuItems.Add(pickupMasterBookingMenuItem);
					NewMenuItem.MenuItems.Add(deliveryMasterBookingMenuItem);
				}
			}

			return result;
		}
		ZMenuItem newBookingMenuItem;
		ZMenuItem quotedBookingMenuItem;
		ZMenuItem pickupMasterBookingMenuItem;
		ZMenuItem deliveryMasterBookingMenuItem;

		public static MultilingualString NewbookingMenuItemText => ResString.GetMultilingualString("MenuItem.TransportBooking.Newbooking", "Booking");

		public static MultilingualString QuotedbookingMenuItemText => ResString.GetMultilingualString("MenuItem.TransportBooking.NewQuotedbooking", "Quote");

		public static MultilingualString PickupMasterBookingMenuItemText => ResString.GetMultilingualString("MenuItem.TransportBooking.NewPickupMasterBooking", "Pickup Master");

		public static MultilingualString DeliveryMasterBookingMenuItemText => ResString.GetMultilingualString("MenuItem.TransportBooking.NewDeliveryMasterBooking", "Delivery Master");

		public static MultilingualString MenuItemDefaultText => ResString.GetMultilingualString("MenuItem.TransportBooking.Default", "(Default)");

		enum BookingType { Booking, Quote, PickupMasterBooking, DeliveryMasterBooking }

		void HandleNewBooking(object sender, EventArgs e)
		{
			CreateAndShowNewBooking(BookingType.Booking);
		}

		void HandleQuotedBooking(object sender, EventArgs e)
		{
			CreateAndShowNewBooking(BookingType.Quote);
		}

		void HandlePickupMasterBooking(object sender, EventArgs e)
		{
			CreateAndShowNewBooking(BookingType.PickupMasterBooking);
		}

		void HandleDeliveryMasterBooking(object sender, EventArgs e)
		{
			CreateAndShowNewBooking(BookingType.DeliveryMasterBooking);
		}

		void CreateAndShowNewBooking(BookingType bookingType)
		{
			var controller = (DtbBookingController)GetNewController(null);
			switch (bookingType)
			{
				case BookingType.Quote:
					controller.MakeNewBookingsAsQuotes();
					break;

				case BookingType.PickupMasterBooking:
					controller.MakeNewBookingsAsMasterBookings(nameof(DtbBookingDirection.PIC));
					break;

				case BookingType.DeliveryMasterBooking:
					controller.MakeNewBookingsAsMasterBookings(nameof(DtbBookingDirection.DLV));
					break;

				default:
					break;
			}

			controller.ShowNewForm();
		}

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

#if DEBUG
		JobInvoicingPostingOption IBulkPostingModuleInternalsForTesting.LastUsedPostingOptionForTest
		{
			get { return lastUsedPostingOptionForTest; }
		}

		JobInvoicingPostingOption lastUsedPostingOptionForTest;

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

		bool IBulkPostingModuleInternalsForTesting.DontDoActualPosting
		{
			get;
			set;
		}

#endif

		protected override BusinessObjectFactory GetNewFactory()
		{
			var factory = base.GetNewFactory();
			SetupFactory(factory);
			return factory;
		}

		void SetupFactory(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				DtbChildEditableService.SetState(factory, DtbChildEditableServiceState.Transport);
				DtbFormStateService.SetState(factory, DtbFormState.Booking);
			}
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DtbBooking; }
		}

		protected override void Dispose(bool isDisposing)
		{
			if (newBookingMenuItem != null)
			{
				newBookingMenuItem.Dispose();
			}

			if (quotedBookingMenuItem != null)
			{
				quotedBookingMenuItem.Dispose();
			}

			base.Dispose(isDisposing);
		}

		// interfaces

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new DtbBookingOperationalActionSupporter(); }
		}

		protected override DataTransferProcessor GetDataImportWizardProcessor(IImportCollectionInfo collectionInfo)
		{
			return new DtbBookingFlattenedDataTransferProcessor((ImportCollectionInfoImplForDtbBookingFlattened)collectionInfo);
		}

		string IImportCollectionInfoProvider.ContextKey
		{
			get { return "DtbBookingModuleImportWizard"; }
		}

		IImportCollectionInfo IImportCollectionInfoProvider.ImportCollectionInfo
		{
			get { return new ImportCollectionInfoImplForDtbBookingFlattened(new DtbBookingFlattenedCollection(Factory)); }
		}
	}
}
