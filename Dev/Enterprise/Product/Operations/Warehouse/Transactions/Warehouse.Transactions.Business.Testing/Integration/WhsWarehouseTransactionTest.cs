using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsWarehouseTransactionTest : WhsTestCaseWithFactory
	{
		public void TestProperties()
		{
			var client = OrgHeader.New(Factory);
			var reference = "Reference1";
			var date = ZDateTime.Now.Date;

			var line = new WhsWarehouseTransactionLine();
			var header = new WhsWarehouseTransaction();
			header.Client = client;
			header.Reference = reference;
			header.Date = date;
			header.TransportCompany = client;
			header.Lines = WhsWarehouseTransactionLineCollection.GetNew(line);

			AssertEquals("Client", client, header.Client);
			AssertEquals("Reference", reference, header.Reference);
			AssertEquals("Date", date, header.Date);
			AssertEquals("TransportCompany", client, header.TransportCompany);
			AssertEquals("Line", line, header.Lines[0]);
			AssertNotNull("Problems", header.Problems);
			AssertEquals("HasErrors", false, header.HasErrors);
		}

		public void TestClearAllNotifications()
		{
			var tran = new WhsWarehouseTransaction();
			var line1 = new WhsWarehouseTransactionLine();
			var line2 = new WhsWarehouseTransactionLine();
			tran.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2);
			AssertEquals(false, line1.HasErrors);
			AssertEquals(false, line2.HasErrors);
			AssertEquals(false, tran.HasErrors);

			tran.Problems.ErrorList.Add("E1");
			tran.Problems.WarningList.Add("W1");
			SetLineErrorsAndWarning(line1);
			SetLineErrorsAndWarning(line2);

			AssertEquals(true, tran.HasErrors);
			AssertEquals(true, line1.HasErrors);
			AssertEquals(true, line2.HasErrors);
			tran.ClearAllNotifications();
			AssertEquals(false, tran.HasErrors);
			AssertEquals(false, line1.HasErrors);
			AssertEquals(false, line2.HasErrors);

			AssertEquals(0, tran.Problems.ErrorList.Count);
			AssertEquals(0, tran.Problems.WarningList.Count);
		}

		public void TestHasErrors()
		{
			var tran = new WhsWarehouseTransaction();
			AssertEquals(false, tran.HasErrors);

			tran.Problems.ErrorList.Add("1");
			AssertEquals(true, tran.HasErrors);
			tran.Problems.ErrorList.Clear();
			AssertEquals(false, tran.HasErrors);

			var line1 = new WhsWarehouseTransactionLine();
			var line2 = new WhsWarehouseTransactionLine();
			tran.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2);
			AssertEquals(false, line1.HasErrors);
			AssertEquals(false, line2.HasErrors);
			AssertEquals(false, tran.HasErrors);

			SetLineErrorsAndWarning(line1);
			AssertEquals(true, tran.HasErrors);
			line1.ClearAllNotifications();
			AssertEquals(false, tran.HasErrors);

			SetLineErrorsAndWarning(line2);
			AssertEquals(true, tran.HasErrors);
			line2.ClearAllNotifications();
			AssertEquals(false, tran.HasErrors);
		}

		public void TestHasWarnings()
		{
			var tran = new WhsWarehouseTransaction();
			AssertEquals(false, tran.HasWarnings);

			tran.Problems.WarningList.Add("1");
			AssertEquals(true, tran.HasWarnings);
			tran.Problems.WarningList.Clear();
			AssertEquals(false, tran.HasWarnings);

			var line1 = new WhsWarehouseTransactionLine();
			var line2 = new WhsWarehouseTransactionLine();
			tran.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2);
			AssertEquals(false, line1.HasWarnings);
			AssertEquals(false, line2.HasWarnings);
			AssertEquals(false, tran.HasWarnings);

			SetLineErrorsAndWarning(line1);
			AssertEquals(true, tran.HasWarnings);
			line1.ClearAllNotifications();
			AssertEquals(false, tran.HasWarnings);

			SetLineErrorsAndWarning(line2);
			AssertEquals(true, tran.HasWarnings);
			line2.ClearAllNotifications();
			AssertEquals(false, tran.HasWarnings);
		}

		#region Implementation

		void SetLineErrorsAndWarning(WhsWarehouseTransactionLine line)
		{
			line.WarehouseProblems.ErrorList.Add("E1");
			line.QuantityProblems.ErrorList.Add("E1");
			line.EntryKeyProblems.ErrorList.Add("E1");
			line.PartAttrib1Problems.ErrorList.Add("E1");
			line.PartAttrib2Problems.ErrorList.Add("E1");
			line.PartAttrib3Problems.ErrorList.Add("E1");

			line.WarehouseProblems.WarningList.Add("W1");
			line.QuantityProblems.WarningList.Add("W1");
			line.EntryKeyProblems.WarningList.Add("W1");
			line.PartAttrib1Problems.WarningList.Add("W1");
			line.PartAttrib2Problems.WarningList.Add("W1");
			line.PartAttrib3Problems.WarningList.Add("W1");
		}

		#endregion
	}
}
