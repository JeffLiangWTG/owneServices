using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(BillContainerCollection))]
	sealed class HouseBillContainerCollectionTest : Customs.Business.Testing.BaseHouseBillContainerCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new BillContainerCollection((Bill)HouseBill);
	}
}
