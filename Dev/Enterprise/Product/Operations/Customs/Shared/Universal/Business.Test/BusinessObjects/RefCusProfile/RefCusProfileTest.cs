using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProfile))]
	class RefCusProfileTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var profileType = Helper.CreateRefCusProfileType("PP0", "HSN", Core.Constants.CountryCodes.Brazil, "dummy Description 0");
			return Helper.CreateRefCusProfile(profileType, "12345678", "ATT1", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		public void TestGetAttributeValue()
		{
			Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var tariffType1 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "HSN", "NCM");
			Factory.Save();
			var tariff1 = Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1.PK, "12345678", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), "dummy Description 0");
			var tariff2 = Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1.PK, "87654321", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), "dummy Description 0");
			var profileType1 = Helper.CreateRefCusProfileType(tariffType1.PK, Core.Constants.CountryCodes.SouthAfrica, "PP0", "dummy Description 0");
			Factory.Save();
			var attributes1 = new List<(string, string)>()
			{
				("LegalCode", "LEGAL CODE 1"),
				("Mandatory", "MANDATORY 1"),
			};
			var profile1 = Helper.CreateRefCusProfile(profileType1, Core.Constants.CountryCodes.SouthAfrica, "12345678", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), attributes1);
			Factory.Save();

			AssertEquals("Should contain 2 attributes", 2, profile1.Attributes.Count);

			CombineAssertions(() =>
			{
				Assert("GetAttributeValue should be Empty for unknow attribute", profile1.GetAttribute("Dummy").IsEmpty);
				AssertEquals("GetAttributeValue when LegalCode should be", "LEGAL CODE 1", profile1.GetAttribute("LegalCode"));
				AssertEquals("GetAttributeValue when Mandatory should be", "MANDATORY 1", profile1.GetAttribute("Mandatory"));
			});
		}

		UniversalReferenceTestDataHelper Helper => helper ??= new UniversalReferenceTestDataHelper(Factory);
		UniversalReferenceTestDataHelper helper;
	}
}
