using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefShippingLineEBLProviderCollection))]
	class RefShippingLineEBLProviderCollectionTest : ActiveBusinessObjectCollectionTestCase<RefShippingLineEBLProviderCollection>
	{
		public void TestCreateRelationshipFilter()
		{
			var refShippingLine = Factory.NewWithValidTestData<RefShippingLine>();

			var refShippingLineEBLProvider1 = Factory.NewWithValidTestData<RefShippingLineEBLProvider>();
			refShippingLineEBLProvider1.RSE_RSL_ShippingLine = refShippingLine.PK;

			Factory.NewWithValidTestData<RefShippingLineEBLProvider>().RSE_RSL_ShippingLine = Factory.NewWithValidTestData<RefShippingLine>().PK;

			Factory.Save();

			var refShippingLineEBLProviderCollection = new RefShippingLineEBLProviderCollection(refShippingLine);

			AssertEquals(1, refShippingLineEBLProviderCollection.Count);
			AssertEquals(refShippingLineEBLProvider1, refShippingLineEBLProviderCollection.First());
		}
	}
}
