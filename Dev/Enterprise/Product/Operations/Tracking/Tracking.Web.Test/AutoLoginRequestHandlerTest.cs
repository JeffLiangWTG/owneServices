using System;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.Web.Security;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentScanning.Web;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Utilities.Test;
using Moq;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class AutoLoginRequestHandlerTest : TestCaseWithFactory, IHttpContextEnabledTestWithAppInstance, IHttpContextEnabledTestWithRequestPath
	{
		[UseSnapshotProtection]
		public void TestProcessRequest_Home_Module_ShouldUseDisposableConnection()
		{
			var errorReporterMock = new Mock<IErrorReporter>();
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				using (var env = new TestWebDbEnvironment())
				{
					env.SetServingWebBasedApp(true);
					Db.ResetAlreadyReported_ForTest();
					RequestHandler.ProcessRequest(HttpContext.Current);
					AssertEquals(expected: true, actual: AppInstance.SiteUser.IsLoggedIn);
				}

				errorReporterMock.Verify(
					reporter => reporter.ReportDeveloperExceptionOrHandleSilently(
						It.IsAny<string>(),
						ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction,
						It.IsAny<InvalidOperationException>()
					),
					Times.Never
				);
			}
		}

		public void TestIsReusable()
		{
			AssertEquals(true, RequestHandler.IsReusable);
		}

		public void TestProcessRequest_NoBusinessContext_NoContact_ShouldRequireLogin()
		{
			TestProcessRequest(string.Empty, false, false, false);
			var expectedRedirectLocation = GetRedirectFromLoginPagePath(AppInstance.DefaultPage);
			AssertEquals(expectedRedirectLocation, HttpContext.Current.Response.RedirectLocation);
		}

		public void TestProcessRequest_Home()
		{
			string expectedRedirectLocation = string.Format("{0}?Ref={1}", AppInstance.DefaultPage, BusinessContextPKAsString);
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Home_Number()
		{
			string expectedRedirectLocation = string.Format("{0}?Number={1}", AppInstance.DefaultPage, BusinessContextNK);
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Home_Module()
		{
			string expectedRedirectLocation = AppInstance.DefaultPage;
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Shipment_Module()
		{
			string expectedRedirectLocation = string.Format("{0}?Table={1}", AppInstance.ShipmentsPage, JobShipmentSchema.Constants.TableName);
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Shipment()
		{
			string expectedRedirectLocation = string.Format("{0}?Ref={1}&Table={2}", AppInstance.ShipmentPage, BusinessContextPKAsString, JobShipmentSchema.Constants.TableName);
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Shipment_Number()
		{
			string expectedRedirectLocation = string.Format("{0}?Number={1}&Table={2}", AppInstance.ShipmentPage, BusinessContextNK, JobShipmentSchema.Constants.TableName);
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Shipment_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Ref={1}&Table={2}", AppInstance.ShipmentPage, BusinessContextPKAsString, JobShipmentSchema.Constants.TableName));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_Shipment_RequireLogin()
		{
			string expectedRedirectLocation = string.Format("{0}?Ref={1}&Table={2}", AppInstance.ShipmentPage, BusinessContextPKAsString, JobShipmentSchema.Constants.TableName);
			TestProcessRequest(expectedRedirectLocation, true);
		}

		public void TestProcessRequest_Shipment_RequireLogin_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Number={1}&Table={2}", AppInstance.ShipmentPage, BusinessContextNK, JobShipmentSchema.Constants.TableName));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_Shipment_RequireLogin_EmptyContactPK()
		{
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Number={1}&Table={2}", AppInstance.ShipmentPage, BusinessContextNK, JobShipmentSchema.Constants.TableName));
			TestProcessRequest(expectedRedirectLocation, false, false, false);
		}

		public void TestProcessRequest_Shipment_Number_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Number={1}&Table={2}", AppInstance.ShipmentPage, BusinessContextNK, JobShipmentSchema.Constants.TableName));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_Shipment_Number_RequireLogin()
		{
			string expectedRedirectLocation = string.Format("{0}?Number={1}&Table={2}", AppInstance.ShipmentPage, BusinessContextNK, JobShipmentSchema.Constants.TableName);
			TestProcessRequest(expectedRedirectLocation, true);
		}

		public void TestProcessRequest_Shipment_Number_RequireLogin_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Number={1}&Table={2}", AppInstance.ShipmentPage, BusinessContextNK, JobShipmentSchema.Constants.TableName));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_Shipment_Number_RequireLogin_EmptyContactPK()
		{
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Number={1}&Table={2}", AppInstance.ShipmentPage, BusinessContextNK, JobShipmentSchema.Constants.TableName));
			TestProcessRequest(expectedRedirectLocation, false, false, false);
		}

		public void TestProcessRequest_Declaration()
		{
			string expectedRedirectLocation = string.Format("{0}?Ref={1}", AppInstance.DeclarationDetailsPage, BusinessContextPKAsString);
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Declaration_Module()
		{
			string expectedRedirectLocation = AppInstance.DeclarationModulePage;
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Declaration_Number()
		{
			string expectedRedirectLocation = string.Format("{0}?Number={1}", AppInstance.DeclarationDetailsPage, BusinessContextNK);
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Declaration_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Ref={1}&Table={2}", AppInstance.ShipmentPage, BusinessContextPKAsString, JobDeclarationSchema.Constants.TableName));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_Declaration_Number_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Number={1}&Table={2}", AppInstance.ShipmentPage, BusinessContextNK, JobDeclarationSchema.Constants.TableName));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_Booking_Module()
		{
			string expectedRedirectLocation = AppInstance.BookingsPage;
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Booking()
		{
			string expectedRedirectLocation = string.Format("{0}?Ref={1}", AppInstance.BookingDetailsPage, BusinessContextPKAsString);
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Booking_EmptyContactPK()
		{
			string expectedRedirectLocation = string.Format("{0}?Ref={1}", AppInstance.BookingDetailsPage, BusinessContextPKAsString);
			TestProcessRequest(expectedRedirectLocation, true, true);
		}

		public void TestProcessRequest_Booking_Number()
		{
			string expectedRedirectLocation = string.Format("{0}?Number={1}", AppInstance.BookingDetailsPage, BusinessContextNK);
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Booking_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Ref={1}", AppInstance.BookingDetailsPage, BusinessContextPKAsString));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_Booking_Number_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Number={1}", AppInstance.BookingDetailsPage, BusinessContextNK));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_Order_Module()
		{
			string expectedRedirectLocation = AppInstance.OrdersPage;
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Order()
		{
			string expectedRedirectLocation = string.Format("{0}?Ref={1}", AppInstance.OrderDetailsPage, BusinessContextPKAsString);
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Order_Number()
		{
			string expectedRedirectLocation = string.Format("{0}?Number={1}", AppInstance.OrderDetailsPage, BusinessContextNK);
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Order_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Ref={1}", AppInstance.OrderDetailsPage, BusinessContextPKAsString));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_Order_Number_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Number={1}", AppInstance.OrderDetailsPage, BusinessContextNK));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_Transaction_Module()
		{
			string expectedRedirectLocation = AppInstance.TransactionsPage;
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Transaction()
		{
			string expectedRedirectLocation = string.Format("/webapp/{0}", InvoiceRequestHandler.RequestHelper.GetHandlerUrl(new ZGuid(BusinessContextPKAsString)));
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_Transaction_Number()
		{
			AssertEquals("Pre-condition", false, AppInstance.SiteUser.IsLoggedIn);
			AssertNull("Pre-condition", GetCurrentAuthenticationTicketFromResponse());
			AssertNull("Pre-condition", HttpContext.Current.Response.RedirectLocation);

			RequestHandler.ProcessRequest(HttpContext.Current);
			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));
		}

		public void TestProcessRequest_Transaction_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("/webapp/{0}", InvoiceRequestHandler.RequestHelper.GetHandlerUrl(new ZGuid(BusinessContextPKAsString))));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_WarehouseOrder_Module()
		{
			string expectedRedirectLocation = AppInstance.WarehouseOrders;
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_WarehouseOrder()
		{
			string expectedRedirectLocation = string.Format("{0}?Ref={1}", AppInstance.WarehouseOrderDetailsPage, BusinessContextPKAsString);
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_WarehouseOrder_Number()
		{
			string expectedRedirectLocation = string.Format("{0}?Number={1}", AppInstance.WarehouseOrderDetailsPage, BusinessContextNK);
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_WarehouseOrder_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Ref={1}", AppInstance.WarehouseOrderDetailsPage, BusinessContextPKAsString));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_WarehouseOrder_Number_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Number={1}", AppInstance.WarehouseOrderDetailsPage, BusinessContextNK));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_eDoc()
		{
			string expectedRedirectLocation = string.Format("{0}?Doc={1}&Ref={2}", AppInstance.ApplicationRoot + eDocsRequestHandler.RequestHelper.BaseUrl, BusinessContextPKAsString, BusinessContextAdditionalRefAsString);
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_eDoc_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("{0}?Doc={1}&Ref={2}", eDocsRequestHandler.RequestHelper.BaseUrl, BusinessContextPKAsString, BusinessContextAdditionalRefAsString));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_Quotations()
		{
			var expectedRedirectLocation = string.Format("/webapp/{0}", QuoteRequestHandler.RequestHelper.GetHandlerUrl(new ZGuid(BusinessContextPKAsString)));
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_QuotationClientReplyNotAccept_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("/webapp/{0}", QuoteClientReplyRequestHandler.RequestHelper.GetHandlerUrlForClientAcceptance(new ZGuid(BusinessContextPKAsString))));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_QuotationClientReplyAccept_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("/webapp/{0}", QuoteClientReplyRequestHandler.RequestHelper.GetHandlerUrlForClientNonAcceptance(new ZGuid(BusinessContextPKAsString))));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_Quotations_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("/webapp/{0}", QuoteRequestHandler.RequestHelper.GetHandlerUrl(new ZGuid(BusinessContextPKAsString))));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_Transaction_EmptyContactPK()
		{
			RequestHandler.ProcessRequest(HttpContext.Current);
			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));
		}

		public void TestProcessRequest_eDoc_EmptyContactPK()
		{
			RequestHandler.ProcessRequest(HttpContext.Current);
			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));
		}

		public void TestProcessRequest_Quotations_EmptyContactPK()
		{
			RequestHandler.ProcessRequest(HttpContext.Current);
			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));
		}

		public void TestProcessRequest_NotYetRedirectedForExcelHack()
		{
			var headers = new NameValueCollection();
			var context = GetContextWithHeaders(headers);
			var requestHandler = new AutoLoginRequestHandler();

			requestHandler.ProcessRequest(context);

			AssertEquals(2, headers.Count);
			AssertEquals("0;url=/webapp/AutoLoginRequestHandler.axd?ClientRedirection=TRUE", headers["Refresh"]);
			AssertEquals("text/html", headers["Content-Type"]);
		}

		public void TestProcessRequest_NotYetRedirectedForExcelHackAndHasQueryString()
		{
			var headers = new NameValueCollection();
			var context = GetContextWithHeaders(headers, "QueryString=Exist");
			var requestHandler = new AutoLoginRequestHandler();

			requestHandler.ProcessRequest(context);

			AssertEquals(2, headers.Count);
			AssertEquals("0;url=/webapp/AutoLoginRequestHandler.axd?QueryString=Exist&ClientRedirection=TRUE", headers["Refresh"]);
			AssertEquals("text/html", headers["Content-Type"]);
		}

		public void TestProcessRequest_HouseBill()
		{
			var expectedRedirectLocation = string.Format("/webapp/{0}", HouseBillRequestHandler.RequestHelper.GetHandlerUrl(new ZGuid(BusinessContextPKAsString)));
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_HouseBill_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("/webapp/{0}", HouseBillRequestHandler.RequestHelper.GetHandlerUrl(new ZGuid(BusinessContextPKAsString))));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_HouseBill_EmptyContactPK()
		{
			RequestHandler.ProcessRequest(HttpContext.Current);
			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));
		}

		public void TestProcessRequest_FreightLabel()
		{
			var expectedRedirectLocation = string.Format("/webapp/{0}", FreightLabelRequestHandler.RequestHelper.GetHandlerUrl(new ZGuid(BusinessContextPKAsString)));
			TestProcessRequest(expectedRedirectLocation);
		}

		public void TestProcessRequest_FreightLabel_PasswordRequired()
		{
			WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var expectedRedirectLocation = GetRedirectFromLoginPagePath(string.Format("/webapp/{0}", FreightLabelRequestHandler.RequestHelper.GetHandlerUrl(new ZGuid(BusinessContextPKAsString))));
			TestProcessRequest(expectedRedirectLocation, false);
		}

		public void TestProcessRequest_Home_IfAlreadyLoggedIn_SetsSecureQueryStringIntoSession()
		{
			AssertEquals("Pre-condition", false, AppInstance.SiteUser.IsLoggedIn);
			AssertNull("Pre-condition", GetCurrentAuthenticationTicketFromResponse());
			AssertNull("Pre-condition", HttpContext.Current.Response.RedirectLocation);

			string expectedRedirectLocation = string.Format("{0}?Ref={1}", AppInstance.DefaultPage, BusinessContextPKAsString);
			AppInstance.SiteUser.LoginForTest(Contact.OrgCode, Contact.Email, "pass");

			RequestHandler.ProcessRequest(HttpContext.Current);

			AssertEquals(expectedRedirectLocation, HttpContext.Current.Response.RedirectLocation);
			AssertNotNull(AppInstance.Session[Global.AutoLoginQueryStringDataIndexer]);
			AssertEquals(Contact.PK, AppInstance.SiteUser.LoggedInUserPK);
		}

		public void TestProcessRequest_FreightLabel_EmptyContactPK()
		{
			RequestHandler.ProcessRequest(HttpContext.Current);
			Assert("Should not be redirected", string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));
		}

		HttpContextBase GetContextWithHeaders(NameValueCollection headers, string queryString = null)
		{
			var mockRequest = new Mock<HttpRequestBase>();
			var mockResponse = new Mock<HttpResponseBase>();

			var mockContext = new Mock<HttpContextBase>();
			mockContext.Setup(x => x.Request).Returns(mockRequest.Object);
			mockContext.Setup(x => x.Response).Returns(mockResponse.Object);
			mockContext.Setup(x => x.ApplicationInstance).Returns(AppInstance);

			mockRequest.SetupAllProperties();
			mockRequest.Setup(x => x.QueryString).Returns(new NameValueCollection());
			var uri = new Uri("http://webtracker/path" + (!string.IsNullOrEmpty(queryString) ? "?" + queryString : string.Empty), UriKind.Absolute);
			mockRequest.Setup(x => x.Url).Returns(uri);
			mockResponse.SetupAllProperties();
			mockResponse.Setup(x => x.Headers.Remove(It.IsAny<string>())).Callback((string s) => { headers.Remove(s); });
			mockResponse.Setup(x => x.AddHeader(It.IsAny<string>(), It.IsAny<string>())).Callback((string s1, string s2) => { headers.Add(s1, s2); });
			mockResponse.Setup(x => x.Headers).Returns(headers);

			return mockContext.Object;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			AppInstance.OnCustomSessionStart(AppInstance, EventArgs.Empty);
			Factory.Save();
		}

		IHttpHandler RequestHandler
		{
			get
			{
				if (fRequestHandler == null)
				{
					fRequestHandler = new AutoLoginRequestHandler();
				}

				return fRequestHandler;
			}
		}

		OrgHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = Factory.NewWithValidTestData<OrgHeader>();
					fHeader.OH_Code = "ORGCODE";
				}

				return fHeader;
			}
		}

		OrgContact Contact
		{
			get
			{
				if (fContact == null)
				{
					fContact = Header.Contacts.AddNew();
					fContact.FillWithValidTestData();
					fContact.OC_Email = "contact@edi.com.au";
					fContact.OC_WebAccessEnabled = true;
					fContact.SetHashedPassword("pass");

					Factory.Save();
				}

				return fContact;
			}
		}

		FormsAuthenticationTicket GetCurrentAuthenticationTicketFromResponse()
		{
			FormsAuthenticationTicket result = null;

			if (((IList)HttpContext.Current.Response.Cookies.AllKeys).Contains(FormsAuthentication.FormsCookieName))
			{
				for (int i = 0; i < HttpContext.Current.Response.Cookies.Count; i++)
				{
					HttpCookie cookie = HttpContext.Current.Response.Cookies[i];
					// not persistent cookie has an expiry date of MinValue
					if (cookie.Name == FormsAuthentication.FormsCookieName && cookie.Expires == DateTime.MinValue && cookie.Value != null)
					{
						result = FormsAuthentication.Decrypt(cookie.Value);
					}
				}
			}

			return result;
		}

		//this is just a hack to generate urls for functional testing.
		//public void TestGetQuery()
		//{
		//    WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost/tracking.web");
		//    TrackingUrlCreator Creator = new TrackingUrlCreator();
		//    string result = Creator.CreateUrl(ZGuid.Empty, TrackingConstants.BusinessContext.WarehouseOrder, new ZGuid("c9d166d5-91fd-4fe1-b501-fabc884550f6"));
		//    AssertEquals("", result);
		//}

		string GetQueryStringWithPKAsString(ZGuid contactPK, string businessContextAsString, string testName)
		{
			return GetQueryStringWithPKAsString(contactPK, businessContextAsString, BusinessContextPKAsString, testName);
		}

		string GetQueryStringWithPKAsString(ZGuid contactPK, string businessContextAsString, string businessContextPKAsString, string testName)
		{
			SecureQueryString result = new SecureQueryString();
			result.Add(TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString());
			result.Add(TrackingConstants.AutoLogin.BusinessContextKey, businessContextAsString);
			result.Add(TrackingConstants.AutoLogin.BusinessContextPKKey, businessContextPKAsString);
			if (businessContextAsString == "eDoc")
			{
				result.Add(TrackingConstants.AutoLogin.BusinessContextAdditionalRefsKey, BusinessContextAdditionalRefAsString);
			}

			AppendQueryString(result, testName);
			return result.ToString();
		}

		string GetQueryStringWithNKAsString(ZGuid contactPK, string businessContextAsString, string testName)
		{
			SecureQueryString result = new SecureQueryString();
			result.Add(TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString());
			result.Add(TrackingConstants.AutoLogin.BusinessContextKey, businessContextAsString);
			result.Add(TrackingConstants.AutoLogin.BusinessContextNKKey, BusinessContextNK);
			AppendQueryString(result, testName);
			return result.ToString();
		}

		string GetQueryStringForModule(ZGuid contactPK, string businessContextAsString, string testName)
		{
			SecureQueryString result = new SecureQueryString();
			result.Add(TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString());
			result.Add(TrackingConstants.AutoLogin.BusinessContextKey, businessContextAsString);
			AppendQueryString(result, testName);
			return result.ToString();
		}

		void AppendQueryString(SecureQueryString query, string testName)
		{
			if (testName.Contains("_RequireLogin"))
			{
				query.Add(TrackingConstants.AutoLogin.RequireLoginKey, true.ToString());
			}
		}

		string GetRedirectFromLoginPagePath(string redirectLocation)
		{
			return string.Format("{0}?ReturnUrl={1}", FormsAuthentication.LoginUrl, HttpContext.Current.Server.UrlEncode(redirectLocation));
		}

		void TestProcessRequest(string expectedRedirectLocation)
		{
			TestProcessRequest(expectedRedirectLocation, true);
		}

		void TestProcessRequest(string expectedRedirectLocation, bool shouldUserBeLoggedIn)
		{
			TestProcessRequest(expectedRedirectLocation, shouldUserBeLoggedIn, false);
		}

		void TestProcessRequest(string expectedRedirectLocation, bool shouldUserBeLoggedIn, bool shouldBeLoggedInAsQuickShipmentUser)
		{
			TestProcessRequest(expectedRedirectLocation, shouldUserBeLoggedIn, shouldBeLoggedInAsQuickShipmentUser, true);
		}

		void TestProcessRequest(string expectedRedirectLocation, bool shouldUserBeLoggedIn, bool shouldBeLoggedInAsQuickShipmentUser, bool shouldHaveApplicationCookie)
		{
			AssertEquals("Pre-condition", false, AppInstance.SiteUser.IsLoggedIn);
			AssertNull("Pre-condition", GetCurrentAuthenticationTicketFromResponse());
			AssertNull("Pre-condition", HttpContext.Current.Response.RedirectLocation);

			RequestHandler.ProcessRequest(HttpContext.Current);
			AssertEquals(shouldUserBeLoggedIn, AppInstance.SiteUser.IsLoggedIn);
			if (shouldBeLoggedInAsQuickShipmentUser)
			{
				Assert(((TrackingSiteUser)WebEnv.AppInstance.SiteUser).IsShipmentQuickViewUser);

				FormsAuthenticationTicket ticket = GetCurrentAuthenticationTicketFromResponse();
				AssertEquals(User.WebUserName, ticket.Name);
				AssertEquals(expectedRedirectLocation, HttpContext.Current.Response.RedirectLocation);
			}
			else
			{
				if (shouldUserBeLoggedIn)
				{
					AssertEquals(Contact.PK, AppInstance.SiteUser.LoggedInUser.PK);
					AssertEquals(Header.PK, ((OrgContactWebUser)AppInstance.SiteUser).LoggedInOrganisation.PK);

					FormsAuthenticationTicket ticket = GetCurrentAuthenticationTicketFromResponse();
					AssertEquals("contact@edi.com.au", ticket.Name);
					AssertEquals(expectedRedirectLocation, HttpContext.Current.Response.RedirectLocation);
					AssertNull(AppInstance.Session[Global.AutoLoginQueryStringDataIndexer]);
				}
				else
				{
					AssertEquals(shouldHaveApplicationCookie, AppInstance.ApplicationCookie.CookieExist());
					if (shouldHaveApplicationCookie)
					{
						AssertEquals("contact@edi.com.au", AppInstance.ApplicationCookie.GetUserEmail());
						AssertEquals("ORGCODE", AppInstance.ApplicationCookie.GetCompanyCode());
					}
				}
			}
		}

		AutoLoginRequestHandler fRequestHandler;
		OrgHeader fHeader;
		OrgContact fContact;
		readonly string BusinessContextPKAsString = ZGuid.NewZGuid().ToString();
		readonly string BusinessContextAdditionalRefAsString = ZGuid.NewZGuid().ToString();
		readonly string BusinessContextNK = "123456789";

		#endregion

		#region IHttpContextEnabledTestWithAppInstance Members

		ZEnterpriseGlobalBase IHttpContextEnabledTestWithAppInstance.AppInstance
		{
			get { return new GlobalForTest(); }
		}

		GlobalForTest AppInstance
		{
			get { return (GlobalForTest)HttpContext.Current.ApplicationInstance; }
		}

		class GlobalForTest : Global
		{
			public new void OnCustomSessionStart(object sender, EventArgs e)
			{
				base.OnCustomSessionStart(sender, e);
			}
		}

		#endregion

		#region IHttpContextEnabledTestWithRequestPath Members

		string IHttpContextEnabledTestWithRequestPath.RequestPath
		{
			get { return "AutoLoginRequestHandler.axd"; }
		}

		string IHttpContextEnabledTestWithRequestPath.QueryString
		{
			get
			{
				string result;

				string testName = CurrentTestName.Replace("Enterprise.Tracking.Web.Testing.AutoLoginRequestHandlerTest.", "");
				switch (testName)
				{
					case "TestProcessRequest_NotYetRedirectedForExcelHackAndHasQueryString":
						result = "QueryString=Exist";
						break;

					case "TestProcessRequest_NotYetRedirectedForExcelHack":
						result = "";
						break;

					case "TestProcessRequest_NoBusinessContext_NoContact_ShouldRequireLogin":
						result = "ClientRedirection=TRUE";
						break;

					default:
						{
							string businessContextAsString = testName
								.Replace("_ShouldUseDisposableConnection", "")
								.Replace("TestProcessRequest_", "")
								.Replace("_Number", "")
								.Replace("_PasswordRequired", "")
								.Replace("_RequireLogin", "")
								.Replace("_Module", "")
								.Replace("Home", "")
								.Replace("_EmptyContactPK", "")
								.Replace("_IfAlreadyLoggedIn_SetsSecureQueryStringIntoSession", "");

							if (testName.IndexOf("_BusinessContextKeyNull") != -1)
							{
								return string.Format("ClientRedirection=TRUE&{0}={1}", TrackingConstants.AutoLogin.SecureQueryStringDataKey, null);
							}

							ZGuid contactPK = testName.IndexOf("_EmptyContactPK") == -1 ? Contact.PK : ZGuid.Empty;
							string queryString = WebUtility.UrlEncode(testName.Contains("_Module") ? GetQueryStringForModule(contactPK, businessContextAsString, testName) : (testName.Contains("_Number") ? GetQueryStringWithNKAsString(contactPK, businessContextAsString, testName) : GetQueryStringWithPKAsString(contactPK, businessContextAsString, testName)));

							result = string.Format("ClientRedirection=TRUE&{0}={1}", TrackingConstants.AutoLogin.SecureQueryStringDataKey, queryString);
						}
						break;
				}

				return result;
			}
		}

		#endregion
	}
}
