using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using APHISArticleCategory = Enterprise.Customs.US.Business.APHIS.ArticleCategory;
using APHISCommodityCharacteristicQualifier = Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier;
using ArticleCategory = Enterprise.Customs.US.Business.APHIS.ArticleCategory;
using CommodityCharacteristicQualifier = Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier;
using CommodityQualifier = Enterprise.Customs.US.Business.APHIS.CommodityQualifier;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISHeader))]
	public class APHISHeaderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<APHISHeader>
	{
		public void TestSetDefaultValues()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				var header = InvoiceLine.APHISHeaders.AddNew();
				Assert("Quantity 1 unit of measure should be empty", header.US_UQ1.IsEmpty);
				Assert("Quantity 2 unit of measure should be empty", header.US_UQ2.IsEmpty);
				Assert("Quantity 3 unit of measure should be empty", header.US_UQ3.IsEmpty);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var header = InvoiceLine.APHISHeaders.AddNew();
				AssertEquals("Quantity 1 unit of measure should be defaulted to KG", APHISUnitOfMeasureList.Codes.KilogramsWeight, header.US_UQ1);
				Assert("Quantity 2 unit of measure should be empty", header.US_UQ2.IsEmpty);
				Assert("Quantity 3 unit of measure should be empty", header.US_UQ3.IsEmpty);
			}
		}

		public void TestGetCusAddInfoTypes()
		{
			var aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			var supporter = aphisHeader as ICusAddInfoTypeSupporter;

			Type type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USAPHISProduct, out type);
			AssertEquals(typeof(APHISProduct), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USAPHISInspection, out type);
			AssertEquals(typeof(APHISInspection), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USAPHISLicense, out type);
			AssertEquals(typeof(APHISLicense), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USAPHISRouting, out type);
			AssertEquals(typeof(APHISRouting), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USAPHISSource, out type);
			AssertEquals(typeof(APHISSource), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue("ZZ!", out type);
			AssertNull(type);
		}

		public void TestIsSendProductWithOutCharacteristicCategoryCode()
		{
			var aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			aphisHeader.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			aphisHeader.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForResearchAndEvaluation;
			Assert(aphisHeader.IsSendProductWithOutCharacteristicCategoryCode);
			Assert(!aphisHeader.Products.AllowNew);

			aphisHeader.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.OrganismsAndVectors;
			Assert(aphisHeader.IsSendProductWithOutCharacteristicCategoryCode);
			Assert(!aphisHeader.Products.AllowNew);

			aphisHeader.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.LaboratoryMammals;
			Assert(aphisHeader.IsSendProductWithOutCharacteristicCategoryCode);
			Assert(!aphisHeader.Products.AllowNew);

			aphisHeader.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.Insects;
			Assert(aphisHeader.IsSendProductWithOutCharacteristicCategoryCode);
			Assert(!aphisHeader.Products.AllowNew);

			aphisHeader.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.BirdsNest;
			aphisHeader.Products.AddNew();
			Assert(aphisHeader.Products.Count > 0);

			aphisHeader.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.Insects;
			Assert(aphisHeader.Products.Count == 0);

			var iAPHISProductCharacteristic = aphisHeader as IAPHISProductCharacteristic;
			var characteristics = iAPHISProductCharacteristic.Characteristics.Cast<APHISCharacteristic>();
			AssertEquals(1, characteristics.Count());
			Assert(characteristics.Any(x => x.CommodityCharacteristicDescription == ZString.Empty && x.CommodityCharacteristicQualifier == ZString.Empty && x.CommodityQualifierCode == ZString.Empty));
		}

		public void TestPGALineReadOnly()
		{
			var aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			aphisHeader.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			Factory.Save();
			aphisHeader.OnLoaded();
			Assert(!aphisHeader.ReadOnly);

			aphisHeader.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			Factory.Save();
			aphisHeader.OnLoaded();
			Assert(aphisHeader.ReadOnly);

			aphisHeader.US_TrackingStatus = ZString.Empty;
			Factory.Save();
			aphisHeader.OnLoaded();
			Assert(!aphisHeader.ReadOnly);

			aphisHeader.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			Factory.Save();
			aphisHeader.OnLoaded();
			Assert(aphisHeader.ReadOnly);

			aphisHeader.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			Factory.Save();
			aphisHeader.OnLoaded();
			Assert(aphisHeader.ReadOnly);
		}

		public void TestCategoryTypeChangeToLiveAnimalsCategory()
		{
			var aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			AssertEquals(0, aphisHeader.Routings.Count);

			aphisHeader.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;

			AssertEquals(1, aphisHeader.Routings.Count);

			var routing = aphisHeader.Routings[0];
			AssertEquals(RoutingTypeList.Codes.OriginalLocation, routing.US_Type);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			ICusAddInfoTypeSupporter supporter = aphisHeader;
			supporter.AssertType(typeof(APHISProduct), CusAddInfoTypeAttribute.Codes.USAPHISProduct);
			supporter.AssertType(typeof(APHISInspection), CusAddInfoTypeAttribute.Codes.USAPHISInspection);
			supporter.AssertType(typeof(APHISLicense), CusAddInfoTypeAttribute.Codes.USAPHISLicense);
			supporter.AssertType(typeof(APHISRouting), CusAddInfoTypeAttribute.Codes.USAPHISRouting);
			supporter.AssertType(typeof(APHISSource), CusAddInfoTypeAttribute.Codes.USAPHISSource);
			supporter.AssertType(null, "ZZ!");

			var product = aphisHeader.Products.AddNew();
			product.US_Age = "Z";
			var inspection = aphisHeader.Inspections.AddNew();
			inspection.US_Location = "A";
			var license = aphisHeader.Licenses.AddNew();
			license.US_Type = APHISLicenseTypeList.Codes.AphisSeedAnalysisCertificate;
			var routing = aphisHeader.Routings.AddNew();
			routing.US_Country = "A";
			var source = aphisHeader.Sources.AddNew();
			source.US_CountryCode = "A";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var addInfo = newFactory.Load<CusAddInfo>(product.PK);
			AssertEquals(typeof(APHISProduct), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(inspection.PK);
			AssertEquals(typeof(APHISInspection), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(license.PK);
			AssertEquals(typeof(APHISLicense), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(routing.PK);
			AssertEquals(typeof(APHISRouting), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(source.PK);
			AssertEquals(typeof(APHISSource), addInfo.GetType());
		}

		public void TestPropertiesBehaviourBasedOnCategoryType()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			AssertPropertiesBehaviourBasedOnCategoryType(header, "$", new[]
			{
				APHISHeader.Schema.US_ProductCondition, APHISHeader.Schema.US_ProductPhysicalState, APHISHeader.Schema.US_ProductStatus,
				APHISHeader.Schema.ShipperOrgPK, APHISHeader.Schema.US_OA_ShipperAddress, APHISHeader.Schema.CropGrowerOrgPK,
				APHISHeader.Schema.US_OA_CropGrowerAddress, APHISHeader.Schema.US_ProductComponent, APHISHeader.Schema.USDAAPHISGrowerPK,
				APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress, APHISHeader.Schema.US_ProductIngredientType
			});
			AssertPropertiesBehaviourBasedOnCategoryType(header, "", new[]
			{
				APHISHeader.Schema.US_ProductCondition, APHISHeader.Schema.US_ProductPhysicalState, APHISHeader.Schema.US_ProductStatus,
				APHISHeader.Schema.ShipperOrgPK, APHISHeader.Schema.US_OA_ShipperAddress, APHISHeader.Schema.CropGrowerOrgPK,
				APHISHeader.Schema.US_OA_CropGrowerAddress, APHISHeader.Schema.US_ProductComponent, APHISHeader.Schema.USDAAPHISGrowerPK,
				APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress, APHISHeader.Schema.US_ProductIngredientType
			});
			var list = new APHISCategoryTypeCodeList();

			list.RemoveCode(APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts);
			AssertPropertiesBehaviourBasedOnCategoryType(header, APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts, new[]
			{
				APHISHeader.Schema.US_ProductStatus, APHISHeader.Schema.CropGrowerOrgPK, APHISHeader.Schema.US_OA_CropGrowerAddress,
				APHISHeader.Schema.US_ProductComponent, APHISHeader.Schema.US_ProductCondition, APHISHeader.Schema.US_ProductPhysicalState,
				APHISHeader.Schema.USDAAPHISGrowerPK, APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress, APHISHeader.Schema.US_ProductIngredientType
			});
			header.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.EggsAndEggProducts;
			AssertPropertiesBehaviourBasedOnCategoryType(header, APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts, new[]
			{
				APHISHeader.Schema.US_ProductStatus, APHISHeader.Schema.CropGrowerOrgPK, APHISHeader.Schema.US_OA_CropGrowerAddress,
				APHISHeader.Schema.USDAAPHISGrowerPK, APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress, APHISHeader.Schema.US_ProductIngredientType
			});

			list.RemoveCode(APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery);
			AssertPropertiesBehaviourBasedOnCategoryType(header, APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery, new[]
			{
				APHISHeader.Schema.US_ProductComponent, APHISHeader.Schema.ShipperOrgPK, APHISHeader.Schema.US_OA_ShipperAddress,
				APHISHeader.Schema.USDAAPHISGrowerPK, APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress, APHISHeader.Schema.US_ProductIngredientType
			});
			list.RemoveCode(APHISCategoryTypeCodeList.Codes.FruitsAndVegetables);
			AssertPropertiesBehaviourBasedOnCategoryType(header, APHISCategoryTypeCodeList.Codes.FruitsAndVegetables, new[]
			{
				APHISHeader.Schema.US_ProductComponent, APHISHeader.Schema.US_ProductStatus, APHISHeader.Schema.US_ProductCondition,
				APHISHeader.Schema.CropGrowerOrgPK, APHISHeader.Schema.US_OA_CropGrowerAddress,
				APHISHeader.Schema.USDAAPHISGrowerPK, APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress
			});
			list.RemoveCode(APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms);
			AssertPropertiesBehaviourBasedOnCategoryType(header, APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms, new[]
			{
				APHISHeader.Schema.US_ProductStatus, APHISHeader.Schema.CropGrowerOrgPK, APHISHeader.Schema.US_OA_CropGrowerAddress,
				APHISHeader.Schema.USDAAPHISGrowerPK, APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress, APHISHeader.Schema.US_ProductIngredientType
			});
			list.RemoveCode(APHISCategoryTypeCodeList.Codes.LiveAnimals);
			AssertPropertiesBehaviourBasedOnCategoryType(header, APHISCategoryTypeCodeList.Codes.LiveAnimals, new[]
			{
				APHISHeader.Schema.US_ProductComponent, APHISHeader.Schema.US_ProductStatus, APHISHeader.Schema.US_ProductCondition,
				APHISHeader.Schema.US_ProductPhysicalState, APHISHeader.Schema.CropGrowerOrgPK, APHISHeader.Schema.US_OA_CropGrowerAddress,
				APHISHeader.Schema.USDAAPHISGrowerPK, APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress, APHISHeader.Schema.US_ProductIngredientType
			});
			list.RemoveCode(APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts);
			AssertPropertiesBehaviourBasedOnCategoryType(header, APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts, new[]
			{
				APHISHeader.Schema.US_ProductComponent, APHISHeader.Schema.US_ProductStatus, APHISHeader.Schema.CropGrowerOrgPK, APHISHeader.Schema.US_OA_CropGrowerAddress,
				APHISHeader.Schema.USDAAPHISGrowerPK, APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress, APHISHeader.Schema.US_ProductIngredientType
			});
			list.RemoveCode(APHISCategoryTypeCodeList.Codes.PropagativeMaterial);
			AssertPropertiesBehaviourBasedOnCategoryType(header, APHISCategoryTypeCodeList.Codes.PropagativeMaterial, new[]
			{
				APHISHeader.Schema.CropGrowerOrgPK, APHISHeader.Schema.US_OA_CropGrowerAddress, APHISHeader.Schema.US_ProductComponent,
				APHISHeader.Schema.US_ProductCondition, APHISHeader.Schema.US_ProductIngredientType
			});

			list.RemoveCode(APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts);
			AssertPropertiesBehaviourBasedOnCategoryType(header, APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts, new[]
			{
				APHISHeader.Schema.ShipperOrgPK, APHISHeader.Schema.US_OA_ShipperAddress, APHISHeader.Schema.CropGrowerOrgPK,
				APHISHeader.Schema.US_OA_CropGrowerAddress, APHISHeader.Schema.US_ProductComponent, APHISHeader.Schema.US_ProductPhysicalState,
				APHISHeader.Schema.US_ProductCondition, APHISHeader.Schema.US_ProductStatus,
				APHISHeader.Schema.USDAAPHISGrowerPK, APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress, APHISHeader.Schema.US_ProductIngredientType
			});
			header.US_CategoryCode = ArticleCategory.RelatedAnimalProductsList.Codes.UsedFarmMachinery;
			AssertPropertiesBehaviourBasedOnCategoryType(header, APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts, new[]
			{
				APHISHeader.Schema.ShipperOrgPK, APHISHeader.Schema.US_OA_ShipperAddress, APHISHeader.Schema.CropGrowerOrgPK,
				APHISHeader.Schema.US_OA_CropGrowerAddress, APHISHeader.Schema.US_ProductComponent, APHISHeader.Schema.US_ProductPhysicalState, APHISHeader.Schema.US_ProductStatus,
				APHISHeader.Schema.USDAAPHISGrowerPK, APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress, APHISHeader.Schema.US_ProductIngredientType
			});
			header.US_CategoryCode = ArticleCategory.RelatedAnimalProductsList.Codes.StrawHayAndGrassAndCanadianOriginSoil;
			AssertPropertiesBehaviourBasedOnCategoryType(header, APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts, new[]
			{
				APHISHeader.Schema.ShipperOrgPK, APHISHeader.Schema.US_OA_ShipperAddress, APHISHeader.Schema.CropGrowerOrgPK,
				APHISHeader.Schema.US_OA_CropGrowerAddress, APHISHeader.Schema.US_ProductComponent, APHISHeader.Schema.US_ProductCondition, APHISHeader.Schema.US_ProductStatus,
				APHISHeader.Schema.USDAAPHISGrowerPK, APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress, APHISHeader.Schema.US_ProductIngredientType
			});

			list.RemoveCode(APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting);
			AssertPropertiesBehaviourBasedOnCategoryType(header, APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting, new[]
			{
				APHISHeader.Schema.US_ProductCondition, APHISHeader.Schema.US_ProductStatus, APHISHeader.Schema.CropGrowerOrgPK,
				APHISHeader.Schema.US_OA_CropGrowerAddress, APHISHeader.Schema.US_ProductComponent,
				APHISHeader.Schema.USDAAPHISGrowerPK, APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress, APHISHeader.Schema.US_ProductIngredientType
			});
			foreach (ICodeDescription pair in list)
			{
				AssertPropertiesBehaviourBasedOnCategoryType(header, pair.Code, new[]
				{
					APHISHeader.Schema.US_ProductCondition, APHISHeader.Schema.US_ProductPhysicalState, APHISHeader.Schema.US_ProductStatus,
					APHISHeader.Schema.ShipperOrgPK, APHISHeader.Schema.US_OA_ShipperAddress, APHISHeader.Schema.CropGrowerOrgPK,
					APHISHeader.Schema.US_OA_CropGrowerAddress, APHISHeader.Schema.US_ProductComponent,
					APHISHeader.Schema.USDAAPHISGrowerPK, APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress, APHISHeader.Schema.US_ProductIngredientType
				});
			}

			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts;
			header.US_CategoryCode = ArticleCategory.RelatedAnimalProductsList.Codes.UsedMeatCovers;
			Assert(!header.US_ProductConditionInfo.ReadOnly);
		}

		void AssertPropertiesBehaviourBasedOnCategoryType(APHISHeader header, ZString categoryType, string[] readOnlyList)
		{
			CombineAssertions(() =>
			{
				header.US_CategoryType = "!";
				header.US_ProductType = "A";
				header.US_ProductNumber = "A";
				header.US_ProductComponent = "A";
				header.US_ProductPhysicalState = "A";
				header.US_ProductCondition = "A";
				header.US_ProductStatus = "A";
				header.US_ProductIngredientType = "A";
				header.US_ScientificGenusName = "A";
				header.US_ScientificSpeciesName = "A";
				header.US_ScientificSubSpeciesName = "A";
				header.ApplicantOrgPK = ZGuid.Missing;
				header.US_OA_ApplicantAddress = ZGuid.Missing;
				header.CropGrowerOrgPK = ZGuid.Missing;
				header.US_OA_CropGrowerAddress = ZGuid.Missing;
				header.USDAAPHISGrowerPK = ZGuid.Missing;
				header.US_OA_USDAAPHISGrowerAddress = ZGuid.Missing;

				header.US_CategoryType = categoryType;
				foreach (var field in new[]
				{
					APHISHeader.Schema.US_ProductComponent, APHISHeader.Schema.US_ProductCondition,
					APHISHeader.Schema.US_ProductPhysicalState, APHISHeader.Schema.US_ProductStatus,
					APHISHeader.Schema.US_ScientificGenusName, APHISHeader.Schema.US_ScientificSpeciesName,
					APHISHeader.Schema.US_ScientificSubSpeciesName, APHISHeader.Schema.ApplicantOrgPK,
					APHISHeader.Schema.US_OA_ApplicantAddress, APHISHeader.Schema.CropGrowerOrgPK,
					APHISHeader.Schema.US_OA_CropGrowerAddress, APHISHeader.Schema.USDAAPHISGrowerPK,
					APHISHeader.Schema.US_OA_USDAAPHISGrowerAddress, APHISHeader.Schema.US_ProductIngredientType
				})
				{
					var info = header.ZPropertyInfoHash.GetPropertySafe(field);
					if (readOnlyList.Contains(field))
					{
						AssertEquals(field + " Value.IsDefault", true, info.Value.IsDefault);
						AssertEquals(field + " Is ReadOnly", true, info.ReadOnly);
					}
					else
					{
						AssertEquals(field + " Value.IsDefault", false, info.Value.IsDefault);
						AssertEquals(field + " Is ReadOnly", false, info.ReadOnly);
					}
				}
			});
		}

		public void TestIsPropagativeMaterialCategoryCode401Or403()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			Assert(!header.IsPropagativeMaterialCategoryCode401Or403);

			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.BulbsAndUndergroundPortionsOfDormantPerennials;
			Assert(header.IsPropagativeMaterialCategoryCode401Or403);

			header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.SeedsForPlantingForSowing;
			Assert(header.IsPropagativeMaterialCategoryCode401Or403);

			header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.PlantsForPlantingOrPropagationWhole;
			Assert(!header.IsPropagativeMaterialCategoryCode401Or403);

			header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.PlantCuttingsForPlantingOrPropagation;
			Assert(!header.IsPropagativeMaterialCategoryCode401Or403);

			header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.RootCuttingsOrRootCrownForPlantingOrPropagation;
			Assert(!header.IsPropagativeMaterialCategoryCode401Or403);

			header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.MeristemTissue;
			Assert(!header.IsPropagativeMaterialCategoryCode401Or403);

			header.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.BudwoodGraftwood;
			Assert(!header.IsPropagativeMaterialCategoryCode401Or403);
		}

		public void TestCharacteristicsForPropagativeMaterial()
		{
			var header1 = InvoiceLine.APHISHeaders.AddNew();
			header1.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			header1.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.SeedsForPlantingForSowing;
			header1.US_ProductPhysicalState = "";
			header1.US_ProductStatus = "";
			header1.US_GrowingMedia = "";

			var iAPHISProductCharacteristic1 = (IAPHISProductCharacteristic)header1;
			AssertEquals(0, iAPHISProductCharacteristic1.Characteristics.Count());

			header1.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.MeristemTissue;
			AssertEquals(1, iAPHISProductCharacteristic1.Characteristics.Count());
			Assert(iAPHISProductCharacteristic1.Characteristics.Any(x => x.CommodityCharacteristicQualifier == ZString.Empty));

			header1.US_ProductPhysicalState = CommodityCharacteristicQualifier.PropagativeMaterialLifeStageA41List.Codes.SeedLargeLot;
			header1.US_ProductStatus = CommodityQualifier.PropagativeMaterialList.Codes.EndangeredSpeciesStatus;

			AssertEquals(2, iAPHISProductCharacteristic1.Characteristics.Count());
			Assert(iAPHISProductCharacteristic1.Characteristics.Any(x => x.CommodityCharacteristicQualifier == CommodityCharacteristicQualifier.PropagativeMaterialLifeStageA41List.Codes.SeedLargeLot));
			Assert(iAPHISProductCharacteristic1.Characteristics.Any(x => x.CommodityCharacteristicQualifier == CommodityQualifier.PropagativeMaterialList.Codes.EndangeredSpeciesStatus));
			AssertEquals(false, iAPHISProductCharacteristic1.Characteristics.Any(x => x.CommodityCharacteristicQualifier == ZString.Empty));

			var header2 = InvoiceLine.APHISHeaders.AddNew();
			header2.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			header2.US_CategoryCode = ArticleCategory.PropagativeMaterialList.Codes.BulbsAndUndergroundPortionsOfDormantPerennials;
			header2.US_ProductPhysicalState = CommodityCharacteristicQualifier.PropagativeMaterialLifeStageA41List.Codes.SeedLargeLot;

			var iAPHISProductCharacteristic2 = (IAPHISProductCharacteristic)header2;
			AssertEquals(1, iAPHISProductCharacteristic2.Characteristics.Count());
			Assert(iAPHISProductCharacteristic2.Characteristics.Any(x => x.CommodityCharacteristicQualifier == CommodityCharacteristicQualifier.PropagativeMaterialLifeStageA41List.Codes.SeedLargeLot));
		}

		public void TestCharacteristicsForFruitsAndVegetables()
		{
			var header1 = InvoiceLine.APHISHeaders.AddNew();
			header1.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header1.US_CategoryType = APHISCategoryTypeCodeList.Codes.FruitsAndVegetables;
			header1.US_CategoryCode = ArticleCategory.FruitsAndVegetablesList.Codes.AboveGroundParts;
			header1.US_ProductPhysicalState = "";
			header1.US_ProductStatus = "";
			header1.US_ProductIngredientType = "";
			header1.US_GrowingMedia = "";

			var iAPHISProductCharacteristic1 = (IAPHISProductCharacteristic)header1;
			AssertEquals(1, iAPHISProductCharacteristic1.Characteristics.Count());
			Assert(iAPHISProductCharacteristic1.Characteristics.Any(x => x.CommodityCharacteristicQualifier == ZString.Empty));

			header1.US_ProductPhysicalState = CommodityCharacteristicQualifier.FruitsAndVegetablesList.Codes.FreshChilled;
			AssertEquals(1, iAPHISProductCharacteristic1.Characteristics.Count());
			Assert(iAPHISProductCharacteristic1.Characteristics.Any(x => x.CommodityCharacteristicQualifier == CommodityCharacteristicQualifier.FruitsAndVegetablesList.Codes.FreshChilled));

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				header1.US_ProductIngredientType = CommodityCharacteristicQualifier.IngredientTypeList.Codes.SingleIngredient;
				AssertEquals(2, iAPHISProductCharacteristic1.Characteristics.Count());
				Assert(iAPHISProductCharacteristic1.Characteristics.Any(x => x.CommodityCharacteristicQualifier == CommodityCharacteristicQualifier.IngredientTypeList.Codes.SingleIngredient));
			}
		}

		public void TestUltimateCosigneeRegistrationNumberWhenNoOrgCusCodeNumber()
		{
			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			var header1 = InvoiceLine.APHISHeaders.AddNew();
			header1.US_LineNo = 1;

			var orgUC = Factory.New<OrgHeader>();
			orgUC.OH_FullName = "ULTIMATE CONSIGNEE";
			orgUC.OH_RL_NKClosestPort = "USCHI";
			var ultimateConsignee = orgUC.MainAddress;
			ultimateConsignee.OA_Address1 = "UC ADDRESS 1";
			ultimateConsignee.OA_Address2 = "UC ADDRESS 2";
			ultimateConsignee.OA_RL_NKRelatedPortCode = "USCHI";
			ultimateConsignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "", Core.Constants.CountryCodes.UnitedStates);

			var org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "CONSIGNEE1";
			org1.OH_RL_NKClosestPort = "USPHL";
			var uc1 = org1.MainAddress;
			uc1.OA_Address1 = "UC1 ADD 1";
			uc1.OA_Address2 = "UC1 ADD 2";
			uc1.OA_RL_NKRelatedPortCode = "USCHI";
			uc1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "888999", Core.Constants.CountryCodes.UnitedStates);

			InvoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee.PK;

			IAPHISHeader iHeader = header1;
			var ultimateCosigneeDetails = iHeader.UltimateCosigneeDetails;
			AssertEquals("UltimateCosigneeDetails", ultimateConsignee.Address1, ultimateCosigneeDetails.AddressLine1);
			var ultimateCosigneeRegistrationNumber = iHeader.UltimateCosigneeRegistrationNumber;
			AssertEquals("UltimateCosigneeRegistrationNumber when empty", "EXEMPT", ultimateCosigneeRegistrationNumber.Number);
			AssertEquals("UltimateCosigneeRegistrationNumberType when empty", EntityIdentificationCodesList.Codes.CBPAssigned, ultimateCosigneeRegistrationNumber.NumberType);

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			var ultimateCosigneeRegistrationNumberX = iHeader.UltimateCosigneeRegistrationNumber;
			AssertEquals("UltimateCosigneeRegistrationNumber", "", ultimateCosigneeRegistrationNumberX.Number);
			AssertEquals("UltimateCosigneeRegistrationNumberType EIN", EntityIdentificationCodesList.Codes.IRSAssigned, ultimateCosigneeRegistrationNumberX.NumberType);

			InvoiceLine.JI_OA_ConsigneeAddress = uc1.PK;
			var header2 = InvoiceLine.APHISHeaders.AddNew();
			header2.US_LineNo = 2;

			IAPHISHeader iHeader2 = header2;
			var ucDetails2 = iHeader2.UltimateCosigneeDetails;
			AssertEquals("UltimateCosigneeDetails with Number", uc1.OA_Address1, ucDetails2.AddressLine1);
			var ucRegNumber = iHeader2.UltimateCosigneeRegistrationNumber;
			AssertEquals("UltimateCosigneeRegistrationNumber", "888999", ucRegNumber.Number);
			AssertEquals("UltimateCosigneeRegistrationNumberType EIN", EntityIdentificationCodesList.Codes.IRSAssigned, ucRegNumber.NumberType);

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertEquals("UltimateCosigneeRegistrationNumber", "888999", ucRegNumber.Number);
			AssertEquals("UltimateCosigneeRegistrationNumberType EIN", EntityIdentificationCodesList.Codes.IRSAssigned, ucRegNumber.NumberType);
		}

		public void TestIAPHISHeaderMembers()
		{
			InvoiceLine.JI_Tariff = "2402106000";
			InvoiceLine.JI_Description = "CATTLE";
			var header = InvoiceLine.APHISHeaders.AddNew();
			// PG01
			header.US_LineNo = 1;
			header.US_IsDocSubmitted = ZBool.True;
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.Inspections.RemoveAndDeleteAll();
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSPortVeterinarian;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ConsumerProductIntendedForAdolescentsAged6To8Years;
			header.US_IntendedUseDescription = "THIS IS VERY LONG DESC";
			// PG02
			header.US_ProductType = ProductCodeQualifiersList.Codes.TaxonomicSerialNumber;
			header.US_ProductNumber = "183838";
			// PG05
			header.US_ScientificGenusName = "BOS";
			header.US_ScientificSpeciesName = "TAURUS";
			header.US_ScientificSubSpeciesName = "SUB TR";
			// PG06
			var source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.PlaceOfGrowth;
			source = header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfManipulation;

			// PG10
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			header.US_CategoryCode = APHISArticleCategory.LiveAnimalsList.Codes.BosAndBisonDomesticCattleHumpedCattleAndBison;
			header.US_ProductStatus = "ST";
			var product = header.Products.AddNew();
			product.US_Age = APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._12Years;
			product = header.Products.AddNew();
			product.US_Age = APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._16Years;

			// PG07
			var identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.CHP;

			// PG13
			var license = header.Licenses.AddNew();
			license = header.Licenses.AddNew();

			// PG17
			header.US_CommoditySpecificName = "BROWN STEERS";

			// PG19, PG20 and PG21
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "CUSTOMS BROKER";
			org.OH_RL_NKClosestPort = "USLAX";
			var customsBroker = org.MainAddress;
			customsBroker.OA_Address1 = "CB ADDRESS 1";
			customsBroker.OA_Address2 = "CB ADDRESS 2";
			customsBroker.OA_RL_NKRelatedPortCode = "USLAX";
			customsBroker.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, "32-323422", Core.Constants.CountryCodes.UnitedStates);

			org = Factory.New<OrgHeader>();
			org.OH_FullName = "PERMIT HOLDER";
			org.OH_RL_NKClosestPort = "USNYC";
			var permitHolder = org.MainAddress;
			permitHolder.OA_Address1 = "PH ADDRESS 1";
			permitHolder.OA_Address2 = "PH ADDRESS 2";
			permitHolder.OA_RL_NKRelatedPortCode = "USNYC";
			permitHolder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.APHISAssignedNumber, "32KD443", Core.Constants.CountryCodes.UnitedStates);

			org = Factory.New<OrgHeader>();
			org.OH_FullName = "ULTIMATE CONSIGNEE";
			org.OH_RL_NKClosestPort = "USCHI";
			var ultimateConsignee = org.MainAddress;
			ultimateConsignee.OA_Address1 = "UC ADDRESS 1";
			ultimateConsignee.OA_Address2 = "UC ADDRESS 2";
			ultimateConsignee.OA_RL_NKRelatedPortCode = "USCHI";
			ultimateConsignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "32-23-234232", Core.Constants.CountryCodes.UnitedStates);

			org = Factory.New<OrgHeader>();
			org.OH_FullName = "CROP GROWER";
			org.OH_RL_NKClosestPort = "AUSYD";
			var cropGrower = org.MainAddress;
			cropGrower.OA_Address1 = "CG ADDRESS 1";
			cropGrower.OA_Address2 = "CG ADDRESS 2";
			cropGrower.OA_RL_NKRelatedPortCode = "AUSYD";

			org = Factory.New<OrgHeader>();
			org.OH_FullName = "USDA GROWER";
			org.OH_RL_NKClosestPort = "AUSYD";
			var usdaGrower = org.MainAddress;
			usdaGrower.OA_Address1 = "GR ADDRESS 1";
			usdaGrower.OA_Address2 = "GR ADDRESS 2";
			usdaGrower.OA_RL_NKRelatedPortCode = "AUSYD";
			permitHolder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.APHISAssignedNumber, "32KD443", Core.Constants.CountryCodes.UnitedStates);

			header.US_OA_ApplicantAddress = permitHolder.PK;
			header.US_OA_CropGrowerAddress = cropGrower.PK;
			header.US_OA_USDAAPHISGrowerAddress = usdaGrower.PK;
			InvoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee.PK;
			Declaration.Branch.GB_OH_OrgProxy = customsBroker.OA_OH;
			var invoice = InvoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "BOB SMITH";
			invoice.US_FDAContactPhoneNo = "6301023498";
			invoice.US_FDAContactEmail = "BOB@WHERE.COM";
			//PG26
			header.US_Qty1 = 1m;
			header.US_UQ1 = APHISUnitOfMeasureList.Codes.Bag;
			header.US_Qty2 = 1m;
			header.US_UQ2 = APHISUnitOfMeasureList.Codes.Box;
			header.US_Qty3 = 1m;
			header.US_UQ3 = APHISUnitOfMeasureList.Codes.NumberCount;

			// PG27
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40@#";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT32423";
			container.CO_RC = containerType.PK;
			containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "20@#";
			containerType.RC_Length = 20m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.DryStorage;
			container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT85454";
			var invoiceLineContainers = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			invoiceLineContainers[0].IsForInvoiceLine = true;
			invoiceLineContainers[1].IsForInvoiceLine = true;

			// PG30
			var inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.PreviouslyScheduled;
			inspection = header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			// PG32
			var routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing = header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.PlaceOfTransshipment;

			IAPHISHeader iHeader = header;
			// PG01
			AssertEquals("LineNo", 1, iHeader.LineNo);
			AssertEquals("ProgramType", APHISProgramCodeList.Codes.AVS, iHeader.ProgramType);
			AssertEquals("IsDocSubmitted", ZBool.True, iHeader.IsDocSubmitted);
			AssertEquals("ProcessingCode", APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSPortVeterinarian, iHeader.ProcessingCode);
			AssertEquals("IntendedUseCode", IntendedUseCodesList.Codes.ConsumerProductIntendedForAdolescentsAged6To8Years, iHeader.IntendedUseCode);
			AssertEquals("IntendedUseDescription", "THIS IS VERY LONG DES", iHeader.IntendedUseDescription);

			// PG02
			AssertEquals("ProductCodeQualifier", ProductCodeQualifiersList.Codes.TaxonomicSerialNumber, iHeader.ProductCodeQualifier);
			AssertEquals("ProductCodeNumber", "183838", iHeader.ProductCodeNumber);

			// PG05
			AssertEquals("ScientificGenusName", "BOS", iHeader.ScientificGenusName);
			AssertEquals("ScientificSpeciesName", "TAURUS", iHeader.ScientificSpeciesName);
			AssertEquals("ScientificSubSpeciesName", "SUB TR", iHeader.ScientificSubSpeciesName);

			// PG06
			var sources = iHeader.Sources.ToArray();
			AssertEquals("Sources", 2, sources.Length);
			AssertEquals("Sources[0].SourceTypeCode", SourceTypeCodesList.Codes.PlaceOfGrowth, sources[0].SourceTypeCode);
			AssertEquals("Sources[1].SourceTypeCode", SourceTypeCodesList.Codes.CountryOfManipulation, sources[1].SourceTypeCode);

			var productCharacteristics = iHeader.ProductCharacteristics.ToArray();
			AssertEquals("ProductCharacteristics", 2, productCharacteristics.Length);

			var productCharacteristic = productCharacteristics[0];
			// PG07 and PG08
			var identities = productCharacteristic.Identities.ToArray();
			AssertEquals("Identities", 0, identities.Length);

			var characteristics = productCharacteristic.Characteristics.ToArray();
			AssertEquals("Characteristics", 1, characteristics.Length);
			AssertEquals("Characteristics[0].CommodityCharacteristicQualifier", APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._12Years, characteristics[0].CommodityCharacteristicQualifier);

			productCharacteristic = productCharacteristics[1];
			identities = productCharacteristic.Identities.ToArray();
			AssertEquals("Identities", 2, identities.Length);
			AssertEquals("Identities[0].IdentityType", APHISItemIdentityNumberQualifierList.Codes.LAT, identities[0].IdentityType);
			AssertEquals("Identities[1].IdentityType", APHISItemIdentityNumberQualifierList.Codes.CHP, identities[1].IdentityType);

			// PG10
			AssertEquals("CategoryTypeCode", APHISCategoryTypeCodeList.Codes.LiveAnimals, iHeader.CategoryTypeCode);
			AssertEquals("CategoryCode", APHISArticleCategory.LiveAnimalsList.Codes.BosAndBisonDomesticCattleHumpedCattleAndBison, iHeader.CategoryCode);

			characteristics = productCharacteristic.Characteristics.ToArray();
			AssertEquals("Characteristics", 1, characteristics.Length);
			AssertEquals("Characteristics[0].CommodityCharacteristicQualifier", APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._16Years, characteristics[0].CommodityCharacteristicQualifier);

			var productComponents = iHeader.ProductComponents.ToArray();
			AssertEquals("ProductComponents", 0, productComponents.Length);

			// PG13 and PG14
			var licenses = iHeader.Licenses.ToArray();
			AssertEquals("Licenses", 2, licenses.Length);

			// PG17
			AssertEquals("CommodityGeneralName", "BROWN STEERS", iHeader.CommoditySpecificName);

			// PG19, PG20 and PG21
			var brokerDetails = iHeader.BrokerDetails;
			AssertEquals("BrokerDetails.Address", ((IPGAContactDetails)OrgHeaderWrapper.New(customsBroker)).CompanyAddress, brokerDetails.Address);
			var ultimateCosigneeDetails = iHeader.UltimateCosigneeDetails;
			AssertEquals("UltimateCosigneeDetails", ultimateConsignee.Address1, ultimateCosigneeDetails.AddressLine1);
			var ultimateCosigneeRegistrationNumber = iHeader.UltimateCosigneeRegistrationNumber;
			AssertEquals("UltimateCosigneeRegistrationNumber", "32-23-234232", ultimateCosigneeRegistrationNumber.Number);
			AssertEquals("UltimateCosigneeRegistrationNumberType", EntityIdentificationCodesList.Codes.IRSAssigned, ultimateCosigneeRegistrationNumber.NumberType);
			var applicantDetails = iHeader.ApplicantDetails;
			AssertEquals("ApplicantDetails", permitHolder.Address1, applicantDetails.AddressLine1);
			AssertEquals("ApplicantAPHISAssignedNumber", "32KD443", iHeader.ApplicantAPHISAssignedNumber.EntityNumber);
			AssertNull("CropGrowerDetails", iHeader.CropGrowerDetails);
			AssertNull("USDAAPHISGrower", iHeader.USDAAPHISGrower);

			// PG26
			var orderedQtyUQs = iHeader.OrderedQtyUQs.ToArray();
			AssertEquals("OrderedQtyUQs", 3, orderedQtyUQs.Length);
			AssertEquals("OrderedQtyUQs[0].UQ", APHISUnitOfMeasureList.Codes.NumberCount, orderedQtyUQs[0].UQ);
			AssertEquals("OrderedQtyUQs[1].UQ", APHISUnitOfMeasureList.Codes.Box, orderedQtyUQs[1].UQ);
			AssertEquals("OrderedQtyUQs[2].UQ", APHISUnitOfMeasureList.Codes.Bag, orderedQtyUQs[2].UQ);

			// PG27
			var containers = iHeader.Containers.ToArray();
			AssertEquals("Containers", 2, containers.Length);
			AssertEquals("Containers[0].ContainerEquipmentID", "CONT32423", containers[0].ContainerEquipmentID);
			AssertEquals("Containers[1].ContainerEquipmentID", "CONT85454", containers[1].ContainerEquipmentID);

			// PG30
			var inspections = iHeader.Inspections.ToArray();
			AssertEquals("Inspections", 2, inspections.Length);
			AssertEquals("Inspections[0].InspectionTestingStatus", InspectionStatusList.Codes.PreviouslyScheduled, inspections[0].InspectionTestingStatus);
			AssertEquals("Inspections[1].InspectionTestingStatus", InspectionStatusList.Codes.BTAAnticipatedArrivalInformation, inspections[1].InspectionTestingStatus);

			// PG32
			var routings = iHeader.Routings.ToArray();
			AssertEquals("Routings", 3, routings.Length);
			AssertEquals("Routings[0].RoutingType", RoutingTypeList.Codes.OriginalLocation, routings[0].RoutingType);
			AssertEquals("Routings[1].RoutingType", RoutingTypeList.Codes.OriginalLocation, routings[1].RoutingType);
			AssertEquals("Routings[2].RoutingType", RoutingTypeList.Codes.PlaceOfTransshipment, routings[2].RoutingType);
		}

		public void DataIsDeletedOnSaving()
		{
			var aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			var product = aphisHeader.Products.AddNew();
			product.US_Age = "A";
			AssertEquals(ZString.Empty, aphisHeader.US_ProgramType);
			Factory.Save();
			AssertEquals(false, aphisHeader.IsDeleted);
			product.Delete();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			Factory.Save();
			AssertEquals(false, aphisHeader.IsDeleted);
			aphisHeader.US_ProgramType = ZString.Empty;
			Factory.Save();
			AssertEquals(true, aphisHeader.IsDeleted);
			aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			Factory.Save();
			AssertEquals(false, aphisHeader.IsDeleted);
			aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			var inspection = aphisHeader.Inspections.AddNew();
			inspection.US_Location = "A";
			Factory.Save();
			AssertEquals(false, aphisHeader.IsDeleted);
			aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			var license = aphisHeader.Licenses.AddNew();
			Factory.Save();
			AssertEquals(false, aphisHeader.IsDeleted);
			aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			var routing = aphisHeader.Routings.AddNew();
			routing.US_Country = "A";
			Factory.Save();
			AssertEquals(false, aphisHeader.IsDeleted);
			aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			var source = aphisHeader.Sources.AddNew();
			source.US_CountryCode = "A";
			Factory.Save();
			AssertEquals(false, aphisHeader.IsDeleted);
		}

		public void TestClone()
		{
			var aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.ABS;
			aphisHeader.US_IsDocSubmitted = ZBool.True;
			aphisHeader.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.CBPAgriculture;
			aphisHeader.US_IntendedUseCode = IntendedUseCodesList.Codes.ConsumerProductIntendedForAdolescentsAged6To8Years;
			aphisHeader.US_CategoryCode = APHISArticleCategory.LiveAnimalsList.Codes.BosAndBisonDomesticCattleHumpedCattleAndBison;
			aphisHeader.US_OA_ApplicantAddress = ZGuid.Missing;
			aphisHeader.US_ProductType = ProductCodeQualifiersList.Codes.TaxonomicSerialNumber;
			aphisHeader.US_ProductNumber = "AS66";
			aphisHeader.US_VehicleNumber = "AAT668";

			aphisHeader.US_Qty1 = 5m;
			aphisHeader.US_UQ1 = APHISUnitOfMeasureList.Codes.Box;
			aphisHeader.US_Qty2 = 6m;
			aphisHeader.US_UQ2 = APHISUnitOfMeasureList.Codes.Bundle;
			aphisHeader.US_Qty3 = 7m;
			aphisHeader.US_UQ3 = APHISUnitOfMeasureList.Codes.Carton;

			aphisHeader.US_CommoditySpecificName = "COM1";
			aphisHeader.US_ScientificGenusName = "GN1";
			aphisHeader.US_ScientificSpeciesName = "SP2";
			aphisHeader.US_ScientificSubSpeciesName = "VR3";

			var source = aphisHeader.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
			source.US_CountryCode = Core.Constants.CountryCodes.Argentina;
			source.US_GeographicLocation = "COLON";
			source.US_ProcessingStartDate = new ZDateTime(2016, 5, 5);
			source.US_ProcessingEndDate = new ZDateTime(2016, 5, 8);
			source.US_ProcessingTypeCode = "ACA";

			var routing = aphisHeader.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.PlaceOfTransshipment;
			routing.US_Country = Core.Constants.CountryCodes.Argentina;
			routing.US_State = "TUC";

			InvoiceLine.Declaration.CopyLastPGADetailsToNewLine = true;
			Factory.Save();

			var aphisHeaderNew = InvoiceLine.APHISHeaders.AddNew();

			AssertEquals("Processing Code must be the same", aphisHeader.US_ProcessingCode, aphisHeaderNew.US_ProcessingCode);
			AssertEquals("Intended Use Code must be the same", aphisHeader.US_IntendedUseCode, aphisHeaderNew.US_IntendedUseCode);
			AssertEquals("Category Code must be the same", aphisHeader.US_CategoryCode, aphisHeaderNew.US_CategoryCode);
			AssertEquals("Applicant must be the same", aphisHeader.US_OA_ApplicantAddress, aphisHeaderNew.US_OA_ApplicantAddress);
			AssertEquals("Vehicle Number must be the same", aphisHeader.US_VehicleNumber, aphisHeaderNew.US_VehicleNumber);
			AssertEquals("IsDocSubmitted must be false", ZBool.False, aphisHeaderNew.US_IsDocSubmitted);

			AssertEquals("Quantity1 must be the same", aphisHeader.US_Qty1, aphisHeaderNew.US_Qty1);
			AssertEquals("Unit1 must be the same", aphisHeader.US_UQ1, aphisHeaderNew.US_UQ1);
			AssertEquals("Quantity2 must be the same", aphisHeader.US_Qty2, aphisHeaderNew.US_Qty2);
			AssertEquals("Unit2 must be the same", aphisHeader.US_UQ2, aphisHeaderNew.US_UQ2);
			AssertEquals("Quantity3 must be the same", aphisHeader.US_Qty3, aphisHeaderNew.US_Qty3);
			AssertEquals("Unit3 must be the same", aphisHeader.US_UQ3, aphisHeaderNew.US_UQ3);

			AssertEquals("CommoditySpecificName must be the same", aphisHeader.US_CommoditySpecificName, aphisHeaderNew.US_CommoditySpecificName);
			AssertEquals("ScientificGenusName must be the same", aphisHeader.US_ScientificGenusName, aphisHeaderNew.US_ScientificGenusName);
			AssertEquals("ScientificSpeciesName must be the same", aphisHeader.US_ScientificSpeciesName, aphisHeaderNew.US_ScientificSpeciesName);
			AssertEquals("ScientificSubSpeciesName must be the same", aphisHeader.US_ScientificSubSpeciesName, aphisHeaderNew.US_ScientificSubSpeciesName);

			AssertEquals("Sources: SourceTypeCode must be the same", aphisHeader.Sources[0].US_SourceTypeCode, aphisHeaderNew.Sources[0].US_SourceTypeCode);
			AssertEquals("Sources: CountryCode must be the same", aphisHeader.Sources[0].US_CountryCode, aphisHeaderNew.Sources[0].US_CountryCode);
			AssertEquals("Sources: GeographicLocation must be the same", aphisHeader.Sources[0].US_GeographicLocation, aphisHeaderNew.Sources[0].US_GeographicLocation);
			AssertEquals("Sources: ProcessingStartDate must be the same", aphisHeader.Sources[0].US_ProcessingStartDate, aphisHeaderNew.Sources[0].US_ProcessingStartDate);
			AssertEquals("Sources: ProcessingEndDate must be the same", aphisHeader.Sources[0].US_ProcessingEndDate, aphisHeaderNew.Sources[0].US_ProcessingEndDate);
			AssertEquals("Sources: ProcessingTypeCode must be the same", aphisHeader.Sources[0].US_ProcessingTypeCode, aphisHeaderNew.Sources[0].US_ProcessingTypeCode);

			AssertEquals("Routings: Type must be the same", aphisHeader.Routings[0].US_Type, aphisHeaderNew.Routings[0].US_Type);
			AssertEquals("Routings: Country must be the same", aphisHeader.Routings[0].US_Country, aphisHeaderNew.Routings[0].US_Country);
			AssertEquals("Routings: State must be the same", aphisHeader.Routings[0].US_State, aphisHeaderNew.Routings[0].US_State);

			InvoiceLine.Declaration.CopyLastPGADetailsToNewLine = false;
			aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var product1 = aphisHeader.Products.AddNew();
			product1.US_Age = "A";
			var product2 = aphisHeader.Products.AddNew();
			product2.US_Age = "Z";
			var identity1 = product2.Identities.AddNew();
			identity1.CY_Data = "A";
			var identity2 = product2.Identities.AddNew();
			identity2.CY_Data = "Z";
			var inspection1 = aphisHeader.Inspections.AddNew();
			inspection1.US_Location = "A";
			var inspection2 = aphisHeader.Inspections.AddNew();
			inspection2.US_Location = "Z";
			var license1 = aphisHeader.Licenses.AddNew();
			var license2 = aphisHeader.Licenses.AddNew();
			var routing1 = aphisHeader.Routings.AddNew();
			routing1.US_Country = "A";
			var routing2 = aphisHeader.Routings.AddNew();
			routing2.US_Country = "Z";
			var source1 = aphisHeader.Sources.AddNew();
			source1.US_CountryCode = "A";
			var source2 = aphisHeader.Sources.AddNew();
			source2.US_CountryCode = "Z";

			var clonedHeader = (APHISHeader)aphisHeader.Clone();
			AssertEquals("clonedHeader.US_ProgramType", APHISProgramCodeList.Codes.AVS, clonedHeader.US_ProgramType);

			AssertEquals("We do not clone Products, they are transactional data", 0, clonedHeader.Products.Count);
			AssertEquals("We do not clone Inspections, they are transactional details", 0, clonedHeader.Inspections.Count);
			AssertEquals("We do not clone Licenses, they are transactional", 0, clonedHeader.Licenses.Count);

			AssertEquals("clonedHeader.Routings.Count", 2, clonedHeader.Routings.Count);
			var clonedRouting = clonedHeader.Routings[0];
			AssertEquals("clonedRouting.US_Country", "A", clonedRouting.US_Country);
			clonedRouting = clonedHeader.Routings[1];
			AssertEquals("clonedRouting.US_Country", "Z", clonedRouting.US_Country);

			AssertEquals("clonedHeader.Sources.Count", 2, clonedHeader.Sources.Count);
			var clonedSource = clonedHeader.Sources[0];
			AssertEquals("clonedSource.US_CountryCode", "A", clonedSource.US_CountryCode);
			clonedSource = clonedHeader.Sources[1];
			AssertEquals("clonedSource.US_CountryCode", "Z", clonedSource.US_CountryCode);
		}

		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<APHISHeader>();
			originalBO.Routings.AddNew();
			originalBO.Sources.AddNew();

			var newBO = (APHISHeader)originalBO.Clone();

			AssertEquals(1, newBO.Routings.Count);
			AssertEquals(1, newBO.Sources.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (APHISHeader)originalBO.Clone(new BusinessObjectCloneArgs(fac, Array.Empty<string>(), typeof(APHISHeader), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Routings[0].Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Sources[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.Routings[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.Sources[0].Factory.GetHashCode());
		}

		public void TestUltimateCosigneeDetails()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESORG";
			orgHeader.MainAddress.OA_Address1 = "MAIN ADDRESS";
			var newAddress = orgHeader.Addresses.AddNew();
			newAddress.OA_Address1 = "IAN TEST ADDRESS";
			InvoiceLine.JI_OA_ConsigneeAddress = newAddress.PK;
			var aphisheader = InvoiceLine.APHISHeaders.AddNew();
			var ultimateConsigneeAddress = ((IAPHISHeader)aphisheader).UltimateCosigneeDetails;
			AssertEquals("IAN TEST ADDRESS", ultimateConsigneeAddress.AddressLine1);

			InvoiceLine.JI_OA_ConsigneeAddress = orgHeader.MainAddress.PK;
			ultimateConsigneeAddress = ((IAPHISHeader)aphisheader).UltimateCosigneeDetails;
			AssertEquals("MAIN ADDRESS", ultimateConsigneeAddress.AddressLine1);
		}

		public void TestIsScientificDataRequired()
		{
			var list = new List<ZString>()
			{
				APHISCategoryTypeCodeList.Codes.LiveAnimals,
				APHISCategoryTypeCodeList.Codes.PropagativeMaterial,
				APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting,
				APHISCategoryTypeCodeList.Codes.FruitsAndVegetables,
				APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery,
				APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms
			};

			var aphisheader = InvoiceLine.APHISHeaders.AddNew();

			foreach (var categoryType in list)
			{
				aphisheader.US_CategoryType = categoryType;
				Assert(aphisheader.IsScientificDataRequired);
			}
		}

		public void TestCharacteristicsForCutFlowersAndGreenery()
		{
			var aphisheader = InvoiceLine.APHISHeaders.AddNew();
			aphisheader.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			aphisheader.US_ProductPhysicalState = "B";
			aphisheader.US_ProductStatus = "A";
			aphisheader.US_ProductCondition = "C";

			var iAPHISProductCharacteristic = aphisheader as IAPHISProductCharacteristic;
			AssertEquals(3, iAPHISProductCharacteristic.Characteristics.Count());

			var ab2 = iAPHISProductCharacteristic.Characteristics.Cast<APHISCharacteristic>().FirstOrDefault(x => x.CommodityQualifierCode == CommodityQualifier.CutFlowersAndGreeneryList.Codes.EndangeredSpeciesStatus);
			AssertNotNull(ab2);
			AssertEquals("A", ab2.CommodityCharacteristicQualifier);
		}

		public void TestGrowerShouldBeAvailableForCutFlowerGreenery()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			AssertEquals("CropGrowerOrgPKInfo.ReadOnly", true, header.CropGrowerOrgPKInfo.ReadOnly);
			AssertEquals("US_OA_CropGrowerAddressInfo.ReadOnly", true, header.US_OA_CropGrowerAddressInfo.ReadOnly);
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			AssertEquals("CropGrowerOrgPKInfo.ReadOnly", false, header.CropGrowerOrgPKInfo.ReadOnly);
			AssertEquals("US_OA_CropGrowerAddressInfo.ReadOnly", false, header.US_OA_CropGrowerAddressInfo.ReadOnly);
			header.Licenses.AddNew();
			AssertEquals("CropGrowerOrgPKInfo.ReadOnly", false, header.CropGrowerOrgPKInfo.ReadOnly);
			AssertEquals("US_OA_CropGrowerAddressInfo.ReadOnly", false, header.US_OA_CropGrowerAddressInfo.ReadOnly);
		}

		public void TestUSDAAPHISGrowerBeAvailableForPropagativeMaterial()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			AssertEquals(true, header.USDAAPHISGrowerPKInfo.ReadOnly);
			AssertEquals(true, header.US_OA_USDAAPHISGrowerAddressInfo.ReadOnly);
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			AssertEquals(false, header.USDAAPHISGrowerPKInfo.ReadOnly);
			AssertEquals(false, header.US_OA_USDAAPHISGrowerAddressInfo.ReadOnly);
		}

		public void TeestIsMiscellanenousAndProcessedProduct()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts;

			AssertEquals("Category Type is MiscellaneousAndProcessedProducts", true, header.MiscellaneousAndProcessedProductsCategory);

			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			AssertEquals("Category Type is CutFlowersAndGreenery", false, header.MiscellaneousAndProcessedProductsCategory);
		}

		public void TestCropGrowerDetailsRegistrationNumber()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.APHISAssignedNumber, "23GGFRD234");
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
			header.US_OA_CropGrowerAddress = orgHeader.MainAddress.PK;

			var iAPHISHeader = header as IAPHISHeader;
			AssertEquals("23GGFRD234", iAPHISHeader.CropGrowerDetailsRegistrationNumber.Number);
			AssertEquals(EntityIdentificationCodesList.Codes.APHISAssigned, iAPHISHeader.CropGrowerDetailsRegistrationNumber.NumberType);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "123");
			iAPHISHeader = header;
			AssertEquals("123", iAPHISHeader.CropGrowerDetailsRegistrationNumber.Number);
			AssertEquals(OrgCusCode.USACodeTypes.ManufacturerID, iAPHISHeader.CropGrowerDetailsRegistrationNumber.NumberType);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456");
			iAPHISHeader = header;
			AssertEquals("456", iAPHISHeader.CropGrowerDetailsRegistrationNumber.Number);
			AssertEquals(EntityIdentificationCodesList.Codes.DUNSNumber, iAPHISHeader.CropGrowerDetailsRegistrationNumber.NumberType);
		}

		public void TestClearPermitDest()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.ForResearchDevelopmentNonFoodProduct;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			header.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForSaleAndDistribution;
			header.US_OA_PermittedAddress = orgHeader.MainAddress.PK;

			header.US_CategoryCode = ArticleCategory.AnimalProductsAndByProductsList.Codes.MilkAndMilkProducts;
			AssertEquals("Permitted address should be cleared", ZGuid.Empty, header.US_OA_PermittedAddress);

			header.US_OA_PermittedAddress = orgHeader.MainAddress.PK;

			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.FruitsAndVegetables;
			AssertEquals("Permitted address should be cleared", ZGuid.Empty, header.US_OA_PermittedAddress);
		}

		#region Implementation

		protected override IEnumerable<APHISHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (APHISHeader)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var aphisHeader = invoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			return aphisHeader;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoiceLine.APHISHeaders.AddNew();
		}

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
