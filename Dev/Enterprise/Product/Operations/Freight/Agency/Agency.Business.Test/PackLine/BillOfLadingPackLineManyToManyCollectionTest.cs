using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business
{
	[TestedType(typeof(BillOfLadingPackLineManyToManyCollection))]
	internal class BillOfLadingPackLineManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<BillOfLading>().RealContainers.AddNew().PackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<BillOfLading>().OuterPackLines.AddNew();
		}
		#endregion
	}
}
