using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class StatusControlForCanadaImportTest : TestCaseWithFactory
	{
		public void TestSkipDataBind()
		{
			var statusControl = new CanadaImportTestStatusControl();
			AssertEquals(true, statusControl.SkipDataBind());

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.CA.IJobDeclaration>();
			statusControl.DataSource = declaration;
			declaration.JE_CustomsOffice = string.Empty;
			AssertEquals(true, statusControl.SkipDataBind());

			declaration.JE_CustomsOffice = "Some Office";
			AssertEquals(false, statusControl.SkipDataBind());
		}

		public void TestOnPreRenderOfficeControlsVisibility()
		{
			var statusControl = new CanadaImportTestStatusControl();
			statusControl.SetUpControlForTesting();

			statusControl.WrapperReleaseOfficeTextForTesting.Text = string.Empty;
			statusControl.ReleaseOfficeTextForTesting.Text = string.Empty;
			statusControl.OnPreRenderForTesting();
			Assert(!statusControl.WrapperReleaseOfficeTextForTesting.Visible);
			Assert(!statusControl.ReleaseOfficeTextForTesting.Visible);

			statusControl.ReleaseOfficeTextForTesting.Text = "Release Office";
			statusControl.OnPreRenderForTesting();
			Assert(!statusControl.WrapperReleaseOfficeTextForTesting.Visible);
			Assert(statusControl.ReleaseOfficeTextForTesting.Visible);

			statusControl.WrapperReleaseOfficeTextForTesting.Text = "Wrapper Release Office";
			statusControl.OnPreRenderForTesting();
			Assert(statusControl.WrapperReleaseOfficeTextForTesting.Visible);
			Assert(!statusControl.ReleaseOfficeTextForTesting.Visible);

			statusControl.ReleaseOfficeTextForTesting.Text = string.Empty;
			statusControl.OnPreRenderForTesting();
			Assert(statusControl.WrapperReleaseOfficeTextForTesting.Visible);
			Assert(!statusControl.ReleaseOfficeTextForTesting.Visible);

			statusControl.CCNTextForTesting.Text = string.Empty;
			statusControl.OnPreRenderForTesting();
			Assert(statusControl.EffectiveCCNTextForTesting.Visible);

			statusControl.CCNTextForTesting.Text = "CCN";
			statusControl.OnPreRenderForTesting();
			Assert(!statusControl.EffectiveCCNTextForTesting.Visible);
		}

		public void TestDeclarationDatesRow()
		{
			var statusControl = new CanadaImportTestStatusControl();
			statusControl.SetUpControlForTesting();

			var shipment = Factory.NewWithValidTestData<TrackingShipment>();
			statusControl.SetDataSourceForTest(shipment);
			statusControl.OnPreRenderForTesting();

			Assert("Row should be hidden for shipments", !statusControl.DeclarationDatesRowForTesting.Visible);

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.CA.IJobDeclaration>();
			statusControl.SetDataSourceForTest(declaration);
			statusControl.OnPreRenderForTesting();

			Assert("Row should be visible for declarations", statusControl.DeclarationDatesRowForTesting.Visible);
		}
	}
}
