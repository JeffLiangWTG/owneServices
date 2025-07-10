using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(ReadOnlyWhsCartonGroupSizeLinkCollection))]
	class ReadOnlyWhsCartonGroupSizeLinkCollectionTest : ActiveBusinessObjectCollectionTestCase<ReadOnlyWhsCartonGroupSizeLinkCollection>
	{
		public void TestAllowNew()
		{
			var collection = new ReadOnlyWhsCartonGroupSizeLinkCollection(Factory.New<WhsCartonGroup>());
			AssertEquals(false, ((IBindingList)collection).AllowNew);
		}

		#region Implementation

		protected override ReadOnlyWhsCartonGroupSizeLinkCollection GetCollectionToTest()
		{
			return new ReadOnlyWhsCartonGroupSizeLinkCollection(Factory.New<WhsCartonGroup>());
		}

		#endregion
	}
}
