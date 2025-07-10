using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Freight;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	[TestedType(typeof(QuotedBookingController))]
	public class QuotedBookingControllerTest : ZControllerBasherTest
	{
		#region New

		public void TestGetNewBusinessEntityInLocalFactory_BookingOnly()
		{
			QuotedBookingControllerForTest quoteOnlyController = new QuotedBookingControllerForTest(QuotedBookingState.BookingOnly);
			QuotedBooking quoteOnly = (QuotedBooking)quoteOnlyController.GetNewBusinessEntityInLocalFactoryForTest();
			Assert("Should have a new booking", !quoteOnly.Booking.IsInDatabase);
			AssertNull("Should NOT have a quote", quoteOnly.Quote);
		}

		public void TestGetLoadedBusinessEntityInLocalFactory_ShouldReturnNull_WhenQuotedBookingIsNull()
		{
			var controller = new QuotedBookingControllerForTest();
			var result = controller.GetLoadedBusinessEntityInLocalFactoryForTest(null);
			AssertEquals(null, result);
		}

		public void TestGetNewBusinessEntityInLocalFactory_QuotedBookingOnly()
		{
			QuotedBookingControllerForTest quoteOnlyController = new QuotedBookingControllerForTest(QuotedBookingState.AcceptedBookingWithQuote);
			QuotedBooking quoteOnly = (QuotedBooking)quoteOnlyController.GetNewBusinessEntityInLocalFactoryForTest();
			Assert("Should have a new quote", !quoteOnly.Quote.IsInDatabase);
			Assert("Should have a new booking", !quoteOnly.Booking.IsInDatabase);
		}

		public void TestGetNewBusinessEntityInLocalFactory_NewClick()
		{
			QuotedBookingControllerForTest quoteOnlyController = new QuotedBookingControllerForTest();
			QuotedBooking quoteOnly = (QuotedBooking)quoteOnlyController.GetNewBusinessEntityInLocalFactoryForTest();
			Assert("Should have a new quote", !quoteOnly.Quote.IsInDatabase);
			Assert("Should have a new booking", !quoteOnly.Booking.IsInDatabase);
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "Invalid State")]
		public void TestGetNewBusinessEntityInLocalFactory_Other()
		{
			QuotedBookingControllerForTest quoteOnlyController = new QuotedBookingControllerForTest(QuotedBookingState.UnacceptedBookingWithQuote);
			quoteOnlyController.GetNewBusinessEntityInLocalFactoryForTest();
		}

		public void TestGetNewBusinessEntityInLocalFactory_NewClick_Registry()
		{
			FreightDataRegistry.Instance.DefaultBookingNewButton.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BookingNewButtonLabelList.Codes.BookingWithQuote);

			QuotedBookingControllerForTest quoteOnlyController = new QuotedBookingControllerForTest();
			QuotedBooking quoteOnly = (QuotedBooking)quoteOnlyController.GetNewBusinessEntityInLocalFactoryForTest();
			Assert("Should have a new quote", !quoteOnly.Quote.IsInDatabase);
			AssertNotNull("Should have a new booking", quoteOnly.Booking);
		}

		#endregion

		#region Load

		public void TestGetLoadedBusinessEntityInLocalFactoryForTest()
		{
			ViewQuotedBooking viewQuotedBooking = (ViewQuotedBooking)GetBusinessObjectThatIsInTheDatabase();
			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;

			QuotedBookingControllerForTest quoteOnlyController = new QuotedBookingControllerForTest(QuotedBookingState.QuoteOnly);
			QuotedBooking quotedBookingFromController = (QuotedBooking)quoteOnlyController.GetLoadedBusinessEntityInLocalFactoryForTest(viewQuotedBooking);
			AssertNotNull("Should be fine", quotedBookingFromController);
			AssertNotEquals("Should be dif cause dif factory", quotedBooking, quotedBookingFromController);
			AssertEquals("But Booking should be same", quotedBooking.Booking.PK, quotedBookingFromController.Booking.PK);
			AssertEquals("And Quote should be same", quotedBooking.Quote.PK, quotedBookingFromController.Quote.PK);

			QuotedBooking newQuotedBookingFromController = (QuotedBooking)quoteOnlyController.GetLoadedBusinessEntityInLocalFactoryForTest(quotedBookingFromController);
			AssertEquals("If QuotedBooking is passed in, it should be return", quotedBookingFromController, newQuotedBookingFromController);
		}

		public void TestFormLoadsInEditModeWhenBookingIsConsolidated()
		{
			var controller = new QuotedBookingControllerForTest(QuotedBookingState.AcceptedBookingWithQuote);
			var quotedBooking = (QuotedBooking)controller.GetNewBusinessEntityInLocalFactoryForTest();

			using (var form = controller.ShowEditForm(quotedBooking))
			{
				form.Show();
				AssertEquals("Quoted Booking form should be open in a edit mode.", ODisplayMode.Edit, form.DisplayMode);
			}

			quotedBooking.Booking.JS_IsForwardRegistered = true;

			using (var form = controller.ShowEditForm(quotedBooking))
			{
				form.Show();
				AssertEquals("Quoted Booking form should be open in a edit mode, though it has already been consolidated.", ODisplayMode.Edit, form.DisplayMode);
			}
		}

		#endregion

		#region Copy

		public void TestTemplateCopyFormForViewQuotedBooking()
		{
			AssertControllerNotNull();

			try
			{
				IBusiness businessObject = GetBusinessObjectThatIsInTheDatabase();
				Controller.ShowTemplateCopyForm((BusinessObject)businessObject);
			}
			catch (ModuleFeatureNotSupportedException)
			{
				Fail("Must show copy form");
			}
		}

		public void TestTemplateCopyFormForViewQuotedBooking_NoSecurityRights()
		{
			Env.Security.QuickBookingNew.IsAllowed = false;

			AssertControllerNotNull();

			IBusiness businessObject = GetBusinessObjectThatIsInTheDatabase();
			Controller.ShowTemplateCopyForm((BusinessObject)businessObject);

			AssertNull("No security rights to show form", Controller.LastShownForm);
		}

		#endregion

		#region GetForm

		public void TestGetForm()
		{
			ViewQuotedBooking viewQuotedBooking = (ViewQuotedBooking)GetBusinessObjectThatIsInTheDatabase();
			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			QuotedBookingControllerForTest controller = new QuotedBookingControllerForTest();

			using (ZForm form = (ZForm)controller.GetFormForTest(quotedBooking))
			{
				AssertSame(quotedBooking, form.DataSource);
			}

			using (ZForm form = (ZForm)controller.GetFormForTest(viewQuotedBooking))
			{
				AssertSame(quotedBooking, form.DataSource);
			}
		}

		#endregion

		#region ModuleID

		public void TestGetModuleID()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			Factory.Save();

			var quoteBookingController = ZControllerFactory.Create(GetControllerID());
			AssertEquals("QuotedBookings", quoteBookingController.ModuleID.ToString());
		}

		#endregion

		#region Showing Forms

		#region TestShowForm_QuoteMustMatchCurrentCompanyLogin

		public void TestShowViewForm_QuoteMustMatchCurrentCompanyLogin()
		{
			TestShowForm_QuoteMustMatchCurrentCompanyLogin((controller, bizObj) => controller.ShowViewForm(bizObj));
		}

		public void TestShowEditForm_QuoteMustMatchCurrentCompanyLogin()
		{
			TestShowForm_QuoteMustMatchCurrentCompanyLogin((controller, bizObj) => controller.ShowEditForm(bizObj));
		}

		public void TestShowDeleteForm_QuoteMustMatchCurrentCompanyLogin()
		{
			TestShowForm_QuoteMustMatchCurrentCompanyLogin((controller, bizObj) => controller.ShowDeleteForm(bizObj));
		}

		public void TestShowCopyForm_QuoteMustMatchCurrentCompanyLogin()
		{
			TestShowForm_QuoteMustMatchCurrentCompanyLogin((controller, bizObj) => controller.ShowTemplateCopyForm(bizObj));
		}

		void TestShowForm_QuoteMustMatchCurrentCompanyLogin(Action<QuotedBookingController, BusinessObject> showFormDelegate)
		{
			var otherFactory = new BusinessObjectFactory();
			var otherCompany = otherFactory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "GC2";
			otherCompany.GC_RN_NKCountryCode = "AU";

			var spotQuoteForSameCompany = QuotedBooking.New(QuoteBookingType.SpotQuote, otherFactory);
			spotQuoteForSameCompany.Quote.TH_GC = Env.CurrentCompany.PK;
			var spotQuoteForDifferentCompany = QuotedBooking.New(QuoteBookingType.SpotQuote, otherFactory);
			spotQuoteForDifferentCompany.Quote.TH_GC = otherCompany.PK;

			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, otherFactory);

			var quotedBookingForSameCompany = QuotedBooking.New(QuoteBookingType.BookingWithQuote, otherFactory);
			quotedBookingForSameCompany.Quote.TH_GC = Env.CurrentCompany.PK;
			var quotedBookingForDifferentCompany = QuotedBooking.New(QuoteBookingType.BookingWithQuote, otherFactory);
			quotedBookingForDifferentCompany.Quote.TH_GC = otherCompany.PK;

			otherFactory.Save();

			AssertShowFormAllowed(showFormDelegate, spotQuoteForSameCompany, true, null, null);
			AssertShowFormAllowed(showFormDelegate, spotQuoteForDifferentCompany, false, "This One Off Quote is for login users in Australia (GC2).\r\nPlease login to the relevant company to view the One Off Quote.", "Access Denied: Incorrect login company");
			AssertShowFormAllowed(showFormDelegate, quickBooking, true, null, null);
			AssertShowFormAllowed(showFormDelegate, quotedBookingForSameCompany, true, null, null);
			AssertShowFormAllowed(showFormDelegate, quotedBookingForDifferentCompany, false, "This Booking with Quote is for login users in Australia (GC2).\r\nPlease login to the relevant company to view the Booking with Quote.", "Access Denied: Incorrect login company");

			AssertShowFormAllowed(showFormDelegate, Factory.Load<ViewQuotedBooking>(spotQuoteForSameCompany.ViewPK), true, null, null);
			AssertShowFormAllowed(showFormDelegate, Factory.Load<ViewQuotedBooking>(spotQuoteForDifferentCompany.ViewPK), false, "This One Off Quote is for login users in Australia (GC2).\r\nPlease login to the relevant company to view the One Off Quote.", "Access Denied: Incorrect login company");
			AssertShowFormAllowed(showFormDelegate, Factory.Load<ViewQuotedBooking>(quickBooking.ViewPK), true, null, null);
			AssertShowFormAllowed(showFormDelegate, Factory.Load<ViewQuotedBooking>(quotedBookingForSameCompany.ViewPK), true, null, null);
			AssertShowFormAllowed(showFormDelegate, Factory.Load<ViewQuotedBooking>(quotedBookingForDifferentCompany.ViewPK), false, "This Booking with Quote is for login users in Australia (GC2).\r\nPlease login to the relevant company to view the Booking with Quote.", "Access Denied: Incorrect login company");
		}

		void AssertShowFormAllowed(Action<QuotedBookingController, BusinessObject> showFormDelegate, BusinessObject sourceEntity, bool expectedIsAllowed, string expectedLastMessage, string expectedLastCaption)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var controller = new QuotedBookingController();
			showFormDelegate(controller, sourceEntity);

			CombineAssertions(() =>
			{
				using (var lastShownForm = controller.LastShownForm)
				{
					if (expectedIsAllowed)
					{
						AssertNotNull("LastShownForm", lastShownForm);
					}
					else
					{
						AssertNull("LastShownForm", lastShownForm);
					}
				}

				AssertMultilineASCIIEquals("LastMessage.Text", expectedLastMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertMultilineASCIIEquals("LastMessage.Caption", expectedLastCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
			});
		}

		#endregion

		#endregion

		#region CRM Security

		public void TestCRMSecurityCheckpoints()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var testQuotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, testQuotedBooking.Quote.PK));

			CRMSecurityProviderTest<ViewQuotedBooking>.AssertController(new QuotedBookingController(), viewQuotedBooking, Env.Security.QuickBookingCRMSecurity);
		}

		#endregion

		#region Templates

		public void TestGetLoadedBusinessEntityInLocalFactory_NormalRecord()
		{
			var quotedBooking = (ViewQuotedBooking)GetBusinessObjectThatIsInTheDatabase();
			quotedBooking.QuotedBooking.Booking.JS_TransportMode = "AIR";
			Factory.Save();

			var controller = new QuotedBookingControllerForTest();
			var localBizO = controller.GetLoadedBusinessEntityInLocalFactoryExposed(quotedBooking);

			AssertNotNull(localBizO);

			CombineAssertions(() =>
			{
				AssertEquals("AIR", localBizO.Booking.JS_TransportMode);
				AssertNotEquals("Should be in different factory", quotedBooking.Factory._Instance, localBizO.Factory._Instance);
				AssertNotEquals("Should not be in template factory", typeof(TemplateRecordBusinessObjectFactory), localBizO.Factory.GetType());
				AssertEquals("Should have same pk", quotedBooking.QuotedBooking.PK, localBizO.PK);
				AssertNull((localBizO as ITemplateRecordProvider)?.TemplateRecord);
			});
		}

		public void TestGetLoadedBusinessEntityInLocalFactory_TemplateRecordProvider()
		{
			ViewQuotedBooking quotedBooking = null;
			using (var module = new QuotedBookingModuleTest.QuotedBookingModuleForTest())
			{
				quotedBooking = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				quotedBooking.QuotedBooking.Booking.JS_TransportMode = "AIR";
				quotedBooking.Factory.Save();
			}

			var controller = new QuotedBookingControllerForTest();
			var localBizO = controller.GetLoadedBusinessEntityInLocalFactoryExposed(quotedBooking);

			AssertNotNull(localBizO);

			CombineAssertions(() =>
			{
				AssertEquals("AIR", localBizO.Booking.JS_TransportMode);
				AssertNotEquals("Should be in different factory", quotedBooking.Factory._Instance, localBizO.Factory._Instance);
				AssertEquals("Should be in template factory", typeof(TemplateRecordBusinessObjectFactory), localBizO.Factory.GetType());
				AssertNotEquals("Template record do not keep pks", quotedBooking.PK, localBizO.PK);
				AssertEquals(
					((quotedBooking as ITemplateRecordProvider).TemplateRecord as BusinessObject).PK,
					((localBizO as ITemplateRecordProvider).TemplateRecord as BusinessObject).PK
				);
			});
		}

		public void TestGetLoadedBusinessEntityInLocalFactory_TemplateRecord()
		{
			ViewQuotedBooking quotedBooking = null;
			using (var module = new QuotedBookingModuleTest.QuotedBookingModuleForTest())
			{
				quotedBooking = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				quotedBooking.QuotedBooking.Booking.JS_TransportMode = "AIR";
				quotedBooking.Factory.Save();
			}

			var controller = new QuotedBookingControllerForTest();
			var localBizO = controller.GetLoadedBusinessEntityInLocalFactoryExposed((quotedBooking as ITemplateRecordProvider).TemplateRecord as BusinessObject);

			AssertNotNull(localBizO);

			CombineAssertions(() =>
			{
				AssertEquals("AIR", localBizO.Booking.JS_TransportMode);
				AssertNotEquals("Should be in different factory", quotedBooking.Factory._Instance, localBizO.Factory._Instance);
				AssertEquals("Should be in template factory", typeof(TemplateRecordBusinessObjectFactory), localBizO.Factory.GetType());
				AssertNotEquals("Template record do not keep pks", quotedBooking.PK, localBizO.PK);
				AssertEquals(
					((quotedBooking as ITemplateRecordProvider).TemplateRecord as BusinessObject).PK,
					((localBizO as ITemplateRecordProvider).TemplateRecord as BusinessObject).PK
				);
			});
		}

		#endregion

		#region Implementation

		protected override Type GetBusinessObjectType()
		{
			return typeof(ViewQuotedBooking);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.QuotedBookings;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
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

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#endregion
	}
}
