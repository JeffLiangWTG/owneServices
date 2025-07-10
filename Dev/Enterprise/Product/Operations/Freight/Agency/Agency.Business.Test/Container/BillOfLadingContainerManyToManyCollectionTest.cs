using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BillOfLadingContainerManyToManyCollection))]
	internal class BillOfLadingContainerManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(BillOfLadingContainerManyToManyCollection), GetCollectionToTest().GetType());
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			BillOfLadingPackLine packLine = Factory.New<BillOfLading>().OuterPackLines.AddNew();
			return new BillOfLadingContainerManyToManyCollection(packLine);
		}
		#endregion
	}
}
