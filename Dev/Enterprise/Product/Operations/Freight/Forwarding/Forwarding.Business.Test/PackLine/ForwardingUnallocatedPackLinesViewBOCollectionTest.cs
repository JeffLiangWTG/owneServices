using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingUnallocatedPackLinesView))]
	sealed class ForwardingUnallocatedPackLinesViewBOCollectionTest : BusinessObjectCollectionViewTestCase<ForwardingUnallocatedPackLinesView>
	{
		protected override ForwardingUnallocatedPackLinesView GetCollectionToTest()
		{
			PackLineNonDependentCollection collection = new PackLineNonDependentCollection(Factory);
			return new ForwardingUnallocatedPackLinesView(null, collection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ForwardingPackLine result = Factory.New<ForwardingPackLine>();
			result.JL_FreightMode = FreightConstants.OuterPackType;
			result.JL_PackageCount = 2;
			return result;
		}
	}
}
