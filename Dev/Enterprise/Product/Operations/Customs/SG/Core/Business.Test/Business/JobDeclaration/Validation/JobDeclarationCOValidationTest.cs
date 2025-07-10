using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobDeclarationCOValidationTest : JobDeclarationValidationTest
	{
		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.COO;
			}
		}

		public void TestVoyageNo()
		{
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Validation.ValidateJE_VoyageFlightNo();
			AssertEquals(true, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			Validation.ValidateJE_VoyageFlightNo();
			AssertEquals(false, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.JE_VoyageFlightNo = "VV1324";
			Validation.ValidateJE_VoyageFlightNo();
			AssertEquals(false, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
		}

		public void TestInwardVessel()
		{
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Validation.ValidateJE_VesselName();
			AssertEquals(false, Declaration.JE_VesselNameInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Validation.ValidateJE_VesselName();
			AssertEquals(true, Declaration.JE_VesselNameInfo.HasMessageErrors());
			Declaration.JE_VesselName = "ADMIRALENGRACHT";
			Validation.ValidateJE_VesselName();
			AssertEquals(false, Declaration.JE_VesselNameInfo.HasMessageErrors());
		}

		public void TestDateOfDeparture()
		{
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.JE_ExportDate = ZDateTime.Empty;
			Declaration.Validation.ValidateJE_ExportDate();
			AssertEquals(true, Declaration.JE_ExportDateInfo.HasMessageErrors());
			Declaration.JE_ExportDate = ZDateTime.Today;
			Declaration.Validation.ValidateJE_ExportDate();
			AssertEquals(false, Declaration.JE_ExportDateInfo.HasMessageErrors());
		}

		public void TestPortOfDischarge()
		{
			Declaration.Lookups.SGLocoList.Load();
			Declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertEquals(false, Declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertEquals(true, Declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
			Declaration.JE_RL_NKPortOfArrival = "ZACPT";
			Declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertEquals(false, Declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
		}

		public void TestForwarder()
		{
			OrgHeader forwarder = Factory.New<OrgHeader>();
			forwarder.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "REG1234564");
			forwarder.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			forwarder.OH_IsForwarder = ZBool.True;
			Declaration.JE_OH_Forwarder = forwarder.PK;
			Validation.ValidateJE_OH_Forwarder();
			AssertEquals(false, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
			Declaration.JE_OH_Forwarder = ZGuid.Empty;
			Validation.ValidateJE_OH_Forwarder();
			AssertEquals(false, Declaration.JE_OH_ForwarderInfo.HasMessageErrors());
		}

		public void TestCheckJE_OH_Manufacturer()
		{
			Declaration.Validation.ValidateJE_OH_Manufacturer();
			AssertEquals(false, Declaration.JE_OH_ManufacturerInfo.HasMessageErrors());
			Declaration.AddInfoValidation.Parent.SG_Cert1Type = "1";
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			Declaration.JE_OH_Manufacturer = manufacturer.PK;
			Declaration.Validation.ValidateJE_OH_Manufacturer();
			AssertEquals(true, Declaration.JE_OH_ManufacturerInfo.HasMessageErrors());
			manufacturer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			manufacturer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.Validation.ValidateJE_OH_Manufacturer();
			AssertEquals(false, Declaration.JE_OH_ManufacturerInfo.HasMessageErrors());
		}

		public void TestExporter()
		{
			Declaration.AddInfoValidation.Parent.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NH;
			Declaration.Validation.ValidateJE_OH_Exporter();
			AssertEquals(true, Declaration.JE_OH_ExporterInfo.HasMessageErrors());
			OrgHeader exporter = Factory.New<OrgHeader>();
			exporter.OH_IsForwarder = true;
			exporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			exporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.JE_OH_Exporter = exporter.PK;
			Declaration.Validation.ValidateJE_OH_Exporter();
			AssertEquals(false, Declaration.JE_OH_ExporterInfo.HasMessageErrors());
		}

		public void TestInwardCarrierAgent_CO()
		{
			var messageError = "You have not entered an Inward Carrier Agent.";
			Declaration.JE_OH_InwardCarrierAgent = ZGuid.Empty;
			AssertEquals(false, Declaration.JE_OH_InwardCarrierAgentInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			var inwardCarrierAgent = Factory.New<OrgHeader>();
			Declaration.JE_OH_InwardCarrierAgent = inwardCarrierAgent.PK;
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			AssertHasMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, "Inward Carrier Agent does not have a UEN reference, (set up in Organisation > Config)");
			inwardCarrierAgent.OH_IsForwarder = true;
			inwardCarrierAgent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			inwardCarrierAgent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, "Inward Carrier Agent does not have a UEN reference, (set up in Organisation > Config)");
		}

		public void TestCheckOutwardShippingLineForwarderPK()
		{
			var messageError = "You have not entered an Outward Carrier Agent.";
			Declaration.OutwardShippingLineForwarderPK = ZGuid.Empty;
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_5_Mail;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_7_Pipeline;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			OrgHeader agent = Factory.New<OrgHeader>();
			Declaration.OutwardShippingLineForwarderPK = agent.PK;
			AssertNoMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			AssertHasMessageError(Declaration.OutwardShippingLineForwarderPKInfo, "Outward Carrier Agent does not have a UEN reference, (set up in Organisation > Config)");
			agent.OH_IsForwarder = true;
			agent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			agent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.OutwardShippingLineForwarderPKInfo, "Outward Carrier Agent does not have a UEN reference, (set up in Organisation > Config)");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			AddInfoCUSDECValidationTest.CreatePortCusCodeList(helper, "ZACPT", "CAPETOWN");
			Factory.Save();
		}
	}
}
