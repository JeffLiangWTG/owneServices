using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInBondBillTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header = (CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();

			var moveDetail = header.Bills.AddNew();
			var row = ((INeedRow)moveDetail).Row;

			var typeDecider = new CusInBondBillTypeDecider();
			Type typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Enterprise.Customs.US.InBond.Business.CusInBondBill", typeForLoad.FullName);
		}
	}
}
