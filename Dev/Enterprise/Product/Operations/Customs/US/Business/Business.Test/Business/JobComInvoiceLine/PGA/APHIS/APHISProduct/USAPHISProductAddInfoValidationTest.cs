using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using CommodityCharacteristicQualifier = Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier;

namespace Enterprise.Customs.US.Business.Testing
{
	class USAPHISProductAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_Age()
		{
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var product = Header.Products.AddNew();
			product.US_Age = ZString.Empty;
			AssertNoNotifications(product.US_AgeInfo);
			product.US_Age = "!";
			AssertHasMessageErrorContaining(product.US_AgeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			product.AddInfoValidation.ValidateUS_Age();
			AssertNoMessageErrorContaining(product.US_AgeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			product.AddInfoValidation.ValidateUS_Age();
			AssertHasMessageErrorContaining(product.US_AgeInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			product = Header.Products.AddNew();
			product.US_Age = "!";
			AssertNoMessageErrorContaining(product.US_AgeInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			product = Header.Products.AddNew();
			product.US_Age = "!";
			AssertHasMessageErrorContaining(product.US_AgeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new CommodityCharacteristicQualifier.LiveAnimalsAgeList())
			{
				product.US_Age = pair.Code;
				AssertNoMessageErrorContaining(product.US_AgeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_SourceTypeCode()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			var product = Header.Products.AddNew();
			product.US_Species = "Species";
			product.US_SourceTypeCode = "!";
			AssertHasMessageErrorContaining(product.US_SourceTypeCodeInfo, ListValidation.InvalidCodeMessageError);
			product.US_SourceTypeCode = SourceTypeCodesList.Codes.Harvested;
			AssertNoMessageErrorContaining(product.US_SourceTypeCodeInfo, ListValidation.InvalidCodeMessageError);
			product.US_SourceTypeCode = "";
			AssertHasMessageErrorContaining(product.US_SourceTypeCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_Genus()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;

			var product = Header.Products.AddNew();
			product.US_Species = "Species";
			product.US_Genus = "GENUS";
			AssertNoMessageErrorContaining(product.US_GenusInfo, ValidationConstants.APHIS.ScientificDataIsRequired("Genus Name", Header.US_ProgramType, "Program Type"));
			product.US_Genus = "";
			AssertHasMessageErrorContaining(product.US_GenusInfo, ValidationConstants.APHIS.ScientificDataIsRequired("Genus Name", Header.US_ProgramType, "Program Type"));
		}

		public void TestCheckUS_Specific()
		{
			var product = Header.Products.AddNew();
			product.US_Genus = "GENUS";

			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			product.US_SpecificName = "";
			AssertNoMessageErrorContaining(product.US_SpecificNameInfo, ValidationConstants.APHIS.ScientificDataIsRequired("Specific Name", APHISProgramCodeList.Codes.APQ, "Program Type"));

			Header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			product.US_SpecificName = "Specific";
			AssertNoMessageErrorContaining(product.US_SpecificNameInfo, ValidationConstants.APHIS.ScientificDataIsRequired("Specific Name", Header.US_ProgramType, "Program Type"));
			product.US_SpecificName = "";
			AssertHasMessageErrorContaining(product.US_SpecificNameInfo, ValidationConstants.APHIS.ScientificDataIsRequired("Specific Name", Header.US_ProgramType, "Program Type"));
		}

		public void TestCheckUS_ProcessingDescription()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var product = Header.Products.AddNew();
			product.US_ProcessingTypeCode = APHISProcessingTypeCodeList.Codes.ACA;
			product.US_ProcessingDescription = "";
			AssertNoMessageErrorContaining(product.US_ProcessingDescriptionInfo, ValidationConstants.APHIS.ScientificDataIsRequired("Processing Description", APHISProcessingTypeCodeList.Codes.ATR, "Processing Type"));

			product.US_ProcessingTypeCode = APHISProcessingTypeCodeList.Codes.ATR;
			product.US_ProcessingDescription = "Processing Description";
			AssertNoMessageErrorContaining(product.US_ProcessingDescriptionInfo, ValidationConstants.APHIS.ScientificDataIsRequired("Processing Description", APHISProcessingTypeCodeList.Codes.ATR, "Processing Type"));
			product.US_ProcessingDescription = "";
			AssertHasMessageErrorContaining(product.US_ProcessingDescriptionInfo, ValidationConstants.APHIS.ScientificDataIsRequired("Processing Description", APHISProcessingTypeCodeList.Codes.ATR, "Processing Type"));
		}

		public void TestCheckUS_CountryCode()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			var product = Header.Products.AddNew();
			product.US_Species = "Species";
			product.US_CountryCode = "!";
			AssertHasMessageErrorContaining(product.US_CountryCodeInfo, ListValidation.InvalidCodeMessageError);
			product.US_CountryCode = "MX";
			AssertNoMessageErrorContaining(product.US_CountryCodeInfo, ListValidation.InvalidCodeMessageError);
			product.US_CountryCode = "";
			AssertHasMessageErrorContaining(product.US_CountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ShowBreed()
		{
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var product = Header.Products.AddNew();
			product.US_ShowBreed = ZString.Empty;
			AssertNoNotifications(product.US_ShowBreedInfo);
			product.US_ShowBreed = "!";
			AssertHasWarningContaining(product.US_ShowBreedInfo, ListValidation.InvalidCodeMessage);
			Declaration.ValidationModes = ValidationModes.None;
			product.AddInfoValidation.ValidateUS_ShowBreed();
			AssertNoWarningContaining(product.US_ShowBreedInfo, ListValidation.InvalidCodeMessage);
			Declaration.RecalculateValidationModesOnDeclaration();
			product.AddInfoValidation.ValidateUS_ShowBreed();
			AssertHasWarningContaining(product.US_ShowBreedInfo, ListValidation.InvalidCodeMessage);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			product = Header.Products.AddNew();
			product.US_ShowBreed = "!";
			AssertNoWarningContaining(product.US_ShowBreedInfo, ListValidation.InvalidCodeMessage);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			product = Header.Products.AddNew();
			product.US_ShowBreed = "!";
			AssertHasWarningContaining(product.US_ShowBreedInfo, ListValidation.InvalidCodeMessage);
			foreach (ICodeDescription pair in new APHISBreedList())
			{
				product.US_ShowBreed = pair.Code;
				AssertNoWarningContaining(product.US_ShowBreedInfo, ListValidation.InvalidCodeMessage);
			}
		}

		public void TestCheckUS_BreedVariety()
		{
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var product = Header.Products.AddNew();
			product.US_BreedVariety = ZString.Empty;
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Breed / Variety");
			AssertHasMessageError(product.US_BreedVarietyInfo, messageError);
			AssertNoMessageErrorContaining(product.US_BreedVarietyInfo, ListValidation.InvalidCodeMessageError);
			product.US_BreedVariety = "!";
			AssertNoMessageError(product.US_BreedVarietyInfo, messageError);
			AssertHasMessageErrorContaining(product.US_BreedVarietyInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			product.AddInfoValidation.ValidateUS_BreedVariety();
			AssertNoMessageError(product.US_BreedVarietyInfo, messageError);
			AssertNoMessageErrorContaining(product.US_BreedVarietyInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			product.AddInfoValidation.ValidateUS_BreedVariety();
			AssertNoMessageError(product.US_BreedVarietyInfo, messageError);
			AssertHasMessageErrorContaining(product.US_BreedVarietyInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			product = Header.Products.AddNew();
			product.US_BreedVariety = "!";
			AssertNoMessageError(product.US_BreedVarietyInfo, messageError);
			AssertNoMessageErrorContaining(product.US_BreedVarietyInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			product = Header.Products.AddNew();
			product.US_BreedVariety = "!";
			AssertNoMessageError(product.US_BreedVarietyInfo, messageError);
			AssertHasMessageErrorContaining(product.US_BreedVarietyInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new CommodityCharacteristicQualifier.LiveAnimalsBirdsList())
			{
				product.US_BreedVariety = pair.Code;
				AssertNoMessageErrors(product.US_BreedVarietyInfo);
			}
			product.US_ShowBreed = APHISBreedList.Codes.BuffaloBison;
			product.US_BreedVariety = CommodityCharacteristicQualifier.LiveAnimalsLlamaAlpacaList.Codes.AlpacaHuacayaVicugnaPacos;
			AssertNoMessageErrors(product.US_BreedVarietyInfo);
			var warningMessage = ValidationConstants.APHIS.NotValidForBreed(APHISBreedList.Codes.BuffaloBison, CommodityCharacteristicQualifier.LiveAnimalsLlamaAlpacaList.Codes.AlpacaHuacayaVicugnaPacos);
			AssertHasWarning(product.US_BreedVarietyInfo, warningMessage);
			product.US_BreedVariety = CommodityCharacteristicQualifier.LiveAnimalsBuffaloBisonList.Codes.AmericanBisonBuffalo;
			AssertNoMessageErrors(product.US_BreedVarietyInfo);
			AssertNoWarnings(product.US_BreedVarietyInfo);
		}

		public void TestCheckUS_Color()
		{
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var product = Header.Products.AddNew();
			product.US_Color = ZString.Empty;
			AssertNoNotifications(product.US_ColorInfo);
			product.US_Color = "!";
			AssertHasMessageErrorContaining(product.US_ColorInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			product.AddInfoValidation.ValidateUS_Color();
			AssertNoMessageErrorContaining(product.US_ColorInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			product.AddInfoValidation.ValidateUS_Color();
			AssertHasMessageErrorContaining(product.US_ColorInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			product = Header.Products.AddNew();
			product.US_Color = "!";
			AssertNoMessageErrorContaining(product.US_ColorInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			product = Header.Products.AddNew();
			product.US_Color = "!";
			AssertHasMessageErrorContaining(product.US_ColorInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new CommodityCharacteristicQualifier.LiveAnimalsColorList())
			{
				product.US_Color = pair.Code;
				AssertNoMessageErrorContaining(product.US_ColorInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_Gender()
		{
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var product = Header.Products.AddNew();
			product.US_Gender = ZString.Empty;
			AssertNoNotifications(product.US_GenderInfo);
			product.US_Gender = "!";
			AssertHasMessageErrorContaining(product.US_GenderInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			product.AddInfoValidation.ValidateUS_Gender();
			AssertNoMessageErrorContaining(product.US_GenderInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			product.AddInfoValidation.ValidateUS_Gender();
			AssertHasMessageErrorContaining(product.US_GenderInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			product = Header.Products.AddNew();
			product.US_Gender = "!";
			AssertNoMessageErrorContaining(product.US_GenderInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			product = Header.Products.AddNew();
			product.US_Gender = "!";
			AssertHasMessageErrorContaining(product.US_GenderInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new CommodityCharacteristicQualifier.LiveAnimalsGenderList())
			{
				product.US_Gender = pair.Code;
				AssertNoMessageErrorContaining(product.US_GenderInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_GestationalAgeIfPregnant()
		{
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var product = Header.Products.AddNew();
			product.US_GestationalAgeIfPregnant = ZString.Empty;
			AssertNoNotifications(product.US_GestationalAgeIfPregnantInfo);
			product.US_GestationalAgeIfPregnant = "!";
			AssertHasMessageErrorContaining(product.US_GestationalAgeIfPregnantInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			product.AddInfoValidation.ValidateUS_GestationalAgeIfPregnant();
			AssertNoMessageErrorContaining(product.US_GestationalAgeIfPregnantInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			product.AddInfoValidation.ValidateUS_GestationalAgeIfPregnant();
			AssertHasMessageErrorContaining(product.US_GestationalAgeIfPregnantInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			product = Header.Products.AddNew();
			product.US_GestationalAgeIfPregnant = "!";
			AssertNoMessageErrorContaining(product.US_GestationalAgeIfPregnantInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			product = Header.Products.AddNew();
			product.US_GestationalAgeIfPregnant = "!";
			AssertHasMessageErrorContaining(product.US_GestationalAgeIfPregnantInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new CommodityCharacteristicQualifier.LiveAnimalsGestationalAgeList())
			{
				product.US_GestationalAgeIfPregnant = pair.Code;
				AssertNoMessageErrorContaining(product.US_GestationalAgeIfPregnantInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_IsFertilizedPregnantGestating()
		{
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var product = Header.Products.AddNew();
			product.US_IsFertilizedPregnantGestating = ZString.Empty;
			AssertNoNotifications(product.US_IsFertilizedPregnantGestatingInfo);
			product.US_IsFertilizedPregnantGestating = "!";
			AssertHasMessageErrorContaining(product.US_IsFertilizedPregnantGestatingInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			product.AddInfoValidation.ValidateUS_IsFertilizedPregnantGestating();
			AssertNoMessageErrorContaining(product.US_IsFertilizedPregnantGestatingInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			product.AddInfoValidation.ValidateUS_IsFertilizedPregnantGestating();
			AssertHasMessageErrorContaining(product.US_IsFertilizedPregnantGestatingInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			product = Header.Products.AddNew();
			product.US_IsFertilizedPregnantGestating = "!";
			AssertNoMessageErrorContaining(product.US_IsFertilizedPregnantGestatingInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			product = Header.Products.AddNew();
			product.US_IsFertilizedPregnantGestating = "!";
			AssertHasMessageErrorContaining(product.US_IsFertilizedPregnantGestatingInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in YesNoDefaultList.GetCachedYesOnlyList(Factory))
			{
				product.US_IsFertilizedPregnantGestating = pair.Code;
				AssertNoMessageErrorContaining(product.US_IsFertilizedPregnantGestatingInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_IsProtectedSpecies()
		{
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var product = Header.Products.AddNew();
			product.US_IsProtectedSpecies = ZString.Empty;
			AssertNoNotifications(product.US_IsProtectedSpeciesInfo);
			product.US_IsProtectedSpecies = "!";
			AssertHasMessageErrorContaining(product.US_IsProtectedSpeciesInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			product.AddInfoValidation.ValidateUS_IsProtectedSpecies();
			AssertNoMessageErrorContaining(product.US_IsProtectedSpeciesInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			product.AddInfoValidation.ValidateUS_IsProtectedSpecies();
			AssertHasMessageErrorContaining(product.US_IsProtectedSpeciesInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			product = Header.Products.AddNew();
			product.US_IsProtectedSpecies = "!";
			AssertNoMessageErrorContaining(product.US_IsProtectedSpeciesInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			product = Header.Products.AddNew();
			product.US_IsProtectedSpecies = "!";
			AssertHasMessageErrorContaining(product.US_IsProtectedSpeciesInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new CommodityCharacteristicQualifier.LiveAnimalsProtectedSpeciesList())
			{
				product.US_IsProtectedSpecies = pair.Code;
				AssertNoMessageErrorContaining(product.US_IsProtectedSpeciesInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_Origin()
		{
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			var product = Header.Products.AddNew();
			product.US_Origin = ZString.Empty;
			AssertHasMessageErrorContaining(product.US_OriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(product.US_OriginInfo, ListValidation.InvalidCodeMessageError);
			product.US_Origin = "!";
			AssertNoMessageErrorContaining(product.US_OriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(product.US_OriginInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			product.AddInfoValidation.ValidateUS_Origin();
			AssertNoMessageErrorContaining(product.US_OriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(product.US_OriginInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			product.AddInfoValidation.ValidateUS_Origin();
			AssertNoMessageErrorContaining(product.US_OriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(product.US_OriginInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			product = Header.Products.AddNew();
			product.US_Origin = "!";
			AssertNoMessageErrorContaining(product.US_OriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(product.US_OriginInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			product = Header.Products.AddNew();
			product.US_Origin = "!";
			AssertNoMessageErrorContaining(product.US_OriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(product.US_OriginInfo, ListValidation.InvalidCodeMessageError);
			product.US_Origin = "AU";
			AssertNoMessageErrorContaining(product.US_OriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(product.US_OriginInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_Type()
		{
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			var product = Header.Products.AddNew();
			product.US_Type = ZString.Empty;
			AssertHasMessageErrorContaining(product.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(product.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			product.US_Type = "!";
			AssertNoMessageErrorContaining(product.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(product.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			product.AddInfoValidation.ValidateUS_Type();
			AssertNoMessageErrorContaining(product.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(product.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			product.AddInfoValidation.ValidateUS_Type();
			AssertNoMessageErrorContaining(product.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(product.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			product = Header.Products.AddNew();
			product.US_Type = "!";
			AssertNoMessageErrorContaining(product.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(product.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			product = Header.Products.AddNew();
			product.US_Type = "!";
			AssertNoMessageErrorContaining(product.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(product.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new CommodityCharacteristicQualifier.AnimalProductsAndByProductsConditionA32List())
			{
				product.US_Type = pair.Code;
				AssertNoMessageErrorContaining(product.US_TypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(product.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_SpecificName()
		{
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			var product = Header.Products.AddNew();
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Product Component Specific Name");
			product.US_SpecificName = "BOB";
			AssertNoMessageError(product.US_SpecificNameInfo, messageError);
			product.US_SpecificName = ZString.Empty;
			AssertHasMessageError(product.US_SpecificNameInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			product.AddInfoValidation.ValidateUS_SpecificName();
			AssertNoMessageError(product.US_SpecificNameInfo, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			product.AddInfoValidation.ValidateUS_SpecificName();
			AssertHasMessageError(product.US_SpecificNameInfo, messageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			product = Header.Products.AddNew();
			product.US_SpecificName = ZString.Empty;
			AssertNoMessageError(product.US_SpecificNameInfo, messageError);
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			product = Header.Products.AddNew();
			product.US_SpecificName = ZString.Empty;
			AssertHasMessageError(product.US_SpecificNameInfo, messageError);
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
