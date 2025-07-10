using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public sealed class CusInBondFeeTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var header = (CusInBondHeader)Factory.New<Integration.Customs.EU.NCTS.ICusInBondHeader>();
				header.BH_HeaderType = "D";
				var bill = (CusInBondBill)Factory.New<Integration.Customs.EU.NCTS.ICusInBondBill>();
				bill.B0_BH = header.PK;
				var goodItem = (CusInBondCargoDesc)Factory.New<Integration.Customs.EU.NCTS.IDepartureCargoDesc>();
				goodItem.BY_ParentID = bill.PK;
				goodItem.BY_ParentTableCode = bill.TablePrefix;
				var fee = (CusInBondFee)Factory.New<Integration.Customs.EU.NCTS.INctsCargoDescFee>();
				fee.BFE_BY = goodItem.PK;
				var row = ((INeedRow)fee).Row;

				var typeDecider = new CusInBondFeeTypeDecider();
				Type typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
				AssertEquals("Enterprise.Customs.EU.NCTS.Business.NctsCargoDescFee", typeForLoad.FullName);
			}
		}
	}
}
