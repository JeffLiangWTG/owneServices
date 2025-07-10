using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestMethodOfPaymentList()
	{
		var fee = GetFee();
		var list = fee.Lookups.MethodOfPaymentList;
		CombineAssertions(() =>
		{
			AssertEquals("Symbols", "A, B, C, D, E, G, H, J, K, L, O, P, R, S, T, U, V, Z", list.CodesAsString);
			AssertSame("Cached", list, fee.Lookups.MethodOfPaymentList);
		});
	}

	CusEntryLineFee GetFee()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		return entryLine.Fees.AddNew();
	}
}
