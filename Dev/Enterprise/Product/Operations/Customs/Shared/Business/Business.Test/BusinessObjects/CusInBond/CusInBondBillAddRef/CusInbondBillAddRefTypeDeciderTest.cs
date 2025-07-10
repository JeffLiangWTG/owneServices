using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	class CusInbondBillAddRefTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header = (CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;

			var bill = (CusInBondBill)header.Bills.AddNew();
			var additionalReferences = bill.GetPropertyValue<IBusinessObjectCollection>("AdditionalReferences");
			var additionalReference = additionalReferences.AddNew();

			var row = ((INeedRow)additionalReference).Row;

			var typeDecider = new CusInbondBillAddRefTypeDecider();
			var typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Enterprise.Customs.US.InBond.Business.CusInbondBillAddRef", typeForLoad.FullName);
		}

		public void TestWhenParentObjDoesSupportICusInbondBillAddRefTypeSupporter()
		{
			var header = (CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;

			var bill = (CusInBondBill)header.Bills.AddNew();
			var additionalReferences = bill.GetPropertyValue<IBusinessObjectCollection>("AdditionalReferences");
			var additionalReference = additionalReferences.AddNew();

			var row = ((INeedRow)additionalReference).Row;

			var typeDecider = new CusInbondBillAddRefTypeDecider();
			var header02 = Factory.New<DummyCusInBondHeader>();
			header02.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			var bill02 = Factory.New<DummyCusInBondBill>();
			var row02 = ((INeedRow)bill02).Row;
			row02[CusInBondBill.Schema.B0_BH] = header02.PK.ToGuid();
			row[CusInbondBillAddRef.Schema.BR_B0] = bill02.PK.ToGuid();
			CombineAssertions(() =>
			{
				AssertNull("Invalid Type", typeDecider.GetTypeForLoad(row, Factory));
				AssertNotNull(ErrorReporter.ExceptionsThrown.FirstOrDefault(x => x.Contains("Enterprise.Customs.Business.Testing.DummyCusInBondBill has not implement Enterprise.Customs.Business.ICusInbondBillAddRefTypeSupporter")));
				AssertNotNull(ErrorReporter.ExceptionsThrown.FirstOrDefault(x => x.Contains("Cannot determine the CusInbondBillAddRef parent object")));
				ErrorReporter.Clear();
			});
		}
	}
}
