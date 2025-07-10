using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class CartageDetailsTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.CartageDetails;
		}

		public void TestVisible()
		{
			TestCartageDetails testForm = new TestCartageDetails();
			testForm.SetupPageForTesting();
			TrackingCartage source = testForm.DataSource as TrackingCartage;
			source.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;

			testForm.ForTest_PageLoad();
			Assert(!testForm.ForTest_FirstDocAddressControl.Visible);
			Assert(!testForm.ForTest_SecondDocAddressControl.Visible);
			Assert(!testForm.ForTest_ThirdDocAddressControl.Visible);
			Assert(!testForm.ForTest_FourthDocAddressControl.Visible);

			source.FirstDocAddress.E2_AddressOverride = ZBool.True;

			testForm.ForTest_PageLoad();
			Assert(testForm.ForTest_FirstDocAddressControl.Visible);
			Assert(!testForm.ForTest_SecondDocAddressControl.Visible);
			Assert(!testForm.ForTest_ThirdDocAddressControl.Visible);
			Assert(!testForm.ForTest_FourthDocAddressControl.Visible);

			source.SecondDocAddress.E2_AddressOverride = ZBool.True;
			source.ThirdDocAddress.E2_AddressOverride = ZBool.True;
			source.FourthDocAddress.E2_AddressOverride = ZBool.True;
			source.LocalClientAddressPK = Factory.New<OrgAddress>().PK;
			testForm.ForTest_PageLoad();
			Assert(testForm.ForTest_FirstDocAddressControl.Visible);
			Assert(testForm.ForTest_SecondDocAddressControl.Visible);
			Assert(testForm.ForTest_ThirdDocAddressControl.Visible);
			Assert(testForm.ForTest_FourthDocAddressControl.Visible);
		}

		public void TestVisibleForLegs()
		{
			bool oldvalue = WebDataRegistry.Instance.TransportShowAllLegs.Value;
			WebDataRegistry.Instance.TransportShowAllLegs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestCartageDetails testForm = new TestCartageDetails();
			testForm.SetupPageForTesting();
			testForm.ForTest_PageLoad();
			Assert(!testForm.ForTest_ZcollapsablepanelLegs.Visible);

			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			TrackingCartage cartage1 = testForm.DataSource as TrackingCartage;
			cartage1.JJ_GB = branch.PK;
			CommonBookedCtgMove booked1 = cartage1.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();

			cartage1.JJ_RS_NKServiceLevel = "abc";
			Factory.Save();
			cartage1.NullFilteredCartageLegsCollection();
			testForm.ForTest_PageLoad();
			Assert(testForm.ForTest_ZcollapsablepanelLegs.Visible);
			WebDataRegistry.Instance.TransportShowAllLegs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldvalue);
		}

		protected override Control GetNewControl()
		{
			return new TestCartageDetails();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebCartageModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebCartageView; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			((TestCartageDetails)TestPage).SetupPageForTesting();
		}

		public class TestCartageDetails : CartageDetails
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}

			public void SetupPageForTesting()
			{
				UnauthorisedDiv = new HtmlGenericControl();
				UnauthorisedLabel = new ZTextLabel();
				AuthorisedContent = new HtmlGenericControl();
				FirstDocAddressControl = new ZDocAddressWebControl();
				SecondDocAddressControl = new ZDocAddressWebControl();
				ThirdDocAddressControl = new ZDocAddressWebControl();
				FourthDocAddressControl = new ZDocAddressWebControl();

				MainJobPanel = new HtmlTableRow();
				AreaGrossWeight = new HtmlTableRow();
				ContainersDataGrid = new ZGrid();
				LooseBookingGrid = new ZGrid();
				LegsDataGrid = new ZGrid();

				LoadOrCreateDataSource();
			}

			#region ControlsForTest

			public Control ForTest_ZcollapsablepanelLegs
			{
				get { return LegsDataGrid; }
			}

			public ZDocAddressWebControl ForTest_FirstDocAddressControl
			{
				get { return FirstDocAddressControl; }
			}

			public ZDocAddressWebControl ForTest_SecondDocAddressControl
			{
				get { return SecondDocAddressControl; }
			}

			public ZDocAddressWebControl ForTest_ThirdDocAddressControl
			{
				get { return ThirdDocAddressControl; }
			}

			public ZDocAddressWebControl ForTest_FourthDocAddressControl
			{
				get { return FourthDocAddressControl; }
			}

			#endregion

			public void ForTest_PageLoad()
			{
				Page_Load(this, EventArgs.Empty);
			}

			protected override BusinessObject GetNewDataSource()
			{
				return Factory.New<TrackingCartage>();
			}
		}
	}
}
