using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USAPHISLicenseAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_RN_CountryCode()
		{
			var license = Header.Licenses.AddNew();
			license.US_RN_CountryCode = "#";
			AssertNoMessageErrorContaining(license.US_RN_CountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(license.US_RN_CountryCodeInfo, ListValidation.InvalidCodeMessageError);
			license.US_RN_CountryCode = ZString.Empty;
			AssertHasMessageErrorContaining(license.US_RN_CountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(license.US_RN_CountryCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			license.AddInfoValidation.ValidateUS_RN_CountryCode();
			AssertNoMessageErrorContaining(license.US_RN_CountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(license.US_RN_CountryCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			license.AddInfoValidation.ValidateUS_RN_CountryCode();
			AssertHasMessageErrorContaining(license.US_RN_CountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(license.US_RN_CountryCodeInfo, ListValidation.InvalidCodeMessageError);
			license.US_RN_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageErrorContaining(license.US_RN_CountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(license.US_RN_CountryCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_Date()
		{
			var license = Header.Licenses.AddNew();
			license.US_Date = new ZDateTime(2015, 7, 1);
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("License Date");
			AssertNoMessageError(license.US_DateInfo, messageError);
			license.US_Date = ZDateTime.Empty;
			AssertHasMessageError(license.US_DateInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			license.AddInfoValidation.ValidateUS_Date();
			AssertNoMessageError(license.US_DateInfo, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			license.AddInfoValidation.ValidateUS_Date();
			AssertHasMessageError(license.US_DateInfo, messageError);
		}

		public void TestCheckUS_DateQualifier()
		{
			var license = Header.Licenses.AddNew();
			license.US_DateQualifier = "#";
			AssertNoMessageErrorContaining(license.US_DateQualifierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(license.US_DateQualifierInfo, ListValidation.InvalidCodeMessageError);
			license.US_DateQualifier = ZString.Empty;
			AssertHasMessageErrorContaining(license.US_DateQualifierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(license.US_DateQualifierInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			license.AddInfoValidation.ValidateUS_DateQualifier();
			AssertNoMessageErrorContaining(license.US_DateQualifierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(license.US_DateQualifierInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			license.AddInfoValidation.ValidateUS_DateQualifier();
			AssertHasMessageErrorContaining(license.US_DateQualifierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(license.US_DateQualifierInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in LPCODateQualifierList.GetListForAPHIS(Factory))
			{
				license.US_DateQualifier = pair.Code;
				AssertNoMessageErrorContaining(license.US_DateQualifierInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(license.US_DateQualifierInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_Number()
		{
			var license = Header.Licenses.AddNew();
			license.US_Number = "ABCD";
			AssertNoMessageErrors(license.US_NumberInfo);

			license.US_Number = ZString.Empty;
			AssertHasMessageError(license.US_NumberInfo, USAPHISLicenseAddInfoValidation.LicenseNumberOrNameRequired);
		}

		public void TestCheckUS_Type()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var license = Header.Licenses.AddNew();
			license.US_Type = "#";
			AssertNoMessageErrorContaining(license.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(license.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			license.US_Type = ZString.Empty;
			AssertHasMessageErrorContaining(license.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(license.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			license.AddInfoValidation.ValidateUS_Type();
			AssertNoMessageErrorContaining(license.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(license.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			license.AddInfoValidation.ValidateUS_Type();
			AssertHasMessageErrorContaining(license.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(license.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in APHISLicenseTypeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.AVS))
			{
				license.US_Type = pair.Code;
				AssertNoMessageErrorContaining(license.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(license.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_Quantity()
		{
			var license = Header.Licenses.AddNew();
			license.US_UnitOfMeasure = "KG";
			license.US_Quantity = 10m;
			var messageError = ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Quantity", "Unit Of Measure");
			AssertNoMessageError(license.US_QuantityInfo, messageError);
			license.US_Quantity = ZDecimal.Zero;
			AssertHasMessageError(license.US_QuantityInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			license.AddInfoValidation.ValidateUS_Quantity();
			AssertNoMessageError(license.US_QuantityInfo, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			license.AddInfoValidation.ValidateUS_Quantity();
			AssertHasMessageError(license.US_QuantityInfo, messageError);
			license.US_UnitOfMeasure = ZString.Empty;
			AssertNoMessageError(license.US_QuantityInfo, messageError);
		}

		public void TestCheckUS_UnitOfMeasure()
		{
			var license = Header.Licenses.AddNew();
			license.US_Quantity = 10m;
			license.US_UnitOfMeasure = "K@";
			var messageError = ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Unit Of Measure", "Quantity");
			AssertNoMessageError(license.US_UnitOfMeasureInfo, messageError);
			AssertHasMessageErrorContaining(license.US_UnitOfMeasureInfo, ListValidation.InvalidCodeMessageError);
			license.US_UnitOfMeasure = "KG";
			AssertNoMessageError(license.US_UnitOfMeasureInfo, messageError);
			AssertNoMessageErrorContaining(license.US_UnitOfMeasureInfo, ListValidation.InvalidCodeMessageError);
			license.US_UnitOfMeasure = ZString.Empty;
			AssertHasMessageError(license.US_UnitOfMeasureInfo, messageError);
			AssertNoMessageErrorContaining(license.US_UnitOfMeasureInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			license.AddInfoValidation.ValidateUS_UnitOfMeasure();
			AssertNoMessageError(license.US_UnitOfMeasureInfo, messageError);
			AssertNoMessageErrorContaining(license.US_UnitOfMeasureInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			license.AddInfoValidation.ValidateUS_UnitOfMeasure();
			AssertHasMessageError(license.US_UnitOfMeasureInfo, messageError);
			AssertNoMessageErrorContaining(license.US_UnitOfMeasureInfo, ListValidation.InvalidCodeMessageError);
			license.US_Quantity = ZDecimal.Zero;
			AssertNoMessageError(license.US_UnitOfMeasureInfo, messageError);
			AssertNoMessageErrorContaining(license.US_UnitOfMeasureInfo, ListValidation.InvalidCodeMessageError);
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		APHISHeader Header
		{
			get
			{
				if (aphisHeader == null)
				{
					aphisHeader = InvoiceLine.APHISHeaders.AddNew();
					aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
				}
				return aphisHeader;
			}
		}
		APHISHeader aphisHeader;

		#endregion
	}
}
