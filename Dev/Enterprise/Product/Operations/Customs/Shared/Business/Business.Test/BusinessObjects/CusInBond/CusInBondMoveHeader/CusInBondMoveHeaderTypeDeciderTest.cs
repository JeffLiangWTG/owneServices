using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInBondMoveHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header = (CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;

			var moveHeader = header.MovementHeader;
			var row = ((INeedRow)moveHeader).Row;

			var typeDecider = new CusInBondMoveHeaderTypeDecider();
			Type typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Enterprise.Customs.US.InBond.Business.CusInBondMoveHeader", typeForLoad.FullName);
		}

		public void TestGetRegistryBranchPK()
		{
			var header = (CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_GB = Guid.NewGuid();
			AssertEquals(header.BH_GB.ToGuid(), header.RegistryBranchPK);

			header.BH_GB = ZGuid.Invalid;
			AssertEquals(Guid.Empty, header.RegistryBranchPK);
		}
	}
}
