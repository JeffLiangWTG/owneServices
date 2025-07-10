using System.Globalization;
using System.IO;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReceiveDocuments : WhsTestCaseWithFactory
	{
		#region TestReceiveVarianceDocument_ProductsWithLongDescriptionAreProperlyHidden

		public void TestReceiveVarianceDocument_ProductsWithLongDescriptionAreProperlyHidden()
		{
			// Data setup
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);

			// Set second product to loong description
			var longDescription = "THIS IS TO TEST A VERY LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG TX";
			data.Part2.OP_Desc = longDescription;
			AssertEquals("Precondition: Receive line2 has a long description", longDescription, line2.InDocketLine.ProductDesc);

			line.InDocketLine.WE_TransactionQuantity = 5m;
			AssertEquals("Precondition: Receive hasOversAndUnders should return true", true, receive.HasOversAndUnders);

			// Print & Test document
			var documentCommand = DocumentCommand.GetDocumentCommand(Factory, receive, "Receive Variance");
			using (var documentPack = new DocumentPack(documentCommand, receive, null, null))
			using (var stream = new MemoryStream())
			{
				var report = documentPack.GetFirstReport();
				report.Save(stream);

				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(stream);
					var worksheetString = excelInterface.WorkSheets[0].ToString(); //because code 'hides' excel rows by making the row height 4 not actually removing the row. ToString() removes the rows as required.
					var nonWantedString = "LONG LONG LONG";

					AssertEquals(string.Format(CultureInfo.InvariantCulture, "Worksheet should NOT contain {0}", nonWantedString), false, worksheetString.Contains(nonWantedString));
				}
			}
		}

		#endregion

		#region TestReceiveVarianceDocument_HiddenReceivedValuesAreIncludedInCalculations

		public void TestReceiveVarianceDocument_HiddenReceivedValuesAreIncludedInCalculations()
		{
			// Data setup
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);
			var line3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m);
			line.InDocketLine.WE_TransactionQuantity = 5m;
			line2.InDocketLine.WE_TransactionQuantity = 5m;
			AssertEquals("Precondition: Receive hasOversAndUnders should return true", true, receive.HasOversAndUnders);

			// Print & Test document
			var documentCommand = DocumentCommand.GetDocumentCommand(Factory, receive, "Receive Variance");
			using (var documentPack = new DocumentPack(documentCommand, receive, null, null))
			using (var stream = new MemoryStream())
			{
				var report = documentPack.GetFirstReport();
				report.Save(stream);

				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(stream);
					var worksheetString = excelInterface.WorkSheets[0].ToString();

					AssertEquals("Product Two Expected Qty is correct", true, worksheetString.Contains("[P2]   {AL}-[7]   {AS}-[UNT]"));
					AssertEquals("Product Two Received is correct", true, worksheetString.Contains("{AU}-[10]   {BA}-[UNT]"));
					AssertEquals("Totals are correct", true, worksheetString.Contains("{AI}-[Totals:]   {AM}-[9]   {AU}-[15]   {BC}-[6]   {BG}-[30]"));
				}
			}
		}

		#endregion
	}
}
