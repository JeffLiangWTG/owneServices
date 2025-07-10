using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProcedureLanguage))]
	class RefCusProcedureLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CreateRefCusProcedureLanguage(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateRefCusProcedureLanguage(Factory);
		}

		RefCusProcedureLanguage CreateRefCusProcedureLanguage(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
			helper.CreateOrGetLanguage("FR", "French");
			Factory.Save();

			var tariffTypeAT = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Switzerland, "", "P1", "", "", "Alcohol Tax", "");
			return helper.CreateRefCusProcedureLanguage(tariffTypeAT, "FR", "Taxe sur l'alcool");
		}
	}
}
