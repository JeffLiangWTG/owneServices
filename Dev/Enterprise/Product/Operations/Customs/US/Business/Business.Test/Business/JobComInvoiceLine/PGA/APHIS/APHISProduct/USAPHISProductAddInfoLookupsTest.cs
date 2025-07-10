using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using CommodityCharacteristicQualifier = Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USAPHISProductAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAgeList()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var list = Product.AddInfoLookups.AgeList;
			AssertEquals("AgeList", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsAgeList>(), list);
		}
		public void TestSourceTypes()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var list = Product.AddInfoLookups.SourceTypes;
			AssertEquals("Should be cached", SourceTypeCodesList.GetListForAPHIS(Factory, APHISProgramCodeList.Codes.AVS), list);
		}

		public void TestCountries()
		{
			AssertEquals("Countries", typeof(RefCountryCollection), Product.AddInfoLookups.Countries.GetType());
		}

		public void TestProcessingTypes()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var list = Product.AddInfoLookups.ProcessingTypes;
			AssertEquals("Should be cached", APHISProcessingTypeCodeList.GetListFor(Factory, APHISProgramCodeList.Codes.AVS, APHISCategoryTypeCodeList.Codes.LiveAnimals), list);
		}

		public void TestBreedVarietyList()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			Product.US_ShowBreed = APHISBreedList.Codes.BirdsNotListedInPoultry;
			var allList = new CodeDescriptionPairList();
			Product.US_ShowBreed = APHISBreedList.Codes.BirdsNotListedInPoultry;
			var list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("BirdsNotListedInPoultry", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsBirdsList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.BuffaloBison;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("BuffaloBison", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsBuffaloBisonList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.Camel;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("Camel", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsCamelList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.Cattle;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("Cattle", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsCattleList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.ChickenPoultry;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("ChickenPoultry", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsChickenList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.DeerMooseCervid;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("DeerMooseCervid", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsDeerMooseCervidList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.Dog;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("Dog", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsDogList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.Donkey;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("Donkey", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsDonkeyList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.DuckPoultry;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("DuckPoultry", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsDuckList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.FinFish;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("FinFish", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsFinFishList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.Goat;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("Goat", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsGoatList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.GoosePoultry;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("GoosePoultry", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsGooseList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.Horse;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("Horse", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsHorseList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.LlamaAlpaca;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("LlamaAlpaca", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsLlamaAlpacaList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.OtherLiveAnimals;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("OtherLiveAnimals", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsOtherList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.OtherPoultry;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("OtherPoultry", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsPoultryOtherList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.Reindeer;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("Reindeer", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsReindeerList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.Sheep;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("Sheep", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsSheepList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.Swine;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("Swine", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsSwineList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.TurkeyPoultry;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("TurkeyPoultry", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsTurkeyList>(), list);
			Product.US_ShowBreed = APHISBreedList.Codes.ZooAnimals;
			list = Product.AddInfoLookups.BreedVarietyList;
			allList.AddRange(list);
			AssertEquals("ZooAnimals", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsZooList>(), list);
			Product.US_ShowBreed = "";
			list = Product.AddInfoLookups.BreedVarietyList;
			AssertEquals("All List is cached", list, Product.AddInfoLookups.BreedVarietyList);
			AssertEquals("All List is cached", list, Product.AddInfoLookups.LiveAnimalsAllBreedList);
			AssertEquals("All List", allList.Count, list.Count);
			var result = new ZStringBuilder();
			foreach (ICodeDescription pair in allList)
			{
				var desc1 = pair.Description;
				var desc2 = list.GetDescriptionFromCode(pair.Code);
				if (desc1 != desc2)
				{
					result.Append(string.Format("{0}_{1}_{2}", pair.Code, desc1, desc2));
				}
			}
			if (!result.IsEmpty)
			{
				Fail("The following did not matched:\r\n" + result.ToStringWithNewLineBetweenAppends());
			}
		}

		public void TestColorList()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var list = Product.AddInfoLookups.ColorList;
			AssertEquals("ColorList", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsColorList>(), list);
		}

		public void TestPregnantList()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var list = Product.AddInfoLookups.PregnantList;
			AssertEquals("PregnantList", YesNoDefaultList.GetCachedYesNoList(Factory), list);
		}

		public void TestGenderList()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var list = Product.AddInfoLookups.GenderList;
			AssertEquals("GenderList", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsGenderList>(), list);
		}

		public void TestGestationalAgeList()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var list = Product.AddInfoLookups.GestationalAgeList;
			AssertEquals("GestationalAgeList", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsGestationalAgeList>(), list);
		}

		public void TestProtectedSpeciesList()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var list = Product.AddInfoLookups.ProtectedSpeciesList;
			AssertEquals("ProtectedSpeciesList", Factory.GetCachedValue<CommodityCharacteristicQualifier.LiveAnimalsProtectedSpeciesList>(), list);
		}

		public void TestOriginList()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var list = Product.AddInfoLookups.OriginList;
			AssertEquals("OriginList", typeof(RefCountryCollection), list.GetType());
		}

		public void TestBreedList()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var list = Product.AddInfoLookups.BreedList;
			AssertEquals("BreedList", Factory.GetCachedValue<APHISBreedList>(), list);
		}

		public void TestTypeList()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var list = Product.AddInfoLookups.TypeList;
			AssertEquals("TypeList", Factory.GetCachedValue<CommodityCharacteristicQualifier.AnimalProductsAndByProductsConditionA32List>(), list);
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

		#endregion
	}
}
