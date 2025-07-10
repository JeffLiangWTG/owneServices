using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobDeclarationValidation_TNPTest : CUSDECValidationTest
	{
		public override void TestMessageSubType()
		{
			Declaration.JE_MessageSubType = "";
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(true, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TTI;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TTF;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.IGM;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.REM;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BRE;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = "ABC";
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(true, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(true, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
		}

		public void TestInwardTransportMode()
		{
			Declaration.JE_TransportMode = "";
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals(true, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BRE;
			Validation.ValidateJE_TransportMode();
			AssertEquals(false, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.REM;
			Validation.ValidateJE_TransportMode();
			AssertEquals(false, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.IGM;
			Validation.ValidateJE_TransportMode();
			AssertEquals(true, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals(false, Declaration.JE_TransportModeInfo.HasMessageErrors());
		}

		public void TestImporter()
		{
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BRE;
			Validation.ValidateJE_OH_Importer();
			AssertEquals(true, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.IGM;
			Validation.ValidateJE_OH_Importer();
			AssertEquals(true, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.REM;
			Validation.ValidateJE_OH_Importer();
			AssertEquals(true, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
			OrgHeader importer = Factory.New<OrgHeader>();
			Declaration.JE_OH_Importer = importer.PK;
			importer.OH_IsConsignee = true;
			importer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			importer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Validation.ValidateJE_OH_Importer();
			AssertEquals(false, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
		}

		public new void TestForwarder()
		{
			Declaration.JE_OH_Forwarder = ZGuid.Empty;
			Declaration.Validation.ValidateJE_OH_Forwarder();
			AssertEquals(false, Declaration.JE_OH_ForwarderInfo.HasMessageError("You have not entered a Forwarder."));
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			Declaration.JE_MessageSubType = "";
			Declaration.JE_HouseBill = "HB-001";
			Declaration.Validation.ValidateJE_OH_Forwarder();
			AssertEquals(true, Declaration.JE_OH_ForwarderInfo.HasMessageError("You have not entered a Forwarder."));
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TTF;
			Declaration.Validation.ValidateJE_OH_Forwarder();
			AssertEquals(false, Declaration.JE_OH_ForwarderInfo.HasMessageError("You have not entered a Forwarder."));
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCO;
			Declaration.JE_OH_Forwarder = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Declaration.Validation.ValidateJE_OH_Forwarder();
			AssertEquals(false, Declaration.JE_OH_ForwarderInfo.HasMessageError("You have not entered a Forwarder."));
			Declaration.JE_HouseBill = "";
			Declaration.JE_OH_Forwarder = ZGuid.Empty;
			Declaration.SG_OutwardHAWB = "OUT-HB";
			Declaration.Validation.ValidateJE_OH_Forwarder();
			AssertEquals(true, Declaration.JE_OH_ForwarderInfo.HasMessageError("You have not entered a Forwarder."));
			Declaration.JE_OH_Forwarder = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Declaration.Validation.ValidateJE_OH_Forwarder();
			AssertEquals(false, Declaration.JE_OH_ForwarderInfo.HasMessageError("You have not entered a Forwarder."));
		}

		public void TestConsignee()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			InvoiceLine.SG_IsStrategic = true;
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(true, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = true;
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = false;
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			consignee.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.JE_OH_Consignee = consignee.PK;
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.JE_OH_Consignee = ZGuid.Empty;
			InvoiceLine.SG_IsStrategic = false;
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.IGM;
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(true, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.JE_OH_Consignee = consignee.PK;
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.JE_OH_Consignee = ZGuid.Empty;
			AssertEquals(true, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.SG_US_NKPlaceOfStorage = SGCPlaces.Constants.FreeTradeZones.KeppelFTZ;
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.SG_US_NKPlaceOfStorage = "";
			Declaration.SG_IsSeaStore = true;
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = false;
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(true, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.JE_OH_Consignee = consignee.PK;
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
		}

		public void TestCheckJE_OH_InwardCarrierAgent_TNP()
		{
			var messageError = "You have not entered an Inward Carrier Agent.";
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.JE_OH_InwardCarrierAgent = ZGuid.Empty;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_5_Mail;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_7_Pipeline;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.REM;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BRE;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			OrgHeader agent = Factory.New<OrgHeader>();
			Declaration.JE_OH_InwardCarrierAgent = agent.PK;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			AssertHasMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, "Inward Carrier Agent does not have a UEN reference, (set up in Organisation > Config)");
			agent.OH_IsForwarder = true;
			agent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			agent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, "Inward Carrier Agent does not have a UEN reference, (set up in Organisation > Config)");
		}

		public void TestCheckOutwardShippingLineForwarderPK_TNP()
		{
			var messageError = "You have not entered an Outward Carrier Agent.";
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.OutwardShippingLineForwarderPK = ZGuid.Empty;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BRE;
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrors(Declaration.OutwardShippingLineForwarderPKInfo);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.REM;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrors(Declaration.OutwardShippingLineForwarderPKInfo);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			Declaration.SG_US_NKPlaceOfStorage = SGCPlaces.Constants.FreeTradeZones.KeppelFTZ;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrors(Declaration.OutwardShippingLineForwarderPKInfo);
			OrgHeader agent = Factory.New<OrgHeader>();
			Declaration.OutwardShippingLineForwarderPK = agent.PK;
			Declaration.SG_US_NKPlaceOfStorage = ZString.Empty;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			AssertHasMessageError(Declaration.OutwardShippingLineForwarderPKInfo, "Outward Carrier Agent does not have a UEN reference, (set up in Organisation > Config)");
			agent.OH_IsForwarder = true;
			agent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			agent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.OutwardShippingLineForwarderPKInfo, "Outward Carrier Agent does not have a UEN reference, (set up in Organisation > Config)");
		}

		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.TNP;
			}
		}
	}
}
