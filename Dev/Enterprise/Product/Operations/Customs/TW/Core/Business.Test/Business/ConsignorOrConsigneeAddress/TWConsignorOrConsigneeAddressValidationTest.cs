using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWConsignorOrConsigneeAddressValidation))]
	sealed class TWConsignorOrConsigneeAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckE2_GovRegNum()
		{
			var vatMessage = "The Taiwan VAT number must be an 8-digit number.";
			var pidMessage = "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10.";
			var pasMessage = "The length of PAS (Passport Number) shouldn't be more than 14.";
			var fffMessage = "Bonded ID for the foreign company must be 'FFF' + Bonded ID.";
			var normalMessage = "The length of Number shouldn't be more than 14.";
			var targetInfo = Address.E2_GovRegNumInfo;
			Address.E2_GovRegNumType = OrgCusCode.CodeTypes.VATCode;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			var twOrg = Factory.NewWithValidTestData<OrgHeader>();
			var twOrgMainAddress = twOrg.MainAddress;
			twOrgMainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var usOrg = Factory.NewWithValidTestData<OrgHeader>();
			var usOrgMainAddress = usOrg.MainAddress;
			usOrgMainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			CombineAssertions("Validate TW Organization", () =>
			{
				Address.E2_OA_Address = twOrgMainAddress.PK;
				Address.E2_GovRegNumType = OrgCusCode.CodeTypes.VATCode;
				Address.E2_GovRegNum = "VAT123456789012";
				AssertHasMessageError(targetInfo, vatMessage);
				AssertNoMessageError(targetInfo, normalMessage);
				Address.E2_GovRegNum = "12345678";
				AssertNoMessageErrorContaining(targetInfo, vatMessage);

				Address.E2_GovRegNumType = TaiwanCodeTypes.PID;
				Address.E2_GovRegNum = "PID123456789012";
				AssertHasMessageError(targetInfo, pidMessage);
				AssertNoMessageError(targetInfo, normalMessage);
				Address.E2_GovRegNum = "0123456789";
				AssertNoMessageError(targetInfo, pidMessage);

				Address.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
				Address.E2_GovRegNum = "PAS123456789012";
				AssertHasMessageError(targetInfo, pasMessage);
				AssertNoMessageError(targetInfo, normalMessage);
				Address.E2_GovRegNum = "01234567890123";
				AssertNoMessageErrorContaining(targetInfo, pasMessage);

				Address.E2_GovRegNumType = Constants.CCPPrefix;
				Address.E2_GovRegNum = "FFF123456789012";
				AssertHasMessageError(targetInfo, normalMessage);
				AssertNoMessageError(targetInfo, fffMessage);
				Address.E2_GovRegNum = "12345678901234";
				AssertNoMessageError(targetInfo, normalMessage);
			});

			CombineAssertions("Validate Foreign Organization", () =>
			{
				Address.E2_OA_Address = usOrgMainAddress.PK;
				Address.E2_GovRegNum = "123456789012345";
				AssertHasMessageError(targetInfo, fffMessage);
				AssertNoMessageError(targetInfo, normalMessage);
				Address.E2_GovRegNum = "FFF123";
				AssertNoMessageError(targetInfo, fffMessage);

				Address.E2_GovRegNumType = OrgCusCode.CodeTypes.VATCode;
				Address.E2_GovRegNum = "VAT123456789012";
				AssertHasMessageError(targetInfo, normalMessage);
				AssertNoMessageError(targetInfo, vatMessage);

				Address.E2_GovRegNumType = TaiwanCodeTypes.PID;
				Address.E2_GovRegNum = "PID123456789012";
				AssertHasMessageError(targetInfo, normalMessage);
				AssertNoMessageError(targetInfo, pidMessage);

				Address.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
				Address.E2_GovRegNum = "PAS123456789012";
				AssertHasMessageError(targetInfo, normalMessage);
				AssertNoMessageError(targetInfo, pasMessage);
			});

			CombineAssertions("Validate Empty Organization", () =>
			{
				Address.E2_OA_Address = ZGuid.Empty;
				Address.E2_GovRegNumType = Constants.CCPPrefix;
				Address.E2_GovRegNum = "123456789012345";
				AssertHasMessageError(targetInfo, normalMessage);
				AssertNoMessageError(targetInfo, fffMessage);

				Address.E2_GovRegNumType = OrgCusCode.CodeTypes.VATCode;
				Address.E2_GovRegNum = "VAT123456789012";
				AssertHasMessageError(targetInfo, normalMessage);
				AssertNoMessageError(targetInfo, vatMessage);

				Address.E2_GovRegNumType = TaiwanCodeTypes.PID;
				Address.E2_GovRegNum = "PID123456789012";
				AssertHasMessageError(targetInfo, normalMessage);
				AssertNoMessageError(targetInfo, pidMessage);

				Address.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
				Address.E2_GovRegNum = "PAS123456789012";
				AssertHasMessageError(targetInfo, normalMessage);
				AssertNoMessageError(targetInfo, pasMessage);
			});
		}

		public void TestCheckE2_GovRegNumType()
		{
			var targetInfo = Address.E2_GovRegNumTypeInfo;
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "XXX", OrgCusCode.CodeTypes.VATCode);

			var validation = Address.Validation;
			Address.E2_GovRegNumType = ZString.Empty;
			Address.E2_GovRegNum = "12345678901234";
			validation.ValidateE2_GovRegNumType();
			AssertHasMessageErrorContaining(targetInfo, "You have not entered");

			Address.E2_GovRegNumType = OrgCusCode.CodeTypes.VATCode;
			Address.E2_GovRegNum = "12345678901234";
			validation.ValidateE2_GovRegNumType();
			AssertNoMessageErrorContaining(targetInfo, "You have not entered");
		}

		JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
		JobDeclaration declaration;
		TWConsignorOrConsigneeAddress Address => address ??= Declaration.ConsignorDocumentaryAddress;
		TWConsignorOrConsigneeAddress address;
	}
}
