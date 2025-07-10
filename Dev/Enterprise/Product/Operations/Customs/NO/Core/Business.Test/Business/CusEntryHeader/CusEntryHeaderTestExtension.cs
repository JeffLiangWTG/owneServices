using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	static class CusEntryHeaderTestExtension
	{
		public static CusEntryHeader AddAgricultureFee(this CusEntryHeader self, decimal amount)
		{
			Assertion.AssertNotNull("[PRE-CONDITION] test arrange helper AddAgricultureFee requires a valid CusEntryHeader", self);
			_ = self.MergedLines.AddNew().Fees.AddNew("RT", amount);
			return self;
		}

		public static CusEntryHeader AddCustomsDutyFee(this CusEntryHeader self, decimal amount)
		{
			Assertion.AssertNotNull("[PRE-CONDITION] test arrange helper AddCustomsDutyFee requires a valid CusEntryHeader", self);
			_ = self.MergedLines.AddNew().Fees.AddNew("TL", amount);
			return self;
		}

		public static CusEntryHeader AddExciseDutyFee(this CusEntryHeader self, decimal amount)
		{
			Assertion.AssertNotNull("[PRE-CONDITION] test arrange helper AddExciseDutyFee requires a valid CusEntryHeader", self);
			_ = self.MergedLines.AddNew().Fees.AddNew("OL", amount);
			return self;
		}

		public static CusEntryHeader AddVatFee(this CusEntryHeader self, decimal amount)
		{
			Assertion.AssertNotNull("[PRE-CONDITION] test arrange helper AddVatFee requires a valid CusEntryHeader", self);
			_ = self.MergedLines.AddNew().Fees.AddNew("MV", amount);
			return self;
		}
	}
}
