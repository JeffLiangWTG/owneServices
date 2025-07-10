using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsAsnLineValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckWN_ExpiryDateIsValidZDateRange

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestCheckWN_ExpiryDateIsValidZDateRange()
		{
			var today = ZDate.Today;
			var dateAfterMaximumFutureYears = today.AddYears(DateRangeValidation.MaximumFutureYears + 1);
			var dateBeforeMaximumFutureYears = today.AddYears(DateRangeValidation.MaximumFutureYears - 1);

			var asnLine = Factory.New<WhsAsnLine>();
			asnLine.WN_ExpiryDate = ZDate.Today;
			AssertNoErrors(asnLine.WN_ExpiryDateInfo);

			asnLine.WN_ExpiryDate = dateBeforeMaximumFutureYears;
			AssertNoErrors(asnLine.WN_ExpiryDateInfo);

			asnLine.WN_ExpiryDate = dateAfterMaximumFutureYears;
			AssertHasError(asnLine.WN_ExpiryDateInfo, String.Format("The date '{0}' is more than {1} years from now and thus is not valid.", dateAfterMaximumFutureYears.ToString("dd-MMM-yyyy"), DateRangeValidation.MaximumFutureYears));
		}

		#endregion
	}
}
