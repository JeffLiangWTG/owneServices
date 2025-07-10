using System;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USImportEntryLineFee))]
	sealed class USImportEntryLineFeeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIFeeMembers()
		{
			var fee = Factory.New<USImportEntryLineFee>();
			fee.USF_ChargeType = "DTY";
			fee.USF_ChargeAmount = 150.45m;
			IFee iFee = fee;
			AssertEquals("IFee.Code_Getter", "DTY", iFee.Code);
			AssertExceptionThrown<NotSupportedException>("IFee.Code_Setter", () => iFee.Code = "ONS");
			AssertEquals("IFee.Amount_Getter", 150.45m, iFee.Amount);
			AssertExceptionThrown<NotSupportedException>("IFee.Amount_Setter", () => iFee.Amount = 20m);
			AssertExceptionThrown<NotSupportedException>("IFee.SelectedRateType_Getter", () => _ = iFee.SelectedRateType);
			AssertExceptionThrown<NotSupportedException>("IFee.SelectedRateType_Setter", () => iFee.SelectedRateType = "D");
			AssertExceptionThrown<NotSupportedException>("IFee.IsOverridden", () => _ = iFee.IsOverridden);
			AssertExceptionThrown<NotSupportedException>("IFee.Delete", iFee.Delete);
		}

		protected override bool IsDeleteSupported() => false;
	}
}
