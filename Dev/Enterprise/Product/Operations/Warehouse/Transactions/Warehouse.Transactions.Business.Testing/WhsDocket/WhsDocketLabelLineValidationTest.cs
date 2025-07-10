using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsDocketLabelLineValidationTest : BusinessObjectValidationTestCase
	{
		#region Validation

		public void TestValidateNumberOfLabelsToPrint()
		{
			var errorLessThanZeroMessage = "Please enter the number of labels to print.  It can be 0 or greater.";
			var errorTooManyMessage = "You cannot print more labels than available.";

			var order = Factory.NewWithValidTestData<WhsOrder>();
			var line = new WhsDocketLabelLine(order, 10, 10);
			line.NumberOfLabelsToPrint = -5;
			AssertHasError(line.NumberOfLabelsToPrintInfo, errorLessThanZeroMessage);

			line.NumberOfLabelsToPrint = 12;
			AssertHasError(line.NumberOfLabelsToPrintInfo, errorTooManyMessage);

			line.NumberOfLabelsToPrint = 10;
			AssertNoErrors(line.NumberOfLabelsToPrintInfo);

			line.NumberOfLabelsToPrint = 1;
			AssertNoErrors(line.NumberOfLabelsToPrintInfo);
		}

		#endregion

	}
}
