using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(OrderManagerRequestMappingCollection))]
	sealed class OrderManagerRequestMappingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<OrderManagerRequestMappingCollection>
	{
		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => false;

		protected override OrderManagerRequestMappingCollection GetCollectionToTest()
		{
			return new OrderManagerRequestMappingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrderManagerRequestMapping();
		}

		#endregion
	}
}
