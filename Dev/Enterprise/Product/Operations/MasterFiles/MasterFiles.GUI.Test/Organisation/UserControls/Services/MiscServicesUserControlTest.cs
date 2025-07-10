using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(MiscServicesUserControl))]
	sealed class MiscServicesUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new MiscServicesUserControl();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyServices" }; }
		}

		[RequiresSTA]
		public void TestSetContainerYardCarrierRelatedPartyControl_Visibility()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_IsContainerYard = true;
			using (ZForm form = new ZForm(orgHeader))
			using (MiscServicesUserControl control = new MiscServicesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(true, control.OH_IsContainerYardBoundCheckBox.Visible);
				AssertEquals(true, control.ContainerYardCarrierRelatedPartyControl.Visible);

				control.OH_IsContainerYardBoundCheckBox.Checked = false;
				AssertEquals(false, control.ContainerYardCarrierRelatedPartyControl.Visible);
			}
		}

		public void TestSetMiscServiceCTOStorageUserControl_Visibility()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_IsRailHead = false;
			orgHeader.OH_IsSeaCTO = true;
			using (ZForm form = new ZForm(orgHeader))
			using (MiscServicesUserControl control = new MiscServicesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(true, control.OH_IsSeaCTOBoundCheckBox.Visible);
				AssertEquals(true, control.MiscServiceCTOStorageUserControl.Visible);

				control.OH_IsSeaCTOBoundCheckBox.Checked = false;
				AssertEquals(false, control.MiscServiceCTOStorageUserControl.Visible);
			}

			orgHeader.OH_IsRailHead = true;
			orgHeader.OH_IsSeaCTO = false;
			using (ZForm form = new ZForm(orgHeader))
			using (MiscServicesUserControl control = new MiscServicesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(true, control.RailHeadCheckBox.Visible);
				AssertEquals(true, control.MiscServiceCTOStorageUserControl.Visible);

				control.RailHeadCheckBox.Checked = false;
				AssertEquals(false, control.MiscServiceCTOStorageUserControl.Visible);
			}
		}

		[RequiresSTA]
		public void TestSetMiscServiceFacilityUserControl_Visibility()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_IsAirCTO = false;
			orgHeader.OH_IsSeaCTO = false;
			orgHeader.OH_IsRailHead = false;
			orgHeader.OH_IsRoadFreightDepot = false;
			orgHeader.OH_IsContainerYard = false;
			orgHeader.OH_IsPackDepot = false;
			orgHeader.OH_IsUnpackDepot = false;

			using (ZForm form = new ZForm(orgHeader))
			using (MiscServicesUserControl control = new MiscServicesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(true, control.OH_IsAirCTOBoundCheckBox.Visible);
				AssertEquals(true, control.OH_IsSeaCTOBoundCheckBox.Visible);
				AssertEquals(true, control.RailHeadCheckBox.Visible);
				AssertEquals(true, control.RoadFreightDepotCheckBox.Visible);
				AssertEquals(true, control.OH_IsContainerYardBoundCheckBox.Visible);
				AssertEquals(true, control.OH_IsPackDepotBoundCheckBox.Visible);
				AssertEquals(true, control.OH_IsUnpackDepotBoundCheckBox.Visible);
				AssertEquals(false, control.MiscServiceFacilityUserControl.Visible);

				control.OH_IsAirCTOBoundCheckBox.Checked = true;
				AssertEquals(true, control.MiscServiceFacilityUserControl.Visible);

				control.OH_IsAirCTOBoundCheckBox.Checked = false;
				control.OH_IsSeaCTOBoundCheckBox.Checked = true;
				AssertEquals(true, control.MiscServiceFacilityUserControl.Visible);

				control.OH_IsSeaCTOBoundCheckBox.Checked = false;
				control.RailHeadCheckBox.Checked = true;
				AssertEquals(true, control.MiscServiceFacilityUserControl.Visible);

				control.RailHeadCheckBox.Checked = false;
				control.RoadFreightDepotCheckBox.Checked = true;
				AssertEquals(true, control.MiscServiceFacilityUserControl.Visible);

				control.RoadFreightDepotCheckBox.Checked = false;
				control.OH_IsContainerYardBoundCheckBox.Checked = true;
				AssertEquals(true, control.MiscServiceFacilityUserControl.Visible);

				control.OH_IsContainerYardBoundCheckBox.Checked = false;
				control.OH_IsPackDepotBoundCheckBox.Checked = true;
				AssertEquals(true, control.MiscServiceFacilityUserControl.Visible);

				control.OH_IsPackDepotBoundCheckBox.Checked = false;
				control.OH_IsUnpackDepotBoundCheckBox.Checked = true;
				AssertEquals(true, control.MiscServiceFacilityUserControl.Visible);

				control.OH_IsUnpackDepotBoundCheckBox.Checked = false;
				AssertEquals(false, control.MiscServiceFacilityUserControl.Visible);
			}
		}
	}
}
