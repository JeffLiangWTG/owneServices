using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BillOfLadingContainerDependentCollection))]
	internal class BillOfLadingContainerDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTestingCorrectCollection()
		{
			AssertEquals(typeof(BillOfLadingContainerDependentCollection), GetCollectionToTest().GetType());
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<BillOfLading>().RealContainers;
		}
		#endregion
	}
}
