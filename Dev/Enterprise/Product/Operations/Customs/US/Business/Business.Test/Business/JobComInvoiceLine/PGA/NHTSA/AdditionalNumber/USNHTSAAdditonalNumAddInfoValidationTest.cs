using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USNHTSAAdditonalNumAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_NHTAdditionalIdentityNumQualifier()
		{
			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			AdditionalNum.US_NHTAdditionalIdentityNumQualifier = string.Empty;
			AssertHasMessageErrorContaining(AdditionalNum.US_NHTAdditionalIdentityNumQualifierInfo, MandatoryValidation.YouHaveNotEntered);

			AdditionalNum.US_NHTAdditionalIdentityNumQualifier = "XXX";
			AssertHasMessageErrorContaining(AdditionalNum.US_NHTAdditionalIdentityNumQualifierInfo, ListValidation.InvalidCodeMessageError);

			AdditionalNum.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			AssertNoMessageErrors(AdditionalNum.US_NHTAdditionalIdentityNumQualifierInfo);
		}

		public void TestCheckUS_NHTAdditionalIdentityNumber()
		{
			AdditionalNum.AddInfo.Validation.ValidateUS_NHTAdditionalIdentityNumber();
			AssertNoMessageErrors(AdditionalNum.US_NHTAdditionalIdentityNumberInfo);

			AdditionalNum.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			AdditionalNum.AddInfo.Validation.ValidateUS_NHTAdditionalIdentityNumber();
			AssertHasWarningContaining(AdditionalNum.US_NHTAdditionalIdentityNumberInfo, MandatoryValidation.YouHaveNotEntered);

			header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._04;
			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			AdditionalNum.US_NHTAdditionalIdentityNumber = "XXX";
			AssertHasMessageErrorContaining(AdditionalNum.US_NHTAdditionalIdentityNumberInfo, ValidationConstants.NHTSA.InvalidAdditionalNumberFormat);

			AdditionalNum.US_NHTAdditionalIdentityNumber = "SALLDHMV2AA100000";
			AssertNoMessageErrors(AdditionalNum.US_NHTAdditionalIdentityNumberInfo);

			var newAdditionalNum = DetailsLine.AdditionalNumbers.AddNew();
			newAdditionalNum.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			newAdditionalNum.US_NHTAdditionalIdentityNumber = "SALLDHMV2AA100000";
			AssertHasMessageErrorContaining(newAdditionalNum.US_NHTAdditionalIdentityNumberInfo, ValidationConstants.NHTSA.YouHaveEnteredMultipleNumbersWithSameTypeAndNumber);

			newAdditionalNum.US_NHTAdditionalIdentityNumber = "SALLDHMV2AA100001";
			AssertNoMessageErrors(newAdditionalNum.US_NHTAdditionalIdentityNumberInfo);
		}

		#region Implementation

		NHTSAHeader Header
		{
			get
			{
				if (header == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					header = invoiceLine.NHTSALines.AddNew();
				}

				return header;
			}
		}
		NHTSAHeader header;

		NHTSADetails DetailsLine
		{
			get { return fDetailsLine ?? (fDetailsLine = Header.NHTSADetails.AddNew()); }
		}
		NHTSADetails fDetailsLine;

		NHTSAAdditionalNum AdditionalNum
		{
			get { return fAdditionalNum ?? (fAdditionalNum = DetailsLine.FirstAdditionalNumber ?? DetailsLine.AdditionalNumbers.AddNew()); }
		}
		NHTSAAdditionalNum fAdditionalNum;

		#endregion
	}
}
