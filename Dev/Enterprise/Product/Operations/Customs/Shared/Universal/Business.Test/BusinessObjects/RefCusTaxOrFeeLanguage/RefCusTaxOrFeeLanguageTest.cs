using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTaxOrFeeLanguage))]
	class RefCusTaxOrFeeLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CreateNewRefCusTaxOrFeeLanguage(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateNewRefCusTaxOrFeeLanguage(Factory);
		}

		RefCusTaxOrFeeLanguage CreateNewRefCusTaxOrFeeLanguage(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateRefCusTaxOrFeeType("VAT", "Value Added Tax");
			helper.CreateOrGetLanguage("ENG", "English");
			factory.Save();
			var taxOrFee = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, 0, 0, "VAT");
			var taxOrFeeLanguage = helper.CreateTaxOrFeeLanguage(taxOrFee, "ENG", "business tax");
			return taxOrFeeLanguage;
		}
	}
}
