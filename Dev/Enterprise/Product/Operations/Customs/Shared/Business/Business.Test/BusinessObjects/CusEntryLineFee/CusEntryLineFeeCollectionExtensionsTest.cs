using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryLineFeeCollectionExtensionsTest : TestCaseWithFactory
	{
		public void TestCalculateTotalFeeAmount()
		{
			AssertExceptionThrown<ArgumentNullException>("fees parameter is required", () => CusEntryLineFeeCollectionExtensions.CalculateTotalFeeAmount(fees: null, ZString.Empty, false));

			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var fee1 = entryLine.Fees.AddOrUpdate("TS1", 1m);
			fee1.CF_IsLandedCostOnly = true;
			var fee2 = entryLine.Fees.AddOrUpdate("TS2", 11m);
			fee2.CF_IsLandedCostOnly = false;

			AssertEquals("GetTotalAmount W/O Landed Cost", ZDecimal.Zero, GetFeesAsEnumerable(entryLine.Fees).CalculateTotalFeeAmount("TS1", includeLandedCostOnly: false));
			AssertEquals("GetTotalAmount With Landed Cost", 1m, GetFeesAsEnumerable(entryLine.Fees).CalculateTotalFeeAmount("TS1", includeLandedCostOnly: true));

			AssertEquals("GetTotalAmount W/O Landed Cost", 11m, GetFeesAsEnumerable(entryLine.Fees).CalculateTotalFeeAmount("TS2", includeLandedCostOnly: false));
			AssertEquals("GetTotalAmount With Landed Cost", 11m, GetFeesAsEnumerable(entryLine.Fees).CalculateTotalFeeAmount("TS2", includeLandedCostOnly: true));
		}
		IEnumerable<CusEntryLineFee> GetFeesAsEnumerable(ICusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> fees) => fees.Cast<CusEntryLineFee>();
	}
}
