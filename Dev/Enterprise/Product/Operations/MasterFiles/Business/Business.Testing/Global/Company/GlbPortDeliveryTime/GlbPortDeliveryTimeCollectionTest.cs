using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPortDeliveryTimeCollection))]
	sealed class GlbPortDeliveryTimeCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbPortDeliveryTimeCollection>
	{
		protected override GlbPortDeliveryTimeCollection GetCollectionToTest()
		{
			return new GlbPortDeliveryTimeCollection(Factory);
		}
	}
}
