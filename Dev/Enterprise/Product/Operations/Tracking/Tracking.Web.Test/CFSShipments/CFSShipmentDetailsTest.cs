using System;
using System.Collections.Generic;
using System.Web.UI;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class CFSShipmentDetailsTest : BasePageWithAuthorisationTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			Globals.IsWeb = true;
		}

		public void TestShipmentDetailsComponentsVisibiltyWhenUserIsShipmentQuickViewUser()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			using (CFSShipmentDetailsForTest page = new CFSShipmentDetailsForTest())
			{
				AssertNotNull("TestOrg", helper.TestOrg);
				AssertNotNull("TestUser", helper.TestSiteUser);
				AssertEquals("QuickViewUser is logged in", true, page.SiteUser.IsShipmentQuickViewUser);

				TrackingCFSShipment testShipment = Factory.NewWithValidTestData<TrackingCFSShipment>();
				testShipment.JS_UniqueConsignRef = "test";
				testShipment.ConsigneeDeliveryAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				testShipment.SiteUser = helper.TestSiteUser;
				testShipment.JS_JS_ColoadMasterShipment = ZGuid.NewZGuid();
				page.TestShipment = testShipment;
				page.SetupPageForTest();

				page.ForTest_RunOnLoad();
				page.ForTest_RunOnPreBind();

				Assert(!page.TransportGridForTest.ShouldShowControl);
				Assert(!page.PackLinesGridForTest.ShouldShowControl);
				Assert(!testShipment.JS_JS_ColoadMasterShipment.IsEmpty);

				Assert(!page.ClientRefLabelForTest.Visible);
				Assert(!page.ClientReftextlabelForTest.Visible);

				Assert(!page.InterimReceiptLabelForTest.Visible);
				Assert(!page.InterimReceipttextlabelForTest.Visible);

				Assert(!page.EntryNoLabelForTest.Visible);
				Assert(!page.EntryNotextlabelForTest.Visible);

				Assert(!page.WhsLocationLabelForTest.Visible);
				Assert(!page.WhsLocationZcodefindboxlabelForTest.Visible);

				Assert(!page.MasterBillLabelForTest.Visible);
				Assert(!page.MasterBilltextLabelForTest.Visible);
			}
		}

		public void TestAdditionalTermsVisibility()
		{
			WebDataRegistry.Instance.UseWebCFSShipmentsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			using (CFSShipmentDetailsForTest page = new CFSShipmentDetailsForTest())
			{
				TrackingCFSShipment testShipment = Factory.NewWithValidTestData<TrackingCFSShipment>();
				page.TestShipment = testShipment;
				page.SetupPageForTest();

				testShipment.JS_RL_NKOrigin = "USORD";
				testShipment.JS_RL_NKDestination = "USLAX";

				Assert("Shipment should be domestic", testShipment.IsDomesticFreight);
				page.ForTest_RunOnLoad();

				Assert("Additional Terms should NOT be visible for Domestic Shipments", !page.AdditionalTermsRowForTest.Visible);
				AssertEquals("Term should be called Payment Term", "Payment Term:", page.PayTermLabelForTest.Text);

				testShipment.JS_RL_NKDestination = "AUSYD";

				Assert("Shipment should NOT be domestic", !testShipment.IsDomesticFreight);
				page.ForTest_RunOnLoad();

				Assert("Additional Terms should be visible for non-Domestic Shipments", page.AdditionalTermsRowForTest.Visible);
				AssertEquals("Term should be called Incoterm", "Incoterm:", page.PayTermLabelForTest.Text);
			}
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints => new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerForwarding };

		protected override BooleanRegistryItem UseWebModule => WebDataRegistry.Instance.UseWebCFSShipmentsModule;

		protected override WebSecurityRight SiteUserSecurityRight => WebSecurityRightsList.WebCFSShipmentView;

		protected override string GetExpectedPageName() => WebTracker.Pages.CFSShipmentDetails;

		protected override Control GetNewControl()
		{
			var page = new CFSShipmentDetailsForTest();
			page.SetupPageForTest();

			return page;
		}
	}
}
