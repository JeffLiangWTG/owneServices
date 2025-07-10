using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(BillContainerCollection))]
	sealed class BillContainerCollectionTest : Customs.Business.Testing.BaseHouseBillContainerCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BillContainerCollection(HouseBill);
		}

		protected override Customs.Business.BaseBillContainerCollection CreateContainers()
		{
			return new BillContainerCollection(HouseBill);
		}

		new Bill HouseBill
		{
			get
			{
				return (Bill)base.HouseBill;
			}
		}
	}
}
