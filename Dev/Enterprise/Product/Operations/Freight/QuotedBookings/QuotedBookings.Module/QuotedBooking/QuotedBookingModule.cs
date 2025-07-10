using System;
using System.Collections.Generic;
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
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Freight.QuotedBookings.DataTransfer;
using Enterprise.Freight.QuotedBookings.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Freight;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.QuotedBookings.Module
{
	[UniversalCopyInstanceType(InstanceType = typeof(QuotedBooking))]
	public class QuotedBookingModule : QuotedBookingModuleBase, IOperationalActionSupportable
#if DEBUG
		, IBulkPostingModuleInternalsForTesting
#endif
	{
		public QuotedBookingModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, OnImportFromXml_Click);
		}

		public override ModuleIdentifier ID => ModuleIDs.QuotedBookings;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.QuotedBookings);

		protected override ZController GetNewController(QuotedBookingState state) => new QuotedBookingController(state);

		protected override IBusinessObjectCollection GetNewGridCollectionCore() =>
			new ViewQuotedBookingCollection(Factory) { AllowTemplateRecords = AllowLoadTemplateRecords };

		protected override void AddNewActionMenuItemsCore(List<MenuItem> menu)
		{
			menu.Add(new ZMenuItem(ResString.GetMultilingualString("1caddb4e-7e9b-4e90-9a68-aa26e4734bd4", "Merge Into Selected Booking"), HandleMergeIntoSelectedBookingAction));

			KMenuItem postMenuItem = BulkPostingHelper.GetPostMenuItem(new PostTransactionsDelegate(PostTransactions)) as KMenuItem;
			postMenuItem.MenuItems.Add(BulkJobProfitPrintingHelper.GetMenuItem(PrintJobProfitDocument) as MenuItem);
			menu.Add(postMenuItem);
		}

		void HandleMergeIntoSelectedBookingAction(object sender, EventArgs e)
		{
			if (Grid.ListManager.Position < 0)
			{
				Globals.Message.Show(Res.GetString("aad73e1f-5b42-4412-867f-66878fb7db52", "Please select a booking to merge into"));
			}
			else
			{
				var viewQuotedBooking = (ViewQuotedBooking)Grid.ListManager.GetCurrent();

				if (viewQuotedBooking != null && viewQuotedBooking.QuotedBooking != null)
				{
					CombineBookingsForm.Show(viewQuotedBooking.QuotedBooking);
				}
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new QuotedBookingFilterStripBusinessObject(AllowLoadTemplateRecords);

		QuotedBooking[] GetQuickBookings()
		{
			return Grid.SelectedElements.Cast<ViewQuotedBooking>().Where(x => x.QuotedBooking.Quote == null).Select(x => x.QuotedBooking).ToArray();
		}

		protected override Type SendEmailInstanceType => typeof(QuotedBooking);

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
				QuotedBooking[] quickBookings = GetQuickBookings();
				if (Grid.SelectedElements.Length > 0 && quickBookings.Length == 0)
				{
					Globals.Message.ShowError(Res.GetString("4B233952-11D1-408F-AB2D-6DB7C3A31DEA", "Posting action can only be run for Quick Booking."));
					return;
				}

				BulkPostingHelper.PostTransactions(postingOption, quickBookings);
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
			BulkJobProfitPrintingHelper.PrintJobProfitDocument(Factory, GetQuickBookings());
		}

		IBulkJobProfitPrintingModuleHelper BulkJobProfitPrintingHelper => fBulkJobProfitPrintingHelper ?? (fBulkJobProfitPrintingHelper = ObjectFactory.Get<IBulkJobProfitPrintingModuleHelper>());

		IBulkJobProfitPrintingModuleHelper fBulkJobProfitPrintingHelper;

		#endregion

		#region Template record

		protected override bool SupportTemplateRecords => true;

		#endregion

		#region ToolBarButtons

		protected override void AddNewStandardMenuItemsCore()
		{
			if (NewMenuItem != null && Equals(ID, ModuleIDs.QuotedBookings))
			{
				var bookingWithQuoteMenuItem = new ZMenuItem(BookingNewButtonLabelList.Descriptions.BookingWithQuote, (_, __) => HandleShowingFormSafely(HandleQuotedBooking));
				var quickBookingMenuItem = new ZMenuItem(BookingNewButtonLabelList.Descriptions.QuickBooking, (_, __) => HandleShowingFormSafely(HandleBookingOnly));
				var preAllocationsMenuItem = new ZMenuItem(BookingNewButtonLabelList.Descriptions.PreAllocations, (_, __) => HandleShowingFormSafely(HandlePreAllocations));

				bookingWithQuoteMenuItem.Name = BookingNewButtonLabelList.Codes.BookingWithQuote;
				quickBookingMenuItem.Name = BookingNewButtonLabelList.Codes.QuickBooking;
				preAllocationsMenuItem.Name = BookingNewButtonLabelList.Codes.PreAllocations;

				NewMenuItem.MenuItems.Add(bookingWithQuoteMenuItem);
				NewMenuItem.MenuItems.Add(quickBookingMenuItem);
				NewMenuItem.MenuItems.Add(preAllocationsMenuItem);

				NewMenuItem.MenuItems[FreightDataRegistry.Instance.DefaultBookingNewButton.Value].Text += " " + Res.GetString("72ed3294-5bba-44a1-abf6-727ecb28b7ae", "(Default)");
				NewMenuItem.MenuItems[FreightDataRegistry.Instance.DefaultBookingNewButton.Value].DefaultItem = true;
			}
		}

		#endregion

		#region Events

		void HandleQuotedBooking()
		{
			GetNewController(QuotedBookingState.AcceptedBookingWithQuote).ShowNewForm();
		}

		void HandleBookingOnly()
		{
			GetNewController(QuotedBookingState.BookingOnly).ShowNewForm();
		}

		void HandlePreAllocations()
		{
			new PreAllocationController().ShowNewForm();
		}

		#endregion

		#region Security / Licence

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.QuickBooking;

		#endregion

		#region ImportFromXML

		void OnImportFromXml_Click(object sender, EventArgs e)
		{
			NewXmlDataTransferDirector(new QuotedBookingValueObjectDataAdapter()).PromptUserAndImport(BillingInterfaceName.QuotedBookingXmlImport); // Interface name for billing purposes
		}

#if DEBUG
		protected virtual // overridden in tests
#endif
		XmlDataTransferDirector NewXmlDataTransferDirector(QuotedBookingValueObjectDataAdapter adapter) => new XmlDataTransferDirector(adapter, true);

		#endregion

		#region IOperationalActionSupportable Members

		public OperationalActionSupporter OperationalActionSupporter => Equals(ID, ModuleIDs.QuotedBookings) ? new QuotedBookingSupporter() : null;

		#endregion

		#region BusinessContexts

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.QuotedBooking };

		#endregion
	}
}
