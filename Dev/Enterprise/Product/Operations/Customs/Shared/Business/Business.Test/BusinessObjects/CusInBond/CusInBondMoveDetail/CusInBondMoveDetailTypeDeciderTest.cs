using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInBondMoveDetailTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header = (CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;

			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var row = ((INeedRow)moveDetail).Row;

			var typeDecider = new CusInBondMoveDetailTypeDecider();
			Type typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Enterprise.Customs.US.InBond.Business.CusInBondMoveDetail", typeForLoad.FullName);
		}
	}
}
