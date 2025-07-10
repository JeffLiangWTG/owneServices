using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Orders.DataTransfer.Testing
{
	[TestedType(typeof(CsvOrderDataImporterBusinessObject))]
	class CsvOrderDataImporterBusinessObjectTest : DataImporterBusinessObjectTest
	{
		public void TestProgressMessageForFatalError()
		{
			ZString expectedErrorForCompleteFailure = "\r\nNo changes were made due to the above errors. Please fix the errors and try again.\r\n";
			ZString expectedErrorForPartialFailure = "\r\nSome Orders could not be imported due to the above errors.\r\n";

			var testObject = new CsvOrderDataImporterBusinessObjectForTest(Factory);

			testObject.RecordsAdded = 0;
			testObject.RecordsUpdated = 0;
			AssertEquals(expectedErrorForCompleteFailure, testObject.ProgressMessageForFatalError);

			testObject.RecordsAdded = 1;
			AssertEquals("Partial failure: record added", expectedErrorForPartialFailure, testObject.ProgressMessageForFatalError);

			testObject.RecordsAdded = 0;
			testObject.RecordsUpdated = 1;
			AssertEquals("Partial failure: record updated", expectedErrorForPartialFailure, testObject.ProgressMessageForFatalError);

			testObject.RecordsAdded = 100;
			testObject.RecordsUpdated = 500;
			AssertEquals("Partial failure: records added and updated", expectedErrorForPartialFailure, testObject.ProgressMessageForFatalError);
		}

		#region Implementation

		class CsvOrderDataImporterBusinessObjectForTest : CsvOrderDataImporterBusinessObject
		{
			public CsvOrderDataImporterBusinessObjectForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public new ZString ProgressMessageForFatalError
			{
				get { return base.ProgressMessageForFatalError; }
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CsvOrderDataImporterBusinessObject(Factory);
		}

		#endregion
	}
}
