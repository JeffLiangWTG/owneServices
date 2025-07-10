using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(BondedFactoryOrgHeaderCollection))]
	sealed class BondedFactoryOrgHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BondedFactoryOrgHeaderCollection(Factory);
		}

		[ExpectNoExceptions]
		public void TestFilterBusinessObjectDefaults()
		{
			var col = new BondedFactoryOrgHeaderCollection(Factory);
			var property1 = col.FilterBusinessObjectDefaults["Registration Country/Type:Property1"];
			var property2 = col.FilterBusinessObjectDefaults["Registration Country/Type:Property2"];

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(property1.Value, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Taiwan).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(property1.IsRemovable, NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(property2.Value, NUnit.Framework.Is.EqualTo(OrgCusCode.TaiwanCodeTypes.CBF).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(property2.IsRemovable, NUnit.Framework.Is.EqualTo(true));
			});
		}
	}
}
