using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(CusBondDetailCollection<AsycudaBill>))]
	public class CusBondDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultValuesForNewChild()
		{
			var coll = (CusBondDetailCollection<AsycudaBill>)GetCollectionToTest();
			var element = coll.AddNew();
			AssertEquals(coll.Master.PK, element.Parent.PK);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var bill = Factory.New<AsycudaBill>();
			return new CusBondDetailCollection<AsycudaBill>(bill);
		}
	}
}
