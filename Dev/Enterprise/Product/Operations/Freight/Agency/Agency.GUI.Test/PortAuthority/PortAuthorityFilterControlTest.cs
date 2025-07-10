using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.GUI
{
	internal class PortAuthorityFilterControlTest : BaseAgencyTest
	{
		public void TestShowThirdPartyOptions()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			PortAuthority filter = new PortAuthority(voyage);
			filter.DeliverTo3rdParty = true;
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(filter))
			{
				dialog.Show();
				PortAuthorityFilterControl control = GetControl(dialog);
				AssertEquals(true, control.ShowThirdPartyOptionsForBinding);
				filter.DeliverTo3rdParty = false;
				AssertEquals(false, control.ShowThirdPartyOptionsForBinding);
			}
		}

		public void TestHideThirdPartyOptions()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			PortAuthority filter = new PortAuthority(voyage);
			filter.DeliverTo3rdParty = false;
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(filter))
			{
				dialog.Show();
				PortAuthorityFilterControl control = GetControl(dialog);
				AssertEquals(false, control.ShowThirdPartyOptionsForBinding);
				filter.DeliverTo3rdParty = true;
				AssertEquals(true, control.ShowThirdPartyOptionsForBinding);
			}
		}

		public void TestOpenError_Shipment()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			AgencyShipment shipment = NewShipment(null, null, false, true);
			Factory.Save();
			PortAuthority filter = new PortAuthority(voyage);
			PortMessageIssue issue = filter.Issues.AddNew(shipment.PK, JobShipmentSchema.Constants.Prefix, "Random Text", "Detail");
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(filter))
			{
				dialog.Show();
				PortAuthorityFilterControl control = GetControl(dialog);
				control.OpenFilterIssueForTesting(issue);
				using (BillOfLadingForm lastShownForm = (BillOfLadingForm)control.LastControllerForTesting.LastShownForm)
				{
					AssertNotNull("Should have shown the BillOfLadingForm", lastShownForm);
					AssertEquals("Should have opened the form for editing.", ODisplayMode.Browse, lastShownForm.DisplayMode);
					BillOfLading bill = (BillOfLading)lastShownForm.BusinessEntity;
					AssertEquals("Should have opened the correct shipment", shipment.PK, bill.PK);
				}
			}
		}

		public void TestOpenError_Vessel()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Queen Anne's Revenge";
			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			Factory.Save();
			PortAuthority filter = new PortAuthority(voyage);
			PortMessageIssue issue = filter.Issues.AddNew(vessel.PK, RefVesselSchema.Constants.Prefix, "Random Text", "Detail");
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(filter))
			{
				dialog.Show();
				PortAuthorityFilterControl control = GetControl(dialog);
				control.OpenFilterIssueForTesting(issue);
				using (RefVesselForm lastShownForm = (RefVesselForm)control.LastControllerForTesting.LastShownForm)
				{
					AssertNotNull("Should have shown the RefVesselForm", lastShownForm);
					AssertEquals("Should have opened the form for editing.", ODisplayMode.Browse, lastShownForm.DisplayMode);
					RefVessel loadedVessel = (RefVessel)lastShownForm.BusinessEntity;
					AssertEquals("Should have opened the correct vessel", vessel.PK, loadedVessel.PK);
				}
			}
		}

		public void TestOpenError_Empty()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			Factory.Save();
			PortAuthority filter = new PortAuthority(voyage);
			PortMessageIssue issue = filter.Issues.AddNew(ZGuid.Empty, "", "Random Text", "Detail");
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(filter))
			{
				dialog.Show();
				PortAuthorityFilterControl control = GetControl(dialog);
				control.OpenFilterIssueForTesting(issue);
				AssertNull(control.LastControllerForTesting);
			}
		}

		#region Implementation
		PortAuthorityFilterControl GetControl(PortAuthorityFilterDialog dialog)
		{
			return (PortAuthorityFilterControl)dialog.Controls["filterControl"];
		}
		#endregion
	}
}
