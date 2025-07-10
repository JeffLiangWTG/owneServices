using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInBondContainerTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header = (CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;

			var bill = (CusInBondBill)header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();

			var row = ((INeedRow)container).Row;

			var typeDecider = new CusInBondContainerTypeDecider();
			Type typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Enterprise.Customs.US.InBond.Business.CusInBondContainer", typeForLoad.FullName);

			// CusAddInfo is use by NCTS and is tested in NCTS solution
		}
	}
}
