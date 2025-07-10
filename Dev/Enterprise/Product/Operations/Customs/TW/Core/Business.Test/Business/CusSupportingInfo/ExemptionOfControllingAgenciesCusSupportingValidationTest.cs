using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ExemptionOfControllingAgenciesCusSupportingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Codes.SpecialCodesForExemptionOfControllingAgencies, "Permit Exemption Codes");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.SpecialCodesForExemptionOfControllingAgencies, "SP1234", "SP1234Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.SpecialCodesForExemptionOfControllingAgencies, "XD5678", "XD5678Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var exemptionOfControllingAgenciesCusSupportings = invoiceLine.ExemptionOfControllingAgenciesCusSupportings;
			var exemptionOfControllingAgenciesCusSupporting = exemptionOfControllingAgenciesCusSupportings.AddNew();
			var targetInfo = exemptionOfControllingAgenciesCusSupporting.CSI_ReferenceNumberInfo;
			CombineAssertions("MessageErrorIfInvalidCode", () =>
			{
				var invalidCodeMessage = "The code you have selected is not in the list.";
				exemptionOfControllingAgenciesCusSupporting.CSI_ReferenceNumber = "SP1234";
				AssertNoMessageError(targetInfo, invalidCodeMessage);
				exemptionOfControllingAgenciesCusSupporting.CSI_ReferenceNumber = "XD1234";
				AssertHasMessageError(targetInfo, invalidCodeMessage);
			}

			);
			CombineAssertions("CheckPermitNumbersPlusSpecialCodeNumber", () =>
			{
				var numbersExceedFiveMessage = "The sum of the number of Import/Export Permit plus the number of Special Code for Exemption of Controlling Agencies should not exceed 5.";
				invoiceLine.PermitCusSupportingCollection.AddNew();
				invoiceLine.PermitCusSupportingCollection.AddNew();
				exemptionOfControllingAgenciesCusSupportings.AddNew();
				exemptionOfControllingAgenciesCusSupporting = exemptionOfControllingAgenciesCusSupportings.AddNew();
				targetInfo = exemptionOfControllingAgenciesCusSupporting.CSI_ReferenceNumberInfo;
				exemptionOfControllingAgenciesCusSupporting.CSI_ReferenceNumber = "XD5678";
				AssertNoMessageError(targetInfo, numbersExceedFiveMessage);
				exemptionOfControllingAgenciesCusSupporting = exemptionOfControllingAgenciesCusSupportings.AddNew();
				targetInfo = exemptionOfControllingAgenciesCusSupporting.CSI_ReferenceNumberInfo;
				exemptionOfControllingAgenciesCusSupporting.CSI_ReferenceNumber = "XD5678";
				AssertHasMessageError(targetInfo, numbersExceedFiveMessage);
			}

			);
			CombineAssertions("CheckSpecialCodeCannotBeDuplicated", () =>
			{
				var specialCodeDuplicatedMessage = ValidationConstants.CusTWControllingMessageHeader.SpecialCodeCannotBeDuplicated;
				exemptionOfControllingAgenciesCusSupportings.RemoveAndDeleteAll();
				var exemptionOfControllingAgenciesCusSupporting1 = exemptionOfControllingAgenciesCusSupportings.AddNew();
				exemptionOfControllingAgenciesCusSupporting1.CSI_ReferenceNumber = "XD5678";
				targetInfo = exemptionOfControllingAgenciesCusSupporting1.CSI_ReferenceNumberInfo;
				AssertNoMessageError(targetInfo, specialCodeDuplicatedMessage);
				var exemptionOfControllingAgenciesCusSupporting2 = exemptionOfControllingAgenciesCusSupportings.AddNew();
				exemptionOfControllingAgenciesCusSupporting2.CSI_ReferenceNumber = "XD5678";
				exemptionOfControllingAgenciesCusSupporting1.Validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageError(targetInfo, specialCodeDuplicatedMessage);
			}

			);
		}
	}
}
