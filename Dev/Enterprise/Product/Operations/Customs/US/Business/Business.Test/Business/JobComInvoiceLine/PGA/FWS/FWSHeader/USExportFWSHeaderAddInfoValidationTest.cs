using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USExportFWSHeaderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_ConfirmationNum()
		{
			ExportFWS.US_ConfirmationNum = "~";
			AssertHasMessageErrorContaining(ExportFWS.US_ConfirmationNumInfo, USExportFWSHeaderAddInfoValidation.ConfirmationNumberInvalidFormat);

			ExportFWS.US_ConfirmationNum = "AABB1234567";
			AssertHasMessageErrorContaining(ExportFWS.US_ConfirmationNumInfo, USExportFWSHeaderAddInfoValidation.ConfirmationNumberInvalidFormat);

			ExportFWS.US_ConfirmationNum = "2015AA1234567";
			AssertNoMessageErrorContaining(ExportFWS.US_ConfirmationNumInfo, USExportFWSHeaderAddInfoValidation.ConfirmationNumberInvalidFormat);
		}

		public void TestCheckUS_TaxonomicSerialNumber()
		{
			ExportFWS.AddInfoValidation.ValidateUS_TaxonomicSerialNumber();
			AssertHasMessageErrorContaining(ExportFWS.US_TaxonomicSerialNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ExportFWS.US_TaxonomicSerialNumber = "~";
			AssertNoMessageErrorContaining(ExportFWS.US_TaxonomicSerialNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(ExportFWS.US_TaxonomicSerialNumberInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ExportFWS.US_ConfirmationNum = "2015AA1234567";
			ExportFWS.AddInfoValidation.ValidateUS_TaxonomicSerialNumber();
			AssertHasMessageErrorContaining(ExportFWS.US_TaxonomicSerialNumberInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);
		}

		public void TestCheckUS_PurposeCode()
		{
			ExportFWS.AddInfoValidation.ValidateUS_PurposeCode();
			AssertHasMessageErrorContaining(ExportFWS.US_PurposeCodeInfo, MandatoryValidation.YouHaveNotEntered);

			ExportFWS.US_PurposeCode = "~";
			AssertNoMessageErrorContaining(ExportFWS.US_PurposeCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(ExportFWS.US_PurposeCodeInfo, ListValidation.InvalidCodeMessageError);

			ExportFWS.US_PurposeCode = FWSPurposeCodeList.Codes.BiomedicalResearch;
			AssertNoMessageErrorContaining(ExportFWS.US_PurposeCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(ExportFWS.US_PurposeCodeInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ExportFWS.US_ConfirmationNum = "2015AA1234567";
			ExportFWS.AddInfoValidation.ValidateUS_PurposeCode();
			AssertHasMessageErrorContaining(ExportFWS.US_PurposeCodeInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ProductExportFWS.AddInfoValidation.ValidateUS_PurposeCode();
			AssertNoMessageErrorContaining(ProductExportFWS.US_PurposeCodeInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_PurposeCode = "~";
			AssertHasMessageErrorContaining(ProductExportFWS.US_PurposeCodeInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_PurposeCode = FWSPurposeCodeList.Codes.BiomedicalResearch;
			AssertNoMessageErrorContaining(ProductExportFWS.US_PurposeCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_WildlifeDescriptionCode()
		{
			ExportFWS.AddInfoValidation.ValidateUS_WildlifeDescriptionCode();
			AssertHasMessageErrorContaining(ExportFWS.US_WildlifeDescriptionCodeInfo, MandatoryValidation.YouHaveNotEntered);

			ExportFWS.US_WildlifeDescriptionCode = "~";
			AssertNoMessageErrorContaining(ExportFWS.US_WildlifeDescriptionCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(ExportFWS.US_WildlifeDescriptionCodeInfo, ListValidation.InvalidCodeMessageError);

			ExportFWS.US_WildlifeDescriptionCode = FWSWildlifeDescriptionCodesList.Codes.BAL;
			AssertNoMessageErrorContaining(ExportFWS.US_WildlifeDescriptionCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(ExportFWS.US_WildlifeDescriptionCodeInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ExportFWS.US_ConfirmationNum = "2015AA1234567";
			ExportFWS.AddInfoValidation.ValidateUS_WildlifeDescriptionCode();
			AssertHasMessageErrorContaining(ExportFWS.US_WildlifeDescriptionCodeInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ProductExportFWS.AddInfoValidation.ValidateUS_WildlifeDescriptionCode();
			AssertNoMessageErrorContaining(ProductExportFWS.US_WildlifeDescriptionCodeInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_WildlifeDescriptionCode = "~";
			AssertHasMessageErrorContaining(ProductExportFWS.US_WildlifeDescriptionCodeInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_WildlifeDescriptionCode = FWSWildlifeDescriptionCodesList.Codes.BAL;
			AssertNoMessageErrorContaining(ProductExportFWS.US_WildlifeDescriptionCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_SpeciesOrigin()
		{
			ExportFWS.AddInfoValidation.ValidateUS_SpeciesOrigin();
			AssertHasMessageErrorContaining(ExportFWS.US_SpeciesOriginInfo, MandatoryValidation.YouHaveNotEntered);

			ExportFWS.US_SpeciesOrigin = "~";
			AssertNoMessageErrorContaining(ExportFWS.US_SpeciesOriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(ExportFWS.US_SpeciesOriginInfo, ListValidation.InvalidCodeMessageError);

			ExportFWS.US_SpeciesOrigin = "US";
			AssertNoMessageErrorContaining(ExportFWS.US_SpeciesOriginInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(ExportFWS.US_SpeciesOriginInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ExportFWS.US_ConfirmationNum = "2015AA1234567";
			ExportFWS.AddInfoValidation.ValidateUS_SpeciesOrigin();
			AssertHasMessageErrorContaining(ExportFWS.US_SpeciesOriginInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ProductExportFWS.AddInfoValidation.ValidateUS_SpeciesOrigin();
			AssertNoMessageErrorContaining(ProductExportFWS.US_SpeciesOriginInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_SpeciesOrigin = "~";
			AssertHasMessageErrorContaining(ProductExportFWS.US_SpeciesOriginInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_SpeciesOrigin = "US";
			AssertNoMessageErrorContaining(ProductExportFWS.US_SpeciesOriginInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_SpeciesOrigin = "ZZ";
			AssertNoMessageErrorContaining(ProductExportFWS.US_SpeciesOriginInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_WildlifeSource()
		{
			ExportFWS.AddInfoValidation.ValidateUS_WildlifeSource();
			AssertHasMessageErrorContaining(ExportFWS.US_WildlifeSourceInfo, MandatoryValidation.YouHaveNotEntered);

			ExportFWS.US_WildlifeSource = "~";
			AssertNoMessageErrorContaining(ExportFWS.US_WildlifeSourceInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(ExportFWS.US_WildlifeSourceInfo, ListValidation.InvalidCodeMessageError);

			ExportFWS.US_WildlifeSource = FWSWildlifeSourceList.Codes.C;
			AssertNoMessageErrorContaining(ExportFWS.US_WildlifeSourceInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(ExportFWS.US_WildlifeSourceInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ExportFWS.US_ConfirmationNum = "2015AA1234567";
			ExportFWS.AddInfoValidation.ValidateUS_WildlifeSource();
			AssertHasMessageErrorContaining(ExportFWS.US_WildlifeSourceInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ProductExportFWS.AddInfoValidation.ValidateUS_WildlifeSource();
			AssertNoMessageErrorContaining(ProductExportFWS.US_WildlifeSourceInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_WildlifeSource = "~";
			AssertHasMessageErrorContaining(ProductExportFWS.US_WildlifeSourceInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_WildlifeSource = FWSWildlifeSourceList.Codes.C;
			AssertNoMessageErrorContaining(ProductExportFWS.US_WildlifeSourceInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_CertificationCode()
		{
			ExportFWS.AddInfoValidation.ValidateUS_CertificationCode();
			AssertHasMessageErrorContaining(ExportFWS.US_CertificationCodeInfo, MandatoryValidation.YouHaveNotEntered);

			ExportFWS.US_CertificationCode = "~";
			AssertNoMessageErrorContaining(ExportFWS.US_CertificationCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(ExportFWS.US_CertificationCodeInfo, ListValidation.InvalidCodeMessageError);

			ExportFWS.US_CertificationCode = FWSCertificationCodeList.Codes.CertificationOfNoWildlife;
			AssertNoMessageErrorContaining(ExportFWS.US_CertificationCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(ExportFWS.US_CertificationCodeInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ExportFWS.US_ConfirmationNum = "2015AA1234567";
			ExportFWS.AddInfoValidation.ValidateUS_CertificationCode();
			AssertHasMessageErrorContaining(ExportFWS.US_CertificationCodeInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ProductExportFWS.AddInfoValidation.ValidateUS_CertificationCode();
			AssertNoMessageErrorContaining(ProductExportFWS.US_CertificationCodeInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_CertificationCode = "~";
			AssertHasMessageErrorContaining(ProductExportFWS.US_CertificationCodeInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_CertificationCode = FWSCertificationCodeList.Codes.CertificationOfNoWildlife;
			AssertNoMessageErrorContaining(ProductExportFWS.US_CertificationCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_WildlifeCategoryCode()
		{
			ExportFWS.AddInfoValidation.ValidateUS_WildlifeCategoryCode();
			AssertHasMessageErrorContaining(ExportFWS.US_WildlifeCategoryCodeInfo, MandatoryValidation.YouHaveNotEntered);

			ExportFWS.US_WildlifeCategoryCode = "~";
			AssertNoMessageErrorContaining(ExportFWS.US_WildlifeCategoryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(ExportFWS.US_WildlifeCategoryCodeInfo, ListValidation.InvalidCodeMessageError);

			ExportFWS.US_WildlifeCategoryCode = FWSWildlifeCategoryCodesList.Codes.Amphibians;
			AssertNoMessageErrorContaining(ExportFWS.US_WildlifeCategoryCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(ExportFWS.US_WildlifeCategoryCodeInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ExportFWS.US_ConfirmationNum = "2015AA1234567";
			ExportFWS.AddInfoValidation.ValidateUS_WildlifeCategoryCode();
			AssertHasMessageErrorContaining(ExportFWS.US_WildlifeCategoryCodeInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ProductExportFWS.AddInfoValidation.ValidateUS_WildlifeCategoryCode();
			AssertNoMessageErrorContaining(ProductExportFWS.US_WildlifeCategoryCodeInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_WildlifeCategoryCode = "~";
			AssertHasMessageErrorContaining(ProductExportFWS.US_WildlifeCategoryCodeInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_WildlifeCategoryCode = FWSWildlifeCategoryCodesList.Codes.Amphibians;
			AssertNoMessageErrorContaining(ProductExportFWS.US_WildlifeCategoryCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_StateCode()
		{
			ExportFWS.AddInfoValidation.ValidateUS_USState();
			AssertHasMessageErrorContaining(ExportFWS.US_USStateInfo, MandatoryValidation.YouHaveNotEntered);

			ExportFWS.US_USState = "~";
			AssertNoMessageErrorContaining(ExportFWS.US_USStateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(ExportFWS.US_USStateInfo, ListValidation.InvalidCodeMessageError);

			ExportFWS.US_SpeciesOrigin = Core.Constants.CountryCodes.UnitedStates;
			ExportFWS.US_USState = "CA";
			AssertNoMessageErrorContaining(ExportFWS.US_USStateInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(ExportFWS.US_USStateInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ExportFWS.US_ConfirmationNum = "2015AA1234567";
			ExportFWS.AddInfoValidation.ValidateUS_USState();
			AssertHasMessageErrorContaining(ExportFWS.US_USStateInfo, USExportFWSHeaderAddInfoValidation.ValueShouldNotBeEnteredIfConfirmationNumberIsEntered);

			ProductExportFWS.AddInfoValidation.ValidateUS_USState();
			AssertNoMessageErrorContaining(ProductExportFWS.US_USStateInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_USState = "~";
			AssertHasMessageErrorContaining(ProductExportFWS.US_USStateInfo, ListValidation.InvalidCodeMessageError);
			ProductExportFWS.US_SpeciesOrigin = Core.Constants.CountryCodes.UnitedStates;
			ProductExportFWS.US_USState = "CA";
			AssertNoMessageErrorContaining(ProductExportFWS.US_USStateInfo, ListValidation.InvalidCodeMessageError);
		}

		#region Implementation

		FWSHeader ExportFWS
		{
			get
			{
				if (exportFWS == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
					exportFWS = invoiceLine.ExportFWS;
				}

				return exportFWS;
			}
		}
		FWSHeader exportFWS;

		FWSHeader ProductExportFWS
		{
			get
			{
				if (productExportFWS == null)
				{
					var exportProduct = Factory.New<CusClassPartPivot>();
					exportProduct.CI_ChildType = ClassificationTypeList.Codes.HTE;
					exportProduct.CD_FWSIndicator = OGAIndicatorList.Codes.Declared;
					productExportFWS = exportProduct.ExportFWS;
				}

				return productExportFWS;
			}
		}
		FWSHeader productExportFWS;

		#endregion
	}
}
