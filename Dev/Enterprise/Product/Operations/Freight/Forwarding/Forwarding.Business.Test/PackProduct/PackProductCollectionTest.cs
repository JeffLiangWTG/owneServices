using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(PackProductCollection))]
	sealed class PackProductCollectionTest : ActiveBusinessObjectCollectionTestCase<PackProductCollection>
	{
		#region Implementation

		protected override PackProductCollection GetCollectionToTest()
		{
			ForwardingPackLine packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			PackProductCollection collection = new PackProductCollection(packLine);

			return collection;
		}

		#endregion
	}
}
