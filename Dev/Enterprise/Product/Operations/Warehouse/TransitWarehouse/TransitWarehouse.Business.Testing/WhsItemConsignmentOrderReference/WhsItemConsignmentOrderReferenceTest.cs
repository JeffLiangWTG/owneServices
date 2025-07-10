using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemConsignmentOrderReference))]
	public class WhsItemConsignmentOrderReferenceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consignmentOrderReference = factory.NewWithValidTestData<WhsItemConsignmentOrderReference>();
			consignmentOrderReference.WOR_ParentTableCode = "WRC";
			return consignmentOrderReference;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var consignmentOrderReference = Factory.NewWithValidTestData<WhsItemConsignmentOrderReference>();
			consignmentOrderReference.WOR_ParentTableCode = "WRC";
			return consignmentOrderReference;
		}
	}
}
