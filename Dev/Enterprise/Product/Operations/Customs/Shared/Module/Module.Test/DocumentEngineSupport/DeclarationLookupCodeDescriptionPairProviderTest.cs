using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class DeclarationLookupCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public void TestCodeDescriptionPairList()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var expectedList = GetMessageSubTypeListForCountry();
			var actualListForAU = CreateCodeDescriptionPairListProvider() as ICodeDescriptionPairListProvider;
			AssertListEqual(actualListForAU.CodeDescriptionPairList, expectedList);
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			expectedList = GetMessageSubTypeListForCountry();
			var actualListForUS = CreateCodeDescriptionPairListProvider() as ICodeDescriptionPairListProvider;
			AssertListEqual(actualListForUS.CodeDescriptionPairList, expectedList);
		}

		public override void TestIsReturningCorrectCollection()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var actualListForAU = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			var expectedList = GetMessageSubTypeListForCountry();
			AssertListEqual(actualListForAU, expectedList);
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var actualListForUS = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			expectedList = GetMessageSubTypeListForCountry();
			AssertListEqual(actualListForUS, expectedList);
			AssertListsForDifferentCountriesDiffer(actualListForAU, actualListForUS);
		}

		protected abstract CodeDescriptionPairList GetSpecificLookupFromJobDecLookup(JobDeclarationFilterLookups lookups);

		void AssertListsForDifferentCountriesDiffer(ReadOnlyCodeDescriptionPairList country1List, ReadOnlyCodeDescriptionPairList country2List)
		{
			Assert("Should be different", !AreListsEqual(country1List, country2List));
		}

		CodeDescriptionPairList GetMessageSubTypeListForCountry()
		{
			return GetSpecificLookupFromJobDecLookup(Lookups);
		}

		JobDeclarationFilterLookups Lookups
		{
			get
			{
				return Factory.GetJobDeclarationFilterBusinessObjectForCountry().Lookups;
			}
		}

		public JobDeclarationFilterBusinessObjectFactory Factory
		{
			get
			{
				return factory ?? (factory = new JobDeclarationFilterBusinessObjectFactory());
			}
		}

		JobDeclarationFilterBusinessObjectFactory factory;
		bool AreListsEqual(ReadOnlyCodeDescriptionPairList list1, ReadOnlyCodeDescriptionPairList list2)
		{
			bool result = true;
			if (list1.Count != list2.Count)
			{
				result = false;
			}
			else
			{
				foreach (CodeDescriptionPair element in list1)
				{
					if (!list2.ContainsCode(element.Code))
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}
	}
}
