using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.APHIS.ArticleCategory;
using Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ArticleCategory = Enterprise.Customs.US.Business.APHIS.ArticleCategory;
using CustomsUniversal = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USAPHISHeaderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAtLeastOneLicenseRequired()
		{
			var categoryTypes = new string[] { APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms, APHISCategoryTypeCodeList.Codes.LiveAnimals };
			var programCodes = new string[] { APHISProgramCodeList.Codes.ABS, APHISProgramCodeList.Codes.AVS, APHISProgramCodeList.Codes.AAC, APHISProgramCodeList.Codes.APQ };

			foreach (var programCode in programCodes)
			{
				foreach (var categoryType in categoryTypes)
				{
					if (categoryType == APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms)
					{
						var header = InvoiceLine.APHISHeaders.AddNew();
						header.US_ProgramType = programCode;
						header.US_CategoryType = categoryType;
						AssertHasMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.AtLeastOneLicenseRequired);

						header.Licenses.AddNew();
						header.AddInfoValidation.ValidateAll();
						AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.AtLeastOneLicenseRequired);
					}
				}
			}

			foreach (var categoryType in categoryTypes)
			{
				foreach (var programCode in programCodes)
				{
					if (categoryType == APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms)
					{
						var header = InvoiceLine.APHISHeaders.AddNew();
						header.US_CategoryType = categoryType;
						header.US_ProgramType = programCode;
						AssertHasMessageErrorContaining(header.US_ProgramTypeInfo, USAPHISHeaderAddInfoValidation.AtLeastOneLicenseRequired);

						header.Licenses.AddNew();
						header.AddInfoValidation.ValidateAll();
						AssertNoMessageErrorContaining(header.US_ProgramTypeInfo, USAPHISHeaderAddInfoValidation.AtLeastOneLicenseRequired);
					}
				}
			}
		}

		public void TestAPHISHeaderValidateCharactorsForAddressDescription()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";

			var party1 = Factory.New<OrgHeader>();
			var orgAddress1 = party1.MainAddress;
			orgAddress1.OA_City = "KYIV";
			orgAddress1.OA_Address1 = "éééÄöß";
			orgAddress1.OA_Address2 = "Address2Äöß";
			orgAddress1.OA_Code = "öß";

			var party2 = Factory.New<OrgHeader>();
			var orgAddress2 = party2.MainAddress;
			orgAddress2.OA_City = "KYIV";
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "address2";
			orgAddress2.OA_Code = "code";

			var header = InvoiceLine.APHISHeaders.AddNew();

			header.US_OA_ApplicantAddress = orgAddress1.PK;
			AssertHasWarning(header.US_OA_ApplicantAddressInfo, addressDescriptionWarning);
			AssertHasWarning(header.US_OA_ApplicantAddressInfo, addressCodeWarning);
			header.US_OA_ApplicantAddress = orgAddress2.PK;
			AssertNoWarning(header.US_OA_ApplicantAddressInfo, addressDescriptionWarning);
			AssertNoWarning(header.US_OA_ApplicantAddressInfo, addressCodeWarning);

			header.US_OA_CropGrowerAddress = orgAddress1.PK;
			AssertHasWarning(header.US_OA_CropGrowerAddressInfo, addressDescriptionWarning);
			AssertHasWarning(header.US_OA_CropGrowerAddressInfo, addressCodeWarning);
			header.US_OA_CropGrowerAddress = orgAddress2.PK;
			AssertNoWarning(header.US_OA_CropGrowerAddressInfo, addressDescriptionWarning);
			AssertNoWarning(header.US_OA_CropGrowerAddressInfo, addressCodeWarning);

			header.US_OA_ShipperAddress = orgAddress1.PK;
			AssertHasWarning(header.US_OA_ShipperAddressInfo, addressDescriptionWarning);
			AssertHasWarning(header.US_OA_ShipperAddressInfo, addressCodeWarning);
			header.US_OA_ShipperAddress = orgAddress2.PK;
			AssertNoWarning(header.US_OA_ShipperAddressInfo, addressDescriptionWarning);
			AssertNoWarning(header.US_OA_ShipperAddressInfo, addressCodeWarning);

			header.US_OA_PermittedAddress = orgAddress1.PK;
			AssertHasWarning(header.US_OA_PermittedAddressInfo, addressDescriptionWarning);
			AssertHasWarning(header.US_OA_PermittedAddressInfo, addressCodeWarning);
			header.US_OA_PermittedAddress = orgAddress2.PK;
			AssertNoWarning(header.US_OA_PermittedAddressInfo, addressDescriptionWarning);
			AssertNoWarning(header.US_OA_PermittedAddressInfo, addressCodeWarning);
		}

		public void TestValidateForsAnimalProductsAndAnimalPG10()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			header.US_CategoryCode = AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForResearchAndEvaluation;

			var source1 = header.Sources.AddNew();
			source1.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfProduction;
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_CategoryCode = AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForSaleAndDistribution;
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_CategoryCode = AnimalProductsAndByProductsList.Codes.OrganismsAndVectors;
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_CategoryCode = AnimalProductsAndByProductsList.Codes.LaboratoryMammals;
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_CategoryCode = AnimalProductsAndByProductsList.Codes.Insects;
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_CategoryCode = AnimalProductsAndByProductsList.Codes.BirdsNest;
			AssertHasMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);
		}

		public void TestValidateForAVSAnimalProductsCategoryType()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;

			AssertHasMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateSourceTypesForAVS);
			header.US_CategoryTypeInfo.ClearAllNotifications();

			var source1 = header.Sources.AddNew();
			source1.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfManipulation;
			header.AddInfoValidation.ValidateUS_CategoryType();
			AssertHasMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateSourceTypesForAVS);

			var source2 = header.Sources.AddNew();
			source2.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
			header.AddInfoValidation.ValidateUS_CategoryType();
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateSourceTypesForAVS);

			source2.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfProduction;
			header.AddInfoValidation.ValidateUS_CategoryType();
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateSourceTypesForAVS);
		}

		public void TestValidateForAPQWithMescellaneousCategoryType()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts;
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateSourceTypeForAPQ);

			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			AssertHasMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateSourceTypeForAPQ);
			var source1 = header.Sources.AddNew();
			source1.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfManipulation;
			AssertHasMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateSourceTypeForAPQ);

			var source2 = header.Sources.AddNew();
			source2.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
			header.AddInfoValidation.ValidateUS_CategoryType();
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateSourceTypeForAPQ);

			source2.US_SourceTypeCode = SourceTypeCodesList.Codes.PlaceOfGrowth;
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateSourceTypeForAPQ);

			source2.US_SourceTypeCode = SourceTypeCodesList.Codes.Harvested;
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateSourceTypeForAPQ);
		}

		public void TestCheckUS_OA_ApplicantAddressLicenseHolder()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			header.US_OA_ApplicantAddress = ZGuid.Empty;

			AssertNoMessageError(header.US_OA_ApplicantAddressInfo, ValidationConstants.APHIS.ApplicantIsRequired);

			header.Licenses.AddNew();

			header.AddInfoValidation.ValidateUS_OA_ApplicantAddress();
			AssertHasMessageError(header.US_OA_ApplicantAddressInfo, ValidationConstants.APHIS.ApplicantIsRequired);
		}

		public void TestValidateForsPropagativeMaterialPG10()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.BulbsAndUndergroundPortionsOfDormantPerennials;
			var source1 = header.Sources.AddNew();
			source1.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfProduction;

			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10PropagativeMaterial);
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.SeedsForPlantingForSowing;
			AssertHasMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10PropagativeMaterial);
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.PlantsForPlantingOrPropagationWhole;
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10PropagativeMaterial);
			AssertHasMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.PlantCuttingsForPlantingOrPropagation;
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10PropagativeMaterial);
			AssertHasMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.RootCuttingsOrRootCrownForPlantingOrPropagation;
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10PropagativeMaterial);
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.MeristemTissue;
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10PropagativeMaterial);
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.BudwoodGraftwood;
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10PropagativeMaterial);
			AssertHasMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);
		}

		public void TestValidateForPG10()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts;
			header.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.MeatAndPoultryProducts;

			AssertHasMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);
			header.US_ProductPhysicalState = "AK";
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);
			header.US_ProductPhysicalState = "";
			header.US_ProductCondition = "DE";
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_ProductCondition = "";
			header.US_ProductStatus = "A";
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_ProductStatus = "";
			header.US_ProductComponent = "A";
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_ProductComponent = "";
			header.Products.AddNew();
			header.AddInfoValidation.ValidateUS_CategoryType();
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.Products.RemoveAndDeleteAll();
			header.AddInfoValidation.ValidateUS_CategoryType();
			AssertHasMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);

			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.FruitsAndVegetables;
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, USAPHISHeaderAddInfoValidation.ValidateForPG10);
		}

		public void TestCheckUS_OA_PermittedAddress()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			header.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForResearchAndEvaluation;
			header.US_OA_PermittedAddress = ZGuid.Empty;
			AssertEquals(false, header.IsPermittedNotApplicable);
			AssertEquals(false, header.IsPermittedAddressRequired);
			header.AddInfoValidation.ValidateUS_OA_PermittedAddress();
			AssertNoMessageErrorContaining(header.US_OA_PermittedAddressInfo, ValidationConstants.APHIS.PermittedDestinationIsRequired);

			header.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.AnimalConsumptionProducts;
			AssertNoMessageErrorContaining(header.US_OA_PermittedAddressInfo, ValidationConstants.APHIS.PermittedDestinationIsRequired);
			header.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForSaleAndDistribution;
			AssertEquals(false, header.IsPermittedNotApplicable);
			AssertEquals(true, header.IsPermittedAddressRequired);
			AssertHasMessageErrorContaining(header.US_OA_PermittedAddressInfo, ValidationConstants.APHIS.PermittedDestinationIsRequired);
			header.US_OA_PermittedAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AssertNoMessageErrorContaining(header.US_OA_PermittedAddressInfo, ValidationConstants.APHIS.PermittedDestinationIsRequired);

			var orgheader = Factory.New<OrgHeader>();
			header.US_OA_PermittedAddress = orgheader.MainAddress.PK;
			var license = header.Licenses.AddNew();
			license.US_Type = APHISLicenseTypeList.Codes.Aphis2006ResearchAndEvaluation;
			AssertHasMessageErrorContaining(header.US_OA_PermittedAddressInfo, USAPHISHeaderAddInfoValidation.APHISIDRequired);

			license.US_Type = APHISLicenseTypeList.Codes.Aphis2006SaleAndDistribution;
			AssertHasMessageErrorContaining(header.US_OA_PermittedAddressInfo, USAPHISHeaderAddInfoValidation.APHISIDRequired);

			orgheader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.APHISAssignedNumber, "1234", Core.Constants.CountryCodes.UnitedStates);
			header.AddInfoValidation.ValidateUS_OA_PermittedAddress();
			AssertNoMessageErrorContaining(header.US_OA_PermittedAddressInfo, USAPHISHeaderAddInfoValidation.APHISIDRequired);
		}

		public void TestCheck_ProductNumber()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProductType = ProductCodeQualifiersList.Codes.FDAProductCode;
			header.US_ProductNumber = "APH0001";
			AssertNoMessageErrorContaining(header.US_ProductNumberInfo, USAPHISHeaderAddInfoValidation.ProductNumberFormat);
			AssertNoMessageErrorContaining(header.US_ProductNumberInfo, USAPHISHeaderAddInfoValidation.ProductNumberFormat);

			header.US_ProductType = ProductCodeQualifiersList.Codes.APHISVeterinaryBiologics;
			AssertHasMessageErrorContaining(header.US_ProductNumberInfo, USAPHISHeaderAddInfoValidation.ProductNumberFormat);

			header.US_ProductNumber = "APH0.01";
			AssertNoMessageErrorContaining(header.US_ProductNumberInfo, USAPHISHeaderAddInfoValidation.ProductNumberFormat);
		}

		public void TestCheckUS_CategoryType()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			header.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.MeatAndPoultryProducts;
			header.US_ProductPhysicalState = "#";
			header.AddInfoValidation.ValidateUS_CategoryType();
			AssertHasMessageErrorContaining(header.US_CategoryTypeInfo, ValidationConstants.APHIS.CategoryAndBreedRequiredForLiveAnimals);

			var product = header.Products.AddNew();
			product.US_ShowBreed = APHISBreedList.Codes.BuffaloBison;

			header.AddInfoValidation.ValidateUS_CategoryType();
			AssertNoMessageErrorContaining(header.US_CategoryTypeInfo, ValidationConstants.APHIS.CategoryAndBreedRequiredForLiveAnimals);
		}

		public void TestValidateThatThereAreSources()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;

			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
			header.AddInfoValidation.ValidateAll();
			AssertNoMessageErrorContaining(header.US_ProgramTypeInfo, USAPHISHeaderAddInfoValidation.AtLeastOneSourcesShouldExistWith267);

			header.Sources.Cast<APHISSource>().ForEach(x => x.US_SourceTypeCode = ZString.Empty);
			header.AddInfoValidation.ValidateAll();
			AssertHasMessageErrorContaining(header.US_ProgramTypeInfo, USAPHISHeaderAddInfoValidation.AtLeastOneSourcesShouldExistWith267);
			AssertNoMessageErrorContaining(header.US_ProgramTypeInfo, USAPHISHeaderAddInfoValidation.AtLeastOneSourcesForAllProgramType);

			header.US_ProgramType = APHISProgramCodeList.Codes.AAC;
			header.Sources.RemoveAndDeleteAll();
			header.AddInfoValidation.ValidateAll();
			AssertHasMessageErrorContaining(header.US_ProgramTypeInfo, USAPHISHeaderAddInfoValidation.AtLeastOneSourcesForAllProgramType);
		}

		public void TestValidateThatThereAreRoutings()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var source2 = header.Sources.AddNew();
			source2.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
			var routing = header.Routings.AddNew();

			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			header.AddInfoValidation.ValidateAll();
			AssertNoMessageErrorContaining(header.US_ProgramTypeInfo, USAPHISHeaderAddInfoValidation.AtLeastOneRoutingShouldExistWith198);

			header.Routings.Cast<APHISRouting>().ForEach(x => x.US_Type = ZString.Empty);
			header.AddInfoValidation.ValidateAll();
			AssertHasMessageErrorContaining(header.US_ProgramTypeInfo, USAPHISHeaderAddInfoValidation.AtLeastOneRoutingShouldExistWith198);
		}

		public void TestEnsureThatAtLeastOneContainerSelected()
		{
			InvoiceLine.Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var header = InvoiceLine.APHISHeaders.AddNew();

			var container1 = InvoiceLine.Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRUX1234562";
			var npContainer1 = InvoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1);
			npContainer1.IsForInvoiceLine = true;

			header.AddInfoValidation.ValidateAll();
			AssertNoMessageErrorContaining(header.US_ProgramTypeInfo, USAPHISHeaderAddInfoValidation.AtLeastOneContainerRequired);

			npContainer1.IsForInvoiceLine = false;
			header.AddInfoValidation.ValidateAll();
			AssertHasMessageErrorContaining(header.US_ProgramTypeInfo, USAPHISHeaderAddInfoValidation.AtLeastOneContainerRequired);

			npContainer1.IsForInvoiceLine = true;
			header.AddInfoValidation.ValidateAll();
			AssertNoMessageErrorContaining(header.US_ProgramTypeInfo, USAPHISHeaderAddInfoValidation.AtLeastOneContainerRequired);
		}

		public void TestCheckUS_ProgramType()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = "#";
			AssertHasMessageErrorContaining(header.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(header.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
			header.US_ProgramType = ZString.Empty;
			AssertNoMessageErrorContaining(header.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(header.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_ProgramType();
			AssertNoMessageErrorContaining(header.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(header.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_ProgramType();
			AssertNoMessageErrorContaining(header.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(header.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
			foreach (ICodeDescription pair in APHISProgramCodeList.GetActiveList(Factory))
			{
				header.US_ProgramType = pair.Code;
				AssertNoMessageErrorContaining(header.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageErrorContaining(header.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckUS_VehicleLength()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_VehicleNumber = "323KDS";
			AssertHasMessageError(header.US_VehicleLengthInfo, ValidationConstants.APHIS.VehicleLengthIsRequiredWhenLicencePlateIsSpecified);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_VehicleLength();
			AssertNoMessageError(header.US_VehicleLengthInfo, ValidationConstants.APHIS.VehicleLengthIsRequiredWhenLicencePlateIsSpecified);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_VehicleLength();
			AssertHasMessageError(header.US_VehicleLengthInfo, ValidationConstants.APHIS.VehicleLengthIsRequiredWhenLicencePlateIsSpecified);
			header.US_VehicleLength = 1;
			AssertNoMessageError(header.US_VehicleLengthInfo, ValidationConstants.APHIS.VehicleLengthIsRequiredWhenLicencePlateIsSpecified);
			header.US_VehicleLength = ZShort.Zero;
			AssertHasMessageError(header.US_VehicleLengthInfo, ValidationConstants.APHIS.VehicleLengthIsRequiredWhenLicencePlateIsSpecified);
			header.US_VehicleNumber = ZString.Empty;
			AssertNoMessageErrors(header.US_VehicleLengthInfo);
		}

		public void TestCheckUS_ProcessingCode()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_ProcessingCode = "#";
			AssertHasMessageErrorContaining(header.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			header.US_ProcessingCode = ZString.Empty;
			AssertNoMessageErrorContaining(header.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(header.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(header.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in APHISGovernmentAgencyProcessingCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.AVS))
			{
				header.US_ProcessingCode = pair.Code;
				AssertNoMessageErrorContaining(header.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_ProcessingCodeA04ProgramCodeAAC()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AAC;
			header.US_ProcessingCode = PGAAgencyProcessingCodeList.Codes.AphisVsPortVeterinarian;
			AssertHasWarningContaining(header.US_ProcessingCodeInfo, ValidationConstants.APHIS.APHISA04OnlySelectedIfArrivingFromScrewwormCountry);
			header.US_ProcessingCode = PGAAgencyProcessingCodeList.Codes.AphisVsAnimalImportCenter;
			AssertNoWarningContaining(header.US_ProcessingCodeInfo, ValidationConstants.APHIS.APHISA04OnlySelectedIfArrivingFromScrewwormCountry);
		}

		public void TestCheckUS_IntendedUseCode()
		{
			var startDate = ZDateTime.Today.AddMonths(-1);
			var endDate = ZDateTime.Today.AddMonths(1);
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var codeType1 = helper.CreateNewOrGetExistingCusCodeType(CustomsUniversal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, CustomsUniversal.RefCusCodeListTypes.Codes.USPGAIntendUseCode);
			var codeType2 = helper.CreateNewOrGetExistingCusCodeType(CustomsUniversal.RefCusCodeListTypes.Codes.USPGALineStatus, CustomsUniversal.RefCusCodeListTypes.Codes.USPGALineStatus);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType1.ZZK_CodeType, "AAA", "AAA", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType2.ZZK_CodeType, "BBB", "BBB", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType1.ZZK_CodeType, "CCC", "CCC", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType1.ZZK_CodeType, "DDD", "DDD", startDate, endDate);
			var code5 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType1.ZZK_CodeType, "EEE", "EEE", startDate, endDate);
			var code6 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType1.ZZK_CodeType, "980.000", "980.000", startDate, endDate);

			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(CustomsUniversal.RefCusCodeList.Attributes.USPGAIntendUseCodeAgency, CustomsUniversal.RefCusCodeList.Attributes.USPGAIntendUseCodeAgency, CustomsUniversal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(CustomsUniversal.RefCusCodeList.Attributes.USPGAIntendUseCodeProgram, CustomsUniversal.RefCusCodeList.Attributes.USPGAIntendUseCodeProgram, CustomsUniversal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, Core.Constants.CountryCodes.UnitedStates);

			var attribute11 = helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, attributeName1.ZXE_Name, CustomsUniversal.RefCusCodeList.AttributeValues.APH);
			var attribute21 = helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeName1.ZXE_Name, CustomsUniversal.RefCusCodeList.AttributeValues.APH);
			var attribute41 = helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, attributeName1.ZXE_Name, CustomsUniversal.RefCusCodeList.AttributeValues.APH);
			var attribute42 = helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, attributeName2.ZXE_Name, CustomsUniversal.RefCusCodeList.AttributeValues.APH);
			var attribute52 = helper.CreateNewOrGetExistingCusCodeListAttribute(code5.PK, attributeName2.ZXE_Name, CustomsUniversal.RefCusCodeList.AttributeValues.APH);
			Factory.Save();

			var header = InvoiceLine.APHISHeaders.AddNew();
			var validation = header.AddInfoValidation;
			validation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(header.US_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoNotifications(header.US_IntendedUseCodeDescInfo);

			header.US_IntendedUseCode = code1.ZZD_Code;
			AssertNoNotifications(header.US_IntendedUseCodeInfo);
			AssertNoNotifications(header.US_IntendedUseCodeDescInfo);

			header.US_IntendedUseCode = code2.ZZD_Code;
			AssertHasMessageError(header.US_IntendedUseCodeInfo, USAPHISHeaderAddInfoValidation.PGAIntendUseCodeNotFound);
			AssertNoNotifications(header.US_IntendedUseCodeDescInfo);

			header.US_IntendedUseCode = code3.ZZD_Code;
			AssertHasWarning(header.US_IntendedUseCodeInfo, USAPHISHeaderAddInfoValidation.PGAIntendUseCodeHasNoAPHAttributeValue);
			AssertNoNotifications(header.US_IntendedUseCodeDescInfo);

			header.US_IntendedUseCode = code4.ZZD_Code;
			AssertNoNotifications(header.US_IntendedUseCodeInfo);
			AssertNoNotifications(header.US_IntendedUseCodeDescInfo);

			header.US_IntendedUseCode = code5.ZZD_Code;
			AssertHasWarning(header.US_IntendedUseCodeInfo, USAPHISHeaderAddInfoValidation.PGAIntendUseCodeHasNoAPHAttributeValue);
			AssertNoNotifications(header.US_IntendedUseCodeDescInfo);

			header.US_IntendedUseCode = USAPHISHeaderAddInfoValidation.ForOtherUse;
			AssertHasMessageErrorContaining(header.US_IntendedUseDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			header.US_IntendedUseDescription = "TEST";
			validation.ValidateUS_IntendedUseCode();
			AssertNoNotifications(header.US_IntendedUseDescriptionInfo);
		}

		public void TestCheckUS_IntendedUseDescription()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_IntendedUseCode = USAPHISHeaderAddInfoValidation.ForOtherUse;
			header.US_IntendedUseDescription = "SDS";
			AssertNoMessageErrorContaining(header.US_IntendedUseDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			header.US_IntendedUseDescription = ZString.Empty;
			AssertHasMessageErrorContaining(header.US_IntendedUseDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_IntendedUseDescription();
			AssertNoMessageErrorContaining(header.US_IntendedUseDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_IntendedUseDescription();
			AssertHasMessageErrorContaining(header.US_IntendedUseDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			header.US_IntendedUseCode = "090.002";
			AssertNoMessageErrorContaining(header.US_IntendedUseDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_CategoryCode()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			header.US_CategoryCode = "#";
			AssertHasMessageErrorContaining(header.US_CategoryCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(header.US_CategoryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			header.US_CategoryCode = ZString.Empty;
			AssertNoMessageErrorContaining(header.US_CategoryCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(header.US_CategoryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_CategoryCode();
			AssertNoMessageErrorContaining(header.US_CategoryCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(header.US_CategoryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_CategoryCode();
			AssertNoMessageErrorContaining(header.US_CategoryCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(header.US_CategoryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			foreach (ICodeDescription pair in new LiveAnimalsList())
			{
				header.US_CategoryCode = pair.Code;
				AssertNoMessageErrorContaining(header.US_CategoryCodeInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageErrorContaining(header.US_CategoryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckUS_CommoditySpecificName()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_CommoditySpecificName = "a";
			AssertNoMessageErrorContaining(header.US_CommoditySpecificNameInfo, MandatoryValidation.YouHaveNotEntered);
			header.US_CommoditySpecificName = ZString.Empty;
			AssertHasMessageErrorContaining(header.US_CommoditySpecificNameInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_CommoditySpecificName();
			AssertNoMessageErrorContaining(header.US_CommoditySpecificNameInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_CommoditySpecificName();
			AssertHasMessageErrorContaining(header.US_CommoditySpecificNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_OA_ApplicantAddress()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			var header = InvoiceLine.APHISHeaders.AddNew();
			header.Licenses.AddNew();

			var list = new APHISCategoryTypeCodeList();
			foreach (var code in new[]
				{
					APHISCategoryTypeCodeList.Codes.LiveAnimals,
					 APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts,
					 APHISCategoryTypeCodeList.Codes.PropagativeMaterial,
					 APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting,
					 APHISCategoryTypeCodeList.Codes.FruitsAndVegetables,
					 APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts,
					 APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms
				})
			{
				list.RemoveCode(code);
				header.US_CategoryType = code;
				header.US_OA_ApplicantAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				AssertNoMessageError(header.US_OA_ApplicantAddressInfo, ValidationConstants.APHIS.ApplicantIsRequired);
				header.US_OA_ApplicantAddress = ZGuid.Empty;
				AssertHasMessageError(header.US_OA_ApplicantAddressInfo, ValidationConstants.APHIS.ApplicantIsRequired);
				Declaration.ValidationModes = ValidationModes.None;
				header.AddInfoValidation.ValidateUS_OA_ApplicantAddress();
				AssertNoMessageError(header.US_OA_ApplicantAddressInfo, ValidationConstants.APHIS.ApplicantIsRequired);
				Declaration.RecalculateValidationModesOnDeclaration();
				header.AddInfoValidation.ValidateUS_OA_ApplicantAddress();
				AssertHasMessageError(header.US_OA_ApplicantAddressInfo, ValidationConstants.APHIS.ApplicantIsRequired);
			}

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "USABV";
			header.US_OA_ApplicantAddress = address.PK;
			header.AddInfoValidation.ValidateUS_OA_ApplicantAddress();
			AssertHasMessageErrorContaining(header.US_OA_ApplicantAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			header.US_OA_ApplicantAddress = orgHeader.MainAddress.PK;
			header.AddInfoValidation.ValidateUS_OA_ApplicantAddress();
			AssertNoMessageErrorContaining(header.US_OA_ApplicantAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			header.US_OA_ApplicantAddress = orgHeader.MainAddress.PK;
			header.AddInfoValidation.ValidateUS_OA_ApplicantAddress();
			AssertNoMessageErrorContaining(header.US_OA_ApplicantAddressInfo, "The state is not a valid");
		}

		public void TestUS_OA_ApplicantAddressAPHISNumberRequired()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_OA_ApplicantAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			var errorMessage = ValidationConstants.APHIS.NumberRequired("Authorized License Holder", "APHIS Establishment Number");
			AssertNoMessageError(header.US_OA_ApplicantAddressInfo, errorMessage);

			var license = header.Licenses.AddNew();
			license.US_Type = APHISLicenseTypeList.Codes.Aphis2006ResearchAndEvaluation;
			header.AddInfoValidation.ValidateUS_OA_ApplicantAddress();
			AssertHasMessageError(header.US_OA_ApplicantAddressInfo, errorMessage);

			license.US_Type = APHISLicenseTypeList.Codes.Aphis2006SaleAndDistribution;
			header.AddInfoValidation.ValidateUS_OA_ApplicantAddress();
			AssertHasMessageError(header.US_OA_ApplicantAddressInfo, errorMessage);
		}

		public void TestCheckUS_OA_CropGrowerAddress()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			var header = InvoiceLine.APHISHeaders.AddNew();
			var list = new APHISCategoryTypeCodeList();
			list.RemoveCode(APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery);
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			header.US_OA_CropGrowerAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			AssertNoMessageErrorContaining(header.US_OA_CropGrowerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			header.US_OA_CropGrowerAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(header.US_OA_CropGrowerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_OA_CropGrowerAddress();
			AssertNoMessageErrorContaining(header.US_OA_CropGrowerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_OA_CropGrowerAddress();
			AssertHasMessageErrorContaining(header.US_OA_CropGrowerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			header.US_OA_ApplicantAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			AssertNoMessageErrorContaining(header.US_OA_CropGrowerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			header.US_OA_ApplicantAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(header.US_OA_CropGrowerAddressInfo, MandatoryValidation.YouHaveNotEntered);

			foreach (ICodeDescription pair in list)
			{
				header.US_CategoryType = pair.Code;
				AssertNoMessageErrorContaining(header.US_OA_CropGrowerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			}

			var errorMessage = ValidationConstants.APHIS.NumberRequired("Crop Grower", "either one of APHIS Assigned, MID and D&D Assigned number");
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			header.US_OA_CropGrowerAddress = orgHeader.MainAddress.PK;
			AssertHasMessageErrorContaining(header.US_OA_CropGrowerAddressInfo, errorMessage);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.APHISAssignedNumber, "23GGFRD234");
			header.AddInfoValidation.ValidateUS_OA_CropGrowerAddress();
			AssertNoMessageErrorContaining(header.US_OA_CropGrowerAddressInfo, errorMessage);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "123");
			header.AddInfoValidation.ValidateUS_OA_CropGrowerAddress();
			AssertNoMessageErrorContaining(header.US_OA_CropGrowerAddressInfo, errorMessage);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "789AAA456");
			header.AddInfoValidation.ValidateUS_OA_CropGrowerAddress();
			AssertNoMessageErrorContaining(header.US_OA_CropGrowerAddressInfo, errorMessage);

			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;

			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "USABC";
			header.US_OA_CropGrowerAddress = address.PK;
			header.AddInfoValidation.ValidateUS_OA_CropGrowerAddress();
			AssertHasMessageErrorContaining(header.US_OA_CropGrowerAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			header.US_OA_CropGrowerAddress = orgHeader.MainAddress.PK;
			header.AddInfoValidation.ValidateUS_OA_CropGrowerAddress();
			AssertNoMessageErrorContaining(header.US_OA_CropGrowerAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			header.US_OA_CropGrowerAddress = orgHeader.MainAddress.PK;
			header.AddInfoValidation.ValidateUS_OA_CropGrowerAddress();
			AssertNoMessageErrorContaining(header.US_OA_CropGrowerAddressInfo, "The state is not a valid");
		}

		public void TestCheckUS_OA_ShipperAddress()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			var header = InvoiceLine.APHISHeaders.AddNew();
			var list = new APHISCategoryTypeCodeList();
			list.RemoveCode(APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts);
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts;
			header.US_CategoryCode = ArticleCategory.RelatedAnimalProductsList.Codes.UsedFarmMachinery;
			header.US_OA_ShipperAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			AssertNoMessageError(header.US_OA_ShipperAddressInfo, ValidationConstants.APHIS.ShipperIsRequiredWhenApplicantIsNotSpecified);
			header.US_OA_ShipperAddress = ZGuid.Empty;
			AssertHasMessageError(header.US_OA_ShipperAddressInfo, ValidationConstants.APHIS.ShipperIsRequiredWhenApplicantIsNotSpecified);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_OA_ShipperAddress();
			AssertNoMessageError(header.US_OA_ShipperAddressInfo, ValidationConstants.APHIS.ShipperIsRequiredWhenApplicantIsNotSpecified);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_OA_ShipperAddress();
			AssertHasMessageError(header.US_OA_ShipperAddressInfo, ValidationConstants.APHIS.ShipperIsRequiredWhenApplicantIsNotSpecified);
			header.US_OA_ApplicantAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			AssertNoMessageError(header.US_OA_ShipperAddressInfo, ValidationConstants.APHIS.ShipperIsRequiredWhenApplicantIsNotSpecified);
			header.US_OA_ApplicantAddress = ZGuid.Empty;
			AssertHasMessageError(header.US_OA_ShipperAddressInfo, ValidationConstants.APHIS.ShipperIsRequiredWhenApplicantIsNotSpecified);
			header.US_CategoryCode = ArticleCategory.RelatedAnimalProductsList.Codes.AnimalCarriers;
			AssertNoMessageError(header.US_OA_ShipperAddressInfo, ValidationConstants.APHIS.ShipperIsRequiredWhenApplicantIsNotSpecified);
			header.US_CategoryCode = ArticleCategory.RelatedAnimalProductsList.Codes.UsedFarmMachinery;
			AssertHasMessageError(header.US_OA_ShipperAddressInfo, ValidationConstants.APHIS.ShipperIsRequiredWhenApplicantIsNotSpecified);

			foreach (ICodeDescription pair in list)
			{
				header.US_CategoryType = pair.Code;
				AssertNoMessageError(header.US_OA_ShipperAddressInfo, ValidationConstants.APHIS.ShipperIsRequiredWhenApplicantIsNotSpecified);
			}

			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts;
			header.US_CategoryCode = ArticleCategory.RelatedAnimalProductsList.Codes.UsedFarmMachinery;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "USABV";
			header.US_OA_ShipperAddress = address.PK;
			header.AddInfoValidation.ValidateUS_OA_CropGrowerAddress();
			AssertHasMessageErrorContaining(header.US_OA_ShipperAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			header.US_OA_ShipperAddress = orgHeader.MainAddress.PK;
			header.AddInfoValidation.ValidateUS_OA_CropGrowerAddress();
			AssertNoMessageErrorContaining(header.US_OA_ShipperAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			header.US_OA_ShipperAddress = orgHeader.MainAddress.PK;
			header.AddInfoValidation.ValidateUS_OA_CropGrowerAddress();
			AssertNoMessageErrorContaining(header.US_OA_ShipperAddressInfo, "The state is not a valid");
		}

		public void TestCheckUS_ProductComponent()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			header.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.MeatAndPoultryProducts;
			header.US_ProductComponent = "#";
			AssertNoMessageErrors(header.US_ProductComponentInfo);
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			header.US_ProductComponent = "#";
			AssertHasMessageErrorContaining(header.US_ProductComponentInfo, ListValidation.InvalidCodeMessageError);
			header.US_ProductComponent = ZString.Empty;
			AssertNoMessageErrors(header.US_ProductComponentInfo);
			Declaration.ValidationModes = ValidationModes.None;
			header.US_ProductComponent = "#";
			AssertNoMessageErrorContaining(header.US_ProductComponentInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_ProductComponent();
			AssertHasMessageErrorContaining(header.US_ProductComponentInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new AnimalProductsAndByProductsConditionA32List())
			{
				header.US_ProductComponent = pair.Code;
				AssertNoMessageErrors(header.US_ProductComponentInfo);
			}
		}

		public void TestCheckUS_ProductCondition()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			header.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.MeatAndPoultryProducts;
			header.US_ProductCondition = "#";
			AssertNoMessageErrors(header.US_ProductConditionInfo);
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			AssertHasMessageErrorContaining(header.US_ProductConditionInfo, ListValidation.InvalidCodeMessageError);
			header.US_ProductCondition = ZString.Empty;
			AssertNoMessageErrors(header.US_ProductConditionInfo);
			Declaration.ValidationModes = ValidationModes.None;
			header.US_ProductCondition = "#";
			AssertNoMessageErrorContaining(header.US_ProductConditionInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_ProductCondition();
			AssertHasMessageErrorContaining(header.US_ProductConditionInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new AnimalProductsAndByProductsConditionA30List())
			{
				header.US_ProductCondition = pair.Code;
				AssertNoMessageErrors(header.US_ProductConditionInfo);
			}
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			header.US_ProductCondition = ZString.Empty;
			AssertHasMessageErrorContaining(header.US_ProductConditionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ProductConditionWhenAPQAndCutFlowersAndGreenery()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			header.US_ProductCondition = "";
			header.US_CategoryCode = CutFlowersAndGreeneryList.Codes.CutFlowersAndGreeneryMixed;
			AssertHasMessageErrorContaining(header.US_ProductConditionInfo, "You have not entered a value.");
			header.US_CategoryCode = CutFlowersAndGreeneryList.Codes.CutFlowers;
			AssertNoMessageErrorContaining(header.US_ProductConditionInfo, "You have not entered a value.");
		}

		public void TestCheckUS_ProductCondition_AssociatedWithScientificNames()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_CategoryType = categoryTypeOfScientificDataRequired[0];
			header.US_ProductIngredientType = ZString.Empty;
			var errorGenus = ValidationConstants.APHIS.ScientificDataIsRequiredForCategory("Scientific Genus Name", header.US_CategoryType);
			var errorSpecies = ValidationConstants.APHIS.ScientificDataIsRequiredForCategory("Scientific Species Name", header.US_CategoryType);
			AssertHasMessageError(header.US_ScientificGenusNameInfo, errorGenus);
			AssertHasMessageError(header.US_ScientificSpeciesNameInfo, errorSpecies);

			header.US_ProductIngredientType = IngredientTypeList.Codes.MixedIngredient;
			AssertNoMessageError(header.US_ScientificGenusNameInfo, errorGenus);
			AssertNoMessageError(header.US_ScientificSpeciesNameInfo, errorSpecies);
		}

		public void TestCheckUS_ProductPhysicalState()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			header.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.MeatAndPoultryProducts;
			header.US_ProductPhysicalState = "#";
			AssertNoMessageErrors(header.US_ProductPhysicalStateInfo);
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			AssertHasMessageErrorContaining(header.US_ProductPhysicalStateInfo, ListValidation.InvalidCodeMessageError);
			header.US_ProductPhysicalState = ZString.Empty;
			AssertNoMessageErrors(header.US_ProductPhysicalStateInfo);
			Declaration.ValidationModes = ValidationModes.None;
			header.US_ProductPhysicalState = "#";
			AssertNoMessageErrorContaining(header.US_ProductPhysicalStateInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_ProductPhysicalState();
			AssertHasMessageErrorContaining(header.US_ProductPhysicalStateInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new AnimalProductsAndByProductsConditionA31List())
			{
				header.US_ProductPhysicalState = pair.Code;
				AssertNoMessageErrors(header.US_ProductPhysicalStateInfo);
			}
		}

		public void TestCheckUS_ProductStatus()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			header.US_ProductStatus = "#";
			AssertNoMessageErrors(header.US_ProductStatusInfo);
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			AssertHasMessageErrorContaining(header.US_ProductStatusInfo, ListValidation.InvalidCodeMessageError);
			header.US_ProductStatus = ZString.Empty;
			AssertNoMessageErrors(header.US_ProductStatusInfo);
			Declaration.ValidationModes = ValidationModes.None;
			header.US_ProductStatus = "#";
			AssertNoMessageErrorContaining(header.US_ProductStatusInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_ProductStatus();
			AssertHasMessageErrorContaining(header.US_ProductStatusInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new PropagativeMaterialLifeStageA42List())
			{
				header.US_ProductStatus = pair.Code;
				AssertNoMessageErrors(header.US_ProductStatusInfo);
			}
		}

		public void TestCheckUS_ProductType()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			var list = new APHISCategoryTypeCodeList();
			header.US_ProductType = "!";
			AssertHasMessageErrorContaining(header.US_ProductTypeInfo, ListValidation.InvalidCodeMessageError);
			header.US_ProductType = ZString.Empty;
			AssertNoMessageErrorContaining(header.US_ProductTypeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_ProductType();
			foreach (ICodeDescription pair in header.AddInfoLookups.ProductTypes)
			{
				header.US_ProductType = pair.Code;
				AssertNoMessageErrors(pair.Code, header.US_ProductTypeInfo);
			}

			header.US_CategoryCode = AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForResearchAndEvaluation;

			var license = header.Licenses.AddNew();
			license.US_Type = APHISLicenseTypeList.Codes.Aphis2006ResearchAndEvaluation;

			header.AddInfoValidation.ValidateUS_CategoryType();
			AssertHasMessageErrorContaining(header.US_ProductTypeInfo, USAPHISHeaderAddInfoValidation.AVBRequiredFor);

			header.US_CategoryCode = AnimalProductsAndByProductsList.Codes.AnimalByProductsForTechnicalUse;
			AssertNoMessageErrorContaining(header.US_ProductTypeInfo, USAPHISHeaderAddInfoValidation.AVBRequiredFor);

			header.US_CategoryCode = AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForSaleAndDistribution;
			AssertHasMessageErrorContaining(header.US_ProductTypeInfo, USAPHISHeaderAddInfoValidation.AVBRequiredFor);
		}

		public void TestCheckUS_Qty1()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_Qty1 = -3m;
			AssertHasMessageErrorContaining(header.US_Qty1Info, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(header.US_Qty1Info, MandatoryValidation.ValueCannotBeZero);
			header.US_Qty1 = ZDecimal.Zero;
			AssertNoMessageErrorContaining(header.US_Qty1Info, MandatoryValidation.ValueCannotBeNegative);
			AssertHasMessageErrorContaining(header.US_Qty1Info, MandatoryValidation.ValueCannotBeZero);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_Qty1();
			AssertNoMessageErrorContaining(header.US_Qty1Info, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(header.US_Qty1Info, MandatoryValidation.ValueCannotBeZero);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_Qty1();
			AssertNoMessageErrorContaining(header.US_Qty1Info, MandatoryValidation.ValueCannotBeNegative);
			AssertHasMessageErrorContaining(header.US_Qty1Info, MandatoryValidation.ValueCannotBeZero);
			header.US_Qty1 = 1m;
			AssertNoMessageErrorContaining(header.US_Qty1Info, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(header.US_Qty1Info, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckUS_UQ1()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_UQ1 = "PK";
			AssertHasMessageErrorContaining(header.US_UQ1Info, ListValidation.InvalidCodeMessageError);
			header.US_UQ1 = "PKG";
			AssertNoMessageErrorContaining(header.US_UQ1Info, ListValidation.InvalidCodeMessageError);
			header.US_UQ1 = "@";
			AssertHasMessageErrorContaining(header.US_UQ1Info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(header.US_UQ1Info, MandatoryValidation.YouHaveNotEntered);
			header.US_UQ1 = ZString.Empty;
			AssertNoMessageErrorContaining(header.US_UQ1Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(header.US_UQ1Info, MandatoryValidation.YouHaveNotEntered);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_UQ1();
			AssertNoMessageErrorContaining(header.US_UQ1Info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(header.US_UQ1Info, MandatoryValidation.YouHaveNotEntered);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_UQ1();
			AssertNoMessageErrorContaining(header.US_UQ1Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(header.US_UQ1Info, MandatoryValidation.YouHaveNotEntered);
			var unitOfMeasureList = new APHISUnitOfMeasureList();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				Factory.ClearCachedValue<CodeDescriptionPairList>("APHISUnitOfMeasureList" + header.US_CategoryType);
				foreach (ICodeDescription pair in unitOfMeasureList)
				{
					header.US_UQ1 = pair.Code;
					AssertNoMessageErrorContaining(header.US_UQ1Info, ListValidation.InvalidCodeMessageError);
					AssertNoMessageErrorContaining(header.US_UQ1Info, MandatoryValidation.YouHaveNotEntered);
				}
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				Factory.ClearCachedValue<CodeDescriptionPairList>("APHISUnitOfMeasureList" + header.US_CategoryType);
				unitOfMeasureList.RemoveCode(APHISUnitOfMeasureList.Codes.AnimalUnit);
				unitOfMeasureList.RemoveCode(APHISUnitOfMeasureList.Codes.StrawsAmpulesDoses);
				foreach (ICodeDescription pair in unitOfMeasureList)
				{
					header.US_UQ1 = pair.Code;
					AssertNoMessageErrorContaining(header.US_UQ1Info, ListValidation.InvalidCodeMessageError);
					AssertNoMessageErrorContaining(header.US_UQ1Info, MandatoryValidation.YouHaveNotEntered);
				}
			}
		}

		public void TestCheckUS_Qty2()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_Qty1 = ZDecimal.Zero;
			header.US_UQ2 = APHISUnitOfMeasureList.Codes.Bag;
			header.US_Qty2 = -8m;
			var qtyMessage = ValidationConstants.APHIS.QuantityANeedsToBeSpecifiedBeforeQuantityBCanBeSpecified("1", "2");
			var messageError = ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Quantity 2", "Quantity Unit 2");
			AssertHasMessageErrorContaining(header.US_Qty2Info, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageError(header.US_Qty2Info, messageError);
			AssertNoMessageError(header.US_Qty2Info, qtyMessage);
			header.US_Qty2 = ZDecimal.Zero;
			AssertNoMessageErrorContaining(header.US_Qty2Info, MandatoryValidation.ValueCannotBeNegative);
			AssertHasMessageError(header.US_Qty2Info, messageError);
			AssertNoMessageError(header.US_Qty2Info, qtyMessage);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_Qty2();
			AssertNoMessageErrorContaining(header.US_Qty2Info, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageError(header.US_Qty2Info, messageError);
			AssertNoMessageError(header.US_Qty2Info, qtyMessage);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_Qty2();
			AssertNoMessageErrorContaining(header.US_Qty2Info, MandatoryValidation.ValueCannotBeNegative);
			AssertHasMessageError(header.US_Qty2Info, messageError);
			AssertNoMessageError(header.US_Qty2Info, qtyMessage);
			header.US_Qty2 = 9m;
			AssertNoMessageErrorContaining(header.US_Qty2Info, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageError(header.US_Qty2Info, messageError);
			AssertHasMessageError(header.US_Qty2Info, qtyMessage);
			header.US_Qty1 = 9m;
			AssertNoMessageErrorContaining(header.US_Qty2Info, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageError(header.US_Qty2Info, messageError);
			AssertNoMessageError(header.US_Qty2Info, qtyMessage);
			header.US_UQ2 = ZString.Empty;
			header.US_Qty2 = ZDecimal.Zero;
			AssertNoMessageErrorContaining(header.US_Qty2Info, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageError(header.US_Qty2Info, messageError);
			AssertNoMessageError(header.US_Qty2Info, qtyMessage);
		}

		public void TestCheckUS_UQ2()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_Qty2 = 8m;
			header.US_UQ2 = "@";
			var messageError = ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Quantity Unit 2", "Quantity 2");
			AssertHasMessageErrorContaining(header.US_UQ2Info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(header.US_UQ2Info, messageError);
			header.US_UQ2 = ZString.Empty;
			AssertNoMessageErrorContaining(header.US_UQ2Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(header.US_UQ2Info, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_UQ2();
			AssertNoMessageErrorContaining(header.US_UQ2Info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(header.US_UQ2Info, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_UQ2();
			AssertNoMessageErrorContaining(header.US_UQ2Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(header.US_UQ2Info, messageError);
			var unitOfMeasureList = new APHISUnitOfMeasureList();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				Factory.ClearCachedValue<CodeDescriptionPairList>("APHISUnitOfMeasureList" + header.US_CategoryType);
				foreach (ICodeDescription pair in unitOfMeasureList)
				{
					header.US_UQ2 = pair.Code;
					AssertNoMessageErrorContaining(header.US_UQ2Info, ListValidation.InvalidCodeMessageError);
					AssertNoMessageError(header.US_UQ1Info, messageError);
				}
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				Factory.ClearCachedValue<CodeDescriptionPairList>("APHISUnitOfMeasureList" + header.US_CategoryType);
				unitOfMeasureList.RemoveCode(APHISUnitOfMeasureList.Codes.AnimalUnit);
				unitOfMeasureList.RemoveCode(APHISUnitOfMeasureList.Codes.StrawsAmpulesDoses);
				foreach (ICodeDescription pair in unitOfMeasureList)
				{
					header.US_UQ2 = pair.Code;
					AssertNoMessageErrorContaining(header.US_UQ2Info, ListValidation.InvalidCodeMessageError);
					AssertNoMessageError(header.US_UQ1Info, messageError);
				}
			}
			header.US_UQ2 = ZString.Empty;
			AssertNoMessageErrorContaining(header.US_UQ2Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(header.US_UQ2Info, messageError);
			header.US_Qty2 = ZDecimal.Zero;
			AssertNoMessageErrorContaining(header.US_UQ2Info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(header.US_UQ2Info, messageError);
		}

		public void TestCheckUS_Qty3()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_Qty2 = ZDecimal.Zero;
			header.US_UQ3 = APHISUnitOfMeasureList.Codes.Bag;
			header.US_Qty3 = -8m;
			var qtyMessage = ValidationConstants.APHIS.QuantityANeedsToBeSpecifiedBeforeQuantityBCanBeSpecified("2", "3");
			var messageError = ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Quantity 3", "Quantity Unit 3");
			AssertHasMessageErrorContaining(header.US_Qty3Info, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageError(header.US_Qty3Info, messageError);
			AssertNoMessageError(header.US_Qty3Info, qtyMessage);
			header.US_Qty3 = ZDecimal.Zero;
			AssertNoMessageErrorContaining(header.US_Qty3Info, MandatoryValidation.ValueCannotBeNegative);
			AssertHasMessageError(header.US_Qty3Info, messageError);
			AssertNoMessageError(header.US_Qty3Info, qtyMessage);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_Qty3();
			AssertNoMessageErrorContaining(header.US_Qty3Info, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageError(header.US_Qty3Info, messageError);
			AssertNoMessageError(header.US_Qty3Info, qtyMessage);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_Qty3();
			AssertNoMessageErrorContaining(header.US_Qty3Info, MandatoryValidation.ValueCannotBeNegative);
			AssertHasMessageError(header.US_Qty3Info, messageError);
			AssertNoMessageError(header.US_Qty3Info, qtyMessage);
			header.US_Qty3 = 9m;
			AssertNoMessageErrorContaining(header.US_Qty3Info, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageError(header.US_Qty3Info, messageError);
			AssertHasMessageError(header.US_Qty3Info, qtyMessage);
			header.US_Qty2 = 9m;
			AssertNoMessageErrorContaining(header.US_Qty3Info, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageError(header.US_Qty3Info, messageError);
			AssertNoMessageError(header.US_Qty3Info, qtyMessage);
			header.US_UQ3 = ZString.Empty;
			header.US_Qty3 = ZDecimal.Zero;
			AssertNoMessageErrorContaining(header.US_Qty3Info, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageError(header.US_Qty3Info, messageError);
			AssertNoMessageError(header.US_Qty3Info, qtyMessage);
		}

		public void TestCheckUS_UQ3()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_Qty3 = 8m;
			header.US_UQ3 = "@";
			var messageError = ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Quantity Unit 3", "Quantity 3");
			AssertHasMessageErrorContaining(header.US_UQ3Info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(header.US_UQ3Info, messageError);
			header.US_UQ3 = ZString.Empty;
			AssertNoMessageErrorContaining(header.US_UQ3Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(header.US_UQ3Info, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_UQ3();
			AssertNoMessageErrorContaining(header.US_UQ3Info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(header.US_UQ3Info, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_UQ3();
			AssertNoMessageErrorContaining(header.US_UQ3Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(header.US_UQ3Info, messageError);
			var unitOfMeasureList = new APHISUnitOfMeasureList();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				Factory.ClearCachedValue<CodeDescriptionPairList>("APHISUnitOfMeasureList" + header.US_CategoryType);
				foreach (ICodeDescription pair in unitOfMeasureList)
				{
					header.US_UQ3 = pair.Code;
					AssertNoMessageErrorContaining(header.US_UQ3Info, ListValidation.InvalidCodeMessageError);
					AssertNoMessageError(header.US_UQ1Info, messageError);
				}
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				Factory.ClearCachedValue<CodeDescriptionPairList>("APHISUnitOfMeasureList" + header.US_CategoryType);
				unitOfMeasureList.RemoveCode(APHISUnitOfMeasureList.Codes.AnimalUnit);
				unitOfMeasureList.RemoveCode(APHISUnitOfMeasureList.Codes.StrawsAmpulesDoses);
				foreach (ICodeDescription pair in unitOfMeasureList)
				{
					header.US_UQ3 = pair.Code;
					AssertNoMessageErrorContaining(header.US_UQ3Info, ListValidation.InvalidCodeMessageError);
					AssertNoMessageError(header.US_UQ1Info, messageError);
				}
			}
			header.US_UQ3 = ZString.Empty;
			AssertNoMessageErrorContaining(header.US_UQ3Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(header.US_UQ3Info, messageError);
			header.US_Qty3 = ZDecimal.Zero;
			AssertNoMessageErrorContaining(header.US_UQ3Info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(header.US_UQ3Info, messageError);
		}

		public void TestCheckUS_ScientificGenusName()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			var list = new APHISCategoryTypeCodeList();
			var scientificDataIsRequiredMessage = ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Scientific Genus Name", "Scientific Species Name");
			foreach (var code in categoryTypeOfScientificDataRequired)
			{
				list.RemoveCode(code);
				header.US_CategoryType = code;
				var messageError = ValidationConstants.APHIS.ScientificDataIsRequiredForCategory("Scientific Genus Name", code);
				header.US_ScientificSpeciesName = "A";
				header.US_ScientificGenusName = "A";
				AssertNoMessageError(code, header.US_ScientificGenusNameInfo, messageError);
				AssertNoMessageError(code, header.US_ScientificGenusNameInfo, scientificDataIsRequiredMessage);
				header.US_ScientificGenusName = ZString.Empty;
				AssertHasMessageError(code, header.US_ScientificGenusNameInfo, messageError);
				AssertNoMessageError(code, header.US_ScientificGenusNameInfo, scientificDataIsRequiredMessage);
				Declaration.ValidationModes = ValidationModes.None;
				header.AddInfoValidation.ValidateUS_ScientificGenusName();
				AssertNoMessageError(code, header.US_ScientificGenusNameInfo, messageError);
				AssertNoMessageError(code, header.US_ScientificGenusNameInfo, scientificDataIsRequiredMessage);
				Declaration.RecalculateValidationModesOnDeclaration();
				header.AddInfoValidation.ValidateUS_ScientificGenusName();
				AssertHasMessageError(code, header.US_ScientificGenusNameInfo, messageError);
				AssertNoMessageError(code, header.US_ScientificGenusNameInfo, scientificDataIsRequiredMessage);
			}
			foreach (ICodeDescription pair in list)
			{
				header.US_CategoryType = pair.Code;
				header.US_ScientificSpeciesName = "A";
				header.US_ScientificGenusName = "A";
				AssertNoMessageError(pair.Code, header.US_ScientificGenusNameInfo, scientificDataIsRequiredMessage);
				header.US_ScientificGenusName = ZString.Empty;
				AssertHasMessageError(pair.Code, header.US_ScientificGenusNameInfo, scientificDataIsRequiredMessage);
				Declaration.ValidationModes = ValidationModes.None;
				header.AddInfoValidation.ValidateUS_ScientificGenusName();
				AssertNoMessageError(pair.Code, header.US_ScientificGenusNameInfo, scientificDataIsRequiredMessage);
				Declaration.RecalculateValidationModesOnDeclaration();
				header.AddInfoValidation.ValidateUS_ScientificGenusName();
				AssertHasMessageError(pair.Code, header.US_ScientificGenusNameInfo, scientificDataIsRequiredMessage);
				header.US_ScientificSpeciesName = ZString.Empty;
				AssertNoMessageError(pair.Code, header.US_ScientificGenusNameInfo, scientificDataIsRequiredMessage);
				header.US_ScientificSpeciesName = "A";
				AssertHasMessageError(pair.Code, header.US_ScientificGenusNameInfo, scientificDataIsRequiredMessage);
			}

			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts;
			var messageErrorText = ValidationConstants.APHIS.ScientificDataIsRequiredForCategory("Scientific Genus Name", header.US_CategoryType);
			var categoryCodeList = new MiscellaneousAndProcessedProductsList();
			foreach (ICodeDescription pair in categoryCodeList)
			{
				header.US_CategoryCode = pair.Code;
				header.US_ScientificGenusName = ZString.Empty;
				header.AddInfoValidation.ValidateUS_ScientificGenusName();

				if (categoryCodeOfScientificDataRequiredCategoryCode.Contains(header.US_CategoryCode))
				{
					AssertHasMessageError(pair.Code, header.US_ScientificGenusNameInfo, messageErrorText);
				}
				else
				{
					AssertNoMessageError(pair.Code, header.US_ScientificGenusNameInfo, messageErrorText);
				}
			}
		}

		public void TestCheckUS_ScientificGenusName_IngredientTypeMIX()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			var allTypeList = new APHISCategoryTypeCodeList();
			var categoryCodeList = new MiscellaneousAndProcessedProductsList();
			foreach (ICodeDescription allTypePair in allTypeList)
			{
				header.US_CategoryType = allTypePair.Code;
				header.US_ProductIngredientType = ZString.Empty;
				var messageErrorText = ValidationConstants.APHIS.ScientificDataIsRequiredForCategory("Scientific Genus Name", header.US_CategoryType);
				if (allTypePair.Code == APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts)
				{
					foreach (ICodeDescription pair in categoryCodeList)
					{
						header.US_CategoryCode = pair.Code;
						header.US_ScientificGenusName = ZString.Empty;
						header.AddInfoValidation.ValidateUS_ScientificGenusName();

						header.US_ProductIngredientType = ZString.Empty;
						if (categoryCodeOfScientificDataRequiredCategoryCode.Contains(header.US_CategoryCode))
						{
							AssertHasMessageError(pair.Code, header.US_ScientificGenusNameInfo, messageErrorText);

							header.US_ProductIngredientType = IngredientTypeList.Codes.MixedIngredient;
							header.AddInfoValidation.ValidateUS_ScientificGenusName();
							AssertNoMessageError(pair.Code, header.US_ScientificGenusNameInfo, messageErrorText);
						}
						else
						{
							AssertNoMessageError(pair.Code, header.US_ScientificGenusNameInfo, messageErrorText);
						}
					}
				}
				else if (categoryTypeOfScientificDataRequired.Contains(allTypePair.Code))
				{
					header.US_ScientificGenusName = "A";
					AssertNoMessageError(allTypePair.Code, header.US_ScientificGenusNameInfo, messageErrorText);

					header.US_ScientificGenusName = ZString.Empty;
					AssertHasMessageError(allTypePair.Code, header.US_ScientificGenusNameInfo, messageErrorText);

					header.US_ProductIngredientType = IngredientTypeList.Codes.MixedIngredient;
					header.AddInfoValidation.ValidateUS_ScientificGenusName();
					AssertNoMessageError(allTypePair.Code, header.US_ScientificGenusNameInfo, messageErrorText);

					header.US_ScientificGenusName = "A";
					AssertNoMessageError(allTypePair.Code, header.US_ScientificGenusNameInfo, messageErrorText);
				}
				else
				{
					header.US_ScientificGenusName = ZString.Empty;
					AssertNoMessageErrorContaining(allTypePair.Code, header.US_ScientificGenusNameInfo, "Scientific Genus Name");

					header.US_CategoryType = categoryTypeOfScientificDataRequired[0];
					header.AddInfoValidation.ValidateUS_ScientificGenusName();
					AssertHasMessageErrorContaining(allTypePair.Code, header.US_ScientificGenusNameInfo, "Scientific Genus Name");
				}
			}
		}

		public void TestCheckUS_ScientificSpeciesName()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			var list = new APHISCategoryTypeCodeList();
			var scientificDataIsRequiredMessage = ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Scientific Species Name", "Scientific Genus Name");
			foreach (var code in categoryTypeOfScientificDataRequired)
			{
				list.RemoveCode(code);
				header.US_CategoryType = code;
				var messageError = ValidationConstants.APHIS.ScientificDataIsRequiredForCategory("Scientific Species Name", code);
				header.US_ScientificGenusName = "A";
				header.US_ScientificSpeciesName = "A";
				AssertNoMessageError(header.US_ScientificSpeciesNameInfo, messageError);
				AssertNoMessageError(header.US_ScientificSpeciesNameInfo, scientificDataIsRequiredMessage);
				header.US_ScientificSpeciesName = ZString.Empty;
				AssertHasMessageError(header.US_ScientificSpeciesNameInfo, messageError);
				AssertNoMessageError(header.US_ScientificSpeciesNameInfo, scientificDataIsRequiredMessage);
				Declaration.ValidationModes = ValidationModes.None;
				header.AddInfoValidation.ValidateUS_ScientificSpeciesName();
				AssertNoMessageError(header.US_ScientificSpeciesNameInfo, messageError);
				AssertNoMessageError(header.US_ScientificSpeciesNameInfo, scientificDataIsRequiredMessage);
				Declaration.RecalculateValidationModesOnDeclaration();
				header.AddInfoValidation.ValidateUS_ScientificSpeciesName();
				AssertHasMessageError(header.US_ScientificSpeciesNameInfo, messageError);
				AssertNoMessageError(header.US_ScientificSpeciesNameInfo, scientificDataIsRequiredMessage);
			}
			foreach (ICodeDescription pair in list)
			{
				header.US_CategoryType = pair.Code;
				header.US_ScientificGenusName = "A";
				header.US_ScientificSpeciesName = "A";
				AssertNoMessageError(header.US_ScientificSpeciesNameInfo, scientificDataIsRequiredMessage);
				header.US_ScientificSpeciesName = ZString.Empty;
				AssertHasMessageError(header.US_ScientificSpeciesNameInfo, scientificDataIsRequiredMessage);
				Declaration.ValidationModes = ValidationModes.None;
				header.AddInfoValidation.ValidateUS_ScientificSpeciesName();
				AssertNoMessageError(header.US_ScientificSpeciesNameInfo, scientificDataIsRequiredMessage);
				Declaration.RecalculateValidationModesOnDeclaration();
				header.AddInfoValidation.ValidateUS_ScientificSpeciesName();
				AssertHasMessageError(header.US_ScientificSpeciesNameInfo, scientificDataIsRequiredMessage);
				header.US_ScientificGenusName = "";
				AssertNoMessageError(header.US_ScientificSpeciesNameInfo, scientificDataIsRequiredMessage);
				header.US_ScientificGenusName = "A";
				AssertHasMessageError(header.US_ScientificSpeciesNameInfo, scientificDataIsRequiredMessage);
			}

			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts;
			var messageErrorText = ValidationConstants.APHIS.ScientificDataIsRequiredForCategory("Scientific Species Name", header.US_CategoryType);
			var categoryCodeList = new MiscellaneousAndProcessedProductsList();
			foreach (ICodeDescription pair in categoryCodeList)
			{
				header.US_CategoryCode = pair.Code;
				header.US_ScientificSpeciesName = ZString.Empty;
				header.AddInfoValidation.ValidateUS_ScientificSpeciesName();

				if (categoryCodeOfScientificDataRequiredCategoryCode.Contains(header.US_CategoryCode))
				{
					AssertHasMessageError(pair.Code, header.US_ScientificSpeciesNameInfo, messageErrorText);
				}
				else
				{
					AssertNoMessageError(pair.Code, header.US_ScientificSpeciesNameInfo, messageErrorText);
				}
			}
		}

		public void TestCheckUS_ScientificSpeciesName_IngredientTypeMIX()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			var allTypeList = new APHISCategoryTypeCodeList();
			var categoryCodeList = new MiscellaneousAndProcessedProductsList();
			foreach (ICodeDescription allTypePair in allTypeList)
			{
				header.US_CategoryType = allTypePair.Code;
				header.US_ProductIngredientType = ZString.Empty;
				var messageErrorText = ValidationConstants.APHIS.ScientificDataIsRequiredForCategory("Scientific Species Name", header.US_CategoryType);
				if (allTypePair.Code == APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts)
				{
					foreach (ICodeDescription pair in categoryCodeList)
					{
						header.US_CategoryCode = pair.Code;
						header.US_ScientificSpeciesName = ZString.Empty;
						header.AddInfoValidation.ValidateUS_ScientificGenusName();

						header.US_ProductIngredientType = ZString.Empty;
						if (categoryCodeOfScientificDataRequiredCategoryCode.Contains(header.US_CategoryCode))
						{
							AssertHasMessageError(pair.Code, header.US_ScientificSpeciesNameInfo, messageErrorText);

							header.US_ProductIngredientType = IngredientTypeList.Codes.MixedIngredient;
							header.AddInfoValidation.ValidateUS_ScientificGenusName();
							AssertNoMessageError(pair.Code, header.US_ScientificSpeciesNameInfo, messageErrorText);
						}
						else
						{
							AssertNoMessageError(pair.Code, header.US_ScientificSpeciesNameInfo, messageErrorText);
						}
					}
				}
				else if (categoryTypeOfScientificDataRequired.Contains(allTypePair.Code))
				{
					header.US_ScientificSpeciesName = "A";
					AssertNoMessageError(allTypePair.Code, header.US_ScientificSpeciesNameInfo, messageErrorText);

					header.US_ScientificSpeciesName = ZString.Empty;
					AssertHasMessageError(allTypePair.Code, header.US_ScientificSpeciesNameInfo, messageErrorText);

					header.US_ProductIngredientType = IngredientTypeList.Codes.MixedIngredient;
					header.AddInfoValidation.ValidateUS_ScientificGenusName();
					AssertNoMessageError(allTypePair.Code, header.US_ScientificSpeciesNameInfo, messageErrorText);

					header.US_ScientificSpeciesName = "A";
					AssertNoMessageError(allTypePair.Code, header.US_ScientificSpeciesNameInfo, messageErrorText);
				}
				else
				{
					header.US_ScientificSpeciesName = ZString.Empty;
					AssertNoMessageErrorContaining(allTypePair.Code, header.US_ScientificSpeciesNameInfo, "Scientific Species Name");

					header.US_CategoryType = categoryTypeOfScientificDataRequired[0];
					header.AddInfoValidation.ValidateUS_ScientificGenusName();
					AssertHasMessageErrorContaining(allTypePair.Code, header.US_ScientificSpeciesNameInfo, "Scientific Species Name");
				}
			}
		}

		readonly static string[] categoryTypeOfScientificDataRequired = new[]
		{
			APHISCategoryTypeCodeList.Codes.LiveAnimals,
			APHISCategoryTypeCodeList.Codes.PropagativeMaterial,
			APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting,
			APHISCategoryTypeCodeList.Codes.FruitsAndVegetables,
			APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery,
			APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms
		};
		readonly static List<ZString> categoryCodeOfScientificDataRequiredCategoryCode = new List<ZString>()
		{
			MiscellaneousAndProcessedProductsList.Codes.Cones,
			MiscellaneousAndProcessedProductsList.Codes.Grains,
			MiscellaneousAndProcessedProductsList.Codes.Grasses,
			MiscellaneousAndProcessedProductsList.Codes.HerbariumSpecimens,
			MiscellaneousAndProcessedProductsList.Codes.InsectsEarthwormsPathogensAndSnails,
			MiscellaneousAndProcessedProductsList.Codes.Nuts,
			MiscellaneousAndProcessedProductsList.Codes.SkinsGoatLambAndSheep,
			MiscellaneousAndProcessedProductsList.Codes.Lumber,
			MiscellaneousAndProcessedProductsList.Codes.Logs
		};

		public void TestCheckUS_ScientificSubSpeciesName()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ScientificSubSpeciesName = "A";
			AssertHasMessageError(header.US_ScientificSubSpeciesNameInfo, ValidationConstants.APHIS.ScientificVarietyNameShouldNotBeSpecifiedUnlessOtherScientificDataIs);
			header.US_ScientificGenusName = "A";
			AssertNoMessageError(header.US_ScientificSubSpeciesNameInfo, ValidationConstants.APHIS.ScientificVarietyNameShouldNotBeSpecifiedUnlessOtherScientificDataIs);
			header.US_ScientificGenusName = "";
			AssertHasMessageError(header.US_ScientificSubSpeciesNameInfo, ValidationConstants.APHIS.ScientificVarietyNameShouldNotBeSpecifiedUnlessOtherScientificDataIs);
			header.US_ScientificSpeciesName = "A";
			AssertNoMessageError(header.US_ScientificSubSpeciesNameInfo, ValidationConstants.APHIS.ScientificVarietyNameShouldNotBeSpecifiedUnlessOtherScientificDataIs);
			header.US_ScientificSpeciesName = "";
			AssertHasMessageError(header.US_ScientificSubSpeciesNameInfo, ValidationConstants.APHIS.ScientificVarietyNameShouldNotBeSpecifiedUnlessOtherScientificDataIs);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_ScientificSubSpeciesName();
			AssertNoMessageError(header.US_ScientificSubSpeciesNameInfo, ValidationConstants.APHIS.ScientificVarietyNameShouldNotBeSpecifiedUnlessOtherScientificDataIs);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_ScientificSubSpeciesName();
			AssertHasMessageError(header.US_ScientificSubSpeciesNameInfo, ValidationConstants.APHIS.ScientificVarietyNameShouldNotBeSpecifiedUnlessOtherScientificDataIs);
		}

		public void TestCheckUS_GrowingMedia()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			AssertNoMessageErrors(header.US_GrowingMediaInfo);

			header.US_GrowingMedia = "AAA";
			AssertHasMessageErrorContaining(header.US_GrowingMediaInfo, "");

			header.US_CategoryCode = PropagativeMaterialList.Codes.PlantsForPlantingOrPropagationWhole;
			header.US_ProductPhysicalState = PropagativeMaterialLifeStageA41List.Codes.WithRoots;
			header.US_GrowingMedia = PropagativeMaterialLifeStageA43List.Codes.ArtificialSoilless;
			AssertNoMessageErrors(header.US_GrowingMediaInfo);
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
		#endregion
	}
}
