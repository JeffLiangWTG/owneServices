using System.Collections.Generic;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.LinerAndAgency.Testing
{
	[HttpContextEnabledTest]
	sealed class LinerAndAgencyContainerDetailsTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.LinerAndAgencyContainerDetails;
		}

		public void TestContainerDetailsComponentsVisibiltyWhenUserIsShipmentQuickViewUser()
		{
			var helper = new TestHelper(Factory);
			helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			using (LinerAndAgencyContainerDetailsForTest page = new LinerAndAgencyContainerDetailsForTest())
			{
				AssertNotNull("TestOrg", helper.TestOrg);
				AssertNotNull("TestUser", helper.TestSiteUser);
				AssertEquals("QuickViewUser is logged in", true, page.SiteUser.IsShipmentQuickViewUser);

				var testContainer = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
				page.ContainerForTest = testContainer;
				page.ForTest_RunOnLoad();
				page.ForTest_RunOnPreBind();

				Assert(!page.DynamicCaption.Visible);
				Assert(!page.DynamicLabel.Visible);
				Assert(!page.JobNumberCaption.Visible);
				Assert(!page.JobNumberLabel.Visible);
				Assert(!page.ContainerStatusCaption.Visible);
				Assert(!page.ContainerStatusLabel.Visible);
				Assert(!page.GrossWeighCaption.Visible);
				Assert(!page.GrossWeightLabel.Visible);
				Assert(!page.PickupCaption.Visible);
				Assert(!page.PickupLabel.Visible);

				Assert(!page.DeliverCaption.Visible);
				Assert(!page.DeliverLabel.Visible);

				Assert(!page.CommodityCaption.Visible);
				Assert(!page.CommodityLabel.Visible);

				Assert(!page.GoodValueCaption.Visible);
				Assert(!page.GoodValueLabel.Visible);

				Assert(!page.CurrencyCaption.Visible);
				Assert(!page.CurrencyLabel.Visible);

				Assert(!page.NetWeightCaption.Visible);
				Assert(!page.NetWeightLabel.Visible);

				Assert(!page.TareWeightCaption.Visible);
				Assert(!page.TareWeightLabel.Visible);

				Assert(!page.IsShipperCheckBox.Visible);
				Assert(!page.ISEmptyCheckBox.Visible);
				Assert(!page.IsDamagedCheckBox.Visible);

				Assert(!page.PaymentTermCaption.Visible);
				Assert(!page.PaymentTermLabel.Visible);

				Assert(!page.ServiceLevelCaption.Visible);
				Assert(!page.ServiceLevelLabel.Visible);

				Assert(!page.ShipperRefCaption.Visible);
				Assert(!page.ShipperRefLabel.Visible);

				Assert(!page.OrderRefCaption.Visible);
				Assert(!page.OrderRefLabel.Visible);

				Assert(!page.GoodsDescriptionCaption.Visible);
				Assert(!page.GoodsDescriptionLabel.Visible);

				Assert(!page.IsControlledAtmosphereCheckBox.Visible);
				Assert(!page.IsChillerCheckBox.Visible);
				Assert(!page.IsFreezerCheckBox.Visible);

				Assert(!page.TemperatureCaption.Visible);
				Assert(!page.TemperatureLabel.Visible);

				Assert(!page.HumidityCaption.Visible);
				Assert(!page.HumidityLabel.Visible);

				Assert(!page.TempRecordCaption.Visible);
				Assert(!page.TempRecordLabel.Visible);

				Assert(!page.AirVentCaption.Visible);
				Assert(!page.AirVentLabel.Visible);

				Assert(!page.ClipOnUnitNumberCaption.Visible);
				Assert(!page.ClipOnUnitNumberLabel.Visible);

				Assert(!page.EmptyPickupFromCaption.Visible);
				Assert(!page.EmptyPickupFromLabel.Visible);

				Assert(!page.EmptyReleaseNumberCaption.Visible);
				Assert(!page.EmptyReleaseNumberLabel.Visible);

				Assert(!page.EmptyReturnedToCaption.Visible);
				Assert(!page.EmptyReturnedToLabel.Visible);

				Assert(!page.VerifiedByCompanyCaption.Visible);
				Assert(!page.VerifiedByCompanyLabel.Visible);
				Assert(!page.VerifiedByPersonCaption.Visible);
				Assert(!page.VerifiedByPersonLabel.Visible);
				Assert(!page.VerifiedByPhoneCaption.Visible);
				Assert(!page.VerifiedByPhoneLabel.Visible);
				Assert(!page.VerifiedByEmailCaption.Visible);
				Assert(!page.VerifiedByEmailLabel.Visible);

				Assert(!page.RefrigrationCaption.Visible);

				Assert(!page.DocumentsGrid.Visible);
				Assert(!page.EditContainer.Visible);
			}
		}

		#region Implementation

		protected override System.Web.UI.Control GetNewControl()
		{
			return new LinerAndAgencyContainerDetailsForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebLinerAndAgencyContainersModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebLinerAndAgencyContainers; }
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerForwarding }; }
		}

		#endregion
	}
}
