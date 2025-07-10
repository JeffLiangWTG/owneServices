using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class VNEAdditionalNumberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			var additionalNumber = AdditionalNumbers.AddNew();
			detail.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.SerialNumber;
			additionalNumber.CY_Code = ItemIdentityNumberQualifierList.Codes.SerialNumber;
			AssertNoMessageError(additionalNumber.CY_CodeInfo, VNEAdditionalNumberValidation.NumberTypeMessageText);

			additionalNumber.CY_Code = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			AssertHasMessageError(additionalNumber.CY_CodeInfo, VNEAdditionalNumberValidation.NumberTypeMessageText);

			additionalNumber.CY_Code = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			AssertNoMessageError(additionalNumber.CY_CodeInfo, VNEAdditionalNumberValidation.NumberTypeMessageText);

			detail.US_IdentityNumberQualifier = "";
			additionalNumber.CY_Code = ItemIdentityNumberQualifierList.Codes.SerialNumber;
			AssertHasWarningContaining(additionalNumber.CY_CodeInfo, "The additional number will not be sent in message.");

			detail.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.SerialNumber;
			additionalNumber.CY_Code = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			AssertNoWarningContaining(additionalNumber.CY_CodeInfo, "The additional number will not be sent in message.");
		}

		public void TestCheckCY_Data()
		{
			var additionalNumber1 = AdditionalNumbers.AddNew();

			detail.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.SerialNumber;
			detail.US_IdentityNumber = "V0000001";
			detail.US_EngineNumber = "E0000001";
			additionalNumber1.CY_Code = ItemIdentityNumberQualifierList.Codes.SerialNumber;
			additionalNumber1.CY_Data = "V0000001";
			AssertHasMessageError(additionalNumber1.CY_DataInfo, VNEAdditionalNumberValidation.NumberMessageText);
			additionalNumber1.CY_Data = "V0000002";
			AssertNoMessageError(additionalNumber1.CY_DataInfo, VNEAdditionalNumberValidation.NumberMessageText);

			var additionalNumber2 = AdditionalNumbers.AddNew();
			additionalNumber2.CY_Code = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumber2.CY_Data = "E0000001";
			AssertHasMessageError(additionalNumber2.CY_DataInfo, VNEAdditionalNumberValidation.NumberMessageText);
			additionalNumber2.CY_Data = "E0000002";
			AssertNoMessageError(additionalNumber2.CY_DataInfo, VNEAdditionalNumberValidation.NumberMessageText);

			additionalNumber2.CY_Code = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumber2.CY_Data = "0123";
			AssertHasMessageError(additionalNumber2.CY_DataInfo, VNEAdditionalNumberValidation.NumberLengthText);

			additionalNumber2.CY_Data = "01234567891234567";
			AssertNoMessageError(additionalNumber2.CY_DataInfo, VNEAdditionalNumberValidation.NumberLengthText);
		}

		VehicleDetails detail;
		VNEAdditionalNumberCollection AdditionalNumbers
		{
			get
			{
				if (additionalNumbers == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					var vne = invoiceLine.VehicleLines.AddNew();
					detail = vne.VehicleAndEngineDetails.AddNew();
					additionalNumbers = detail.AdditionalNumbers;
				}
				return additionalNumbers;
			}
		}
		VNEAdditionalNumberCollection additionalNumbers;
	}
}
