using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPartShipCollection<CusPartShip>))]
	sealed class CusPartShipCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			return new CusPartShipCollection<CusPartShip>(hawb, Factory);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(CusPartShipCollection<CusPartShip>);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusPartShip>();
		}
	}
}
