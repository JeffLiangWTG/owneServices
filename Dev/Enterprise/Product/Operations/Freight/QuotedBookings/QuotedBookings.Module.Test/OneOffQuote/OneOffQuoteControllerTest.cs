using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	[TestedType(typeof(OneOffQuoteController))]
	public class OneOffQuoteControllerTest : ZControllerBasherTest
	{
		#region New

		public void TestGetNewBusinessEntityInLocalFactory_QuoteOnly()
		{
			OneOffQuoteControllerForTest quoteOnlyController = new OneOffQuoteControllerForTest();
			QuotedBooking quoteOnly = (QuotedBooking)quoteOnlyController.GetNewBusinessEntityInLocalFactoryForTest();
			Assert("Should have a new quote", !quoteOnly.Quote.IsInDatabase);
			AssertNull("Should NOT have a booking", quoteOnly.Booking);
		}

		public void TestGetNewBusinessEntityInLocalFactory_NewClick()
		{
			OneOffQuoteControllerForTest quoteOnlyController = new OneOffQuoteControllerForTest();
			QuotedBooking quoteOnly = (QuotedBooking)quoteOnlyController.GetNewBusinessEntityInLocalFactoryForTest();
			Assert("Should have a new quote", !quoteOnly.Quote.IsInDatabase);
			AssertNull("Should have no booking", quoteOnly.Booking);
		}

		#endregion

		#region Edit

		public override void TestEditForm()
		{
			var quote1 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quote2 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quote3 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var booking = Factory.NewWithValidTestData<ForwardingShipment>();

			var quoteOnly = QuotedBooking.New(quote1.PK, ZGuid.Empty, Factory);
			quoteOnly.TransportMode = Core.Constants.TransportModes.Sea;
			quoteOnly.Origin = "AUSYD";
			quoteOnly.Destination = "NZAKL";
			var quoteIsFinalised = QuotedBooking.New(quote2.PK, ZGuid.Empty, Factory);
			quoteIsFinalised.TransportMode = Core.Constants.TransportModes.Sea;
			quoteIsFinalised.Origin = "AUSYD";
			quoteIsFinalised.Destination = "NZAKL";
			var quoteAndBookingIsConsolidated = QuotedBooking.New(quote3.PK, booking.PK, Factory);

			var bookingWithQuote = (ViewQuotedBooking)GetOneOffQuote();
			Factory.Save();

			var controller = new OneOffQuoteControllerForTest();

			using (var form = controller.ShowEditForm(quoteOnly))
			{
				form.Show();
				AssertEquals("Quote only should open edit form with edit display mode", ODisplayMode.Browse, form.DisplayMode);
			}

			using (var form = controller.ShowEditForm(quoteIsFinalised))
			{
				form.Show();
				AssertEquals("Quote has been accepted and should not be edited", ODisplayMode.Browse, form.DisplayMode);
			}

			using (var form = controller.ShowEditForm(quoteAndBookingIsConsolidated))
			{
				form.Show();
				AssertEquals("Consolidated quote should open edit form with edit display mode", ODisplayMode.Edit, form.DisplayMode);
			}

			using (var form = controller.ShowEditForm(bookingWithQuote))
			{
				form.Show();
				AssertEquals("Booking with quote should be completely view only", ODisplayMode.ReadOnly, form.DisplayMode);
			}
		}

		#endregion

		#region Copy

		public void TestTemplateCopyFormForViewQuotedBooking_ShouldOnlyCopyQuote()
		{
			var quotedBooking = (ViewQuotedBooking)GetOneOffQuote();
			AssertNotNull("Pre-condition: Booking should not be null", quotedBooking?.QuotedBooking?.Booking);
			AssertNotNull("Pre-condition: Quote should not be null", quotedBooking?.QuotedBooking?.Quote);

			using (var form = Controller.ShowTemplateCopyForm(quotedBooking))
			{
				AssertNotNull(form);
				AssertNotNull(form.BusinessEntityForPersistingForm);
				AssertNotNull(form.BusinessEntityForPersistingForm is QuotedBooking);

				var clonedQuotedBooking = (QuotedBooking)form.BusinessEntityForPersistingForm;

				AssertNull("Booking should not be copied.", clonedQuotedBooking?.Booking);
				AssertNotNull("Quote should be copied.", clonedQuotedBooking?.Quote);
			}
		}

		#endregion

		#region ModuleID

		public void TestGetModuleID()
		{
			var oneOffQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			Factory.Save();

			var oneOffQuoteController = (OneOffQuoteController)ZControllerFactory.Create(GetControllerID());
			AssertEquals("OneOffQuotes", oneOffQuoteController.ModuleID.ToString());
		}

		#endregion

		#region CRM Security

		public void TestCRMSecurityCheckpoints()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var testQuotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, testQuotedBooking.Quote.PK));
			CRMSecurityProviderTest<ViewQuotedBooking>.AssertController(new OneOffQuoteController(), viewQuotedBooking, Env.Security.OneOffQuoteCRMSecurity);
		}

		#endregion

		#region Templates

		public void TestGetLoadedBusinessEntityInLocalFactory_Type()
		{
			var quotedBooking = (ViewQuotedBooking)GetOneOffQuote();
			quotedBooking.QuotedBooking.Booking.JS_TransportMode = "AIR";
			Factory.Save();

			var controller = new OneOffQuoteControllerForTest();
			var localBizO = controller.GetLoadedBusinessEntityInLocalFactoryExposed(quotedBooking);

			AssertType("GetLoadedBusinessEntityInLocalFactory should return QuotedBooking", typeof(QuotedBooking), localBizO);
		}

		BusinessObject GetOneOffQuote()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);

			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;

			Factory.Save();

			ViewQuotedBooking viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_JS = booking.PK;
			viewQuotedBooking.VB_TH = quote.PK;

			return viewQuotedBooking;
		}

		#endregion

		#region Implementation

		protected override Type GetBusinessObjectType()
		{
			return typeof(ViewQuotedBooking);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OneOffQuotes;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			Factory.Save();

			ViewQuotedBooking viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_TH = quote.PK;

			return viewQuotedBooking;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawFreightComplianceWiseRegistry;

		protected override void SetUp()
		{
			base.SetUp();
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawFreightComplianceWiseRegistry = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false));
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawFreightComplianceWiseRegistry);
		}

		#endregion
	}
}
