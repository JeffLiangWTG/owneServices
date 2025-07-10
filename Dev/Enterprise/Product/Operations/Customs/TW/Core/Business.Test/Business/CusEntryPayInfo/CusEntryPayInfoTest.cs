using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryPayInfo))]
	sealed class CusEntryPayInfoTest : Customs.Business.Testing.CusEntryPayInfoTest
	{
		[ExpectNoExceptions]
		public void TestTypeDescription()
		{
			var entryPayInfo = Factory.NewWithValidTestData<CusEntryPayInfo>();
			var entryChargeTypeList = new EntryChargeTypeList();
			foreach (CodeDescriptionPair dutyTaxFeeCodePair in entryChargeTypeList)
			{
				entryPayInfo.C9_TransactionType = dutyTaxFeeCodePair.Code;
				NUnit.Framework.Assert.That(entryPayInfo.TypeDescription, NUnit.Framework.Is.EqualTo(dutyTaxFeeCodePair.Description).Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestReasonDescription()
		{
			var entryPayInfo = Factory.NewWithValidTestData<CusEntryPayInfo>();
			var reasonOfPaymentList = new ReasonOfPaymentList();
			foreach (CodeDescriptionPair reasonOfPaymentPair in reasonOfPaymentList)
			{
				entryPayInfo.C9_PaymentReasonCode = reasonOfPaymentPair.Code;
				NUnit.Framework.Assert.That(entryPayInfo.ReasonDescription, NUnit.Framework.Is.EqualTo(reasonOfPaymentPair.Description).Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestOtherChargeDeductionAmount()
		{
			var entryPayInfo = Factory.New<CusEntryPayInfo>();
			entryPayInfo.OtherChargeDeductionAmount = 9999m;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryPayInfo.OtherChargeDeductionAmount, NUnit.Framework.Is.EqualTo(9999m).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(entryPayInfo.GetSystemDefinedValue<ZDecimal>(Constants.GenAddOnColumnFieldName.OtherChargeDeductionAmount), NUnit.Framework.Is.EqualTo(9999m).Using(CustomComparers.TypeComparison));
			});
		}
	}
}
