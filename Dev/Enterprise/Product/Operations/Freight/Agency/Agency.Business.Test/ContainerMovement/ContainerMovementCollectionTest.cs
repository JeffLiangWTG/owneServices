using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerMovementCollection))]
	internal class ContainerMovementCollectionTest : ActiveBusinessObjectCollectionTestCase<ContainerMovementCollection>
	{
		public void TestAllowNew()
		{
			IBindingList collection = new ContainerMovementCollection(Factory, true);
			AssertEquals(true, collection.AllowNew);
			collection = new ContainerMovementCollection(Factory, false);
			AssertEquals(false, collection.AllowNew);
			collection = new ContainerMovementCollection(Factory, true, new AdhocCollectionRelationship(typeof(ContainerMovement)));
			AssertEquals(true, collection.AllowNew);
			collection = new ContainerMovementCollection(Factory, false, new AdhocCollectionRelationship(typeof(ContainerMovement)));
			AssertEquals(false, collection.AllowNew);
		}

		#region Implementation
		protected override ContainerMovementCollection GetCollectionToTest()
		{
			return new ContainerMovementCollection(Factory, false);
		}
		#endregion
	}
}
