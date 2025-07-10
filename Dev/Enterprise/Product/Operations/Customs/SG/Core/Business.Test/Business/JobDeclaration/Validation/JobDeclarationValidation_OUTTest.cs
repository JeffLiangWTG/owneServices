using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobDeclarationValidation_OUTTest : CUSDECValidationTest
	{
		public override void TestMessageSubType()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.JE_MessageSubType = "";
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(true, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKO;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
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
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.REX;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(true, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
		}

		public void TestInwardTransport()
		{
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			Declaration.JE_TransportMode = "";
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals("DRT can have or not have Inward Transport Mode", false, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals("DRT can have or not have Inward Transport Mode", false, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCE;
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals("Any non DRT dec cannot have Inward Transport entered", true, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCR;
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals("Any non DRT dec cannot have Inward Transport entered", true, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.JE_TransportMode = "";
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals(false, Declaration.JE_TransportModeInfo.HasMessageErrors());
		}

		public void TestDateOfArrival()
		{
			Declaration.Validation.ValidateJE_DateOfArrival();
			AssertEquals(false, Declaration.JE_DateOfArrivalInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.Validation.ValidateJE_DateOfArrival();
			AssertEquals(true, Declaration.JE_DateOfArrivalInfo.HasMessageErrors());
			Declaration.JE_DateOfArrival = ZDateTime.Today;
			Declaration.Validation.ValidateJE_DateOfArrival();
			AssertEquals(false, Declaration.JE_DateOfArrivalInfo.HasMessageErrors());
		}

		public void TestExportDate()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.JE_ExportDate = ZDateTime.Empty;
			Declaration.Validation.ValidateJE_ExportDate();
			AssertEquals(true, Declaration.JE_ExportDateInfo.HasMessageErrors());
			Declaration.JE_ExportDate = ZDateTime.Today;
			Declaration.Validation.ValidateJE_ExportDate();
			AssertEquals(false, Declaration.JE_ExportDateInfo.HasMessageErrors());
			Declaration.SG_US_NKPlaceOfStorage = "KZ";
			Declaration.JE_ExportDate = ZDateTime.Empty;
			Declaration.Validation.ValidateJE_ExportDate();
			AssertEquals(false, Declaration.JE_ExportDateInfo.HasMessageErrors());
		}

		public void TestMessageSubtype()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.SG_IsSeaStore = true;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.SG_IsSeaStoreInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(true, Declaration.SG_IsSeaStoreInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = false;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.SG_IsSeaStoreInfo.HasMessageErrors());
		}

		public void TestConsignee()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "LW1", SGCPlaces.Constants.PremiseType.LicensedWarehouse, "LW1");
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
			Declaration.SG_US_NKPlaceOfCargoRelease = "LW1";
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(true, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.JE_OH_Consignee = consignee.PK;
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.JE_OH_Consignee = ZGuid.Empty;
			AssertEquals(true, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.SG_US_NKPlaceOfStorage = SGCPlaces.Constants.FreeTradeZones.ChangiFTZ;
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.SG_US_NKPlaceOfStorage = "";
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(true, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = true;
			Declaration.Validation.ValidateJE_OH_Consignee();
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
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

		public void TestCheckJE_OH_InwardCarrierAgent_OUT()
		{
			var messageError = "You have not entered an Inward Carrier Agent.";
			Declaration.JE_OH_InwardCarrierAgent = ZGuid.Empty;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
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
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCS;
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

		public void TestOutwardShippingLineForwarder()
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

		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.OUT;
			}
		}
	}
}
