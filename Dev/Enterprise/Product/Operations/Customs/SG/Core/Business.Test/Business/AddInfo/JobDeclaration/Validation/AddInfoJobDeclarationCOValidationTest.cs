using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobDeclarationCOValidationTest : AddInfoJobDeclarationValidationTest
	{
		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.COO;
			}
		}

		public void TestCountryofFinalDestination()
		{
			Validation.ValidateSG_RN_NKFinalDestination();
			AssertEquals(true, AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_RN_NKFinalDestination = Core.Constants.CountryCodes.Bulgaria;
			Validation.ValidateSG_RN_NKFinalDestination();
			AssertEquals(false, AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo.HasMessageErrors());
		}

		public void TestCertificateTypes()
		{
			var message = $"You have not entered a Certificate Type.";
			Validation.ValidateSG_Cert1Type();
			AssertHasMessageError(AddInfoJobDeclaration.SG_Cert1TypeInfo, message);
			AddInfoJobDeclaration.SG_Cert1Type = "1";
			Validation.ValidateSG_Cert1Type();
			AssertNoMessageError(AddInfoJobDeclaration.SG_Cert1TypeInfo, message);
			Validation.ValidateSG_Cert2Type();
			AssertNoMessageError(AddInfoJobDeclaration.SG_Cert2TypeInfo, message);
			message = "2 - (GSP Form A under Cumulative ASEAN) is no longer allowed by SG Customs.";
			AddInfoJobDeclaration.SG_Cert2Type = "2";
			Validation.ValidateSG_Cert2Type();
			AssertHasMessageError(AddInfoJobDeclaration.SG_Cert2TypeInfo, message);
			message = "Cert. Type 2 should be different to Cert. Type 1";
			AddInfoJobDeclaration.SG_Cert2Type = "1";
			Validation.ValidateSG_Cert2Type();
			AssertHasMessageError(AddInfoJobDeclaration.SG_Cert2TypeInfo, message);
			AddInfoJobDeclaration.SG_Cert2Type = "5";
			Validation.ValidateSG_Cert2Type();
			AssertNoMessageError(AddInfoJobDeclaration.SG_Cert2TypeInfo, message);
		}

		public void TestApplicationProductType()
		{
			Validation.ValidateSG_ApplicationProductType();
			AssertEquals(true, AddInfoJobDeclaration.SG_ApplicationProductTypeInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			Validation.ValidateSG_ApplicationProductType();
			AssertEquals(false, AddInfoJobDeclaration.SG_ApplicationProductTypeInfo.HasMessageErrors());
		}

		public void TestCertSendInvDetails()
		{
			AddInfoJobDeclaration.SG_Cert1Type = "4";
			Validation.ValidateSG_CertSendInvDetails();
			AssertEquals("Not required for this certificate type", false, AddInfoJobDeclaration.SG_CertSendInvDetailsInfo.HasWarnings());
			AddInfoJobDeclaration.SG_Cert1Type = "1";
			AssertEquals("Required for this certificate type = should have defaulted this check box indicator", true, AddInfoJobDeclaration.SG_CertSendInvDetails);
			Validation.ValidateSG_ApplicationProductType();
			AssertEquals(false, AddInfoJobDeclaration.SG_ApplicationProductTypeInfo.HasWarnings());
			AddInfoJobDeclaration.SG_Cert1Type = "4";
			AddInfoJobDeclaration.SG_CertSendInvDetails = true;
			Validation.ValidateSG_CertSendInvDetails();
			AssertEquals("Not required for this certificate type - indicator overridden to send details - warning provided", true, AddInfoJobDeclaration.SG_CertSendInvDetailsInfo.HasWarnings());
			AddInfoJobDeclaration.SG_Cert1Type = "1";
			Validation.ValidateSG_ApplicationProductType();
			AssertEquals(false, AddInfoJobDeclaration.SG_ApplicationProductTypeInfo.HasWarnings());
		}
	}
}
