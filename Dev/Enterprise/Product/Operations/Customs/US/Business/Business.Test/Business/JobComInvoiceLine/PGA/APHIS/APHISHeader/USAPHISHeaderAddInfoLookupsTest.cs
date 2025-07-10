using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.APHIS.ArticleCategory;
using Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ArticleCategory = Enterprise.Customs.US.Business.APHIS.ArticleCategory;
using CommodityCharacteristicQualifier = Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier;
using CustomsUniversal = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.US.Business.Testing
{
	class USAPHISHeaderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPrograms()
		{
			var list = Header.AddInfoLookups.Programs;
			AssertEquals("Programs", APHISProgramCodeList.GetActiveList(Factory), list);
		}
		public void TestIntendedUseCodes()
		{
			var startDate = ZDateTime.Today.AddMonths(-1);
			var endDate = ZDateTime.Today.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var codeType1 = helper.CreateNewOrGetExistingCusCodeType(CustomsUniversal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, CustomsUniversal.RefCusCodeListTypes.Codes.USPGAIntendUseCode);
			var codeType2 = helper.CreateNewOrGetExistingCusCodeType(CustomsUniversal.RefCusCodeListTypes.Codes.USPGALineStatus, CustomsUniversal.RefCusCodeListTypes.Codes.USPGALineStatus);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType1.ZZK_CodeType, "AAA", "AAA", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType2.ZZK_CodeType, "BBB", "BBB", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType1.ZZK_CodeType, "CCC", "CCC", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType1.ZZK_CodeType, "DDD", "DDD", startDate, endDate);
			var code5 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType1.ZZK_CodeType, "EEE", "EEE", startDate, endDate);

			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(CustomsUniversal.RefCusCodeList.Attributes.USPGAIntendUseCodeAgency, CustomsUniversal.RefCusCodeList.Attributes.USPGAIntendUseCodeAgency, CustomsUniversal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(CustomsUniversal.RefCusCodeList.Attributes.USPGAIntendUseCodeProgram, CustomsUniversal.RefCusCodeList.Attributes.USPGAIntendUseCodeProgram, CustomsUniversal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, Core.Constants.CountryCodes.UnitedStates);

			var attribute11 = helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, attributeName1.ZXE_Name, CustomsUniversal.RefCusCodeList.AttributeValues.APH);
			var attribute21 = helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeName1.ZXE_Name, CustomsUniversal.RefCusCodeList.AttributeValues.APH);
			var attribute41 = helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, attributeName1.ZXE_Name, CustomsUniversal.RefCusCodeList.AttributeValues.APH);
			var attribute42 = helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, attributeName2.ZXE_Name, CustomsUniversal.RefCusCodeList.AttributeValues.APH);
			var attribute52 = helper.CreateNewOrGetExistingCusCodeListAttribute(code5.PK, attributeName2.ZXE_Name, CustomsUniversal.RefCusCodeList.AttributeValues.APH);
			Factory.Save();

			var list1 = Header.AddInfoLookups.IntendedUseCodes;
			AssertEquals(true, list1.ContainsCode(code1.ZZD_Code));
			AssertEquals(false, list1.ContainsCode(code2.ZZD_Code));
			AssertEquals(false, list1.ContainsCode(code3.ZZD_Code));
			AssertEquals(false, list1.ContainsCode(code4.ZZD_Code));
			AssertEquals(false, list1.ContainsCode(code5.ZZD_Code));

			var list2 = Header.AddInfoLookups.IntendedUseCodes;
			AssertSame(list1, list2);
		}

		public void TestOrganisations()
		{
			var list = Header.AddInfoLookups.Organisations;
			AssertEquals("Organisations", typeof(OrganisationsFindBoxCollection), list.GetType());
		}

		public void TestProcessingCodes()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var list = Header.AddInfoLookups.ProcessingCodes;
			AssertEquals("ProcessingCodes", APHISGovernmentAgencyProcessingCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.AVS), list);
		}

		public void TestCategoryTypes()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var list = Header.AddInfoLookups.CategoryTypes;
			AssertEquals("CategoryTypes", APHISCategoryTypeCodeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.AVS), list);
		}

		public void TestCategoryCodes()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			Header.US_CategoryType = ZString.Empty;
			var list = Header.AddInfoLookups.CategoryCodes;
			AssertEquals("CategoryCodes Empty", 0, list.Count);

			Header.US_CategoryType = "!";
			list = Header.AddInfoLookups.CategoryCodes;
			AssertEquals("CategoryCodes Invalid", 0, list.Count);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			list = Header.AddInfoLookups.CategoryCodes;
			AssertEquals("CategoryCodes LiveAnimals", Factory.GetCachedValue<LiveAnimalsList>(), list);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts;
			list = Header.AddInfoLookups.CategoryCodes;
			AssertEquals("CategoryCodes RelatedAnimalProducts", Factory.GetCachedValue<RelatedAnimalProductsList>(), list);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			list = Header.AddInfoLookups.CategoryCodes;
			AssertEquals("CategoryCodes AnimalProductsAndAnimalByProducts", Factory.GetCachedValue<AnimalProductsAndByProductsList>(), list);
			Assert(list.ContainsCode("306A"));
			Assert(list.ContainsCode("306B"));
			Assert(list.ContainsCode("309A"));
			Assert(list.ContainsCode("309B"));

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.FruitsAndVegetables;
			list = Header.AddInfoLookups.CategoryCodes;
			var loadListFromCachedValue = Factory.GetCachedValue<ArticleCategory.FruitsAndVegetablesList>();
			AssertEquals("CategoryCodes FruitsAndVegetables", loadListFromCachedValue, list);
			AssertEquals("601, 602, 603", loadListFromCachedValue.CodesAsString);

			AssertEquals("Description FruitsAndVegetable 601", "Above Ground Parts", list.GetDescriptionFromCode("601"));
			AssertEquals("Description FruitsAndVegetable 602", "All Plant Parts", list.GetDescriptionFromCode("602"));
			AssertEquals("Description FruitsAndVegetable 603", "Below Ground Parts", list.GetDescriptionFromCode("603"));
		}

		public void TestProductConditions()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			Header.US_CategoryType = ZString.Empty;
			var list = Header.AddInfoLookups.ProductConditions;
			AssertEquals("ProductConditions Empty", 0, list.Count);

			Header.US_CategoryType = "!";
			list = Header.AddInfoLookups.ProductConditions;
			AssertEquals("ProductConditions Invalid", 0, list.Count);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			list = Header.AddInfoLookups.ProductConditions;
			AssertEquals("ProductConditions AnimalProductsAndAnimalByProducts", Factory.GetCachedValue<AnimalProductsAndByProductsConditionA30List>(), list);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			list = Header.AddInfoLookups.ProductConditions;
			AssertEquals("ProductConditions CutFlowersAndGreenery", Factory.GetCachedValue<CutFlowersAndGreeneryTypeList>(), list);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms;
			list = Header.AddInfoLookups.ProductConditions;
			AssertEquals("ProductConditions GeneticallyEngineeredOrganisms", Factory.GetCachedValue<GeneticallyEngineeredOrganismsTypeA101List>(), list);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts;
			list = Header.AddInfoLookups.ProductConditions;
			AssertEquals("ProductConditions RelatedAnimalProducts", Factory.GetCachedValue<RelatedAnimalProductsConditionA20List>(), list);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			Header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.PlantsForPlantingOrPropagationWhole;
			list = Header.AddInfoLookups.ProductConditions;
			AssertEquals(0, list.Count);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			Header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.SeedsForPlantingForSowing;
			list = Header.AddInfoLookups.ProductConditions;
			AssertEquals(0, list.Count);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			Header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.BulbsAndUndergroundPortionsOfDormantPerennials;
			list = Header.AddInfoLookups.ProductConditions;
			AssertEquals(0, list.Count);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts;
			list = Header.AddInfoLookups.ProductConditions;
			AssertEquals("ProductConditions MiscellaneousAndProcessedProducts", Factory.GetCachedValue<MiscellaneousAndProcessedProductsConditionList>(), list);
		}

		public void TestProductPhysicalStates()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			Header.US_CategoryType = ZString.Empty;
			var list = Header.AddInfoLookups.ProductPhysicalStates;
			AssertEquals("ProductPhysicalStates Empty", 0, list.Count);

			Header.US_CategoryType = "!";
			list = Header.AddInfoLookups.ProductPhysicalStates;
			AssertEquals("ProductPhysicalStates Invalid", 0, list.Count);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			list = Header.AddInfoLookups.ProductPhysicalStates;
			AssertEquals("ProductPhysicalStates AnimalProductsAndAnimalByProducts", Factory.GetCachedValue<AnimalProductsAndByProductsConditionA31List>(), list);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			list = Header.AddInfoLookups.ProductPhysicalStates;
			AssertEquals("ProductPhysicalStates CutFlowersAndGreenery", Factory.GetCachedValue<CutFlowersAndGreeneryPhysicalStateList>(), list);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.FruitsAndVegetables;
			list = Header.AddInfoLookups.ProductPhysicalStates;
			var loadListFromCachedValue = Factory.GetCachedValue<CommodityCharacteristicQualifier.FruitsAndVegetablesList>();
			AssertEquals("ProductPhysicalStates FruitsAndVegetables", loadListFromCachedValue, list);
			AssertEquals("FRC, FRF, SHR", loadListFromCachedValue.CodesAsString);

			AssertEquals("Description FruitsAndVegetable FRC", "Fresh Chilled", list.GetDescriptionFromCode("FRC"));
			AssertEquals("Description FruitsAndVegetable FRF", "Fresh Frozen", list.GetDescriptionFromCode("FRF"));
			AssertEquals("Description FruitsAndVegetable SHR", "Shredded", list.GetDescriptionFromCode("SHR"));

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms;
			list = Header.AddInfoLookups.ProductPhysicalStates;
			AssertEquals("ProductPhysicalStates GeneticallyEngineeredOrganisms", Factory.GetCachedValue<GeneticallyEngineeredOrganismsLifeStageA102List>(), list);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts;
			list = Header.AddInfoLookups.ProductPhysicalStates;
			AssertEquals("ProductPhysicalStates MiscellaneousAndProcessedProducts", Factory.GetCachedValue<MiscellaneousAndProcessedProductsPhysicalStateList>(), list);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			list = Header.AddInfoLookups.ProductPhysicalStates;
			AssertEquals("ProductPhysicalStates PropagativeMaterial", CommodityCharacteristicQualifier.PropagativeMaterialLifeStageA41List.GetPropagativeMaterialLifeStageA41List(Factory, Header.US_CategoryCode), list);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts;
			list = Header.AddInfoLookups.ProductPhysicalStates;
			AssertEquals("ProductPhysicalStates RelatedAnimalProducts", Factory.GetCachedValue<RelatedAnimalProductsConditionA21List>(), list);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting;
			list = Header.AddInfoLookups.ProductPhysicalStates;
			AssertEquals("ProductPhysicalStates SeedsNotForPlanting", Factory.GetCachedValue<CommodityCharacteristicQualifier.SeedsNotForPlantingList>(), list);
		}

		public void TestProductIngredientTypes()
		{
			var categoryTypes = new APHISCategoryTypeCodeList().GetAllCodes().ToList();
			categoryTypes.AddRange(new[] { "", "!" });

			foreach (var code in categoryTypes)
			{
				Header.US_CategoryType = code;
				var list = Header.AddInfoLookups.ProductIngredientTypes;
				if (Header.US_CategoryType == APHISCategoryTypeCodeList.Codes.FruitsAndVegetables)
				{
					AssertEquals("ProductIngredientTypes", Factory.GetCachedValue<IngredientTypeList>(), list);
					AssertEquals("ProductIngredientTypes Count", 2, list.Count);
				}
				else
				{
					AssertEquals("ProductIngredientTypes Invalid", 0, list.Count);
				}
			}
		}

		public void TestProductComponents()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			Header.US_CategoryType = ZString.Empty;
			var list = Header.AddInfoLookups.ProductComponents;
			AssertEquals("ProductComponents Empty", 0, list.Count);

			Header.US_CategoryType = "!";
			list = Header.AddInfoLookups.ProductComponents;
			AssertEquals("ProductComponents Invalid", 0, list.Count);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			list = Header.AddInfoLookups.ProductComponents;
			AssertEquals("ProductComponents AnimalProductsAndAnimalByProducts", Factory.GetCachedValue<AnimalProductsAndByProductsConditionA32List>(), list);

			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms;
			list = Header.AddInfoLookups.ProductComponents;
			AssertEquals("ProductComponents GeneticallyEngineeredOrganisms", Factory.GetCachedValue<GeneticallyEngineeredOrganismsIntergenericA100List>(), list);
		}

		public void TestProductStatusList()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var list = Header.AddInfoLookups.ProductStatusList;
			AssertNotNull(list);
		}

		public void TestProductGrowingMediaList()
		{
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			Header.US_CategoryCode = PropagativeMaterialList.Codes.BulbsAndUndergroundPortionsOfDormantPerennials;
			var list = Header.AddInfoLookups.GrowingMediaList;
			AssertEquals(0, list.Count);

			Header.US_CategoryCode = PropagativeMaterialList.Codes.PlantsForPlantingOrPropagationWhole;
			Header.US_ProductPhysicalState = PropagativeMaterialLifeStageA41List.Codes.WithRoots;
			list = Header.AddInfoLookups.GrowingMediaList;
			AssertEquals(3, list.Count);
			Assert(list.ContainsCode(PropagativeMaterialLifeStageA43List.Codes.ArtificialSoilless));
			Assert(list.ContainsCode(PropagativeMaterialLifeStageA43List.Codes.BareRootNoMedia));
			Assert(list.ContainsCode(PropagativeMaterialLifeStageA43List.Codes.Soil));

			Header.US_ProductPhysicalState = PropagativeMaterialLifeStageA41List.Codes.WithoutRoots;
			list = Header.AddInfoLookups.GrowingMediaList;
			AssertEquals(0, list.Count);

			Header.US_CategoryCode = PropagativeMaterialList.Codes.SeedsForPlantingForSowing;
			list = Header.AddInfoLookups.GrowingMediaList;
			AssertEquals(0, list.Count);

			Header.US_CategoryCode = PropagativeMaterialList.Codes.PlantCuttingsForPlantingOrPropagation;
			Header.US_ProductPhysicalState = PropagativeMaterialLifeStageA41List.Codes.WithRoots;
			list = Header.AddInfoLookups.GrowingMediaList;
			AssertEquals(3, list.Count);
			Assert(list.ContainsCode(PropagativeMaterialLifeStageA43List.Codes.ArtificialSoilless));
			Assert(list.ContainsCode(PropagativeMaterialLifeStageA43List.Codes.BareRootNoMedia));
			Assert(list.ContainsCode(PropagativeMaterialLifeStageA43List.Codes.Soil));

			Header.US_ProductPhysicalState = PropagativeMaterialLifeStageA41List.Codes.WithoutRoots;
			list = Header.AddInfoLookups.GrowingMediaList;
			AssertEquals(2, list.Count);
			Assert(list.ContainsCode(PropagativeMaterialLifeStageA43List.Codes.ArtificialSoilless));
			Assert(list.ContainsCode(PropagativeMaterialLifeStageA43List.Codes.BareRootNoMedia));

			Header.US_CategoryCode = PropagativeMaterialList.Codes.RootCuttingsOrRootCrownForPlantingOrPropagation;
			list = Header.AddInfoLookups.GrowingMediaList;
			AssertEquals(0, list.Count);

			Header.US_CategoryCode = PropagativeMaterialList.Codes.MeristemTissue;
			list = Header.AddInfoLookups.GrowingMediaList;
			AssertEquals(0, list.Count);

			Header.US_CategoryCode = PropagativeMaterialList.Codes.BudwoodGraftwood;
			Header.US_ProductPhysicalState = PropagativeMaterialLifeStageA41List.Codes.WithRoots;
			list = Header.AddInfoLookups.GrowingMediaList;
			AssertEquals(3, list.Count);
			Assert(list.ContainsCode(PropagativeMaterialLifeStageA43List.Codes.ArtificialSoilless));
			Assert(list.ContainsCode(PropagativeMaterialLifeStageA43List.Codes.BareRootNoMedia));
			Assert(list.ContainsCode(PropagativeMaterialLifeStageA43List.Codes.Soil));

			Header.US_ProductPhysicalState = PropagativeMaterialLifeStageA41List.Codes.WithoutRoots;
			list = Header.AddInfoLookups.GrowingMediaList;
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode(PropagativeMaterialLifeStageA43List.Codes.BareRootNoMedia));
		}

		public void TestProductTypes()
		{
			var list = new APHISProgramCodeList();
			list.RemoveCode(APHISProgramCodeList.Codes.APQ);
			Header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			AssertEquals("ProductTypes", ProductCodeQualifiersList.GetAPHISProductCodeQualifiers(Factory), Header.AddInfoLookups.ProductTypes);
			foreach (var programType in list.ToArray().Select(x => x.Code).Concat(new[] { "", "@" }))
			{
				Header.US_ProgramType = programType;
				AssertEquals("ProductTypes", ProductCodeQualifiersList.GetAPHISProductCodeQualifiers(Factory), Header.AddInfoLookups.ProductTypes);
			}
		}

		public void TestUnitOfMeasureList()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
				var list = Header.AddInfoLookups.UnitOfMeasureList;
				AssertEquals(4, list.Count);

				Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.FruitsAndVegetables;
				list = Header.AddInfoLookups.UnitOfMeasureList;
				AssertEquals(36, list.Count);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				Factory.ClearCachedValue<CodeDescriptionPairList>("APHISUnitOfMeasureList" + APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery);
				Factory.ClearCachedValue<CodeDescriptionPairList>("APHISUnitOfMeasureList" + APHISCategoryTypeCodeList.Codes.FruitsAndVegetables);

				Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
				var list = Header.AddInfoLookups.UnitOfMeasureList;
				AssertEquals(6, list.Count);

				Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.FruitsAndVegetables;
				list = Header.AddInfoLookups.UnitOfMeasureList;
				AssertEquals(38, list.Count);
			}
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
			get { return header ?? (header = InvoiceLine.APHISHeaders.AddNew()); }
		}
		APHISHeader header;
		#endregion
	}
}
