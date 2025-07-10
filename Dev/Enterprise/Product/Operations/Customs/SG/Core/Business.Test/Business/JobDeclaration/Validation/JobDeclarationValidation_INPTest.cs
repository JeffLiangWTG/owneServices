using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobDeclarationValidation_INPTest : JobDeclarationValidation_InwardTest
	{
		public override void TestMessageSubType()
		{
			Declaration.JE_MessageSubType = "";
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(true, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.SFZ;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GTR;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.SHO;
			Declaration.TradersRemarks.AddNew();
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DES;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.REX;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCS;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCR;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCE;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCO;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCI;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = "ABC";
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(true, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(true, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
		}

		public void TestInwardTransport()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "LW1", SGCPlaces.Constants.PremiseType.LicensedWarehouse, "LW1");
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "BWCY1", SGCPlaces.Constants.PremiseType.BondedWarehouseClass2Yard, "BWCY1");
			Factory.Save();
			Declaration.JE_TransportMode = "";
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals(true, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.SG_US_NKPlaceOfCargoRelease = "LW1";
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals(false, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.SG_US_NKPlaceOfCargoRelease = "BWCY1";
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals(true, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.SG_US_NKPlaceOfCargoRelease = "";
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals(false, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.SHO;
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals(false, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCO;
			Declaration.SG_GoodsPreviouslyExemptedFromDuties = true;
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals(false, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.SG_GoodsPreviouslyExemptedFromDuties = false;
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals(false, Declaration.JE_TransportModeInfo.HasMessageErrors());
		}

		public void TestTradersRemarks()
		{
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.SFZ;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.SHO;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertHasMessageError(Declaration.JE_MessageSubTypeInfo, "Traders Remarks are required for INP SHO declarations");
			Declaration.TradersRemarks.AddNew();
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
		}

		public void TestSubType()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "BW1", SGCPlaces.Constants.PremiseType.BondedWarehouse, "BW1");
			Factory.Save();
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKP;
			Declaration.SG_US_NKPlaceOfReceipt = "BW1";
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(true, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
		}

		public void TestExporter()
		{
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Validation.ValidateJE_OH_Exporter();
			AssertEquals(false, Declaration.JE_OH_ExporterInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.REX;
			Validation.ValidateJE_OH_Exporter();
			AssertEquals(true, Declaration.JE_OH_ExporterInfo.HasMessageErrors());
			OrgHeader exporter = Factory.New<OrgHeader>();
			exporter.OH_IsForwarder = true;
			exporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			exporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.JE_OH_Exporter = exporter.PK;
			Validation.ValidateJE_OH_Exporter();
			AssertEquals(false, Declaration.JE_OH_ExporterInfo.HasMessageErrors());
		}

		public void TestSG_OH_Consignee()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.SFZ;
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(true, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = true;
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = false;
			Declaration.AddInfoValidation.Parent.SG_US_NKPlaceOfStorage = SGCPlaces.Constants.FreeTradeZones.ChangiFTZ;
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.AddInfoValidation.Parent.SG_US_NKPlaceOfStorage = "";
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(true, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			consignee.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.JE_OH_Consignee = consignee.PK;
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
		}

		public void TestCheckJE_OH_Claimant()
		{
			var claimant = Factory.New<OrgHeader>();
			claimant.OH_Code = "CLAIMTST";
			Factory.Save();
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.SFZ;
			Declaration.JE_OH_Claimant = ZGuid.Empty;
			AssertNoMessageErrors("Empty Claimant should not error for most Dec types", Declaration.JE_OH_ClaimantInfo);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GTR;
			Declaration.ClaimantAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(Declaration.JE_OH_ClaimantInfo, AddInfoJobDeclarationValidation_INP.ClaimantRequiredForGTR);
			claimant.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.JE_OH_Claimant = claimant.PK;
			AssertNoMessageError(Declaration.JE_OH_ClaimantInfo, AddInfoJobDeclarationValidation_INP.ClaimantRequiredForGTR);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Declaration.ClaimantAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("Claimant details can be entered for Dec type BKT", Declaration.JE_OH_ClaimantInfo, AddInfoJobDeclarationValidation_INP.ClaimantDetailsMessage);
			Declaration.JE_OH_Claimant = ZGuid.Empty;
			AssertNoMessageError("Claimant details can also be left blank for Dec type BKT", Declaration.JE_OH_ClaimantInfo, AddInfoJobDeclarationValidation_INP.ClaimantDetailsMessage);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			Declaration.JE_OH_Claimant = claimant.PK;
			AssertHasMessageError(Declaration.JE_OH_ClaimantInfo, AddInfoJobDeclarationValidation_INP.ClaimantDetailsMessage);
			Declaration.JE_OH_Claimant = ZGuid.Empty;
			AssertNoMessageError(Declaration.JE_OH_ClaimantInfo, AddInfoJobDeclarationValidation_INP.ClaimantDetailsMessage);
		}

		public void TestInwardCarrierAgent()
		{
			var messageError = "You have not entered an Inward Carrier Agent.";
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.JE_OH_InwardCarrierAgent = ZGuid.Empty;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, messageError);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
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
			OrgHeader agent = Factory.New<OrgHeader>();
			Declaration.JE_OH_InwardCarrierAgent = agent.PK;
			AssertHasMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, "Inward Carrier Agent does not have a UEN reference, (set up in Organisation > Config)");
			agent.OH_IsForwarder = true;
			agent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			agent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, "Inward Carrier Agent does not have a UEN reference, (set up in Organisation > Config)");
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.REX;
			Declaration.JE_TransportMode = "";
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertEquals(false, Declaration.JE_OH_InwardCarrierAgentInfo.HasMessageErrors());
		}

		public void TestOutwardShippingLineForwarderPK_INP()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			var messageError = "You have not entered an Outward Carrier Agent.";
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.REX;
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			Declaration.SG_OutwardTransportMode = ZString.Empty;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.SG_US_NKPlaceOfStorage = SGCPlaces.Constants.PremiseType.BondedWarehouse;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			Declaration.SG_US_NKPlaceOfStorage = SGCPlaces.Constants.FreeTradeZones.ChangiFTZ;
			Declaration.OutwardShippingLineForwarderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.OutwardShippingLineForwarderPKInfo, messageError);
			OrgHeader agent = Factory.New<OrgHeader>();
			Declaration.OutwardShippingLineForwarderPK = agent.PK;
			Declaration.SG_US_NKPlaceOfStorage = ZString.Empty;
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
				return MessageTypeCodeList.Codes.INP;
			}
		}
	}
}
