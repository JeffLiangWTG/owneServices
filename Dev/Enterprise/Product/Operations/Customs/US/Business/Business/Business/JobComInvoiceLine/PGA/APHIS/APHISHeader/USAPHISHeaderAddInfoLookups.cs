//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAPHISHeaderAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSAPHISHeaderAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using ArticleCategory = Enterprise.Customs.US.Business.APHIS.ArticleCategory;
using CommodityCharacteristicQualifier = Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier;
using CustomsUniversal = Enterprise.Core.Constants.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USAPHISHeaderAddInfoLookups : AutoUSAPHISHeaderAddInfoLookups
	{
		public USAPHISHeaderAddInfoLookups(AutoUSAPHISHeaderAddInfo parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairList Programs
		{
			get { return APHISProgramCodeList.GetActiveList(Factory); }
		}

		public ICodeDescriptionPairList ProcessingCodes
		{
			get { return APHISGovernmentAgencyProcessingCodeList.GetListForProgram(Factory, Parent.US_ProgramType); }
		}

		public ICodeDescriptionPairList CategoryTypes
		{
			get { return APHISCategoryTypeCodeList.GetListForProgram(Factory, Parent.US_ProgramType); }
		}

		public ICodeDescriptionPairList IntendedUseCodes => RefCusCodeListTypes.GetCachedListMatchAllAttributes(
			Factory,
			Core.Constants.CountryCodes.UnitedStates,
			CustomsUniversal.RefCusCodeListTypes.Codes.USPGAIntendUseCode,
			ZDateTime.Today,
			new[] { new KeyValuePair<ZString, ZString>(CustomsUniversal.RefCusCodeList.Attributes.USPGAIntendUseCodeAgency, CustomsUniversal.RefCusCodeList.AttributeValues.APH) },
			new[] { new KeyValuePair<ZString, ZString>(CustomsUniversal.RefCusCodeList.Attributes.USPGAIntendUseCodeProgram, null) });

		public ICodeDescriptionPairList CategoryCodes
		{
			get
			{
				switch (Parent.US_CategoryType)
				{
					case APHISCategoryTypeCodeList.Codes.LiveAnimals:
						return Factory.GetCachedValue<ArticleCategory.LiveAnimalsList>();
					case APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts:
						return Factory.GetCachedValue<ArticleCategory.RelatedAnimalProductsList>();
					case APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts:
						return Factory.GetCachedValue<ArticleCategory.AnimalProductsAndByProductsList>();
					case APHISCategoryTypeCodeList.Codes.PropagativeMaterial:
						return Factory.GetCachedValue<ArticleCategory.PropagativeMaterialList>();
					case APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting:
						return Factory.GetCachedValue<ArticleCategory.SeedsNotForPlantingList>();
					case APHISCategoryTypeCodeList.Codes.FruitsAndVegetables:
						return Factory.GetCachedValue<ArticleCategory.FruitsAndVegetablesList>();
					case APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts:
						return Factory.GetCachedValue<ArticleCategory.MiscellaneousAndProcessedProductsList>();
					case APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery:
						return Factory.GetCachedValue<ArticleCategory.CutFlowersAndGreeneryList>();
					case APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms:
						return Factory.GetCachedValue<ArticleCategory.GeneticallyEngineeredOrganismsList>();
					default:
						return new CodeDescriptionPairList();
				}
			}
		}

		public ICodeDescriptionPairList ProductConditions
		{
			get
			{
				switch (Parent.US_CategoryType)
				{
					case APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.AnimalProductsAndByProductsConditionA30List>();
					case APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.CutFlowersAndGreeneryTypeList>();
					case APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.GeneticallyEngineeredOrganismsTypeA101List>();
					case APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.RelatedAnimalProductsConditionA20List>();
					case APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.MiscellaneousAndProcessedProductsConditionList>();
					default:
						return new CodeDescriptionPairList();
				}
			}
		}

		public ICodeDescriptionPairList ProductIngredientTypes
		{
			get
			{
				switch (Parent.US_CategoryType)
				{
					case APHISCategoryTypeCodeList.Codes.FruitsAndVegetables:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.IngredientTypeList>();
					default:
						return new CodeDescriptionPairList();
				}
			}
		}

		public ICodeDescriptionPairList ProductPhysicalStates
		{
			get
			{
				switch (Parent.US_CategoryType)
				{
					case APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.AnimalProductsAndByProductsConditionA31List>();
					case APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.CutFlowersAndGreeneryPhysicalStateList>();
					case APHISCategoryTypeCodeList.Codes.FruitsAndVegetables:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.FruitsAndVegetablesList>();
					case APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.GeneticallyEngineeredOrganismsLifeStageA102List>();
					case APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.MiscellaneousAndProcessedProductsPhysicalStateList>();
					case APHISCategoryTypeCodeList.Codes.PropagativeMaterial:
						return CommodityCharacteristicQualifier.PropagativeMaterialLifeStageA41List.GetPropagativeMaterialLifeStageA41List(Factory, Parent.US_CategoryCode);
					case APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.RelatedAnimalProductsConditionA21List>();
					case APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.SeedsNotForPlantingList>();
					default:
						return new CodeDescriptionPairList();
				}
			}
		}

		public ICodeDescriptionPairList ProductComponents
		{
			get
			{
				switch (Parent.US_CategoryType)
				{
					case APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.AnimalProductsAndByProductsConditionA32List>();
					case APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms:
						return
							Factory.GetCachedValue<CommodityCharacteristicQualifier.GeneticallyEngineeredOrganismsIntergenericA100List>();
					default:
						return new CodeDescriptionPairList();
				}
			}
		}

		public ICodeDescriptionPairList ProductStatusList
		{
			get
			{
				switch (Parent.US_CategoryType)
				{
					case APHISCategoryTypeCodeList.Codes.PropagativeMaterial:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.PropagativeMaterialLifeStageA42List>();
					case APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.CutFlowersAndGreeneryLifeStageA82List>();
					default:
						return new CodeDescriptionPairList();
				}
			}
		}

		public ICodeDescriptionPairList GrowingMediaList
		{
			get
			{
				return CommodityCharacteristicQualifier.PropagativeMaterialLifeStageA43List.GetPropagativeMaterialA43List(Factory, Parent.US_CategoryCode, Parent.US_ProductPhysicalState);
			}
		}

		public ICodeDescriptionPairList ProductTypes
		{
			get { return ProductCodeQualifiersList.GetAPHISProductCodeQualifiers(Factory); }
		}

		public CodeDescriptionPairList UnitOfMeasureList
		{
			get
			{
				return Factory.GetCachedValue("APHISUnitOfMeasureList" + Parent.US_CategoryType, () =>
				{
					CodeDescriptionPairList result;
					if (Parent.US_CategoryType == APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery)
					{
						result = new CodeDescriptionPairList();
						result.AddPair(APHISUnitOfMeasureList.Codes.StemsOfCutFlowers, APHISUnitOfMeasureList.Descriptions.StemsOfCutFlowers);
						result.AddPair(APHISUnitOfMeasureList.Codes.Box, APHISUnitOfMeasureList.Descriptions.Box);
						result.AddPair(APHISUnitOfMeasureList.Codes.BouquetOfCutFlowers, APHISUnitOfMeasureList.Descriptions.BouquetOfCutFlowers);
						result.AddPair(APHISUnitOfMeasureList.Codes.Bunch, APHISUnitOfMeasureList.Descriptions.Bunch);
						if (ZZCustomsFunctionality.IsAPHIS2024Effective)
						{
							result.AddPair(APHISUnitOfMeasureList.Codes.KilogramsWeight, APHISUnitOfMeasureList.Descriptions.KilogramsWeight);
							result.AddPair(APHISUnitOfMeasureList.Codes.GramsWeight, APHISUnitOfMeasureList.Descriptions.GramsWeight);
						}
					}
					else
					{
						result = new APHISUnitOfMeasureList();
						if (!ZZCustomsFunctionality.IsAPHIS2024Effective)
						{
							result.RemoveCode(APHISUnitOfMeasureList.Codes.AnimalUnit);
							result.RemoveCode(APHISUnitOfMeasureList.Codes.StrawsAmpulesDoses);
						}
					}
					result.SortByDescription();
					return result;
				});
			}
		}

		public OrganisationsFindBoxCollection Organisations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		protected new USAPHISHeaderAddInfo Parent
		{
			get { return (USAPHISHeaderAddInfo)base.Parent; }
		}

		protected APHISHeader Header
		{
			get { return Parent.Parent; }
		}
	}
}
