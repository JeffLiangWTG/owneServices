using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(WarehouseOrganisationsFindBoxCollection))]
	sealed class WarehouseOrganisationsFindBoxCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WarehouseOrganisationsFindBoxCollection(Factory);
		}

		[ExpectNoExceptions]
		public void TestFilterBusinessObjectDefaults()
		{
			var collection = new WarehouseOrganisationsFindBoxCollection(Factory);
			var filter = collection.FilterBusinessObjectDefaults["Registration Country/Type:Property1"];
			NUnit.Framework.Assert.That(filter.Value, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Taiwan).Using(CustomComparers.TypeComparison), "Registration Country/Type:Property1:1");
			filter = collection.FilterBusinessObjectDefaults["Registration Country/Type:Property2"];
			NUnit.Framework.Assert.That(filter.Value, NUnit.Framework.Is.EqualTo(OrgCusCode.CodeTypes.WarehouseControlledPremisesID).Using(CustomComparers.TypeComparison), "Registration Country/Type:Property2:1");
		}
	}
}
