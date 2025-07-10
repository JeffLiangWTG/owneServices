using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	class USAPHISSourceAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_SourceTypeCode()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var source = Header.Sources.AddNew();
			source.US_SourceTypeCode = "!";
			AssertNoMessageErrorContaining(source.US_SourceTypeCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(source.US_SourceTypeCodeInfo, ListValidation.InvalidCodeMessageError);
			source.US_SourceTypeCode = ZString.Empty;
			AssertHasMessageErrorContaining(source.US_SourceTypeCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(source.US_SourceTypeCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			source.AddInfoValidation.ValidateUS_SourceTypeCode();
			AssertNoMessageErrorContaining(source.US_SourceTypeCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(source.US_SourceTypeCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			source.AddInfoValidation.ValidateUS_SourceTypeCode();
			AssertHasMessageErrorContaining(source.US_SourceTypeCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(source.US_SourceTypeCodeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in SourceTypeCodesList.GetListForAPHIS(Factory, APHISProgramCodeList.Codes.AVS))
			{
				source.US_SourceTypeCode = pair.Code;
				AssertNoMessageErrorContaining(source.US_SourceTypeCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(source.US_SourceTypeCodeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_ProcessingTypeCode()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			var source = Header.Sources.AddNew();
			source.US_ProcessingTypeCode = ZString.Empty;
			AssertNoMessageErrors(source.US_ProcessingTypeCodeInfo);
			source.US_ProcessingTypeCode = "!";
			AssertHasMessageErrorContaining(source.US_ProcessingTypeCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			source.AddInfoValidation.ValidateUS_ProcessingTypeCode();
			AssertNoMessageErrorContaining(source.US_ProcessingTypeCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			source.AddInfoValidation.ValidateUS_ProcessingTypeCode();
			AssertHasMessageErrorContaining(source.US_ProcessingTypeCodeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in APHISProcessingTypeCodeList.GetListFor(Factory, APHISProgramCodeList.Codes.AVS, APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts))
			{
				source.US_ProcessingTypeCode = pair.Code;
				AssertNoMessageErrors(source.US_ProcessingTypeCodeInfo);
			}
		}

		public void TestCheckUS_ProcessingDescription()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var source = Header.Sources.AddNew();
			source.US_ProcessingTypeCode = APHISProcessingTypeCodeList.Codes.ATR;
			source.US_ProcessingDescription = "BOB";
			AssertNoMessageError(source.US_ProcessingDescriptionInfo, ValidationConstants.APHIS.ProcessingDescriptionRequiresForOtherTreatmentType);
			source.US_ProcessingDescription = ZString.Empty;
			AssertHasMessageError(source.US_ProcessingDescriptionInfo, ValidationConstants.APHIS.ProcessingDescriptionRequiresForOtherTreatmentType);
			source.US_ProcessingTypeCode = APHISProcessingTypeCodeList.Codes.AST16;
			AssertNoMessageError(source.US_ProcessingDescriptionInfo, ValidationConstants.APHIS.ProcessingDescriptionRequiresForOtherTreatmentType);
		}

		public void TestCheckUS_CountryCode()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var source = Header.Sources.AddNew();
			source.US_CountryCode = "!";
			AssertNoMessageErrorContaining(source.US_CountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(source.US_CountryCodeInfo, ListValidation.InvalidCodeMessageError);
			source.US_CountryCode = ZString.Empty;
			AssertHasMessageErrorContaining(source.US_CountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(source.US_CountryCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			source.AddInfoValidation.ValidateUS_CountryCode();
			AssertNoMessageErrorContaining(source.US_CountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(source.US_CountryCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			source.AddInfoValidation.ValidateUS_CountryCode();
			AssertHasMessageErrorContaining(source.US_CountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(source.US_CountryCodeInfo, ListValidation.InvalidCodeMessageError);
			source.US_CountryCode = "AU";
			AssertNoMessageErrorContaining(source.US_CountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(source.US_CountryCodeInfo, ListValidation.InvalidCodeMessageError);
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
