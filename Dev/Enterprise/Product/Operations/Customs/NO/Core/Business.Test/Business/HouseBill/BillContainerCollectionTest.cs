using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(BillContainerCollection))]
	sealed class BillContainerCollectionTest : Customs.Business.Testing.BaseHouseBillContainerCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => CreateContainers();

		protected override Customs.Business.BaseBillContainerCollection CreateContainers() => new BillContainerCollection((Bill)HouseBill);
	}
}
