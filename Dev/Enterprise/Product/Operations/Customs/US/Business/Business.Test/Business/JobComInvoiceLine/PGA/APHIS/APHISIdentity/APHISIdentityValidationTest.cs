using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	class APHISIdentityValidationTest : TestCaseWithFactory
	{
		public void TestCheckUseMultipleNumbers()
		{
			Identity.UseMultipleNumbers = true;
			AssertHasMessageError(Identity.UseMultipleNumbersInfo, ValidationConstants.APHIS.AtLeastOneNumberRangeIsRequired);
			Declaration.ValidationModes = ValidationModes.None;
			Identity.Validation.ValidateUseMultipleNumbers();
			AssertNoMessageError(Identity.UseMultipleNumbersInfo, ValidationConstants.APHIS.AtLeastOneNumberRangeIsRequired);
			Declaration.RecalculateValidationModesOnDeclaration();
			Identity.Validation.ValidateUseMultipleNumbers();
			AssertHasMessageError(Identity.UseMultipleNumbersInfo, ValidationConstants.APHIS.AtLeastOneNumberRangeIsRequired);
			var range = Identity.NumberRanges.AddNew();
			Identity.Validation.ValidateUseMultipleNumbers();
			AssertNoMessageError(Identity.UseMultipleNumbersInfo, ValidationConstants.APHIS.AtLeastOneNumberRangeIsRequired);
		}

		public void TestCheckCY_Code()
		{
			Identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			AssertNoMessageErrorContaining(Identity.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Identity.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			Identity.CY_Code = ZString.Empty;
			AssertHasMessageErrorContaining(Identity.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Identity.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			Identity.CY_Code = "!";
			AssertNoMessageErrorContaining(Identity.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Identity.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			Identity.Validation.ValidateCY_Code();
			AssertNoMessageErrorContaining(Identity.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Identity.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			Identity.Validation.ValidateCY_Code();
			AssertNoMessageErrorContaining(Identity.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Identity.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in APHISItemIdentityNumberQualifierList.GetListWithoutBouquet(Factory))
			{
				Identity.CY_Code = pair.Code;
				AssertNoMessageErrorContaining(Identity.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(Identity.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckCY_Data()
		{
			Identity.CY_Data = "K".PadRight(USAPHISIdentityNumberRangeAddInfoSchema.US_StartNumber.MaxLength + 1, 'k');
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Identification Number");
			AssertHasWarning(Identity.CY_DataInfo, ValidationConstants.APHIS.IdentityNumberMaxLengthWarningMessage);
			AssertNoMessageError(Identity.CY_DataInfo, messageError);
			Identity.CY_Data = "K";
			AssertNoWarning(Identity.CY_DataInfo, ValidationConstants.APHIS.IdentityNumberMaxLengthWarningMessage);
			AssertNoMessageError(Identity.CY_DataInfo, messageError);
			Identity.CY_Data = ZString.Empty;
			AssertNoWarning(Identity.CY_DataInfo, ValidationConstants.APHIS.IdentityNumberMaxLengthWarningMessage);
			AssertHasMessageError(Identity.CY_DataInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			Identity.Validation.ValidateCY_Data();
			AssertNoWarning(Identity.CY_DataInfo, ValidationConstants.APHIS.IdentityNumberMaxLengthWarningMessage);
			AssertNoMessageError(Identity.CY_DataInfo, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			Identity.Validation.ValidateCY_Data();
			AssertNoWarning(Identity.CY_DataInfo, ValidationConstants.APHIS.IdentityNumberMaxLengthWarningMessage);
			AssertHasMessageError(Identity.CY_DataInfo, messageError);
			Identity.UseMultipleNumbers = true;
			AssertNoWarning(Identity.CY_DataInfo, ValidationConstants.APHIS.IdentityNumberMaxLengthWarningMessage);
			AssertNoMessageError(Identity.CY_DataInfo, messageError);
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

		APHISProduct Product
		{
			get { return product ?? (product = Header.Products.AddNew()); }
		}
		APHISProduct product;

		APHISIdentity Identity
		{
			get { return identity ?? (identity = Product.Identities.AddNew()); }
		}
		APHISIdentity identity;

		#endregion
	}
}
