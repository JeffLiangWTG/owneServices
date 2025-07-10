using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefFacilityForm))]
	sealed class RefFacilityFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new RefFacilityForm(Factory.New<RefFacility>());

		public void TestDetailsGroupBox()
		{
			var facility = Factory.NewWithValidTestData<RefFacility>();
			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			var geography = ZGeography.CreatePoint(151.1928512, -33.916385);

			facility.RFT_PostCode = "1234";
			facility.RFT_Address1 = "4321";
			facility.RFT_Address2 = "54321";
			facility.RFT_City = "SYD";
			facility.RFT_State = "NSW";
			facility.RFT_RL_NKLocationCode = unloco.RL_Code;
			facility.RFT_GeoLocation = geography;

			using (var form = new RefFacilityForm(facility))
			{
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals(facility.RFT_FacilityType, form.RFT_FacilityTypeTextBox.Text);
					AssertEquals(facility.RFT_Code, form.RFT_CodeTextBox.Text);
					AssertEquals(facility.RFT_Name, form.RFT_NameTextBox.Text);
					AssertEquals(facility.RFT_Address1, form.RFT_Address1TextBox.Text);
					AssertEquals(facility.RFT_Address2, form.RFT_Address2TextBox.Text);
					AssertEquals(facility.RFT_City, form.RFT_CityTextBox.Text);
					AssertEquals(facility.RFT_PostCode, form.RFT_PostCodeTextBox.Text);
					AssertEquals(facility.RFT_StateCodeDesc.ToLower(), form.RFT_StateTextBox.Text.ToLower());
					AssertEquals(facility.RFT_UNLOCODesc, form.RFT_RL_NKLocationCodeTextBox.Text);
					AssertEquals(facility.RFT_GeoLocation.Latitude.ToString(), form.LatitudeTextBox.Text);
					AssertEquals(facility.RFT_GeoLocation.Longitude.ToString(), form.LongitudeTextBox.Text);
					AssertEquals("RFT_IsActiveCheckBox shuould be checked", true, form.RFT_IsActiveCheckBox.Checked);
				});
			}
		}

		[RequiresSTA]
		public void TestAdditionalDetailsGroupBox()
		{
			var facility = Factory.NewWithValidTestData<RefFacility>();

			facility.RFT_IsActive = true;
			facility.RFT_IsAir = true;
			facility.RFT_IsRail = false;
			facility.RFT_IsRoad = false;
			facility.RFT_IsSea = false;
			facility.RFT_IsInlandWaterway = false;
			facility.RFT_SMDGCode = "US123";

			using (var form = new RefFacilityForm(facility))
			{
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals(facility.RFT_TerminalType, Core.Constants.TransportModes.Air);
					AssertEquals(facility.RFT_SMDGCode, form.RFT_SMDGCodeTextBox.Text);
				});
			}
		}

		[RequiresSTA]
		public void TestAvailableIntegrationGroupBox()
		{
			var facility = Factory.NewWithValidTestData<RefFacility>();

			facility.RFT_ContainerAutomationAvailable = true;

			using (var form = new RefFacilityForm(facility))
			{
				form.Show();

				AssertEquals("RFT_ContainerAutomationAvailableCheckBox shuould be checked", true, form.RFT_ContainerAutomationAvailableCheckBox.Checked);
			}
		}
	}
}
