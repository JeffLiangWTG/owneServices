using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsPickShortLineValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWZS_ShortUnits

		public void TestCheckWZS_ShortUnits()
		{
			var receive = Factory.New<WhsReceive>();
			var receiveLine = receive.Lines.AddNew();
			var shortLine = Factory.New<WhsPickShortLine>();
			shortLine.WZS_WE_InventoryLine = receiveLine.PK;
			shortLine.InventoryLine.WE_StockOnHand = 10.2m;

			shortLine.WZS_ShortUnits = -1;
			AssertHasError(shortLine.WZS_ShortUnitsInfo, "Pick Short Line Units must be greater than zero.");

			shortLine.WZS_ShortUnits = 10.3m;
			AssertHasError(shortLine.WZS_ShortUnitsInfo, "Pick Short Line Units must not exceed Available Stock on related Inventory Line record.");

			shortLine.WZS_ShortUnits = 0;
			AssertHasError(shortLine.WZS_ShortUnitsInfo, "Pick Short Line Units must be greater than zero.");

			shortLine.WZS_ShortUnits = 10.1m;
			AssertNoErrors(shortLine.WZS_ShortUnitsInfo);
		}

		#endregion

		#region TestCheckWZS_WE_InventoryLine

		public void TestCheckWZS_WE_InventoryLine()
		{
			var shortLine = Factory.New<WhsPickShortLine>();
			shortLine.WZS_WE_InventoryLine = Factory.New<WhsReceiveLine>().PK;
			AssertMandatoryValidationError(shortLine.WZS_WE_InventoryLineInfo, isExpectingError: false);

			shortLine.WZS_WE_InventoryLine = ZGuid.Empty;
			AssertMandatoryValidationError(shortLine.WZS_WE_InventoryLineInfo, isExpectingError: true);
		}

		#endregion

		#region TestCheckWZS_WE_TransactionLine

		public void TestCheckWZS_WE_TransactionLine()
		{
			var order = Factory.New<WhsOrder>();
			var shortLine = Factory.New<WhsPickShortLine>();
			shortLine.WZS_WE_TransactionLine = order.Lines.AddNew().PK;
			AssertMandatoryValidationError(shortLine.WZS_WE_TransactionLineInfo, isExpectingError: false);

			shortLine.WZS_WE_TransactionLine = ZGuid.Empty;
			AssertMandatoryValidationError(shortLine.WZS_WE_TransactionLineInfo, isExpectingError: true);
		}

		#endregion

		#region TestCheckWZS_ShortedDateTimeUtc

		public void TestCheckWZS_ShortedDateTimeUtc()
		{
			var shortLine = Factory.New<WhsPickShortLine>();
			shortLine.WZS_ShortedDateTimeUtc = ZDateTime.UtcNow;
			AssertMandatoryValidationError(shortLine.WZS_ShortedDateTimeUtcInfo, isExpectingError: false);

			shortLine.WZS_ShortedDateTimeUtc = ZDateTime.Empty;
			AssertMandatoryValidationError(shortLine.WZS_ShortedDateTimeUtcInfo, isExpectingError: true);
		}

		#endregion

		#region TestCheckWZS_GS_NKShortedBy

		public void TestCheckWZS_GS_NKShortedBy()
		{
			var shortLine = Factory.New<WhsPickShortLine>();
			shortLine.WZS_GS_NKShortedBy = "~BP";
			AssertMandatoryValidationError(shortLine.WZS_GS_NKShortedByInfo, isExpectingError: false);

			shortLine.WZS_GS_NKShortedBy = ZString.Empty;
			AssertMandatoryValidationError(shortLine.WZS_GS_NKShortedByInfo, isExpectingError: true);
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var shortLine = Factory.New<WhsPickShortLine>();
			var validation = new TestWhsPickShortLineValidation(shortLine);

			var list = new string[]
			{
				WhsPickShortLineSchema.Constants.WZS_WE_InventoryLine,
				WhsPickShortLineSchema.Constants.WZS_WE_TransactionLine
			};

			foreach (var propertyInfo in shortLine.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (list.Contains(propertyInfo.Name))
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		#endregion

		#region TestWhsPickShortLineValidation

		class TestWhsPickShortLineValidation : WhsPickShortLineValidation
		{
			public TestWhsPickShortLineValidation(WhsPickShortLine parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
