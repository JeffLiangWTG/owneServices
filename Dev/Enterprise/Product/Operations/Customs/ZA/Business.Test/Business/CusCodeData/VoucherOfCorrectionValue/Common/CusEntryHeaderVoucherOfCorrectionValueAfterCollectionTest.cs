using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderVoucherOfCorrectionValueAfterCollection))]
	class CusEntryHeaderVoucherOfCorrectionValueAfterCollectionTest : CusCodeDataCollectionTest<VoucherOfCorrectionValueAfter>
	{
		protected override CusCodeDataCollection<VoucherOfCorrectionValueAfter> GetCusCodeDataCollection() => Factory.New<CusEntryHeader>().VoucherOfCorrectionValueAfters;

		public void TestCanSetZeroValue()
		{
			CombineAssertions("ePP values", () =>
			{
				var entryHeader = Factory.New<CusEntryHeader>();
				var collection = entryHeader.VoucherOfCorrectionValueAfters;
				collection.SetValue(VOCValueTypeList.Codes.Penalty, 0.0m, entryHeader.PenaltyAmountAfterInfo);
				collection.SetValue(VOCValueTypeList.Codes.ProvisionalPayment, 0.0m, entryHeader.ProvisionalPaymentAmountAfterInfo);

				AssertEquals(2, collection.Count);
				AssertEquals(VOCValueTypeList.Codes.Penalty, collection[0].CY_Code);
				AssertEquals(0.0m, collection[0].CY_Value);
				AssertEquals(VOCValueTypeList.Codes.ProvisionalPayment, collection[1].CY_Code);
				AssertEquals(0.0m, collection[1].CY_Value);
			});

			CombineAssertions("Non-ePP value", () =>
			{
				var entryHeader = Factory.New<CusEntryHeader>();
				var collection = entryHeader.VoucherOfCorrectionValueAfters;
				collection.SetValue(VOCValueTypeList.Codes.CustomsValue, 0.0m, entryHeader.CustomsValueInfo);

				AssertEquals(0, collection.Count);
				AssertEquals(false, collection.ContainsCode(VOCValueTypeList.Codes.CustomsValue));
			});
		}
	}
}
