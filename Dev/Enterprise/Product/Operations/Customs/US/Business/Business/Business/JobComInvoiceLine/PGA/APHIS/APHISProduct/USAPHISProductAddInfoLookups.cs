//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAPHISProductAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSAPHISProductAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using CommodityCharacteristicQualifier = Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier;

namespace Enterprise.Customs.US.Business
{
	public class USAPHISProductAddInfoLookups : AutoUSAPHISProductAddInfoLookups
	{
		public USAPHISProductAddInfoLookups(AutoUSAPHISProductAddInfo parent)
			: base(parent)
		{
		}

		public CommodityCharacteristicQualifier.LiveAnimalsAgeList AgeList
		{
			get { return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsAgeList>(); }
		}

		public ICodeDescriptionPairList LiveAnimalsAllBreedList
		{
			get
			{
				return Factory.GetCachedValue<ICodeDescriptionPairList>("LiveAnimalsAllBreedList", () =>
				{
					var result = new CommodityCharacteristicQualifier.LiveAnimalsBirdsList();
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsBuffaloBisonList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsCamelList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsCattleList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsChickenList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsDeerMooseCervidList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsDogList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsDonkeyList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsDuckList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsFinFishList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsGoatList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsGooseList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsHorseList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsLlamaAlpacaList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsOtherList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsPoultryOtherList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsReindeerList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsSheepList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsSwineList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsTurkeyList>());
					result.AddRange(Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsZooList>());
					result.Sort();
					return result;
				});
			}
		}

		public ICodeDescriptionPairList BreedVarietyList
		{
			get
			{
				switch (Parent.US_ShowBreed)
				{
					case APHISBreedList.Codes.BirdsNotListedInPoultry:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsBirdsList>();
					case APHISBreedList.Codes.BuffaloBison:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsBuffaloBisonList>();
					case APHISBreedList.Codes.Camel:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsCamelList>();
					case APHISBreedList.Codes.Cattle:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsCattleList>();
					case APHISBreedList.Codes.ChickenPoultry:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsChickenList>();
					case APHISBreedList.Codes.DeerMooseCervid:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsDeerMooseCervidList>();
					case APHISBreedList.Codes.Dog:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsDogList>();
					case APHISBreedList.Codes.Donkey:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsDonkeyList>();
					case APHISBreedList.Codes.DuckPoultry:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsDuckList>();
					case APHISBreedList.Codes.FinFish:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsFinFishList>();
					case APHISBreedList.Codes.Goat:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsGoatList>();
					case APHISBreedList.Codes.GoosePoultry:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsGooseList>();
					case APHISBreedList.Codes.Horse:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsHorseList>();
					case APHISBreedList.Codes.LlamaAlpaca:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsLlamaAlpacaList>();
					case APHISBreedList.Codes.OtherLiveAnimals:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsOtherList>();
					case APHISBreedList.Codes.OtherPoultry:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsPoultryOtherList>();
					case APHISBreedList.Codes.Reindeer:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsReindeerList>();
					case APHISBreedList.Codes.Sheep:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsSheepList>();
					case APHISBreedList.Codes.Swine:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsSwineList>();
					case APHISBreedList.Codes.TurkeyPoultry:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsTurkeyList>();
					case APHISBreedList.Codes.ZooAnimals:
						return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsZooList>();
					default:
						return LiveAnimalsAllBreedList;
				}
			}
		}
		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		public ICodeDescriptionPairList SourceTypes
		{
			get { return SourceTypeCodesList.GetListForAPHIS(Factory, Product.ProgramType); }
		}

		public ICodeDescriptionPairList ProcessingTypes
		{
			get
			{
				var programType = ZString.Empty;
				var categoryType = ZString.Empty;

				var header = Product.Header;
				if (header != null)
				{
					programType = header.US_ProgramType;
					categoryType = header.US_CategoryType;
				}

				return APHISProcessingTypeCodeList.GetListFor(Factory, programType, categoryType);
			}
		}

		public CommodityCharacteristicQualifier.LiveAnimalsColorList ColorList
		{
			get { return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsColorList>(); }
		}

		public ICodeDescriptionPairList PregnantList
		{
			get { return YesNoDefaultList.GetCachedYesNoList(Factory); }
		}

		public CommodityCharacteristicQualifier.LiveAnimalsGenderList GenderList
		{
			get { return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsGenderList>(); }
		}

		public CommodityCharacteristicQualifier.LiveAnimalsGestationalAgeList GestationalAgeList
		{
			get { return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsGestationalAgeList>(); }
		}

		public CommodityCharacteristicQualifier.LiveAnimalsProtectedSpeciesList ProtectedSpeciesList
		{
			get { return Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsProtectedSpeciesList>(); }
		}

		public RefCountryCollection OriginList
		{
			get { return new RefCountryCollection(Factory); }
		}

		public APHISBreedList BreedList
		{
			get { return Factory.GetCachedValue<APHISBreedList>(); }
		}

		public CommodityCharacteristicQualifier.AnimalProductsAndByProductsConditionA32List TypeList
		{
			get { return Factory.GetCachedValue<CommodityCharacteristicQualifier.AnimalProductsAndByProductsConditionA32List>(); }
		}

		protected new USAPHISProductAddInfo Parent
		{
			get { return (USAPHISProductAddInfo)base.Parent; }
		}

		protected APHISProduct Product
		{
			get { return Parent.Parent; }
		}
	}
}
