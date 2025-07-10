using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.ReflectiveFieldMap;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolCostSupporterTest : TestCaseWithFactory
	{
		public void TestShipmentsShouldNotBeIncluded()
		{
			var dataProvider = new DocDataProviderReflector(typeof(ForwardingConsol.ForwardingConsolCostSupporter));
			Assert("Shipments should not be included.", !dataProvider.Members.Any(property => ((PropertyDescription)property).Property.Name == "Shipments"));
		}

		public void TestIGenericJobCostSupporter_Precondition()
		{
			var consol = Factory.New<ForwardingConsol>();
			var costSupporter = (IGenericJobCostSupporter)new ForwardingConsol.ForwardingConsolCostSupporter(consol);

			AssertEquals("ExcludedApportionmentMethods", 0, costSupporter.ExcludedApportionmentMethods.Count());
			AssertEquals("IsApportionmentFilterEnabled", true, costSupporter.IsApportionmentFilterEnabled);
		}
	}
}
