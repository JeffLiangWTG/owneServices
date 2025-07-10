using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(ViewQuotedBookingCollection))]
	public class ViewQuotedBookingCollectionTest : BusinessObjectCollectionTestCase
	{
		public virtual void TestModuleID()
		{
			AssertEquals(ModuleIDs.QuotedBookings, ZMetaData.GetModuleId(Collection));
		}

		public void TestGetBusinessObjectFromCode_ShouldGetCurrentCompanyBusinessObject()
		{
			var newFactory1 = new BusinessObjectFactory();
			var quote1 = QuotedBooking.CreateNewQuote(newFactory1, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote1.TH_QuoteNumber = "QUOTE001";
			quote1.QuoteNumberAlreadySet = true;
			quote1.TH_GC = ZGuid.Empty;
			quote1.TH_GS_NKFirstSignatory = "111";
			newFactory1.Save();

			var newFactory2 = new BusinessObjectFactory();
			var quote2 = QuotedBooking.CreateNewQuote(newFactory2, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote2.TH_QuoteNumber = "QUOTE001";
			quote2.QuoteNumberAlreadySet = true;
			quote2.TH_GC = Env.CurrentCompany.PK;
			quote2.TH_GS_NKFirstSignatory = "222";
			newFactory2.Save();

			var viewQuotedBooking1 = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking1.VB_TH = quote1.PK;

			var viewQuotedBooking2 = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking2.VB_TH = quote2.PK;

			var findBoxListProvider = (IFindBoxListProvider)new ViewQuotedBookingCollection(Factory)
			{
				viewQuotedBooking1,
				viewQuotedBooking2
			};
			AssertEquals
			(
				"Should get business-object that belong to current company.",
				"222",
				((ViewQuotedBooking)findBoxListProvider.GetBusinessObjectFromCode(quote1.TH_QuoteNumber)).QuotedBooking.Quote.TH_GS_NKFirstSignatory
			);
		}

		public void TestGetBusinessObjectFromCode()
		{
			Quote quote1 = Factory.NewWithValidTestData<Quote>();
			quote1.TH_OneTimeQuote = true;
			quote1.TH_QuoteNumber = "111";

			Quote quote2 = Factory.NewWithValidTestData<Quote>();
			quote2.TH_OneTimeQuote = true;
			quote2.TH_QuoteNumber = "222";

			Quote quote3 = Factory.NewWithValidTestData<Quote>();
			quote3.TH_OneTimeQuote = true;
			quote3.TH_QuoteNumber = "333";

			ViewQuotedBooking qB1 = Factory.New<ViewQuotedBooking>();
			qB1.VB_TH = quote1.PK;

			ViewQuotedBooking qB2 = Factory.New<ViewQuotedBooking>();
			qB2.VB_TH = quote2.PK;

			ViewQuotedBooking qB3 = Factory.New<ViewQuotedBooking>();
			qB3.VB_TH = quote3.PK;

			ViewQuotedBookingCollection list = new ViewQuotedBookingCollection(Factory);
			list.Add(qB1);
			list.Add(qB2);
			list.Add(qB3);

			AssertEquals(qB1, ((IFindBoxListProvider)list).GetBusinessObjectFromCode(quote1.TH_QuoteNumber));
			AssertEquals(qB2, ((IFindBoxListProvider)list).GetBusinessObjectFromCode(quote2.TH_QuoteNumber));
			AssertEquals(qB3, ((IFindBoxListProvider)list).GetBusinessObjectFromCode(quote3.TH_QuoteNumber));
		}

		public void TestAllowNew()
		{
			ViewQuotedBookingCollection viewQuotedBookingCollection = new ViewQuotedBookingCollection(Factory);
			Assert("Can't Add new", !viewQuotedBookingCollection.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ViewQuotedBookingCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ViewQuotedBooking>();
		}

		public void TestIFilterModuleExtraNotificationProvider()
		{
			var consol = Factory.New<ForwardingConsol>();
			var booking = QuotedBooking.CreateNewBooking(Factory);

			var quoteBooking = Factory.New<ViewQuotedBooking>();
			quoteBooking.VB_JS = booking.PK;

			var collection = new ViewQuotedBookingCollection(Factory);
			collection.ParentConsol = consol;

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);
			using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				var attachRequest = new ShipmentConsolAttachRequest(null, null);
				helper.Setup(m => m.IsAllowedToAttachShipment(consol, booking)).Returns(attachRequest);
				helper.SetupProperty(p => p.IsGatewayServiceLevelCheckSuspended).SetReturnsDefault(true);

				var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
				AssertNull(notificationProvider.GetExtraNotification(null));
				AssertNull(notificationProvider.GetExtraNotification(Factory.New<DummyBusinessObject>()));

				var notification = notificationProvider.GetExtraNotification(quoteBooking);
				AssertNull("Attach is allowed, no notification", notification);
				helper.VerifyAll();

				attachRequest = new ShipmentConsolAttachRequest(() => "ERROR?", null);
				helper.Setup(m => m.IsAllowedToAttachShipment(consol, booking)).Returns(attachRequest);
				helper.SetupProperty(p => p.IsGatewayServiceLevelCheckSuspended).SetReturnsDefault(true);

				notificationProvider = collection;
				notification = notificationProvider.GetExtraNotification(quoteBooking);
				AssertEquals("Attach is not allowed, error notification", true, notification.Type == CargoWise.ComponentModel.NotificationType.Error);
				AssertEquals("Attach is not allowed, error notification", "ERROR?", notification.Message);
				helper.VerifyAll();

				attachRequest = new ShipmentConsolAttachRequest(null, () => "WARNING?");
				helper.Setup(m => m.IsAllowedToAttachShipment(consol, booking)).Returns(attachRequest);
				helper.SetupProperty(p => p.IsGatewayServiceLevelCheckSuspended).SetReturnsDefault(true);
				notificationProvider = collection;
				notification = notificationProvider.GetExtraNotification(quoteBooking);
				AssertEquals("warning notification", true, notification.Type == CargoWise.ComponentModel.NotificationType.Warning);
				AssertEquals("warning notification", "WARNING?", notification.Message);
				helper.VerifyAll();
			}
		}

		public void TestIFilterModuleExtraNotificationProvider_CheckExistingBookings()
		{
			var consol = Factory.New<ForwardingConsol>();
			var booking = QuotedBooking.CreateNewBooking(Factory);

			Factory.Save();

			consol.Shipments.Add(booking);
			var quoteBooking = Factory.New<ViewQuotedBooking>();
			quoteBooking.VB_JS = booking.PK;

			var collection = new ViewQuotedBookingCollection(Factory) { ParentConsol = consol };

			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			AssertNull(notificationProvider.GetExtraNotification(null));
			AssertNull(notificationProvider.GetExtraNotification(Factory.New<DummyBusinessObject>()));

			var notification = notificationProvider.GetExtraNotification(quoteBooking);
			AssertEquals("The booking has been attached", "This record has already been selected. Please ensure you select only records that have not already been used.", notification.Message);
		}

		public void TestFindBoxListProvider()
		{
			var collection = new ViewQuotedBookingCollection(Factory);
			AssertEquals(typeof(ViewQuotedBookingCollection.ViewQuotedBookingListProvider), collection.FindBoxListProviderExposedForTest.GetType());
			collection.AllowTemplateRecords = true;
			AssertEquals(typeof(ViewQuotedBookingCollection.ViewQuotedBookingListWithTemplatesProvider), collection.FindBoxListProviderExposedForTest.GetType());
			collection.CanHandleBothQuoteAndShipmentCodes = true;
			AssertEquals(typeof(ViewQuotedBookingCollection.QuoteOrShipmentListWithTemplatesProvider), collection.FindBoxListProviderExposedForTest.GetType());
			collection.AllowTemplateRecords = false;
			AssertEquals(typeof(ViewQuotedBookingCollection.QuoteOrShipmentListProvider), collection.FindBoxListProviderExposedForTest.GetType());
		}

		public void TestExternalListValidationIsValidTemplateRecordPK()
		{
			var templateRecord1 = Factory.New<StmTemplateRecord>();
			templateRecord1.STR_ModuleID = nameof(ModuleId.QuotedBookings);
			var templateRecord2 = Factory.New<StmTemplateRecord>();
			templateRecord2.STR_ModuleID = "XYZ";
			Factory.Save();

			var collection = new ViewQuotedBookingCollection(new BusinessObjectFactory());
			collection.AllowTemplateRecords = true;

			Assert(((IExternalListValidation)collection).IsValidTemplateRecordPK(templateRecord1.PK));
			Assert(!((IExternalListValidation)collection).IsValidTemplateRecordPK(templateRecord2.PK));
			Assert(!((IExternalListValidation)collection).IsValidTemplateRecordPK(ZGuid.NewZGuid()));
		}
	}
}
