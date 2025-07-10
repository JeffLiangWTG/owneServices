using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class WhsDocketFlattenedValidationTest : BusinessObjectValidationTestCase
	{
		#region Line_WE_ExpiryDate

		public void TestLine_WE_ExpiryDateInvalidDate()
		{
			var flattenedRecord = new WhsDocketFlattened();
			flattenedRecord.Line_WE_ExpiryDate = ZDate.Today;
			AssertEquals(false, flattenedRecord.Line_WE_ExpiryDateInfo.HasErrors());

			flattenedRecord.Line_WE_ExpiryDate = ZDate.Invalid;
			AssertEquals(true, flattenedRecord.Line_WE_ExpiryDateInfo.HasErrors());
		}

		public void TestLine_WE_ExpiryDateInvalidDateRange()
		{
			var flattenedRecord = new WhsDocketFlattened();
			flattenedRecord.Line_WE_ExpiryDate = ZDate.Today;
			AssertEquals(false, flattenedRecord.Line_WE_ExpiryDateInfo.HasErrors());

			flattenedRecord.Line_WE_ExpiryDate = new ZDate(1941, 06, 22);
			AssertEquals(true, flattenedRecord.Line_WE_ExpiryDateInfo.HasErrors());
		}

		#endregion

		#region TestLine_WE_PackingDate

		public void TestLine_WE_PackingDateInvalidDate()
		{
			var flattenedRecord = new WhsDocketFlattened();
			flattenedRecord.Line_WE_PackingDate = ZDate.Today;
			AssertEquals(false, flattenedRecord.Line_WE_PackingDateInfo.HasErrors());

			flattenedRecord.Line_WE_PackingDate = ZDate.Invalid;
			AssertEquals(true, flattenedRecord.Line_WE_PackingDateInfo.HasErrors());
		}

		public void TestLine_WE_PackingDateInvalidDateRange()
		{
			var flattenedRecord = new WhsDocketFlattened();
			flattenedRecord.Line_WE_PackingDate = ZDate.Today;
			AssertEquals(false, flattenedRecord.Line_WE_PackingDateInfo.HasErrors());

			flattenedRecord.Line_WE_PackingDate = new ZDate(1941, 06, 22);
			AssertEquals(true, flattenedRecord.Line_WE_PackingDateInfo.HasErrors());
		}

		#endregion
	}
}
