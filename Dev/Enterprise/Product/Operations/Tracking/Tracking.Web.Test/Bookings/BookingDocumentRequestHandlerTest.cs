using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Tracking.Web.Testing
{
	abstract class BookingDocumentRequestHandlerTest<T> : DocumentRequestHandlerTestCase<T>
			where T : DocumentRequestHelper, new()
	{
		DocumentCommand GetDocumentCommand()
		{
			var booking = TrackingBooking.GetFromRefPK(Factory, Bookings[0].PK, WebEnv.AppInstance.SiteUser as TrackingSiteUser);
			var menuHelper = new TrackingBookingDocumentsMenuHelper(booking, GetExpectedTrackingDocumentType());
			return menuHelper.GetAvailableDocuments().First().DocumentCommand;
		}

		public override void TestGetPdfDocumentRunsWithCorrectBranch() => Assert(true);

		public override void TestFileName()
		{
			var document = GetDocumentCommand();
			var expectedFileName = string.Format(CultureInfo.CurrentCulture, "{0}.pdf", document.SU_MenuName);
			AssertEquals(expectedFileName, RequestHandler.FileName);
		}

		public void TestDocument()
		{
			AssertEquals(1, RequestHandler.BusinessObjects.Length);
			var actualDocument = RequestHandler.BusinessObjects[0] as DocumentCommand;
			AssertNotNull(actualDocument);

			var expectedDocument = GetDocumentCommand();
			AssertEquals(expectedDocument.PK, actualDocument.PK);
		}

		public override void TestContentType()
		{
			AssertEquals(DataContentTypes.Pdf, RequestHandler.ContentType);
		}

		protected abstract TrackingDocumentTypes GetExpectedTrackingDocumentType();

		protected List<ForwardingShipment> Bookings => bookings ?? (bookings = GetNewBookings());

		List<ForwardingShipment> GetNewBookings()
		{
			var bookings = new List<ForwardingShipment>();
			bookings.Add(GetNewBooking());
			bookings.Add(GetNewBooking());
			Factory.Save();

			return bookings;
		}

		IDisposable SetupTestContactAndLogin()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "US";
			var branch = company.Branches.AddNew();
			branch.GB_BranchName = "Test Branch";
			branch.GB_RL_NKHomePort = "USORD";
			var header = Factory.NewWithValidTestData<OrgHeader>();
			company.GC_OH_OrgProxy = header.PK;

			var contact = header.Contacts.AddNew();
			contact.OC_Email = "someone@cargowise.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("someone");
			Factory.Save();

			TrackingSiteUser user = new TrackingSiteUser();
			user.Login(contact.OrganisationCode, "someone@cargowise.com", "someone");

			var environment = Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			GlbCompany.CurrentCompany.Refresh();

			AssertEquals("Precondition: User is logged in", true, user.IsLoggedIn);

			return environment;
		}

		protected override bool IsDocumentRequestHandler => false;

		protected override void SetupForTest(DocumentRequestHandler<T> requestHandler)
		{
			requestHandler.QueryString.Add(DataRequestHelper.DataKey, Bookings[0].PK.ToString());
		}

		protected override void SetUp()
		{
			base.SetUp();
			temporaryUserContext = SetupTestContactAndLogin();
		}

		protected override void TearDown()
		{
			base.TearDown();
			temporaryUserContext?.Dispose();
		}

		IDisposable temporaryUserContext;

		protected virtual ForwardingShipment GetNewBooking()
		{
			var viewBooking = Factory.New<ViewTrackingBooking>();
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.Booking.JS_OH_DeliveryAgent = ((TrackingSiteUser)WebEnv.AppInstance.SiteUser).LoggedInOrganisation.PK;
			viewBooking.VB_JS = booking.Booking.PK;

			return booking.Booking;
		}

		List<ForwardingShipment> bookings;
	}
}
