using System;
using System.Security.Principal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class WebTrackerLogonControllerTest : TransactionedTestCase
	{
		public void TestLogonUrl()
		{
			using (WebDataRegistry.Instance.WebTrackerUrl.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, rootUrl))
			{
				identityMock.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);

				var result = GetUrlResult(moduleID.ToString(), recordID.ToString());
				var expected = TrackingUrlBuilder.BuildUrl(rootUrl, contactPK, moduleID, recordID, true);

				AssertEquals(expected, result);
			}
		}

		public void TestLogonUrl_RecordID_NK()
		{
			using (WebDataRegistry.Instance.WebTrackerUrl.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, rootUrl))
			{
				identityMock.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);

				var result = GetUrlResult(moduleID.ToString(), "naturalKey");
				var expected = TrackingUrlBuilder.BuildUrl(rootUrl, contactPK, moduleID, "naturalKey", true);

				AssertEquals(expected, result);
			}
		}

		public void TestLogonUrl_NoRootUrl()
		{
			using (WebDataRegistry.Instance.WebTrackerUrl.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, null))
			{
				identityMock.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);

				var result = GetUrlResult(moduleID.ToString(), recordID.ToString());

				AssertEquals(string.Empty, result);
			}
		}

		public void TestLogonUrl_NoContactPK()
		{
			using (WebDataRegistry.Instance.WebTrackerUrl.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, rootUrl))
			{
				identityMock.SetupGet(x => x.ProviderType).Returns(GlbStaffSchema.Constants.Prefix);

				var result = GetUrlResult(moduleID.ToString(), recordID.ToString());
				var expected = TrackingUrlBuilder.BuildUrl(rootUrl, Guid.Empty, moduleID, recordID, true);

				AssertEquals(expected, result);
			}
		}

		public void TestLogonUrl_NoBusinessContext()
		{
			using (WebDataRegistry.Instance.WebTrackerUrl.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, rootUrl))
			{
				identityMock.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);

				var result = GetUrlResult(null, recordID.ToString());
				var expected = TrackingUrlBuilder.BuildUrl(rootUrl, contactPK, TrackingConstants.BusinessContext.NoBusinessContext, recordID, true);

				AssertEquals(expected, result);
			}
		}

		public void TestLogonUrl_NoRecordID()
		{
			using (WebDataRegistry.Instance.WebTrackerUrl.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, rootUrl))
			{
				identityMock.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);

				var result = GetUrlResult(moduleID.ToString());
				var expected = TrackingUrlBuilder.BuildUrl(rootUrl, contactPK, moduleID, null, true);

				AssertEquals(expected, result);
			}
		}

		protected string GetUrlResult(string moduleID, string recordID = null)
		{
			var response = controller.GetUrl(moduleID, recordID);

			return response.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
		}

		protected override void SetUp()
		{
			controller = new WebTrackerLogonController();
			contactPK = Guid.NewGuid();
			moduleID = TrackingConstants.BusinessContext.Shipment;
			recordID = Guid.NewGuid();

			identityMock = new Mock<IGlowAuthenticationTicketIdentity>();
			identityMock.SetupGet(x => x.IsAuthenticated).Returns(true);
			identityMock.SetupGet(x => x.ProviderKey).Returns(contactPK);
			controller.User = new GenericPrincipal(identityMock.Object, null);
		}
		WebTrackerLogonController controller;
		readonly string rootUrl = "http://localhost/Tracking";
		Guid contactPK;
		TrackingConstants.BusinessContext moduleID;
		Guid recordID;
		Mock<IGlowAuthenticationTicketIdentity> identityMock;
	}
}
