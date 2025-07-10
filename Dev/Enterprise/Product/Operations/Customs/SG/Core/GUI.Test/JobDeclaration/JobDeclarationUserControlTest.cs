using CargoWise.Windows.UI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.SG.V4.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(SGJobDeclarationUserControl))]
	sealed class JobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<SGJobDeclarationUserControl, JobDeclaration>
	{
		public void TestControlVisibility_InwardTransport()
		{
			using (var jobDeclarationForm = new TestJobDeclarationForm(Declaration))
			{
				jobDeclarationForm.Show();
				//RAI/MAI/PIP
				Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.MasterBillForSeaBoundTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.JE_MasterBillForAirBoundTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.IsInwardHandCarriedCheckBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.VesselFindBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.VoyageFlightNoBoundTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.FolioNumberTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.InwardBerthFindBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.InwardBerthFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.InwardHousebillTextBox.Visible);
				//SEA
				Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.MasterBillForSeaBoundTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.JE_MasterBillForAirBoundTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.IsInwardHandCarriedCheckBox.Visible);
				AssertEquals("Ocean Bill", jobDeclarationForm.DeclarationUserControl.MasterBillForSeaBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.VesselFindBox.Visible);
				AssertEquals("Vessel", jobDeclarationForm.DeclarationUserControl.VesselFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.VoyageFlightNoBoundTextBox.Visible);
				AssertEquals("Voyage Number", jobDeclarationForm.DeclarationUserControl.VoyageFlightNoBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.FolioNumberTextBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.InwardHousebillTextBox.Visible);
				//AIR
				Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.MasterBillForSeaBoundTextBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.JE_MasterBillForAirBoundTextBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.IsInwardHandCarriedCheckBox.Visible);
				AssertEquals("Master Bill", jobDeclarationForm.DeclarationUserControl.JE_MasterBillForAirBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.VesselFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.VoyageFlightNoBoundTextBox.Visible);
				AssertEquals("Flight No. / Aircraft Registration", jobDeclarationForm.DeclarationUserControl.VoyageFlightNoBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.InwardBerthFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.InwardHousebillTextBox.Visible);
				//ROA
				Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
				Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.MasterBillForSeaBoundTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.JE_MasterBillForAirBoundTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.IsInwardHandCarriedCheckBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.VesselFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.VoyageFlightNoBoundTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.FolioNumberTextBox.Visible);
				AssertEquals("Registration", jobDeclarationForm.DeclarationUserControl.VoyageFlightNoBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.InwardHousebillTextBox.Visible);
				//none
				Declaration.JE_TransportMode = "";
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.InwardHousebillTextBox.Visible);
			}
		}

		public void TestControlVisibility_OutwardTransport()
		{
			using (var jobDeclarationForm = new TestJobDeclarationForm(Declaration))
			{
				jobDeclarationForm.Show();
				//RAI/MAI/PIP
				Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardOceanBillTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardMasterBillForAirControl.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.IsOutwardHandCarriedCheckBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardVesselCodeFindBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardVoyageFlightTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardBerthCodeFindBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardVesselNRTCalcEdit.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardVesselNationalityFindBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardVesselTypeDropDownEdit.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardHouseBillTextBox.Visible);
				//SEA
				Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardOceanBillTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardMasterBillForAirControl.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.IsOutwardHandCarriedCheckBox.Visible);
				AssertEquals("Ocean Bill", jobDeclarationForm.DeclarationUserControl.OutwardOceanBillTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselCodeFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselNRTCalcEdit.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselNationalityFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselTypeDropDownEdit.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVoyageFlightTextBox.Visible);
				AssertEquals("Voyage", jobDeclarationForm.DeclarationUserControl.OutwardVoyageFlightTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardHouseBillTextBox.Visible);
				//AIR
				Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardOceanBillTextBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardMasterBillForAirControl.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.IsOutwardHandCarriedCheckBox.Visible);
				AssertEquals("Master Bill", jobDeclarationForm.DeclarationUserControl.OutwardMasterBillForAirControl.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardVesselCodeFindBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardVesselNRTCalcEdit.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardVesselNationalityFindBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardVesselTypeDropDownEdit.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVoyageFlightTextBox.Visible);
				AssertEquals("Flight/Rego.", jobDeclarationForm.DeclarationUserControl.OutwardVoyageFlightTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardBerthCodeFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardHouseBillTextBox.Visible);
				//ROA
				Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
				Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardOceanBillTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardMasterBillForAirControl.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.IsOutwardHandCarriedCheckBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardVesselCodeFindBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardVesselNRTCalcEdit.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardVesselNationalityFindBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardVesselTypeDropDownEdit.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVoyageFlightTextBox.Visible);
				AssertEquals("Registration", jobDeclarationForm.DeclarationUserControl.OutwardVoyageFlightTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardHouseBillTextBox.Visible);
				//none
				Declaration.SG_OutwardTransportMode = "";
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardHouseBillTextBox.Visible);
			}
		}

		public void TestControlVisibility_OutwardTransport_OnLoad()
		{
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			using (var jobDeclarationForm = new TestJobDeclarationForm(Declaration))
			{
				jobDeclarationForm.Show();
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardOceanBillTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardMasterBillForAirControl.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.IsOutwardHandCarriedCheckBox.Visible);
				AssertEquals("Ocean Bill", jobDeclarationForm.DeclarationUserControl.OutwardOceanBillTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselCodeFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselNRTCalcEdit.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselNationalityFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselTypeDropDownEdit.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVoyageFlightTextBox.Visible);
				AssertEquals("Voyage", jobDeclarationForm.DeclarationUserControl.OutwardVoyageFlightTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardBerthCodeFindBox.Visible);
			}
		}

		public void TestContainerCountControlVisibility()
		{
			using (var jobDeclarationForm = new TestJobDeclarationForm(Declaration))
			{
				jobDeclarationForm.Show();
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.JE_ContainerCountCalcEdit.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
			}
		}

		public void TestControlVisibility_VesselBerths()
		{
			using (var jobDeclarationForm = new TestJobDeclarationForm(Declaration))
			{
				Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
				jobDeclarationForm.Show();
				Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
				Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
				//Inward
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.MasterBillForSeaBoundTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.JE_MasterBillForAirBoundTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.IsInwardHandCarriedCheckBox.Visible);
				AssertEquals("Ocean Bill", jobDeclarationForm.DeclarationUserControl.MasterBillForSeaBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.VesselFindBox.Visible);
				AssertEquals("Vessel", jobDeclarationForm.DeclarationUserControl.VesselFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.VoyageFlightNoBoundTextBox.Visible);
				AssertEquals("Voyage Number", jobDeclarationForm.DeclarationUserControl.VoyageFlightNoBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.FolioNumberTextBox.Visible);
				AssertEquals("Vessel Berth is required for TN 4.0", true, jobDeclarationForm.DeclarationUserControl.InwardBerthFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.InwardHousebillTextBox.Visible);
				//Outward
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardOceanBillTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardMasterBillForAirControl.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.IsOutwardHandCarriedCheckBox.Visible);
				AssertEquals("Ocean Bill", jobDeclarationForm.DeclarationUserControl.OutwardOceanBillTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselCodeFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselNRTCalcEdit.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselNationalityFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVoyageFlightTextBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselTypeDropDownEdit.Visible);
				AssertEquals("Voyage", jobDeclarationForm.DeclarationUserControl.OutwardVoyageFlightTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Vessel Berth is required for TN 4.0", true, jobDeclarationForm.DeclarationUserControl.OutwardBerthCodeFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardHouseBillTextBox.Visible);
				Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
				//Inward
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.MasterBillForSeaBoundTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.JE_MasterBillForAirBoundTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.IsInwardHandCarriedCheckBox.Visible);
				AssertEquals("Ocean Bill", jobDeclarationForm.DeclarationUserControl.MasterBillForSeaBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.VesselFindBox.Visible);
				AssertEquals("Vessel", jobDeclarationForm.DeclarationUserControl.VesselFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.VoyageFlightNoBoundTextBox.Visible);
				AssertEquals("Voyage Number", jobDeclarationForm.DeclarationUserControl.VoyageFlightNoBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.FolioNumberTextBox.Visible);
				AssertEquals("Vessel Berth is NOT required for TN 4.1 - Field should not be shown", false, jobDeclarationForm.DeclarationUserControl.InwardBerthFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.InwardHousebillTextBox.Visible);
				//Outward
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardOceanBillTextBox.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.OutwardMasterBillForAirControl.Visible);
				AssertEquals(false, jobDeclarationForm.DeclarationUserControl.IsOutwardHandCarriedCheckBox.Visible);
				AssertEquals("Ocean Bill", jobDeclarationForm.DeclarationUserControl.OutwardOceanBillTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselCodeFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselNRTCalcEdit.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselNationalityFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVoyageFlightTextBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardVesselTypeDropDownEdit.Visible);
				AssertEquals("Voyage", jobDeclarationForm.DeclarationUserControl.OutwardVoyageFlightTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Vessel Berth is NOT required for TN 4.1 - Field should not be shown", false, jobDeclarationForm.DeclarationUserControl.OutwardBerthCodeFindBox.Visible);
				AssertEquals(true, jobDeclarationForm.DeclarationUserControl.OutwardHouseBillTextBox.Visible);
				Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
				AssertEquals("For TN4.1 JE_Folio is used for Aircraft Registration if required.", true, jobDeclarationForm.DeclarationUserControl.FolioNumberTextBox.Visible);
			}
		}

		public void TestBondedWarehouseVisibility()
		{
			//sg uses the concept of SGCPlaces for Bonded warehouse..these fields are mandatory in SG, therefore no reason to test their visibility
			using (var jobDeclarationForm = new TestJobDeclarationForm(Declaration))
			{
				jobDeclarationForm.Show();
				AssertEquals(false, jobDeclarationForm.CustomsBrokerageUserControl.DeclarationUserControlForTesting.BondedWarehouseDocAddressControl.Visible);
			}
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		sealed class TestJobDeclarationForm : JobDeclarationForm
		{
			internal TestJobDeclarationForm(JobDeclaration declaration)
				: base(declaration)
			{
			}

			internal SGJobDeclarationUserControl DeclarationUserControl => (SGJobDeclarationUserControl)CustomsBrokerageUserControl.DeclarationUserControlForTesting;
		}
	}
}
