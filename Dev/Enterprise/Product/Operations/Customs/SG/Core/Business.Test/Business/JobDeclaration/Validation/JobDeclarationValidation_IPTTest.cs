using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobDeclarationValidation_IPTTest : JobDeclarationValidation_InwardTest
	{
		public override void TestMessageSubType()
		{
			Declaration.JE_MessageSubType = "";
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(true, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKP;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DUT;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DNG;
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertEquals(false, Declaration.JE_MessageSubTypeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
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
			Declaration.SG_GoodsPreviouslyExemptedFromDuties = true;
			Validation.ValidateJE_VesselName();
			AssertEquals(false, Declaration.JE_VesselNameInfo.HasMessageErrors());
			Declaration.SG_GoodsPreviouslyExemptedFromDuties = false;
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals(false, Declaration.JE_TransportModeInfo.HasMessageErrors());
			Declaration.JE_TransportMode = "ABC";
			Declaration.Validation.ValidateJE_TransportMode();
			AssertEquals(true, Declaration.JE_TransportModeInfo.HasMessageErrors());
		}

		public void TestMasterBillNumber()
		{
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Validation.ValidateJE_MasterBill();
			AssertEquals(true, Declaration.JE_MasterBillInfo.HasMessageErrors());
			Declaration.JE_MasterBill = "123";
			Validation.ValidateJE_MasterBill();
			AssertEquals(false, Declaration.JE_MasterBillInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Validation.ValidateJE_MasterBill();
			AssertEquals(false, Declaration.JE_MasterBillInfo.HasMessageErrors());
			Declaration.JE_MasterBill = "";
			Validation.ValidateJE_MasterBill();
			AssertEquals(true, Declaration.JE_MasterBillInfo.HasMessageErrors());
		}

		public void TestDateOfArrival()
		{
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DNG;
			Declaration.Validation.ValidateJE_DateOfArrival();
			AssertEquals(false, Declaration.JE_DateOfArrivalInfo.HasMessageErrors());
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Declaration.Validation.ValidateJE_DateOfArrival();
			AssertEquals(true, Declaration.JE_DateOfArrivalInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Declaration.JE_DateOfArrival = ZDateTime.Now;
			AssertEquals(false, Declaration.JE_DateOfArrivalInfo.HasMessageErrors());
		}

		public void TestCheckJE_OH_InwardCarrierAgent()
		{
			string expectedMessage = MandatoryValidation.YouHaveNotEnteredMessage("Inward Carrier Agent");
			Declaration.JE_MessageType = ZString.Empty;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Declaration.JE_OH_InwardCarrierAgent = ZGuid.Empty;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, expectedMessage);
			Declaration.JE_MessageType = MessageType;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError("Validation func changes with message type", Declaration.JE_OH_InwardCarrierAgentInfo, expectedMessage);
			var org = Factory.New<OrgHeader>();
			Declaration.JE_OH_InwardCarrierAgent = org.PK;
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, expectedMessage);
			Declaration.JE_TransportMode = ZString.Empty;
			Declaration.JE_OH_InwardCarrierAgent = ZGuid.Empty;
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, expectedMessage);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, expectedMessage);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, expectedMessage);
			var inwardCarrierAgent = Factory.New<OrgHeader>();
			Declaration.JE_OH_InwardCarrierAgent = inwardCarrierAgent.PK;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, expectedMessage);
			AssertHasMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, "Inward Carrier Agent does not have a UEN reference, (set up in Organisation > Config)");
			inwardCarrierAgent.OH_IsForwarder = true;
			inwardCarrierAgent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			inwardCarrierAgent.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Declaration.InwardCarrierAgentAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(Declaration.JE_OH_InwardCarrierAgentInfo, "Inward Carrier Agent does not have a UEN reference, (set up in Organisation > Config)");
		}

		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.IPT;
			}
		}
	}
}
