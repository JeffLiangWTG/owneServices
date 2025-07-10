using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Module.Testing
{
	[TestedType(typeof(CusInBondBillModuleCollection))]
	sealed class CusInBondBillStandaloneCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestOceanBillTypeIsNotIncluded()
		{
			var inbondHeader = Factory.New<CusInBondHeader>();
			inbondHeader.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			inbondHeader.BH_ApplicationCode = "AMS";
			var inbondMoveHeader1 = inbondHeader.InBondMovementHeaders.AddNew();
			var bill1 = inbondHeader.Bills.AddNew();
			Factory.Save();
			var bills = Factory.Load<CusInBondBill>(new ZQuery(CusInBondBillSchema.B0_BH, inbondHeader.PK));
			AssertEquals(2, bills.Length);
			Assert(bills.Any(x => x.B0_ShipmentType == CusInBondBill.OceanBillType));
			var collection = new CusInBondBillModuleCollection(Factory);
			collection.Load();
			AssertEquals(1, collection.Count);
			Assert(!collection.Cast<CusInBondBill>().Any(x => x.B0_ShipmentType == CusInBondBill.OceanBillType));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusInBondBillModuleCollection(Factory);
		}
	}
}
